# SPLA - local AI assistant

<p align="center">
  <img src="Images/MainLogo.png" alt="SPLA Logo" width="300" />
</p>

Portable local AI assistant.

**Connect** a local LLM, **open** a project, and start **working**.

Русская версия — [`README.ru.md`](README.ru.md).

## What matters in SPLA

**.NET only.** The agent, its tools, and every plugin are C# and .NET — no Python runtime, no Node
process, no subprocess-based tool implementations in the agent's execution path. One toolchain to
build, debug, and ship means one failure surface, not three. This is a hard constraint on the core
and plugins, not a slogan: a plugin that needs Python or Node to *run* the agent's tools does not
belong here. The web client is the one deliberate exception — it is a browser UI (Vue/TypeScript),
not part of the agent runtime, and talks to the service the same way any other client would.

**Project-oriented.** Everything the agent does is anchored to the `.spla` project file — the
workspace, instructions, LLM endpoint, plugins, roles, permissions. This is not a config file for
form's sake: it is the single entry point for the UI, for the CLI, and for third-party code that
embeds the agent (see below).

**Componentized.** The agent is not only an application but a library. A chat is an ordinary object:
reference `SPLA.Runtime` from your own C# code, open the same `.spla` file, and get a full agent
with no CLI, no service, and no UI:

```csharp
var settings = ConfigLoader.LoadAndResolve("my.spla");
using var runtime = new AgentRuntime(settings, loggerFactory);
var chat = new ChatRegistry(runtime).CreateNew("my task");
await chat.SendAsync(text, callbacks, permission, clarify, ct, images);
```

This is not a hypothetical — [`demo/workers/`](demo/workers/) holds working examples of exactly this
kind of embedding: [VisionAgent](demo/workers/VisionAgent/README.md) (camera frames analyzed by a
model), [LogSentry](demo/workers/LogSentry/README.md) (log-file triage), and
[Summarizer](demo/workers/Summarizer/README.md) (a document run across a matrix of prompts and
models).

**Modular.** Modularity works in both directions:

- the "head" (the model) can be swapped without touching anything else — change the LLM endpoint in
  `.spla`, or run the same prompt through several local models in turn;
- or, the other way around, hand *your* tools to an external head: SPLA exposes `/mcp` on its
  service and acts as an MCP server, so a large external model (in another chat, or another product
  entirely) gets hands — access to local files, the shell, SPLA's plugins — through standard MCP
  rather than copy-paste.

**Roles.** A role is a set of instructions and permissions assigned to a chat rather than to the
project as a whole: within one project, different chats can hold different roles. And chats are not
isolated islands — a dedicated tool, `agent_correspond`, lets them write to each other, so several
chats with different roles can be brought together as a team of agents that talks directly, up to
and including an argument between several points of view on one task, instead of being coordinated
by hand through a human.

## Core Capabilities

1. **Local LLMs.** SPLA is designed for local models and OpenAI-compatible APIs. The default setup uses LM Studio at `http://127.0.0.1:1234/v1`, but another compatible runtime can be used.
2. **Extensible tool system.** Tools are registered through the MCP host and plugins; a separate MCP client lets the agent call tools on external MCP servers, and SPLA itself can act as an MCP server for an external model (see above).
3. **Project organization.** An SPLA project is described by a `.spla` file in the working directory. It defines the workspace, agent mode, instructions, documentation, ignored paths, LLM endpoint, plugins, roles, and permissions. The file can be associated with the application and opened as a dedicated workspace.
4. **Security through modes, roles, and zones.** The five agent modes (`Chat`, `Research`, `Inspect`, `Edit`, `Agent`) still set the ceiling on a chat's autonomy. Underneath them, permissions are moving to a model of zones and grants on the edges between them, while roles bundle instructions and permissions for a specific chat. See [Security](#security) below.

## Architecture

Internally the system is laid out in layers — from concepts that know nothing about the protocol or
the OS, up to the windows people talk to the agent through:

