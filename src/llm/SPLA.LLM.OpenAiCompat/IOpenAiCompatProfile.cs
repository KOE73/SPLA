using System.Net.Http.Headers;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;

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

    /// <summary>
    /// Writes the reasoning selection into the body in this provider's dialect.
    /// <para>
    /// This is the one knob with no cross-vendor standard: <c>reasoning_effort</c> with a fixed
    /// vocabulary here, a <c>reasoning</c> object there, a chat-template kwarg elsewhere, a token
    /// budget on the API-native providers. The default below is the OpenAI form, which LM Studio and
    /// most local runtimes also take.
    /// </para>
    /// <param name="capability">What is actually known about the model. A profile must not invent a
    /// lever for a model nobody described — see <see cref="Domain.Settings.LLMSettings.ModelReasoning"/>
    /// for what that costs.</param>
    /// </summary>
    void ShapeReasoning(IDictionary<string, object> payload, ReasoningChoice choice, ReasoningCapability capability)
        => OpenAiReasoningDialect.Apply(payload, choice, capability);
}

/// <summary>
/// The OpenAI form of the reasoning lever: a single <c>reasoning_effort</c> string. Shared default
/// because LM Studio, vLLM and OpenAI itself all read it.
/// <para>
/// Verified against LM Studio 0.3.x, whose server accepts exactly
/// <c>none | minimal | low | medium | high | xhigh</c> and rejects anything else with a 400 — note
/// that this is the <i>server's</i> vocabulary and not the model's: the same build advertises Qwen3.8
/// as <c>["off","low","medium","xhigh","on"]</c>. "off" is the model's word for what the wire calls
/// "none", and that translation happens here rather than being pushed onto the person choosing.
/// </para>
/// <para>
/// The chat-template kwarg (<c>enable_thinking</c>) is deliberately not sent alongside: LM Studio
/// ignores it for these templates (measured — thinking continued, 23 reasoning tokens, with
/// <c>enable_thinking:false</c>), and OpenAI rejects unknown body fields outright.
/// </para>
/// </summary>
public static class OpenAiReasoningDialect
{
    /// <summary>What "off" is called on the wire, as opposed to in a model's option list.</summary>
    public const string OffWord = "none";

    public static void Apply(IDictionary<string, object> payload, ReasoningChoice choice, ReasoningCapability capability)
    {
        // Nothing chosen, or nothing known about the model: say nothing. An unadvertised model that
        // silently accepts a field it does not implement is the dangerous case, not the safe one.
        if (choice.IsDefault || !capability.Supported) return;

        switch (choice.Mode)
        {
            case ReasoningMode.Off when capability.CanDisable:
                payload["reasoning_effort"] = OffWord;
                break;

            case ReasoningMode.On:
                // "Think, at your usual depth." Only worth saying when the model would not have.
                if (!capability.DefaultEnabled && capability.DefaultEffort is { Length: > 0 } d)
                    payload["reasoning_effort"] = d;
                break;

            case ReasoningMode.Effort when choice.Effort is { Length: > 0 } e:
                payload["reasoning_effort"] = e;
                break;

            // A token budget has no OpenAI-dialect field. Providers that take one say so through
            // ReasoningCapability.SupportsTokenBudget and override this method.
            case ReasoningMode.Budget:
                break;
        }
    }
}

/// <summary>Plain OpenAI-compatible: nothing to add. LM Studio, vLLM and OpenAI itself.</summary>
public sealed class PlainOpenAiCompatProfile : IOpenAiCompatProfile
{
    public static readonly PlainOpenAiCompatProfile Instance = new();

    public void ShapeBody(IDictionary<string, object> payload) { }
    public void ShapeHeaders(HttpRequestHeaders headers) { }
}
