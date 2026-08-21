# Changes — unreleased

The scannable list: one line per change, no dates, no detail. Derived from the entry headings in
`current-log.md` rather than written independently, so the two cannot drift apart. Frozen into
`CHANGELOGS/<version>.md` on release, then started empty again.

This list and the summary are what CI puts into the release body; the full log stays in the
repository and is linked from it.

**Covers work since `v0.2.3`**, frozen in [`CHANGELOGS/v0.2.3.md`](v0.2.3.md).

---

### Added

- Continuous integration: solution build, .NET tests, web type-check, bundle and vitest, on every
  push to `work` and every pull request into `main`.
- Release automation: merging into `main` re-runs the checks, publishes apps and plugins, and
  attaches `SPLA.zip` to a GitHub release; a manual run does the same against any ref.
- MCP: SPLA tools served over stdio, usable by an external agent.
- Mounts: folders outside the project root, declared by name, each its own security zone.
- Multi-project service: several projects at once, with a project picker in the client.
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

### Changed

- OS-specific desktop code (shell integration, self-relaunch, browser launcher) moved into its own
  `SPLA.Platform` library, out of `SPLA.UI.Avalonia`.

- `agent_spawn` takes a plain task; a skill is now optional rather than the only way to spawn.
- An instance holds a lease instead of an owner: it lives while somebody is connected or work is in
  flight, and only an idle one is ever dropped.
- A question outlives the window that triggered it; closing a window no longer answers "deny".

### Fixed

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
