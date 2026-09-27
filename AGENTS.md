# AGENTS.md — Codex root

**Read [`Docs/AI/PROJECT.md`](Docs/AI/PROJECT.md) before any task, and follow its router tree.**
That file is the common root for both agent families. Claude Code imports it, but Codex has no
import, so read it explicitly. This file holds only what is specific to Codex.

## First action of any repository work

(This paragraph is a word-for-word copy of the one in `PROJECT.md`. Codex has no import, and
this is the one step that must never be missed. Edit both copies together.)

Check whether a work session is bound to you. The SessionStart hook's `[session.sh]` line tells
you, or run `.AI/tools/session.sh current`. If none is bound, run
`.AI/tools/session.sh new <kebab-slug>`, or run `session.sh bind <name>` to resume one
(`session.sh status` lists them). The agent family and session id are detected automatically.
Then keep `.AI/sessions/<session>/STATE.md` current, as `.AI/flow.md` §1 describes. Q&A and pure
discussion skip this.

## Router tree — Codex root

```
AGENTS.md                         ← you are here (Codex)
├─ Docs/AI/PROJECT.md  [router]   common root: read first
├─ .agents/skills/<name>/SKILL.md (discovery) where Codex finds shared skills; they are owned by `.AI/flow.md`. Use one by reading it, or with `$<name>`
└─ .codex/hooks.json              machine-local, git-ignored. `.AI/tools/session.sh install-codex-hook` creates it
```

## Codex-specific

- **SessionStart hook**: `.codex/hooks.json` is not tracked, because the Cate app merges
  absolute-path hooks into it. Once per clone, run `.AI/tools/session.sh install-codex-hook`, then
  trust the hook in the TUI with `/hooks`. The matcher is `startup|resume|compact`, so after a
  clear there is no hook line. Run `session.sh current` instead.
- **Sandbox network**: the shell sandbox disables the network. `.AI/tools/cross_review.sh`
  launches both reviewer CLIs, and they need the network, so request escalated permissions for
  that command up front. Don't wait for it to fail first. The reviewers stay read-only either way.
- **Sandbox `ps`**: `ps` is denied, so `session.sh` detects the family from the environment
  (`CODEX_SESSION_ID`/`CODEX_THREAD_ID`, plus `CODEX_SANDBOX` when a Claude id is also present). If
  it ever picks the wrong family, prefix the command with `AI_SESSION_AGENT=codex`.
- The Claude side is described in `CLAUDE.md`. Both columns are compared in `.AI/flow.md` §5.
