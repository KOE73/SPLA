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

---

## 2026-08-19

- **A sub-agent can be given a plain task, not only a skill.** `agent_spawn` and `agent_spawn_batch`
  now take `input` as the required argument and `skill` as an optional one. Delegation is the point
  of a sub-agent, and a written procedure is only one way to describe work — requiring a skill for
  every spawn meant every ad-hoc sub-task needed a file written for it first, so in practice nothing
  got delegated unless someone had planned for it in advance. A pinned run is unchanged: the
  procedure is activated directly and its prompt stays frozen, so a stray `skill_deactivate` cannot
  delete the instructions the sub-agent was spawned to follow. A free-form run gets the chat's
  per-iteration recomposition instead, which is what lets it find and activate a skill for itself if
  one turns out to fit — in its own session, never the parent's.

- **`SPLA.CLI.exe --help-mcp` explains how to drive this build over MCP.** The text is
  `MCP_USAGE.md`, embedded in the exe and also shipped as a plain file beside it in the zip. It
  documents the things a foreign head otherwise has to discover by trial and error: that `mcp` must
  be `args[0]` and takes no project path, so the project is selected by the process's working
  directory; that the server is one long-lived process per session; that the tool list is the
  project's and varies; that credentials are configured ahead of time in the project and never
  passed by the caller; and that "ask" permission verdicts refuse rather than prompt, there being no
  window to prompt in. It also says what SPLA *is* — a project-scoped agent runtime rather than a
  tool bag — so the connecting head can suggest project-level fixes instead of working around them.

- **A folder is entered on purpose, and a launch profile says with what.** Running SPLA where there
  is no `*.spla` used to hand it *more* than a project would: no boundary, a passthrough sandbox, no
  edge accounting, and its chats and KV written into the shared `~/.spla` alongside every other
  project-less session — while its settings, connections and capabilities were inherited wholesale
  from the machine defaults. `spla init` now creates the manifest with an explicit profile:
  `minimal` (the default — no built-in features, no plugins, only the LLM connection is inherited),
  `standard`, or `inherit` for the old behaviour, which is still reachable but no longer what
  happens when nobody chose. A profile is a parameter and a template, never a field in the manifest:
  the file ends up holding the result, so an existing project means exactly what it always did. A
  non-interactive run in a folder with no manifest now fails with a message naming `--init` instead
  of silently deciding. The manifest search deliberately does not walk up the tree — a command acts
  where you are standing.

- **One process writes a project, and the lock says where the live one listens.** Nothing previously
  stopped `serve`, `chat run` and `mcp` from opening the same `.spla/` at once and racing over its
  chats, KV and token tally. A live instance now holds `<project>/.spla/instance.json` open with
  writes denied, and that one file answers both questions: who has it, and what address to use
  instead. Liveness is the OS's answer rather than a pid check — the handle closes on any death,
  including a kill or a crash, and the same deny-write is honoured over SMB, so a project on a share
  is protected from two machines and not just two processes. `chat run` on a busy project now
  attaches to the live instance as a client, which is why a scripted run shows up in the window that
  already has that project open.

- **A question outlives the window that triggered it.** Permission and clarification requests used
  to live on the client connection: closing a window answered every outstanding one with "deny", and
  the turn finished by refusing everything it had been about to do. They now belong to the chat, so
  any window watching it can answer, a window opened later is shown what is still pending, and a
  closed window means "nobody is looking right now" rather than "no". The wait is bounded by
  `agent.ask_timeout_minutes` (60 by default, 0 for no limit) and timing out says so instead of
  pretending a person decided.

- **An instance holds a lease instead of an owner.** Open project A, open project B, go back to A,
  close the window, then remember B — under "whoever spawned it kills it", that killed B mid-turn.
  Nothing owns an instance now: it lives while somebody is connected or work is in flight, and lets
  go after a grace period when neither is true. Only a genuinely idle one is ever dropped; a running
  turn, a question waiting on a person, and a turn that stopped halfway are exactly the states
  somebody walks back to their desk for. Leaving is cheap because an instance holds nothing unique —
  chats, KV and the tally are on disk — so eviction costs the next warm-up and never any work.
  `spla ps` lists what is running and asks each one what it is doing; `spla stop` asks one to go and
  is refused while it is busy, `--force` cancels first. The desktop shell no longer kills its
  service child on exit; it passes `--idle-timeout` and lets the child decide, and the next window
  finds and joins it through the project's lock.

