using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPLA.Agent;

/// <summary>
/// Renders the live "working memory" block injected into the prompt each turn. By convention, only
/// entries whose key starts with <see cref="KeyPrefix"/> are auto-shown — the agent puts its plan,
/// current step, etc. under <c>context:*</c> keys and removes them when stale. Everything else in
/// the key/value store stays out of the prompt and is read on demand via agent_memory_get / agent_memory_list.
/// </summary>
public static class WorkingMemoryInjector
{
    public const string KeyPrefix = "context:";

    /// <summary>
    /// Builds the block from store entries (scope, key, value). Returns <c>null</c> when there are
    /// no <c>context:*</c> entries, so the caller can skip injecting an empty section.
    /// </summary>
    public static string? Render(IEnumerable<(string scope, string key, string value)> entries)
    {
        var ctx = entries
            .Where(e => e.key.StartsWith(KeyPrefix, System.StringComparison.Ordinal))
            .ToList();
        if (ctx.Count == 0) return null;

        var sb = new StringBuilder();
        sb.Append("--- Working memory (live) ---\n");
        sb.Append("These entries are auto-shown because their keys start with `context:`. ");
        sb.Append("Maintain them with agent_memory_set / agent_memory_delete; remove stale entries.\n");
        foreach (var (scope, key, value) in ctx)
            sb.Append($"[{scope}] {key} = {value}\n");
        return sb.ToString().TrimEnd();
    }
}
