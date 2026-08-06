namespace SPLA.Domain.Models;

/// <summary>
/// How a tool call ended, kept apart from what it produced.
/// <para>
/// The two were one string until now, and the conflation was not free: a tool returning
/// <c>"error: 'name' is required"</c> went through the success path, so error metrics counted only
/// thrown exceptions and quietly reported refusals and bad arguments as successes. Sniffing the text
/// could not have fixed it either — tools write <c>"error: "</c>, the host writes <c>"Error: "</c>,
/// and a search result may legitimately begin with the word Error.
/// </para>
/// <para>
/// Finer than the single <c>isError</c> flag that the wire formats carry, on purpose: refusal and
/// failure are the same thing to a model but not to an audit log, and the distinction is cheap to
/// keep here and lossy to reconstruct later. The external projection folds both into that flag.
/// </para>
/// </summary>
public enum ToolOutcome
{
    /// <summary>The tool did what was asked. Says nothing about whether the answer was useful —
    /// "no matches found" is a perfectly successful search.</summary>
    Ok,

    /// <summary>The tool tried and could not: bad arguments, a broken connection, an exception.</summary>
    Failed,

    /// <summary>The call never ran. Policy, an unraised tool set, a disabled plugin, a human saying
    /// no. Separate from <see cref="Failed"/> because nothing was attempted and nothing changed.</summary>
    Refused
}

/// <summary>
/// One piece of what a tool produced. A result carries a list of these rather than one blob, which
/// is the shape both the Model Context Protocol and the Anthropic messages API converged on
/// independently — because a call can legitimately answer with prose *and* a picture *and* a pointer
/// to something too big to inline.
/// </summary>
public abstract record ToolContent;

/// <summary>Prose, or anything else that belongs in the conversation verbatim.</summary>
public sealed record ToolText(string Text) : ToolContent;

/// <summary>
/// An image the tool produced. <paramref name="Data"/> is base64, without a data-URL prefix.
/// <para>
/// A tool declares that it has a picture and stops there. How that picture reaches a particular
/// model — inline in the tool message, or as a separate turn because that provider's vision API
/// will not take it inline — is the conversation layer's problem, not the browser plugin's.
/// </para>
/// </summary>
public sealed record ToolImage(string Data, string MimeType) : ToolContent;

/// <summary>
/// A pointer to bulk data the tool deliberately kept out of the conversation — a blob handle, a
/// file, a fetched document. The text stays small; whoever wants the body asks for it by
/// <paramref name="Uri"/>.
/// <para>
/// Tools already worked this way, they just had to say it in prose ("stored as {handle}"). Saying
/// it in the type is what lets the pipeline, the UI and an external adapter each do something
/// sensible with it instead of parsing a sentence.
/// </para>
/// </summary>
public sealed record ToolResource(string Uri, string? MimeType = null, string? Description = null)
    : ToolContent;

/// <summary>
/// What a tool call produced: an outcome and the content behind it.
/// <para>
/// Construct through the factory methods rather than the initialiser — <see cref="Text"/>,
/// <see cref="Fail"/> and <see cref="Refuse"/> make the outcome an explicit act at every return
/// site. There is deliberately no implicit conversion from <see cref="string"/>: it would let the
/// hundred-odd existing <c>return "error: …"</c> statements compile into successful results, and
/// the type would look fixed while the values kept lying.
/// </para>
/// </summary>
public sealed class ToolResult
{
    public ToolOutcome Outcome { get; init; } = ToolOutcome.Ok;

    public IReadOnlyList<ToolContent> Content { get; init; } = [];

    /// <summary>
    /// Why it was refused or how it failed, for logs, metrics and audit — never a substitute for
    /// telling the model the same thing in <see cref="Content"/>. Null when the call succeeded.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>Every text block joined by blank lines: the result as a conversation message sees
    /// it. Non-text content is not represented here — it is routed by whoever handles it.</summary>
    public string TextContent =>
        string.Join("\n\n", Content.OfType<ToolText>().Select(t => t.Text));

    public bool IsError => Outcome != ToolOutcome.Ok;

    /// <summary>The ordinary answer: the tool did its job and has something to say.</summary>
    public static ToolResult Text(string text) => new() { Content = [new ToolText(text)] };

    /// <summary>Success with no output — a completed action that has nothing to report.</summary>
    public static ToolResult Empty() => new();

    /// <summary>The tool tried and could not. <paramref name="message"/> is what the model reads;
    /// <paramref name="reason"/> is the short machine-facing cause for logs and metrics.</summary>
    public static ToolResult Fail(string message, string? reason = null) => new()
    {
        Outcome = ToolOutcome.Failed,
        Content = [new ToolText(message)],
        Reason = reason ?? message
    };

    /// <summary>The call never ran. Same two audiences as <see cref="Fail"/>.</summary>
    public static ToolResult Refuse(string message, string? reason = null) => new()
    {
        Outcome = ToolOutcome.Refused,
        Content = [new ToolText(message)],
        Reason = reason ?? message
    };

    /// <summary>A successful result assembled from several pieces — text plus a picture, text plus
    /// a pointer to the bulk it left out.</summary>
    public static ToolResult From(params ToolContent[] content) => new() { Content = content };
}
