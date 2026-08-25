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
    /// True when this tool may keep running after the turn that called it ends: the caller gets a
    /// task id back immediately, and the result arrives on a later turn boundary. See
    /// <c>docs/adr/ADR_20260824-2_core_background-tool-calls.md</c>.
    /// <para>
    /// Read by nobody yet — the pipeline stage that acts on it (<c>ToolPipelineStage.Background</c>)
    /// is declared but not implemented. Declared here first, ahead of the stage, so the flag exists
    /// on every tool's metadata (default <c>false</c>) before anything is asked to set it, the same
    /// order <see cref="ConversationBound"/> was introduced in.
    /// </para>
    /// <para>
    /// <b>The two axes are exclusive by definition, not by a rule someone has to remember:</b> a
    /// <see cref="ConversationBound"/> call means something only inside the conversation of the head
    /// that issued it, and a call detached from its turn has, at the moment it would report back, no
    /// conversation left to mean anything inside — the referent a <see cref="ConversationBound"/>
    /// tool depends on is gone. A tool that sets both is declaring a call that can outlive the one
    /// place its result would make sense.
    /// </para>
    /// <para>Not serialized to every tool's schema — only a tool with this set to <c>true</c> gets
    /// the <c>background</c> parameter added, so the common case costs nothing in every request.</para>
    /// </summary>
    public bool SupportsBackground { get; set; }

    /// <summary>
    /// When true the serializer adds "strict": true to the function payload (OpenAI strict
    /// function calling). Only set this for tools where every parameter is either listed in
    /// <c>required</c> or declared with a nullable type — the provider enforces this contract.
    /// </summary>
    public bool StrictSchema { get; set; }
}

