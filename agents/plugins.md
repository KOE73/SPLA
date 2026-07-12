# SPLA Plugin System & Naming Conventions

SPLA supports a dynamic plugin system that allows the AI agent to gain new capabilities without modifying the core application. 

## Plugin Discovery
Plugins are located in the `plugins/` directory adjacent to the main executable. Each plugin must be in its own subdirectory and contain a `meta.yaml` manifest.

DLL plugins may additionally implement `ISplaPluginPanelProvider`. The service then routes opaque
`plugin.panel.*` messages between the web panel and the plugin-owned session without learning the
provider's domain model. Panel providers own their sessions; the UI owns only layout and viewer state.

## `meta.yaml` Structure
```yaml
id: my_plugin
version: 1.0.0
type: dll             # Or 'exe' for out-of-process
entry_point: MyPlugin.dll
default_prompt: |
  You are equipped with my_plugin tools. 
  Use them when the user asks about my specific domain.
```

## Tool Naming Convention
Model-facing tool names are part of the LLM contract. They must be stable, easy to copy into tool calls, and compatible with OpenAI-style function/tool calling.

All tools exposed to the model **MUST** use lower_snake_case:

`<domain>_<action>[_object]`

Rules:

- Use only ASCII letters, digits, and underscore.
- Use lowercase only.
- Do not use dots, spaces, Cyrillic, CamelCase, or namespace-style names.
- Keep the name atomic and understandable without UI hierarchy.
- Keep the name at most 64 characters.
- Use the broad functional domain as the prefix, not the plugin folder or UI group unless they are the same concept.
- Do not rely on the name as the whole description; the tool description must explain when to use the tool, required inputs, and practical limits.

The compatible provider-level shape is `^[a-zA-Z0-9_-]{1,64}$`; SPLA intentionally narrows this to lower_snake_case so models do not have to choose between dotted namespaces, hyphens, and mixed casing.

### Examples
- `system_read_file`
- `network_discover_hosts`
- `network_scan_tcp_ports`
- `onec_explain_object`
- `plugin_run_command`

## Tool Help
Tool descriptions should stay short so the model can choose the right tool without loading rare usage details into every prompt.

Complex tools SHOULD implement `IToolHelpProvider` and return LLM-oriented help text:

```csharp
public sealed class MyTool : IMcpTool, IToolHelpProvider
{
    public string? GetHelpText() => """
        tool: my_domain_do_action

        summary: ...

        arguments:
          target:
            formats:
              - ...

        limits:
          maxItems: 100

        examples:
          - request:
              target: example
        """;
}
```

SPLA registers one system meta-tool, `agent_info`, which routes by active tool name through `McpHost`.

When a tool implements `IToolHelpProvider`, `McpHost` automatically prefixes its model-facing description with `[H]`. Do not write this flag manually in tool descriptions.

See [Tool Help System](tool-help.md) for the full flow and examples.

Rules:

- `agent_info` only returns help for tools currently registered in the host.
- Disabled plugins or disabled plugin tools are not visible through `agent_info`.
- Tools unavailable in the current agent mode are not exposed through `agent_info`.
- If the name is partial or inexact, `agent_info` returns suggestions from visible tools.
- Do not move detailed usage formats into `description`; keep them in `GetHelpText()`.

## Project Settings Integration (`.spla`)
Plugins and their specific tools can be toggled via the `.spla` project file:

```yaml
plugins:
  test:
    enabled: true
    custom_prompt: "Override the default plugin prompt here."
    tools:
      test_ping_host: false # Disables just this specific tool
```

## Skills

Skills are instruction documents (`.md` files), not compiled code. They live in `SPLA.Skills.<PluginId>/`, separate from the plugin project.

### Naming convention

`[plugin-id].[skill-name]` — set in the frontmatter `id:` field, not derived from the filename.

Examples: `network.range-audit`, `network.host-audit`, `onec.object-explain`.

### File structure

```
SPLA.Skills.Network/
  SPLA.Skills.Network.csproj   ← Microsoft.Build.NoTargets, CopySkills target
  network-range-audit.md
  host-audit.md
  ...
```

### Skill `.md` frontmatter

```markdown
---
id: network.range-audit
description: One-line description shown in the system prompt index.
---
```

### Runtime

- `SkillManager` scans `plugins/*/skills/*.md` at startup — no plugin dependency.
- `agent_info {"id": "<skill-id>"}` returns the full skill body on demand; model calls it when the request matches.
- `IsEnabled` / `IsPreloaded` flags are persisted in `.spla` under `skills:`.

```yaml
skills:
  network.range-audit:
    enabled: true
    preloaded: false   # true = inject full body into system prompt immediately
```

### Adding a new skill project

1. Create `SPLA.Skills.<Name>/` directory.
2. Add `SPLA.Skills.<Name>.csproj` using `Microsoft.Build.NoTargets` SDK with a `CopySkills` target that copies `*.md` to the correct `plugins/<id>/skills/` subfolder in UI and CLI output.
3. Add the project to `SPLA.slnx`.
4. Add an `xcopy` line for it in `PublishAll.cmd`.
