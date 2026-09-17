# Tool Argument Parsing — Conventions

How tools in SPLA read their JSON arguments.

## Core Rule

**All tools parse arguments manually via `ToolJson`.** Do not use `JsonSerializer.Deserialize<T>` for tool parameters.

Why:
- Language models are unreliable sources: they can send `null` for any field, add unexpected fields, or omit optional ones.
- `JsonSerializer.Deserialize<T>` throws `JsonException` when a JSON-null lands in a non-nullable value type (`int`, `bool`). This causes the tool to return a generic parse error instead of useful feedback.
- `ToolJson` methods are null-safe by design: absent field and JSON-null both return the method's "absent" result, so no special handling is needed.

## ToolJson API

All methods live in `SPLA.MCP.Core.Json.ToolJson` (static class).

### String

```csharp
// Returns null if absent or JSON-null.
string? GetString(JsonElement root, string name)

// Same, but Trim()s the result. Returns null when trimmed value is empty.
// Use for host names, paths, identifiers — anything where whitespace is noise.
string? GetStringTrimmed(JsonElement root, string name)
```

### Integer

```csharp
// Returns null if absent, JSON-null, or not a number.
int? GetInt32(JsonElement root, string name)

// Returns defaultValue if absent or JSON-null.
int GetInt32(JsonElement root, string name, int defaultValue)

// Returns defaultValue if absent or JSON-null; Math.Clamp(value, min, max) otherwise.
// Use for timeouts, counts, ports — any bounded numeric parameter.
int GetInt32Clamped(JsonElement root, string name, int defaultValue, int min, int max)
```

### Boolean

```csharp
// Returns defaultValue if absent, JSON-null, or not a boolean token.
bool GetBoolean(JsonElement root, string name, bool defaultValue)
```

### Array

```csharp
// Returns null if absent, JSON-null, or not a JSON array.
// Empty array → non-null empty string[].
// Non-string elements are silently skipped.
string[]? GetStringArray(JsonElement root, string name)
```

### Dictionary

```csharp
// Returns null if absent, JSON-null, or not a JSON object.
// Only string-valued properties are included; others are silently skipped.
// Key comparison is OrdinalIgnoreCase.
// Use for HTTP headers, metadata maps.
Dictionary<string, string>? GetStringDictionary(JsonElement root, string name)
```

---

## Patterns

### Required field

```csharp
var host = ToolJson.GetStringTrimmed(root, "host");
if (host is null) return "error: host is required";
```

No special helper needed — GetStringTrimmed returns null for absent/null/empty, the null check is the guard.

### Optional field with default

```csharp
// string
var server = ToolJson.GetStringTrimmed(root, "server") ?? "localhost";

// int
var timeout = ToolJson.GetInt32(root, "timeout", 3000);

// bool
var regex = ToolJson.GetBoolean(root, "regex", false);
```

### Bounded integer (timeout, port, count)

```csharp
var port     = ToolJson.GetInt32Clamped(root, "port",     defaultValue: 443,  min: 1,   max: 65535);
var timeout  = ToolJson.GetInt32Clamped(root, "timeout",  defaultValue: 3000, min: 500, max: 30_000);
var count    = ToolJson.GetInt32Clamped(root, "count",    defaultValue: 10,   min: 1,   max: 100);
```

### String array

```csharp
var engines = ToolJson.GetStringArray(root, "engines");
// engines is null when omitted, string[] when present (possibly empty)
```

### Object / header map

```csharp
var headers = ToolJson.GetStringDictionary(root, "headers");
if (headers != null)
    foreach (var (key, val) in headers)
        request.Headers.TryAddWithoutValidation(key, val);
```

### Multiple required fields at once

```csharp
var path    = ToolJson.GetStringTrimmed(root, "path");
var oldText = ToolJson.GetString(root, "old_text");
var newText = ToolJson.GetString(root, "new_text");

if (path is null || oldText is null || newText is null)
    return "error: path, old_text and new_text are required";
```

---

## Strict Schema

