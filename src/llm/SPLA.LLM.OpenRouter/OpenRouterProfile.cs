using System.Net.Http.Headers;
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

    public void ShapeHeaders(HttpRequestHeaders headers)
    {
        if (!string.IsNullOrWhiteSpace(_referer)) headers.TryAddWithoutValidation("HTTP-Referer", _referer);
        if (!string.IsNullOrWhiteSpace(_title)) headers.TryAddWithoutValidation("X-Title", _title);
    }
}
