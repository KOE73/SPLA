# SPLA Project Structure

This file is an agent-facing map of the current repository layout.

## Solution

The active solution file is `SPLA.slnx`.

Projects live under `src/` grouped by architectural layer, with tests under `tests/`. Each layer
folder carries a directional `AGENTS.md` where it has non-trivial local invariants (`src/core`,
`src/agent`, `src/service`, `src/apps`, `src/plugins`) — read the one for the layer you are editing
in addition to the root `AGENTS.md`.

### `src/core/` — foundation (no upward/outward deps)

- `SPLA.Domain`: Shared domain models and cross-project contracts. Contains ambient-scope utilities that tools use without signature changes: `ProgressScope`/`ProgressTree`/`ProgressNode` (progress tree per agent turn), `ToolHostScope` (current `IToolHost`+`AgentMode`), `ClarifyScope`, `AgentSessionScope`. Also the host boundary (`ISandbox`/`IWorkspace`/`IShell`/`ICapabilityGate`), project backends (`IProjectBackend`/`IBucket`), identity, and secrets.
- `SPLA.MCP.Core`: MCP host core, tool abstractions, plugin metadata, plugin loading, permissions (`PermissionManager`, `PermissionScope`), and tool help routing.
- `SPLA.Observability`: Shared logging, tracing, metrics, correlation, and log destination infrastructure.

### `src/agent/` — the agent loop

- `SPLA.Agent`: The single agent loop (`ConversationOrchestrator`), layered system-prompt builder (`SystemPromptBuilder`), mode-based tool gating (`ToolModeFilter`), and headless skill sub-agents (`SpawnedAgentRunner`).
- `SPLA.Runtime`: The autonomous, transport-neutral agent runtime — the embeddable "SPLA Runtime". Owns the full agent stack for one project (`AgentRuntime`: LLM client, tool host, plugins, skills, project KV, prompt, token tallies), per-chat state (`ChatRuntime`), the open-chat set (`ChatRegistry`), the multi-project home (`AgentRuntimeRegistry`), the in-process domain-event hub (`ServiceEvents`), and sidecar image persistence (`ChatImages`). Zero dependencies on any client or protocol: CLI, the WebSocket service, and standalone hosts (see `demo/`) all reference this project and get an identical agent. Wire/DTO projections of these objects live in `SPLA.Service` (`Chat/RuntimeProjections.cs`), never here.
- `SPLA.MCP.BasicTools`: Built-in tools for filesystem, shell, .NET build/test, context, date/time, web fetch, and web search. Reach the system only through `HostServices.Sandbox`.

### `src/llm/`

- `SPLA.LLM.LMStudio`: Local LLM provider integration for LM Studio / OpenAI-compatible API endpoints.

### `src/editor/`

- `SPLA.Editor.Schema`: Schema/JSON editor domain (content sources, schema registry) for the workspace forms/text editors.

### `src/identity/`

- `SPLA.Identity.Windows`: Windows identity provider (`net10.0-windows`) mapping `WindowsIdentity`/`WindowsPrincipal` → `IIdentity`. A platform DLL loaded by reflection, not referenced by the host.

### `src/service/` — service core & wire protocol

- `SPLA.Service`: WebSocket host (`SplaServiceHost`), per-project/per-chat runtimes (`AgentRuntime`/`ChatRuntime`/`AgentRuntimeRegistry`), and the message-handler registry (`Protocol/MessageRouter.cs` + `Protocol/Handlers/*`). `ClientConnection` owns only transport, auth/handshake, correlation, and turn plumbing.
- `SPLA.Service.Contracts`: The wire protocol — `MessageTypes` constants and payload DTOs shared with the web client.

### `src/apps/` — entry points

- `SPLA.CLI`: Command-line entry point (REPL + `serve`) on the same core/LLM/tools/observability layers.
- `SPLA.Server`: Domain deployment host — Negotiate auth, per-user file areas, identity provider loaded by config. References no platform.
- `SPLA.UI.Avalonia`: Desktop shell. A WebView over the web client that spawns a child `SPLA.CLI serve` on loopback (`EmbeddedServiceLauncher`) and talks the same protocol a browser does.

