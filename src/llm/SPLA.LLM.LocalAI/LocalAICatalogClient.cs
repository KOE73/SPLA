using System.Net.Http.Headers;
using System.Text.Json;

namespace SPLA.LLM.LocalAI;

/// <summary>
/// Reads a model's configured context window from LocalAI's extension API. Unlike LM Studio, LocalAI
/// has no native "loaded instance" surface to poll — a model's window lives in its (optional) saved
/// YAML config, read back as JSON via <c>GET /api/models/config-json/{id}</c>. A model started without
/// an explicit config (loaded ad hoc from a gguf) has none; that is reported as "unknown", not an error.
/// </summary>
public sealed class LocalAICatalogClient
{
    private readonly HttpClient _httpClient;

    public LocalAICatalogClient(HttpClient httpClient) => _httpClient = httpClient;

    /// <summary>The model's <c>context_size</c>, or null when the model has no saved config or the
    /// field is absent/zero.</summary>
    public async Task<int?> GetContextLengthAsync(
        string endpoint, string modelId, string? apiKey, CancellationToken cancellationToken = default)
    {
        var uri = BuildConfigJsonUri(endpoint, modelId);
        if (uri == null) return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        if (!string.IsNullOrEmpty(apiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("context_size", out var el)) return null;
        if (el.ValueKind != JsonValueKind.Number || !el.TryGetInt32(out var size) || size <= 0) return null;
        return size;
    }

    /// <summary>Derives {scheme}://{host}/api/models/config-json/{id} from an OpenAI-style base url.
    /// The id is appended raw (not percent-encoded): LocalAI's route matches the literal slashes in
    /// ids like "google/gemma-4-26b-a4b-qat" and does not accept the %2F-encoded form.</summary>
    private static Uri? BuildConfigJsonUri(string baseUrl, string modelId)
    {
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(modelId)) return null;
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)) return null;
        return new Uri($"{uri.Scheme}://{uri.Authority}/api/models/config-json/{modelId}");
    }
}
