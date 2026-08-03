namespace SPLA.Domain.Llm;

/// <summary>
/// The declared composition of a pipeline — a list, not a running thing.
/// <para>
/// Assembly happens at three levels with three lifetimes, and mixing them up is how this kind of
/// design rots:
/// </para>
/// <list type="number">
/// <item><b>Host, at process start</b> — declares the blueprint: which middleware, and the host's own
/// policy implementations (permissive locally, role-based on a server; file ledger or database).
/// This is the only place CLI, server and worker differ.</item>
/// <item><b>Runtime, in its constructor</b> — bakes the blueprint into an immutable gateway, after
/// plugins are loaded, because providers arrive as plugins and are enabled per project.</item>
/// <item><b>Per turn</b> — nothing is assembled. Only a <see cref="LlmTurnContext"/> is created.</item>
/// </list>
/// </summary>
public sealed class LlmPipelineBlueprint
{
    private readonly List<ILlmMiddleware> _middleware = new();

    /// <summary>Adds host-owned middleware. Any stage is allowed.</summary>
    public LlmPipelineBlueprint Use(ILlmMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _middleware.Add(middleware);
        return this;
    }

    /// <summary>
    /// Adds plugin-contributed middleware, which may only occupy <see cref="LlmPipelineStage.Policy"/>
    /// or <see cref="LlmPipelineStage.Content"/>.
    /// <para>
    /// Accounting and Transport are sealed so that "a plugin slipped between Usage and the provider
    /// and zeroed the accounting" is impossible <i>by construction</i> rather than by review. Note
    /// that a plugin still supplies providers — but a provider is data for the terminal step, not a
    /// layer, so it cannot intercept anything.
    /// </para>
    /// </summary>
    public LlmPipelineBlueprint UseFromPlugin(string pluginId, ILlmMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        if (middleware.Stage is not (LlmPipelineStage.Policy or LlmPipelineStage.Content))
            throw new InvalidOperationException(
                $"Plugin '{pluginId}' tried to add middleware at stage {middleware.Stage}. " +
                "Plugins may only extend Policy and Content; Accounting and Transport are host-owned.");

        _middleware.Add(middleware);
        return this;
    }

    /// <summary>
    /// Bakes the delegate chain once. Ordering is by <see cref="ILlmMiddleware.Stage"/> and is stable
    /// within a stage, so registration order still decides ties but can never violate the stage
    /// contract. Validation belongs here — at startup — not at the first turn a user runs.
    /// </summary>
    public ILlmGateway Build(ILlmClientResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        LlmTurnDelegate pipeline = (ctx, ct) => resolver.Resolve(ctx).InvokeAsync(ctx, ct);

        // Fold from the inside out so the first element of the ordered list ends up outermost.
        foreach (var middleware in _middleware.OrderBy(m => m.Stage).Reverse())
        {
            var next = pipeline;
            var current = middleware;
            pipeline = (ctx, ct) => current.InvokeAsync(ctx, next, ct);
        }

        return new LlmPipeline(pipeline);
    }

    private sealed class LlmPipeline : ILlmGateway
    {
        private readonly LlmTurnDelegate _entry;

        public LlmPipeline(LlmTurnDelegate entry) => _entry = entry;

        public Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, CancellationToken ct = default)
            => _entry(ctx, ct);
    }
}

/// <summary>Trivial resolver for a single built-in provider. Replaced by the provider registry once
/// providers ship as plugins.</summary>
public sealed class SingleClientResolver : ILlmClientResolver
{
    private readonly ILlmClient _client;

    public SingleClientResolver(ILlmClient client) => _client = client;

    public ILlmClient Resolve(LlmTurnContext ctx) => _client;
}
