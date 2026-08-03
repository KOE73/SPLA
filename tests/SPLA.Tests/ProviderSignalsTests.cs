using System.Net;
using System.Net.Http.Headers;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Llm;
using SPLA.Domain.Llm.Middleware;
using SPLA.Domain.Models;
using SPLA.Domain.Secrets;
using SPLA.Domain.Settings;
using SPLA.LLM.OpenAiCompat;

namespace SPLA.Tests;

public class RateLimitSignalsTests
{
    private static HttpResponseHeaders Headers(params (string Name, string Value)[] items)
    {
        var res = new HttpResponseMessage(HttpStatusCode.OK);
        foreach (var (name, value) in items) res.Headers.TryAddWithoutValidation(name, value);
        return res.Headers;
    }

    [Fact]
    public void No_rate_limit_headers_yields_no_facts()
        => Assert.Empty(RateLimitSignals.From(Headers()));

    [Fact]
    public void Remaining_and_limit_are_reported()
    {
        var facts = RateLimitSignals.From(Headers(
            ("X-RateLimit-Limit", "200"), ("X-RateLimit-Remaining", "150")));

        Assert.Equal("150", facts.Single(f => f.Key == "ratelimit.remaining").Value);
        Assert.Equal("200", facts.Single(f => f.Key == "ratelimit.limit").Value);
        Assert.Equal(ProviderFactSeverity.Normal, facts.Single(f => f.Key == "ratelimit.remaining").Severity);
    }

    [Theory]
    [InlineData("100", "15", ProviderFactSeverity.Warn)]       // 15% left
    [InlineData("100", "2", ProviderFactSeverity.Critical)]    // 2% left
    [InlineData("100", "80", ProviderFactSeverity.Normal)]
    public void Severity_is_scaled_against_the_limit(string limit, string remaining, ProviderFactSeverity expected)
    {
        var facts = RateLimitSignals.From(Headers(
            ("X-RateLimit-Limit", limit), ("X-RateLimit-Remaining", remaining)));

        Assert.Equal(expected, facts.Single(f => f.Key == "ratelimit.remaining").Severity);
    }

    [Fact]
    public void Exhausted_budget_is_critical_even_without_a_limit_header()
    {
        var facts = RateLimitSignals.From(Headers(("X-RateLimit-Remaining", "0")));
        Assert.Equal(ProviderFactSeverity.Critical, facts.Single(f => f.Key == "ratelimit.remaining").Severity);
    }

    // Providers disagree: OpenRouter sends milliseconds since the epoch, others send seconds, others a
    // plain delta. Getting this wrong silently shows a reset in 1970 or in the year 55000.
    [Fact]
    public void Reset_accepts_millisecond_epoch()
    {
        var when = DateTimeOffset.UtcNow.AddMinutes(5);
        var facts = RateLimitSignals.From(Headers(
            ("X-RateLimit-Reset", when.ToUnixTimeMilliseconds().ToString())));

        var reset = facts.Single(f => f.Key == "ratelimit.reset").ResetsAt;
        Assert.NotNull(reset);
        Assert.True(Math.Abs((reset!.Value - when).TotalSeconds) < 2);
    }

    [Fact]
    public void Reset_accepts_second_epoch()
    {
        var when = DateTimeOffset.UtcNow.AddMinutes(5);
        var facts = RateLimitSignals.From(Headers(
            ("X-RateLimit-Reset", when.ToUnixTimeSeconds().ToString())));

        var reset = facts.Single(f => f.Key == "ratelimit.reset").ResetsAt;
        Assert.True(Math.Abs((reset!.Value - when).TotalSeconds) < 2);
    }

    [Fact]
    public void Reset_accepts_a_relative_delta()
    {
        var facts = RateLimitSignals.From(Headers(("X-RateLimit-Reset", "60")));
        var reset = facts.Single(f => f.Key == "ratelimit.reset").ResetsAt;
        Assert.True(Math.Abs((reset!.Value - DateTimeOffset.UtcNow.AddSeconds(60)).TotalSeconds) < 2);
    }

    [Fact]
    public void Retry_after_is_reported_as_a_duration()
    {
        var facts = RateLimitSignals.From(Headers(("Retry-After", "30")));
        var fact = facts.Single(f => f.Key == "ratelimit.retry_after");
        Assert.Equal(ProviderFactKind.Duration, fact.Kind);
        Assert.Equal("30", fact.Value);
    }
}

