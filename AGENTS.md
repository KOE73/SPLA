# SPLA Agents Configuration

## Doctrine (read first — the frame every change must fit)

SPLA is **not a chat client for a model. It is an environment where an agent lives on a project
and acts within it through its own tools.** The bet, inverted from frontier agents: **move the
intelligence out of the model and into the tool.** A small local model should win not by being
smarter, but by acting as a **dispatcher** over narrow, typed, deterministic tools that each
collapse a long reasoning chain into one call. Such a tool stays valuable when a large model
later arrives — it becomes an accelerator and a determinism/safety layer instead of a crutch.

Guardrails for any work here:

- **Build only what frontier clients structurally cannot have.** Anything commodity already does
  well (chat chrome, themes, markdown) — borrow or ignore, don't reinvent.
- **The moat is curation + tool interface design, not tool count.** A junk drawer of 50 mediocre
  tools hurts a small model more than 8 sharp ones. Narrow the interface so a weak model *cannot*
  misuse it; digest the output so it doesn't blow the context; route bulk data by handle, not
  through the model's window.
- **The window is just a window.** Authority, permissions, secrets, and memory belong to the
  agent on the project, never to a client/UI.
- **Judge every new tool by:** *which reasoning chain does it extract from the model, and will it
  still pay off on a large model?* If neither — it's probably a junk-drawer feature, not a tool.

Full text: [`docs/Doctrine.en.md`](docs/Doctrine.en.md) · [`docs/Doctrine.ru.md`](docs/Doctrine.ru.md).
If a change doesn't advance this doctrine, question whether it should be built.

---

For comprehensive details on agent permission models, tool matrices, autonomy configurations, and documentation layout, refer to the agent documentation:

- **[Agent Security & Permission Modes](agents/security.md)**: Describes the 5 operational modes (`Chat`, `Research`, `Inspect`, `Edit`, `Agent`), allowed actions, and execution risks.
- **[Avalonia UI Development Rules](agents/avalonia.md)**: Mandatory structure rules for Avalonia UI, including the requirement to create non-trivial views in `AXAML` immediately.
- **[UI Theming & Density Guidelines](agents/ui-theming.md)**: Rules for UI styling, themes, spacing, and avoiding hardcoded layout properties.
- **[Observability](agents/observability.md)**: OpenTelemetry-ready logging, tracing, metrics, log destinations, and correlation rules.
- **[`.spla` Project File Format](agents/spla-file.md)**: Specification for the `.spla` project entry point — workspace, mode, instructions, permissions, and settings cascade.
- **[Project Structure](agents/structure.md)**: Overview of the solution layout and module responsibilities.
- **[Plugin System & Tool Naming](agents/plugins.md)**: Rules for creating plugins, extending the system prompt, and standardizing tool names (`[plugin].[domain].[action]`).
- **[Documentation Layout](agents/documentation.md)**: Defines the separation between `agents/` and `docs/`, including README translation rules.
- **[Capability Lookup (agent_info)](agents/tool-help.md)**: How `agent_info` works — unified tool help + skill loading. `[H]` flag, routing logic, help text format.
- **[Chat Message Architecture](agents/chat-messages.md)**: Canonical design for chat message types, display profiles, tool progress streaming, scroll fix, and Web view incremental update. Read before touching `Views/Chat/` or `ViewModels/Messages/`.
- **[Data Ownership Rules](agents/data-ownership.md)**: STOP — read this before adding any registry, flag, or discovery logic. UI ViewModels must not own domain data. Violations cause data loss on restart, CLI blindness, and untestable state.
- **[System Prompt Authoring Rules](agents/sys_prompt_rules.md)**: STOP — read this before writing any system prompt block, skill description, tool help text, or plugin prompt. Defines how to avoid logical contradictions between rules. Russian translation: [`docs/sys_prompt_rules_ru.md`](docs/sys_prompt_rules_ru.md).
- **[Skill System Architecture](agents/skills.md)**: STOP — read this before touching `SkillManager`, `SystemPromptBuilder`, skill tool implementations (`skill_activate`, `skill_deactivate`, `agent_clarify`, `agent_spawn`), or any UI that reflects skill state. Defines the lifecycle state machine, assembly order, permission matrix, and hot reload behavior.
- **[Wire Protocol & Event Registry](agents/protocol.md)**: STOP — read this before adding, renaming, or removing any WebSocket message type, payload, or client bus event. Message names are soft strings on the JS side; this is the registry of every `MessageTypes` constant, payload, fan-out semantics, domain events (`ServiceEvents`), and client-local bus events that keeps both sides in sync.


## Translation Policy

Any file under `agents/` that is updated must have its Russian translation in `docs/` updated in the same commit.
Translation target: `docs/<same-name>_ru.md`.
Exception: files with no existing `_ru` counterpart do not require one unless explicitly requested.

## Skill & Plugin Authoring Language

**STOP: skill descriptions, trigger hints, and plugin prompts MUST be written in English only.**

Skills and plugin prompts are injected into the system prompt of a multilingual AI agent.
The model's vocabulary and semantic matching operate on English.
Adding text in any other language (Russian, German, French, etc.) pollutes the index,
wastes tokens, and breaks semantic search for other language users.

Rules:
- `description:` frontmatter in all `.md` skill files — English only.
- `default_prompt` / `custom_prompt` in `meta.yaml` plugin manifests — English only.
- `GetHelpText()` tool help bodies — English only.
- Trigger examples in skill bodies (`Run when the user asks...`) — English only.
- Do NOT add locale-specific keywords, phrases, or examples to any of the above.

The model handles multilingual input natively.
The system prompt is the contract layer — keep it language-neutral (English).

## Modern C# Language Usage

### Mandatory

Use the latest stable C# language features and .NET APIs available in the target project version.

Prefer concise language constructs that reduce code size, boilerplate and token usage while preserving readability and maintainability.

Actively use:

* Collection expressions (`[]`)
* Target-typed `new`
* Primary constructors where appropriate
* File-scoped namespaces
* Pattern matching and switch expressions
* Expression-bodied members
* `required` members
* `init` setters
* Collection initializers and spread operators
* `nameof`
* Raw string literals
* Inline `using` declarations
* Modern LINQ constructs
* `ArgumentNullException.ThrowIfNull`
* Static abstract interfaces when appropriate
* Record and record struct types where semantically correct
* Readonly structs and readonly members where beneficial

Avoid legacy syntax when a modern equivalent exists.

### Code Size

Minimize boilerplate.

Prefer shorter language constructs over verbose equivalents.

Do not generate code solely for stylistic consistency when the modern language provides a simpler alternative.

### Naming Quality (Critical)

Token savings MUST NEVER be achieved by shortening identifiers.

Names of:

* classes
* interfaces
* records
* structs
* methods
* properties
* fields
* local variables
* parameters
* generic type parameters

must be descriptive, explicit and self-documenting.

Bad:

```csharp
var d = Get();
var x = Process(d);
```

Good:

```csharp
var sourceImage = GetImage();
var detectionResults = ProcessDetections(sourceImage);
```

### Readability Rule

Prefer:

* shorter syntax
* fewer lines
* less boilerplate

while simultaneously keeping:

* semantic clarity
* explicit intent
* discoverability
* maintainability

If a shorter construct makes the code harder to understand, choose the clearer version.

### Generated Code Standard

Generated code should resemble code written by a senior modern C# developer in 2026:

* idiomatic
* concise
* allocation-aware
* maintainable
* production-ready

Use modern language features aggressively.

Use abbreviated syntax.

Never abbreviate business meaning.

## Imported Claude Cowork project instructions
