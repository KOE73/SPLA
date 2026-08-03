namespace SPLA.Domain.Llm;

/// <summary>
/// What a provider contributes: an id to be selected by, and a client to run turns with.
/// <para>
/// Deliberately not a middleware. A provider is <i>data for the terminal step</i>, so it cannot wrap,
/// observe or short-circuit anything — which is what keeps "a provider slipped past accounting"
/// impossible by construction rather than by review. See <see cref="LlmPipelineBlueprint"/>.
/// </para>
/// </summary>
public interface ILlmProviderDescriptor
{
    /// <summary>The value a connection's <c>provider</c> field carries. Case-insensitive, stable —
    /// it is persisted in user config, so renaming one breaks projects.</summary>
    string Id { get; }

    /// <summary>Human-readable label for pickers and settings.</summary>
    string DisplayName { get; }

    /// <summary>The client for this provider. One instance serves every connection using it:
    /// per-turn variation (endpoint, key, model) travels in the context, never in the client.</summary>
    ILlmClient Client { get; }
}

/// <summary>A provider descriptor built from parts, for hosts and plugins that have nothing to add
/// beyond the three values.</summary>
public sealed class LlmProviderDescriptor : ILlmProviderDescriptor
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required ILlmClient Client { get; init; }
}

/// <summary>
/// The set of providers available to one runtime, keyed by <see cref="ILlmProviderDescriptor.Id"/>.
/// <para>
/// Built once per runtime, after plugins load — providers arrive as plugins and are enabled per
/// project, so the set is not process-wide. Registration is closed before the first turn: a registry
/// that could change underneath a running conversation would make "which model answered" unanswerable
/// after the fact.
/// </para>
/// </summary>
public sealed class LlmProviderRegistry
{
    private readonly Dictionary<string, ILlmProviderDescriptor> _providers =
        new(StringComparer.OrdinalIgnoreCase);

    private string? _defaultId;

    /// <summary>
    /// Adds a provider. The first one registered becomes the fallback for connections that name no
    /// provider — projects predating the field, and the synthesized default connection.
    /// </summary>
    /// <exception cref="InvalidOperationException">Two providers claim the same id. Silently letting
    /// one win would make which client serves a connection depend on plugin load order.</exception>
    public LlmProviderRegistry Register(ILlmProviderDescriptor provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(provider.Id))
            throw new InvalidOperationException("A provider must have a non-empty id.");

        if (!_providers.TryAdd(provider.Id, provider))
            throw new InvalidOperationException(
                $"Provider id '{provider.Id}' is already registered. Ids are what connections persist, " +
                "so two providers cannot share one.");

        _defaultId ??= provider.Id;
        return this;
    }

    /// <summary>Every registered provider, in registration order.</summary>
    public IReadOnlyCollection<ILlmProviderDescriptor> All => _providers.Values;

    public bool Contains(string? id) => !string.IsNullOrWhiteSpace(id) && _providers.ContainsKey(id);

    /// <summary>
    /// Finds the provider for an id, falling back to the default when the id is absent.
    /// </summary>
    /// <exception cref="InvalidOperationException">The id is named but unknown — a typo in
    /// <c>provider:</c>, or a plugin that is not loaded. Failing loudly beats quietly answering from
    /// the wrong provider with the wrong credential.</exception>
    public ILlmProviderDescriptor Resolve(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            if (_defaultId == null)
                throw new InvalidOperationException("No LLM providers are registered.");
            return _providers[_defaultId];
        }

        if (_providers.TryGetValue(id, out var found)) return found;

        throw new InvalidOperationException(
            $"Unknown LLM provider '{id}'. Registered: {string.Join(", ", _providers.Keys)}. " +
            "Check the connection's provider field, or whether the plugin supplying it is enabled.");
    }
}

/// <summary>
/// Dispatches each turn to the client of the connection's provider. Replaces
/// <see cref="SingleClientResolver"/> now that more than one provider exists.
/// <para>
/// The provider id rides on <see cref="LlmTurnContext.Settings"/> because that is what already
/// carries the resolved connection for the turn — the resolver never re-reads project config, and so
/// cannot disagree with the rest of the pipeline about which connection is in play.
/// </para>
/// </summary>
public sealed class ProviderClientResolver : ILlmClientResolver
{
    private readonly LlmProviderRegistry _registry;

    public ProviderClientResolver(LlmProviderRegistry registry) => _registry = registry;

    public ILlmClient Resolve(LlmTurnContext ctx) => _registry.Resolve(ctx.Settings.Provider).Client;
}
