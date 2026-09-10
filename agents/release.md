# Releases, CI, and the changelog

STOP — read this before every push to `work`, and before touching `CHANGELOGS/`, the version in
`Directory.Build.props`, or `.github/workflows/`. Branches, commits and the ban on touching `main`
are in [git.md](git.md).

## CI and releases: the release is decided in a commit, not by CI

Two workflows under `.github/workflows/`, both on `windows-latest` (`SPLA.Tests` targets
`net10.0-windows` and proves itself against a real `WindowsIdentity`; `PublishAll.ps1` is
PowerShell and registers a file association):

- **`ci.yml`** — pushes to `main`/`work`, and pull requests into `main`. Builds the solution, runs
  `SPLA.Tests`, runs the web client's vitest suite, and checks that the changelog summary is
  current. The web type-check and bundle come free with the solution build, which already shells out
  to npm through `Exec` targets. On `work` this is a signal; on a pull request into `main` it is the
  gate.
- **`release.yml`** — any push to `main`, or a manual run from the Actions tab. Under this
  branching model `main` receives nothing but releases (see [git.md](git.md)), so a push to it *is*
  the release decision, already taken when the pull request was squash-merged. There is no `paths`
  filter and no other trigger deciding whether "this counts" — every push to `main` is a release by
  definition.

**CI does not decide the version, does not write the changelog, and does not compose the release
body. All three are decided locally, before the push that makes them real, and CI only checks that
what was decided is actually there:**

| What | Decided where | CI's job |
| --- | --- | --- |
| Version number | `Directory.Build.props`, by hand, in the release commit | read it, refuse if that tag already exists |
| Frozen changelog | `CHANGELOGS/v<version>.md`, written in the release commit | refuse if it is missing |
| Release body | `CHANGELOGS/v<version>-notes.md`, written in the release commit | attach it verbatim (`generate_release_notes: false`) — CI does not assemble prose from `current-*.md` |
| Working changelog reset | `current-log.md` / `current-list.md` / `current-summary.md` emptied in the release commit | refuse if any still holds content past its preamble |

CI's role is a gate that either finds all four already true, or fails without touching anything.
Nothing it computes is allowed to change what gets published — that was exactly the failure mode
behind [`v0.2.4`](../CHANGELOGS/v0.2.4.md): a `run_number` nobody could reproduce locally, and a
release body assembled from working files whose staleness check the actual damage slipped past (see
`ADR_20260818-3_deterministic-release-commit`, which supersedes
[`ADR_20260818-2_build_versioning-and-changelog`](../docs/adr/ADR_20260818-2_build_versioning-and-changelog.md)
on this point).

**The version still has three parts**, `0.<minor>.<build>`, and Major/Minor are still moved by hand,
rarely, to mark a new chapter. What changed is `<build>`: it is now a plain number set by hand in the
same release commit as everything else in the table above, not `release.yml`'s `run_number`. A local
build with `PublishAll.cmd` still falls back to `0.<minor>.0` — the build number only means something
once it has been chosen for an actual release.

**Making a release, end to end:**

1. **Ask first — see "Before pushing to `work`" below.** Everything after this point only happens
   once the owner has said yes.
2. In one commit on `work`: bump `<build>` in `Directory.Build.props`; freeze `current-log.md` /
   `current-list.md` / `current-summary.md` into `CHANGELOGS/v<version>.md` and empty the three
   working files; write `CHANGELOGS/v<version>-notes.md` — the literal release body, not assembled by
   CI. Push.
3. The owner opens the `work → main` pull request and squash-merges it on GitHub, same as any other
   release — see [git.md](git.md). Nothing about this step changed.
4. The push to `main` runs `release.yml`. It checks the table above, runs the same tests as `ci.yml`,
   tags the `main` commit it is running against with the version from `Directory.Build.props`, runs
   `PublishAll.ps1 -VersionBuild <build>`, and creates the GitHub release from
   `CHANGELOGS/v<version>-notes.md` with `SPLA.zip` attached. If any check in the table fails, nothing
   is tagged or published.

