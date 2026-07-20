using System.Text.Json;
using SPLA.Plugins.OneC.Graph;
using SPLA.Plugins.OneC.Indexing;
using SPLA.Plugins.OneC.Models;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Web;

/// <summary>
/// Project-scoped backend for the human-facing Vue configuration browser. Each request opens its
/// own SQLite connection so UI reads do not share a connection with model-facing tools.
/// </summary>
public sealed class OneCWebActions(string databasePath, string workspacePath)
{
    private readonly string _databasePath = databasePath;
    private readonly string _workspacePath = Path.GetFullPath(workspacePath);

    public async Task<object?> InvokeAsync(string action, JsonElement payload, CancellationToken ct = default) =>
        action switch
        {
            "overview" => WithDatabase(Overview),
            "search" => WithDatabase(database => Search(database, String(payload, "query"))),
            "object" => WithDatabase(database => Object(database, String(payload, "fullName"))),
            "graph" => WithDatabase(database => Graph(
                database,
                RequiredString(payload, "fullName"),
                String(payload, "mode"),
                Integer(payload, "depth", 2),
                Integer(payload, "limit", 250))),
            "rebuild" => await RebuildAsync(RequiredString(payload, "path"), ct),
            _ => throw new InvalidOperationException($"Unknown OneC web action: {action}")
        };

    private T WithDatabase<T>(Func<OneCIndexDatabase, T> action)
    {
        using var database = new OneCIndexDatabase(_databasePath);
        database.EnsureCreated();
        return action(database);
    }

    private static object Overview(OneCIndexDatabase database)
    {
        var objects = database.GetAllObjects();
        var objectCount = database.CountObjects();
        var byFullName = objects
            .GroupBy(item => item.FullName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var owns = database.GetRelationsByType(RelationType.Owns);
        var childNames = owns
            .Select(relation => relation.ToFullName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var childrenByParent = owns
            .GroupBy(relation => relation.FromFullName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var sections = objects
            .Where(item => !childNames.Contains(item.FullName) && IsTopLevelObject(item))
            .GroupBy(item => item.Kind)
            .OrderBy(group => SectionOrder(group.Key))
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new
            {
                kind = group.Key,
                count = group.Count(),
                objects = group
                    .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(item => BuildTreeNode(item, byFullName, childrenByParent, []))
                    .ToList()
            })
            .ToList();

        return new
        {
            objectCount,
            relationCount = database.CountRelations(),
            sectionCount = sections.Count,
            treeTruncated = objectCount > objects.Count,
            sections
        };
    }

    private static object Search(OneCIndexDatabase database, string? query) => new
    {
        results = string.IsNullOrWhiteSpace(query)
            ? []
            : database.FindObjects(query.Trim(), 80).Select(ToObjectDto).ToList()
    };

    private static object? Object(OneCIndexDatabase database, string? fullName) =>
        string.IsNullOrWhiteSpace(fullName)
            ? null
            : database.GetObjectByFullName(fullName) is { } item
                ? ToObjectDto(item)
                : null;

    private static object Graph(
        OneCIndexDatabase database,
        string fullName,
        string? mode,
        int depth,
        int limit)
    {
        var graph = new OneCGraphBuilder(database).Build(
            new(fullName, ParseMode(mode), Math.Clamp(depth, 1, 8), Math.Clamp(limit, 1, 1_000)));

        return new
        {
            summary = new
            {
                rootFullName = graph.Summary.RootFullName,
                mode = graph.Summary.Mode.ToString(),
                depth = graph.Summary.Depth,
                limit = graph.Summary.Limit,
                nodeCount = graph.Summary.NodeCount,
                edgeCount = graph.Summary.EdgeCount,
                truncated = graph.Summary.Truncated,
                relationTypeCounts = graph.Summary.RelationTypeCounts
            },
            nodes = graph.Nodes.Select(node => new
            {
                id = node.Id,
                label = node.Label,
                kind = node.Kind,
                isCenter = node.IsCenter
            }),
            edges = graph.Edges.Select(edge => new
            {
                id = edge.Id,
                source = edge.SourceId,
                target = edge.TargetId,
                type = edge.RelationType
            })
        };
    }

    private async Task<object> RebuildAsync(string path, CancellationToken ct)
    {
        var configurationPath = ResolveWorkspacePath(path);
        if (!Directory.Exists(configurationPath))
            throw new DirectoryNotFoundException($"Configuration directory not found: {configurationPath}");

        return await Task.Run<object>(() =>
        {
            using var database = new OneCIndexDatabase(_databasePath);
            var report = new OneCIndexer(database).Index(configurationPath);
            return new
            {
                configurationPath,
                objectsAdded = report.ObjectsAdded,
                objectsUpdated = report.ObjectsUpdated,
                relationsAdded = report.RelationsAdded,
                filesSkipped = report.FilesSkipped,
                filesWithErrors = report.FilesWithErrors,
                elapsedSeconds = Math.Round(report.Elapsed.TotalSeconds, 2),
                errors = report.Errors.Take(20).ToList()
            };
        }, ct);
    }

    private string ResolveWorkspacePath(string path)
    {
        var candidate = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(_workspacePath, path));
        var relative = Path.GetRelativePath(_workspacePath, candidate);
        if (Path.IsPathRooted(relative)
            || relative.Equals("..", StringComparison.Ordinal)
            || relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The configuration dump must be inside the project workspace.");
        }

        return candidate;
    }

