# SPLA - local AI assistant

<p align="center">
  <img src="Images/MainLogo.png" alt="SPLA Logo" width="300" />
</p>

Portable local AI assistant.

**Connect** a local LLM, **open** a project, and start **working**.

## Typical Tasks

- Analyze a codebase.
- Explain project architecture.
- Search and modify files.
- Run builds and tests.
- Diagnose network problems.
- Work with domain-specific plugins such as 1C.

## Why SPLA

- Local-first.
- OpenAI-compatible.
- Project-oriented.
- Extensible through plugins.
- Explicit permission model.

## Core Capabilities

1. **Local LLMs.** SPLA is designed for local models and OpenAI-compatible APIs. The default setup uses LM Studio at `http://127.0.0.1:1234/v1`, but another compatible runtime can be used.
2. **Extensible tool system.** Tools are registered through the MCP host and plugins.
3. **Project organization.** An SPLA project is described by a `.spla` file in the working directory. It defines the workspace, agent mode, instructions, documentation, ignored paths, LLM endpoint, plugins, and permissions. The file can be associated with the application and opened as a dedicated workspace.
4. **Security modes.** The `Chat`, `Research`, `Inspect`, `Edit`, and `Agent` modes restrict file reads, writes, shell commands, network access, and agent autonomy.

## Tools

The agent's capabilities have two layers: built-in tools that form SPLA's basic working environment, and plugins that provide specialized capabilities. This keeps the core toolset compact while allowing projects to enable only the additional capabilities they need.

### Built-in Tools

- **Project and environment:** retrieve the current project context, working directory, date, and time.
- **Files and images:** list directories; read, create, write, patch, and delete files; find files and text; view images.
- **Command line:** run commands in the workspace. Within the granted permissions, the agent can use it to run builds, tests, and other project utilities.
- **Web access:** retrieve the contents of a web page at a specified URL.
- **Working memory:** store, read, list, and delete notes scoped either to the current chat or shared across the project.
- **Long-task control:** context checkpoints and named marks that the agent can return to when needed.
- **Work organization:** help for available tools, skill activation, clarification requests, and delegating subtasks to other agents.

The exact set of available tools depends on project settings, enabled capabilities, and the selected security mode.

### Plugins

Plugins are separate from the built-in toolset and can be enabled per project. The following implemented plugins add tools to the agent:

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

Tools: `sql_connections`, `sql_test_connection`, `sql_manage_connection`, `sql_schema`, `sql_query`, `sql_query_plan`, `sql_execute`, `sql_verify_context`.

#### SSH

Runs commands on remote hosts and works with persistent interactive SSH sessions that remain visible to the user.

Tools: `ssh_list_hosts`, `ssh_run`, `ssh_sessions`, `ssh_session_exec`, `ssh_session_wait`, `ssh_session_send`, `ssh_session_close`.

#### 1C

Indexes exported 1C configuration source and helps inspect objects, references, and dependencies.

Tools: `onec_build_index`, `onec_find_object`, `onec_get_object`, `onec_explain_object`, `onec_find_references`, `onec_find_readers`, `onec_find_writers`, `onec_get_dependencies`, `onec_get_reverse_dependencies`.

#### Test

An internal plugin for verifying the plugin loading mechanism.

Tool: `test_ping_host` — returns a test response confirming that the plugin is loaded and available to the agent.

The additional `sql_avalonia` module integrates the SQL plugin with the user interface.

## Quick Start and Building from Source

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

SPLA uses five agent modes:

| Mode | Purpose |
|------|---------|
| Chat | Discussion only |
| Research | Read and analyze |
| Inspect | Diagnostics and inspection |
| Edit | Modify project files |
| Agent | Autonomous multi-step execution |

## Plugins

Plugins are located next to the published application in the `plugins/` directory. Each plugin has its own `meta.yaml` and can add tools, prompt instructions, UI commands, settings, and interactive panels. Tool naming rules and plugin metadata are described in [plugins.md](agents/plugins.md).

## Responsible Use

SPLA network tools are intended for research, diagnostics, inventory, and the normal work of network administrators on systems and networks they own, operate, or are explicitly authorized to assess.

Do not use SPLA for unauthorized scanning, probing, access attempts, disruption, or any activity against third-party networks without permission.

## License

This project is licensed under the [MIT License](LICENSE).
