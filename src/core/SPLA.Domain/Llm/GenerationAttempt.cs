namespace SPLA.Domain.Llm;

/// <summary>Why a generation was abandoned. The two members differ in where the attempt died: one
/// produced text and was cut off for what that text became, the other never began.</summary>
public enum AttemptOutcome
{
    /// <summary>The output fell into a repetition loop and the guard cancelled the read.</summary>
    Repetition,

    /// <summary>The provider refused with a rate limit and the turn is waiting to ask again. Unlike
    /// <see cref="Repetition"/> this attempt generated nothing, so it carries no partial text — only
    /// how long it took to be refused, and what the wait will be.</summary>
    RateLimited
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

    /// <summary>How long the pipeline will pause before trying again, or null when nothing follows —
    /// the attempts were spent, or this outcome does not wait at all.
    /// <para>
    /// Structured rather than left inside <see cref="Note"/> because a surface that speaks the user's
    /// language has to compose its own sentence, and digging a number back out of an English one is
    /// how a display starts depending on the wording of a log line.
    /// </para></summary>
    public TimeSpan? Wait { get; init; }

    /// <summary>Whether <see cref="Wait"/> is the provider's own figure rather than our schedule.
    /// Carried separately because the two mean different things to a reader: one is a fact to sit out,
    /// the other a guess we are free to abandon — and the pipeline treats them differently too, since
    /// only the guess answers to the configured ceilings.</summary>
    public bool WaitStated { get; init; }
}
