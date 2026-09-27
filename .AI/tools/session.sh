#!/usr/bin/env bash
# Session work folders — one shared implementation for Claude Code and Codex.
# Parent: .AI/flow.md §1
#
#   session.sh new <slug>
#       Create .AI/sessions/<YYMMDD>-<slug>/STATE.md from the template and print its path.
#   session.sh bind <session-dir-name>
#       Record this runtime session in an existing session's STATE.md.
#   (Overrides, only when detection is wrong: `new <slug> --agent <a> --id <id>`,
#    `bind <name> <agent> <id>`, or AI_SESSION_AGENT=claude|codex.)
#   session.sh current
#       Print the session bound to this runtime — the fallback when no hook context arrived.
#   session.sh status
#       List open sessions (STATE.md present and not `- 상태: closed`).
#   session.sh hook <claude|codex>
#       SessionStart hook entry: reads the hook JSON on stdin, prints additionalContext.
#       Never fails a session start: any error degrades to a plain-text note, exit 0.
#   session.sh install-codex-hook
#       Merge the project's SessionStart entry into the machine-local .codex/hooks.json
#       (idempotent). Run once per clone; then trust it in the Codex TUI with /hooks.
#
# Agent and runtime session id are detected, so callers normally pass neither: the nearest
# ancestor process named `claude` or `codex` decides the family (both env vars are present
# when one agent runs inside the other), and the id comes from that family's variable —
# CLAUDE_CODE_SESSION_ID, or CODEX_SESSION_ID / CODEX_THREAD_ID.
# The rules for using this live in .AI/flow.md §1.
set -uo pipefail

# Resolved from the script's own path, so no git is needed (hooks must not depend on it).
ROOT="$(cd "$(dirname "$0")/../.." && pwd -P)"
SESSIONS="$ROOT/.AI/sessions"

die() { printf 'session.sh: %s\n' "$*" >&2; exit 1; }
now() { date '+%Y-%m-%d %H:%M'; }

state_field() { # <STATE.md> <label>  -> value after "- <label>: "
  sed -n "s/^- $2: //p" "$1" | head -1
}

detect_agent() {
  # 1. Explicit override, for the rare nested case the rules below cannot settle.
  case "${AI_SESSION_AGENT:-}" in claude|codex) echo "$AI_SESSION_AGENT"; return ;; esac
  # 2. Nearest agent ancestor. Codex's seatbelt sandbox denies `ps` ("operation not
  #    permitted"), so an empty answer falls through to the environment instead of `unknown`.
  local pid=$$ line ppid comm i=0
  while [ "$pid" -gt 1 ] && [ $i -lt 40 ]; do
    line="$(ps -o ppid=,comm= -p "$pid" 2>/dev/null)" || break
    [ -n "$line" ] || break
    ppid="$(printf '%s' "$line" | awk '{print $1}')"
    comm="$(printf '%s' "$line" | awk '{ $1=""; sub(/^ /,""); print }')"
    case "$(basename "$comm")" in
      claude) echo claude; return ;;
      codex)  echo codex;  return ;;
    esac
    [ -n "$ppid" ] || break
    pid="$ppid"; i=$((i + 1))
  done
  # 3. Environment. One family's id alone decides. With both present (one agent nested in
  #    the other), a set CODEX_SANDBOX means we are inside a sandboxed Codex shell — the only
  #    place `ps` is denied — so the innermost agent is Codex.
  local c="${CLAUDE_CODE_SESSION_ID:-}" x="${CODEX_SESSION_ID:-${CODEX_THREAD_ID:-}}"
  if [ -n "$x" ] && [ -z "$c" ]; then echo codex
  elif [ -n "$c" ] && [ -z "$x" ]; then echo claude
  elif [ -n "$x" ] && [ -n "${CODEX_SANDBOX:-}" ]; then echo codex
  else echo unknown; fi
}

detect_id() { # <agent>
  case "$1" in
    claude) printf '%s' "${CLAUDE_CODE_SESSION_ID:-}" ;;
    codex)  printf '%s' "${CODEX_SESSION_ID:-${CODEX_THREAD_ID:-}}" ;;
  esac
}

rewrite() { # <file> <sed-expr>  — BSD/GNU-safe in-place edit that keeps the file's mode
  local tmp rc; tmp="$(mktemp)" || return 1
  sed "$2" "$1" > "$tmp" && cat "$tmp" > "$1"; rc=$?
  rm -f "$tmp"; return $rc
}

touch_state() { rewrite "$1" "s/^- 갱신: .*/- 갱신: $(now)/"; }

