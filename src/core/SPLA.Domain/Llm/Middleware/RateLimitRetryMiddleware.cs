using System.Diagnostics;
using System.Globalization;
using SPLA.Domain.Interfaces;

namespace SPLA.Domain.Llm.Middleware;

/// <summary>
/// Waits and asks again when the provider refuses with a rate limit.
/// <para>
/// The stage this occupies was declared with the pipeline itself and stood empty until now, so the
/// only thing that ever retried a 429 was the person reading "wait a moment and retry".
/// </para>
/// <para>
/// <b>What counts as a reason to try again.</b> Only <see cref="LlmErrorKind.RateLimited"/>. The
/// judgement of which wire response deserves that name belongs to the provider's profile, not here —
/// this layer never sees HTTP. That split is what lets one provider learn that its 429 sometimes means
/// an empty balance without a single line changing in this file.
/// </para>
/// <para>
/// <b>Why a cancelled turn cannot be mistaken for a failed one.</b> Nothing here catches
/// <see cref="OperationCanceledException"/>, so Stop can never be read as a transport failure and
/// answered with another attempt. The neighbouring guard at <see cref="LlmPipelineStage.Output"/> needs
/// <see cref="TurnAttempt"/> to tell its own abort from the user's; this layer aborts nothing, so it
/// needs no such discrimination — a property worth keeping if the set of retryable reasons ever grows.
/// </para>
/// </summary>
public sealed class RateLimitRetryMiddleware : ILlmMiddleware
{
    public LlmPipelineStage Stage => LlmPipelineStage.Retry;

    public async Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, LlmTurnDelegate next, CancellationToken ct)
    {
        var policy = ctx.Settings.Retry;
        var attempts = Math.Max(1, policy.Attempts);

        var callerDelta = ctx.OnDelta;
        var callerReasoning = ctx.OnReasoning;

        // Whether a single character has reached the caller's sinks. Not a statistic: see Retryable.
        var emitted = false;
        ctx.ObserveDelta(_ => emitted = true);
        ctx.ObserveReasoning(_ => emitted = true);

        try
        {
            var spentWaiting = TimeSpan.Zero;

            for (var attemptNo = 1; ; attemptNo++)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    return await next(ctx, ct);
                }
                catch (LlmRequestException ex) when (ex.Kind == LlmErrorKind.RateLimited)
                {
                    stopwatch.Stop();
                    if (attemptNo >= attempts || emitted) throw;

                    // A stated Retry-After is not a guess and is obeyed as given; a computed one is,
                    // and answers to every ceiling. See ADR_20260908 §5.
                    var stated = StatedRetryAfter(ex);
                    var delay = stated ?? ComputeDelay(policy, attemptNo);

                    if (stated == null && spentWaiting + delay > TimeSpan.FromSeconds(policy.Total)) throw;

                    Report(ctx, attemptNo, ex, delay, stated != null, stopwatch.Elapsed);

                    // The turn's own token: Stop ends the wait at once instead of after it.
                    await Task.Delay(delay, ct);
                    spentWaiting += delay;
                }
            }
        }
        finally
        {
            // Nothing this middleware did to the context outlives it. The context is the caller's.
            ctx.OnDelta = callerDelta;
            ctx.OnReasoning = callerReasoning;
        }
    }

    /// <summary>
    /// The pause before attempt <paramref name="attemptNo"/> + 1, grown geometrically and capped.
    /// <para>
    /// Its own method for a reason that is not yet visible in the code: the day several chats share one
    /// key, attempts that fail together will wake together and strike together, and the cure is a small
    /// random spread added here. Written as one place so that day costs one edit and no caller.
    /// </para>
    /// </summary>
    private static TimeSpan ComputeDelay(Settings.SplaRetrySection policy, int attemptNo)
    {
        var seconds = policy.MinDelay * Math.Pow(policy.Step, attemptNo - 1);
        return TimeSpan.FromSeconds(Math.Clamp(seconds, 0, policy.MaxDelay));
    }

    /// <summary>
    /// The delay the provider itself named, or null when it named none.
    /// <para>
    /// Read from the facts the failure already carries rather than from a header, because middleware
    /// speaks the domain model and never the wire. Only the delta form is understood: a
    /// <c>Retry-After</c> given as an HTTP date is legal and unseen here, and guessing at a format
    /// nobody has observed would be the same mistake as guessing at a refusal's wording.
    /// </para>
    /// </summary>
    private static TimeSpan? StatedRetryAfter(LlmRequestException ex)
    {
        var fact = ex.Signals.FirstOrDefault(f => f.Key == "ratelimit.retry_after");
        if (fact == null) return null;

        return double.TryParse(fact.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
               && seconds >= 0
            ? TimeSpan.FromSeconds(seconds)
            : null;
    }

    /// <summary>
    /// Tells the turn's sink that an attempt was refused and how long the next one will wait. Without
    /// it the turn simply goes quiet, which reads as a hang — and a wait nobody can see is a wait
    /// nobody knows to stop. Best-effort, like every other bystander on this path.
    /// </summary>
    private static void Report(
        LlmTurnContext ctx, int attemptNo, LlmRequestException ex, TimeSpan delay, bool stated, TimeSpan elapsed)
    {
        // The sentence is English, so the number in it is formatted the English way — the ambient
        // culture would otherwise put a Russian decimal comma inside an English phrase.
        var seconds = delay.TotalSeconds.ToString("0.#", CultureInfo.InvariantCulture);

        try
        {
            ctx.OnAttempt?.Invoke(new GenerationAttempt
            {
                Index = attemptNo,
                Outcome = AttemptOutcome.RateLimited,
                Note = stated
                    ? $"rate-limited; the provider asked for {seconds}s"
                    : $"rate-limited; retrying in {seconds}s",
                Chars = 0,
                Duration = elapsed,
                Wait = delay,
                WaitStated = stated
            });
        }
        catch { /* a bystander must not break the turn */ }
    }
}
