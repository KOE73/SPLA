namespace SPLA.Domain.Llm;

/// <summary>
/// A provider's client — the terminal step of the pipeline, and the only place in the system that
/// knows a wire format.
/// <para>
/// One operation, deliberately. Streaming is not a second method: it is the presence of sinks on the
/// <see cref="LlmTurnContext"/>. Three overloads would have to be written N times over for N
/// providers and wrapped M times over by M middleware; one collapses that.
/// </para>
/// <para>
/// A client knows nothing about accounting, permissions, quotas or privacy — those are middleware.
/// It receives an already-resolved connection and an already-materialized credential, does the call,
/// and reports what came back. A new provider therefore requires no change anywhere else.
/// </para>
/// </summary>
public interface ILlmClient
{
    Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, CancellationToken ct = default);
}

/// <summary>
/// Picks the client for a turn. Stage 1 resolves to the single built-in provider; once providers
/// arrive as plugins this consults the provider registry by the connection's <c>provider</c> field.
/// </summary>
public interface ILlmClientResolver
{
    ILlmClient Resolve(LlmTurnContext ctx);
}
