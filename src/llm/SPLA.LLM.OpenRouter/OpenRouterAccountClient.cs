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
/// <item><c>GET /api/v1/credits</c> — needs a management key (OpenRouter's older name for it was
/// "provisioning key"; the UI calls it Management API key and issues it at
/// <c>openrouter.ai/settings/management-keys</c>). Anything else gets
/// <c>403 Only management keys can perform this operation</c>. Account-wide credits purchased and
/// used. Skipped entirely when no admin key is configured, which is the normal case.</item>
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

        sections.Add(new ProviderFactSection
        {
            Title = "This key", Facts = await ReadKeyAsync(endpoint, apiKey, ct), DeepLink = DeepLink
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
                // Points at where the missing credential is issued rather than at the balance page the
                // user cannot read yet — the link is only useful if it lands on the thing to fix.
                DeepLink = "https://openrouter.ai/settings/management-keys",
                Facts =
                [
                    new ProviderFact
                    {
                        Key = "credits.unavailable",
                        Label = "Balance",
                        // Named as OpenRouter's UI names it today: "provisioning key" was their old
                        // term and no longer appears anywhere the user could go looking for it.
                        Value = "needs a management key",
                        Kind = ProviderFactKind.Text
                    }
                ]
            });
        }

        return sections;
    }

    private async Task<List<ProviderFact>> ReadKeyAsync(string endpoint, string? apiKey, CancellationToken ct)
    {
        var (root, error) = await GetJsonAsync(endpoint, "key", apiKey, ct);
        var now = DateTimeOffset.UtcNow;

        // A failed probe used to produce an empty list, which the panel rendered exactly like a key
        // that genuinely reports nothing. Say which one it was.
        if (root is not { } data)
            return
            [
                new ProviderFact
                {
                    Key = "key.unavailable", Label = "Key info", Value = error ?? "unavailable",
                    Kind = ProviderFactKind.Text, Severity = ProviderFactSeverity.Warn, ObservedAt = now
                }
            ];

        if (data.TryGetProperty("data", out var inner)) data = inner;

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

        // `limit: null` is OpenRouter saying "no cap on this key" — a real answer. Omitting the row
        // made an uncapped key look like a key we failed to read.
        facts.Add(limit is { } l
            ? new ProviderFact
            {
                Key = "key.limit", Label = "Key limit", Value = l.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture),
                Unit = "USD", Kind = ProviderFactKind.Money, ObservedAt = now
            }
            : new ProviderFact
            {
                Key = "key.limit", Label = "Key limit", Value = "uncapped",
                Kind = ProviderFactKind.Text, ObservedAt = now
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

        // Arrives in the same response and was being dropped. It is the figure that actually explains
        // a 429, which is more use than a spend total that stays at zero on free models.
        if (data.TryGetProperty("rate_limit", out var rl) && rl.ValueKind == JsonValueKind.Object)
        {
            var requests = Number(rl, "requests");
            var interval = rl.TryGetProperty("interval", out var iv) && iv.ValueKind == JsonValueKind.String
                ? iv.GetString() : null;
            if (requests is { } rq)
                facts.Add(new ProviderFact
                {
                    Key = "key.rate_limit", Label = "Rate limit",
                    Value = interval is { Length: > 0 }
                        ? $"{rq:0.##} req / {interval}"
                        : $"{rq:0.##} req",
                    Kind = ProviderFactKind.Text, ObservedAt = now
                });
        }

        // `label` is deliberately not surfaced: OpenRouter defaults it to a truncated form of the key
        // itself, and this panel must not leak credential material.

        return facts;
    }

    private async Task<List<ProviderFact>> ReadCreditsAsync(string endpoint, string? adminKey, CancellationToken ct)
    {
        var (root, error) = await GetJsonAsync(endpoint, "credits", adminKey, ct);
        if (root is not { } data)
            return
            [
                new ProviderFact
                {
                    Key = "credits.unavailable", Label = "Balance", Value = error ?? "unavailable",
                    Kind = ProviderFactKind.Text, Severity = ProviderFactSeverity.Warn,
                    ObservedAt = DateTimeOffset.UtcNow
                }
            ];
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
    /// One GET. An account panel is strictly informational: a provider that is down, a key without the
    /// right scope, or a field that moved must degrade rather than take down the settings screen that
    /// would let the user fix it. It degrades to a <i>reason</i>, not to silence — "no data" and "the
    /// call failed" look identical once the row is gone, and only one of them is the user's to fix.
    /// </summary>
    private async Task<(JsonElement? Data, string? Error)> GetJsonAsync(
        string endpoint, string path, string? key, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key)) return (null, "no key configured");
        try
        {
            var baseUrl = endpoint.EndsWith('/') ? endpoint : endpoint + "/";
            using var req = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(baseUrl), path));
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);

            using var res = await _http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
                return (null, (int)res.StatusCode switch
                {
                    401 or 403 => $"key rejected ({(int)res.StatusCode})",
                    404 => "endpoint has no /" + path,
                    _ => $"HTTP {(int)res.StatusCode}"
                });

            using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
            return (doc.RootElement.Clone(), null);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { return (null, ex.Message); }
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
