using System.Text.Json;
using SPLA.Plugins.OneC.Context;
using SPLA.Plugins.OneC.Graph;
using SPLA.Plugins.OneC.Indexing;
using SPLA.Plugins.OneC.Models;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Web;

/// <summary>
/// Server side of the "1C Configuration Browser" web panel. Every method returns a plain,
/// JSON-serializable DTO that the Vue panel consumes through the <c>plugin.action</c> channel
/// (see <see cref="OneCPlugin.InvokeActionAsync"/>).
///
/// This replaces the logic that used to live in the Avalonia <c>OneCOverviewPanel</c> code-behind:
/// the tree, the overview counters, search, the object card, the three graph modes, and the index
/// rebuild all move here so the presentation layer (now HTML/JS instead of XAML) stays thin.
/// </summary>
public sealed class OneCBrowserActions
{
    private readonly OneCIndexDatabase _db;

    public OneCBrowserActions(OneCIndexDatabase db) => _db = db;

    // ── overview: counters + the owns-hierarchy tree ──────────────────────────

    public object Overview()
    {
        var objects = _db.GetAllObjects();
        var byFullName = objects
            .GroupBy(o => o.FullName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var owns = _db.GetRelationsByType(RelationType.Owns);
        var kindCounts = objects.GroupBy(o => o.Kind).ToDictionary(g => g.Key, g => g.Count());

        return new
        {
            objectCount = objects.Count,
            relationCount = _db.CountRelations(),
            sectionCount = kindCounts.Count,
            tree = BuildTree(objects, byFullName, owns)
        };
    }

    // ── search: substring match over full name + short name ───────────────────

    public object Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new { results = Array.Empty<object>() };

        var results = _db.GetAllObjects()
            .Where(o => o.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || o.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(80)
            .Select(ToDto)
            .ToList();

        return new { results };
    }

    // ── object card ───────────────────────────────────────────────────────────

    public object? Object(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return null;
        var obj = _db.GetAllObjects().FirstOrDefault(
            o => string.Equals(o.FullName, fullName, StringComparison.OrdinalIgnoreCase));
        return obj is null ? null : ToDto(obj);
    }

    // ── graph: nodes + edges for the Cytoscape panel ──────────────────────────

    public object Graph(string? fullName, string? mode, int depth, int limit)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new InvalidOperationException("graph requires full_name");

        var graphMode = ParseMode(mode);
        var graph = new OneCGraphBuilder(_db).Build(new(fullName, graphMode, depth, limit));

        return new
        {
            root = graph.Summary.RootFullName,
            mode = graph.Summary.Mode.ToString(),
            depth = graph.Summary.Depth,
            limit = graph.Summary.Limit,
            nodeCount = graph.Summary.NodeCount,
            edgeCount = graph.Summary.EdgeCount,
            truncated = graph.Summary.Truncated,
            // The web panel feeds this straight into window.loadGraph(...) / cytoscape.
            elements = JsonDocument.Parse(CytoscapeGraphAdapter.ToJson(graph)).RootElement.Clone()
        };
    }

    // ── context export: Copy / Insert text for the chat prompt ────────────────

    public object Format(string? formatterId, string? fullName, string? mode, int depth, int limit)
    {
        var formatter = OneCContextFormatters.All.FirstOrDefault(f => f.Id == formatterId)
                        ?? OneCContextFormatters.All[0];

        OneCObject? obj = string.IsNullOrWhiteSpace(fullName)
            ? null
            : _db.GetAllObjects().FirstOrDefault(
                o => string.Equals(o.FullName, fullName, StringComparison.OrdinalIgnoreCase));

        OneCGraph? graph = null;
        if (obj is not null && !string.IsNullOrWhiteSpace(mode))
            graph = new OneCGraphBuilder(_db).Build(new(obj.FullName, ParseMode(mode), depth, limit));

        return new { text = formatter.Format(new OneCSelection(obj, graph)) };
    }

    public object Formatters() =>
        new { formatters = OneCContextFormatters.All.Select(f => new { id = f.Id, name = f.DisplayName }) };

    // ── rebuild: (re)index a 1C configuration dump directory ──────────────────

    public async Task<object> RebuildAsync(string? path, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("rebuild requires a configuration dump path");

        var absPath = Path.GetFullPath(path);
        if (!Directory.Exists(absPath))
            throw new InvalidOperationException($"directory not found: {absPath}");

        var indexer = new OneCIndexer(_db);
        var report = await Task.Run(() => indexer.Index(absPath), ct);

        return new
        {
            configPath = absPath,
            objectsAdded = report.ObjectsAdded,
            objectsUpdated = report.ObjectsUpdated,
            relationsAdded = report.RelationsAdded,
            filesSkipped = report.FilesSkipped,
            filesWithErrors = report.FilesWithErrors,
            elapsedSeconds = Math.Round(report.Elapsed.TotalSeconds, 2),
            errors = report.Errors.Take(20).ToList()
        };
    }

    // ── tree building (ported from OneCOverviewPanel.BuildTree) ────────────────

    private static List<object> BuildTree(
        List<OneCObject> objects,
        Dictionary<string, OneCObject> byFullName,
        List<RelationRow> owns)
    {
        var childNames = owns.Select(r => r.ToFullName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var childrenByParent = owns
            .GroupBy(r => r.FromFullName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        return objects
            .Where(o => !childNames.Contains(o.FullName) && IsTopLevelObject(o))
            .GroupBy(o => o.Kind)
            .OrderBy(g => SectionOrder(g.Key))
            .ThenBy(g => SectionTitle(g.Key))
            .Select(group => (object)new
            {
                section = SectionTitle(group.Key),
                kind = group.Key,
                count = group.Count(),
                objects = group
                    .OrderBy(o => o.Name)
                    .Select(o => ObjectNode(o, byFullName, childrenByParent))
                    .ToList()
            })
            .ToList();
    }

    private static object ObjectNode(
        OneCObject obj,
        Dictionary<string, OneCObject> byFullName,
        Dictionary<string, List<RelationRow>> childrenByParent) =>
        new
        {
            name = obj.Name,
            kind = obj.Kind,
            fullName = obj.FullName,
            children = childrenByParent.TryGetValue(obj.FullName, out var children)
                ? children
                    .Where(r => byFullName.ContainsKey(r.ToFullName))
                    .Select(r => ObjectNode(byFullName[r.ToFullName], byFullName, childrenByParent))
                    .ToList()
                : []
        };

    private static object ToDto(OneCObject o) => new
    {
        name = o.Name,
        kind = o.Kind,
        fullName = o.FullName,
        path = o.Path ?? "",
        summary = o.Summary ?? ""
    };

    private static OneCGraphMode ParseMode(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "references" => OneCGraphMode.References,
        "dataflow" => OneCGraphMode.DataFlow,
        "data_flow" => OneCGraphMode.DataFlow,
        _ => OneCGraphMode.Dependencies
    };

    private static bool IsTopLevelObject(OneCObject obj) => obj.FullName.Count(c => c == '.') == 1;

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

    private static string SectionTitle(string kind) => kind switch
    {
        "Catalog" => "Справочники",
        "Document" => "Документы",
        "CommonModule" => "Общие модули",
        "AccumulationRegister" => "Регистры накопления",
        "InformationRegister" => "Регистры сведений",
        "AccountingRegister" => "Регистры бухгалтерии",
        "Report" => "Отчеты",
        "Processing" => "Обработки",
        "BusinessProcess" => "Бизнес-процессы",
        "Task" => "Задачи",
        _ => kind
    };
}
