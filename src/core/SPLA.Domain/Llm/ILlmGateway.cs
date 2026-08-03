namespace SPLA.Domain.Llm;

/// <summary>
/// The composed pipeline — the ONLY way to reach a model.
/// <para>
/// <b>Never publish <see cref="ILlmClient"/> itself.</b> Not to the orchestrator, not to the spawn
/// runner, not to a plugin. If provider clients are reachable directly, accounting, quotas and
/// privacy are all optional in practice, and the first caller in a hurry will bypass them.
/// </para>
/// <para>
/// <b>The boundary that must not move:</b> a gateway call is ONE network call to ONE model. An agent
/// turn is N of those, and the agent loop lives above the gateway, never inside it. Anything that
/// reasons about a whole turn is not middleware.
/// </para>
/// </summary>
public interface ILlmGateway
{
    Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, CancellationToken ct = default);
}
