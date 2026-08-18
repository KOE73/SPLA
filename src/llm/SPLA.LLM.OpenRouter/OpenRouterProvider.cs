using Microsoft.Extensions.Logging;
using SPLA.Domain.Llm;
using SPLA.LLM.OpenAiCompat;
using System.Linq;

namespace SPLA.LLM.OpenRouter;

/// <summary>
/// The OpenRouter provider: everything the rest of the system needs in order to talk to OpenRouter,
/// in one place that nothing else has to know about.
/// <para>
/// It owns three things — the id connections persist, the dialect its transport speaks, and the
/// account telemetry only it knows how to read. What it deliberately does <b>not</b> own is the
/// transport itself: <see cref="OpenAiCompatibleClient"/> is shared, because a fork of that code per
/// provider would give four subtly different streaming implementations and one correct one.
/// </para>
/// <para>
/// Adding a provider means adding a project like this one and registering it. No middleware, no
/// change to the pipeline, no change to the settings protocol.
/// </para>
/// </summary>
public sealed class OpenRouterProvider : ILlmProviderDescriptor, IProviderAccountInfo, IModelCatalogInfo, IReasoningCatalog
{
    public const string ProviderId = "openrouter";

    /// <summary>The endpoint a new OpenRouter connection should start with.</summary>
    public const string DefaultEndpoint = "https://openrouter.ai/api/v1";

    public string Id => ProviderId;
    public string DisplayName => "OpenRouter";
    public ILlmClient Client { get; }

    /// <summary>Account balance, key limits and usage.</summary>
    public IProviderAccountInfo Account { get; }

    /// <summary>
    /// The descriptor itself implements the capability, forwarding to <see cref="Account"/>.
    /// <para>
    /// This is what makes the type check work. Callers hold a <see cref="ILlmProviderDescriptor"/>
    /// from the registry and ask <c>is IProviderAccountInfo</c> — exactly the
    /// <see cref="Domain.Interfaces.ITokenUsageReporter"/> pattern. Hiding the capability behind a
    /// property instead would force every caller to know this concrete type, which is the coupling
    /// the capability interface exists to prevent.
    /// </para>
    /// </summary>
    public Task<IReadOnlyList<ProviderFactSection>> GetAccountInfoAsync(
        string endpoint, string? apiKey, string? adminKey, CancellationToken ct = default)
        => Account.GetAccountInfoAsync(endpoint, apiKey, adminKey, ct);

    /// <summary>The model catalog, with prices and capabilities as OpenRouter reports them.</summary>
    public OpenRouterCatalogClient Catalog { get; }

    /// <summary>
    /// The capability itself: OpenRouter models are never "loaded", so their context window comes
    /// from the catalog's <c>context_length</c> rather than a native management surface.
    /// </summary>
    public async Task<int?> GetContextLengthAsync(
        string endpoint, string modelId, string? apiKey, CancellationToken ct = default)
    {
        var models = await Catalog.GetModelsAsync(endpoint, apiKey, ct);
        var match = models.FirstOrDefault(m => string.Equals(m.Id, modelId, StringComparison.OrdinalIgnoreCase));
        return match is { ContextLength: > 0 } ? match.ContextLength : null;
    }

    /// <summary>
    /// The reasoning descriptor, from the same catalog call as the context window. OpenRouter is the
    /// one provider here that publishes all three axes — switch, effort scale and token budget — so
    /// its answer needs no reconstruction from a flat option list.
    /// </summary>
    public async Task<Domain.Models.ReasoningCapability> GetReasoningAsync(
        string endpoint, string modelId, string? apiKey, CancellationToken ct = default)
    {
        var models = await Catalog.GetModelsAsync(endpoint, apiKey, ct);
        var match = models.FirstOrDefault(m => string.Equals(m.Id, modelId, StringComparison.OrdinalIgnoreCase));
        return match?.Reasoning ?? Domain.Models.ReasoningCapability.Unknown;
    }

    /// <param name="appTitle">Sent as <c>X-Title</c> so traffic is attributable on OpenRouter's side.</param>
    public OpenRouterProvider(HttpClient http, ILoggerFactory loggerFactory, string? appTitle = "SPLA", string? appUrl = null)
    {
        Client = new OpenAiCompatibleClient(
            http,
            loggerFactory.CreateLogger<OpenAiCompatibleClient>(),
            new OpenRouterProfile(referer: appUrl, title: appTitle));

        Account = new OpenRouterAccountClient(http);
        Catalog = new OpenRouterCatalogClient(http);
    }
}
