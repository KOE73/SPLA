using System.Net.Http.Headers;
using System.Text.Json;
using SPLA.Domain.Models;

namespace SPLA.LLM.OpenRouter;

/// <summary>One model as OpenRouter's catalog describes it. Richer than the bare id list an
/// OpenAI-compatible <c>/models</c> gives, which is the whole reason this client exists.</summary>
public sealed class OpenRouterModel
{
    public required string Id { get; init; }
    public string Name { get; init; } = string.Empty;

    /// <summary>Prompt price per token, as a decimal string. "0" means free.</summary>
    public string PromptPrice { get; init; } = string.Empty;

    /// <summary>Completion price per token, as a decimal string.</summary>
    public string CompletionPrice { get; init; } = string.Empty;

    public int ContextLength { get; init; }

    public bool SupportsTools { get; init; }
    public bool SupportsReasoning { get; init; }

    /// <summary>What the catalog says about this model's reasoning channel.</summary>
    public ReasoningCapability Reasoning { get; init; } = ReasoningCapability.Unknown;

    /// <summary>
    /// True when the model costs nothing. Taken from the reported price rather than from the
    /// <c>:free</c> suffix alone — the suffix is a naming convention, the price is the fact.
    /// </summary>
    public bool IsFree =>
        (decimal.TryParse(PromptPrice, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var p) && p == 0) &&
        (decimal.TryParse(CompletionPrice, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var c) && c == 0);

    /// <summary>Price per million prompt tokens, for display. Empty when unknown.</summary>
    public string PromptPricePerMillion => PerMillion(PromptPrice);

    /// <summary>Price per million completion tokens, for display.</summary>
    public string CompletionPricePerMillion => PerMillion(CompletionPrice);

    // Formatted invariantly, not in the current culture: these values cross a JSON boundary to the
    // web UI and get compared as strings. On a ru-RU machine the culture default renders 0.14 as
    // "0,14", which is neither what the provider said nor something the UI can parse back.
    private static string PerMillion(string perToken)
        => decimal.TryParse(perToken, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v)
            ? (v * 1_000_000m).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)
            : string.Empty;
}

/// <summary>
/// Reads <c>GET /api/v1/models</c>. Unauthenticated on OpenRouter's side, so the catalog can be
/// browsed before a key is entered.
/// <para>
/// Nothing here is cached. The catalog is hundreds of entries that change weekly; holding it would
/// buy a stale copy and an invalidation problem. What <i>is</i> worth keeping is the handful of
/// fields for a model the user actually picked — and those belong in that model's config entry, as
/// data with a date on it, not in a cache.
/// </para>
/// </summary>
public sealed class OpenRouterCatalogClient
{
    private readonly HttpClient _http;

    public OpenRouterCatalogClient(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<OpenRouterModel>> GetModelsAsync(
        string endpoint, string? apiKey = null, CancellationToken ct = default)
    {
        var baseUrl = endpoint.EndsWith('/') ? endpoint : endpoint + "/";
        using var req = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(baseUrl), "models"));
        if (!string.IsNullOrWhiteSpace(apiKey))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var res = await _http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
        if (!doc.RootElement.TryGetProperty("data", out var arr) || arr.ValueKind != JsonValueKind.Array)
            return [];

        var models = new List<OpenRouterModel>();
        foreach (var item in arr.EnumerateArray())
        {
            var id = Str(item, "id");
            if (string.IsNullOrEmpty(id)) continue;

            var pricing = item.TryGetProperty("pricing", out var p) ? p : default;
            var supported = item.TryGetProperty("supported_parameters", out var sp) && sp.ValueKind == JsonValueKind.Array
                ? sp.EnumerateArray().Select(x => x.GetString() ?? "").ToArray()
                : [];

            models.Add(new OpenRouterModel
            {
                Id = id,
                Name = Str(item, "name"),
                PromptPrice = pricing.ValueKind == JsonValueKind.Object ? Str(pricing, "prompt") : "",
                CompletionPrice = pricing.ValueKind == JsonValueKind.Object ? Str(pricing, "completion") : "",
                ContextLength = item.TryGetProperty("context_length", out var cl) && cl.ValueKind == JsonValueKind.Number
                    ? cl.GetInt32() : 0,
                SupportsTools = supported.Contains("tools"),
                SupportsReasoning = supported.Contains("reasoning") || supported.Contains("include_reasoning"),
                Reasoning = ReadReasoning(item, supported)
            });
        }

        return models;
    }

    /// <summary>
    /// Reads OpenRouter's per-model <c>reasoning</c> descriptor — the richest one any provider in the
    /// fleet publishes, and the shape <see cref="ReasoningCapability"/> is modelled on:
    /// <c>{ mandatory, default_enabled, supports_max_tokens, supported_efforts, default_effort }</c>.
    /// Falls back to <c>supported_parameters</c> for a model that lists <c>reasoning</c> but carries
    /// no descriptor.
    /// </summary>
    private static ReasoningCapability ReadReasoning(JsonElement item, string[] supported)
    {
        if (item.TryGetProperty("reasoning", out var r) && r.ValueKind == JsonValueKind.Object)
        {
            var efforts = r.TryGetProperty("supported_efforts", out var se) && se.ValueKind == JsonValueKind.Array
                ? se.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => x.Length > 0).ToList()
                : [];

            return new ReasoningCapability
            {
                Known = true,
                Supported = true,
                Mandatory = Bool(r, "mandatory"),
                DefaultEnabled = Bool(r, "default_enabled"),
                Efforts = efforts,
                DefaultEffort = Str(r, "default_effort") is { Length: > 0 } d ? d : null,
                SupportsTokenBudget = Bool(r, "supports_max_tokens")
            };
        }

        // No descriptor. The parameter list still tells us whether the lever exists at all; how deep
        // it goes is then unknown, and the UI offers only the switch.
        if (supported.Contains("reasoning") || supported.Contains("include_reasoning"))
            return new ReasoningCapability
            {
                Known = true,
                Supported = true,
                SupportsTokenBudget = supported.Contains("reasoning_max_tokens")
            };

        return ReasoningCapability.None;
    }

    private static bool Bool(JsonElement el, string field)
        => el.TryGetProperty(field, out var v) && v.ValueKind == JsonValueKind.True;

    private static string Str(JsonElement el, string field)
        => el.ValueKind == JsonValueKind.Object && el.TryGetProperty(field, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() ?? string.Empty
            : string.Empty;
}