The tag is created by the workflow, on the `main` commit it just built and tested — never by hand,
and never before that commit exists. This is different from step 2's local commit: that commit lives
on `work`, and a squash-merge gives it a new hash on `main`, so tagging it before the merge would tag
an object the release was never actually built from. **Do not tag and do not trigger a release**
yourself — same rule, and same reason, as not pushing to `main`.

### Before pushing to `work`: ask whether this is a release

**Every push to `work` — always, no exceptions for "just this once" — ask the owner in the same turn
whether this batch should become a release, before running `git push`.** Accumulating on `work`
without releasing is the normal state most of the time; the point of asking is that the owner decides
*when*, and a push is the last moment that decision is still open before the next session might not
think to raise it. A short one-line question is enough — "Push to `work`. Release this batch, or
keep accumulating?" — but it must be asked, not assumed either way. If the answer is yes, do step 2
above in the same push rather than a separate one; if the answer is no, push as normal and move on.

## Changelog: three working files under `CHANGELOGS/`

GitHub's generated notes list *merged pull requests*, and under this branching model there is
exactly one per release — the squash of `work → main`. Left alone, a release covering months of work
reads as a single line. So the notes are written, not generated:

| File | What it is | When it is written |
| --- | --- | --- |
| `current-log.md` | the detailed record, grouped by date | appended as work happens |
| `current-list.md` | one line per change, no dates | derived from the log's bold entry headings |
| `current-summary.md` | the prose account, by theme | rewritten from scratch **before each push** |

**What earns an entry:** something a person outside your own head would notice. Time spent is not
the test — an hour's fix that changes visible behaviour gets an entry, two days of refactoring that
changes nothing observable does not. In practice this is the commit-type table in
[git.md](git.md#commit-messages-the-type-is-load-bearing).

**The list is derived, never authored twice.** Write the log entry as a bold sentence that stands on
its own, then take that sentence for the list. Two independently written files drift; a derived one
cannot.

**Rewrite `current-summary.md` before pushing to `work`.** Not on every log entry — nine rewrites out
of ten would have no reader, and the summary costs roughly fifty log entries to produce. The push is
the right moment to consider it because it is the boundary where a release becomes possible at all:
before it, nothing can be released; after it, anything can. Update the `<!-- covers: YYYY-MM-DD -->`
marker to the newest date in the log when you do.

That marker is what makes staleness checkable: `ci.yml` warns when it trails the log on a push to
`work`, so a stale summary is visible long before it could end up composed into anything. It is no
longer `release.yml`'s problem — the release body is a file written by hand in the release commit
(see "CI and releases" above), never assembled from `current-*.md` at publish time.

**Publishable content starts after the first `---`.** Everything above it in these files explains
how the file itself works. This still matters for readability, but nothing reads it out mechanically
any more — `CHANGELOGS/v<version>-notes.md` is written directly, by hand, in the release commit.

**Coverage is checked against `git log main..work`, never against conversation memory.** Multiple
people and multiple sessions commit to `work`; a session only knows what it did, not what landed
alongside it. Before opening the `work → main` pull request, diff the two branches and confirm every
`feat`/`fix`/`!` commit has a log entry — a commit that arrived from outside the current session is
exactly as reportable as one written in it, and the log has no way to notice a missing entry on its
own. This already happened once: a CLI flag landed in the same window as a CI change, from a
different piece of work, and was merged into `main..work` with no entry until the gap was caught
during PR review.

### Freezing a release

Freezing — merging the three working files into `CHANGELOGS/<version>.md` (sections `Summary`,
`Changes`, `Log`), emptying them again, and writing `CHANGELOGS/v<version>-notes.md` — is not a
follow-up step after the release. It **is** step 2 of "Making a release, end to end" above: it
happens locally, in the same commit that bumps the version, *before* the push that becomes the
release. CI checks that it happened; it never does it and never writes to `work`.

**Check whether a past release is missing its freeze at the start of a session, mechanically:** if
`git tag` holds a release tag with no matching `CHANGELOGS/<version>.md`, something was published
without this step — under the current algorithm that should be impossible (CI refuses to publish
without it), so treat it as a sign the release predates this rule, or that a manual run bypassed it.
Say so. This is a comparison, not a recollection — do not rely on having noticed the release, or on
being reminded.
