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

    /// <summary>Recording what happened, once per network attempt. Host-owned, sealed to plugins.</summary>
    Accounting = 400,

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
