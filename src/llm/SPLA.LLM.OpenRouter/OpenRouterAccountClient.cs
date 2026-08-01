using System.Net.Http.Headers;
using System.Text.Json;
using SPLA.Domain.Llm;

namespace SPLA.LLM.OpenRouter;

/// <summary>
/// Reads OpenRouter's account-level figures. This is the half of a provider that has nothing to do
/// with running a turn, and the reason a provider is a project rather than a dialect knob.
/// <para>
/// Two endpoints, two different credentials, and the split is OpenRouter's, not ours:
/// </para>
/// <list type="bullet">
/// <item><c>GET /api/v1/key</c> — works with the ordinary inference key. Per-key spend limit, what is
/// left of it, and usage over day/week/month. This is most of the value, and it costs nothing to
/// ask for.</item>
/// <item><c>GET /api/v1/credits</c> — needs a provisioning ("management") key. Account-wide credits
/// purchased and used. Skipped entirely when no admin key is configured, which is the normal case.</item>
/// </list>
/// <para>
/// No attempt is made to reconcile the two. Two keys of one account share a balance but have separate
/// limits, and nothing in our config can know two keys belong together — so each connection reports
/// what its own credential returned, labelled as such, rather than us inventing an account model to
/// make the numbers look tidy.
/// </para>
/// </summary>
public sealed class OpenRouterAccountClient : IProviderAccountInfo
{
    private const string DeepLink = "https://openrouter.ai/activity";

    private readonly HttpClient _http;

    public OpenRouterAccountClient(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<ProviderFactSection>> GetAccountInfoAsync(
        string endpoint, string? apiKey, string? adminKey, CancellationToken ct = default)
    {
        var sections = new List<ProviderFactSection>();

        var keyFacts = await ReadKeyAsync(endpoint, apiKey, ct);
        if (keyFacts.Count > 0)
            sections.Add(new ProviderFactSection
            {
                Title = "This key", Facts = keyFacts, DeepLink = DeepLink
            });

        if (!string.IsNullOrWhiteSpace(adminKey))
        {
            var creditFacts = await ReadCreditsAsync(endpoint, adminKey, ct);
            if (creditFacts.Count > 0)
                sections.Add(new ProviderFactSection
                {
                    Title = "Account credits", Facts = creditFacts, DeepLink = "https://openrouter.ai/credits"
                });
        }
        else
        {
            sections.Add(new ProviderFactSection
            {
                Title = "Account credits",
                DeepLink = "https://openrouter.ai/credits",
                Facts =
                [
                    new ProviderFact
                    {
                        Key = "credits.unavailable",
                        Label = "Balance",
                        Value = "needs a provisioning key",
                        Kind = ProviderFactKind.Text
                    }
                ]
            });
        }

        return sections;
    }

    private async Task<List<ProviderFact>> ReadKeyAsync(string endpoint, string? apiKey, CancellationToken ct)
    {
        var root = await GetJsonAsync(endpoint, "key", apiKey, ct);
        if (root is not { } data) return [];
        if (data.TryGetProperty("data", out var inner)) data = inner;

        var now = DateTimeOffset.UtcNow;
        var facts = new List<ProviderFact>();

        var limit = Number(data, "limit");
        var remaining = Number(data, "limit_remaining");

        if (remaining is { } left)
        {
            // A key with a cap is the case worth warning about; an uncapped key never goes critical.
            var severity = limit is { } cap && cap > 0
                ? (left / cap) switch { < 0.05m => ProviderFactSeverity.Critical, < 0.20m => ProviderFactSeverity.Warn, _ => ProviderFactSeverity.Normal }
                : ProviderFactSeverity.Normal;

            facts.Add(new ProviderFact
            {
                Key = "key.limit_remaining", Label = "Key limit left", Value = left.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture),
                Unit = "USD", Kind = ProviderFactKind.Money, Severity = severity, ObservedAt = now
            });
        }

        if (limit is { } l)
            facts.Add(new ProviderFact
            {
                Key = "key.limit", Label = "Key limit", Value = l.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture),
                Unit = "USD", Kind = ProviderFactKind.Money, ObservedAt = now
            });

