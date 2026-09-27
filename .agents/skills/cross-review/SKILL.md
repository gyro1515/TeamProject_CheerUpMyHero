---
name: cross-review
description: Cross-review a plan, a bug diagnosis, or a finished diff with independent read-only reviewers from two model families (Claude and Codex; models and effort from .AI/cross-review.conf, overridable per person), fanned out over several perspectives, iterated to consensus. Use for post-diff validation of non-trivial changes, for plans touching a risk subsystem in .AI/flow.md §3, for disputed root causes, and whenever the user asks to 검증/체크/교차검증/크로스리뷰 something. Works identically from a Claude or a Codex primary.
argument-hint: integration | diagnosis | post-diff
---

# Cross-review (Claude ↔ Codex, multi-perspective)

Parent: [`.AI/flow.md`](../../../.AI/flow.md) (§2 steps 3 and 6).

```
SKILL.md  [router]                 ← you are here
├─ .AI/reviewer.md                 persona and verdict format; the script prepends it
├─ .AI/cross-review.conf           reviewer models, effort, cap (+ git-ignored .local.conf)
└─ .AI/tools/cross_review.sh       runs one round, validates verdicts, prints the table
```

Two model families review the same material independently, then review each other's verdicts,
until they converge. The primary session orchestrates: it writes the evidence bundle and the
prompts, runs `.AI/tools/cross_review.sh`, reads the verdict files, and judges convergence. It
is **never** one of the reviewers. The script launches fresh read-only CLI processes for both
families, so the procedure and the result are the same whichever family is the primary.

Two models agreeing is **not** verification: their failure modes are correlated. Consensus lets
you show a change to the user with confidence. It never replaces the mechanical gate in
`.AI/flow.md` §2 step 5 (the compile check), and it never authorizes something the user didn't ask for.

## Modes

- `integration` — a plan that crosses system boundaries, changes serialized or saved data, or
  touches a risk subsystem (`.AI/flow.md` §3 is the canonical list). Run it before writing.
- `diagnosis` — a disputed or unproven root cause. **Refuse to run without a reproduction or
  concrete evidence on record.** Reviewing a guess produces two confident guesses.
- `post-diff` — a finished change. Weigh depth by blast radius, not diff size: a one-line change
  to `BackendManager` or a serialized field can outweigh a 500-line UI change.

Trivial edits (comments, whitespace, a rename the user dictated) are exempt. Name the exemption
in the report, because an unrecorded "that was trivial" is how skipped reviews go unseen.

## Cost controls

1. **One question per round, fanned out across perspectives.** Ask one sharp question with
   explicit numbered claims to attack. For a small, single-surface candidate, use one
   perspective (two reviewers total). When the candidate has several distinct attack
   surfaces, write 2–4 perspective prompts, each with the same question plus its own named
   slice. Both families get the same slices, so a gap on one side shows against the other.
   The slice menu for this project is below. Pick the ones the candidate actually has, and
   number the chosen ones p1, p2, … in order:
   - runtime behavior and lifecycle (coroutines, `SingletonMono`, scene reset, pooling)
   - data and backend (UGS queue, Cloud Save shape, Economy/gacha, Excel/SO schema)
   - Unity assets and wiring (prefab/scene/SO references, Resources paths, `.meta`)
   - for workflow/config candidates: Claude↔Codex parity and fidelity to the user's request,
     and script/hook correctness
2. **One evidence bundle per candidate**, at `$T/evidence.md`. Keep it unchanged during a
   round. Preserve it as `evidence-r<n>.md` before revising it for the next candidate.
3. **A parsed three-line header** (`VERDICT` / `BLOCKERS` / `SUMMARY`), so convergence is a
   table, not two essays to reconcile. `.AI/reviewer.md` defines it.
4. **Three rounds maximum** unless the user explicitly authorizes more. Count across revisions
   of the same issue; a new diff doesn't reset the counter. An exhausted count never turns an
   unresolved finding into a pass.

## Setup

The task folder is the change folder of the current work session (`.AI/flow.md` §1):

```
T=".AI/sessions/<session>/<change>"      # e.g. .AI/sessions/260927-foo/fix-reward-grant
mkdir -p "$T"
```