### `src/plugins/` — plugins & skills

- `SPLA.Plugins.Network`: Network plugin with host info, LAN scan, port scan, ping, nslookup, HTTP GET/HEAD, port check, and traceroute tools.
- `SPLA.Plugins.Roslyn`: Roslyn C# tooling plugin. `roslyn_compile_check` compiles a self-contained snippet against the .NET BCL and returns diagnostics. `roslyn_script_run` compiles and runs a C# script (top-level statements) that drives a plan by invoking tools via `ctx.Run(name, args)`, with `Task.WhenAll` parallelism and progress reporting. `roslyn_project_build`, `roslyn_project_run`, and `roslyn_project_test` shell out to the real `dotnet` SDK against actual files in the workspace (build a `.csproj`/`.sln`, run a project or a single `.cs` file-based app, run `dotnet test`) — `Scope.Shell`, `Risk.High`; a shared `DotnetCli` runner enforces workspace-path containment, a hard timeout, and process-tree kill. `CopyPlugin` copies the built plugin to `plugins/roslyn/` in the CLI and UI output directories (paths are relative to `src/plugins/` → `src/apps/`).
- `SPLA.Skills.Network`: Network skill definitions (`.md` files). Independent of the plugin — skills are instructions, not code. Built via `Microsoft.Build.NoTargets`; `CopySkills` target copies files to `plugins/network.skills/` in the CLI and UI output directories (paths are relative to `src/plugins/` → `src/apps/`).
- `SPLA.Plugins.OneC`: 1C analysis plugin with indexing, object lookup/explanation, references, dependency analysis, readers/writers, and a UI-neutral typed graph-data layer suitable for a future web client. The former `SPLA.Plugins.OneC.Avalonia` project was removed.
- `SPLA.Plugins.Sql`: SQL query/schema/execute plugin. Its settings UI (named connections) ships as an in-plugin web panel; the former `SPLA.Plugins.Sql.Avalonia` editor was removed. The shared `SPLA.Plugins.Host.Avalonia` project was removed with the legacy plugin UI layers.
- `SPLA.Plugins.Browser`: Playwright-driven browser automation plugin.
- `SPLA.Plugins.Browser.Screencast`: Experimental, separate headless-browser panel provider. It
  streams in-memory frames through the generic plugin-panel transport and does not replace or modify
  `SPLA.Plugins.Browser`.
- `SPLA.Plugins.Test`: Test plugin manifest. Present in the repository but not currently listed in `SPLA.slnx`.

### `tests/`

- `SPLA.Tests`: Test project. Protocol tests bind an OS-assigned free port (`FreePort()`), never a hardcoded one.

### `demo/` — standalone runtime hosts

- `VisionAgent`: A headless console app that proves the `SPLA.Runtime` boundary: it references only the runtime (no CLI/service/UI), opens a normal `.spla` project file (`vision.spla`), and loops "grab a video frame (USB camera / RTSP / file via OpenCvSharp) → send it as a chat turn with an image → print the model's analysis". The analysis prompt is ordinary `agent.instructions`; the demo's own knobs live in a `vision:` section of the same `.spla` (unknown sections are ignored by the standard loader).

## Non-code top-level folders

- `web/`: The Vue 3 + TS + Vite browser client, embedded into `SPLA.Service` at build time.
- `docs/`, `agents/`, `Images/`: Documentation and assets (unchanged by the layered `src/` layout).

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
- `agents/protocol.md`: Wire protocol & event registry (every `MessageTypes` constant; guarded by `ProtocolDocTests`).
- `agents/security.md`: Agent permission modes and capabilities.
- `agents/skills.md`: Skill system architecture (lifecycle, assembly order, permission matrix).
- `agents/spla-file.md`: `.spla` project file format.
- `agents/structure.md`: This repository structure map.
- `agents/sys_prompt_rules.md`: System-prompt authoring rules (avoiding rule contradictions).
- `agents/tool-args.md`: Tool argument conventions.
- `agents/tool-help.md`: Tool help system flow.
- `agents/ui-theming.md`: UI theming and density rules.
- `agents/data-ownership.md`: Data ownership rules — UI must not own domain data. Read before adding any registry, flag, or discovery logic.

