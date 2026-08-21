# Changes — unreleased

The scannable list: one line per change, no dates, no detail. Derived from the entry headings in
`current-log.md` rather than written independently, so the two cannot drift apart. Frozen into
`CHANGELOGS/<version>.md` on release, then started empty again.

This list and the summary are what CI puts into the release body; the full log stays in the
repository and is linked from it.

---

### Added

- Continuous integration: solution build, .NET tests, web type-check, bundle and vitest, on every
  push to `work` and every pull request into `main`.
- Release automation: merging into `main` re-runs the checks, publishes apps and plugins, and
  attaches `SPLA.zip` to a GitHub release; a manual run does the same against any ref.
- MCP: SPLA tools served over stdio, usable by an external agent.
- Mounts: folders outside the project root, declared by name, each its own security zone.
- Multi-project service: several projects at once, with a project picker in the client.
- Launch profiles: `spla init` enters a folder with `minimal` (default), `standard` or `inherit`.
- One writer per project, enforced by a lock file that also publishes the live instance's address.
- `spla ps` and `spla stop`: see what is running on this machine, and ask one of them to go.
- `spla hub`: instances register themselves with it, including from other machines; `spla ps
  --registry <url>` reads it, and `SPLA.Server --hub` hosts the same routes.
- Chat and instance states — idle, working, waiting, stalled — as sidebar badges, a tray icon that
  blinks while an agent is waiting for you, and an `instances` tab in the Debug surface.
- Server deployment: domain identity over NTLM, per-user file areas, group sharing.
- DPAPI secret store with explicit scopes and per-entry ACLs.
- Skill library as a project of its own, fed by declared sources, with a librarian that answers by
  subject and one that reads the question.
- SSH: live pty sessions, SFTP transfer, upload as the mirror of download.
- Roslyn plugin: build, run and test .NET projects as tools.
- Browser plugin: first wave of Playwright automation with a screencast panel.
- OneC: Vue configuration browser.
- Headless batch runner in the CLI, now on `Spectre.Console.Cli`.
- `--sys-prompt-file` on `spla chat run`, reading a system-prompt addition from a file the way
  `--prompt-file` already does for prompts.
- Loop guard against degenerate LLM generation.
- Reasoning lever driven by what the provider advertises.
- Branch stamp on published builds.

- `spla chat run --show-statistic` / `--show-statistic-file` / `--show-statistic-format`: a per-cell
  run report (model that actually answered, endpoint, settings, tokens, timing) on screen or as a
  companion file in json, yaml or md.
- Run reports say what the reasoning lever became on the wire, next to what was requested.
- `SPLA.CLI.exe --help-mcp`: how to drive this build over MCP, embedded in the exe and shipped as
  `MCP_USAGE.md` beside it.
- The registry holds participants (agent, window, hub), addressable by `focus` and `stop-project`.
- `spla start [project] [--registry]` and `spla stop --all --registry`, and `spla hub` may now start
  agents through a host-provided spawner.
- One tray shell per session (`--hub`), with Open raising an existing window instead of duplicating
  it; Unload split into Close and Kill, both addressing the project.
- A project manager web page served by the hub, reachable from the tray, listing every project the
  machine remembers alongside what is currently running.
- MCP reports progress for every tool call (`_meta.progressToken`), and a call can be cancelled or
  kept alive mid-call.
- A spawned agent's tool activity reports into its caller's progress tree instead of a detached one,
  and reaches native clients over a new `progress.node` stream.
- A spawned run's context-fill percentage and full transcript are visible, including in the web
  client's `agent_spawn` tool card.
- The loop guard is on by default for chats, not only for spawned runs.
- Resource reads carry a content type, backed by a format-converter registry; six `resource_*` tools
  (behind `agent.unified_resources`, default off).
- An HTTP endpoint (`POST /mcp`, off by default) lets stdio-proxy MCP clients share one running
  instance.

### Changed

- The agent runs as a service; windows, terminals and remote clients are its clients.
- Avalonia became a window manager over one web renderer; the parallel native chat was deleted.
- All settings moved into the web client as one tabbed surface.
- Projects became storage brokers handing out named buckets instead of holding files.
- Four hand-rolled path checks became one boundary; a call is a movement between zones.
- Connection keys became secret references the settings editor never sees.
- Tool calls went through a pipeline instead of eight hand-wired concerns.
- `ILLMService` became a middleware pipeline behind one gateway; providers dispatch by `provider`.
- Plugin panels moved from Avalonia to the web client.
- Projects reorganized into a layered `src/` tree; `SPLA.Runtime` extracted.
- `docs/` split by lifetime into ADR, PLAN and IDEA.
- Version scheme is now `0.<minor>.<build>`, with the build number assigned by CI.
- `agent_spawn` takes a plain task; a skill is now optional rather than the only way to spawn.
- An instance holds a lease instead of an owner: it lives while somebody is connected or work is in
  flight, and only an idle one is ever dropped.
- A question outlives the window that triggered it; closing a window no longer answers "deny".

### Fixed

- A running turn in one chat no longer locks the composer in another.
- A trust flag survives a reload instead of resetting at exit.
- Web dependencies install when the manifests change, not once per checkout.
- A publish no longer fails because `git` is missing.
- The SSH terminal follows the window instead of the size its pty was born with.
- An SSH session can no longer wedge on a marker that never prints.
- The project tree shows every file rather than an extension whitelist.
- Provider observations survive the accounting stage, which rebuilt the turn result and dropped every
  field it did not set itself; and a per-call fact can no longer overwrite the connection's last known
  rate-limit budget.

- Token usage is recorded by the LLM pipeline rather than by each host, so callers that wire no
  callbacks — spawned sub-agents among them — are counted too.
- A distributed build serves the browser client instead of 404 for every page: the web bundle was
  never actually embedded, and a build made inside the repository hid it by finding `web/dist` on
  disk. The build now fails if the bundle is missing from the assembly.
- `SPLA.CLI.exe` ships self-contained like the desktop app, so an extracted zip no longer needs the
  ASP.NET Core runtime installed for the service behind the window to start.
- A service child that dies on startup reports its exit code and output instead of a bare health
  timeout, and a slow first start from a zip gets 120 seconds rather than 30.
- The Built-in tools panel explains what each `core.*` toggle registers and why it needs a restart.
- A chat save writes to a temp file and renames it into place, so a concurrent read can no longer see
  a truncated file.
- CLI help text stays English regardless of the machine's UI culture.
- The hub's default port moved off 5060 (browsers block it) to 5077; `SPLA_HUB_PORT` overrides it.
- A window whose agent went away shows a lost-connection banner (after several failed retries, backed
  off, in about three seconds) with a way out, instead of retrying forever in silence.

### Breaking

- A tool result is a `ToolResult`, not a string.
- A project's root is its manifest's own directory and cannot be moved.
- `.spla/skills` is gone; skills come from declared sources.
- `.spla` is no longer readable through the sandbox.
- `AgentCallbacks.OnTokenUsage` removed — `OnLlmTurn` carries the whole turn outcome, and recording
  it is the pipeline's job now.
- `projectId` removed from the wire envelope: a project belongs to the connection, and a second
  project is a second connection.
- A folder without a manifest is no longer entered silently — profiles are chosen, not inherited by
  default, and a non-interactive run there fails instead of guessing.
