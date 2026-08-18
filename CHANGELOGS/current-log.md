# Log — unreleased

The detailed record, appended to as work happens and grouped by date. Never reordered, never
rewritten — this is the working log, not a narrative. Frozen into `CHANGELOGS/<version>.md` on
release, then started empty again.

An entry earns its place if someone outside your own head would notice the change. Time spent is
not the test: an hour's fix that changes visible behaviour gets an entry, two days of refactoring
that changes nothing observable does not. See the changelog rules in `AGENTS.md`.

Each entry is **a bold sentence saying what changed**, optionally followed by the detail. The bold
sentences are what `current-list.md` is built from, which is why they have to stand on their own.

> **Entries begin on 2026-08-18**, when this log was introduced. The cycle's earlier work is covered
> by `current-summary.md` and `current-list.md`; its commit-level detail is in git.

---

## 2026-08-18

- **Continuous integration runs on every push to `work` and every pull request into `main`.**
  Builds the solution, runs `SPLA.Tests`, and runs the web client's vitest suite. The web type-check
  and bundle come free with the solution build, which already shells out to npm through the `Exec`
  targets in `SPLA.Service.csproj` and `SPLA.Editor.Schema.csproj`. On `work` a red build is a
  signal; on a pull request into `main` it is the gate.

- **Releases are produced by CI rather than by hand.** A tag `v0.2.*` or a manual run re-runs the
  full check set against the exact commit being released, then runs `PublishAll.ps1` and attaches
  `SPLA.zip` to a GitHub release. Checks run *before* a manual run creates its tag, and the tag is
  deleted again if the publish fails afterwards — a tag this workflow created must never outlive the
  build it names. A tag pushed by a person is never removed automatically.

- **The version scheme became `0.<minor>.<build>`.** Three components: the minor is moved by hand
  when a new chapter starts, the build is the release workflow's run number and is never set by
  hand. The previous four-component scheme had a third component that changed only sometimes, so it
  carried no decision while still having to be read to identify a release.

- **`PublishAll.ps1` takes `-VersionBuild` and forwards it to every build and publish in the run,**
  so every binary in one ZIP reports one version. Run without it — which is what `PublishAll.cmd`
  does — the build falls back to `0.2.0`, the same as any other local build.

- **Release notes come from `CHANGELOGS/`, not from commit messages.** GitHub's generated notes list
  merged pull requests, and under the `work → main` squash model there is exactly one per release,
  so a release covering months of work would otherwise read as a single line.

- **A release is cut by merging into `main`, not by a second confirmation afterwards.** `main`
  receives nothing but releases, so a push to it already *is* the decision — taken when the pull
  request was merged. The workflow filters that push through an **allow-list of source paths**
  rather than an ignore-list of documentation: documentation is an open set (`CHANGELOGS/`,
  `docs/adr`, `docs/plans`, `agents/` all appeared over time) while source roots are closed and
  documented, and a filter over an open set has to be extended every time the set grows. `tests/`
  and `demo/` are outside the list on purpose — neither reaches `SPLA.zip`, and `ci.yml` has already
  run them.

- **The tag-push trigger was dropped rather than kept alongside the path filter.** GitHub's
  documentation does not define whether a `paths` filter applies to tag pushes, and the failure mode
  of guessing wrong is a release that silently does not happen. A manual run against any ref covers
  the same ground and still produces a correctly numbered tag.

- **Fix: `PublishAll.ps1` failed on its first CI run, before reaching any real build step.**
  `npm --prefix web install` used a relative `web` path; on the GitHub-hosted Windows runner
  (checkout under a subst'd `D:\a\...` drive) it resolved against the repo root instead, and npm
  reported it could not find `package.json` there. `--prefix` now takes an absolute path built from
  `$PSScriptRoot`, which cannot be misresolved this way regardless of the exact mechanism. Confirmed
  against the actual CI log, not reproduced locally — the same script ran clean in a plain local
  checkout.
