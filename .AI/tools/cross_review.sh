#!/usr/bin/env bash
# Launch one cross-review round: every perspective prompt × both model families, in parallel,
# read-only, then validate each verdict header and print the convergence table.
# Parent: .agents/skills/cross-review/SKILL.md
#
#   cross_review.sh <task-dir> <round> [--only claude|codex] [--perspective <k>]
#   cross_review.sh --policy      print the effective REVIEW_POLICY (recommended | required)
#
# <task-dir>  .AI/sessions/<session>/<change>  (absolute or repo-relative)
# Inputs      <task-dir>/prompt-r<round>-p<k>.md   one file per perspective (k = 1, 2, ...)
#             The orchestrator writes them: the single question, that perspective's slice,
#             the bundle path, and the numbered claims. .AI/reviewer.md is prepended here.
# Outputs     <task-dir>/{claude,codex}-verdict-r<round>-p<k>.md   the verdicts
#             <task-dir>/codex-r<round>-p<k>.log, claude-r<round>-p<k>.json   raw run records
#             <task-dir>/{claude,codex}-config-r<round>-p<k>.txt   settings that verdict ran with
#             <task-dir>/config-r<round>.txt   per-verdict settings of the whole round (rebuilt
#             from those files on every invocation, so a scoped retry never relabels old verdicts)
# Exit        0 all verdicts valid · 3 some reviewer produced no valid verdict (round one-sided)
#
# Identical from a Claude or a Codex primary: both families run as fresh CLI processes, so the
# primary is never one of the reviewers. Reviewer models and effort come from
# .AI/cross-review.conf (team defaults) overridden by .AI/cross-review.local.conf (per person,
# git-ignored) — never from the primary's own model or a user-global CLI config.
set -uo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd -P)"
PERSONA="$ROOT/.AI/reviewer.md"
cd "$ROOT" || exit 1   # `claude --agent reviewer` resolves .claude/agents from cwd; codex needs the repo

die() { printf 'cross_review.sh: %s\n' "$*" >&2; exit 1; }

CLAUDE_MODEL=""; CLAUDE_EFFORT=""; CODEX_MODEL=""; CODEX_EFFORT=""; MAX_PERSPECTIVES=""
REVIEW_POLICY="recommended"
load_conf() { # <file> — parsed, never sourced
  local f="$1" line key val n=0
  [ -f "$f" ] || return 0
  while IFS= read -r line || [ -n "$line" ]; do
    n=$((n + 1))
    line="${line%%#*}"; line="$(printf '%s' "$line" | tr -d '[:space:]')"
    [ -n "$line" ] || continue
    key="${line%%=*}"; val="${line#*=}"
    [ "$key" != "$line" ] || die "$f:$n: expected KEY=VALUE"
    case "$key" in
      CLAUDE_MODEL|CODEX_MODEL)
        [[ "$val" =~ ^[A-Za-z0-9._:-]+$ ]] || die "$f:$n: bad $key '$val'" ;;
      CLAUDE_EFFORT|CODEX_EFFORT)
        [[ "$val" =~ ^[a-z]+$ ]] || die "$f:$n: bad $key '$val'" ;;
      MAX_PERSPECTIVES)
        [[ "$val" =~ ^([1-9][0-9]*)?$ ]] || die "$f:$n: bad $key '$val'" ;;   # empty = no cap
      REVIEW_POLICY)
        [[ "$val" =~ ^(recommended|required)$ ]] || die "$f:$n: bad $key '$val'" ;;
      *) die "$f:$n: unknown key '$key'" ;;
    esac
    eval "$key=\$val"      # key is whitelisted above; val is assigned, not evaluated
  done < "$f"
}
load_conf "$ROOT/.AI/cross-review.conf"
load_conf "$ROOT/.AI/cross-review.local.conf"
for k in CLAUDE_MODEL CLAUDE_EFFORT CODEX_MODEL CODEX_EFFORT; do
  eval "[ -n \"\${$k}\" ]" || die "$k is not set (.AI/cross-review.conf)"
done

# When to run a review is the agent's call (.AI/flow.md §2); this only reports the person's policy.
if [ "${1:-}" = --policy ]; then printf '%s\n' "$REVIEW_POLICY"; exit 0; fi

