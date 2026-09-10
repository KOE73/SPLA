# Git: branches, commits, and what an agent may do on its own

STOP — read this before you commit, branch, merge, push, delete a branch, or write a commit message.
Pushing to `work` carries one more rule — ask whether the batch is a release — which lives in
[release.md](release.md#before-pushing-to-work-ask-whether-this-is-a-release).

## A branch per piece of work, `work` integrates, `main` releases

**The owner routinely runs several agents on this repo at once.** One shared working tree cannot
hold that — uncommitted changes from one piece of work block another. So: a non-trivial piece of
work gets its own branch `<area>/<short-name>` (e.g. `security/connection-secrets`) in its own
`git worktree` under `.claude/worktrees/<name>`, not the shared checkout.

Those branches start from **`work`** and merge back into **`work`**. `work` is the integration
branch and carries the detailed history — every feature branch, every fix, every intermediate
commit. It is where the project actually lives day to day, and it is the branch a session is
normally on.

**`main` is not an integration branch.** It receives whole releases, not individual pieces of work:
the owner decides when a batch is worth releasing, opens a pull request `work → main` on GitHub, and
squash-merges it there. One squash = one commit on `main` = one release point, so `main` reads as a
list of releases rather than a transcript of how they were built. This replaces the older "merge
each branch into `main`" rule, which in turn had replaced "everything on main" — `main` stopped
being the merge target the moment releases got their own meaning.

**Never merge, push, or fast-forward anything into `main` yourself, and never open or merge the
pull request.** Not when the work is finished, not when its tests pass, not when the user says a
piece of work is done. Deciding that a release exists is the owner's call, taken on GitHub, and it
is deliberately outside what an agent does here. Merging a feature branch into `work` is a normal
action you may take **when asked**; `main` is not.

A quick fix that touches one or two files and is going to be committed in the same turn does not
need the branch/worktree ceremony — commit it on `work` and use judgement. When in doubt, branch; a
spurious branch costs a merge, a missing one costs someone else's uncommitted work.

**A push to `work` can be rejected as diverged even when nothing is actually lost.** The owner
merges pull requests on GitHub (including squash-merges) between sessions, which gives `origin/work`
commits your local branch never saw under those hashes — a normal, expected outcome of this
workflow, not a sign of conflicting work. Before asking the owner how to reconcile, check it
yourself: diff the subjects/content of the commits `origin/work` has that you don't
(`git log HEAD..origin/work`) against your own local history (`git log --all --grep=<key phrase>`,
or just read the diffs). If the content already exists locally under different hashes (rebase,
squash, or a same-change commit from elsewhere), say so and proceed to reconcile — merge or
force-push as the situation calls for — without making the owner re-derive what you can already see.
Only ask when the remote-only commits contain something genuinely not present locally.

**State the current branch near the start of a session — one of the first sentences, not buried.**
With several worktrees around, "which checkout is this" is not obvious from the chat alone, and the
cost of assuming wrong (editing on the wrong branch, merging the wrong thing) is high enough that a
one-line `git branch --show-current` up front is cheap insurance. Re-state it if the session switches
worktrees or branches mid-conversation — the same reasoning applies at that point, not just at
session start.

Alongside it, run `git worktree list` and `git branch --no-merged work` once and mention what they
show — what else is checked out, and what else has unmerged work — so the user does not have to ask
"what's out there" separately. Skip the mention only if both come back empty/trivial (just `work`,
nothing unmerged); a one-liner beats silence, but two empty tables are noise.

**On completion, and only when asked:** `git merge --no-ff <branch>` into **`work`**, then
immediately `git worktree remove` and `git branch -d`. Do not leave a merged branch/worktree lying
around — check `git branch --no-merged work` before assuming a branch is safe to drop, same as
before. Nothing about finishing a branch involves `main`.

**Do not commit, amend, push, tag, or reset anything unless the user asks for it in the message you
are answering.** Finishing a piece of work is not permission to record it. Neither is the work being
correct, tested, and obviously ready. This applies on a feature branch exactly as it does on `work`
— a branch is not a lower-stakes place to commit unasked. **Merging into `work` is its own action
and needs its own ask**, separate from the ask (if any) that authorized commits on the branch.
Anything aimed at `main` is not an ask you can satisfy at all (see above).

- A commit requested earlier authorizes **that** commit only. It does not stand for the next one, or
  for "everything from now on". If in doubt, you were not asked.
- Leave the work in the working tree and say what changed. The user decides when it becomes history.
- **Never stage with `git add -A` or `git add .`** when the tree holds changes you did not make —
  concurrent work by the user or another agent is normal here. Stage explicit paths, and check
  `git status` for files you never touched before every commit.
- Same rule for anything else that leaves the machine or is hard to undo: pushing, force-pushing,
  deleting branches, rewriting history.
- **Before deleting any branch, check what would be lost.** `git branch --no-merged work` and
  `git log work..<branch>`. A branch whose commits are all in `work` is free to delete; one with
  unique commits is not — preserve it as a tag (`archive/<name>`), push the tag, and say what was in
  it. Never let a delete be the reason work disappears.

**If `git status` looks clean but a file is visibly new in the editor, suspect a stale untracked
cache before anything else.** `core.untrackedcache=true` is a per-clone, unversioned `.git/config`
setting — it does not travel with the repo. When enabled, `git status` can silently miss newly
created files while `git status <path>` (or `--untracked-files=all`) still finds them, and the IDE
(which watches the filesystem directly, not the git index) shows the truth. Fix per machine:
`git config core.untrackedcache false`. Do this once per clone if you hit the symptom; it is not a
repo-wide setting you can ship in tracked files.

**Do remind, though.** Silence while changes pile up is its own failure — a large mixed working tree
is hard to review and easy to lose. Say something (one line, not a nag) when:

- more than roughly ten files are uncommitted, or
- the changes span several areas at once (`src/`, `web/`, `docs/`, `agents/`, `tests/`), or
- a self-contained piece of work just built and passed its tests — the natural place to draw a line.

State what is uncommitted, in which areas, and offer to commit. Then wait.

## Commit messages: the type is load-bearing

`<type>(<scope>): <subject>`, with `!` before the colon for a breaking change —
`feat(tools)!: a tool result is a ToolResult, not a string`.

| Type | Use for | Reaches the changelog |
| --- | --- | --- |
| `feat` | new observable behaviour | yes |
| `fix` | corrected observable behaviour | yes |
| `refactor` | internal restructuring, nothing observable changes | no |
| `docs`, `test`, `chore`, `build` | documentation, tests, housekeeping, build plumbing | no |
| any type with `!` | breaking change | yes, under Breaking |

This used to be a habit — pleasant, optional, and unevenly followed. It is now **load-bearing**: the
type decides whether a change is written into `CHANGELOGS/` (see [release.md](release.md)), so
getting it wrong is how work goes missing from a release rather than merely how a log looks untidy.
Scope is the area (`tools`, `llm`, `skills`, `security`, `web`, …), matching the zone names used in
`docs/` filenames.

Subjects are English, like branch and pull-request titles. Prose documentation stays Russian.
