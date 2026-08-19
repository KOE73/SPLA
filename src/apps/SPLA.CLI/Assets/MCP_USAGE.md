# Using this SPLA build as an MCP server

`SPLA.CLI.exe mcp` serves this build's tools to a foreign head (Claude Code or any
other MCP client) over stdio. This file exists so a foreign head does not have to
rediscover any of the following by trial and error.

## What you are connecting to

SPLA is not a tool bag. It is an agent runtime whose unit of everything is the
**project** — a `*.spla` file plus its folder. The project decides which LLM
endpoint is used, which plugins and toolsets are loaded, which mode the agent runs
in, which SSH/SQL/other connections exist and under what credentials, and which
skills are available. Two projects on the same machine give you two different tool
lists and two different sets of reachable hosts. That is the design, not a
limitation: the project is the security and configuration boundary.

What this means for you, practically:

- **Read `tools/list` per connection.** Nothing about the tool set is fixed.
- **The tools are already configured.** `ssh_run` against a host named in the
  project needs no credentials from you — the project holds them (see Secrets).
  You are being handed a pre-wired environment, not a generic shell.
- **You can suggest project-level work to the user.** If a task keeps needing a
  host, a connection, or a procedure that does not exist yet, the fix usually is
  not a cleverer tool call — it is the user adding it to the project (a
  connection, a credential, a skill). Say so.

### Skills

A **skill** is a procedure the project's owner wrote — a curated, repeatable way to
do one job (`linux-host.capture`, `network.range-audit`). `skill_find` searches the
catalogue, `skill_activate` pins one to the session. When a skill covers the work,
using it beats improvising: it encodes what the owner actually wants done.

When nothing in the catalogue fits, do the work directly. But if it is work the
user will clearly repeat, suggest writing a skill for it — that is how the
catalogue grows, and it is a suggestion the owner is usually glad to get.

### Modes

`Chat | Research | Inspect | Edit | Agent` — a capability ceiling, not a
personality. It decides which tools are even visible and which are permitted.
`Research` is read-only-ish; `Edit` and `Agent` can change things. The project sets
the default; `agent_spawn` can pick a stricter one per sub-agent.

## Starting the server

- `mcp` MUST be the very first argument (`args[0]`), exactly. It is checked before
  anything else runs, because stdout belongs to the JSON-RPC protocol from the
  first byte — even the usual `=== SPLA CLI ===` banner is suppressed for this
  command and rerouted to stderr instead.
- There is no `--project` / positional path argument for `mcp`. The project file
  is found the same way `chat` finds it: by searching the **current working
  directory** upward for a `*.spla` file. **To open a specific project, set the
  spawned process's working directory to that project's folder** — do not pass
  the path as an argument, it will be rejected as an unknown command.

  MCP client config example (adjust `command` to your actual exe path):
  ```json
  {
    "mcpServers": {
      "spla": {
        "command": "C:\\path\\to\\SPLA.CLI.exe",
        "args": ["mcp"],
        "cwd": "C:\\path\\to\\your-project-folder"
      }
    }
  }
  ```
  If your MCP client's config format has no `cwd` field, wrap the exe in a small
  `.bat`/`.ps1` that `cd`s into the project folder first and then runs the exe.
  With no project found in the working directory, the server falls back to
  default settings — it still runs, just without your project's
  connections/mode/tools, which is almost never what you wanted.

- The process is long-lived for the whole session: it starts once, builds one
  `AgentRuntime`, and then serves every `tools/call` from that same process over
  the open stdin/stdout pipe until the pipe closes. It is not restarted per call.
  Startup (LLM endpoint, project, tool count) is logged once to stderr:
  ```
  Project file: C:\...\Your-Project.spla
  Project: Your-Project
  Workspace: C:\...\your-project-folder
  Mode:      Edit
  [spla-mcp] project: C:\...\your-project-folder
  [spla-mcp] offering 35 tools
  [spla-mcp] ready
  ```

## Secrets and credentials

Anything needing a credential — SSH hosts, SQL connections, API endpoints — is
configured **in the project, ahead of time, through the SPLA UI or CLI**. The
credentials live in the secret store (DPAPI-backed on Windows) and are referenced
by name, e.g. a host whose auth reads `credential 'secret:user:MiHomo 21.2'`.

Consequences for you:

- **There are no tools for reading or writing secrets, by design.** You cannot
  fetch a password, and you should not ask the user to paste one to you.
- **You do not need them.** Call `ssh_run` with the host name; the runtime
  resolves the credential itself. Connections appear pre-authenticated.
- **If something is not configured, that is the user's step, not yours.** Tell
  them to add the host/connection and its credential in the SPLA UI (or CLI), and
  what to name it. After that the tools work with no further involvement from you.
- Use `ssh_list_hosts` (and the equivalent listing tool for other connection
  types) to see what is already configured before assuming anything is missing.

## Headless constraints

This mode has no window, so nothing can ask the user anything mid-call:

- Any tool whose permission verdict is "ask" refuses instead, with a speaking
  refusal you can act on. That is by design, not a bug — relay it to the user
  rather than retrying, since retrying cannot change the verdict.
- `agent_clarify` returns `no_handler` for the same reason.
- So: front-load context. A call that would have asked a follow-up question just
  fails instead.

## Delegating work: agent_spawn

`agent_spawn` runs a **sub-agent** — the same agent loop, an isolated session
(own conversation, own working memory, own skill session), the same tools — and
returns only its final message.

Two shapes:

- **Free-form task** — give `input` alone: `{"input": "On host MiHomo 21.2,
  report the OS name and kernel version. Nothing else."}`. No skill needed.
- **Pinned procedure** — add `skill`: the sub-agent runs that one procedure and
  nothing else.

Use it for work whose *middle* you do not need: bulk file reads, wide searches,
per-host checks, anything that would otherwise flood your context with output you
are going to summarise anyway. `agent_spawn_batch` does the same across many
tasks in parallel (bounded concurrency, default 3).

Two things to get right:

- **The sub-agent cannot see your conversation.** Everything it needs goes in
  `input`.
- **Ask for what you want back.** Only the last assistant message returns, so say
  "report X" explicitly or the useful part stays in the sub-agent's session.

`mode` picks the sub-agent's ceiling and may be stricter than yours — a read-only
`Research` sub-agent for a survey task is a good habit. Spawns nest up to 3 deep,
then refuse.

## Building/finding the exe

- Framework-dependent (needs .NET/ASP.NET Core runtime installed): a plain
  `dotnet build` produces `src\apps\SPLA.CLI\bin\{Debug|Release}\net10.0\SPLA.CLI.exe`.
- Self-contained single file (no runtime needed on the target machine, this is
  what ships in the product zip): `.publish\work\SPLA.CLI.exe`, produced by
  `PublishAll.ps1` or directly via:
  ```
  dotnet publish src\apps\SPLA.CLI\SPLA.CLI.csproj -p:PublishProfile=SingleFile -c Release
  ```
  (output path is set inside `SingleFile.pubxml`, currently `.publish\SPLA.CLI\win-x64\`
  for a direct publish, `.publish\work\` when run through `PublishAll.ps1`).

## Seeing this file again

`SPLA.CLI.exe --help-mcp` prints this exact file (it is embedded in the exe, not
read from disk, so it works from anywhere). This markdown file also ships as a
plain file, `MCP_USAGE.md`, right next to `SPLA.CLI.exe` in the product zip.