T="${1:-}"; R="${2:-}"; shift 2 2>/dev/null || true
[ -n "$T" ] && [ -n "$R" ] || die "usage: cross_review.sh <task-dir> <round> [--only claude|codex] [--perspective <k>]"
case "$T" in /*) ;; *) T="$ROOT/$T" ;; esac
[ -d "$T" ] || die "no task dir: $T"
printf '%s' "$R" | grep -Eq '^[0-9]+$' || die "round must be an integer: $R"
ONLY=""; PONLY=""
while [ $# -gt 0 ]; do
  case "$1" in
    --only) ONLY="${2:?}"; shift 2 ;;
    --perspective) PONLY="${2:?}"; shift 2 ;;
    *) die "unknown option $1" ;;
  esac
done
case "$ONLY" in ""|claude|codex) ;; *) die "--only must be claude or codex, got: $ONLY" ;; esac
# Only the families this invocation launches need their CLI (a one-sided review uses --only).
[ "$ONLY" = codex ]  || command -v claude >/dev/null || die "claude CLI not found (use --only codex for a one-sided review)"
[ "$ONLY" = claude ] || command -v codex  >/dev/null || die "codex CLI not found (use --only claude for a one-sided review)"
[ -f "$PERSONA" ] || die "missing $PERSONA"

ALL=()       # every perspective of this round — the convergence check covers all of them
PROMPTS=()   # the ones launched by this invocation (narrowed by --perspective)
for f in "$T"/prompt-r"$R"-p*.md; do
  [ -f "$f" ] || continue
  k="${f##*-p}"; k="${k%.md}"
  ALL+=("$k")
  [ -z "$PONLY" ] || [ "$k" = "$PONLY" ] || continue
  PROMPTS+=("$k")
done
[ ${#ALL[@]} -gt 0 ] || die "no $T/prompt-r$R-p<k>.md"
if [ -n "$MAX_PERSPECTIVES" ] && [ ${#ALL[@]} -gt "$MAX_PERSPECTIVES" ]; then
  die "round $R has ${#ALL[@]} perspective prompts; MAX_PERSPECTIVES=$MAX_PERSPECTIVES (.AI/cross-review*.conf) — merge slices or raise the cap"
fi
[ ${#PROMPTS[@]} -gt 0 ] || die "no prompt for perspective p$PONLY in round $R"

# One invocation per round at a time: overlapping runs share verdict, log and config paths.
# mkdir is atomic. A normal exit releases the lock. An interrupted run signals its reviewers but
# KEEPS the lock: a reviewer that outlives the signal could still write into this round, so only
# a person who has checked that none is running may remove it.
LOCK="$T/.lock-r$R"
if ! mkdir "$LOCK" 2>/dev/null; then
  die "round $R is already running in $T (or an interrupted run left $LOCK — remove it once no reviewer of that run is alive)"
fi
pids=(); labels=(); KEEP_LOCK=""
on_signal() {
  trap - INT TERM
  KEEP_LOCK=1
  local p kids=""
  # Children first recorded, then each attempt subshell killed (so it cannot relaunch), then the
  # reviewer CLIs it had started.
  for p in ${pids[@]+"${pids[@]}"}; do
    kids="$kids $(pgrep -P "$p" 2>/dev/null | tr '\n' ' ')"
  done
  for p in ${pids[@]+"${pids[@]}"}; do kill -TERM "$p" 2>/dev/null; done
  for p in $kids; do kill -TERM "$p" 2>/dev/null; done
  printf 'cross_review.sh: interrupted; reviewers were signalled. Once none is running, remove %s\n' "$LOCK" >&2
  exit 130
}
trap '[ -n "$KEEP_LOCK" ] || rmdir "$LOCK" 2>/dev/null' EXIT
trap on_signal INT TERM

compose() { # <k>  -> full prompt on stdout
  cat "$PERSONA"
  printf '\n\n---\n\n## This round (round %s, perspective p%s)\n\nTask folder: %s\n\n' "$R" "$1" "$T"
  cat "$T/prompt-r$R-p$1.md"
}

valid_header() { # <file>
  [ -s "$1" ] || return 1
  sed -n 1p "$1" | grep -Eq '^VERDICT: (approve|approve-with-fixes|disagree)$' &&
  sed -n 2p "$1" | grep -Eq '^BLOCKERS: [0-9]+$' &&
  sed -n 3p "$1" | grep -Eq '^SUMMARY: .+'
}

run_codex() { # <k>
  local k="$1" out="$T/codex-verdict-r$R-p$1.md" log="$T/codex-r$R-p$1.log"
  rm -f "$out"
  printf '%s %s\n' "$CODEX_MODEL" "$CODEX_EFFORT" > "$T/codex-config-r$R-p$1.txt"
  codex exec -s read-only -m "$CODEX_MODEL" -c model_reasoning_effort="$CODEX_EFFORT" \
    -o "$out" "$(compose "$k")" < /dev/null > "$log" 2>&1
  # The banner must prove the configured settings; a mismatch voids the verdict.
  if ! { grep -q "^model: $CODEX_MODEL$" "$log" && grep -q '^sandbox: read-only$' "$log" &&
         grep -q "^reasoning effort: $CODEX_EFFORT$" "$log"; }; then
    mv -f "$out" "$out.banner-mismatch" 2>/dev/null; return 1
  fi
  valid_header "$out"
}

run_claude() { # <k>
  local k="$1" out="$T/claude-verdict-r$R-p$1.md" raw="$T/claude-r$R-p$1.json"
  rm -f "$out"
  printf '%s %s\n' "$CLAUDE_MODEL" "$CLAUDE_EFFORT" > "$T/claude-config-r$R-p$1.txt"
  # The prompt goes on stdin, and the tool lists use the `=` form: both flags are variadic and
  # would otherwise swallow a positional prompt as tool names (seen: "deny rule 'Answer'").
  compose "$k" | claude -p --agent reviewer --model "$CLAUDE_MODEL" --effort "$CLAUDE_EFFORT" \
    --allowedTools=Read,Grep,Glob --disallowedTools=Edit,Write,Bash,NotebookEdit \
    --output-format json > "$raw" 2> "$T/claude-r$R-p$1.err"
  python3 - "$raw" "$out" "$CLAUDE_MODEL" <<'PY' || return 1
import json, sys
raw, out, model = sys.argv[1:4]
d = json.load(open(raw))
used = list((d.get("modelUsage") or {}).keys())
# An alias (`sonnet`) resolves to a full id (`claude-sonnet-5`), so match by containment.
if d.get("is_error") or not any(model == u or model in u for u in used):
    sys.exit(1)                      # errored run, or the configured model did not answer
open(out, "w").write((d.get("result") or "").strip() + "\n")
PY
  valid_header "$out"
}

attempt() { # <family> <k>   one relaunch on an invalid verdict, overwriting the same paths
  "run_$1" "$2" && return 0
  "run_$1" "$2"
}

for k in "${PROMPTS[@]}"; do
  for fam in claude codex; do
    [ -z "$ONLY" ] || [ "$fam" = "$ONLY" ] || continue
    attempt "$fam" "$k" & pids+=("$!"); labels+=("$fam-p$k")
  done
done

failed=0
for i in "${!pids[@]}"; do
  wait "${pids[$i]}" || { failed=1; printf 'NO VALID VERDICT: %s (see its log in %s)\n' "${labels[$i]}" "$T" >&2; }
done

# Convergence table over the EXPECTED set — every perspective of the round × both families —
# not over whatever verdict files exist, so a scoped retry can never hide a missing or invalid
# verdict behind a CONVERGED line.
# Each row shows the settings recorded when THAT verdict was produced; a verdict without such a
# record proves nothing about its model and counts as missing.
verdicts=""; total_blockers=0; missing=0
receipt="$T/config-r$R.txt"
printf 'local override at last invocation: %s\n' \
  "$([ -f "$ROOT/.AI/cross-review.local.conf" ] && echo yes || echo no)" > "$receipt"
printf '\n%-14s %-28s %-20s %-8s %s\n' "REVIEWER" "MODEL/EFFORT" "VERDICT" "BLOCKERS" "SUMMARY"
for k in "${ALL[@]}"; do
  for fam in claude codex; do
    f="$T/$fam-verdict-r$R-p$k.md"; c="$T/$fam-config-r$R-p$k.txt"
    cfg="$([ -s "$c" ] && tr ' ' '/' < "$c" | head -1)"
    printf '%s %s\n' "$fam-p$k" "${cfg:-none}" >> "$receipt"
    if valid_header "$f" && [ -n "$cfg" ]; then
      v="$(sed -n 1p "$f" | cut -d' ' -f2)"; n="$(sed -n 2p "$f" | cut -d' ' -f2)"
      verdicts="$verdicts $v"; total_blockers=$((total_blockers + n))
      printf '%-14s %-28s %-20s %-8s %s\n' "$fam-p$k" "$cfg" "$v" "$n" "$(sed -n 3p "$f" | cut -c10-)"
    else
      missing=$((missing + 1))
      printf '%-14s %-28s %-20s\n' "$fam-p$k" "${cfg:--}" "$([ -f "$f" ] && echo INVALID || echo MISSING)"
    fi
  done
done
distinct="$(printf '%s\n' $verdicts | sort -u | grep -c .)"
if [ "$failed" -ne 0 ] || [ "$missing" -ne 0 ]; then
  echo "ROUND $R: ONE-SIDED — both families must deliver a valid verdict before convergence counts."
  exit 3
elif [ "$distinct" -eq 1 ] && [ "$total_blockers" -eq 0 ]; then
  echo "ROUND $R: CONVERGED (all$verdicts, 0 blockers)"
else
  echo "ROUND $R: NOT CONVERGED — total blockers $total_blockers; read the verdict files for live disagreements."
fi
exit 0
