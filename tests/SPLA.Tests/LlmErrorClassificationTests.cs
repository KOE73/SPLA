using System.Net;
using SPLA.Domain.Interfaces;
using SPLA.LLM.LMStudio;

namespace SPLA.Tests;

/// <summary>Provider HTTP failures must collapse into one actionable <see cref="LlmErrorKind"/> — the
/// same context-window signal whether it arrives as an OpenAI/vLLM 400 with a JSON message or an LM
/// Studio 500 with an HTML page.</summary>
public class LlmErrorClassificationTests
{
    [Fact]
    public void OpenAI_style_context_400_is_ContextExhausted()
    {
        var body = "{\"error\":{\"message\":\"This model's maximum context length is 8192 tokens. However, your messages resulted in 9001 tokens.\",\"type\":\"invalid_request_error\",\"code\":\"context_length_exceeded\"}}";
        var ex = LMStudioClient.ClassifyHttpFailure(HttpStatusCode.BadRequest, "Bad Request", body);
        Assert.Equal(LlmErrorKind.ContextExhausted, ex.Kind);
        Assert.Contains("too long", ex.Message);
        Assert.DoesNotContain("<", ex.Message); // no raw markup leaked
    }

    [Fact]
    public void Vllm_style_context_400_is_ContextExhausted()
    {
        var body = "{\"object\":\"error\",\"message\":\"This model's maximum context length is 4096 tokens. However, you requested 5000 tokens.\",\"type\":\"BadRequestError\"}";
        var ex = LMStudioClient.ClassifyHttpFailure(HttpStatusCode.BadRequest, "Bad Request", body);
        Assert.Equal(LlmErrorKind.ContextExhausted, ex.Kind);
    }

    [Fact]
    public void LmStudio_html_500_is_ProviderError_with_context_hint()
    {
        var body = "<!DOCTYPE html><html><head><title>Error</title></head><body><pre>Internal Server Error</pre></body></html>";
        var ex = LMStudioClient.ClassifyHttpFailure(HttpStatusCode.InternalServerError, "Internal Server Error", body);
        Assert.Equal(LlmErrorKind.ProviderError, ex.Kind);
        Assert.Contains("context window", ex.Message);
        Assert.DoesNotContain("DOCTYPE", ex.Message); // HTML body not surfaced to the user
    }

    [Fact]
    public void Auth_401_is_AuthFailed()
    {
        var ex = LMStudioClient.ClassifyHttpFailure(HttpStatusCode.Unauthorized, "Unauthorized", "{\"error\":{\"message\":\"invalid api key\"}}");
        Assert.Equal(LlmErrorKind.AuthFailed, ex.Kind);
        Assert.Contains("API key", ex.Message);
    }

    [Fact]
    public void RateLimit_429_is_RateLimited()
    {
        var ex = LMStudioClient.ClassifyHttpFailure((HttpStatusCode)429, "Too Many Requests", "");
        Assert.Equal(LlmErrorKind.RateLimited, ex.Kind);
    }

    [Fact]
    public void ExtractProviderMessage_ignores_html_bodies()
    {
        Assert.Null(LMStudioClient.ExtractProviderMessage("<html>nope</html>"));
        Assert.Equal("boom", LMStudioClient.ExtractProviderMessage("{\"error\":{\"message\":\"boom\"}}"));
        Assert.Equal("flat", LMStudioClient.ExtractProviderMessage("{\"message\":\"flat\"}"));
    }
}
