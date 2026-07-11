using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SPLA.LLM.LMStudio;

/// <summary>
/// LM Studio server management against the native <c>/api/v1/*</c> API:
/// model listing with capabilities, load and unload. Kept separate from
/// <see cref="LMStudioClient"/> (chat) because the native API cannot carry custom tools or
/// assistant messages and is therefore unsuitable for SPLA's dialogue.
/// </summary>
public sealed class LMStudioManagementClient : IModelManagementService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public LMStudioManagementClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<bool> IsAvailableAsync(string baseUrl, string apiKey, CancellationToken cancellationToken = default)
    {
        var uri = BuildNativeUri(baseUrl, "models");
        if (uri == null) return false;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<ModelInfo>> GetModelDetailsAsync(string baseUrl, string apiKey = "lm-studio", CancellationToken cancellationToken = default)
    {
        var nativeUri = BuildNativeUri(baseUrl, "models");
        if (nativeUri != null)
        {
            try
            {
                var native = await FetchNativeAsync(nativeUri, apiKey, cancellationToken);
                if (native.Count > 0) return native;
            }
            catch
            {
                // Native endpoint unavailable (non-LM Studio provider) — fall through.
            }
        }

        // Fallback: OpenAI-compatible /v1/models (ids only, no capabilities).
        return await FetchOpenAiIdsAsync(baseUrl, apiKey, cancellationToken);
    }

    public async Task LoadModelAsync(string baseUrl, string apiKey, string modelKey, CancellationToken cancellationToken = default)
    {
        var uri = BuildNativeUri(baseUrl, "models/load")
                  ?? throw new InvalidOperationException("Cannot derive the LM Studio management endpoint from the configured URL.");
        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(new { model = modelKey }, options: _jsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UnloadModelAsync(string baseUrl, string apiKey, string instanceId, CancellationToken cancellationToken = default)
    {
        var uri = BuildNativeUri(baseUrl, "models/unload")
                  ?? throw new InvalidOperationException("Cannot derive the LM Studio management endpoint from the configured URL.");
        using var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(new { instance_id = instanceId }, options: _jsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    // ── Native list parsing ────────────────────────────────────────────────

    private async Task<List<ModelInfo>> FetchNativeAsync(Uri uri, string apiKey, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);

        var result = new List<ModelInfo>();

        // The native API returns { "models": [...] }; older/alternate shapes use { "data": [...] }.
        if (!doc.RootElement.TryGetProperty("models", out var arr) || arr.ValueKind != JsonValueKind.Array)
        {
            if (!doc.RootElement.TryGetProperty("data", out arr) || arr.ValueKind != JsonValueKind.Array)
                return result;
        }

        foreach (var item in arr.EnumerateArray())
        {
            // Id is "key" in the native API, "id" in OpenAI-shaped payloads.
            var id = GetStr(item, "key");
            if (string.IsNullOrEmpty(id)) id = GetStr(item, "id");
            if (string.IsNullOrEmpty(id)) continue;

            var type = GetStr(item, "type");

            // capabilities is an object: { vision, trained_for_tool_use, reasoning: { allowed_options, default } }
            bool vision = false, tools = false;
            var reasoningOptions = new List<string>();
            var reasoningDefault = "";
            if (item.TryGetProperty("capabilities", out var caps) && caps.ValueKind == JsonValueKind.Object)
            {
                vision = GetBool(caps, "vision");
                tools = GetBool(caps, "trained_for_tool_use") || GetBool(caps, "tool_use");
                if (caps.TryGetProperty("reasoning", out var reason) && reason.ValueKind == JsonValueKind.Object)
                {
                    if (reason.TryGetProperty("allowed_options", out var opts) && opts.ValueKind == JsonValueKind.Array)
                        foreach (var o in opts.EnumerateArray())
                            if (o.ValueKind == JsonValueKind.String) reasoningOptions.Add(o.GetString() ?? "");
                    reasoningDefault = GetStr(reason, "default");
                }
            }

            // quantization is an object { name, bits_per_weight } in the native API, or a bare string elsewhere.
            string quant = ""; int bits = 0;
            if (item.TryGetProperty("quantization", out var q))
            {
                if (q.ValueKind == JsonValueKind.Object) { quant = GetStr(q, "name"); bits = GetInt(q, "bits_per_weight"); }
                else if (q.ValueKind == JsonValueKind.String) quant = q.GetString() ?? "";
            }

            // loaded_instances is a non-empty array when the model is loaded. The instance's config
            // carries the OPERATIVE context window (context_length it was loaded with) — distinct
            // from the model's max_context_length, and the one requests actually fail against.
            bool loaded = false;
            string loadedInstanceId = "";
            int loadedContextLength = 0;
            if (item.TryGetProperty("loaded_instances", out var li) && li.ValueKind == JsonValueKind.Array && li.GetArrayLength() > 0)
            {
                loaded = true;
                loadedInstanceId = GetStr(li[0], "id");
                if (li[0].TryGetProperty("config", out var cfg) && cfg.ValueKind == JsonValueKind.Object)
                    loadedContextLength = GetInt(cfg, "context_length");
            }
            if (!loaded) loaded = string.Equals(GetStr(item, "state"), "loaded", StringComparison.OrdinalIgnoreCase);

            result.Add(new ModelInfo
            {
                Id = id,
                DisplayName = GetStr(item, "display_name"),
                Type = type,
                Arch = GetStr(item, "architecture"),
                Quantization = quant,
                BitsPerWeight = bits,
                ParamsString = GetStr(item, "params_string"),
                SizeBytes = GetLong(item, "size_bytes"),
                MaxContextLength = GetInt(item, "max_context_length"),
                LoadedContextLength = loadedContextLength,
                IsLoaded = loaded,
                LoadedInstanceId = loadedInstanceId,
                SupportsVision = vision || string.Equals(type, "vlm", StringComparison.OrdinalIgnoreCase),
                SupportsTools = tools,
                ReasoningOptions = reasoningOptions,
                ReasoningDefault = reasoningDefault,
                HasDetails = true
            });
        }

        return result;
    }

    private async Task<List<ModelInfo>> FetchOpenAiIdsAsync(string baseUrl, string apiKey, CancellationToken cancellationToken)
    {
        if (!baseUrl.EndsWith('/')) baseUrl += "/";
        var uri = new Uri(new Uri(baseUrl), "models");

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);

        var result = new List<ModelInfo>();
        if (doc.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                var id = GetStr(item, "id");
                if (string.IsNullOrEmpty(id)) continue;
                // vLLM extends the OpenAI shape with max_model_len (the serving window). Other
                // OpenAI-compatible servers occasionally use context_window/context_length.
                var contextLen = GetInt(item, "max_model_len");
                if (contextLen == 0) contextLen = GetInt(item, "context_window");
                if (contextLen == 0) contextLen = GetInt(item, "context_length");
                result.Add(new ModelInfo { Id = id, MaxContextLength = contextLen, HasDetails = false });
            }
        }
        return result;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static string GetStr(JsonElement el, string name) =>
        el.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() ?? "" : "";

    private static bool GetBool(JsonElement el, string name) =>
        el.TryGetProperty(name, out var p) && (p.ValueKind == JsonValueKind.True || p.ValueKind == JsonValueKind.False) && p.GetBoolean();

    private static int GetInt(JsonElement el, string name) =>
        el.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var v) ? v : 0;

    private static long GetLong(JsonElement el, string name) =>
        el.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number && p.TryGetInt64(out var v) ? v : 0;

    /// <summary>Derives {scheme}://{host}/api/v1/{relative} from an OpenAI-style base url, or null.</summary>
    private static Uri? BuildNativeUri(string baseUrl, string relative)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return null;
        if (!baseUrl.EndsWith('/')) baseUrl += "/";
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)) return null;
        return new Uri($"{uri.Scheme}://{uri.Authority}/api/v1/{relative}");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"LM Studio management request failed: {(int)response.StatusCode} ({response.ReasonPhrase}). Body: {body}",
            null, response.StatusCode);
    }
}
