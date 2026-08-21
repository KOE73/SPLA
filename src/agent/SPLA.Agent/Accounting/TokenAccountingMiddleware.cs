using SPLA.Domain.Interfaces;
using SPLA.Domain.Llm;
using SPLA.Observability;

namespace SPLA.Agent.Accounting;

/// <summary>
/// Folds the provider's token figures into the persistent tallies and the telemetry meter — once per
/// network attempt, for every caller, whether or not anyone is watching.
/// <para>
/// This used to be a host's job: each entry point subscribed <c>OnTokenUsage</c> and repeated the same
/// two <see cref="ITokenUsageStore.Record"/> calls. Three copies of one duty, and the fourth caller —
/// spawned sub-agents — never had it at all, so their tokens reached the telemetry meter (written from
/// the loop unconditionally) but never the project's tally. Two ledgers of one fact, already
/// disagreeing. The same argument the repetition guard is built on applies here: a concern that must
/// hold for every call belongs to the pipeline, not to whoever remembered to wire it.
/// </para>
/// <para>
/// At <see cref="LlmPipelineStage.Accounting"/>, which is inside <see cref="LlmPipelineStage.Retry"/>
/// and <see cref="LlmPipelineStage.Output"/> precisely so that a regenerated or retried answer is
/// counted as the second paid call it is, rather than folded into the first.
/// </para>
/// </summary>
public sealed class TokenAccountingMiddleware : ILlmMiddleware
{
    private readonly IReadOnlyList<ITokenUsageStore> _stores;

    /// <summary>The tallies to feed — project-lifetime and machine-wide, in whatever combination the
    /// host keeps. A host with none still gets the telemetry counters.</summary>
    public TokenAccountingMiddleware(params ITokenUsageStore[] stores) => _stores = stores;

    public LlmPipelineStage Stage => LlmPipelineStage.Accounting;

    public async Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, LlmTurnDelegate next, CancellationToken ct)
    {
        var result = await next(ctx, ct);

        var prompt = result.Message.PromptTokens;
        var completion = result.Message.CompletionTokens;

        // Recorded even when both are null: a store distinguishes "nothing reported" from "nothing
        // spent" — see LlmTurnStatus.UsageMissing, which exists for the same reason.
        foreach (var store in _stores)
            store.Record(prompt, completion);

        if (prompt is > 0) SplaTelemetry.PromptTokens.Add(prompt.Value);
        if (completion is > 0) SplaTelemetry.CompletionTokens.Add(completion.Value);

        return result;
    }
}