    private static object BuildTreeNode(
        OneCObject item,
        IReadOnlyDictionary<string, OneCObject> byFullName,
        IReadOnlyDictionary<string, List<RelationRow>> childrenByParent,
        HashSet<string> ancestors)
    {
        var branch = new HashSet<string>(ancestors, StringComparer.OrdinalIgnoreCase);
        if (!branch.Add(item.FullName))
            return new
            {
                name = item.Name,
                kind = item.Kind,
                fullName = item.FullName,
                children = Array.Empty<object>()
            };

        var children = childrenByParent.TryGetValue(item.FullName, out var relations)
            ? relations
                .Where(relation => byFullName.ContainsKey(relation.ToFullName))
                .Select(relation => BuildTreeNode(byFullName[relation.ToFullName], byFullName, childrenByParent, branch))
                .ToList()
            : [];

        return new
        {
            name = item.Name,
            kind = item.Kind,
            fullName = item.FullName,
            children
        };
    }

    private static object ToObjectDto(OneCObject item) => new
    {
        name = item.Name,
        kind = item.Kind,
        fullName = item.FullName,
        path = item.Path ?? string.Empty,
        summary = item.Summary ?? string.Empty
    };

    private static OneCGraphMode ParseMode(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "references" => OneCGraphMode.References,
        "dataflow" or "data_flow" => OneCGraphMode.DataFlow,
        _ => OneCGraphMode.Dependencies
    };

    private static bool IsTopLevelObject(OneCObject item) => item.FullName.Count(character => character == '.') == 1;

    private static int SectionOrder(string kind) => kind switch
    {
        "Catalog" => 10,
        "Document" => 20,
        "CommonModule" => 30,
        "AccumulationRegister" => 40,
        "InformationRegister" => 50,
        "AccountingRegister" => 60,
        "Report" => 70,
        "Processing" => 80,
        "BusinessProcess" => 90,
        "Task" => 100,
        _ => 500
    };

    private static string RequiredString(JsonElement payload, string name) =>
        String(payload, name) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"OneC web action requires '{name}'.");

    private static string? String(JsonElement payload, string name) =>
        payload.ValueKind == JsonValueKind.Object
        && payload.TryGetProperty(name, out var value)
        && value.ValueKind == JsonValueKind.String
            ? value.GetString()?.Trim()
            : null;

    private static int Integer(JsonElement payload, string name, int fallback) =>
        payload.ValueKind == JsonValueKind.Object
        && payload.TryGetProperty(name, out var value)
        && value.TryGetInt32(out var result)
            ? result
            : fallback;
}
