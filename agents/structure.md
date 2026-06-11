# SPLA Project Structure

This file is an agent-facing map of the current repository layout.

## Solution

The active solution file is `SPLA.slnx`.

Projects included in the solution:

- `SPLA.Domain`: Shared domain models and cross-project contracts.
- `SPLA.LLM.LMStudio`: Local LLM provider integration for LM Studio / OpenAI-compatible API endpoints.
- `SPLA.MCP.Core`: MCP host core, tool abstractions, plugin metadata, plugin loading, command descriptors, permissions integration, and tool help routing.
- `SPLA.MCP.BasicTools`: Built-in tools for filesystem, shell, .NET build/test, context, date/time, web fetch, and web search.
- `SPLA.Observability`: Shared logging, tracing, metrics, correlation, and log destination infrastructure.
- `SPLA.Plugins.Host.Avalonia`: Avalonia UI plugin host contracts shared between the main UI and UI plugins.
- `SPLA.Plugins.Network`: Network plugin with host info, LAN scan, port scan, ping, nslookup, HTTP GET/HEAD, port check, and traceroute tools.
- `SPLA.Plugins.OneC`: 1C analysis plugin with indexing, object lookup/explanation, references, dependency analysis, readers/writers, and graph data support.
- `SPLA.Plugins.OneC.Avalonia`: Avalonia UI plugin for the 1C analysis experience. Its manifest type is `avalonia-ui` and it depends on `onec`.
- `SPLA.UI.Avalonia`: Main desktop application. Hosts chat, settings, plugin commands, plugin panels, webchat assets, themes, and project interaction services.
- `SPLA.CLI`: Command-line entry point using the same core, LLM, basic tools, and observability layers.
- `SPLA.Tests`: Test project.

## Root Files

- `AGENTS.md`: Entry-point instructions for agents working in this repository.
- `README.md`: Main English user README.
- `README_RU.md`: Main Russian user README.
- `spla.spla`: Example/current SPLA project file for this workspace.
- `PublishAll.cmd`: Release packaging script for UI, CLI, plugins, and ZIP output.
- `SPLA.slnx`: Solution file with project entries and solution folders for README, user docs, and agent docs.

## Documentation Layout

- `agents/`: Agent-facing documentation and implementation rules. Files here are written in English.
- `docs/`: User-facing README-style documentation. English files use `readme_*.md`; Russian files use `readme_*_ru.md`.

See `agents/documentation.md` for the exact documentation and translation rules.

Current user docs:

- `docs/TODO.md`: User-maintained TODO notes. Agents should not treat this file as agent instructions.
- `docs/readme_security_ru.md`: Russian user-facing security/modes documentation.

Current agent docs:

- `agents/avalonia.md`: Avalonia UI development rules.
- `agents/documentation.md`: Documentation layout and translation rules.
- `agents/observability.md`: Observability conventions.
- `agents/plugins.md`: Plugin system and tool naming conventions.
- `agents/security.md`: Agent permission modes and capabilities.
- `agents/spla-file.md`: `.spla` project file format.
- `agents/structure.md`: This repository structure map.
- `agents/tool-help.md`: Tool help system flow.
- `agents/ui-theming.md`: UI theming and density rules.

## Plugins

Plugin projects use `meta.yaml` manifests and are published into `.publish/work/plugins/<plugin-id>/` by `PublishAll.cmd`.

Current plugin manifests:

- `SPLA.Plugins.Network/meta.yaml`: `id: network`, `type: dll`.
- `SPLA.Plugins.OneC/meta.yaml`: `id: onec`, `type: dll`.
- `SPLA.Plugins.OneC.Avalonia/meta.yaml`: `id: onec_avalonia`, `type: avalonia-ui`, `depends_on: onec`.
- `SPLA.Plugins.Test/meta.yaml`: Test plugin manifest. The project exists in the repository but is not currently listed in `SPLA.slnx`.

## Publish Output

Generated output is not source:

- `.publish/work/`: Ready-to-run application folder.
- `.publish/work/plugins/`: Published plugin folders.
- `.publish/zip/SPLA.zip`: ZIP package created by `PublishAll.cmd`.

Do not treat `.publish/` contents as authoritative source.

## Out-of-Solution Notes

- `SPLA.Plugins.Test` exists with a `.csproj` and `meta.yaml`, but it is not currently included in `SPLA.slnx`.
- `SPLA.Tools.OneC` exists as a folder but currently contains only build artifacts in this workspace and is not included in `SPLA.slnx`.
