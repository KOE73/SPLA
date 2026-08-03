# Tool Sets — Levels and Activation

A **tool set** is the unit the user levels and the agent activates. A plugin is the unit of *delivery*
(assembly, settings, credentials, UI panels) and takes no part in exposure. Today each plugin ships
exactly one set with the plugin's own id; several sets out of one assembly is supported by the model
and not yet used.

Read this before touching `ToolSetRegistry`, `ToolSetSession`, `McpHost` gating, `toolset_*` tools, or
the tool-set line in the status bar.

## The two halves

| | Level | Activation |
|---|---|---|
| Question | "is this allowed at all?" | "is it armed right now?" |
| Decided by | the person, in settings | a skill, the agent, or the person |
| Lives in | `toolsets:` in `.spla` / defaults | `IToolSetSession` on the chat |
| Lifetime | until the setting changes | until the chat ends or it is lowered |

The agent can never widen what the level allows — it only chooses from inside it. Parallel chats each
have their own activation, so "the agent raised ssh" never leaks into the chat next door.

## Levels

| Level (`toolsets:` value) | In context | Raised by |
|---|---|---|
| `disabled` | nothing; the set does not exist for the model | — |
| `skill_demand` | nothing until a skill requires it | a skill's `requires`, mechanically |
| `agent_demand` | one declaration line | the model, via `toolset_activate` |
| `enabled` | full definitions of every tool | — (always disclosed) |

**A set with no entry derives its level from its supplier's on/off flag** — an enabled plugin is
`enabled`, a disabled one is `disabled`. That is what keeps projects written before tool sets
behaving exactly as they did, with no migration.

`on` and `off` are not accepted as level words: YAML 1.1 reads them as booleans, and a level whose
meaning depends on quoting is a trap.

## The two texts a set author writes

Definitions are generated from the tools' own schemas. Only two things are written by hand:

- **`description`** in the plugin's `meta.yaml` — what the set is.
- **`summon`** in the same file — *when to call for it*. This is the half that cannot be derived, and
  it is the whole value of the `agent_demand` level. Same role as a skill's description in the skills
  index. English only.

Per-tool detail (argument formats, limits, examples) goes in `ToolFunctionDefinition.Details`, next to
the schema it documents. `McpHost` folds it into the tool's description at the moment the set is
disclosed.

> **There is no help tool.** `agent_info`, `IToolHelpProvider`, `GetHelpText()` and the `[H]` marker
> were removed in favour of this: documentation arrives with the tool or not at all. The model never
> decides whether to go and read more, no turn is spent on a lookup, and no documentation lands loose
> in the middle of the conversation where a rewind or a compaction can drop it.

## Refusals

What the model is told when it calls a tool it cannot use is deliberate:

- **`disabled`** → `Tool 'x' not found.` A set the user levelled off must not admit it exists.
- **not raised** → the set is named, along with who can raise it. A dead end costs more than the
  disclosure: a model told "no such tool" about a tool it can see earlier in the history just retries.

## Where the code is

| Piece | File |
|---|---|
| Levels, provenance | `SPLA.MCP.Core/ToolSets/ToolSetLevel.cs` |
| Set catalogue, level resolution | `SPLA.MCP.Core/ToolSets/ToolSetRegistry.cs` |
| Per-chat activation | `SPLA.Domain/Agent/ToolSetSession.cs` |
| Gating (disclosure + refusal) | `SPLA.MCP.Core/McpHost.cs` |
| Model-facing tools | `SPLA.MCP.Core/Tools/ToolSetActivateTool.cs` |
| Declaration block in the prompt | `SPLA.Agent/Composition/ToolSetsContributor.cs` |
| Skill-driven raising | `SPLA.MCP.Core/Tools/SkillActivateTool.cs` |

Design record: [`PLAN_20260803_core_toolset-levels`](../docs/plans/PLAN_20260803_core_toolset-levels.md)
and §6 of [`IDEA_20260802_core_maf-directions`](../docs/ideas/IDEA_20260802_core_maf-directions.md).

## Rules

- **Never gate a tool by level directly.** The level belongs to the set; a single tool is switched off
  with `settings.Plugins[id].Tools[toolName]`, which is a different question.
- **Skill requirements resolve against `GetPermittedToolNames()`, not `GetToolDefinitions()`.** A skill
  at `skill_demand` exists to raise the set it needs; judging it against what is raised beforehand
  would mark exactly those skills unavailable.
- **Deactivation is a permission for the model, never a duty.** No prompt text may tell it that it
  must release a set — that reintroduces the bookkeeping decision this design removed. The person's
  control is the status bar.
- **A set a skill raised is the skill's to drop.** `toolset_deactivate` refuses it.
