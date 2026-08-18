using System.Net;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.LLM.LMStudio;
using SPLA.LLM.OpenAiCompat;
using SPLA.LLM.OpenRouter;

namespace SPLA.Tests;

/// <summary>
/// The reasoning lever, end to end minus the socket: what a provider advertises, how that becomes a
/// capability, and what a chosen value turns into on the wire.
/// <para>
/// Every fixture here is a real response, captured from the running providers on 2026-08-17, because
/// the whole design rests on the fact that nobody agrees on this parameter and no invented example
/// would show that. LM Studio reports Qwen3.8 as a switch and an effort scale in ONE list, with an
/// "xhigh" and no "high"; its server then accepts a different vocabulary from the one the model
/// advertises ("none", not "off"); OpenRouter publishes a structured descriptor instead; and LocalAI
/// says nothing at all while accepting every field it is sent.
/// </para>
/// </summary>
public class ReasoningLeverTests
{
    // ── The scalar grammar ────────────────────────────────────────────────────

    [Theory]
    [InlineData(null, ReasoningMode.Default)]
    [InlineData("", ReasoningMode.Default)]
    [InlineData("  ", ReasoningMode.Default)]
    [InlineData("off", ReasoningMode.Off)]
    [InlineData("none", ReasoningMode.Off)]
    [InlineData("on", ReasoningMode.On)]
    [InlineData("high", ReasoningMode.Effort)]
    [InlineData("xhigh", ReasoningMode.Effort)]
    [InlineData("budget:12000", ReasoningMode.Budget)]
    public void Grammar_reads_every_shape(string? raw, ReasoningMode expected)
        => Assert.Equal(expected, ReasoningChoice.Parse(raw).Mode);

    [Fact]
    public void An_unknown_word_is_an_effort_word_kept_verbatim()
    {
        // The point of the open end: a provider inventing a level tomorrow needs no code change.
        var choice = ReasoningChoice.Parse("ultra");
        Assert.Equal(ReasoningMode.Effort, choice.Mode);
        Assert.Equal("ultra", choice.Effort);
        Assert.Equal("ultra", choice.ToString());
    }

    [Theory]
    [InlineData("off")]
    [InlineData("on")]
    [InlineData("medium")]
    [InlineData("budget:8000")]
    public void Grammar_round_trips(string raw)
        => Assert.Equal(raw, ReasoningChoice.Parse(raw).ToString());

    // ── LM Studio's flat option list ──────────────────────────────────────────

    [Fact]
    public void A_mixed_option_list_is_a_switch_and_a_scale_at_once()
    {
        // Verbatim from LM Studio 0.3.x for qwen/qwen3.8-27b. Note: no "high".
        var caps = ReasoningCapability.FromOptions(["off", "low", "medium", "xhigh", "on"], "xhigh");

        Assert.True(caps.Known);
        Assert.True(caps.Supported);
        Assert.True(caps.CanDisable);
        Assert.Equal(["low", "medium", "xhigh"], caps.Efforts);
        Assert.Equal("xhigh", caps.DefaultEffort);
        Assert.DoesNotContain("high", caps.Efforts);
    }

    [Fact]
    public void A_toggle_model_has_no_scale()
    {
        var caps = ReasoningCapability.FromOptions(["off", "on"], "on");

        Assert.True(caps.CanDisable);
        Assert.True(caps.DefaultEnabled);
        Assert.Empty(caps.Efforts);
        Assert.True(caps.HasLever);
    }

    [Fact]
    public void A_model_that_only_offers_on_cannot_be_switched_off()
    {
        // mistralai/ministral-3-14b-reasoning: allowed_options ["on"].
        var caps = ReasoningCapability.FromOptions(["on"], "on");

        Assert.True(caps.Supported);
        Assert.True(caps.Mandatory);
        Assert.False(caps.CanDisable);
    }

    [Fact]
    public void An_effort_only_model_has_no_off()
    {
        // openai/gpt-oss-20b: ["low","medium","high"], default "low".
        var caps = ReasoningCapability.FromOptions(["low", "medium", "high"], "low");

        Assert.True(caps.Mandatory);
        Assert.Equal(["low", "medium", "high"], caps.Efforts);
        Assert.Equal("low", caps.DefaultEffort);
    }

    [Fact]
    public void No_options_is_unknown_not_absent()
    {
        // The distinction the whole design turns on: a provider saying nothing is not a model saying no.
        var caps = ReasoningCapability.FromOptions([], null);

        Assert.False(caps.Known);
        Assert.False(caps.Supported);

        // None is the other thing: a provider that looked and said "this model does not reason".
        Assert.True(ReasoningCapability.None.Known);
        Assert.False(ReasoningCapability.None.Supported);
    }