- **`spla hub` gives a machine, or a network, one place to see what is running.** The lock files
  answer "what is running here" and nothing more — a `serve` on another host cannot be discovered
  that way at all, since there is no shared filesystem and a pid means nothing across machines. So
  an instance can instead register itself with a hub whose address it was given
  (`spla serve --registry http://host:5060`), and the registration channel *is* the liveness signal:
  the socket closing is the instance leaving, however it left. Nothing to prune, no heartbeat to
  tune, identical on one machine and twelve. `spla ps --registry <url>` reads it; the hub relays a
  stop but never performs one, because only the instance knows whether it may go. It is a mode of
  the same binary, not a second program, and `SPLA.Server --hub` maps the same routes rather than
  reimplementing them.

- **You can see, from across the room, that an agent is waiting for you.** Chats carry a state on
  the wire — idle, working, waiting, stalled — in the same vocabulary the instance and the eviction
  rule use, so there is one set of words rather than three. The sidebar badges each chat and
  aggregates above the list, and the chat list is re-sent the moment a question appears or clears,
  because a badge that arrives a turn late is not a badge. A tray icon lists every instance on the
  machine, opens or unloads one, and blinks while any of them is waiting; it reads a push channel on
  the hub rather than polling, for the same reason. The Debug surface gained an `instances` tab: the
  full dump of what the process thinks it is doing, what its lock claims, and every question it is
  blocked on.

- **A project is now a property of the connection, not of every message.** `projectId` is gone from
  the wire envelope. It made every sender responsible for remembering it, and forgetting it once
  wrote settings into whichever project the connection happened to default to — the web client
  carried a helper and a warning comment for exactly this, in some forty places. It also made a
  local claim untrue: a process has one working directory, so a window "holding" several projects
  was telling the truth about its runtimes and a lie about anything resolved relative to it. A
  second project is a second connection: locally a second window, on a server one socket moved by
  `project.open`. The desktop shell no longer changes the working directory at all — that belongs to
  the serve instance, which holds exactly one project by construction.

- **The Built-in tools panel explains what each `core.*` toggle actually does.** Every row now
  carries a short blurb and the literal tool names it registers, plus a note that turning one off
  removes tools from `McpHost` rather than just trimming the prompt — which is why it needs a
  restart. The open question about making tool registration hot-reloadable is recorded in a plan doc
  rather than solved here.

- **Fix: a chat save no longer empties the file readers can see.** `SaveChat` used
  `File.WriteAllText`, which truncates before writing — anyone reading mid-write got an empty or
  half-written file, and `ListChats` silently skips whatever it cannot parse, so the symptom was a
  chat missing from the sidebar for one refresh. The race was already there; the instance work just
  shifted timing enough to provoke it roughly one run in three. Writes now go to a temporary file and
  are renamed into place — atomic on both Windows and POSIX — so a crash mid-write leaves the
  previous version intact instead of a partial one.

---

## 2026-08-20

- **`SPLA.Platform` holds the OS-specific desktop code.** `WindowsShellIntegration` moved out of
  `SPLA.UI.Avalonia`, the duplicated self-relaunch resolution logic was consolidated into this
  shared, UI-free library, and a cross-platform browser launcher replaced the inline `Process.Start`
  behind "open in browser".

- **CLI argument parsing is strict now.** `config.UseStrictParsing()` on the main command parser
  means a misspelled option like `--idle-timout` produces a non-zero exit instead of being silently
  ignored, across every command that goes through the Spectre parser.

- **Fix: CLI help text stays English regardless of the machine's UI culture.** Spectre localises its
  own help chrome and, with no culture pinned, followed `CurrentUICulture` — so a Russian Windows
  printed Russian headings around English command and option descriptions. `SetApplicationCulture` is
  now a single shared helper both `CommandApp`s go through.

- **The registry holds participants, not only agents.** A registration now carries a role — agent,
  window, or hub — which is the one missing concept behind three symptoms at once: nothing could
  raise a window that already existed, and closing a project stopped its agent while leaving its
  windows pointed at a service that would never answer again. Two new relays ride on top of the
  existing per-instance stop: `focus`, addressed to one participant (what Open uses instead of
  starting a second window), and `stop-project`, addressed to everything on one project. Both are
  relays, not actions — a window decides by closing unconditionally, an agent may refuse mid-turn
  because it holds work nobody else has. A registration naming no role is still an agent, so nothing
  built before this breaks.

