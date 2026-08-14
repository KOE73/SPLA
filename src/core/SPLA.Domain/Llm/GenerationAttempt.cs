namespace SPLA.Domain.Llm;

/// <summary>Why a generation was abandoned. One member today — repetition is the only guard that
/// abandons a generation mid-stream — but the type exists so a network-retry reporter can add its own
/// member later without every consumer's switch needing to be rewritten from scratch.</summary>
public enum AttemptOutcome
{
    /// <summary>The output fell into a repetition loop and the guard cancelled the read.</summary>
    Repetition
}

/// <summary>
/// One abandoned generation: an attempt the pipeline started, decided was going nowhere, and cut off
/// before the provider could finish it.
/// <para>
/// <b>Deliberately no token count.</b> The provider only reports usage for a generation it finished —
/// cancelling the read mid-stream means no usage figure ever arrives, so there is nothing honest to
/// put in a token field. <see cref="Chars"/> is what actually happened: characters the socket handed
/// back before the guard pulled the plug. Do not add a token estimate here; an estimate dressed up as
/// a count is worse than admitting the number does not exist.
/// </para>
/// </summary>
public sealed record GenerationAttempt
{
    /// <summary>1-based position of this attempt within the call — the first generation is 1, the
    /// retry (if any) is 2, and so on.</summary>
    public required int Index { get; init; }

    public required AttemptOutcome Outcome { get; init; }

    /// <summary>The abandoned answer text, as captured by the guard's observer — already
    /// length-capped, never the full generation.</summary>
    public string? Content { get; init; }

    /// <summary>The abandoned reasoning text, captured the same way as <see cref="Content"/>.</summary>
    public string? Reasoning { get; init; }

    /// <summary>Human-readable cause, e.g. "repetition in reasoning: period 137 chars, x24".</summary>
    public string? Note { get; init; }

    /// <summary>Total characters the abandoned generation produced across both channels before it was
    /// cut off — the only honest measure of what this attempt cost, in the absence of usage figures.</summary>
    public required int Chars { get; init; }

    public required TimeSpan Duration { get; init; }
}
