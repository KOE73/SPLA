using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System.Text;
using System.Text.Json;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Agent working memory exposed to the model: a flat key/value scratchpad in two scopes —
/// <c>session</c> (this chat, default) and <c>project</c> (shared, persistent). Keys prefixed with
/// <c>context:</c> are auto-injected into the prompt each turn. This is a fundamental
/// (<see cref="ToolScope.Agent"/>) capability: always available, in every mode.
/// </summary>
public sealed class AgentMemoryTool : IMcpTool, IToolHelpProvider
{
    private const int AutoThreshold = 25;  // below this match_count list returns everything by default
    private const int AutoSample    = 10;  // above threshold: show this many + hint

    private readonly IKeyValueStore _session;
    private readonly IKeyValueStore _project;

    public AgentMemoryTool(IKeyValueStore session, IKeyValueStore project)
    {
        _session = session;
        _project = project;
    }

    public string Name => "agent.memory";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Agent working memory (key/value). Actions: get|set|list|count|delete|clear. " +
                          "scope: session (default) or project. " +
                          "Keys with 'context:' prefix are auto-injected into your prompt every turn. " +
                          "list supports filter/top/skip for safe paginated access — never dumps all data blindly.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    action = new
                    {
                        type = "string",
                        @enum = new[] { "get", "set", "list", "count", "delete", "clear" },
                        description = "get | set | list | count | delete | clear"
                    },
                    key = new
                    {
                        type = "string",
                        description = "Entry key. Required for get/set/delete. Use namespace:name convention (e.g. context:plan, task:current)."
                    },
                    value = new
                    {
                        type = "string",
                        description = "Value to store. Required for set."
                    },
                    scope = new
                    {
                        type = "string",
                        @enum = new[] { "session", "project" },
                        description = "session = this chat (default, persists with chat); project = shared across all chats, always persistent."
                    },
                    filter = new
                    {
                        type = "string",
                        description = "list/count/clear: substring matched against keys, case-insensitive (SQL: WHERE key LIKE '%filter%'). E.g. 'context:' or 'hosts:'. clear without filter removes ALL entries in scope."
                    },
                    top = new
                    {
                        type = "integer",
                        description = "list: max entries to return (SQL: SELECT TOP N). Default: auto (all if ≤25 matches, first 10 + hint if more)."
                    },
                    skip = new
                    {
                        type = "integer",
                        description = "list: entries to skip before applying top (SQL: OFFSET N). Use with top for pagination. Default: 0."
                    }
                },
                required = new[] { "action" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;

            var action    = GetString(root, "action")?.ToLowerInvariant();
            var key       = GetString(root, "key");
            var value     = GetString(root, "value");
            var scopeName = GetString(root, "scope")?.ToLowerInvariant();
            var filter    = GetString(root, "filter");
            var top       = GetInt(root, "top");
            var skip      = GetInt(root, "skip") ?? 0;
            var store     = scopeName == "project" ? _project : _session;

            return action switch
            {
                "set" => string.IsNullOrWhiteSpace(key)
                    ? Done("error: key required for set")
                    : SetAndReport(store, key!, value ?? string.Empty),

                "get" => string.IsNullOrWhiteSpace(key)
                    ? Done("error: key required for get")
                    : Done(store.Get(key!) is { } v ? v : $"not_found: [{store.Scope}] {key}"),

                "delete" => string.IsNullOrWhiteSpace(key)
                    ? Done("error: key required for delete")
                    : Done(store.Delete(key!)
                        ? $"ok: deleted [{store.Scope}] {key}"
                        : $"not_found: [{store.Scope}] {key}"),

                "count" => Done(RenderCount(scopeName, filter)),

                "list" => Done(RenderList(scopeName, filter, top, skip)),

                "clear" => Done(RenderClear(scopeName, filter)),

                null  => Done("error: 'action' field is missing or not a string. Use: get | set | list | count | delete | clear"),
                _     => Done($"error: unknown action '{action}'. Use: get | set | list | count | delete | clear")
            };
        }
        catch (JsonException)
        {
            return Done("error: invalid_json");
        }
    }

    // ── clear ────────────────────────────────────────────────────────────────

    private string RenderClear(string? scopeName, string? filter)
    {
        var sb = new StringBuilder();
        if (scopeName != "project") AppendClear(sb, _session, filter);
        if (scopeName != "session") AppendClear(sb, _project, filter);
        return sb.ToString().TrimEnd();
    }

    private static void AppendClear(StringBuilder sb, IKeyValueStore store, string? filter)
    {
        var removed = store.DeleteWhere(filter);
        if (string.IsNullOrEmpty(filter))
            sb.AppendLine($"ok: cleared [{store.Scope}] — {removed} entries removed");
        else
            sb.AppendLine($"ok: cleared [{store.Scope}] filter='{filter}' — {removed} entries removed");
    }

    // ── count ────────────────────────────────────────────────────────────────

    private string RenderCount(string? scopeName, string? filter)
    {
        var sb = new StringBuilder();
        if (scopeName != "project") AppendCount(sb, _session, filter);
        if (scopeName != "session") AppendCount(sb, _project, filter);
        return sb.ToString().TrimEnd();
    }

    private static void AppendCount(StringBuilder sb, IKeyValueStore store, string? filter)
    {
        var all     = store.List();
        var matched = ApplyFilter(all, filter);
        sb.AppendLine($"[{store.Scope}]");
        sb.AppendLine($"  total_count: {all.Count}");
        if (!string.IsNullOrEmpty(filter))
            sb.AppendLine($"  match_count: {matched.Count}  (filter: '{filter}')");
        else
            sb.AppendLine($"  match_count: {all.Count}");
    }

    // ── list ─────────────────────────────────────────────────────────────────

    private string RenderList(string? scopeName, string? filter, int? top, int skip)
    {
        var sb = new StringBuilder();
        if (scopeName != "project") AppendList(sb, _session, filter, top, skip);
        if (scopeName != "session") AppendList(sb, _project, filter, top, skip);
        var text = sb.ToString().TrimEnd();
        return text.Length == 0
            ? (string.IsNullOrEmpty(filter) ? "(empty)" : $"(no entries matching '{filter}')")
            : text;
    }

    private static void AppendList(StringBuilder sb, IKeyValueStore store, string? filter, int? top, int skip)
    {
        var all     = store.List();
        var matched = ApplyFilter(all, filter);

        // Determine how many to actually show.
        int effectiveSkip = Math.Max(0, skip);
        var page = matched.Skip(effectiveSkip).ToList();

        int showCount;
        bool autoMode = top == null;
        if (autoMode)
            showCount = page.Count <= AutoThreshold ? page.Count : AutoSample;
        else
            showCount = Math.Min(top!.Value, page.Count);

        var shown = page.Take(showCount).ToList();
        int remaining = matched.Count - effectiveSkip - showCount;

        // Header (SQL-style metadata)
        var filterPart = string.IsNullOrEmpty(filter) ? string.Empty : $", filter: '{filter}'";
        var skipPart   = effectiveSkip > 0 ? $", skip: {effectiveSkip}" : string.Empty;
        var topPart    = top.HasValue ? $", top: {top}" : string.Empty;
        sb.AppendLine($"[{store.Scope}] total: {all.Count}, match: {matched.Count}{filterPart}{skipPart}{topPart} — showing {showCount}");

        foreach (var kv in shown)
            sb.AppendLine($"  {kv.Key} = {Truncate(kv.Value)}");

        if (remaining > 0)
        {
            int nextSkip = effectiveSkip + showCount;
            sb.AppendLine($"  hint: {remaining} more. Next page: skip={nextSkip}" +
                          (top.HasValue ? $", top={top}" : $", top={showCount}") +
                          (string.IsNullOrEmpty(filter) ? string.Empty : $", filter='{filter}'"));
        }
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static Task<string> SetAndReport(IKeyValueStore store, string key, string value)
    {
        store.Set(key, value);
        return Done($"ok: set [{store.Scope}] {key}");
    }

    private static IReadOnlyList<KeyValuePair<string, string>> ApplyFilter(
        IReadOnlyList<KeyValuePair<string, string>> items, string? filter)
        => string.IsNullOrEmpty(filter)
            ? items
            : items.Where(kv => kv.Key.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

    private static string Truncate(string value, int max = 200)
        => value.Length <= max ? value : value[..max] + $"… (+{value.Length - max} chars)";

    private static string? GetString(JsonElement root, string name)
        => root.ValueKind == JsonValueKind.Object
           && root.TryGetProperty(name, out var el)
           && el.ValueKind == JsonValueKind.String
            ? el.GetString()
            : null;

    private static int? GetInt(JsonElement root, string name)
    {
        if (root.ValueKind != JsonValueKind.Object) return null;
        if (!root.TryGetProperty(name, out var el)) return null;
        return el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var v) ? v : null;
    }

    private static Task<string> Done(string result) => Task.FromResult(result);

    public string? GetHelpText() => """
        tool: agent.memory

        summary: Agent working memory — key/value scratchpad, two scopes, full SQL-style access.

        scopes:
          session: this chat only (default). Persists with the chat across restarts.
          project: shared across all chats in this project. Always persistent.

        actions:
          set:    Requires key + value. Overwrites existing key.
          get:    Requires key. Returns value or not_found.
          delete: Requires key. Removes the entry.
          count:  Returns total_count and match_count (with optional filter). Zero data overhead.
          list:   Paginated listing. Never dumps all data blindly — see pagination below.
          clear:  Bulk delete. No filter = wipe entire scope. filter='prefix:' = remove matching keys only.
                  Returns count of removed entries. Use instead of many individual deletes.

        key naming convention — always namespace:name (lower-case-kebab):
          context:  Auto-injected into your prompt every turn. Live working state.
                    RULE: delete context: entries when stale — keep the prompt clean.
                    Examples: context:plan, context:step, context:target-host
          task:     Task/progress tracking. Not auto-injected.
                    Examples: task:current, task:next
          project:  Persistent project knowledge. Use scope=project.
                    Examples: project:build-cmd, project:test-runner
          note:     Free-form notes.
                    Examples: note:constraint, note:finding

        list pagination (SQL analogies):
          filter  WHERE key LIKE '%x%'   substring match on key, case-insensitive
          top     SELECT TOP N            max entries to return
          skip    OFFSET N               entries to skip (combine with top for pages)

          default behaviour (no top):
            match ≤ 25  → return all
            match > 25  → return first 10 + hint with next-page args

          response header always shows: total / match / skip / top / showing N
          hint line shows exact args for the next page.

        recommended workflow for large stores:
          1. count first  → { action: count, filter: "hosts:" }
                            total_count: 1542 / match_count: 87
          2. sample       → { action: list, filter: "hosts:", top: 5 }
          3. paginate     → { action: list, filter: "hosts:", top: 20, skip: 0 }
                            { action: list, filter: "hosts:", top: 20, skip: 20 }

        examples:
          - { action: set,    key: "context:plan",    value: "1) scan 2) audit 3) report" }
          - { action: set,    key: "context:step",    value: "auditing 10.0.0.5" }
          - { action: set,    key: "task:current",    value: "port scan 10.0.0.0/24" }
          - { action: set,    scope: project, key: "project:build-cmd", value: "dotnet build" }
          - { action: get,    key: "context:plan" }
          - { action: count }
          - { action: count,  filter: "hosts:" }
          - { action: list }
          - { action: list,   filter: "context:" }
          - { action: list,   filter: "hosts:", top: 10 }
          - { action: list,   filter: "hosts:", top: 10, skip: 10 }
          - { action: delete, key: "context:step" }
          - { action: clear }                              — wipe entire session
          - { action: clear, scope: project }              — wipe entire project scope
          - { action: clear, filter: "context:" }          — remove only context: keys
          - { action: clear, scope: session, filter: "task:" }
        """;
}
