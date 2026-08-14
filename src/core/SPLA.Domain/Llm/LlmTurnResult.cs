using SPLA.Domain.Models;

namespace SPLA.Domain.Llm;

/// <summary>How a turn ended. Recorded as-is by the accounting stage — a failed or cancelled turn
/// still burned tokens and must not silently vanish from the tally.</summary>
public enum LlmTurnStatus
{
    /// <summary>Completed and the provider reported usage.</summary>
    Ok,

    /// <summary>Completed, but the provider reported no usage figures. Not the same as zero.</summary>
    UsageMissing,

    /// <summary>The provider or the transport failed.</summary>
    Error,

    /// <summary>Cancelled by the user or a guard. Tokens were still spent.</summary>
    Canceled,

    /// <summary>
    /// The model produced degenerate output — a repetition loop — and every attempt allowed for this
    /// turn ended the same way. Distinct from <see cref="Error"/> because nothing failed: the provider
    /// answered, the transport was fine, the tokens were spent. <see cref="LlmTurnResult.Message"/>
    /// carries the last partial generation so a caller can show what happened, but it is not an
    /// answer and must never be treated as one.
    /// </summary>
    Degenerate
}

/// <summary>
/// The outcome of one network call to one model.
/// </summary>
public sealed class LlmTurnResult
{
    /// <summary>The assembled assistant message — content, reasoning and tool calls.</summary>
    public required ChatMessage Message { get; init; }

    /// <summary>
    /// The model the provider says answered, which is not always the one that was asked for:
    /// <c>model: auto</c> resolves server-side and clouds substitute dated versions. Accounting keys
    /// off this, never off the requested name.
    /// </summary>
    public string? ModelReported { get; init; }

    /// <summary>
    /// Provider-reported counters exactly as they came off the wire (<c>prompt_tokens</c>,
    /// <c>cache_read_input_tokens</c>, …). Kept raw and untranslated so a counter this build has
    /// never heard of is still recorded rather than silently dropped.
    /// </summary>
    public IReadOnlyDictionary<string, long> RawUsage { get; init; }
        = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Observations the provider volunteered alongside the answer — rate-limit budget, reset times,
    /// anything else it puts in headers. Declared, not acted upon: the client knows the wire format
    /// and reports what it saw; a middleware knows the policy and decides what that means.
    /// <para>
    /// They ride here rather than in <see cref="RawUsage"/> because they are not usage: usage is per
    /// call and billable, these describe the <i>key's</i> current standing and are free to collect —
    /// the provider already sent them.
    /// </para>
    /// </summary>
    public IReadOnlyList<ProviderFact> Signals { get; init; } = [];

    public LlmTurnStatus Status { get; init; } = LlmTurnStatus.Ok;

    /// <summary>Wall-clock time of the call, stamped by the pipeline.</summary>
    public TimeSpan Duration { get; init; }
}
