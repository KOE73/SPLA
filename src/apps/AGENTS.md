# src/apps — entry points (hosts & clients)

`SPLA.CLI` (REPL + `serve`), `SPLA.Server` (domain deployment: Negotiate, per-user areas, identity
DLL by config), `SPLA.UI.Avalonia` (desktop shell). Read the root `AGENTS.md` first.

## Invariants

- **Apps are thin; authority lives on the agent/service.** Permissions, secrets, memory, and project
  state belong to the runtime, never to a client/UI. An app assembles the stack and renders — it does
  not re-implement agent behaviour.
- **`SPLA.UI.Avalonia` is a WebView shell over the web client.** It spawns a child `SPLA.CLI serve`
  on loopback (`EmbeddedServiceLauncher`) and talks the same WebSocket protocol a browser does —
  "embedded = the client starts its own service." Do not add a second, in-process agent stack to the
  desktop app, and do not give it the ASP.NET dependency.
- **`SPLA.Server` references no platform.** The identity provider (Windows, later Linux) is a DLL
  named in `server.json` and loaded by reflection. Platform choice enters as data, not as a project
  reference — swap the DLL, not this host.
- **Server defaults must be conservative.** A network-reachable deployment should not default users
  into write/execute-capable modes; treat the safe mode as the server default and require explicit
  escalation.

## CLI surface is documented, and the doc is part of the change

`docs/README_CLI.ru.md` + `docs/README_CLI.en.md` list every command and flag the CLI accepts. Any
change to argument parsing (`SPLA.CLI/Program.cs`, `Cli/CliBootstrap.cs`, `Cli/ServeCommand.cs`,
`Cli/SecretCommands.cs`, `Cli/ChatCommands.cs`, `Cli/InteractiveRepl.cs`) updates both files in the
same commit. A stale reference is worse than none — it is followed literally and fails, and the
failure looks like a broken tool rather than a wrong document.

## Build/publish

Publish layout is flat (`<exe>\plugins\<name>`), independent of this source tree's folders. If you
move a project, update `PublishAll.cmd` / `PublishServer.cmd` / `RunServer.cmd` accordingly and run
`PublishAll.cmd` twice to confirm a clean build.
