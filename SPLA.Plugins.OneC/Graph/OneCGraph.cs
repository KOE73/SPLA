namespace SPLA.Plugins.OneC.Graph;

public sealed record OneCGraph(
    IReadOnlyList<OneCGraphNode> Nodes,
    IReadOnlyList<OneCGraphEdge> Edges,
    OneCGraphSummary Summary);

public sealed record OneCGraphNode(
    string Id,
    string Label,
    string Kind,
    bool IsCenter);

public sealed record OneCGraphEdge(
    string Id,
    string SourceId,
    string TargetId,
    string RelationType);

public sealed record OneCGraphSummary(
    string RootFullName,
    OneCGraphMode Mode,
    int Depth,
    int Limit,
    int NodeCount,
    int EdgeCount,
    bool Truncated,
    IReadOnlyDictionary<string, int> RelationTypeCounts);

public sealed record OneCGraphRequest(
    string RootFullName,
    OneCGraphMode Mode,
    int Depth = 1,
    int Limit = 100);

public enum OneCGraphMode
{
    Dependencies,
    References,
    DataFlow
}
