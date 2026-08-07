using SPLA.Domain.Models;

namespace SPLA.MCP.Core;

/// <summary>
/// What a caller other than this chat's own head may be served.
/// <para>
/// A separate question from <c>ToolSetLevel</c>, which asks how much of a set reaches the model and
/// at what price in context. This asks <b>whose call it is</b>. The two are orthogonal and folding
/// them together would be a mistake: a set the owner levelled off is hidden from everyone, while a
/// conversation-bound tool is hidden only from callers that are not the conversation.
/// </para>
/// <para>
/// Deliberately not a configuration system. There is one rule today, it follows from a decision
/// already taken, and a foreign caller has no way to reach anything else — narrowing by name exists
/// because a skill may want a smaller table, not because anything needs configuring yet.
/// </para>
/// </summary>
public sealed record ToolExposure
{
    /// <summary>Everything the project permits, minus what is bound to a conversation.</summary>
    public static readonly ToolExposure Default = new();

    /// <summary>
    /// When set, only these tools are served — on top of, never instead of, the rules below. Used to
    /// hand a caller the smaller table a particular skill needs rather than the whole workshop.
    /// </summary>
    public IReadOnlySet<string>? OnlyTools { get; init; }

    /// <summary>
    /// Whether this tool may be served.
    /// <para>
    /// The refusal for a conversation-bound tool is not policy and cannot be configured away: a mark
    /// set on somebody else's conversation, or a rollback of it, has no referent. There is nothing to
    /// permit.
    /// </para>
    /// </summary>
    public bool Allows(ToolFunctionDefinition tool)
    {
        if (tool.ConversationBound) return false;
        if (OnlyTools is not null && !OnlyTools.Contains(tool.Name)) return false;
        return true;
    }
}