| Layer | What it is |
|---|---|
| Domain (`SPLA.Domain`) | concepts and contracts that know nothing about the protocol, the provider, or the operating system |
| Tool pipeline (`SPLA.MCP.Core`) | the path of a call from the name the model uttered to its execution, and the assembly of the context the model learned that name from |
| Chat runtime (`SPLA.Runtime` / `SPLA.Agent`) | the live state of a conversation and the turn loop itself |
| LLM providers (`src/llm`) | integrations with whoever actually answers the request |
| Service and protocol (`src/service`) | the agent behind a wire, clients connect from outside |
| Plugins and tools | what the agent can do with its hands |
| Clients (web / CLI / Avalonia) | the windows people talk to the agent through |

In essence the agent is `SPLA.CLI` and the runtime beneath it; the CLI has many modes, and the
different ways to reach the agent are its facets rather than separate products:

- `spla chat open` — an interactive REPL, a chat in the terminal;
- `spla chat run` — a headless one-shot/batch run: one or more prompts against one or more models,
  output to the screen or to files — the mode for scripting and automation;
- `spla serve` — the same runtime raised as a service behind a WebSocket protocol, which other
  clients connect to;
- `spla` in MCP mode — the CLI itself speaks MCP over stdio to an external head (see "Modular" above);
- `spla start` / `spla stop` / `spla ps` — bring an agent up on a project and leave it running in the
  background, stop it, see what is already running.

Separately from the CLI, `SPLA.Runtime` can be referenced directly from your own C# code (see
"Componentized" above) — another way to get an agent with no CLI, service, or UI at all.

As for the graphical clients: the web client (`web/`, Vue 3 + TypeScript + Vite) is the actual
renderer — file browser, code/Markdown editor, project and role settings, plugin panels are all Vue
code talking to the service over WebSocket. That same web client can be opened directly in an
ordinary browser, without Avalonia and without installing anything — it only needs a running
`spla serve` (or an embedded service) reachable over the network. The Avalonia desktop app
(`SPLA.UI.Avalonia`) is no longer a separate UI but a thin shell: a native window frame and a tray
icon around a `WebView` showing that same web client. It has no native chat or settings screens of
its own any more.

Each project has its own chats and agent state; one service can serve several projects and clients
at once.

## Tools

The agent's capabilities have two layers: built-in tools that form SPLA's basic working environment, and plugins that provide specialized capabilities. This keeps the core toolset compact while allowing projects to enable only the additional capabilities they need.

### Built-in Tools

- **Project and environment:** retrieve the current project context, working directory, date, and time.
- **Files and images:** list directories; read, create, write, patch, and delete files; find files and text; view images.
- **Command line:** run commands in the workspace, including persistent interactive shell sessions the agent can send further input to and return to. Within the granted permissions, the agent can use it to run builds, tests, and other project utilities.
- **Web access:** retrieve the contents of a web page at a specified URL.
- **Working memory:** a two-tier key-value store — notes for the current chat or shared across the project — that the agent reads, writes, lists, and clears.
- **Long-task control:** context checkpoints and named marks that the agent can return to when needed.
- **Sub-agents:** spawn one or several sub-agents for a subtask and correspond with them while they work.
- **MCP client:** call tools on external MCP servers configured for the project.
- **Work organization:** help for available tools, skill activation, and clarification requests.

The exact set of available tools depends on project settings, enabled capabilities, and the selected security mode.

### Plugins

Plugins are distributed separately from the built-in toolset and can be enabled for individual projects.
Plugins are located next to the published application in the `plugins/` directory.
Each plugin has its own `meta.yaml`; a plugin can add tools, prompt instructions, UI commands, settings, and its own interactive panels.
Tool naming rules and plugin metadata are described in [plugins.md](agents/plugins.md).

The following implemented plugins add tools to the agent:

#### Browser

Controls a full Chromium-family browser through Playwright: opens websites, executes JavaScript, works with tabs and page elements, takes screenshots, and collects diagnostic messages.

This is not another form of the built-in `web_fetch`. The built-in tool performs a regular HTTP request and extracts page text without a browser interface, JavaScript, or a user profile. The Browser plugin launches Microsoft Edge, Google Chrome, or a managed Chromium instance; all three use the Chromium engine.

