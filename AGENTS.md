# AGENTS.md

Shared project instructions for every coding agent (Codex reads this file directly; Claude Code
imports it from `CLAUDE.md`). This is the single source — edit it here, never a copy.

## Workflow — read first

Before any task that writes repository files, read `.AI/flow.md` and follow it. It covers:
the per-session work folder (`.AI/tools/session.sh`, which comes first, before any work), the change
flow and gates, the risk-subsystem list, the Korean report format, and how Claude and Codex
share skills and talk to each other through Cate. For verification or a second opinion, use the
`cross-review` skill (`.agents/skills/cross-review/SKILL.md`).

First action of any repository work: check whether a work session is bound to you. The
SessionStart hook's `[session.sh]` line tells you, or run `.AI/tools/session.sh current`.
If none is bound, run `.AI/tools/session.sh new <kebab-slug>`, or `session.sh bind <name>` to
resume one (`session.sh status` lists them). Agent and session id are detected automatically.
Then keep `.AI/sessions/<session>/STATE.md` current as `.AI/flow.md` §1 describes.

## Project

"Cheer Up, My Hero" is a 2D mobile strategy-defense game (like Nyanko/Paladog) with territory
management. It runs on Unity **2022.3.62f2** with URP 2D and the New Input System, and targets
Android (plus WebGL and Standalone). Code comments, commit messages, and the README are in Korean.

## Build / run / test

- There is no CLI build script and no lint. The Unity Test Framework is installed, but no tests
  exist outside third-party code. Builds and play-testing go through the Editor. The build scenes,
  in order, are `StartScene` → `MainScene` → `BattleScene` → `EmptyScene`. Play from `StartScene`,
  because it initializes the backend.
- Batch-mode compile check. Use this exact path, because Unity 6 editors are also installed on
  this Mac. It fails while the project is open in the Editor:
  `/Applications/Unity/Hub/Editor/2022.3.62f2/Unity.app/Contents/MacOS/Unity -batchmode -quit -nographics -projectPath . -logFile -`
- To run EditMode tests headless, add `-runTests -testPlatform EditMode -testResults <path>` in
  place of `-quit`. For scene or UI changes, play the affected scene. Test scenes are in
  `Assets/Z_ForTest/`.
- Scripting defines: `DOTWEEN` is set everywhere, and `USERTEST` on Android, Standalone, and
  WebGL. In `BackendManager`, a `USERTEST` player build uses the UGS environment `usertest`.
  Everything else, including the Editor, uses `dev`.
- Commits use a `[Feat]` / `[Fix]` / `[Refactor]` / `[Chore]` / `[Build]` prefix followed by a
  Korean summary. Feature branches are named `Feat_<Topic>_<Name>_<YYMMDD>` and merge into `Develop`.
  Commit `.meta` files together with their assets.

## Code style

C# uses 4-space indentation and matches the brace layout of nearby files. Types and public
members are `PascalCase`; locals and private fields are `camelCase` (some files use `_camelCase`,
so match the file). Keep one `MonoBehaviour` per file with the same name. The root
`.editorconfig` only holds C++ rules, so don't reformat unrelated code.

## Project docs — router tree

Read only the document your task touches. [`Docs/AI/PROJECT.md`](Docs/AI/PROJECT.md) maps tasks
to documents:

- [`Docs/AI/architecture.md`](Docs/AI/architecture.md): the systems as they exist. It covers
  which manager owns what, scene transitions, Excel → SO data, the UGS backend, UI and pooling
  Resources paths, the EventManager API, combat, and third-party code.
- [`Docs/AI/module-rules.md`](Docs/AI/module-rules.md): read it **before adding a script,
  prefab, manager, or event, or before connecting two systems**. It covers ownership, call
  direction, and when to use EventManager versus a manager call.
- [`Docs/AI/ai-workflow.md`](Docs/AI/ai-workflow.md): the AI setup explained for teammates
  (Korean). It says what is tracked and what is local, and how to set the cross-review models and
  effort per person.

Three facts you need even before routing:
- Reading `SingletonMono<T>.Instance` auto-creates the manager, and it returns null after a
  persistent one is destroyed.
- `UIManager.OpenUI<T>()` and `ObjectPoolManager.Get(PoolType)` load prefabs by **name** from
  `Resources/`.
- Third-party code is vendor code, so don't modify it. That covers `Assets/Plugins` (DOTween,
  UniTask), the packs in `Assets/Externals` (ExcelImporter, SPUM, …), and `Assets/GoogleMobileAds`.
