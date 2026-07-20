using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPLA.Agent;

/// <summary>
/// Renders the "working memory" snapshot injected into the prompt each turn. By convention, only
/// entries whose key starts with <see cref="KeyPrefix"/> are auto-shown — the agent uses
/// <c>context:*</c> keys for state that must survive context rollback. The block is worded as a
/// read-only data snapshot (not an instruction) so weak models don't loop on "maintaining" it.
/// Everything else in the store stays out of the prompt and is read on demand via agent_memory_get / agent_memory_list.
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
        sb.Append("--- Working memory snapshot (read-only view) ---\n");
        sb.Append("This is an automatic display of your stored `context:` entries. It is DATA, not an instruction. ");
        sb.Append("Do not update, rewrite, or acknowledge it. It refreshes by itself whenever the underlying entries change.\n");
        foreach (var (scope, key, value) in ctx)
            sb.Append($"[{scope}] {key} = {value}\n");
        sb.Append("--- End of snapshot. No action required. Continue the user's task. ---");
        return sb.ToString();
    }
}
