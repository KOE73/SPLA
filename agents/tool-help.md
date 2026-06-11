# Tool Help System

SPLA keeps tool descriptions short and moves detailed usage documentation into an on-demand help channel.

## Goal

The model should see enough information to choose a tool, but not every rare argument format, limit, and edge case upfront.

```text
tool schema = short model-facing selection metadata
tool help   = detailed usage documentation requested only when needed
```

## Core Rule

Tools with extended documentation are marked with the `[H]` flag in their description.

```text
[H] = extended help available
```

If a `[H]` tool's argument formats, limits, ranges, or advanced behavior are unclear, the model should call `tool.help` before invoking the tool.

The model must not guess complex argument formats.

## Runtime Flow

1. A tool implements `IMcpTool`.
2. If the tool has extended documentation, it also implements `IToolHelpProvider`.
3. The plugin or system code registers the tool in `McpHost`.
4. `McpHost` stores only active, enabled tools.
5. When model-facing tool definitions are requested, `McpHost.GetToolDefinitions()` checks each registered tool.
6. If a tool implements `IToolHelpProvider` and returns non-empty help text, `McpHost` prefixes its description with `[H]`.
7. The model sees the short description plus `[H]`.
8. If details are needed, the model calls `tool.help` with the target tool name.
9. `tool.help` asks `McpHost` for help by name.
10. `McpHost` returns help only for tools that are registered, enabled, and available in the current agent mode.
11. If the tool name is partial or inexact, `McpHost` returns suggestions from visible tools.
12. Disabled plugins, disabled tools, and tools denied by the current mode are not exposed through `tool.help`.

## Tool Contract

Use `IToolHelpProvider` for tools with non-trivial formats, limits, or risk rules.

```csharp
public sealed class PortScanTool : IMcpTool, IToolHelpProvider
{
    public string Name => "network.scan.ports";

    public ToolDefinition GetDefinition() => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Scans TCP ports on one host.",
            Parameters = ...
        }
    };

    public string? GetHelpText() => """
        tool: network.scan.ports

        summary: Scan TCP ports on one host. Use for a single host or IP, not for subnet scanning.

        arguments:
          host:
            required: true
            formats:
              - hostname
              - ipv4_address
          ports:
            required: false
            default: common
            formats:
              - common
              - single_port
              - comma_list
              - range
            examples:
              - 80
              - 22,80,443
              - 1-1024

        risk:
          all_ports: use only when explicitly requested by the user.

        examples:
          - request:
              host: 192.168.1.10
              ports: 80,443
        """;
}
```

Do not manually write `[H]` in the tool description. `McpHost` adds it automatically.

## Model-Facing Example

Registered tool:

```text
name: network.scan.ports
description: Scans TCP ports on one host.
```

Tool implements `IToolHelpProvider`, so the model receives:

```text
name: network.scan.ports
description: [H] Scans TCP ports on one host.
```

When unsure, the model calls:

```json
{
  "name": "network.scan.ports"
}
```

`tool.help` returns:

```yaml
found: true
tool: network.scan.ports
plugin: network
description: Scans TCP ports on one host.
parameters:
  ...
help: |
  tool: network.scan.ports
  summary: ...
  arguments:
    ...
```

## Missing Or Disabled Tools

If a tool is disabled, its plugin is disabled, or it is not registered:

```yaml
found: false
reason: tool_not_registered_or_plugin_disabled
```

If a tool exists but is unavailable in the current agent mode:

```yaml
found: false
reason: tool_not_available_in_current_mode
```

If the name is inexact:

```yaml
found: false
reason: exact_tool_not_found
suggestions:
  - network.scan.ports
  - network.scan.lan
```

## Recommended Help Format

Help text should be optimized for LLM use, not human prose.

Recommended sections:

- `tool`: exact tool name.
- `summary`: one or two lines.
- `arguments`: formats, defaults, requirements, and examples.
- `limits`: caps, ranges, and safety boundaries.
- `risk`: actions that require extra care.
- `examples`: valid request objects.

Keep descriptions short. Put formats, edge cases, and advanced behavior in `GetHelpText()`.
