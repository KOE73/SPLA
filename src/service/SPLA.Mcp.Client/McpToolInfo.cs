using System.Text.Json.Nodes;

namespace SPLA.Mcp.Client;

/// <summary>
/// One tool as the server described it. Kept in the protocol's own vocabulary — this is a record of
/// what a stranger said, not a decision about it.
///
/// <para>The translation into an <c>IMcpTool</c> with our Scope/Effect/Risk happens above (step 3),
/// and it is where <see cref="DestructiveHint"/> is allowed to matter and <see cref="ReadOnlyHint"/>
/// is not: an annotation can only ever tighten the verdict, because lying in one of these is
/// profitable in exactly one direction. Keeping both fields here and applying that rule elsewhere is
/// deliberate — this type must stay an honest transcript.</para>
/// </summary>
public sealed record McpToolInfo
{
    public required string Name { get; init; }

    /// <summary>A human-facing name the server may supply alongside the machine one.</summary>
    public string? Title { get; init; }

    public string Description { get; init; } = string.Empty;

    /// <summary>The JSON Schema for the tool's arguments, verbatim. Passed on to the model as it
    /// stands: rewriting a stranger's schema is how a call starts failing for reasons neither side
    /// can see.</summary>
    public JsonNode? InputSchema { get; init; }

    /// <summary>Server's claim that the tool only reads. <b>Never acted on.</b> See the type remarks.</summary>
    public bool? ReadOnlyHint { get; init; }

    /// <summary>Server's claim that the tool destroys something. Acted on, because it can only make
    /// the verdict stricter.</summary>
    public bool? DestructiveHint { get; init; }

    public static McpToolInfo FromJson(JsonNode node)
    {
        var annotations = node["annotations"];
        return new McpToolInfo
        {
            Name = node["name"]?.GetValue<string>() ?? string.Empty,
            Title = node["title"]?.GetValue<string>(),
            Description = node["description"]?.GetValue<string>() ?? string.Empty,
            InputSchema = node["inputSchema"]?.DeepClone(),
            ReadOnlyHint = AsBool(annotations?["readOnlyHint"]),
            DestructiveHint = AsBool(annotations?["destructiveHint"])
        };
    }

    private static bool? AsBool(JsonNode? node)
    {
        try { return node?.GetValue<bool>(); }
        catch { return null; }   // a server is free to put something else there; that is not our problem
    }
}

/// <summary>One progress tick from a running remote call.</summary>
/// <param name="Progress">How far, in the server's own units.</param>
/// <param name="Total">Out of how much, when the server knows. Null means it does not.</param>
/// <param name="Message">What it is doing, when the server says.</param>
public readonly record struct McpProgress(double Progress, double? Total, string? Message);
