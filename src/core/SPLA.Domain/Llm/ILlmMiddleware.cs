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

    /// <summary>Recording what happened. Host-owned, sealed to plugins.</summary>
    Accounting = 300,

    /// <summary>Retries and credential materialization — innermost, next to the wire. Host-owned, sealed to plugins.</summary>
    Transport = 400
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
