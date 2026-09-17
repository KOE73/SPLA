namespace SPLA.Domain.Llm;

/// <summary>One step of the pipeline; call <c>next</c> to continue, or don't to refuse the turn.</summary>
public delegate Task<LlmTurnResult> LlmTurnDelegate(LlmTurnContext ctx, CancellationToken ct);

/// <summary>
/// Where a middleware sits. The pipeline is ordered by this, not by registration order, so the
/// constraints that matter are expressed in the type rather than trusted to whoever writes the list.
/// </summary>
public enum LlmPipelineStage
{
    /// <summary>Outermost. Tracing and connection resolution — a refusal further in must still be traced.</summary>
    Trace = 0,

    /// <summary>May the caller do this at all: authorization, privacy, quota. Open to plugins.</summary>
    Policy = 100,

    /// <summary>Shaping what is sent: capability checks, image downscaling, attachment limits. Open to plugins.</summary>
    Content = 200,

    /// <summary>
    /// Repeating a failed network attempt. Its own stage, and deliberately <b>outside</b>
    /// <see cref="Accounting"/>: every attempt costs money — a provider may bill the prefill of a
    /// call that then dropped — so each one has to produce its own ledger row. Were it inside, a
    /// turn that retried three times would be recorded as one. Host-owned, sealed to plugins.
    /// </summary>
    Retry = 300,

    /// <summary>
    /// Judging what came back, with the right to reject an attempt and ask for another. The mirror of
    /// <see cref="Content"/>: that stage shapes what is sent, this one weighs what was produced — a
    /// generation that degenerated into a repetition loop, an answer truncated by the token ceiling,
    /// a model that wrote a tool call as prose instead of calling it.
    /// <para>
    /// Placed <b>outside</b> <see cref="Accounting"/> for the same reason <see cref="Retry"/> is: a
    /// regenerated answer is a second call to the model and must produce its own ledger row, or two
    /// paid attempts would be recorded as one. Placed <b>inside</b> <see cref="Retry"/> so that the
    /// network retry never sees a cancellation raised here and cannot mistake a rejected answer for a
    /// dropped socket. Host-owned, sealed to plugins: a guard a plugin could unhook is not a guard.
    /// </para>
    /// </summary>
    Output = 350,

    /// <summary>Recording what happened, once per network attempt. Host-owned, sealed to plugins.</summary>
    Accounting = 400,

    /// <summary>
    /// Holding requests apart so a provider's rate limit is not tripped in the first place — the
    /// mirror of <see cref="Retry"/>, which only wakes up once it has been.
    /// <para>
    /// Nearly innermost, and that placement is the whole design. A rate limit counts <i>requests</i>,
    /// so every request has to pass the gate — including the ones born inside a loop further out: a
    /// retry after a refusal, a regeneration after <see cref="Output"/> rejected an answer. Placed
    /// beside <see cref="Retry"/>, as first sketched, it would have paced only the first attempt of a
    /// turn and let precisely the bursts it exists to prevent straight through.
    /// </para>
    /// <para>
    /// Its state is the one thing here that outlives a turn, because the budget it protects belongs to
    /// the credential and is shared by every chat holding it. Host-owned, sealed to plugins.
    /// </para>
    /// </summary>
    Pacing = 450,

    /// <summary>Credential materialization — innermost, next to the wire. Host-owned, sealed to plugins.</summary>
    Transport = 500
}

/// <summary>
/// A cross-cutting concern wrapped around the model call.
/// <para>
/// Middleware speaks the canonical domain model and knows policy; it never knows a wire format.
/// Providers know the wire format and never know policy. That line is what keeps a new provider from
/// touching policy code and a new policy from touching providers.
/// </para>
/// </summary>
public interface ILlmMiddleware
{
    LlmPipelineStage Stage { get; }

    Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, LlmTurnDelegate next, CancellationToken ct);
}
