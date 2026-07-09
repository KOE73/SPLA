using SPLA.Plugins.OneC.Models;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Graph;

public sealed class OneCGraphBuilder
{
    private readonly OneCIndexDatabase _db;

    public OneCGraphBuilder(OneCIndexDatabase db) => _db = db;

    public OneCGraph Build(OneCGraphRequest request)
    {
        var root = _db.GetObjectByFullName(request.RootFullName);
        if (root is null)
        {
            return Empty(request);
        }

        var depth = Math.Clamp(request.Depth, 1, 8);
        var limit = Math.Max(1, request.Limit);
        var nodes = new Dictionary<string, OneCGraphNode>(StringComparer.OrdinalIgnoreCase)
        {
            [root.FullName] = new(root.FullName, root.Name, root.Kind, true)
        };
        var edges = new List<OneCGraphEdge>();
        var visited = new HashSet<long>();
        var truncated = false;

        string[]? typeFilter = request.Mode == OneCGraphMode.DataFlow
            ? [RelationType.Writes, RelationType.Reads, RelationType.Queries]
            : null;

        if (request.Mode is OneCGraphMode.Dependencies or OneCGraphMode.DataFlow)
        {
            ExpandOut(root.Id, depth, typeFilter, limit, nodes, edges, visited, ref truncated);
        }
        else
        {
            ExpandIn(root.Id, depth, typeFilter, limit, nodes, edges, visited, ref truncated);
        }

        return new OneCGraph(
            nodes.Values.ToList(),
            edges,
            BuildSummary(request, depth, limit, nodes.Count, edges, truncated));
    }

    private void ExpandOut(
        long fromId,
        int depth,
        string[]? types,
        int limit,
        Dictionary<string, OneCGraphNode> nodes,
        List<OneCGraphEdge> edges,
        HashSet<long> visited,
        ref bool truncated)
    {
        if (depth == 0 || !visited.Add(fromId) || edges.Count >= limit) return;

        foreach (var relation in _db.GetRelationsFrom(fromId, types, limit))
        {
            if (edges.Count >= limit)
            {
                truncated = true;
                return;
            }

            nodes.TryAdd(relation.ToFullName, new(relation.ToFullName, ShortName(relation.ToFullName), relation.ToKind, false));
            edges.Add(new(relation.Id.ToString(), relation.FromFullName, relation.ToFullName, relation.Type));
            ExpandOut(relation.ToObjectId, depth - 1, types, limit, nodes, edges, visited, ref truncated);
        }
    }

    private void ExpandIn(
        long toId,
        int depth,
        string[]? types,
        int limit,
        Dictionary<string, OneCGraphNode> nodes,
        List<OneCGraphEdge> edges,
        HashSet<long> visited,
        ref bool truncated)
    {
        if (depth == 0 || !visited.Add(toId) || edges.Count >= limit) return;

        foreach (var relation in _db.GetRelationsTo(toId, types, limit))
        {
            if (edges.Count >= limit)
            {
                truncated = true;
                return;
            }

            nodes.TryAdd(relation.FromFullName, new(relation.FromFullName, ShortName(relation.FromFullName), relation.FromKind, false));
            edges.Add(new(relation.Id.ToString(), relation.FromFullName, relation.ToFullName, relation.Type));
            ExpandIn(relation.FromObjectId, depth - 1, types, limit, nodes, edges, visited, ref truncated);
        }
    }

    private static OneCGraph Empty(OneCGraphRequest request) =>
        new(
            [],
            [],
            new(
                request.RootFullName,
                request.Mode,
                request.Depth,
                request.Limit,
                0,
                0,
                false,
                new Dictionary<string, int>()));

    private static OneCGraphSummary BuildSummary(
        OneCGraphRequest request,
        int depth,
        int limit,
        int nodeCount,
        IEnumerable<OneCGraphEdge> edges,
        bool truncated) =>
        new(
            request.RootFullName,
            request.Mode,
            depth,
            limit,
            nodeCount,
            edges.Count(),
            truncated,
            edges.GroupBy(e => e.RelationType)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase));

    private static string ShortName(string fullName) =>
        fullName.Split('.').LastOrDefault() ?? fullName;
}