        AddMoney(facts, data, "usage", "key.usage", "Used (all time)", now);
        AddMoney(facts, data, "usage_daily", "key.usage_daily", "Used today", now);
        AddMoney(facts, data, "usage_weekly", "key.usage_weekly", "Used this week", now);
        AddMoney(facts, data, "usage_monthly", "key.usage_monthly", "Used this month", now);

        if (data.TryGetProperty("is_free_tier", out var free) &&
            free.ValueKind is JsonValueKind.True or JsonValueKind.False)
            facts.Add(new ProviderFact
            {
                Key = "key.free_tier", Label = "Tier",
                Value = free.GetBoolean() ? "free" : "paid",
                Kind = ProviderFactKind.Text, ObservedAt = now
            });

        return facts;
    }

    private async Task<List<ProviderFact>> ReadCreditsAsync(string endpoint, string? adminKey, CancellationToken ct)
    {
        var root = await GetJsonAsync(endpoint, "credits", adminKey, ct);
        if (root is not { } data) return [];
        if (data.TryGetProperty("data", out var inner)) data = inner;

        var now = DateTimeOffset.UtcNow;
        var facts = new List<ProviderFact>();

        var total = Number(data, "total_credits");
        var used = Number(data, "total_usage");

        // OpenRouter reports the two totals, not the difference. Deriving it here is arithmetic on
        // reported facts, not a guess — but both inputs are kept so a surprising balance can be traced.
        if (total is { } t && used is { } u)
            facts.Add(new ProviderFact
            {
                Key = "credits.balance", Label = "Balance", Value = (t - u).ToString("0.####", System.Globalization.CultureInfo.InvariantCulture),
                Unit = "USD", Kind = ProviderFactKind.Money, ObservedAt = now,
                Severity = (t - u) switch
                {
                    <= 0 => ProviderFactSeverity.Critical,
                    < 1 => ProviderFactSeverity.Warn,
                    _ => ProviderFactSeverity.Normal
                }
            });

        if (total is { } tt)
            facts.Add(new ProviderFact { Key = "credits.total", Label = "Purchased", Value = tt.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture), Unit = "USD", Kind = ProviderFactKind.Money, ObservedAt = now });
        if (used is { } uu)
            facts.Add(new ProviderFact { Key = "credits.used", Label = "Used", Value = uu.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture), Unit = "USD", Kind = ProviderFactKind.Money, ObservedAt = now });

        return facts;
    }

    /// <summary>
    /// One GET, returning null on anything that goes wrong. An account panel is strictly informational:
    /// a provider that is down, a key without the right scope, or a field that moved must degrade to
    /// "nothing to show" rather than take down the settings screen that would let the user fix it.
    /// </summary>
    private async Task<JsonElement?> GetJsonAsync(string endpoint, string path, string? key, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        try
        {
            var baseUrl = endpoint.EndsWith('/') ? endpoint : endpoint + "/";
            using var req = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(baseUrl), path));
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);

            using var res = await _http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode) return null;

            using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
            return doc.RootElement.Clone();
        }
        catch (OperationCanceledException) { throw; }
        catch { return null; }
    }

    private static void AddMoney(List<ProviderFact> into, JsonElement data, string field, string key, string label, DateTimeOffset now)
    {
        if (Number(data, field) is { } v)
            into.Add(new ProviderFact
            {
                Key = key, Label = label, Value = v.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture),
                Unit = "USD", Kind = ProviderFactKind.Money, ObservedAt = now
            });
    }

    private static decimal? Number(JsonElement data, string field)
        => data.TryGetProperty(field, out var el) && el.ValueKind == JsonValueKind.Number && el.TryGetDecimal(out var v)
            ? v
            : null;
}
