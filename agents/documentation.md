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

## Translation Rule

When the user asks to translate documentation without explicitly naming a source file, target file,
or folder, assume the request applies only to user-facing README-style files.

Default behavior:

- Look in `docs/` for matching `readme_*.md` and `readme_*_ru.md` files.
- If there is an obvious matching pair, translate between those two files.
- If there are several possible candidates, use file names and last modified dates to infer the
  intended source, but only when the choice is clear.
- If the target is still ambiguous, ask a concise clarification question before editing.
- Do not translate or rewrite `agents/` files unless the user explicitly asks for agent documentation.
