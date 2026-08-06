namespace SPLA.Domain.Models;

public class ToolDefinition
{
    public string Type { get; set; } = "function";
    public ToolFunctionDefinition Function { get; set; } = new();
}

public class ToolFunctionDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object? Parameters { get; set; }

    /// <summary>
    /// Everything about this tool that does not fit one line: argument formats, defaults, limits,
    /// worked examples. Kept apart from <see cref="Description"/> because it is only disclosed when
    /// the owning tool set is, and folded into the description at that moment (<c>McpHost</c>).
    ///
    /// <para>This replaced the former help tool. Documentation is not something the model fetches by
    /// making a call — it arrives with the tool or not at all, so there is no lookup to decide on, no
    /// turn spent on it, and no text landing loose in the middle of the conversation.</para>
    /// </summary>
    public string? Details { get; set; }
    
    // Permission Metadata
    public ToolScope Scope { get; set; } = ToolScope.Project;
    public ToolEffect Effect { get; set; } = ToolEffect.Read;
    public ToolRisk Risk { get; set; } = ToolRisk.Low;

    /// <summary>
    /// True when the call only means something inside the conversation of the head that issued it:
    /// it sets a mark, rewinds the history, asks this chat's user, or changes what the next turn
    /// discloses. Such a tool cannot be served to a foreign head — not as a matter of policy, but
    /// because outside its own conversation the call has no referent.
    ///
    /// <para>Deliberately a fourth axis rather than another <see cref="ToolScope"/> value: scope is
    /// load-bearing for permissions (it maps to the project's read/write/shell/internet overrides),
    /// and this asks a different question. It is a property of the tool, never a decision about who
    /// may call it — that decision belongs to whatever profile serves the caller.</para>
    /// </summary>
    public bool ConversationBound { get; set; }

    /// <summary>
    /// When true the serializer adds "strict": true to the function payload (OpenAI strict
    /// function calling). Only set this for tools where every parameter is either listed in
    /// <c>required</c> or declared with a nullable type — the provider enforces this contract.
    /// </summary>
    public bool StrictSchema { get; set; }
}

