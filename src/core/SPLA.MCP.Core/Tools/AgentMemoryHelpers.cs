using SPLA.Domain.Agent;
using SPLA.MCP.Core.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SPLA.MCP.Core.Tools;

internal static class AgentMemoryHelpers
{
    internal const int AutoThreshold = 25;
    internal const int AutoSample    = 10;

    /// <summary>
    /// Resolves the target store: the shared project store for scope "project", otherwise the
    /// running chat's session store from <see cref="AgentSessionScope"/>. Returns null when scope
    /// resolves to session but no chat run is active (no ambient session).
    /// </summary>
    internal static IKeyValueStore? SelectStore(IKeyValueStore project, string? scope)
        => scope == "project" ? project : AgentSessionScope.Current?.SessionKv;

    internal static IReadOnlyList<KeyValuePair<string, string>> ApplyFilter(
        IReadOnlyList<KeyValuePair<string, string>> items, string? filter, string? where = null, bool regexMode = false)
    {
        var result = string.IsNullOrEmpty(filter)
            ? items
            : items.Where(kv => KvGlob.KeyMatch(kv.Key, filter, regexMode)).ToList();

        if (!string.IsNullOrEmpty(where))
            result = result.Where(kv => KvGlob.WhereMatch(kv.Value, where)).ToList();

        return result;
    }

    internal static string Truncate(string value, int max = 200)
        => value.Length <= max ? value : value[..max] + $"… (+{value.Length - max} chars)";

    internal static void AppendList(StringBuilder sb, IKeyValueStore store, string? filter, int? top, int skip, string? where = null, bool regexMode = false)
    {
        var all     = store.List();
        var matched = ApplyFilter(all, filter, where, regexMode);

        int effectiveSkip = System.Math.Max(0, skip);
        var page = matched.Skip(effectiveSkip).ToList();

        int showCount;
        bool autoMode = top == null;
        if (autoMode)
            showCount = page.Count <= AutoThreshold ? page.Count : AutoSample;
        else
            showCount = System.Math.Min(top!.Value, page.Count);

        var shown     = page.Take(showCount).ToList();
        int remaining = matched.Count - effectiveSkip - showCount;

        var filterPart = string.IsNullOrEmpty(filter) ? string.Empty : $", filter: '{filter}'";
        var wherePart  = string.IsNullOrEmpty(where)  ? string.Empty : $", where: '{where}'";
        var skipPart   = effectiveSkip > 0 ? $", skip: {effectiveSkip}" : string.Empty;
        var topPart    = top.HasValue ? $", top: {top}" : string.Empty;
        sb.AppendLine($"[{store.Scope}] total: {all.Count}, match: {matched.Count}{filterPart}{wherePart}{skipPart}{topPart} — showing {showCount}");

        foreach (var kv in shown)
            sb.AppendLine($"  {kv.Key} = {Truncate(kv.Value)}");

        if (remaining > 0)
        {
            int nextSkip = effectiveSkip + showCount;
            sb.AppendLine($"  hint: {remaining} more. Next page: skip={nextSkip}" +
                          (top.HasValue ? $", top={top}" : $", top={showCount}") +
                          (string.IsNullOrEmpty(filter) ? string.Empty : $", filter='{filter}'") +
                          (string.IsNullOrEmpty(where)  ? string.Empty : $", where='{where}'"));
        }
    }

    internal static void AppendClear(StringBuilder sb, IKeyValueStore store, string? filter)
    {
        var removed = store.DeleteWhere(filter);
        if (string.IsNullOrEmpty(filter))
            sb.AppendLine($"ok: cleared [{store.Scope}] — {removed} entries removed");
        else
            sb.AppendLine($"ok: cleared [{store.Scope}] filter='{filter}' — {removed} entries removed");
    }
}
