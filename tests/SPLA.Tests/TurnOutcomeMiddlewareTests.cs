using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Llm;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Llm.Middleware;
using SPLA.Agent.Accounting;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;

namespace SPLA.Tests;

public class TurnOutcomeMiddlewareTests
{
    private static LlmTurnContext Context() => new()
    {
        Messages = [new ChatMessage { Role = ChatRole.User, Content = "hi" }],
        Settings = new LLMSettings { BaseUrl = "http://localhost/v1", ModelName = "m" }
    };

    private static LlmTurnResult Answer() => new()
    {
        Message = new ChatMessage { Role = ChatRole.Assistant, Content = "hello" },
        ModelReported = "qwen/qwen3.8-27b",
        RawUsage = new Dictionary<string, long> { ["prompt_tokens"] = 10, ["completion_tokens"] = 2 },
        Signals =
        [
            new ProviderFact { Key = "reasoning.wire", Label = "Reasoning", Value = "reasoning_effort=medium", Scope = ProviderFactScope.Call },
            new ProviderFact { Key = "ratelimit.remaining", Label = "Requests left", Value = "42" }
        ]
    };

    private static Task<LlmTurnResult> Run(LlmTurnResult result) =>
        new TurnOutcomeMiddleware().InvokeAsync(Context(), (_, _) => Task.FromResult(result), CancellationToken.None);

    [Fact]
    public async Task Provider_signals_survive_the_accounting_stage()
    {
        // The regression: this stage used to rebuild the result field by field, which silently dropped
        // everything it did not itself set — so rate-limit budget and the reasoning-on-the-wire fact
        // never reached anyone, and nothing failed to say so.
        var outcome = await Run(Answer());

        Assert.Equal(2, outcome.Signals.Count);
        Assert.Contains(outcome.Signals, f => f.Key == "reasoning.wire" && f.Value == "reasoning_effort=medium");
        Assert.Contains(outcome.Signals, f => f.Key == "ratelimit.remaining");
    }

    [Fact]
    public async Task The_answer_and_the_reported_model_are_carried_through()
    {
        var outcome = await Run(Answer());

        Assert.Equal("hello", outcome.Message.Content);
        Assert.Equal("qwen/qwen3.8-27b", outcome.ModelReported);
        Assert.Equal(10, outcome.RawUsage["prompt_tokens"]);
    }

    [Fact]
    public async Task A_turn_that_reported_no_usage_is_marked_missing_not_zero()
    {
        var silent = new LlmTurnResult
        {
            Message = new ChatMessage { Role = ChatRole.Assistant, Content = "hello" },
            Signals = [new ProviderFact { Key = "reasoning.wire", Label = "Reasoning", Value = "(nothing sent)", Scope = ProviderFactScope.Call }]
        };

        var outcome = await Run(silent);

        Assert.Equal(LlmTurnStatus.UsageMissing, outcome.Status);
        Assert.Single(outcome.Signals);
    }

    [Fact]
    public async Task The_duration_is_stamped_here()
    {
        var outcome = await Run(Answer());

        Assert.True(outcome.Duration >= TimeSpan.Zero);
        Assert.NotEqual(default, outcome.Duration);
    }
}

public class ProviderStateMiddlewareTests
{
    private static LlmTurnContext Context() => new()
    {
        Messages = [new ChatMessage { Role = ChatRole.User, Content = "hi" }],
        Settings = new LLMSettings { BaseUrl = "http://localhost/v1", ModelName = "m", ConnectionId = "conn" }
    };

    private static async Task<ProviderStateStore> Run(params ProviderFact[] signals)
    {
        var store = new ProviderStateStore();
        var result = new LlmTurnResult
        {
            Message = new ChatMessage { Role = ChatRole.Assistant, Content = "hello" },
            Signals = signals
        };
        await new ProviderStateMiddleware(store).InvokeAsync(Context(), (_, _) => Task.FromResult(result), CancellationToken.None);
        return store;
    }

    private static ProviderFact Budget(string value) =>
        new() { Key = "ratelimit.remaining", Label = "Requests left", Value = value };

    private static ProviderFact CallFact() =>
        new() { Key = "reasoning.wire", Label = "Reasoning", Value = "(nothing sent)", Scope = ProviderFactScope.Call };

