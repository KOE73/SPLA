using Microsoft.Extensions.Logging;
using SPLA.Domain.Llm;
using SPLA.LLM.OpenAiCompat;

namespace SPLA.LLM.LocalAI;

/// <summary>
/// The LocalAI provider. Chat (including <c>usage</c>) goes through the shared
/// <see cref="OpenAiCompatibleClient"/> — LocalAI's chat endpoint is plain OpenAI-compatible and needs
/// no dialect of its own. The one thing it does need is <see cref="IModelCatalogInfo"/>: LocalAI has no
/// native load/unload management surface like LM Studio, so the operative context window is read
/// per-model from its own extension endpoint instead (<see cref="LocalAICatalogClient"/>).
/// </summary>
public sealed class LocalAIProvider : ILlmProviderDescriptor, IModelCatalogInfo
{
    public const string ProviderId = "localai";

    public string Id => ProviderId;
    public string DisplayName => "LocalAI";
    public ILlmClient Client { get; }

    public LocalAICatalogClient Catalog { get; }

    public Task<int?> GetContextLengthAsync(
        string endpoint, string modelId, string? apiKey, CancellationToken ct = default)
        => Catalog.GetContextLengthAsync(endpoint, modelId, apiKey, ct);

    public LocalAIProvider(HttpClient http, ILoggerFactory loggerFactory)
    {
        Client = new OpenAiCompatibleClient(http, loggerFactory.CreateLogger<OpenAiCompatibleClient>());
        Catalog = new LocalAICatalogClient(http);
    }
}