    // ── The wire, OpenAI dialect ──────────────────────────────────────────────

    private static Dictionary<string, object> Wire(string? level, ReasoningCapability caps, IOpenAiCompatProfile? profile = null)
    {
        var payload = new Dictionary<string, object>();
        (profile ?? PlainOpenAiCompatProfile.Instance).ShapeReasoning(payload, ReasoningChoice.Parse(level), caps);
        return payload;
    }

    [Fact]
    public void Off_travels_as_the_servers_word_not_the_models()
    {
        // LM Studio advertises "off" but its endpoint answers 400 to reasoning_effort:"off" —
        // "Supported values: none, minimal, low, medium, high, xhigh". The translation happens here
        // so nobody choosing an option has to know two vocabularies.
        var caps = ReasoningCapability.FromOptions(["off", "low", "medium", "xhigh", "on"], "xhigh");

        Assert.Equal("none", Wire("off", caps)["reasoning_effort"]);
    }

    [Fact]
    public void An_effort_word_goes_through_untouched()
    {
        var caps = ReasoningCapability.FromOptions(["off", "low", "medium", "xhigh", "on"], "xhigh");

        Assert.Equal("xhigh", Wire("xhigh", caps)["reasoning_effort"]);
    }

    [Fact]
    public void Nothing_is_sent_for_a_model_nobody_described()
    {
        // The measured failure this guards: LocalAI validates no field and forwards it to a model
        // whose template ignores it — reasoning_effort:"none" made Gemma-4 emit 600 tokens of
        // "0.5-0.5-0.5" instead of a one-word answer. A lever nobody advertised does not get pulled.
        Assert.Empty(Wire("off", ReasoningCapability.Unknown));
        Assert.Empty(Wire("high", ReasoningCapability.Unknown));
    }

    [Fact]
    public void Nothing_is_sent_when_no_choice_was_made()
        => Assert.Empty(Wire("", ReasoningCapability.FromOptions(["off", "on"], "on")));

    [Fact]
    public void Off_is_not_forced_on_a_model_that_cannot_be_silenced()
    {
        var caps = ReasoningCapability.FromOptions(["on"], "on");

        Assert.Empty(Wire("off", caps));
    }

    [Fact]
    public void On_says_nothing_to_a_model_that_already_reasons()
    {
        // Silence is the correct wire form here: the model's default IS on, so naming a depth would
        // override a default nobody chose to change.
        var caps = ReasoningCapability.FromOptions(["off", "on"], "on");

        Assert.Empty(Wire("on", caps));
    }

    // ── The wire, OpenRouter dialect ──────────────────────────────────────────

    [Fact]
    public void OpenRouter_uses_its_own_block()
    {
        var caps = new ReasoningCapability
        {
            Known = true, Supported = true, DefaultEnabled = true,
            Efforts = ["high", "medium"], DefaultEffort = "high", SupportsTokenBudget = true
        };
        var profile = new OpenRouterProfile();

        var effort = Wire("medium", caps, profile);
        var block = Assert.IsType<Dictionary<string, object?>>(effort["reasoning"]);
        Assert.Equal("medium", block["effort"]);
        Assert.False(effort.ContainsKey("reasoning_effort"));

        var off = Wire("off", caps, profile);
        Assert.Equal(false, Assert.IsType<Dictionary<string, object?>>(off["reasoning"])["enabled"]);
    }

    [Fact]
    public void A_token_budget_reaches_only_the_provider_that_takes_one()
    {
        var withBudget = new ReasoningCapability
        {
            Known = true, Supported = true, Efforts = ["high"], SupportsTokenBudget = true
        };
        var withoutBudget = ReasoningCapability.FromOptions(["off", "low", "medium"], "low");

        var block = Assert.IsType<Dictionary<string, object?>>(
            Wire("budget:12000", withBudget, new OpenRouterProfile())["reasoning"]);
        Assert.Equal(12000, block["max_tokens"]);

        // The OpenAI dialect has no field for it, so it is dropped rather than mistranslated.
        Assert.Empty(Wire("budget:12000", withoutBudget));
    }

