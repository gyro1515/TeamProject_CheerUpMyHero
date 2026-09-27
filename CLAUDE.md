# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

@Docs/AI/PROJECT.md

## Claude Code root

The common root above is imported from `Docs/AI/PROJECT.md`. Codex reads the same file through
`AGENTS.md`. Put anything both families need there or below it, never here. This file holds only
what is specific to Claude Code.

## Router tree — Claude root

```
CLAUDE.md                                   ← you are here (Claude)
├─ Docs/AI/PROJECT.md  [router]             common root (imported above)
├─ .claude/skills/<name>/SKILL.md           stub per shared skill: same frontmatter, body points to `.agents/skills/<name>/SKILL.md` (edit that one)
├─ .claude/agents/reviewer.md               read-only reviewer that cross_review.sh launches
└─ .claude/settings.json                    SessionStart hook (tracked). Personal settings: settings.local.json
```

## Claude-specific

- **Skills**: shared skills live in `.agents/skills/`. `.claude/skills/<name>/SKILL.md` is a
  tracked stub with the same frontmatter whose body says to read and follow the canonical file,
  so edit the file in `.agents/skills/` (and the stub's frontmatter, if that changed). Invoke a
  skill with the Skill tool (for example `/cross-review`).
- **SessionStart hook**: `.claude/settings.json` runs `.AI/tools/session.sh hook claude` on
  `startup|resume|clear|compact`. It injects this runtime's session id and the work session bound
  to it.
- **Reviewer agent**: `.claude/agents/reviewer.md` is the read-only reviewer that `cross_review.sh`
  launches with `claude -p --agent reviewer`. Its model and effort come from
  `.AI/cross-review.conf`. Don't use it as a writer.
- The Codex side is described in `AGENTS.md`. Both columns are compared in `.AI/flow.md` §5.
