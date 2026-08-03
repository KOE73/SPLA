using System.Net.Http.Headers;

namespace SPLA.LLM.OpenAiCompat;

/// <summary>
/// The small, declarative difference between one OpenAI-compatible provider and another.
/// <para>
/// LM Studio, vLLM, OpenAI and OpenRouter all speak the same <c>chat/completions</c> dialect; what
/// varies is a handful of extra headers and body fields. Expressing that as data rather than as a
/// subclass keeps the streaming, tool-call and error-classification logic in exactly one place —
/// forking it per provider is how those end up subtly different and only one of them correct.
/// </para>
/// <para>
/// A profile is <b>not</b> a provider. It is the dialect knob a provider hands to the shared
/// transport; the provider itself — its id, its settings, its account telemetry — lives in its own
/// project and owns this object rather than being replaced by it.
/// </para>
/// </summary>
public interface IOpenAiCompatProfile
{
    /// <summary>Adds provider-specific fields to the request body, in place.</summary>
    void ShapeBody(IDictionary<string, object> payload);

    /// <summary>Adds provider-specific headers. Authorization is already set by the caller.</summary>
    void ShapeHeaders(HttpRequestHeaders headers);
}

/// <summary>Plain OpenAI-compatible: nothing to add. LM Studio, vLLM and OpenAI itself.</summary>
public sealed class PlainOpenAiCompatProfile : IOpenAiCompatProfile
{
    public static readonly PlainOpenAiCompatProfile Instance = new();

    public void ShapeBody(IDictionary<string, object> payload) { }
    public void ShapeHeaders(HttpRequestHeaders headers) { }
}
