namespace SPLA.MCP.Core.Composition;

/// <summary>
/// One contributor's typed answer. Today it carries context and nothing else, because context is the
/// only kind of contribution that currently has a composition path.
///
/// <para>The other kinds from the design — tools, tool middleware, policies — are deliberately
/// <b>not</b> declared yet. A tool contribution is only meaningful once activation levels exist to
/// gate it (the ToolSet work), and a field declared before it has a producer and a consumer is a
/// guess about a shape nobody has had to satisfy. When tools join, they join as their own type
/// (<c>ToolRegistration</c>) alongside <see cref="Context"/>, not as more context: the whole point
/// of one mechanism with several types is that "adds text" and "executes code" stay distinguishable
/// where trust and permissions are decided.</para>
/// </summary>
public sealed record AgentContribution
{
    /// <summary>A contributor with nothing to add this time.</summary>
    public static readonly AgentContribution None = new();

    public IReadOnlyList<ContextItem> Context { get; init; } = [];

    public static AgentContribution FromContext(params ContextItem[] items) =>
        items.Length == 0 ? None : new() { Context = items };

    public static AgentContribution FromContext(IEnumerable<ContextItem> items)
    {
        var list = items.ToList();
        return list.Count == 0 ? None : new() { Context = list };
    }
}