Every tool should set `StrictSchema = true` in its `ToolFunctionDefinition` when the schema supports it.

**Compatible** = all properties are either in `required` OR declared with a nullable JSON type (`["string", "null"]`).

When `StrictSchema = true` the serializer adds `"strict": true` to the OpenAI-compatible function payload, which instructs the provider to enforce the schema at grammar level — the model cannot deviate from it.

**Required fields stay as `"type": "string"`.
Optional fields use `"type": ["string", "null"]` AND are listed in `required`.**

```csharp
// required string
host = new { type = "string", description = "..." }

// optional string
server = new { type = new[] { "string", "null" }, description = "... Null = localhost." }

// optional int
timeout = new { type = new[] { "integer", "null" }, description = "... Null = 3000 ms." }

// optional bool
regex = new { type = new[] { "boolean", "null" }, description = "... Null = false." }

// required array — all in required[]
required = new[] { "host", "server", "timeout", "regex" }
```

Tools that use `JsonSerializer.Deserialize<T>` with non-nullable value types (`int`, `bool`) **cannot** use `StrictSchema = true` safely — the model may send null and the deserializer throws. Migrate to `ToolJson` first.

### Under StrictSchema, "not supplied" is a value — never an absence

Because every property sits in `required`, **a model cannot leave a field out.** It must put something there, and what it puts is `0`, `""`, `null`, or the literal string `"null"`. A tool that reads mere presence as intent is reading noise.

This is harmless until a tool has **mutually exclusive argument groups** — "set it absolutely, or nudge it, or move one side". Then it becomes a trap with no exit:

1. the model sends the group it wants, and fills the others with zeros because it has to;
2. the tool sees two groups and refuses the call;
3. the error tells the model to send one group alone;
4. the schema forbids exactly that, so the model resends the same call — and keeps resending until a loop guard kills the turn.

This happened: `geom_box` deadlocked a live model on `{"cx":375,…,"dx":0,…,"edge":"null","by":0}` five times in a row.

Rules that follow:

- **Treat zero as absent for every optional number, and accept what that costs.** The tempting refinement — "zero is absent for a displacement like `dx`, but a value like `cx` or `angle` may legitimately be zero" — sounds right and fails in practice: the model fills the *value* group with zeros too, and the deadlock simply moves. It cost a second live run of thirteen identical refused calls to learn this.

  What you give up is the ability to say "set this to exactly zero". Price it honestly: for `geom_box`, `cx=0` puts a box half off-screen and `width=0` cannot be drawn, so only `angle=0` — "straighten it" — was a real loss, and it survives as a `dangle` of the opposite sign. If a field genuinely needs zero as a command, that is a signal the argument groups are wrong, not that the rule needs an exception. **One rule the model can learn beats a rule with a carve-out it cannot.**
- **Read `"null"` as null.** Models emit the string when forced to fill a field they are not using.
- **Never write an error that asks the model to omit a field.** It cannot. Say which value means untouched instead: *"leave those at null or 0"*.
- **A zero must not double as a command.** If a no-op call currently means "re-render" or "show me the state", give that its own tool — once zero means absent, the trick stops working.

---

## Full Example

```csharp
public Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct = default)
{
    try
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;

        // required
        var host = ToolJson.GetStringTrimmed(root, "host");
        if (host is null) return Task.FromResult("error: host is required");

        // optional bounded int
        var port    = ToolJson.GetInt32Clamped(root, "port",    defaultValue: 443,  min: 1,   max: 65535);
        var timeout = ToolJson.GetInt32Clamped(root, "timeout", defaultValue: 3000, min: 500, max: 30_000);

        // optional bool
        var secure = ToolJson.GetBoolean(root, "secure", true);

        // optional string array
        var patterns = ToolJson.GetStringArray(root, "patterns");

        // optional header map
        var headers = ToolJson.GetStringDictionary(root, "headers");

        // ... execute
    }
    catch (JsonException) { return Task.FromResult("error: invalid_json"); }
}
```
