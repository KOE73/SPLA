using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SPLA.Domain.Settings;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// Lightweight diagnostics for a connection config: reachability ping, model listing, and a
/// one-shot test chat. All operations are directed at whatever endpoint+key the caller supplies —
/// they do not require the connection to be saved into the project settings first.
/// </summary>
public static class ConnectionDiagOps
{
    private static readonly HttpClient Http = new();
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Returns the /models URL for an OpenAI-compatible endpoint string.</summary>
    private static string ModelsUrl(string? endpoint) => $"{(endpoint ?? "").TrimEnd('/')}/models";

    /// <summary>Returns the /chat/completions URL for an OpenAI-compatible endpoint string.</summary>
    private static string ChatUrl(string? endpoint) => $"{(endpoint ?? "").TrimEnd('/')}/chat/completions";

    private static void AddAuth(HttpRequestMessage req, string? apiKey)
    {
        if (!string.IsNullOrWhiteSpace(apiKey))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    // ── Bulk health check ─────────────────────────────────────────────────────

    /// <summary>Returns the cached health snapshot without making any network calls.</summary>
    public static ConnectionsHealthPayload GetCachedHealth(
        IReadOnlyList<SplaConnectionSection> connections,
        ConcurrentDictionary<string, (bool Ok, string? Error)> cache) => new()
    {
        Statuses = connections.Select(c =>
        {
            var found = cache.TryGetValue(c.Id, out var h);
            return new ConnectionHealthDto { Id = c.Id, Ok = found ? h.Ok : null, Error = found ? h.Error : null };
        }).ToList()
    };

    /// <summary>Pings all connections in parallel, updates the cache, and returns the snapshot.
    /// Caller should fire-and-forget via Task.Run to avoid blocking the message loop.</summary>
    public static async Task<ConnectionsHealthPayload> PingAllAsync(
        IReadOnlyList<SplaConnectionSection> connections,
        ConcurrentDictionary<string, (bool Ok, string? Error)> cache)
    {
        var tasks = connections.Select(async c =>
        {
            var result = await PingAsync(new ConnectionDiagRequest
                { Id = c.Id, Endpoint = c.Endpoint, ApiKey = c.ApiKey, Provider = c.Provider });
            cache[c.Id] = (result.Ok, result.Error);
            return new ConnectionHealthDto { Id = c.Id, Ok = result.Ok, Error = result.Error };
        });
        return new() { Statuses = [.. await Task.WhenAll(tasks)] };
    }

    // ── Ping ──────────────────────────────────────────────────────────────────

    public static async Task<ConnectionPingResultPayload> PingAsync(ConnectionDiagRequest? req)
    {
        var id = req?.Id;
        if (string.IsNullOrWhiteSpace(req?.Endpoint))
            return new() { Id = id, Ok = false, Error = "No endpoint configured" };

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, ModelsUrl(req.Endpoint));
            AddAuth(request, req.ApiKey);
            using var response = await Http.SendAsync(request, cts.Token);
            return new()
            {
                Id = id,
                Ok = response.IsSuccessStatusCode,
                Error = response.IsSuccessStatusCode ? null : $"{(int)response.StatusCode} {response.ReasonPhrase}"
            };
        }
        catch (OperationCanceledException)
        {
            return new() { Id = id, Ok = false, Error = "Timeout (6 s)" };
        }
        catch (Exception ex)
        {
            return new() { Id = id, Ok = false, Error = ex.Message };
        }
    }

    // ── Model list ────────────────────────────────────────────────────────────

    public static async Task<ConnectionModelsResultPayload> GetModelsAsync(ConnectionDiagRequest? req)
    {
        var id = req?.Id;
        if (string.IsNullOrWhiteSpace(req?.Endpoint))
            return new() { Id = id, Error = "No endpoint configured" };

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, ModelsUrl(req.Endpoint));
            AddAuth(request, req.ApiKey);
            using var response = await Http.SendAsync(request, cts.Token);
            if (!response.IsSuccessStatusCode)
                return new() { Id = id, Error = $"{(int)response.StatusCode} {response.ReasonPhrase}" };

            var json = await response.Content.ReadAsStringAsync(cts.Token);
            using var doc = JsonDocument.Parse(json);
            var models = new List<string>();

            // OpenAI-compat returns { "data": [...] }; some providers use { "models": [...] }
            if (!doc.RootElement.TryGetProperty("data", out var arr) || arr.ValueKind != JsonValueKind.Array)
                doc.RootElement.TryGetProperty("models", out arr);

            if (arr.ValueKind == JsonValueKind.Array)
                foreach (var item in arr.EnumerateArray())
                {
                    var mid = item.TryGetProperty("id", out var el) ? el.GetString()
                            : item.TryGetProperty("key", out el) ? el.GetString() : null;
                    if (!string.IsNullOrEmpty(mid)) models.Add(mid);
                }

            return new() { Id = id, Models = models };
        }
        catch (OperationCanceledException)
        {
            return new() { Id = id, Error = "Timeout (10 s)" };
        }
        catch (Exception ex)
        {
            return new() { Id = id, Error = ex.Message };
        }
    }

    // ── Test chat ─────────────────────────────────────────────────────────────

    public static async Task<ConnectionTestResultPayload> TestChatAsync(ConnectionDiagRequest? req)
    {
        var id = req?.Id;
        if (string.IsNullOrWhiteSpace(req?.Endpoint))
            return new() { Id = id, Error = "No endpoint configured" };

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            var body = JsonSerializer.Serialize(new
            {
                model = req.Model ?? "",
                messages = new[] { new { role = "user", content = "Hi. Скажи кратко кто ты" } },
                max_tokens = 300,
                stream = false
            }, Json);

            using var request = new HttpRequestMessage(HttpMethod.Post, ChatUrl(req.Endpoint))
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            AddAuth(request, req.ApiKey);

            using var response = await Http.SendAsync(request, cts.Token);
            var responseJson = await response.Content.ReadAsStringAsync(cts.Token);
            if (!response.IsSuccessStatusCode)
                return new() { Id = id, Error = $"{(int)response.StatusCode}: {responseJson[..Math.Min(200, responseJson.Length)]}" };

            using var doc = JsonDocument.Parse(responseJson);
            var reply = "";
            if (doc.RootElement.TryGetProperty("choices", out var choices)
                && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("message", out var msg)
                    && msg.TryGetProperty("content", out var content))
                    reply = content.GetString() ?? "";
            }
            return new() { Id = id, Reply = reply };
        }
        catch (OperationCanceledException)
        {
            return new() { Id = id, Error = "Timeout (30 s)" };
        }
        catch (Exception ex)
        {
            return new() { Id = id, Error = ex.Message };
        }
    }
}
