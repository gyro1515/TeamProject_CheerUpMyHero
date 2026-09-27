# PROJECT.md — project documentation router

Parent: [`AGENTS.md`](../../AGENTS.md). This file routes a task to the one document that owns it.
Read only the documents your task touches. Don't read the whole tree up front. Every document in
`Docs/AI/`, and `.AI/flow.md`, names its parent in a `Parent:` line under its title, so you can
always climb back here.

```
AGENTS.md                              root: workflow entry, build/test, code style
├── .AI/flow.md                        how to work: session folder, change flow, gates, risk list, Claude↔Codex
│   └── .agents/skills/cross-review/SKILL.md   verification procedure
│       ├── .AI/reviewer.md            reviewer persona and verdict format
│       └── .AI/cross-review.conf      reviewer models/effort (+ .local.conf override)
└── Docs/AI/PROJECT.md                 this router: what the project is and where its rules live
    ├── architecture.md                existing systems: managers, scenes, data, backend, events, combat
    ├── module-rules.md                rules for new code: ownership, direction, EventManager use, prefabs
    └── ai-workflow.md                 (Korean, for humans) AI setup, local vs repo, model config
```

## Task owners

- **Before any repository work, and for the change flow, gates, or the report format**, read
  [`.AI/flow.md`](../../.AI/flow.md). Its §3 is the canonical list of risk subsystems, where
  server data, player saves, currency and gacha, static tables, serialization, and lifecycle
  need plan review.
- **To find which manager owns something, or how scenes, static data, the UGS backend,
  UI and pooling paths, events, or combat work today**, read [`architecture.md`](architecture.md).
- **To add a script, prefab, manager, or event, or to connect two systems**, read
  [`module-rules.md`](module-rules.md) before you write code. It covers direct calls versus
  events versus manager calls, where an event struct goes, subscribe and unsubscribe placement,
  and whether a new `SingletonMono` is justified.
- **To verify a plan, a diagnosis, or a diff with the other model family**, use the
  `cross-review` skill ([`SKILL.md`](../../.agents/skills/cross-review/SKILL.md)).
- **To change the reviewer models, the effort, or the cost cap**, see
  [`.AI/cross-review.conf`](../../.AI/cross-review.conf). For the per-person override, see
  [`ai-workflow.md`](ai-workflow.md) §3.
- **To change the AI setup itself (hooks, session tool, what is tracked and what is local)**,
  read [`ai-workflow.md`](ai-workflow.md), then `.AI/flow.md` §4–§5.

## Keeping the tree honest

- Every document owns one topic. When you learn a fact, put it in the owning document. Don't
  repeat it in a parent. A parent says only when to go to the child.
- A new document is added as a child of this router, with one "Task owners" bullet naming its
  trigger, and with a `Parent:` line of its own.
- If code you changed makes a routed document wrong, update that document in the same change.
