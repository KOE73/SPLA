using System.Text.Json;
using SPLA.Plugins.OneC.Graph;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC;

/// <summary>
/// Builds the Cytoscape.js-compatible JSON payload for the graph view.
///
/// Output format:
/// {
///   "nodes": [ { "id": "...", "label": "...", "kind": "..." } ],
///   "edges": [ { "source": "...", "target": "...", "type": "..." } ]
/// }
///
/// This class is pure data — no Avalonia / UI dependency.
/// The resulting JSON is injected into the WebView via window.loadGraph(json).
/// </summary>
public class GraphDataBuilder
{
    private readonly OneCIndexDatabase _db;

    public GraphDataBuilder(OneCIndexDatabase db) => _db = db;

    // ── View modes ────────────────────────────────────────────────────────────

    public enum ViewMode { Dependencies, References, DataFlow }

    // ── Main entry point ──────────────────────────────────────────────────────

    /// <summary>
    /// Build graph JSON centred on <paramref name="fullName"/>.
    /// </summary>
    public string Build(string fullName, ViewMode mode, int depth = 1)
    {
        var graphMode = mode switch
        {
            ViewMode.Dependencies => OneCGraphMode.Dependencies,
            ViewMode.References => OneCGraphMode.References,
            ViewMode.DataFlow => OneCGraphMode.DataFlow,
            _ => OneCGraphMode.Dependencies
        };

        var graph = new OneCGraphBuilder(_db).Build(new(fullName, graphMode, depth, 150));
        return Serialize(graph);
    }

    // ── Serialization ─────────────────────────────────────────────────────────

    private static string Serialize(OneCGraph graph) =>
        JsonSerializer.Serialize(new
        {
            nodes = graph.Nodes.Select(n => new { id = n.Id, label = n.Label, kind = n.Kind, isCenter = n.IsCenter }),
            edges = graph.Edges.Select(e => new { id = e.Id, source = e.SourceId, target = e.TargetId, type = e.RelationType }),
        });
}

