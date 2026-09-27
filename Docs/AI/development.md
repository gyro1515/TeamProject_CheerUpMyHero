# Development: build, compile gate, tests, commits, code style

Parent: [`PROJECT.md`](PROJECT.md). Read this before building, compiling, testing, committing, or
writing C#.

## Build / run / test

- There is no CLI build script and no lint. The Unity Test Framework is installed, but no tests
  exist outside third-party code. Builds and play-testing go through the Editor. The build scenes,
  in order, are `StartScene` → `MainScene` → `BattleScene` → `EmptyScene`. Play from `StartScene`,
  because it initializes the backend.
- **Batch-mode compile check** (the *Mechanical gate* in `.AI/flow.md` §2). Use this exact
  path, because Unity 6 editors are also installed on this Mac. It fails while the project is
  open in the Editor:
  `/Applications/Unity/Hub/Editor/2022.3.62f2/Unity.app/Contents/MacOS/Unity -batchmode -quit -nographics -projectPath . -logFile -`
- To run EditMode tests headless, add `-runTests -testPlatform EditMode -testResults <path>` in
  place of `-quit`. For scene or UI changes, play the affected scene. Test scenes are in
  `Assets/Z_ForTest/`.
- A first import, or any reimport, can re-serialize `Assets/Resources/DB/*.asset` from the Excel
  row classes (see [`architecture.md`](architecture.md) → *Static game data*). Compare `git status`
  before and after the gate, and report any asset diff it produced instead of committing it along.
- Scripting defines: `DOTWEEN` is set everywhere, and `USERTEST` on Android, Standalone, and
  WebGL. In `BackendManager`, a `USERTEST` player build uses the UGS environment `usertest`.
  Everything else, including the Editor, uses `dev`.

## Commits and branches

Commits use a `[Feat]` / `[Fix]` / `[Refactor]` / `[Chore]` / `[Build]` prefix followed by a Korean
summary. History also has lowercase prefixes, `[Test]`, and unprefixed commits; write new ones with
the capitalized forms above. Feature branches are named `Feat_<Topic>_<Name>_<YYMMDD>`. During team development they
merged into `Develop`. Every GitHub PR opened so far has targeted `main`, and `main` and `Develop`
may point to the same commit or drift apart. Before choosing a PR base, run
`git rev-list --left-right --count origin/main...origin/Develop`. Commit `.meta` files together
with their assets. Commit, push, and PR creation need the user's explicit request.

## Code style

C# uses 4-space indentation (a few files contain tabs; don't convert them) and matches the brace layout of nearby files. Types and public
members are `PascalCase`; locals and private fields are `camelCase` (some files use `_camelCase`,
so match the file). Keep one `MonoBehaviour` per file with the same name. The root
`.editorconfig` holds a global `charset = utf-8` and otherwise only C++ rules, so don't reformat
unrelated code. Some file names differ from their class name; see
[`architecture.md`](architecture.md) → *Naming traps* before renaming anything.