- **The hub may start agents, and the CLI can drive it.** `IInstanceSpawner` hands the hub a
  capability to start (off unless passed — a deployment that must not spawn gets 501, never a silent
  power nobody granted), while `RegistryHub` stays an index with no handles and no way to end
  anything. Refusing to start left a machine with no desktop unable to bring a project up at all,
  which is the case this exists for. New CLI: `spla start [project] [--registry]` brings an agent up
  and walks away — already-running counts as success; `spla stop --all --registry` closes the
  project, agent and windows together.

- **One tray shell per session, and Open raises the window you already have.** The tray was always a
  machine-wide view but existed per-process, so three open projects meant three identical icons. It
  now lives in its own process (`--hub`); every window asks for that shell and all but the first lose
  a session-scoped mutex and exit. Open now asks the hub whether a window already has the project and
  raises it instead of launching a duplicate. Unload became Close and Kill, both addressing the
  project (agent and windows together) instead of only the agent.

- **Fix: a window whose agent went away says so, instead of retrying in silence.** Closing an agent
  left its window retrying every 1.5s forever with nothing on screen — from the outside the window
  had simply stopped working. The client now backs off (0.5s to 15s) and, after several consecutive
  failures, shows a banner that the connection is lost and offers a way out, without hiding the
  conversation underneath it. "Restart the agent" appears only in the native shell, which is the only
  one that can start one.

- **Fix: the lost-connection banner fires in about three seconds, not nine.** The original threshold
  assumed a restarting service would reconnect before it fired; a real restart takes longer than any
  threshold worth waiting for, so the extra six seconds bought nothing. Being wrong early is cheap —
  the banner clears itself the moment the socket returns.

- **A project manager, served by the hub.** The hub now serves the same web bundle every other host
  does, with `/` redirecting to the manager surface. `/registry/projects` merges the machine's
  remembered project list with what is currently registered, so a project with nothing running — the
  row somebody came to press Start on — is visible too; `/registry/forget` drops a remembered entry
  without touching the project itself. This is also where a refused Close is finally reported, since
  the tray has nowhere to put a message without stealing focus.

- **The project manager is reachable from the tray, in a frame or a browser.** `SurfaceWindow` gained
  an explicit base URL, since the hub surface is served by the registry hub rather than by a
  project's own agent service and the tray shell that opens it never starts one. Offered both ways
  because a browser tab outlives the tray shell being restarted and can be opened from another
  machine, where there is no tray at all.

- **Fix: the hub moved off port 5060, which browsers refuse to open.** 5060 is SIP's port and sits on
  the blocked list of every Chromium browser and Firefox, so the project manager failed outright with
  `ERR_UNSAFE_PORT`. Default is now 5077.

- **`SPLA_HUB_PORT` overrides the hub port, with one shared resolver.** Order is `--port`, then the
  variable, then the built-in default. Resolved in one place because the CLI running `spla hub` and
  the shell looking for one have to agree on this number without talking to each other; the browsers'
  blocked-port list lives there as data, so picking one of them now produces a named warning instead
  of a bare `ERR_UNSAFE_PORT`.

- **MCP reports progress for every tool, and a call can be withdrawn.** A foreign head calling a SPLA
  tool over MCP saw nothing until the result. `tools/call` now opens a progress tree when the client
  sends `_meta.progressToken`, since every tool already reports through `ProgressScope` — nothing
  tool-specific was needed. Progress writes are serialised so a second writer on the pipe cannot
  produce a stray line, and the call no longer holds the read loop, which also means a cancellation
  or keepalive sent mid-call is now reachable instead of stuck behind it.

- **A spawned agent reports into its caller's progress tree.** A spawn used to open a progress tree
  of its own, detaching everything below it — the sub-agent's tool calls landed nowhere anyone was
  subscribed to while the caller's node sat silent. The fix leaves the caller's tree in place so the
  sub-agent's calls become children of its node, which every existing renderer (CLI status line,
  Avalonia and web tool trees, MCP `notifications/progress`) then shows with no further wiring.

- **Fix: every spawn tick says which task it is about.** The node's label was the literal string
  `"agent_spawn"` for every delegation, so a batch of five read as five identical rows. Naming the
  task once at the start did not survive tick coalescing, since an opening line is exactly what a
  faster-arriving neighbour overwrites.

- **Fix: each spawned run in a batch gets a branch of its own, named after its task.** A batch ran its
  tasks on parallel flows that all inherited the same current node, so three sub-agents hung their
  tool calls off `agent_spawn_batch` as one undifferentiated row of siblings — showing what had been
  done but not who did it. The tree now reads `agent_spawn_batch › audit ports › port_scan`.

