# SPLA Documentation Layout

SPLA separates user-facing documentation from agent-facing instructions, and separates documents by
**how long they stay true** rather than by topic. The reasoning behind this layout — and what an ADR
is — lives in [`ADR_20260731_docs_genres-and-naming`](../docs/adr/ADR_20260731_docs_genres-and-naming.md).

## Folders and genres

| Path | Prefix | Contents | Lifecycle |
|---|---|---|---|
| `agents/` | — | agent-facing rules: architecture, conventions, permission models. **English only.** | must match the code; a mismatch is a bug |
| `docs/adr/` | `ADR_` | decisions: what was chosen and why, including what was rejected | **never edited** |
| `docs/plans/` | `PLAN_` | work plans, with a status line | edited as work proceeds, closed when done |
| `docs/ideas/` | `IDEA_` | ideas, insights, "would be nice" notes | become a plan or die |
| `docs/` | `readme_` | user-facing guides (`readme_*_ru.md` for Russian) | edited to stay correct |
| `docs/reviews/` | date | outside-eye audits of the code (see below) | **never edited** |

File name: `GENRE_YYYYMMDD_zone_short-name.md`. Two documents on one day → `YYYYMMDD-2`.

Both the folder and the prefix carry the genre, deliberately: the folder keeps the store tidy, the
prefix keeps the meaning attached to the file when the path is not visible — in search results, in a
diff, in a list of changed files, in a link from another document.

**Zone** is a closed list, derived from namespaces minus the `SPLA` prefix: `core`, `secrets`, `llm`,
`agent`, `service`, `web`, `apps`, `plugins` (refined in the name: `plugins_sql`), `editor`, `build`,
`docs`. A new zone is added by editing the ADR, not on impulse.

## Reviews: the outside eye

A file in `docs/reviews/` is produced by pointing a strong model at **the code and nothing else** —
no `agents/`, no ADRs, no plans, no explanation of intent. It hunts for what is dangerous, wrong or
suspicious.

The missing context is the point, not a flaw. Documentation tells the observer how things were meant
to be, and it promptly stops seeing how they actually are. Only someone who does not know that
something was "on purpose" will notice that the purpose was bad — or that the code does not match it.

A review is a **list of observations, never a verdict**. Sorting happens in a second, separate pass,
this time with the documentation at hand, and every finding lands in one of three buckets:

1. **Real defect** → fix it, or file a `PLAN_` / `IDEA_` if it cannot be fixed now.
2. **Deliberate** → an ADR already explains it; close the finding by pointing at it. **If no ADR
   exists**, the finding just proved the decision was never written down — write it. A decision that
   lives only in someone's head is indistinguishable from a bug to an outside reader.
3. **Rethink** → the finding stands and the old decision no longer holds → write a new ADR and mark
   the old one superseded.

The third bucket is why the exercise exists; the first two a linter could largely produce.

**Never edit a review.** It is an observation at a point in time. Fixing a finding does not change
the review — the outcome lives in the code, in a plan, or in a new ADR.

**Watch for the repeat.** If the same finding keeps landing in bucket 2 review after review, the
observer is not the problem: the code is failing to communicate its own intent. A comment is missing,
or a rule in `agents/`, or a name is misleading. Fix the opacity, not the finding.

Full reasoning: [`ADR_20260731-2_docs_reviews-fresh-eyes`](../docs/adr/ADR_20260731-2_docs_reviews-fresh-eyes.md).

## Rules that matter

- **Never edit an `ADR_`.** A decision that changed gets a *new* record, and the old one gets a
  single line pointing at its replacement. Editing it to "keep it current" destroys the only record
  of how the thinking evolved — which the code can never show, because code only shows the latest
  state.
- **Status** appears only in `ADR_` (accepted / superseded by …) and `PLAN_` (in progress / closed).
  Genre and date are already in the file name; repeating them inside only drifts.
- **The word `DESIGN` is not used.** It meant intent, description and plan at once, which is exactly
  what has to be told apart.
- **Describing how something works now belongs in `agents/`**, not in a design document. That is the
  only genre obliged to track the code.
- When moving documents, use `git mv` so history survives.

## Docs across parallel branches (`docs/ideas`, `docs/plans`, `docs/adr`)

Branch-per-piece ([git.md](git.md)) solves code conflicts; these files fail differently — usually
not a git conflict at all, which is the dangerous case, since nothing forces anyone to notice.

- **Naming collisions.** `GENRE_YYYYMMDD_zone_short-name.md` (see above) already carries a `-N`
  suffix for same-day files (`IDEA_20260813-2_...`, `IDEA_20260813-4_...`) — use it. Before picking
  a slot, check the next free `-N` across **`git log --all`**, not just the current branch: two
  branches started the same day and merged later can otherwise both land on `-2` — git merges that
  cleanly as two distinct files with near-identical names, so the collision is invisible until a
  human reads them.
- **ADRs never get edited — including to resolve a conflict.** If a later ADR reaches a different
  conclusion than an earlier one, it says so explicitly ("supersedes ADR_YYYYMMDD_..."); it does not
  rewrite the old file's answer. This already follows from "ADR = record of how the thinking
  evolved", but the parallel-branch case is where forgetting it actually bites: two branches can
  each honestly believe their ADR is the current answer.
- **STOP-marked files under `agents/`** (protocol.md, secrets.md, toolsets.md, composition.md,
  skills.md, …) declare themselves authoritative over specific code. If your branch changed code a
  STOP-file governs, updating that file is part of the same merge, not a follow-up — a docs/code
  split that survives the merge is exactly the drift these files exist to prevent. Check this when
  merging *any* branch into `work`, including one you did not author.

## Translation Rule

Terminology for any Russian text — including which English terms of art must not be translated
literally — lives in [`glossary.md`](glossary.md).

Any file under `agents/` that is updated must have its Russian translation in `docs/` updated in the
same commit. Translation target: `docs/<same-name>_ru.md`. Exception: files with no existing `_ru`
counterpart do not require one unless explicitly requested.

When the user asks to translate documentation without explicitly naming a source file, target file,
or folder, assume the request applies only to user-facing README-style files.

Default behavior:

- Look in `docs/` for matching `readme_*.md` and `readme_*_ru.md` files.
- If there is an obvious matching pair, translate between those two files.
- If there are several possible candidates, use file names and last modified dates to infer the
  intended source, but only when the choice is clear.
- If the target is still ambiguous, ask a concise clarification question before editing.
- Do not translate or rewrite `agents/` files unless the user explicitly asks for agent documentation.
