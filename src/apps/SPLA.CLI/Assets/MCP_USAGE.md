# Using this SPLA build as an MCP server

`SPLA.CLI.exe` serves this build's tools to a foreign MCP head (Claude Code,
Claude Desktop, or any other MCP client). This file exists so a foreign head
does not have to rediscover any of the following by trial and error.

## The four modes, at a glance

There are exactly four ways to connect. Pick by row, then jump to its section
below for the nuances and examples.

| # | Mode | Command | Transport your client sees | Writer lease? | Best for |
|---|------|---------|------------------------------|----------------|----------|
| 1 | **Join-or-start** (default) | `spla mcp` | stdio | No — this process only proxies | The common case: one client, one project, no fuss |
| 2 | **Standalone** | `spla mcp --standalone` | stdio | Yes — held for the connection's life | A CI job / throwaway sandbox that is the only thing touching the project |
| 3 | **Direct HTTP** | `spla serve` running, client points at `POST http://127.0.0.1:<port>/mcp` | HTTP (plain or SSE) | No — shared with every other client of that instance | Several MCP clients on the same project at once, or a client that only takes a URL |
| 4 | **Hub front door** | `spla hub` running, client points at `POST http://127.0.0.1:5077/mcp?project=<name>` | HTTP (plain or SSE) | No | Several *different* projects, one fixed address, or the hub is already the machine's front door |

Quick trade-offs:

- **1 (join-or-start)** costs one extra proxy hop and needs `SPLA.CLI.exe`
  reachable from itself (to spawn a child if nobody is serving yet). In
  exchange it never blocks anything else from opening the same project.
- **2 (standalone)** is the simplest process topology — one process, no
  child, no proxy — but it takes the project's one writer lease for as long
  as the stdio pipe is open, so a second `spla mcp`, an app window, or
  anything else touching the same project gets refused meanwhile.
- **3 (direct HTTP)** is mode 1's proxy target, run by hand instead of
  auto-started: you keep one long-lived `spla serve` and point every client
  at it, so they all share one writer instead of each taking their own path.
  Costs you managing that process and its port yourself.
- **4 (hub)** is mode 3 generalized across every project on the machine: one
  fixed address, `?project=` picks which one, and a project that isn't
  running yet gets started for you. Costs a `spla hub` running and the
  project having been opened at least once already (so the hub knows it).

If you only remember one thing: **mode 1 is the default and almost always
right.** Reach for 2 only when you deliberately want the writer lease and
nothing else touching the project; reach for 3 or 4 only when more than one
client needs the same project(s) at once, or your client can't be given a
command to run at all.

---

## Mode 1 — Join-or-start (default)

```
spla mcp
```

A project has exactly one writer. Building a fresh `AgentRuntime` — which is
what this command used to always do — takes that writer lease for as long as
the MCP connection lives, which refuses an app window, a REPL, or a second
`spla mcp` on the same project for no better reason than that this command
insisted on building its own runtime. `POST /mcp` on `spla serve` exists for
exactly this caller: it dispatches against a runtime the host already has
open, so any number of MCP clients can share it the way any number of browser
windows already share `/ws`.

So the default is: find a live instance for this project and proxy stdio to
its `/mcp` route; if nobody is serving the project yet, start one — a
loopback child, idle-timeout'd, that quietly outlives this connection until
nothing needs it. Either way this process never becomes the writer.

