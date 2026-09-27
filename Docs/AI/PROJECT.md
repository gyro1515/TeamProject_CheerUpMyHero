# PROJECT.md — common root router

Parent: the two agent roots. [`CLAUDE.md`](../../CLAUDE.md) imports this file, and
[`AGENTS.md`](../../AGENTS.md) tells Codex to read it first. Everything both agent families share
starts here. The roots hold only what differs per family.

## First action of any repository work

Check whether a work session is bound to you. The SessionStart hook's `[session.sh]` line tells
you, or run `.AI/tools/session.sh current`. If none is bound, run
`.AI/tools/session.sh new <kebab-slug>`, or run `session.sh bind <name>` to resume one
(`session.sh status` lists them). The agent family and session id are detected automatically.
Then keep `.AI/sessions/<session>/STATE.md` current, as `.AI/flow.md` §1 describes. Q&A and pure
discussion skip this.

## Project

"Cheer Up, My Hero" is a 2D mobile strategy-defense game (like Nyanko/Paladog) with territory
management. It runs on Unity **2022.3.62f2** with URP 2D and the New Input System (Active Input Handling is *Both*;
some code still reads the legacy `Input` class), and targets
Android (plus WebGL and Standalone). Player data lives on Unity Gaming Services. Code comments,
commit messages, and the README are in Korean. Internal agent work (prompts, evidence, agent
messages) is in English, and everything the user reads is in Korean.

Three facts you need before you route anywhere:
- Reading `SingletonMono<T>.Instance` auto-creates the manager. After a persistent manager is
  destroyed, `Instance` returns null.
- `UIManager.OpenUI<T>()` and `ObjectPoolManager.Get(PoolType)` load prefabs by **name** from
  `Resources/`.
- Third-party code is vendor code, so don't modify it. That covers `Assets/Plugins` (DOTween,
  UniTask, the Android/iOS ad libraries), the packs in `Assets/Externals` (ExcelImporter, SPUM, …),
  `Assets/GoogleMobileAds`, `Assets/ExternalDependencyManager`, `Assets/MobileDependencyResolver`, and
  TextMesh Pro. The full list and one exception are in `Docs/AI/architecture.md` → *Third-party*.

## Router tree

Open a node only when its trigger matches your task, and go only as deep as the task needs.
A node marked `[router]` shows its own subtree. Who names which parent is set in
*Keeping the tree honest* below.

```
CLAUDE.md / AGENTS.md            agent roots. Their family-only branches are in their own trees
└─ Docs/AI/PROJECT.md            ← you are here: common root
   ├─ .AI/flow.md  [router]      BEFORE any repository work (file writes, or longer investigations)
   │  ├─ §1 Session folder          the STATE.md rules
   │  ├─ §2 Change flow             steps, the compile gate, the Korean report format
   │  ├─ §3 Risk subsystems         changes here get a plan review offered
   │  ├─ §4 Claude ↔ Codex          shared files, talking through Cate
   │  ├─ §5 Runtime mechanics       the per-family difference table
   │  ├─ .AI/tools/session.sh       session folders, SessionStart hook, Codex hook installer
   │  ├─ .AI/tools/doc_check.sh     docs baseline and sync check: trees, Parent lines, doc paths, skill stubs
   │  └─ .agents/skills/cross-review/SKILL.md  [router]   to verify a plan, a diagnosis, or a diff (recommended, on request)
   │     ├─ .AI/reviewer.md            reviewer persona and verdict format
   │     ├─ .AI/cross-review.conf      reviewer models, effort, cap, review policy (+ .local.conf override)
   │     └─ .AI/tools/cross_review.sh  runs one round
   ├─ Docs/AI/development.md     BEFORE building, compiling, testing, committing, or writing C#
   │  ├─ Build / run / test         the batch-mode compile command, tests, defines
   │  ├─ Commits and branches
   │  └─ Code style
   ├─ Docs/AI/architecture.md  [router]   to find how an existing system works, BEFORE changing one
   │  ├─ Managers / Startup and scene transitions / Static game data / Backend (UGS)
   │  ├─ Resources-path conventions / Events / Naming traps / Third-party
   │  ├─ Docs/AI/systems/app-shell.md          start scene, tutorial, main menu, settings, audio, input, popups
   │  ├─ Docs/AI/systems/player-data.md        save shape, load order, saving, currencies
   │  ├─ Docs/AI/systems/territory.md          buildings, grid, building synergies
   │  ├─ Docs/AI/systems/deck-and-units.md     unit cards, deck presets, unit synergies
   │  ├─ Docs/AI/systems/artifacts.md          artifacts: inventory, upgrade, stats, active skills
   │  ├─ Docs/AI/systems/gacha-and-rewards.md  gacha, pity, mail, ad rewards, grant risks
   │  ├─ Docs/AI/systems/stages.md             stage ids, unlocks, waves, destiny/challenge modifiers
   │  └─ Docs/AI/systems/battle.md             battle runtime: spawning, hero, units, damage, battle end
   ├─ Docs/AI/module-rules.md    BEFORE adding a script, prefab, manager, or event, or connecting two systems
   │  ├─ Project bindings / Decision procedure   which mechanism to use
   │  ├─ Rules O C E P              ownership, EventManager channels, values, prefabs
   │  ├─ Anti-patterns / Verification greps
   │  └─ Known exceptions           existing code not to copy
   ├─ Docs/AI/ai-workflow.md     (Korean, for people) setup after clone, docs check and CI, reviewer model config, local vs repo
   ├─ .github/workflows/doc-check.yml    CI: runs doc_check.sh on every PR and every push to main or Develop
   └─ .github/pull_request_template.md   the PR checklist (Korean), including the docs question
```

## Keeping the tree honest

- Every document owns one topic. When you learn a fact, put it in the owning leaf. A router
  says only *when* to go to a child, never the child's facts.
- A **router** is a node marked `[router]` in this full tree, plus the two roots, this file, and every
  `.agents/skills/*/SKILL.md`. It shows its own subtree in a fenced tree, and `doc_check.sh` treats
  exactly these nodes as valid parents. To turn a leaf into a router, mark it in this tree and give
  it a tree of its own.
- **Shared nodes** appear in this full tree, and also in their direct parent router's tree when
  that parent is not this file. Each one names that parent in a `Parent:` line, or in a comment
  for scripts, the conf, and the `.github/` files. Shared trees list every node by its exact
  path, never with a wildcard.
- **Family nodes** are the files under `.claude/` and `.codex/`. They appear only in their own
  root's tree (`CLAUDE.md` or `AGENTS.md`), never here, and they name that root as parent. JSON
  files can't hold a comment, so their entry in the root's tree is the record. A root's tree may
  use a `<name>` placeholder for one file per shared skill.
- A root may also list a **discovery path**, marked `(discovery)`: the location where that family's
  tool finds a shared node (for example `.agents/skills/`). This is not ownership, and the node keeps
  its shared parent.
- When you add, move, or rename a node, update every place above that applies, in the same change.
  `.AI/tools/doc_check.sh` checks these rules, and CI runs it too.
- Create a new leaf only when no existing leaf owns its topic. A long section is a hint to split,
  not a rule.
- Family-specific instructions go only in `CLAUDE.md` or `AGENTS.md`. If both roots would say
  the same thing, it belongs here instead. The one deliberate duplicate is the first-action
  paragraph: `AGENTS.md` repeats it word for word, because Codex has no import. Edit both copies
  together.
- If code you changed makes a routed document wrong, update that document in the same change.