The browser can use one of three profile types:

- a new temporary profile with no saved data whose state is discarded when the browser closes;
- a persistent project profile that preserves authentication, cookies, and other state between launches;
- an existing Edge or Chrome user profile with its saved authentication and cookies, which the agent can access only after the user explicitly selects it.

Tools:

- lifecycle: `browser_start`, `browser_stop`, `browser_status`, `browser_list_profiles`;
- tabs and navigation: `browser_tabs`, `browser_new_tab`, `browser_switch_tab`, `browser_close_tab`, `browser_navigate`, `browser_wait_navigation`;
- page inspection: `browser_snapshot`, `browser_inspect`, `browser_get_text`, `browser_screenshot`;
- actions: `browser_click`, `browser_fill`, `browser_press`, `browser_select`, `browser_scroll`, `browser_wait_element`, `browser_upload`;
- diagnostics: `browser_console`, `browser_page_errors`.

#### Browser Screencast

A separate experimental interactive-browser plugin. Independently of the Browser plugin, it launches its own headless browser, streams its image to the Browser Lab panel, and accepts mouse and keyboard input from the user.

Browser Screencast does not use the Browser plugin's session or profiles and does not replace its automation tools. The page image is delivered to the UI as an in-memory frame stream, while the browser itself remains a full, separate Chromium instance.

Tool: `browser_screencast_info` — reports the purpose and experimental status of the panel.

#### Network

Provides network diagnostics, local-network inventory, and network-service checks.

Tools:

- host and routes: `network_get_host_info`, `network_ping_host`, `network_ping_host_stats`, `network_trace_route`, `network_get_routes`, `network_get_arp_cache`;
- DNS and WHOIS: `network_resolve_host`, `network_query_dns`, `network_reverse_dns`, `network_check_dns_propagation`, `network_lookup_whois`;
- HTTP and TLS: `network_http_get`, `network_http_head`, `network_http_post`, `network_check_http_redirects`, `network_check_tls`;
- discovery and probes: `network_discover_hosts`, `network_scan_tcp_ports`, `network_check_tcp_port`, `network_probe_tcp`, `network_probe_udp`, `network_probe_smtp`, `network_wake_host`.

#### Roslyn

Compiles and executes C# code and builds, runs, and tests real .NET projects.

Tools: `roslyn_compile_check`, `roslyn_script_run`, `roslyn_project_build`, `roslyn_project_run`, `roslyn_project_test`.

#### SQL

Works with configured database connections: inspects schemas, runs safe queries, analyzes query plans, and performs controlled data modifications.

Tools: `sql_connections`, `sql_test_connection`, `sql_schema`, `sql_query`, `sql_query_plan`, `sql_execute`, `sql_verify_context`.

#### SSH

Runs commands on remote hosts and works with persistent interactive SSH sessions that remain visible to the user.

Tools: `ssh_list_hosts`, `ssh_run`, `ssh_sessions`, `ssh_session_exec`, `ssh_session_wait`, `ssh_session_send`, `ssh_session_close`.

#### 1C

Indexes exported 1C configuration source and helps inspect objects, references, and dependencies.

Tools: `onec_build_index`, `onec_find_object`, `onec_get_object`, `onec_explain_object`, `onec_find_references`, `onec_find_readers`, `onec_find_writers`, `onec_get_dependencies`, `onec_get_reverse_dependencies`.

#### Documents

Reads Word documents by meaning rather than as raw XML, and works with spreadsheet rows by column
header rather than by cell address.

Tools: `document_extract`, `spreadsheet_inspect`, `spreadsheet_read_rows`, `spreadsheet_append_rows`.

#### Test

An internal plugin for verifying the plugin loading mechanism.

Tool: `test_ping_host` — returns a test response confirming that the plugin is loaded and available to the agent.

The additional `sql_avalonia` module integrates the SQL plugin with the user interface.

## Related Projects

