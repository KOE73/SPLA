namespace SPLA.MCP.Core.Composition;

/// <summary>
/// Where an assembled context item is delivered. Both values are real destinations today, and they
/// are not interchangeable: the system prompt is one message rebuilt on every iteration of the agent
/// loop, while a turn message is its own message inserted after it. The working-memory snapshot is
/// deliberately the second kind — it is worded as data rather than instruction, and keeping it out
/// of the prompt is what stops weak models from "maintaining" it.
/// </summary>
public enum ContextPlacement
{
    /// <summary>Concatenated into the single system message.</summary>
    SystemPrompt,

    /// <summary>Its own system-role message, re-rendered per turn and never persisted.</summary>
    TurnMessage
}

/// <summary>
/// One unit of context contributed to the agent's assembled surface — the generalisation of the old
/// <c>PromptSegment</c>: the origin is a string owned by the contributor instead of a fixed enum, so
/// a new source of context does not require a new enum member in the core.
///
/// <para><see cref="Body"/> is the clean content (what a human should read); <see cref="Prefix"/> and
/// <see cref="Suffix"/> are the separators and machine headers that surround it in the final text.
/// The split is not cosmetic: it is what lets anything downstream measure, group or shorten a body
/// while the framing stays intact — a single pre-rendered string could only be measured.</para>
/// </summary>
public sealed record ContextItem
{
    /// <summary>Which piece of the contributor this is: a feature id, a skill id, a plugin id, an
    /// instruction file name. Scoped by <see cref="Contributor"/>, so it need only be unique there.</summary>
    public required string Source { get; init; }

    /// <summary>Human-readable heading for the debug view and the manifest.</summary>
    public required string Title { get; init; }

    /// <summary>The content itself, without separators.</summary>
    public required string Body { get; init; }

    public string Prefix { get; init; } = string.Empty;
    public string Suffix { get; init; } = string.Empty;

    public ContextPlacement Placement { get; init; } = ContextPlacement.SystemPrompt;

    /// <summary>Id of the contributor that produced this item. Stamped by
    /// <see cref="AgentContextComposer"/> — a contributor never fills it in, which is what makes the
    /// manifest's attribution trustworthy.</summary>
    public string Contributor { get; init; } = string.Empty;

    /// <summary>Exactly what this item adds to the assembled text. Concatenating this over every
    /// system-prompt item reproduces the prompt byte-for-byte.</summary>
    public string Text => Prefix + Body + Suffix;

    /// <summary>Provider-free size estimate — see <see cref="TokenEstimate"/>.</summary>
    public int ApproxTokens => TokenEstimate.Of(Text);
}

/// <summary>
/// Rough token count for attribution only: ~4 characters per token, the same heuristic the context
/// debug view already used, defined once here so two places cannot disagree.
///
/// <para><b>Never use this to decide what to send.</b> The authoritative figures are the provider's:
/// <c>prompt_tokens</c> on the response and the model's context window resolved from the provider's
/// own catalog. This estimate exists to answer "which contributor is eating the window", where being
/// off by ten per cent changes nothing.</para>
/// </summary>
public static class TokenEstimate
{
    public static int Of(string? text) => string.IsNullOrEmpty(text) ? 0 : (text.Length + 3) / 4;
}
