using SPLA.Plugins.OneC.Graph;
using SPLA.Plugins.OneC.Models;

namespace SPLA.Plugins.OneC.Context;

/// <summary>
/// A selection the user wants to export into the chat prompt: an object and, optionally, the graph
/// currently rendered for it. Formatters below turn that into copy/insert-ready text.
///
/// Moved verbatim from SPLA.Plugins.OneC.Avalonia — this is pure text formatting with no UI
/// dependency, so the web browser panel reuses it through the <c>format</c> plugin action.
/// </summary>
public sealed record OneCSelection(OneCObject? Object, OneCGraph? Graph = null);

public interface IOneCContextFormatter
{
    string Id { get; }
    string DisplayName { get; }
    string Format(OneCSelection selection);
}

public sealed class FullNameContextFormatter : IOneCContextFormatter
{
    public string Id => "full-name";
    public string DisplayName => "FullName";
    public string Format(OneCSelection selection) => selection.Object?.FullName ?? "";
}

public sealed class ObjectCardYamlContextFormatter : IOneCContextFormatter
{
    public string Id => "object-card-yaml";
    public string DisplayName => "Object card YAML";

    public string Format(OneCSelection selection)
    {
        if (selection.Object is not { } obj) return "";

        return $"""
            object:
              full_name: {obj.FullName}
              kind: {obj.Kind}
              path: {obj.Path ?? ""}
              summary: {obj.Summary ?? ""}
            """;
    }
}

public sealed class GraphSummaryContextFormatter : IOneCContextFormatter
{
    public string Id => "graph-summary";
    public string DisplayName => "Graph summary";

    public string Format(OneCSelection selection)
    {
        if (selection.Graph is not { } graph) return "";

        return $"""
            graph:
              root: {graph.Summary.RootFullName}
              mode: {graph.Summary.Mode}
              depth: {graph.Summary.Depth}
              nodes: {graph.Summary.NodeCount}
              edges: {graph.Summary.EdgeCount}
              truncated: {graph.Summary.Truncated}
            """;
    }
}

public sealed class SelectedRelationsContextFormatter : IOneCContextFormatter
{
    public string Id => "selected-relations";
    public string DisplayName => "Selected relations";

    public string Format(OneCSelection selection)
    {
        if (selection.Graph is not { } graph) return "";

        var lines = graph.Edges
            .Take(50)
            .Select(edge => $"- {edge.SourceId} --{edge.RelationType}--> {edge.TargetId}");

        return string.Join(Environment.NewLine, lines);
    }
}

/// <summary>The formatters offered in the panel's "Context export" dropdown, in display order.</summary>
public static class OneCContextFormatters
{
    public static IReadOnlyList<IOneCContextFormatter> All { get; } =
    [
        new FullNameContextFormatter(),
        new ObjectCardYamlContextFormatter(),
        new GraphSummaryContextFormatter(),
        new SelectedRelationsContextFormatter()
    ];
}