    [Fact]
    public async Task The_key_standing_is_stored()
    {
        var store = await Run(Budget("42"));

        Assert.Equal("42", Assert.Single(store.Get("conn")).Value);
    }

    [Fact]
    public async Task A_fact_about_one_call_is_not_the_connections_state()
    {
        var store = await Run(CallFact());

        Assert.Empty(store.Get("conn"));
    }

    [Fact]
    public async Task A_call_scoped_fact_cannot_erase_the_last_budget_reading()
    {
        // The trap: the store keeps the latest list per connection, and a local provider sends no
        // rate-limit headers at all. Were per-call facts stored, every ordinary turn would overwrite
        // the last real reading with something that was never about the key.
        var store = new ProviderStateStore();
        store.Record("conn", [Budget("42")]);

        var result = new LlmTurnResult
        {
            Message = new ChatMessage { Role = ChatRole.Assistant, Content = "hello" },
            Signals = [CallFact()]
        };
        await new ProviderStateMiddleware(store).InvokeAsync(Context(), (_, _) => Task.FromResult(result), CancellationToken.None);

        Assert.Equal("42", Assert.Single(store.Get("conn")).Value);
    }
}

public class TokenAccountingMiddlewareTests
{
    private sealed class FakeStore : ITokenUsageStore
    {
        public TokenUsageTotals Total { get; } = new();
        public TokenUsageTotals Session { get; } = new();
        public event EventHandler? Changed;
        public void Record(int? promptTokens, int? completionTokens)
        {
            Total.Add(promptTokens, completionTokens);
            Session.Add(promptTokens, completionTokens);
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    private static LlmTurnContext Context() => new()
    {
        Messages = [new ChatMessage { Role = ChatRole.User, Content = "hi" }],
        Settings = new LLMSettings { BaseUrl = "http://localhost/v1", ModelName = "m" }
    };

    private static LlmTurnResult Answer(int? prompt, int? completion) => new()
    {
        Message = new ChatMessage
        {
            Role = ChatRole.Assistant,
            Content = "hello",
            PromptTokens = prompt,
            CompletionTokens = completion
        }
    };

    private static async Task<FakeStore> Run(params LlmTurnResult[] calls)
    {
        var store = new FakeStore();
        var middleware = new TokenAccountingMiddleware(store);
        foreach (var call in calls)
            await middleware.InvokeAsync(Context(), (_, _) => Task.FromResult(call), CancellationToken.None);
        return store;
    }

    [Fact]
    public async Task A_call_is_recorded_without_anyone_subscribing_to_anything()
    {
        // The point of moving this into the pipeline: a caller that wires no callbacks at all — a
        // spawned sub-agent, the librarian — is counted anyway. It used to be counted by whichever
        // host remembered to, and sub-agents were counted by nobody.
        var store = await Run(Answer(100, 20));

        Assert.Equal(100, store.Total.PromptTokens);
        Assert.Equal(20, store.Total.CompletionTokens);
        Assert.Equal(1, store.Total.Turns);
    }

    [Fact]
    public async Task Every_network_attempt_is_its_own_row()
    {
        // Accounting sits inside Retry and Output, so a regenerated answer passes through twice and
        // must be counted twice — both attempts were paid for.
        var store = await Run(Answer(100, 20), Answer(100, 5));

        Assert.Equal(200, store.Total.PromptTokens);
        Assert.Equal(25, store.Total.CompletionTokens);
        Assert.Equal(2, store.Total.Turns);
    }

    [Fact]
    public async Task A_provider_that_reports_nothing_adds_no_turn()
    {
        var store = await Run(Answer(null, null));

        Assert.Equal(0, store.Total.Turns);
        Assert.Equal(0, store.Total.TotalTokens);
    }

    [Fact]
    public async Task Both_tallies_are_fed_from_the_one_place()
    {
        var project = new FakeStore();
        var machine = new FakeStore();
        await new TokenAccountingMiddleware(project, machine)
            .InvokeAsync(Context(), (_, _) => Task.FromResult(Answer(7, 3)), CancellationToken.None);

        Assert.Equal(10, project.Total.TotalTokens);
        Assert.Equal(10, machine.Total.TotalTokens);
    }
}
