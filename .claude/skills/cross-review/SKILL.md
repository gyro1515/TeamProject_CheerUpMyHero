---
name: cross-review
description: Cross-review a plan, a bug diagnosis, or a finished diff with independent read-only reviewers from two model families (Claude and Codex; models and effort from .AI/cross-review.conf, overridable per person), fanned out over several perspectives, iterated to consensus. Recommended, not required: offer it for non-trivial finished changes, for plans touching a risk subsystem in .AI/flow.md §3, and for disputed root causes, and run it when the user asks or agrees, including whenever the user asks to 검증/체크/교차검증/크로스리뷰 something. Works identically from a Claude or a Codex primary.
argument-hint: integration | diagnosis | post-diff
---

<!-- Parent: CLAUDE.md (Claude root). Stub: keep this frontmatter identical to the canonical file;
.AI/tools/doc_check.sh fails if they differ. -->

Read `.agents/skills/cross-review/SKILL.md` and follow it exactly, applying the arguments given
to this skill. That file is the canonical skill, shared with Codex, so edit it there, not here.
