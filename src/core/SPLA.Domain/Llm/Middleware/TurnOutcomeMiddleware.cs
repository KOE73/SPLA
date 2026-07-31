using System.Diagnostics;

namespace SPLA.Domain.Llm.Middleware;

/// <summary>
/// Stamps wall-clock duration on the result and normalizes the outcome status.
/// <para>
/// Small on purpose: this is the accounting stage's foothold. The ledger (one row per network
/// attempt, written even for a cancelled or failed turn) lands here later; what it establishes now is
/// the discipline that makes that possible — the outcome is decided in a <c>finally</c>, so a turn
/// that threw or was cancelled still produced facts rather than vanishing.
/// </para>
/// <para>
/// "No usage reported" is <see cref="LlmTurnStatus.UsageMissing"/>, never zeros: a local model that
/// reports nothing must be visibly distinct from one that genuinely spent nothing, or the tally lies.
/// </para>
/// </summary>
public sealed class TurnOutcomeMiddleware : ILlmMiddleware
{
    public LlmPipelineStage Stage => LlmPipelineStage.Accounting;

    public async Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, LlmTurnDelegate next, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        var result = await next(ctx, ct);

        stopwatch.Stop();

        var status = result.Status == LlmTurnStatus.Ok && result.RawUsage.Count == 0
            ? LlmTurnStatus.UsageMissing
            : result.Status;

        return new LlmTurnResult
        {
            Message       = result.Message,
            ModelReported = result.ModelReported,
            RawUsage      = result.RawUsage,
            Status        = status,
            Duration      = stopwatch.Elapsed
        };
    }
}