[SPLA.Skills](https://github.com/KOE73/SPLA.Skills) — public library of reusable SPLA agent
skills: self-contained procedures an agent finds by subject and loads on demand.

## Quick Start and Building from Source

Ready-to-run packages are published on the
[Releases page](https://github.com/KOE73/SPLA/releases) as `SPLA.zip`, versioned
`v0.<minor>.<build>`. Build from source if you want the current development state instead of the
latest release.

Requirements:

- build: .NET 10 SDK;
- runtime: .NET 10 Runtime and an accessible OpenAI-compatible endpoint, such as a local LM Studio instance.

1. Build the desktop app, CLI, plugins, and ZIP package with the shared script:

```powershell
.\PublishAll.cmd
```

The ready-to-use application folder is created at `.publish/work/`. You can move that folder elsewhere and run SPLA from there. The ZIP archive is created at `.publish/zip/SPLA.zip`.

2. Start LM Studio or another local OpenAI-compatible endpoint, or make sure you have access to one.
3. Run the desktop UI from `.publish/work/` and set the path to your LLM endpoint.

You can also run the CLI version from `.publish/work/` to work through the terminal.

The settings screen contains a short description. A project can be created in any folder. In practice, it is a `*.spla` settings file that can be opened through the UI or CLI. The project defines permissions, instructions, documentation, ignored paths, plugins, and related settings.

You can switch projects from the project list in the UI or open multiple windows. Each project has its own settings, chats, and agent state, while projects can use the same plugins and tools.

## Security

SPLA still uses five agent modes — they set the ceiling on what a chat may do regardless of anything
else, from discussion-only to autonomous multi-step execution:

| Mode | Purpose |
|------|---------|
| Chat | Discussion only |
| Research | Read and analyze |
| Inspect | Diagnostics and inspection |
| Edit | Modify project files |
| Agent | Autonomous multi-step execution |

Underneath that layer, the permission model is moving to zones: named areas (project, local file
system, internet, shell, ...) with grants on the edge between zones rather than one flat list of
per-call permissions. Roles bundle a set of grants and instructions and are assigned to a specific
chat (see above). This is an actively developing direction, not a finished guarantee — treat it as
the course SPLA's permission model is taking, not as settled fact.

## Architecture Diagrams (alpha)

A side, auxiliary subproject living next to the main one —
[`docs/diagrams/`](docs/diagrams/README.md) and the [`tools/spla-diagram`](tools/spla-diagram/)
editor. Its purpose runs in both directions.

**Outward:** show the project's internal structures so they are easier to understand — not as prose
retelling, but as a picture you can look at.

**Back:** a drawn architecture also makes the meaning easier for the agent to grasp. The diagram
stops being an illustration of the text and becomes an input: the model reads it as a source of
structure and intent, and the owner edits the architecture on a canvas rather than in conversation.

That reverse direction is what the rest of the design grows from — a drawing tool alone would not do.
If a model reads the diagram, then:

- the diagram must carry **reasons**, not just topology, or it saves no words;
- the text must be **verifiable as current**, or the model will confidently lie from a stale
  description — hence every text field records its own provenance, and there is no base language:
  Russian and English are equal;
- every element of the picture must have an **unambiguous reading**, or the model fills in the gaps
  itself — hence nesting a block inside a frame is an assertion rather than decoration, and a view
  must declare its **axis**: what that nesting actually classifies.

Hence the views: the same codebase lays out differently, and that is a choice of question rather than
of style — the "turn backbone" (from the inbox to the return to the model), a semantic atlas of
subsystems, security zones, processes.

Launch it from the repository root; the script builds the editor app and starts a local server
itself:

```powershell
.\ViewArchitecture.cmd
```

Then open <http://localhost:8777/app/>. Layout is manual only — there is no auto-layout and there
will not be one.

All of the above is alpha.

## Responsible Use

SPLA network tools are intended for research, diagnostics, inventory, and the normal work of network administrators on systems and networks they own, operate, or are explicitly authorized to assess.

Do not use SPLA for unauthorized scanning, probing, access attempts, disruption, or any activity against third-party networks without permission.

## License

This project is licensed under the [MIT License](LICENSE).
