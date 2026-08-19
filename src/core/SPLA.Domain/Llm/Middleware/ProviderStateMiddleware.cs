using SPLA.Domain.Interfaces;

namespace SPLA.Domain.Llm.Middleware;

/// <summary>
/// Moves the observations a provider volunteered into the state store.
/// <para>
/// This is the policy half of the split: the client saw HTTP headers and declared facts without
/// knowing who cares; this knows where such facts belong and never sees a wire format. Add a provider
/// with different headers and nothing here changes; add a rule about nearly-exhausted budgets and no
/// provider changes.
/// </para>
/// <para>
/// Sits at <see cref="LlmPipelineStage.Accounting"/> and catches as well as returns, because the
/// failure path carries the signals worth having — a 429 is precisely the response that reports how
/// much budget is left, and it arrives as an exception.
/// </para>
/// </summary>
public sealed class ProviderStateMiddleware : ILlmMiddleware
{
    private readonly ProviderStateStore _store;

    public ProviderStateMiddleware(ProviderStateStore store) => _store = store;

    public LlmPipelineStage Stage => LlmPipelineStage.Accounting;

    public async Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, LlmTurnDelegate next, CancellationToken ct)
    {
        try
        {
            var result = await next(ctx, ct);
            Record(ctx, result.Signals);
            return result;
        }
        catch (LlmRequestException ex)
        {
            Record(ctx, ex.Signals);
            throw;
        }
    }

    /// <summary>Stores the connection-scoped half and drops the rest. A fact about one call
    /// (<see cref="ProviderFactScope.Call"/>) is not the key's standing, and letting one in would do
    /// more than add a wrong row: the store keeps the latest list per connection, so a response
    /// carrying only per-call facts would erase the last real budget reading.</summary>
    private void Record(LlmTurnContext ctx, IReadOnlyList<ProviderFact> facts)
    {
        var standing = facts.Where(f => f.Scope == ProviderFactScope.Connection).ToList();
        if (standing.Count > 0) _store.Record(ctx.Settings.ConnectionId, standing);
    }
}
