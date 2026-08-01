using Microsoft.Extensions.Logging;
using SPLA.Domain.Llm;
using SPLA.LLM.OpenAiCompat;

namespace SPLA.LLM.LMStudio;

/// <summary>
/// The local OpenAI-compatible family: LM Studio, vLLM, OpenAI itself, and anything else that speaks
/// the plain dialect.
/// <para>
/// Four ids share one client on purpose. They differ in where they are and what credential they take
/// — both per-connection facts that travel in the turn context — not in what they speak. Registering
/// them separately is what lets the settings UI offer sensible endpoint defaults and, later, per-id
/// forms, without pretending there are four transports.
/// </para>
/// </summary>
public static class LmStudioProvider
{
    public const string LmStudioId = "lmstudio";
    public const string VllmId = "vllm";
    public const string OpenAiId = "openai";
    public const string CompatId = "openai-compat";

    /// <summary>
    /// Builds the descriptors for the plain-dialect family. Order matters: the first one registered
    /// becomes the fallback for connections that name no provider, and a local LM Studio is the least
    /// surprising thing to fall back to — it needs no credential and cannot spend money.
    /// </summary>
    public static IEnumerable<ILlmProviderDescriptor> Create(HttpClient http, ILoggerFactory loggerFactory)
    {
        var client = new OpenAiCompatibleClient(http, loggerFactory.CreateLogger<OpenAiCompatibleClient>());

        yield return new LlmProviderDescriptor { Id = LmStudioId, DisplayName = "LM Studio", Client = client };
        yield return new LlmProviderDescriptor { Id = VllmId, DisplayName = "vLLM", Client = client };
        yield return new LlmProviderDescriptor { Id = OpenAiId, DisplayName = "OpenAI", Client = client };
        yield return new LlmProviderDescriptor { Id = CompatId, DisplayName = "OpenAI-compatible", Client = client };
    }
}
