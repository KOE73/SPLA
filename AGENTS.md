# SPLA Agents Configuration

For comprehensive details on agent permission models, tool matrices, autonomy configurations, and documentation layout, refer to the agent documentation:

- **[Agent Security & Permission Modes](agents/security.md)**: Describes the 5 operational modes (`Chat`, `Research`, `Inspect`, `Edit`, `Agent`), allowed actions, and execution risks.
- **[Avalonia UI Development Rules](agents/avalonia.md)**: Mandatory structure rules for Avalonia UI, including the requirement to create non-trivial views in `AXAML` immediately.
- **[UI Theming & Density Guidelines](agents/ui-theming.md)**: Rules for UI styling, themes, spacing, and avoiding hardcoded layout properties.
- **[Observability](agents/observability.md)**: OpenTelemetry-ready logging, tracing, metrics, log destinations, and correlation rules.
- **[`.spla` Project File Format](agents/spla-file.md)**: Specification for the `.spla` project entry point — workspace, mode, instructions, permissions, and settings cascade.
- **[Project Structure](agents/structure.md)**: Overview of the solution layout and module responsibilities.
- **[Plugin System & Tool Naming](agents/plugins.md)**: Rules for creating plugins, extending the system prompt, and standardizing tool names (`[plugin].[domain].[action]`).
- **[Documentation Layout](agents/documentation.md)**: Defines the separation between `agents/` and `docs/`, including README translation rules.


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
