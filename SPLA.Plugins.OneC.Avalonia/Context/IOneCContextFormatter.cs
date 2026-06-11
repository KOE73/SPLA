using SPLA.Plugins.OneC.Graph;
using SPLA.Plugins.OneC.Models;

namespace SPLA.Plugins.OneC.Avalonia.Context;

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
              fullName: {obj.FullName}
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
