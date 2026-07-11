using System.Net;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.LLM.LMStudio;

namespace SPLA.Tests;

/// <summary>
/// The operative context window must come from the LOADED instance's config (what requests actually
/// fail against), not the model's advertised maximum — e.g. gemma-4 loaded with 101120 of its 262144.
/// Detection order: connection override → LM Studio native loaded config → model max / vLLM
/// max_model_len → unknown.
/// </summary>
public class ContextLengthDetectionTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;
        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => _respond = respond;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(_respond(request));
    }

    private static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
    };

    // Shaped after the real LM Studio /api/v1/models response the feature was designed against.
    private const string NativeBody = """
        {
          "models": [
            {
              "type": "llm",
              "key": "google/gemma-4-12b-qat",
              "display_name": "Gemma 4 12B QAT",
              "architecture": "gemma4",
              "loaded_instances": [
                { "id": "google/gemma-4-12b-qat", "config": { "context_length": 101120 } }
              ],
              "max_context_length": 262144,
              "capabilities": { "vision": true, "trained_for_tool_use": true }
            },
            {
              "type": "llm",
              "key": "qwen/qwen3-coder-30b",
              "loaded_instances": [],
              "max_context_length": 32768
            }
          ]
        }
        """;

    [Fact]
    public async Task Loaded_instance_context_wins_over_model_max()
    {
        var client = new LMStudioManagementClient(new HttpClient(new StubHandler(req =>
            req.RequestUri!.AbsolutePath.StartsWith("/api/") ? Json(NativeBody) : Json("{\"data\":[]}"))));

        var models = await client.GetModelDetailsAsync("http://127.0.0.1:1234/v1/");
        var gemma = models.Single(m => m.Id == "google/gemma-4-12b-qat");

        Assert.Equal(101120, gemma.LoadedContextLength);
        Assert.Equal(262144, gemma.MaxContextLength);
        Assert.Equal(101120, gemma.EffectiveContextLength); // operative, not the max
        Assert.True(gemma.IsLoaded);

        var unloaded = models.Single(m => m.Id == "qwen/qwen3-coder-30b");
        Assert.Equal(0, unloaded.LoadedContextLength);
        Assert.Equal(32768, unloaded.EffectiveContextLength); // falls back to model max
    }

    [Fact]
    public async Task Vllm_max_model_len_is_parsed_from_openai_fallback()
    {
        // Native endpoint 404s (vLLM has no /api/v1); the OpenAI list carries max_model_len.
        var client = new LMStudioManagementClient(new HttpClient(new StubHandler(req =>
            req.RequestUri!.AbsolutePath.StartsWith("/api/")
                ? new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") }
                : Json("{\"data\":[{\"id\":\"meta-llama/Llama-3.1-8B\",\"object\":\"model\",\"max_model_len\":16384}]}"))));

        var models = await client.GetModelDetailsAsync("http://vllm-host:8000/v1/");
        var m = models.Single();
        Assert.Equal(16384, m.MaxContextLength);
        Assert.Equal(16384, m.EffectiveContextLength);
    }

    [Fact]
    public void Connection_context_length_override_flows_into_llm_settings()
    {
        var resolved = SettingsResolver.Resolve(null, new SplaProject
        {
            Connections = new List<SplaConnectionSection>
            {
                new() { Id = "main", Endpoint = "http://127.0.0.1:1234/v1/", Model = "auto", ContextLength = 50000 }
            }
        });

        var llm = resolved.ToLLMSettings();
        Assert.Equal(50000, llm.ContextLength);
    }

    [Fact]
    public void Missing_override_leaves_context_length_null_for_autodetect()
    {
        var resolved = SettingsResolver.Resolve(null, new SplaProject());
        Assert.Null(resolved.ToLLMSettings().ContextLength);
    }
}