- **The progress tree reaches clients, and is rendered.** `OnProgressTree` had existed since progress
  was built with no subscriber; native clients saw less than a foreign head over MCP, since
  `tool.progress` only reports the top-level call. `progress.node` now carries one node per change,
  flat and append-only rather than a snapshot, alongside `tool.progress` — structural frames (a
  node's first appearance and its finish) are never throttled, only the ticks between them are, and
  per node, so a scan reporting per host cannot silence a tool running underneath it.

- **Fix: a sub-agent's activity reaches the flat progress bar, and shows how full its context is.**
  The single-bar view forwarded root nodes only, so once a spawned run got a node of its own the CLI
  status line and the service's flat progress showed a spinning `agent_spawn` saying nothing for
  minutes. The bar now follows the newest tick at any depth. Alongside it, a context-fill figure rides
  every tick — there is no percent-done for an agent, but there is a ceiling it is walking towards,
  which is what makes a runaway legible before it gets expensive. No percentage is shown when the
  model's window is undeclared, since a bar against a guessed denominator is a number nobody can act
  on.

- **The loop guard is on by default for chats, and the rotted 2026-07-10 review is gone.** A chat had
  no repeat guard unless a config turned it on, while a spawned run has had one unconditionally since
  spawning existed — backwards, since the chat is the one with a person paying for the tokens. The
  guard cannot fire on merely-repetitive work: it needs the same tool, same arguments, same result, no
  accompanying text, and a round under ten seconds, all consecutively, and the first trip only asks
  the model whether it is stuck. Still switchable per project. Also records the turn-budget decision:
  no automatic cutoff, since no number is legitimate for every long run — visibility (turn, narrative,
  context fill, a working Stop button) is what replaces it.

- **The context-fill percentage is now a real percentage.** Config almost never declares a context
  length, so the figure added by the previous entry was in practice a bare token count with nothing to
  compare against. The runner now asks the provider for the window the same way chats already do,
  waiting up to two seconds rather than leaving it to arrive on its own — fire-and-forget made the
  figure appear only when a run was slow. A provider that is gone is cached as gone.

- **A spawned run keeps its transcript, and a client can read it back.** A sub-agent's whole
  conversation used to be thrown away the moment the tool returned, so a run that came back with
  something odd had nothing left to ask. Kept in memory, bounded to fifty most-recent runs per
  process — a batch of twenty spawns is twenty transcripts and nineteen are never read, so the default
  has to be cheap. A miss on read is `found: false`, not an error, since the ring is bounded on
  purpose.

- **A spawned run's transcript is visible in the web client.** Wires the `subagent.get`/
  `subagent.result` readback into the `agent_spawn` tool card: a "show sub-agent transcript" toggle
  fetches and renders the run's system/user/assistant turns.

---

## 2026-08-21

- **Resource reads carry a type, not just bytes, and a converter registry sits behind them.**
  `ReadAsync` returns `ResourceContent(Bytes, ContentType)` instead of a bare `byte[]` — the address
  says where something lives, the type says what came back. A converter registry
  (`SPLA.Domain/Formats`) registers by `(source, target)` MIME pair, one hop only, no path search
  across registered pairs. Three converters carry real traffic from day one: identity for `image/*`, a
  UTF-8 decoder that fails loudly on non-text bytes, and JSON→YAML; `image_view` now resolves through
  the registry instead of dead-ending on "not a viewable image". Six `resource_*` tools
  (read/exists/list/write/delete/mkdir) expose the address space to the model, one tool per verb since
  Effect/Risk differ per verb. `resource_read` takes an optional `as` target type — omitted, a safe
  default applies (text inline, binary to a blob handle, so an unlabelled video cannot fill the
  context window); given, it routes through the converter registry. Everything sits behind
  `agent.unified_resources` (default false), verified inert in the system prompt when off.

- **MCP gets an HTTP endpoint, off by default, plus two bugs found wiring up its settings.** `spla
  serve` can now map `POST /mcp` for stdio-proxy clients to share one running instance instead of each
  taking its own writer lease, gated by the `mcp:` project section (`enabled`/`port`) and off by
  default. Also fixes `ConfigLoader.GetSectionValue` missing the `"mcp"` case (saving MCP settings
  threw and silently failed to persist), and the Projects hub's "Open" button, which in the desktop
  shell only started the agent headless and pointed at the tray — it now asks the native host to open
  a window directly through a new `openProject` bridge message.
