# Changes — unreleased

The scannable list: one line per change, no dates, no detail. Derived from the entry headings in
`current-log.md` rather than written independently, so the two cannot drift apart. Frozen into
`CHANGELOGS/<version>.md` on release, then started empty again.

This list and the summary are what CI puts into the release body; the full log stays in the
repository and is linked from it.

**Covers work since `v0.2.3`**, frozen in [`CHANGELOGS/v0.2.3.md`](v0.2.3.md).

---

### Added

- Launch profiles: `spla init` enters a folder with `minimal` (default), `standard` or `inherit`.
- One writer per project, enforced by a lock file that also publishes the live instance's address.
- `spla ps` and `spla stop`: see what is running on this machine, and ask one of them to go.
- `spla hub`: instances register themselves with it, including from other machines; `spla ps
  --registry <url>` reads it, and `SPLA.Server --hub` hosts the same routes.
- Chat and instance states — idle, working, waiting, stalled — as sidebar badges, a tray icon that
  blinks while an agent is waiting for you, and an `instances` tab in the Debug surface.
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
- A unified resource address space (`scheme://authority/path`, plus opaque `blob:` addresses):
  `ResourceRegistry` maps scheme to provider, `file://` and `sftp://` ship as the first two, and a
  base read/exists/list verb set is mandatory to register at all. Six `resource_*` tools
  (read/exists/list/write/delete/mkdir) expose it to the model, entirely behind
  `agent.unified_resources` (default off, verified byte-for-byte inert when off).
- Resource reads carry a content type (`ResourceContent(Bytes, ContentType)`), not just bytes, backed
  by a one-hop format-converter registry (`(source, target)` MIME pairs); `resource_read`'s optional
  `as` picks the outbound type, defaulting to text-inline-or-blob-handle when omitted.
- An HTTP endpoint (`POST /mcp`, off by default) lets stdio-proxy MCP clients share one running
  instance.
- Strict CLI argument parsing: a misspelled option now exits non-zero instead of being ignored.
- A `documents` plugin: `document_extract` reads a Word `.docx` for its meaning (markdown, plain text
  or a typed JSON block tree), and `spreadsheet_inspect` / `spreadsheet_read_rows` /
  `spreadsheet_append_rows` read and extend `.xlsx`/`.csv` by column header rather than by cell
  address. The same extraction registers as `docx → markdown | text | json` pairs in the core
  converter registry.
- A tool call can run detached from its turn (`background: true`) and deliver its result as a
  message on the chat's next turn; `task_list` / `task_output` / `task_cancel` manage what is
  running. `system_run_shell`, `agent_spawn`, `agent_spawn_batch`, `web_fetch` and
  `ssh_session_exec` opt in; a background task's live progress reaches the chat window and survives
  the human's next turn instead of being cleared with it.

### Changed

- OS-specific desktop code (shell integration, self-relaunch, browser launcher) moved into its own
  `SPLA.Platform` library, out of `SPLA.UI.Avalonia`.

- `agent_spawn` takes a plain task; a skill is now optional rather than the only way to spawn.
- An instance holds a lease instead of an owner: it lives while somebody is connected or work is in
  flight, and only an idle one is ever dropped.
- A question outlives the window that triggered it; closing a window no longer answers "deny".

### Fixed

- A shell command that asks a question (`Overwrite? [y/N]`, credentials, `Do you want to continue?`)
  no longer hangs forever: the run comes back with the question, a session id and
  `Status: waiting_for_input`, and `system_resume_shell` / `system_kill_shell` answer it or end it.

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

- `AgentCallbacks.OnTokenUsage` removed — `OnLlmTurn` carries the whole turn outcome, and recording
  it is the pipeline's job now.
- `projectId` removed from the wire envelope: a project belongs to the connection, and a second
  project is a second connection.
- A folder without a manifest is no longer entered silently — profiles are chosen, not inherited by
  default, and a non-interactive run there fails instead of guessing.
