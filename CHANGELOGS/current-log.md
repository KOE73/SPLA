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
