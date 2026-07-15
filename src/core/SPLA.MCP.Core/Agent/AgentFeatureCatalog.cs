using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.MCP.Core.Agent;

/// <summary>
/// Canonical order and dependency graph of the built-in agent capability ids ("core.*"). Pure
/// metadata — no tool or prompt content — so both the tool-registration side (AgentRuntime, which
/// knows how to build each feature's concrete <see cref="IMcpTool"/> instances) and the prompt side
/// (SystemPromptBuilder, which knows each feature's prompt fragment text) resolve the same
/// <c>agent.capabilities</c> setting into the same enabled-id set, in the same order, once.
/// </summary>
public static class AgentFeatureCatalog
{
    /// <summary>All known feature ids, in the order features are registered/rendered.</summary>
    public static readonly IReadOnlyList<string> Order = new[]
    {
        "core.workspace",
        "core.tool-help",
        "core.discipline",
        "core.files",
        "core.shell",
        "core.web",
        "core.memory",
        "core.checkpoints",
        "core.skills",
        "core.spawn",
        "core.clarify",
        "core.blobs",
    };

    private static readonly Dictionary<string, string[]> RequiresMap = new(System.StringComparer.Ordinal)
    {
        ["core.checkpoints"] = new[] { "core.memory" },
        ["core.skills"] = new[] { "core.tool-help" },
    };

    /// <summary>Ids another feature depends on, or empty when it has none.</summary>
    public static IReadOnlyList<string> RequiresOf(string id)
        => RequiresMap.TryGetValue(id, out var r) ? r : System.Array.Empty<string>();

    /// <summary>
    /// Resolves the <c>agent.capabilities</c> project setting against the canonical catalog:
    /// <list type="bullet">
    /// <item><c>null</c> configured list → every known feature is enabled (full backward compatibility).</item>
    /// <item>empty list → no feature is enabled.</item>
    /// <item>unknown id → dropped, with a warning logged.</item>
    /// <item>a feature's <see cref="RequiresOf"/> deps are auto-included transitively, with an info log.</item>
    /// </list>
    /// Returns the enabled ids in canonical <see cref="Order"/>, regardless of the input order.
    /// </summary>
    public static IReadOnlyList<string> Resolve(IReadOnlyList<string>? configured, ILogger? logger = null)
    {
        if (configured == null) return Order;

        var known = new HashSet<string>(Order, System.StringComparer.Ordinal);
        var enabled = new HashSet<string>(System.StringComparer.Ordinal);
        var queue = new Queue<string>();

        foreach (var id in configured)
        {
            if (!known.Contains(id))
            {
                logger?.LogWarning("Unknown agent capability id ignored: {Id}", id);
                continue;
            }
            if (enabled.Add(id)) queue.Enqueue(id);
        }

        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            foreach (var dep in RequiresOf(id))
            {
                if (!enabled.Add(dep)) continue;
                logger?.LogInformation("Auto-enabling required capability {Dep} for {Id}", dep, id);
                queue.Enqueue(dep);
            }
        }

        return Order.Where(enabled.Contains).ToList();
    }
}
