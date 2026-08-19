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

- **`spla chat run` gained `--sys-prompt-file <path>`.** `--sys-prompt` only took inline text, forcing
  a long prompt variant onto the command line; this reads the same way `--prompt-file` already does.
  Combines with `--sys-prompt` if both are given — file first, then the inline text.

- **Fix (incomplete): `PublishAll.ps1` failed on its first CI run, before reaching any real build
  step.** `npm --prefix web install` used a relative `web` path; the working theory was that it
  resolved against the repo root instead of `web/` on the GitHub-hosted Windows runner, and
  `--prefix` was changed to an absolute path built from `$PSScriptRoot`. This did not fix it — see
  the next entry.

- **Fix (actual): `npm --prefix <dir> install` on npm 10.9.8 reads `package.json` from the process's
  working directory, ignoring `--prefix`, absolute or not.** Confirmed by direct reproduction —
  `npx npm@10.9.8 --prefix <web dir, absolute> install` from a directory with no `package.json`
  fails exactly like the CI run did; the same command on this machine's npm 11.9.0 works. `npm ...
  ci` and `npm run <script>` were not affected — which is why `ci.yml`'s `npm --prefix web ci`
  step had been passing the whole time and gave no reason to suspect `--prefix` itself. Changed to a
  real `Push-Location`/`Pop-Location` around a plain `npm install` / `npm run build`, which has no
  npm-version dependency to get wrong. Also bumped `node-version` in both workflows from `22` to
  `24`, matching the owner's machine (npm 11.9.0) — Node 22 is what actions/setup-node was pinned to
  and is what carried npm 10.9.8 into CI in the first place.

- **`spla chat run` reports what actually ran.** `--show-statistic` prints a per-cell report and
  `--show-statistic-file` writes it beside the result as a companion file — `--show-statistic-format`
  takes a comma-separated list (`json`, `yaml`, `md`), one file per format. The report names the model
  the provider says answered — not the one that was asked for, which differs under `model: auto` and
  wherever a cloud substitutes a dated build — along with the connection, provider and endpoint, the
  sampling and reasoning settings, token totals under the provider's own counter names, timing, status
  and where the output went. A failed cell still reports: which model on which endpoint failed, with
  which settings, is the case where the report is worth most.

- **The reasoning lever now reports what went on the wire, not what was asked for.** The report
  carries `reasoning_requested` and `reasoning_wire` side by side, and `(nothing sent)` is a normal
  answer: the lever is only sent for a model whose entry declares `reasoning_options`, so
  `--reasoning medium` against an undeclared model sends nothing at all. Observed as a payload diff
  rather than by asking the provider profile, so a dialect this build has never seen still reports
  correctly.

- **Fix: provider observations could not travel past the accounting stage.** `TurnOutcomeMiddleware`
  rebuilt the turn result field by field, dropping every field it did not itself set — `Signals` among
  them. Nothing above that stage could ever see what a provider volunteered. The model-info popup was
  unaffected: `ProviderStateMiddleware` is registered *inside* the accounting stage and read the
  signals before they were dropped. The result is copied now rather than rebuilt.

- **A provider fact says what it is a fact about.** `ProviderFact.Scope` separates the credential's
  standing (rate-limit budget, balance — worth storing, the next call needs it) from what was true of
  one call. `ProviderStateStore` keeps the latest list per connection, so without the distinction a
  response carrying only per-call facts would erase the last real budget reading — and against a local
  provider, which sends no rate-limit headers at all, that would be every single turn. One shape, one
  channel, one discriminator: the alternative was a second parallel list on the turn result.

- **Token accounting moved into the LLM pipeline, out of the hosts.** Six entry points — the two CLI
  paths, the service, and three demo workers — each subscribed a callback and repeated the same two
  `ITokenUsageStore.Record` calls. The seventh caller, spawned sub-agents, subscribed nothing, so its
  tokens reached the telemetry meter (written unconditionally from the agent loop) but never the
  project's tally: two ledgers of one fact, already disagreeing. `TokenAccountingMiddleware` now
  records both, at the `Accounting` stage, once per network attempt — which also means a regenerated
  or retried answer is counted as the second paid call it is, rather than folded into the first. The
  same argument the repetition guard was built on: a duty that must hold for every call belongs to
  the pipeline, not to whoever remembered to wire it.

- **`AgentCallbacks.OnTokenUsage` is gone; `OnLlmTurn` carries the same event, whole.** The narrower
  hook delivered a strict subset of what the wider one already carried, so two hooks fired about one
  thing and each host answered "how many tokens" from a different one. Hosts that showed a token line
  read it off `OnLlmTurn` now; hosts that only recorded it no longer do anything at all.


## 2026-08-19

- **Fix: a distributed build served 404 for every page — the web client was never embedded in it.**
  `SPLA.Service.csproj` added the `web/dist` glob to `@(EmbeddedResource)` from a target hooked
  `BeforeTargets="CoreCompile"`, and `CoreCompile` does not read that list: `PrepareResources` has
  already turned it into `@(_CoreCompileResourceInputs)`, which is what the compiler is handed. The
  items were being added to a collection nobody reads again, and `SPLA.Service.dll` shipped with zero
  manifest resources. `BeforeTargets="PrepareResources"` is not enough either — `BeforeTargets`
  appends to the target's `DependsOnTargets`, so it still runs after all of the resource processing;
  the hook is `AssignTargetPaths`, the first of those dependencies. A second bug sat behind the first:
  the logical name read `%(RecursiveDir)` from inside a property function on the very item being
  declared, which yields an empty string, so all six files claimed the name
  `SPLA.Service.WebClient.` and the compiler rejected the set with CS1508 — the duplicate-identifier
  error this file's history blamed on double target hooks. The glob is a separate item now and the
  metadata is read off it by name.

  It survived this long because the embedded copy is only ever read when `WebAssets` finds no
  `web/dist` on disk, and a publish made **into** the repository (`.publish\work`) walks up and finds
  the repository's own. Every build tested where it was built worked. Only a zip extracted somewhere
  else had nothing to fall back to.

- **The build fails now if the web client is not inside the assembly.** `VerifyWebClientEmbedded`
  errors when no resource named `SPLA.Service.WebClient.index.html` was produced. The failure this
  guards against is invisible in the tree that produced it and only appears on someone else's
  machine, which is the definition of a check worth paying for on every build.

- **`SPLA.CLI.exe` ships self-contained, like the desktop app already did.** It publishes through its
  own `SingleFile` publish profile instead of hard-coding `SelfContained=false` in the project body.
  The desktop app spawns this exe as its service child, so a self-contained shell over a
  framework-dependent child means that on a machine without the runtime the window opens, looks
  perfectly normal, and everything behind it is dead. Worse, the missing piece there is the *ASP.NET
  Core* runtime — `SPLA.Service` takes a `FrameworkReference` on it — which is not what someone who
  installed .NET would think to check. A plain `dotnet build` stays RID-less, so the dev tree keeps
  the `bin\<cfg>\<tfm>\` layout `EmbeddedServiceLauncher` walks.

- **A service child that dies on startup says why, instead of being reported as a 30-second timeout.**
  `EmbeddedServiceLauncher` captures the child's output and checks whether it is still alive between
  health polls: a process that has already exited will never answer, so waiting out the rest of the
  budget only delays the report and throws away the reason. The exit code is named where it is known
  — `0x80008096` is the apphost saying the framework is missing — and whatever the child printed is
  appended. With a dead child now reported the moment it dies, the budget for a live one rose from 30
  to 120 seconds: 30 was measured against a warm dev tree, and a first run from a zip has to unpack a
  self-extracting exe, be scanned by an antivirus while it does, and load the plugin folder before
  the listener opens.
