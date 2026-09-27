# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

@AGENTS.md

## Claude Code runtime notes

`AGENTS.md` above is the shared source, and Codex reads the same file. Put project or workflow
guidance there or in `.AI/flow.md`, never here. This file holds only what differs in Claude Code:

- Shared skills live in `.agents/skills/`. `.claude/skills/<name>` is a symlink to them, so edit
  the file in `.agents/skills/`. Invoke a skill with the Skill tool (for example `/cross-review`).
- The SessionStart hook in `.claude/settings.json` runs `.AI/tools/session.sh hook claude`. It
  injects this runtime's session id and the work session bound to it.
- `.claude/agents/reviewer.md` is the read-only reviewer that `cross_review.sh` launches with
  `claude -p --agent reviewer`. Don't use it as a writer.