Per-layer directional docs also live next to the code: `src/core/AGENTS.md`, `src/agent/AGENTS.md`,
`src/service/AGENTS.md`, `src/apps/AGENTS.md`, `src/plugins/AGENTS.md`.

## Skills

Skills are named instruction sets stored as `.md` files in `src/plugins/SPLA.Skills.<PluginId>/`.
They are independent of plugin code — a skill describes a procedure, not a capability declaration.

- `src/plugins/SPLA.Skills.Network/`: Network skills. Built by `SPLA.Skills.Network.csproj` (Microsoft.Build.NoTargets).
  - **Debug builds:** the `CopySkills` target copies `meta.yaml` + `skills/*.md` into a flat
    `plugins/network.skills/` folder in the CLI and UI output. Because that folder carries its own
    `type: skills` `meta.yaml`, `PluginManager` registers the skills from it directly (via
    `RegisterFromPlugin`).
  - **Publish builds:** `PublishAll.cmd` instead `xcopy`s `src\plugins\SPLA.Skills.Network\skills` into
    `plugins\network\skills\` (a `skills` subfolder of the network DLL plugin), where the legacy
    `SkillManager.LoadSkills` scan path (`plugins/*/skills/*.md`) picks them up.
  - Net effect is the same skill set either way; the two layouts + two discovery mechanisms are a
    known fragility, not a bug.

**Skill file format:**
```markdown
---
id: plugin.skill-name
description: One-line description used as the skill index entry in the system prompt.
---

# Skill Title

## Tool availability
[standard preamble — check `agent_info`, prefer lower_snake_case network tools, fall back to shell when allowed]

[skill instructions...]
```

**Runtime behavior:**
- `SkillManager` (in `SPLA.MCP.Core`) scans `plugins/*/skills/*.md` on startup and builds a registry.
- `CapabilityRegistry` includes skills alongside tools and plugins in a unified list.
- `SidebarPanelViewModel` displays skills from `CapabilityRegistry` — it does NOT scan files or own skill data.
- System prompt receives only the skill index (`id — description`, one line per skill).
- Model calls `agent_info {"id": "<skill-id>"}` to get full skill instructions when a request matches.
- `IsPreloaded=true` (per-skill setting in `.spla`) injects the full body into the system prompt immediately.
- Flags `IsEnabled`/`IsPreloaded` persist via the `skills:` section in `.spla` / `defaults.yaml`.

**Slash commands:** `/skills` lists all skills; `/skills load <id>` injects skill body into the current chat context.

## Plugins

Plugin projects use `meta.yaml` manifests and are published into `.publish/work/plugins/<plugin-id>/` by `PublishAll.cmd`.

Current plugin manifests:

- `src/plugins/SPLA.Plugins.Network/meta.yaml`: `id: network`, `type: dll`.
- `src/plugins/SPLA.Plugins.Roslyn/meta.yaml`: `id: roslyn`, `type: dll`.
- `src/plugins/SPLA.Plugins.OneC/meta.yaml`: `id: onec`, `type: dll`.
- `src/plugins/SPLA.Plugins.Sql/meta.yaml`: `id: sql`, `type: dll`. (The former `sql_avalonia` settings-editor plugin was removed; its connection editor now lives in the plugin's web settings panel.)
- `src/plugins/SPLA.Plugins.Browser/meta.yaml`: `id: browser`, `type: dll`.
- `src/plugins/SPLA.Plugins.Browser.Screencast/meta.yaml`: `id: browser_screencast`, `type: dll`.
- `src/plugins/SPLA.Plugins.Test/meta.yaml`: Test plugin manifest. The project exists in the repository but is not currently listed in `SPLA.slnx`.

## Publish Output

Generated output is not source:

- `.publish/work/`: Ready-to-run application folder.
- `.publish/work/plugins/`: Published plugin folders.
- `.publish/zip/SPLA.zip`: ZIP package created by `PublishAll.cmd`.

Do not treat `.publish/` contents as authoritative source.

## Out-of-Solution Notes

- `src/plugins/SPLA.Plugins.Test` exists with a `.csproj` and `meta.yaml`, but it is not currently included in `SPLA.slnx`.