public class ProviderStateStoreTests
{
    private static ProviderFact Fact(string value) => new()
        { Key = "ratelimit.remaining", Label = "left", Value = value };

    [Fact]
    public void Unknown_connection_reads_empty()
        => Assert.Empty(new ProviderStateStore().Get("nope"));

    [Fact]
    public void Latest_observation_wins()
    {
        var store = new ProviderStateStore();
        store.Record("c1", [Fact("10")]);
        store.Record("c1", [Fact("9")]);

        Assert.Equal("9", store.Get("c1").Single().Value);
    }

    // A provider that sends no headers must not erase what an earlier response reported: "unknown"
    // and "nothing left" would otherwise look identical.
    [Fact]
    public void An_empty_observation_does_not_erase_the_last_known_state()
    {
        var store = new ProviderStateStore();
        store.Record("c1", [Fact("10")]);
        store.Record("c1", []);

        Assert.Equal("10", store.Get("c1").Single().Value);
    }

    [Fact]
    public void State_is_kept_per_connection()
    {
        var store = new ProviderStateStore();
        store.Record("work", [Fact("5")]);
        store.Record("home", [Fact("500")]);

        Assert.Equal("5", store.Get("work").Single().Value);
        Assert.Equal("500", store.Get("home").Single().Value);
    }
}

public class CredentialsMiddlewareTests
{
    private sealed class StubResolver : ISecretResolver
    {
        private readonly string? _value;
        private readonly Exception? _throw;
        public StubResolver(string? value = null, Exception? ex = null) { _value = value; _throw = ex; }

        public ValueTask<string?> ResolveAsync(string? reference, CancellationToken ct = default)
            => _throw != null ? throw _throw : ValueTask.FromResult(_value);
        public string? Resolve(string? reference) => _throw != null ? throw _throw : _value;
        public ValueTask<SecretEntry?> GetEntryAsync(string key, SecretScope scope, CancellationToken ct = default)
            => ValueTask.FromResult<SecretEntry?>(null);
    }

    private static LlmTurnContext Context(string apiKey) => new()
    {
        Messages = [],
        Settings = new LLMSettings { ApiKey = apiKey }
    };

    private static LlmTurnDelegate Capture(List<string?> seen) => (ctx, _) =>
    {
        seen.Add(ctx.Settings.ApiKey);
        return Task.FromResult(new LlmTurnResult { Message = new ChatMessage { Role = ChatRole.Assistant } });
    };

    [Fact]
    public async Task A_literal_key_is_passed_through_untouched()
    {
        var seen = new List<string?>();
        var ctx = Context("lm-studio");

        await new CredentialsMiddleware(new StubResolver("SHOULD-NOT-BE-USED"))
            .InvokeAsync(ctx, Capture(seen), default);

        Assert.Equal("lm-studio", seen.Single());
    }

    [Fact]
    public async Task A_reference_is_materialized_before_the_provider_call()
    {
        var seen = new List<string?>();
        var ctx = Context("secret:user:openrouter");

        await new CredentialsMiddleware(new StubResolver("sk-real-key"))
            .InvokeAsync(ctx, Capture(seen), default);

        Assert.Equal("sk-real-key", seen.Single());
    }

    [Fact]
    public async Task An_unresolvable_reference_fails_as_auth_rather_than_reaching_the_provider()
    {
        var seen = new List<string?>();

        var ex = await Assert.ThrowsAsync<LlmRequestException>(() =>
            new CredentialsMiddleware(new StubResolver(value: null))
                .InvokeAsync(Context("secret:user:missing"), Capture(seen), default));

        Assert.Equal(LlmErrorKind.AuthFailed, ex.Kind);
        Assert.Empty(seen);   // the provider was never called
    }

    [Fact]
    public async Task A_resolver_failure_is_reported_as_auth_not_as_a_raw_exception()
    {
        var ex = await Assert.ThrowsAsync<LlmRequestException>(() =>
            new CredentialsMiddleware(new StubResolver(ex: new UnauthorizedAccessException("denied")))
                .InvokeAsync(Context("secret:shared:locked"), Capture([]), default));

        Assert.Equal(LlmErrorKind.AuthFailed, ex.Kind);
    }

    // The middleware sits at Transport so it is INSIDE accounting: the ledger records the turn without
    // ever having had the key material.
    [Fact]
    public void Runs_innermost()
        => Assert.Equal(LlmPipelineStage.Transport, new CredentialsMiddleware(new StubResolver()).Stage);
}