cmd_new() {
  local slug="" agent="" id=""
  while [ $# -gt 0 ]; do
    case "$1" in
      --agent) agent="${2:?}"; shift 2 ;;
      --id)    id="${2:?}"; shift 2 ;;
      -*)      die "unknown option $1" ;;
      *)       [ -z "$slug" ] || die "one slug only"; slug="$1"; shift ;;
    esac
  done
  [ -n "$slug" ] || die "usage: session.sh new <slug> [--agent claude|codex] [--id <id>]"
  printf '%s' "$slug" | grep -Eq '^[a-z0-9]+(-[a-z0-9]+)*$' || die "slug must be kebab-case: $slug"
  [ -n "$agent" ] || agent="$(detect_agent)"
  [ -n "$id" ] || id="$(detect_id "$agent")"
  [ -n "$id" ] || die "cannot detect the runtime session id for agent '$agent'; pass --agent and --id"

  local name dir t
  name="$(date +%y%m%d)-$slug"
  dir="$SESSIONS/$name"
  t="$(now)"
  mkdir -p "$SESSIONS" || die "cannot create $SESSIONS"
  # A plain mkdir is the existence check: it is atomic, so two agents creating the same name at
  # once cannot both pass it and overwrite each other's STATE.md.
  mkdir "$dir" 2>/dev/null ||
    die "already exists: .AI/sessions/$name (resume it with bind, or pick another slug)"
  cat > "$dir/STATE.md" <<EOF
# 세션: $name

- 상태: active
- 생성: $t
- 런타임 세션: $agent:$id
- 갱신: $t

## 요청 (사용자 원문 요지 — 잊지 말 것)

-

## 작업 목록

| change | 내용 | 상태 | 비고 |
|---|---|---|---|

## 결정 사항 (사용자 확정 / "(내 판단)" 표시한 에이전트 판단)

-

## 미결 질문

-

## 진행 로그

- $t 세션 생성 ($agent)
EOF
  printf '%s\n' ".AI/sessions/$name/STATE.md"
}

cmd_bind() {
  local name="${1:-}" agent="${2:-}" id="${3:-}"
  [ -n "$name" ] || die "usage: session.sh bind <session-dir-name> [<agent> <runtime-session-id>]"
  [ -n "$agent" ] || agent="$(detect_agent)"
  [ -n "$id" ] || id="$(detect_id "$agent")"
  [ -n "$id" ] || die "cannot detect the runtime session id for agent '$agent'; pass <agent> <id>"
  local f="$SESSIONS/$name/STATE.md"
  [ -f "$f" ] || die "no such session: $name"
  # Serialize this script's own read-modify-write of STATE.md: two binds at once would otherwise
  # lose one registration. (Agents editing STATE.md follow the one-writer rule, .AI/flow.md §4.)
  local n=0
  BIND_LOCK="$SESSIONS/$name/.bind-lock"   # global: the EXIT trap runs after this function returns
  until mkdir "$BIND_LOCK" 2>/dev/null; do
    n=$((n + 1)); [ "$n" -lt 50 ] || die "busy: $BIND_LOCK (remove it if no session.sh is running)"
    sleep 0.1
  done
  trap 'rmdir "$BIND_LOCK" 2>/dev/null' EXIT
  if ! grep -qF "$agent:$id" "$f"; then
    rewrite "$f" "s/^- 런타임 세션: \(.*\)$/- 런타임 세션: \1, $agent:$id/" || die "cannot update $f"
  fi
  touch_state "$f"
  printf '%s\n' ".AI/sessions/$name/STATE.md"
}

bound_to() { # <agent> <id>  -> path of the open session recording <agent>:<id>
  [ -n "$2" ] && [ -d "$SESSIONS" ] || return 0
  local f
  for f in "$SESSIONS"/*/STATE.md; do
    [ -f "$f" ] || continue
    [ "$(state_field "$f" '상태')" = closed ] && continue
    grep -qF "$1:$2" "$f" && printf '%s\n' ".AI/sessions/$(basename "$(dirname "$f")")/STATE.md"
  done
}

cmd_current() {
  local agent id b; agent="$(detect_agent)"; id="$(detect_id "$agent")"
  b="$(bound_to "$agent" "$id")"
  if [ -n "$b" ]; then printf '%s\n' "$b"; else
    printf 'none bound to %s:%s\n' "$agent" "${id:-unknown}" >&2; exit 1; fi
}

list_open() {
  [ -d "$SESSIONS" ] || return 0
  local f st
  for f in "$SESSIONS"/*/STATE.md; do
    [ -f "$f" ] || continue
    st="$(state_field "$f" '상태')"
    [ "$st" = closed ] && continue
    printf '%s\t%s\t%s\n' "$(basename "$(dirname "$f")")" "$st" "$(state_field "$f" '갱신')"
  done
}

cmd_status() {
  local rows; rows="$(list_open)"
  if [ -z "$rows" ]; then echo "열린 세션 없음 (.AI/sessions/)"; return 0; fi
  printf 'SESSION\tSTATUS\tUPDATED\n%s\n' "$rows" | column -t -s "$(printf '\t')"
}