**The session is held open by a socket, not by the calls.** `POST /mcp` is
stateless per request, so a client that goes quiet between calls (not
finished, just idle) would otherwise let the instance's own idle-timeout
reclaim it out from under the session. To prevent that, this process also
opens one ordinary WebSocket to the instance and simply holds it open for as
long as the stdio pipe lives — the lease counts connected clients, so a
socket that says nothing still says "somebody is here." Losing this hold (an
instance behind a token this process wasn't given, etc.) is never fatal —
proxying still works, you just lose the idle-timeout protection, and the
tool says so on stderr.

**What you get if join-or-start itself can't get off the ground** — nothing
to join, nowhere to spawn a child (no `SPLA.CLI.exe`/`.dll` reachable from
`AppContext.BaseDirectory`), a child that never came up: this degrades to
mode 2 (standalone) automatically, with a line on stderr saying so. A broken
join-or-start becoming "works, but takes the writer lease" beats "does not
work at all."

```
[spla-mcp] project: C:\...\your-project-folder
[spla-mcp] joined instance at http://127.0.0.1:54213 — proxying stdio to POST http://127.0.0.1:54213/mcp
```
(`joined` becomes `started` when nothing was already serving the project.)

## Mode 2 — Standalone

```
spla mcp --standalone
```

Builds the `AgentRuntime` directly in this process and serves it over stdio
for the life of the connection — no proxy, no child process, no other
instance involved. This is the original, simpler behavior, and it is still
the right call when this really is going to be the only thing touching the
project (a CI job, a throwaway sandbox): one process instead of two, at the
cost of refusing anyone else who tries to open the project while it runs.

```
Project file: C:\...\Your-Project.spla
Project: Your-Project
Workspace: C:\...\your-project-folder
Mode:      Edit
[spla-mcp] project: C:\...\your-project-folder
[spla-mcp] offering 35 tools (standalone)
```

Use this deliberately, not as a default — reach for it when you specifically
want the writer lease (e.g. you know nothing else will touch this project
for the session), not merely because it looks simpler on paper.

## Mode 3 — Direct HTTP (`spla serve` + `POST /mcp`)

If you already know several MCP clients need the same project at once, or you
want one connection address to outlive any single client, run `spla serve`
for that project yourself and point every MCP client at its `POST /mcp`
endpoint:

```
http://127.0.0.1:<port>/mcp
```

(port from the instance's lock file, from `spla ps`, or a fixed one you set
via `mcp.port` — see [Config](#config-mcp-section) below). All requests to
`/mcp` dispatch against the one runtime that `serve` already has open — the
same way any number of browser windows already share its `/ws` — so multiple
heads share one writer instead of each grabbing their own lease. Mode 1's
join-or-start does exactly this for you automatically in the common case; do
this by hand only when you want control over exactly which instance gets
started, or when your MCP client can only be pointed at a URL, not at a
command to run (mode 4 is the other case that needs a URL).

`/mcp` speaks in one of two shapes, and which one you get is entirely up to
the request you send — the server does not need telling ahead of time:

- **Plain** (default — no special header) — one JSON-RPC line in, one line
  out. A call that opts into `_meta.progressToken` still *runs to completion
  correctly* (a slow tool like `ssh_run` or a long `agent_spawn` is not cut
  off — the request just stays open until it finishes), it just has nowhere
  to push intermediate ticks, so they are dropped.
- **SSE** (send `Accept: text/event-stream` on the request) — MCP's
  "streamable HTTP" transport. The connection stays open and every frame is
  pushed the moment it exists — progress notifications as the call runs, then
  the final reply — each as its own `data: <json-rpc>\n\n` event. This is the
  real network equivalent of stdio's progress channel: the same live ticks,
  just over HTTP instead of a pipe.

  ```bash
  curl -N -X POST http://127.0.0.1:<port>/mcp \
    -H "Content-Type: application/json" -H "Accept: text/event-stream" \
    -d '{"jsonrpc":"2.0","id":1,"method":"tools/call",
         "params":{"name":"ssh_run","arguments":{...},
                    "_meta":{"progressToken":"t1"}}}'
  ```

So: stdio (modes 1/2) and SSE-over-`/mcp` (modes 3/4) are the two ways a
foreign MCP head sees live progress. **`/ws` is a third, different thing and
is not an MCP transport at all** — it is SPLA's own protocol (chat turns,
tool broadcasts, permission asks), spoken only by SPLA's own clients (the web
UI, the Avalonia app). A person can open the project there and watch the
chat/turn while MCP-triggered work runs, which is observing from the other
side — not something a foreign MCP head can itself subscribe to.

## Mode 4 — Hub front door

If a machine is running `spla hub` (see the hub's own docs — briefly: one
process per machine, fixed port `5077` by default, keeps an index of every
instance and can start one on request), it answers `POST /mcp?project=<name>`
itself, and proxies to whichever instance is actually serving that project —
starting one first if none is. This is the option when your MCP client can
only be given a fixed URL, not a command to run and a working directory, and
you have — or expect to have — more than one project in play: the hub's
address never changes, whether or not anything is currently running, and
`?project=` (not `cwd`) is what picks which project you land on.

```
http://127.0.0.1:5077/mcp?project=YourProjectName
```

`project` is matched against the project's own name (from its manifest, the
same name the hub's Projects window lists it under — click the `MCP` pill
next to a project there to copy this exact address), or a manifest path
works unconditionally. Two different projects sharing a display name make
the name ambiguous — the hub answers `409` with both candidates rather than
guessing; address by manifest path instead when that happens. A project the
hub does not know about at all (never opened, never registered) answers
`404` — open it once first.

**Multiple projects, multiple clients: don't rely on `cwd` to keep them
straight.** If you're juggling several projects through mode 1, each one is a
separate `mcpServers` entry in your client's config, told apart only by
whatever key you give that entry (`cwd` picks the project, but the *name* is
what you actually read in the client's tool list) — name them after the
project, not generically `"spla"`, or the tool list becomes indistinguishable
noise once you have three of them. Mode 4 sidesteps this entirely: one entry,
one address, `?project=` in the URL is unambiguous by construction.

### Discovering projects: the hub's own MCP surface

The four modes above all assume you already know which project you want.
`POST http://127.0.0.1:5077/hub/mcp` (same host as mode 4, no `?project=`) is
a different, smaller thing: an MCP endpoint answered by the hub *itself*,
listing one tool, `hub_projects_list`, which returns every project this
machine knows — running or not, with its state, window count, and whether it
has an MCP endpoint available right now. It is the same data the hub's
Projects window shows, just callable.

Use it when you don't yet know the project name to put in mode 4's
`?project=` — point a client at `/hub/mcp` first, call `hub_projects_list`,
then either switch that client to mode 4 with the name you found or open a
second connection. It is read-only: this endpoint cannot start or stop
anything, only tell you what exists. `spla hub` must be running for it to
answer at all — it is not served by a plain `spla serve`/mode 1 instance,
which knows only about itself.

---

## Config: `mcp:` section

A project's `.spla` file (or the machine's `defaults.yaml`) can carry:

