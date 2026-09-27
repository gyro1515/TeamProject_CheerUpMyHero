# AI working flow — shared by Claude Code and Codex

Parent: [`Docs/AI/PROJECT.md`](../Docs/AI/PROJECT.md) (the common root).

One copy serves both agent families. Don't put family-specific instructions here. They belong in
the family's root, `CLAUDE.md` or `AGENTS.md`, and §5 compares the two.

```
.AI/flow.md  [router]           ← you are here
├─ §1 Session folder            before any repository work (file writes, or longer investigations)
├─ §2 Change flow               steps, gates, report format
├─ §3 Risk subsystems           the canonical list
├─ §4 Claude ↔ Codex            shared files, Cate protocol
├─ §5 Runtime mechanics         per-family difference table
├─ .AI/tools/session.sh         the §1 tool
└─ .agents/skills/cross-review/SKILL.md  [router]   §2 steps 3 and 6
   ├─ .AI/reviewer.md
   ├─ .AI/cross-review.conf
   └─ .AI/tools/cross_review.sh
```

## 1. Session folder — before any repository work

Do this before the first file write, and before any investigation longer than a few reads.
Q&A and pure discussion skip it. Read-only reviewers launched by `cross-review` skip it.

- The SessionStart hook (§5) injects a `[session.sh]` line, which says whether a work session
  is already bound to this runtime. If you see no such line (the hook isn't trusted yet, or it
  failed), run `.AI/tools/session.sh current` instead: it answers the same question from the
  environment. If a session is bound, read its `STATE.md` and continue from there.
- Otherwise, if the user is continuing earlier work, bind to that session:
  `.AI/tools/session.sh bind <session-dir-name>`. If you don't know which session, run
  `session.sh status` and ask the user. Never guess.
- Otherwise create one: `.AI/tools/session.sh new <kebab-slug>`, which creates
  `.AI/sessions/<YYMMDD>-<slug>/STATE.md`.
- The script detects the agent family and the runtime session id itself. It finds the
  nearest `claude`/`codex` ancestor process, then reads `CLAUDE_CODE_SESSION_ID` or
  `CODEX_SESSION_ID`. Never invent an id. If detection fails, the script refuses, and you
  should report that. If detection picks the wrong family (one agent nested inside the other),
  set `AI_SESSION_AGENT=claude|codex` for the command. Always create and bind through the script: it is the one
  implementation both families share, so the folder layout and the `STATE.md` headings stay
  identical.

`STATE.md` is the only record that survives compaction, so keep it current:

- **요청**: the user's requests in close-to-original wording, including later corrections.
  Record a new request here before acting on it.
- **작업 목록**: one row per change, with its status.
- **결정 사항**: say who decided each one. Mark your own calls "(내 판단)" so the report can
  surface them.
- **미결 질문**, and **진행 로그**: timestamped, with troubleshooting and the fix for each problem.
- Update it at every step boundary, before waiting on the user, and right after a user
  correction. Bump the `- 갱신:` line.

Per-change artifacts (evidence bundles, reviewer verdicts, logs, diffs) go in
`.AI/sessions/<session>/<change>/`. `<task>` in any skill means that `<session>/<change>` pair.
After a compaction or resume, read `STATE.md` before anything else. Never pick a session by
directory mtime. A session ends only when the user says so: set `- 상태: closed`, and delete
the folder only when the user asks. `.AI/sessions/` is git-ignored.

## 2. Change flow

1. **Session**: §1.
2. **Read before designing**: the common root (`Docs/AI/PROJECT.md`), the document its
   tree routes the task to, and the code the change touches. For new
   scripts, prefabs, managers, or events, that includes `Docs/AI/module-rules.md`. Reuse an
   existing manager, base class, or pattern before adding a new one, and name the existing
   thing you considered.
3. **Plan review, risky changes only**: when the change touches a risk subsystem (§3) or
   crosses system boundaries, run `cross-review` in `integration` mode on the plan before writing.
