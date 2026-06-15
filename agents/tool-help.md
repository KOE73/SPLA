# agent.info — Capability Lookup

`agent.info` is the single meta-tool for looking up any agent capability: tools, skills, or a full index.
It replaces the former `tool.help` + `skill.load` pair.

## Concept

```text
agent.info {"id": "network.scan.ports"}   → tool schema + help text
agent.info {"id": "network.range-audit"}  → full skill instruction body
agent.info {}                             → full index: all tools + all skills
agent.info {"id": "scan"}                 → partial match: suggestions from tools + skills
```

The tool resolves by kind automatically. No need to know in advance whether the id is a tool or a skill.

## When to use

- Tool flag `[H]` in a description means extended help is available. Call `agent.info {"id": "<tool-name>"}` before using it if the argument formats are unclear.
- When the user's request matches a skill in the `--- Skills ---` index, call `agent.info {"id": "<skill-id>"}` to load the full instructions.
- Call `agent.info {}` to get the full capability index when orientation is needed.

## `[H]` Flag

Tools with extended documentation are marked with `[H]` in their model-facing description.

```text
[H] = extended help available via agent.info
```

`McpHost` adds this prefix automatically when a tool implements `IToolHelpProvider` and returns non-empty help text.
Do NOT write `[H]` manually in tool descriptions.

## Runtime Flow

1. Tool implements `IMcpTool`.
2. For extended docs: also implements `IToolHelpProvider`.
3. Registered in `McpHost`.
4. Model sees short description + optional `[H]`.
5. If needed, model calls `agent.info {"id": "tool-name"}`.
6. `agent.info` checks skills first (exact id match), then falls through to tool registry.
7. Returns full body (skill) or schema + help (tool).
8. Partial id returns suggestions from both tools and skills.
9. Empty id returns full index.

## Tool Contract

```csharp
public sealed class MyTool : IMcpTool, IToolHelpProvider
{
    public string Name => "network.scan.ports";

    public ToolDefinition GetDefinition() => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Scans TCP ports on one host.",
            // McpHost adds [H] automatically
        }
    };

    public string? GetHelpText() => """
        tool: network.scan.ports

        summary: Scan TCP ports on one host.

        arguments:
          host:
            required: true
          ports:
            required: false
            default: common
            formats: [common, single_port, comma_list, range]

        examples:
          - request:
              host: 192.168.1.10
              ports: 80,443
        """;
}
```

## Responses

**Skill match:**
```yaml
found: true
kind: skill
skill: network.range-audit
---
# Range Audit
... full skill body ...
```

**Tool match:**
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
```

**Partial match:**
```yaml
found: false
reason: exact_tool_not_found
suggestions:
  - network.scan.ports
  - network.scan.lan
skill_suggestions:
  - network.range-audit
```

**Not found / disabled:**
```yaml
found: false
reason: tool_not_registered_or_plugin_disabled
```

## Recommended Help Format

Help text is optimized for LLM use, not human prose.

Sections:
- `tool`: exact tool name
- `summary`: one or two lines
- `arguments`: formats, defaults, requirements, examples
- `limits`: caps, ranges, safety boundaries
- `risk`: actions requiring extra care
- `examples`: valid request objects

Keep descriptions short. Put formats, edge cases, and advanced behavior in `GetHelpText()`.
