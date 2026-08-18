using System.Net.Http.Headers;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.LLM.OpenAiCompat;

namespace SPLA.LLM.OpenRouter;

/// <summary>
/// OpenRouter's dialect on top of plain OpenAI-compatible. Two differences, both documented by
/// OpenRouter:
/// <list type="bullet">
/// <item><c>usage.include</c> — asks for the usage block to carry the call's actual cost, so spend is
/// a reported fact rather than something we recompute from a price list that may have moved.</item>
/// <item><c>HTTP-Referer</c> / <c>X-Title</c> — how OpenRouter attributes traffic to an app. Optional
/// on their side, and sent only when configured.</item>
/// </list>
/// </summary>
public sealed class OpenRouterProfile : IOpenAiCompatProfile
{
    private readonly string? _referer;
    private readonly string? _title;

    public OpenRouterProfile(string? referer = null, string? title = null)
    {
        _referer = referer;
        _title = title;
    }

    public void ShapeBody(IDictionary<string, object> payload)
        => payload["usage"] = new Dictionary<string, object> { ["include"] = true };

    /// <summary>
    /// OpenRouter's <c>reasoning</c> block rather than the bare <c>reasoning_effort</c>. It is the one
    /// place in the fleet where all three axes are expressible at once — <c>enabled</c>,
    /// <c>effort</c> and <c>max_tokens</c> — and OpenRouter normalizes whichever the downstream model
    /// actually understands, translating an effort into a budget or back as needed.
    /// </summary>
    public void ShapeReasoning(IDictionary<string, object> payload, ReasoningChoice choice, ReasoningCapability capability)
    {
        if (choice.IsDefault || !capability.Supported) return;

        var block = new Dictionary<string, object?>();
        switch (choice.Mode)
        {
            case ReasoningMode.Off when capability.CanDisable:
                block["enabled"] = false;
                break;

            case ReasoningMode.On:
                block["enabled"] = true;
                break;

            case ReasoningMode.Effort when choice.Effort is { Length: > 0 } e:
                block["effort"] = e;
                break;

            case ReasoningMode.Budget when choice.TokenBudget is { } b && capability.SupportsTokenBudget:
                block["max_tokens"] = b;
                break;
        }

        if (block.Count > 0) payload["reasoning"] = block;
    }

    public void ShapeHeaders(HttpRequestHeaders headers)
    {
        if (!string.IsNullOrWhiteSpace(_referer)) headers.TryAddWithoutValidation("HTTP-Referer", _referer);
        if (!string.IsNullOrWhiteSpace(_title)) headers.TryAddWithoutValidation("X-Title", _title);
    }
}
