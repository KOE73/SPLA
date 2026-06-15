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

1. **Local LLMs.** SPLA is designed for local models and OpenAI-compatible APIs. The default setup is LM Studio at `http://127.0.0.1:1234/v1`, but another compatible runtime can be used.
2. **Extensible tool system.** Tools are registered through the MCP host and plugins.
3. **Project organization.** An SPLA project is described by a `.spla` file in the working directory. It defines the workspace, agent mode, instructions, documentation, ignored paths, LLM endpoint, plugins, and permissions. The file can be associated with the application and opened as a dedicated workspace.
4. **Security modes.** The `Chat`, `Research`, `Inspect`, `Edit`, and `Agent` modes restrict file reads, writes, shell commands, network access, and agent autonomy.

## Tools

- Basic system tools: read, write, create, delete, patch, find files, search text, run commands, current date/time, context, `dotnet build`, `dotnet test`, web fetch, web search.
- Network tools: host info, LAN scan, port scan, ping, nslookup, HTTP GET, HTTP HEAD, port check, traceroute.

## Quick Start and Building from Source

Requirements:
- build: .NET 10.
- runtime: a local LLM runtime, such as LM Studio, and .NET 10 Runtime.

1. Build the desktop app, CLI, plugins, and ZIP package with the shared script:

```powershell
.\PublishAll.cmd
```

The ready-to-use application folder is created at `.publish/work/`. You can move that folder elsewhere and run SPLA from there. The ZIP archive is created at `.publish/zip/SPLA.zip`.

2. Start LM Studio or another local OpenAI-compatible endpoint, or make sure you have access to one.
3. Run the desktop UI from `.publish/work/` and set the path to your LLM endpoint.

You can also run the CLI version from `.publish/work/` to work through the terminal.

The settings screen contains a short description. A project can be created in any folder. In practice, it is a `*.spla` file with settings that can be opened through the UI or CLI. The project defines permissions, instructions, documentation, ignored paths, plugins, and related settings.

One window means one project. If you need to work with several projects, open several windows. Projects are isolated from each other, while still being able to use the same plugins and tools.

## Security

SPLA uses 5 agent modes:

| Mode | Purpose |
|--------|--------|
| Chat | Discussion only |
| Research | Read and analyze |
| Inspect | Diagnostics and inspection |
| Edit | Modify project files |
| Agent | Autonomous multi-step execution |

## Plugins

Plugins are located next to the published application in the `plugins/` directory. Each plugin has its own `meta.yaml` and can add tools, prompt instructions, and, when needed, Avalonia UI panels. Tool naming rules and plugin metadata are described in [plugins.md](agents/plugins.md).

## Responsible Use

SPLA network tools are intended for research, diagnostics, inventory, and the normal work of network administrators on systems and networks they own, operate, or are explicitly authorized to assess.

Do not use SPLA for unauthorized scanning, probing, access attempts, disruption, or any activity against third-party networks without permission.

## License

This project is licensed under the [MIT License](LICENSE).
