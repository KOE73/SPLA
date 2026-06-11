# SPLA Plugin System & Naming Conventions

SPLA supports a dynamic plugin system that allows the AI agent to gain new capabilities without modifying the core application. 

## Plugin Discovery
Plugins are located in the `plugins/` directory adjacent to the main executable. Each plugin must be in its own subdirectory and contain a `meta.yaml` manifest.

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
To prevent collisions and maintain a clean architecture, all tools exposed by plugins **MUST** follow this structured naming convention:

`[plugin].[domain].[action]`

- **plugin**: The unique identifier of the plugin (matches `id` in `meta.yaml`). E.g., `onec`, `system`, `git`.
- **domain**: The area of responsibility or subsystem. E.g., `fs`, `network`, `object`.
- **action**: The specific action performed by the tool. E.g., `read`, `find`, `explain`.

### Examples
- `system.fs.read`
- `onec.object.explain`
- `test.sys.ping`

## Tool Help
Tool descriptions should stay short so the model can choose the right tool without loading rare usage details into every prompt.

Complex tools SHOULD implement `IToolHelpProvider` and return LLM-oriented help text:

```csharp
public sealed class MyTool : IMcpTool, IToolHelpProvider
{
    public string? GetHelpText() => """
        tool: my_plugin.domain.action

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

SPLA registers one system meta-tool, `tool.help`, which routes by active tool name through `McpHost`.

When a tool implements `IToolHelpProvider`, `McpHost` automatically prefixes its model-facing description with `[H]`. Do not write this flag manually in tool descriptions.

See [Tool Help System](tool-help.md) for the full flow and examples.

Rules:

- `tool.help` only returns help for tools currently registered in the host.
- Disabled plugins or disabled plugin tools are not visible through `tool.help`.
- Tools unavailable in the current agent mode are not exposed through `tool.help`.
- If the name is partial or inexact, `tool.help` returns suggestions from visible tools.
- Do not move detailed usage formats into `description`; keep them in `GetHelpText()`.

## Project Settings Integration (`.spla`)
Plugins and their specific tools can be toggled via the `.spla` project file:

```yaml
plugins:
  test:
    enabled: true
    custom_prompt: "Override the default plugin prompt here."
    tools:
      test.sys.ping: false # Disables just this specific tool
```
