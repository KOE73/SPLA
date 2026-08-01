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
            _store.Record(ctx.Settings.ConnectionId, result.Signals);
            return result;
        }
        catch (LlmRequestException ex)
        {
            _store.Record(ctx.Settings.ConnectionId, ex.Signals);
            throw;
        }
    }
}