json_field() { # <json> <key>
  printf '%s' "$1" | python3 -c 'import json,sys
try: print(json.load(sys.stdin).get(sys.argv[1],""))
except Exception: print("")' "$2" 2>/dev/null
}

cmd_hook() {
  local agent="${1:-unknown}" payload id="" source="" bound ctx rows
  payload="$(cat 2>/dev/null)"
  if command -v python3 >/dev/null 2>&1; then
    id="$(json_field "$payload" session_id)"; source="$(json_field "$payload" source)"
  fi
  [ -n "$id" ] || id="$(detect_id "$agent")"
  bound="$(bound_to "$agent" "$id" | head -1)"

  ctx="[session.sh] agent=$agent runtime_session_id=${id:-unknown} source=${source:-unknown}"
  if [ -n "$bound" ]; then
    ctx="$ctx
This runtime session is bound to $bound. Read it now and continue from it (rules: .AI/flow.md)."
  else
    ctx="$ctx
No work session is bound to this runtime session. Before starting repository work, follow .AI/flow.md §1:
resume an open session with \`.AI/tools/session.sh bind <name>\` or create one with
\`.AI/tools/session.sh new <kebab-slug>\` (agent and id are detected).
Read-only reviewers launched by cross-review skip this."
  fi
  rows="$(list_open)"
  [ -z "$rows" ] || ctx="$ctx
Open sessions (name / status / updated):
$rows"

  if command -v python3 >/dev/null 2>&1 &&
     CTX="$ctx" python3 -c 'import json,os
print(json.dumps({"hookSpecificOutput":{"hookEventName":"SessionStart","additionalContext":os.environ["CTX"]}}))' 2>/dev/null; then
    :
  else
    printf '%s\n' "$ctx"          # plain-text fallback: still context, never a failed start
  fi
  exit 0
}

# .codex/hooks.json is machine-local here: Cate merges its bridge hooks (absolute paths into the
# user's home) into that file and adds it to .git/info/exclude, so it cannot be the tracked
# carrier of the project hook. This installs the project's SessionStart entry into it — merged,
# idempotent, other entries untouched. Claude needs no installer: its project hook lives in the
# tracked .claude/settings.json, and Cate writes to settings.local.json instead.
CODEX_HOOK_CMD='sh -c '"'"'r=$(git rev-parse --show-toplevel 2>/dev/null) && exec "$r/.AI/tools/session.sh" hook codex; exit 0'"'"
cmd_install_codex_hook() {
  command -v python3 >/dev/null 2>&1 || die "python3 is required to merge .codex/hooks.json"
  mkdir -p "$ROOT/.codex" || die "cannot create .codex/"
  HOOKS="$ROOT/.codex/hooks.json" CMD="$CODEX_HOOK_CMD" python3 - <<'PY' || die "merge failed"
import json, os, sys, tempfile
path, cmd = os.environ["HOOKS"], os.environ["CMD"]
def read():
    try:
        with open(path) as f:
            return f.read()
    except FileNotFoundError:
        return None
# Another tool (Cate) may merge into this file too. Compare-and-swap: build the new content from
# one snapshot, re-read just before replacing, and start over if the file changed meanwhile.
# The replace itself is atomic (temp file + os.replace), so a reader never sees half a file.
# Limit: the re-read and the replace are two steps, and the other tool takes no lock, so a write
# that lands in that instant can still be lost. Run this while Cate is not setting up hooks.
for _ in range(5):
    before = read()
    data = json.loads(before) if before is not None else {}
    starts = data.setdefault("hooks", {}).setdefault("SessionStart", [])
    if any(".AI/tools/session.sh" in h.get("command", "") for e in starts for h in e.get("hooks", [])):
        print("already installed: .codex/hooks.json"); sys.exit(0)
    starts.insert(0, {"matcher": "startup|resume|compact",
                      "hooks": [{"type": "command", "command": cmd, "timeout": 10,
                                 "additionalContextLimit": 2500}]})
    fd, tmp = tempfile.mkstemp(dir=os.path.dirname(path), prefix=".hooks.json.")
    os.chmod(tmp, os.stat(path).st_mode & 0o777 if before is not None else 0o644)
    with os.fdopen(fd, "w") as f:
        json.dump(data, f, indent=2); f.write("\n")
    if read() == before:
        os.replace(tmp, path)
        print("installed: .codex/hooks.json — trust it once in the Codex TUI with /hooks"); sys.exit(0)
    os.unlink(tmp)
sys.exit("the file kept changing under another writer; run install-codex-hook again")
PY
}

case "${1:-}" in
  install-codex-hook) shift; cmd_install_codex_hook ;;
  new)     shift; cmd_new "$@" ;;
  bind)    shift; cmd_bind "$@" ;;
  current) shift; cmd_current ;;
  status)  shift; cmd_status ;;
  hook)    shift; cmd_hook "$@" ;;
  *)       sed -n '2,25p' "$0" | sed 's/^# \{0,1\}//'; exit 2 ;;
esac