4. **Implement**: surgical changes only. Don't reformat or refactor beyond the request.
5. **Mechanical gate**: run the Unity batch-mode compile from `Docs/AI/development.md` after any `.cs`
   change. Report the command, the exit code, and the error count. If it cannot run (the
   Editor has the project open, the Editor isn't installed, and so on), write `BLOCKED: <reason>`,
   which is not a pass.
6. **Post-diff review**: for any non-trivial change, run `cross-review` in `post-diff` mode.
   Trivial means comments, whitespace, or a rename the user dictated. Name that exemption
   in the report.
7. **Report** in Korean, in this order:
   - **목적** (what it was for)
   - **한 일** (what was done, and how)
   - **과정·트러블슈팅** (what went wrong and how it was resolved)
   - **결과** (the final state, with gate receipts, verdict headers, and open items)

   Also list the "(내 판단)" decisions.

When to commit, push, or open a PR (only on the user's explicit request) and the commit and
branch conventions are in `Docs/AI/development.md` → *Commits and branches*.

## 3. Risk subsystems (canonical list — `cross-review` points here)

- **Server data and UGS**: `BackendManager` (the request queue and retry, Economy
  write-lock conflicts), Cloud Save keys in `DB/Constants.cs`, Cloud Code modules
  (`Economy/`, `GachaV2/`) and their generated bindings.
- **Player data**: `PlayerDataManager` / `TileDataHandler` save shape. Changing a serialized
  field silently breaks existing cloud saves.
- **Currency, gacha, and rewards**: pity counters, reward grants (`RewardPanelUI`, `PostBox`,
  ad rewards in `AdManager`). Check that each grant happens exactly once and that failures are
  handled.
- **Static data tables**: Excel → ScriptableObject (`ExcelAsset` sheet and column names,
  `SetData` id collisions), and the `Resources/DB` load paths.
- **Unity serialization**: renaming a serialized field, class, or file, or moving it, breaks
  prefab, scene, and SO references and `.meta` GUIDs.
- **Lifecycle**: `SingletonMono` (auto-create on `Instance`, null after a persistent singleton
  is destroyed), scene transitions and `ISceneResettable`, and the object-pool get/release pairing.

## 4. Claude ↔ Codex collaboration

- **Same instructions**: both roots load the common root `Docs/AI/PROJECT.md`. Its *Keeping the
  tree honest* section owns the rules for what goes in the common tree and what goes in a root.
- **Same skills**: canonical skills live in `.agents/skills/<name>/`, which Codex discovers.
  `.claude/skills/<name>` is a symlink to that directory. Add new shared skills the same way.
- **Same tools**: `.AI/tools/*.sh` are the executable parts (sessions, cross-review), and
  both families call them identically.
- **Talking to the other session via Cate**: `cate panel list` shows the terminals. Before
  sending anything, `cate terminal read --panel <id>` and confirm the other agent is idle at
  its prompt. Only then use `cate terminal type --panel <id> "<message>"` and, after a pause
  of about a second, `cate terminal press --panel <id> enter`. An Enter sent right after the
  typed text can be lost. Read the panel again and confirm it shows `Working`; if the text is
  still sitting in the input box, press Enter once more. Keep the typed message to one line
  that points at a request file. Never type into a busy TUI or one that is waiting on an
  approval. Put the reply you want in a file under the session folder, and read that
  file instead of scraping the other agent's screen.
- **Same files**: only one writer at a time per file. If both families work in one session,
  both bind to it (§1), and each logs its actions in `STATE.md`.

## 5. Runtime mechanics (the only per-family differences)

| | Claude Code | Codex |
|---|---|---|
| Root (auto-loaded) | `CLAUDE.md`: imports `Docs/AI/PROJECT.md`, plus Claude-only notes | `AGENTS.md`: says to read `Docs/AI/PROJECT.md` first (no import), repeats the first action, plus Codex-only notes |
| Skills | `.claude/skills/cross-review` → symlink | `.agents/skills/cross-review` |
| Invoke a skill | Skill tool / `/cross-review` | read `.agents/skills/<name>/SKILL.md` (or `$cross-review`) |
| SessionStart hook | `.claude/settings.json` (tracked) → `session.sh hook claude`; matcher `startup\|resume\|clear\|compact` | `.codex/hooks.json` (machine-local, git-ignored: Cate merges absolute-path hooks into it) → `session.sh hook codex`; matcher `startup\|resume\|compact` (no `clear` entry; after a Codex clear, run `session.sh current`). Install per clone with `session.sh install-codex-hook`, then trust it once via `/hooks` |
| Read-only reviewer | `claude -p --agent reviewer` (`.claude/agents/reviewer.md`, tools Read/Grep/Glob) | `codex exec -s read-only` |
| Reviewer persona | `.AI/reviewer.md` (shared, prepended by `cross_review.sh`) | same file |
| Runtime session id | `CLAUDE_CODE_SESSION_ID` | `CODEX_SESSION_ID` (= `CODEX_THREAD_ID`) |
| Running `cross_review.sh` | normal Bash, in the background | **needs escalated permissions**: the Codex shell sandbox disables the network (`CODEX_SANDBOX_NETWORK_DISABLED=1`), but both reviewer CLIs call their APIs. Request escalation for the command up front, not after it fails |

The reviewer models and effort come from `.AI/cross-review.conf` (the team defaults), with
`.AI/cross-review.local.conf` overriding it per person (git-ignored). They never come from the
primary's own model, so on one machine a round gets the same reviewers from either primary.
Every verdict records the settings it ran with (`<task>/<family>-config-r<n>-p<k>.txt`, summed
up in `config-r<n>.txt`), and the round table shows them per row.