```yaml
mcp:
  enabled: true   # false = spla serve never maps POST /mcp at all
  port: 15077     # fixed port instead of the usual ephemeral one
```

Also editable from the SPLA web UI's Settings → MCP tab. Takes effect on the
**next** `spla serve` start — Kestrel binds its listener once, at startup,
the same as a plugin enable flag needing a restart to load. An explicit
`--port` on the `spla serve` command line still wins over `mcp.port`.

`enabled: false` only affects an instance that was already running when you
reach it (mode 3's manual `spla serve`, or an app window). It has no effect
on mode 1's join-or-start or mode 4's hub: both start a child with MCP forced
on regardless of the project's own setting, because starting a child for the
sole purpose of answering `/mcp` and having it refuse that exact route would
just be a confusing failure with extra steps. The setting only ever stops MCP
from being mapped on an instance you (or a window) start some other way.

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

### Running with no project at all

Modes 1 and 2 both tolerate finding no `*.spla` anywhere above the working
directory: settings resolve to machine defaults (a synthesized LM Studio
connection, `Mode: Edit`, MCP forced on), and the server still runs — just
with no project-specific connections, tools, or skills. This is a legitimate
way to get a basic tool set plus `agent_spawn` with nothing configured yet,
not only a degraded fallback. The one thing to watch: without a manifest,
the workspace root is simply wherever the process's working directory
happens to be (`Directory.GetCurrentDirectory()`) — for modes 1/2 that is
your MCP client's `cwd` for this server entry, which some clients set to
something unhelpful (their own install directory, for instance) unless you
set it yourself. Set `cwd` explicitly in the client's server config to the
folder you actually want treated as the workspace.

**Do not silently run on defaults and call it done.** If `tools/list` (or a
call's refusal) makes it clear you have no project, no configured
connection, or a synthesized LM Studio fallback nobody actually set up, tell
the human on the other end and ask what they want instead of guessing:

- If they already know what they want (a specific project folder, a specific
  mode, a specific connection), just ask which one and act on the answer.
- If they say they don't know, or don't answer the question you'd expect
  them to have an opinion on, don't leave them stuck — explain the real
  options in plain terms: point them at the [four connection modes](#the-four-modes-at-a-glance)
  above if the question is *how am I even talking to you*, or at the fact
  that connections/mode/skills are configured **in the project** (via the
  SPLA UI or CLI, not through you) if the question is *why don't I have
  tool X*. Give them something concrete to do next — "open the SPLA UI and
  add a connection" beats "you should configure a project."
- Don't repeat this interrogation every turn once they've answered once for
  this session — ask, get a decision (even "just use the defaults, I don't
  care"), and move on.

### Skills

A **skill** is a procedure the project's owner wrote — a curated, repeatable way to
do one job (`linux-host.capture`, `network.range-audit`). `skill_find` searches the
catalogue, `skill_activate` pins one to the session. When a skill covers the work,
using it beats improvising: it encodes what the owner actually wants done.

When nothing in the catalogue fits, do the work directly. But if it is work the
user will clearly repeat, suggest writing a skill for it — that is how the
catalogue grows, and it is a suggestion the owner is usually glad to get.

### Modes (agent mode, not connection mode)

`Chat | Research | Inspect | Edit | Agent` — a capability ceiling, not a
personality. Don't confuse this with the four *connection* modes above: this
is the agent's own permission ceiling, orthogonal to how you connected. It
decides which tools are even visible and which are permitted. `Research` is
read-only-ish; `Edit` and `Agent` can change things. The project sets the
default; `agent_spawn` can pick a stricter one per sub-agent.

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
  default settings — see [Running with no project at all](#running-with-no-project-at-all)
  above.

- The process is long-lived for the whole session: it starts once and then
  serves every `tools/call` over the open stdin/stdout pipe until the pipe
  closes. It is not restarted per call. What it logs to stderr on startup
  depends on which mode it took — see the per-mode examples above.

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

A Russian translation, `MCP_USAGE_RU.md`, ships alongside it for people
reading the docs — it is not embedded and `--help-mcp` never returns it, so a
foreign MCP head always gets this English source regardless of the process's
locale.
