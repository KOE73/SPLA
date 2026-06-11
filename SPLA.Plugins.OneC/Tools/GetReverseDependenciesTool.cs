using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Tools;

/// <summary>
/// onec.get_reverse_dependencies — who depends on this object?
/// Used for impact analysis: "what breaks if I change X?"
/// </summary>
public class GetReverseDependenciesTool : IMcpTool
{
    private readonly OneCIndexDatabase _db;

    public string Name => "onec.dependency.reverse";
    public string Description => "Gets all incoming dependencies to a 1C object (who uses this object). Returns a graph-like structure.";

    public GetReverseDependenciesTool(OneCIndexDatabase db) => _db = db;

    public ToolDefinition GetDefinition() => new()
    {
        Function = new ToolFunctionDefinition
        {
            Name        = Name,
            Description = "Return the reverse dependency graph (who depends on this object) up to a given depth. " +
                          "Use this to assess the impact of changing a register, module, or catalog.",
            Scope       = global::SPLA.Domain.Models.ToolScope.Project,
            Effect      = global::SPLA.Domain.Models.ToolEffect.Read,
            Risk        = global::SPLA.Domain.Models.ToolRisk.Low,
            Parameters  = new
            {
                type       = "object",
                properties = new
                {
                    fullName      = new { type = "string",  description = "Root object full name." },
                    depth         = new { type = "integer", description = "Traversal depth 1 or 2 (default 1)." },
                    relationTypes = new { type = "array",   items = new { type = "string" }, description = "Relation types to follow (default all)." },
                    limit         = new { type = "integer", description = "Max edges to return (default 100)." },
                },
                required = new[] { "fullName" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var doc      = JsonDocument.Parse(argumentsJson);
        var fullName = doc.RootElement.TryGetProperty("fullName", out var fn) ? fn.GetString() ?? "" : "";
        var depth    = doc.RootElement.TryGetProperty("depth",    out var d)  ? Math.Clamp(d.GetInt32(), 1, 2) : 1;
        var limit    = doc.RootElement.TryGetProperty("limit",    out var lm) ? lm.GetInt32() : 100;

        string[]? types = null;
        if (doc.RootElement.TryGetProperty("relationTypes", out var rt) && rt.ValueKind == System.Text.Json.JsonValueKind.Array)
            types = rt.EnumerateArray().Select(e => e.GetString()!).ToArray();

        var root = _db.GetObjectByFullName(fullName);
        if (root is null)
            return Task.FromResult($"error: object '{fullName}' not found in index.");

        var nodes   = new Dictionary<string, string>();
        var edges   = new List<(string From, string To, string Type)>();
        var visited = new HashSet<long>();

        Traverse(root.Id, fullName, root.Kind, depth, types, nodes, edges, visited, limit);

        var yaml = YamlResponse.Object(b =>
        {
            b.Field("root",      fullName);
            b.Field("depth",     depth);
            b.Field("nodeCount", nodes.Count);
            b.Field("edgeCount", edges.Count);
            b.Field("truncated", edges.Count >= limit);

            b.Section("nodes", nb =>
            {
                foreach (var kv in nodes)
                {
                    nb.Field("- fullName", kv.Key);
                    nb.Field("  kind",     kv.Value);
                }
            });

            b.Section("edges", eb =>
            {
                foreach (var (from, to, type) in edges)
                {
                    eb.Field("- from", from);
                    eb.Field("  to",   to);
                    eb.Field("  type", type);
                }
            });
        });
        return Task.FromResult(yaml);
    }

    private void Traverse(
        long toId, string toFull, string toKind,
        int depth, string[]? types,
        Dictionary<string, string> nodes,
        List<(string, string, string)> edges,
        HashSet<long> visited,
        int limit)
    {
        if (visited.Contains(toId) || edges.Count >= limit) return;
        visited.Add(toId);
        nodes.TryAdd(toFull, toKind);

        var rels = _db.GetRelationsTo(toId, types, 200);
        foreach (var r in rels)
        {
            if (edges.Count >= limit) break;
            nodes.TryAdd(r.FromFullName, r.FromKind);
            // Edge direction: from → to (meaning "FromObject depends on ToObject")
            edges.Add((r.FromFullName, toFull, r.Type));

            if (depth > 1)
                Traverse(r.FromObjectId, r.FromFullName, r.FromKind, depth - 1, types, nodes, edges, visited, limit);
        }
    }
}

