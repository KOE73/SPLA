using System.Net;
using System.Text;
using SPLA.Domain.Llm;
using SPLA.LLM.OpenRouter;

namespace SPLA.Tests;

/// <summary>
/// The "i" popup is the only place a user ever learns why a key is behaving the way it is, so the rule
/// under test is not "parse JSON" but "never render silence": a value OpenRouter reports as absent, a
/// call that failed, and a call that was never made must each say which one they were.
/// </summary>
public class OpenRouterAccountInfoTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, (HttpStatusCode Code, string Body)> _routes;
        public StubHandler(Dictionary<string, (HttpStatusCode, string)> routes) => _routes = routes;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var path = request.RequestUri!.AbsolutePath.TrimEnd('/');
            var key = path[(path.LastIndexOf('/') + 1)..];
            if (!_routes.TryGetValue(key, out var r))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") });

            return Task.FromResult(new HttpResponseMessage(r.Code)
            {
                Content = new StringContent(r.Body, Encoding.UTF8, "application/json")
            });
        }
    }

    private static OpenRouterAccountClient Client(params (string Path, HttpStatusCode Code, string Body)[] routes)
        => new(new HttpClient(new StubHandler(routes.ToDictionary(r => r.Path, r => (r.Code, r.Body)))));

    private static ProviderFact? Find(IReadOnlyList<ProviderFactSection> sections, string key)
        => sections.SelectMany(s => s.Facts).FirstOrDefault(f => f.Key == key);

    // A real uncapped key: OpenRouter answers `limit: null`, which is an answer, not a failure.
    private const string UncappedKey = """
    {"data":{"limit":null,"usage":0,"limit_remaining":null,"is_free_tier":false,
             "rate_limit":{"requests":10,"interval":"10s"}}}
    """;

    [Fact]
    public async Task An_uncapped_key_says_uncapped_instead_of_dropping_the_row()
    {
        var sections = await Client(("key", HttpStatusCode.OK, UncappedKey))
            .GetAccountInfoAsync("https://openrouter.ai/api/v1", "sk-test", null);

        Assert.Equal("uncapped", Find(sections, "key.limit")?.Value);
    }

    [Fact]
    public async Task Rate_limit_is_reported_because_it_is_what_explains_a_429()
    {
        var sections = await Client(("key", HttpStatusCode.OK, UncappedKey))
            .GetAccountInfoAsync("https://openrouter.ai/api/v1", "sk-test", null);

        Assert.Equal("10 req / 10s", Find(sections, "key.rate_limit")?.Value);
    }

    // Zero spend on free models is a true reading and must survive as $0, not vanish.
    [Fact]
    public async Task Zero_usage_is_reported_as_zero()
    {
        var sections = await Client(("key", HttpStatusCode.OK,
                """{"data":{"usage":0,"usage_daily":0,"usage_weekly":0,"usage_monthly":0}}"""))
            .GetAccountInfoAsync("https://openrouter.ai/api/v1", "sk-test", null);

        Assert.Equal("0", Find(sections, "key.usage")?.Value);
        Assert.Equal("0", Find(sections, "key.usage_daily")?.Value);
    }

    [Fact]
    public async Task A_rejected_key_reports_the_rejection_rather_than_an_empty_section()
    {
        var sections = await Client(("key", HttpStatusCode.Unauthorized, "{}"))
            .GetAccountInfoAsync("https://openrouter.ai/api/v1", "sk-bad", null);

        var fact = Find(sections, "key.unavailable");
        Assert.NotNull(fact);
        Assert.Contains("401", fact!.Value);
        Assert.Equal(ProviderFactSeverity.Warn, fact.Severity);
    }

    // The distinction the panel exists to draw: no admin key is the user's to fix, a rejected admin key
    // is a different problem, and both used to render as nothing at all.
    [Fact]
    public async Task A_missing_admin_key_and_a_rejected_one_report_differently()
    {
        var withoutAdmin = await Client(("key", HttpStatusCode.OK, UncappedKey))
            .GetAccountInfoAsync("https://openrouter.ai/api/v1", "sk-test", null);
        Assert.Equal("needs a management key", Find(withoutAdmin, "credits.unavailable")?.Value);

        var withBadAdmin = await Client(
                ("key", HttpStatusCode.OK, UncappedKey),
                ("credits", HttpStatusCode.Forbidden, "{}"))
            .GetAccountInfoAsync("https://openrouter.ai/api/v1", "sk-test", "sk-admin-bad");
        Assert.Contains("403", Find(withBadAdmin, "credits.unavailable")!.Value);
    }

    [Fact]
    public async Task Balance_is_derived_from_the_two_totals_openrouter_reports()
    {
        var sections = await Client(
                ("key", HttpStatusCode.OK, UncappedKey),
                ("credits", HttpStatusCode.OK, """{"data":{"total_credits":25,"total_usage":4.5}}"""))
            .GetAccountInfoAsync("https://openrouter.ai/api/v1", "sk-test", "sk-admin");

        Assert.Equal("20.5", Find(sections, "credits.balance")?.Value);
    }

    // OpenRouter's `label` defaults to a truncated form of the key itself; this panel must never carry
    // credential material.
    [Fact]
    public async Task The_key_label_is_never_surfaced()
    {
        var sections = await Client(("key", HttpStatusCode.OK,
                """{"data":{"label":"sk-or-v1-abcdef","usage":0}}"""))
            .GetAccountInfoAsync("https://openrouter.ai/api/v1", "sk-test", null);

        Assert.DoesNotContain(sections.SelectMany(s => s.Facts), f => f.Value.Contains("sk-or-v1"));
    }
}