**Write `$T/evidence.md` before launching anyone.** It contains:
- the setting and constraints, including what is already settled, so nobody spends a round
  re-arguing it;
- the material under review: the plan, the diagnosis with its reproduction, or the diff.
  Save a diff as `$T/candidate.diff` (`git diff > …`; for untracked files use
  `git diff --no-index /dev/null <file>` or quote them) and record the candidate identity
  (`git rev-parse HEAD` plus `shasum` of the changed files);
- facts and gate results already established: the compile receipt, or `BLOCKED` and why;
- **numbered claims to attack**, stated so that each one can be proven wrong;
- cited paths the reviewers can actually open. Reference files by path and symbol or heading,
  not by line number.

Then write one prompt per perspective, `$T/prompt-r1-p<k>.md`. Each contains the single
question, that perspective's slice, `Bundle: $T/evidence.md`, and the claims to focus on. The
script prepends `.AI/reviewer.md` (the shared persona and output format), so don't restate it.

## Rounds

```
.AI/tools/cross_review.sh "$T" 1            # all perspectives × {claude, codex}, in parallel
```

- Run it in the background when your harness supports that (Claude: `run_in_background`;
  Codex: a background shell), because a round takes minutes. The script waits for all
  reviewers itself.
- **Codex primary: request escalated permissions for this command up front.** The Codex
  shell sandbox disables the network, but both reviewer CLIs need it. Run inside the sandbox,
  every reviewer fails and the round comes back ONE-SIDED (exit 3). The reviewers stay
  read-only either way, because the script itself enforces `-s read-only` and the Claude
  tool allowlist. A Claude primary needs no escalation.
- Reviewer models and effort come from `.AI/cross-review.conf`, overridden per person by the
  git-ignored `.AI/cross-review.local.conf` (`Docs/AI/ai-workflow.md` §3). They are never the
  primary's own model. `MAX_PERSPECTIVES` there caps the prompts per round: if the round has more,
  the script refuses, so merge slices. Each verdict's own settings are recorded (`$T/<family>-config-r<n>-p<k>.txt`, summed up in
  `$T/config-r<n>.txt`) and shown in the table's MODEL/EFFORT column. A scoped retry after a
  settings change therefore shows mixed rows, not a relabelled round, and a verdict without that
  record counts as INVALID. It verifies Codex's banner
  (`model`, `sandbox: read-only`, `reasoning effort`) and Claude's `modelUsage`, and relaunches
  a reviewer once if it produced no valid header. The table always covers every perspective
  of the round × both families, including ones not relaunched this time. Exit code 3 means
  some reviewer in that set has no valid verdict (MISSING/INVALID), so the round is
  **one-sided**: say so, don't count it as a round, and fix the
  cause (quota, auth, trust prompt) before continuing. Retry only that side with
  `--only <family>` (and `--perspective <k>`).
- One run per round at a time: the script holds `$T/.lock-r<n>` and refuses a second run of the
  same round. If you interrupt a run, it signals its reviewers and keeps the lock. Check that no
  reviewer of that run is still alive, remove the lock, then rerun.
- Read the printed table (or `head -3 $T/*-verdict-r1-*.md`). Every reviewer has the same
  verdict and 0 blockers → **converged**, done. Otherwise, list only the *live* disagreements.

**Later rounds.** Write `$T/prompt-r<n>-p<k>.md` containing: the latest candidate and gate
receipts, paths to the previous round's verdict files **from both families** (each reviewer is a
fresh process with no memory), the short list of live disagreements, and what changed since
then. Then run `cross_review.sh "$T" <n>`. A reviewer that concedes when the other is right has
succeeded. When both agree that fix X is needed, that is consensus, even if a verdict line still
says `disagree`.

**After a fix, sweep the file it touched.** A round resolves the passage it quoted, not
necessarily the defect. Before calling a finding fixed, check the whole file for other
instances of the same kind of claim or bug, and state what the sweep covered.

**No consensus at the limit.** Stop and report both positions to the user in Korean, up to 5
lines each.

## After

Record the outcome in the session `STATE.md`: the verdict headers per round, what changed
because of the review, and what remains open. In the user report, give the outcome, not the
transcript. The verdict files stay in `$T` as the record.
