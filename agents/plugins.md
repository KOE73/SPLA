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

## Tool Details
Tool descriptions stay short: the model chooses between tools by them, and rare usage detail read on
every request is a cost paid by every other tool too.

Everything that does not fit one line — argument formats, defaults, limits, worked examples — goes in
`Details` on the tool's definition, next to the schema it documents:

```csharp
public ToolDefinition GetDefinition() => new()
{
    Type = "function",
    Function = new ToolFunctionDefinition
    {
        Name = Name,
        Details = DetailsText,
        Description = "Scans TCP ports on one host.",
        ...
    }
};

private static readonly string DetailsText = """
    tool: network_scan_tcp_ports

    summary: Scan TCP ports on one host.

    arguments:
      ports:
        default: common
        formats: [common, single_port, comma_list, range]

    limits:
      maxItems: 100

    examples:
      - request:
          host: 192.168.1.10
          ports: 80,443
    """;
```

`McpHost` folds `Details` into the model-facing description at the moment the tool's set is disclosed.

Rules:

- **There is no help tool and no `[H]` marker.** They were removed with the tool-set work: a tool is
  disclosed with everything it has to say about itself, or not at all. Do not reintroduce a lookup
  call — see [Tool Sets](toolsets.md).
- Keep `Description` to one or two lines; put every format and edge case in `Details`.
- `Details` is written for a model, not for a person: terse, structured, example-first. English only.
- A tool whose set is not disclosed contributes nothing at all — neither description nor details.

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

### The `"*"` entry

`IsPluginEnabled` resolves in this order: an entry naming the plugin wins if it sets `enabled`;
otherwise an entry under the key `"*"` wins if it sets `enabled`; otherwise the plugin is enabled.

```yaml
plugins:
  "*":
    enabled: false   # every plugin without its own entry is off
```

This is what a `minimal` launch profile (see [`spla-file.md`](spla-file.md#the--plugin-entry)) writes
instead of naming every plugin installed on the machine that created the project: the manifest travels
in git, and a list of one machine's plugins does not belong in a file another machine will check out.

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

- `SkillLibrary` scans `plugins/*/skills/*.md` at startup — no plugin dependency.
- `skill_activate {"id": "<skill-id>"}` injects the full skill body into the prompt for the run; the model calls it when the request matches.
- The `IsEnabled` flag is persisted in `.spla` under `skills:`.

```yaml
skills:
  network.range-audit:
    enabled: true
```

### Adding a new skill project

1. Create `SPLA.Skills.<Name>/` directory.
2. Add `SPLA.Skills.<Name>.csproj` using `Microsoft.Build.NoTargets` SDK with a `CopySkills` target that copies `*.md` to the correct `plugins/<id>/skills/` subfolder in UI and CLI output.
3. Add the project to `SPLA.slnx`.
4. Add an `xcopy` line for it in `PublishAll.cmd`.