    // ── Reading the providers ─────────────────────────────────────────────────

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(respond(request));
    }

    private static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
    };

    [Fact]
    public async Task LM_Studio_capabilities_become_a_capability()
    {
        const string body = """
            {
              "models": [
                {
                  "type": "llm",
                  "key": "qwen/qwen3.8-27b",
                  "display_name": "Qwen3.8 27B",
                  "capabilities": {
                    "vision": true,
                    "trained_for_tool_use": true,
                    "reasoning": { "allowed_options": ["off","low","medium","xhigh","on"], "default": "xhigh" }
                  }
                }
              ]
            }
            """;
        var client = new LMStudioManagementClient(new HttpClient(new StubHandler(_ => Json(body))));

        var models = await client.GetModelDetailsAsync("http://127.0.0.1:1234/v1/");
        var caps = models.Single().Reasoning;

        Assert.True(caps.Known);
        Assert.True(caps.CanDisable);
        Assert.Equal(["low", "medium", "xhigh"], caps.Efforts);
    }

    [Fact]
    public async Task A_described_model_without_a_reasoning_block_is_a_no_not_a_silence()
    {
        // LM Studio names the block for every model that has one, so its absence from a model it DID
        // describe is an answer: qwen3-coder-30b has no reasoning channel. The lever then disappears
        // rather than sitting there greyed out.
        const string body = """
            { "models": [ { "type": "llm", "key": "qwen/qwen3-coder-30b",
                            "capabilities": { "trained_for_tool_use": true } } ] }
            """;
        var client = new LMStudioManagementClient(new HttpClient(new StubHandler(_ => Json(body))));

        var caps = (await client.GetModelDetailsAsync("http://127.0.0.1:1234/v1/")).Single().Reasoning;

        Assert.True(caps.Known);
        Assert.False(caps.Supported);
    }

    [Fact]
    public async Task A_model_from_the_bare_id_list_stays_unknown()
    {
        // The OpenAI-compatible fallback describes nothing about anything — the one case where the
        // UI must say "we were not told" instead of either offering or hiding the lever.
        const string body = """{ "data": [ { "id": "some-model", "object": "model" } ] }""";
        var client = new LMStudioManagementClient(new HttpClient(new StubHandler(_ => Json(body))));

        var caps = (await client.GetModelDetailsAsync("http://127.0.0.1:1234/v1/")).Single().Reasoning;

        Assert.False(caps.Known);
        Assert.False(caps.Supported);
    }

    [Fact]
    public async Task OpenRouters_descriptor_carries_all_three_axes()
    {
        // Trimmed from the live catalog entry for nvidia/nemotron-3-ultra-550b-a55b.
        const string body = """
            {
              "data": [
                {
                  "id": "nvidia/nemotron-3-ultra-550b-a55b",
                  "name": "NVIDIA: Nemotron 3 Ultra",
                  "context_length": 512288,
                  "pricing": { "prompt": "0.0000006", "completion": "0.0000036" },
                  "supported_parameters": ["reasoning","reasoning_effort","temperature","tools"],
                  "reasoning": {
                    "mandatory": false,
                    "default_enabled": true,
                    "supports_max_tokens": true,
                    "supported_efforts": ["high","medium"],
                    "default_effort": "high"
                  }
                }
              ]
            }
            """;
        var catalog = new OpenRouterCatalogClient(new HttpClient(new StubHandler(_ => Json(body))));

        var model = (await catalog.GetModelsAsync("https://openrouter.ai/api/v1")).Single();

        Assert.True(model.Reasoning.Known);
        Assert.False(model.Reasoning.Mandatory);
        Assert.True(model.Reasoning.DefaultEnabled);
        Assert.True(model.Reasoning.SupportsTokenBudget);
        Assert.Equal(["high", "medium"], model.Reasoning.Efforts);
        Assert.Equal("high", model.Reasoning.DefaultEffort);
    }

    [Fact]
    public async Task An_openrouter_model_without_reasoning_says_so_positively()
    {
        const string body = """
            { "data": [ { "id": "some/plain-model", "supported_parameters": ["temperature","tools"] } ] }
            """;
        var catalog = new OpenRouterCatalogClient(new HttpClient(new StubHandler(_ => Json(body))));

        var caps = (await catalog.GetModelsAsync("https://openrouter.ai/api/v1")).Single().Reasoning;

        // OpenRouter describes every model, so absence there IS a statement — unlike silence from a
        // bare OpenAI-compatible endpoint.
        Assert.True(caps.Known);
        Assert.False(caps.Supported);
    }

    // ── Config's manual declaration ───────────────────────────────────────────

    [Fact]
    public void A_declared_option_list_stands_in_for_a_silent_provider()
    {
        var entry = new ResolvedModelEntry
        {
            Connection = new SplaConnectionSection { Id = "c", Provider = "openai-compat" },
            Entry = new SplaModelSection
            {
                Id = "m",
                Model = "my-vllm-model",
                ReasoningOptions = ["off", "on"],
                ReasoningDefault = "on"
            }
        };

        Assert.True(entry.DeclaredReasoning.Known);
        Assert.True(entry.DeclaredReasoning.CanDisable);

        var undeclared = new ResolvedModelEntry
        {
            Connection = new SplaConnectionSection { Id = "c" },
            Entry = new SplaModelSection { Id = "m", Model = "x" }
        };
        Assert.False(undeclared.DeclaredReasoning.Known);
    }
}
