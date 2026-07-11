using System;
using System.Net;

namespace SPLA.Domain.Interfaces;

/// <summary>Why an LLM request failed, coarse enough for the UI to show one actionable line.</summary>
public enum LlmErrorKind
{
    /// <summary>The prompt exceeded the model's context window (or the server ran out of context room).</summary>
    ContextExhausted,
    /// <summary>Authentication/authorization rejected the request (bad or missing API key).</summary>
    AuthFailed,
    /// <summary>The provider rate-limited or is overloaded.</summary>
    RateLimited,
    /// <summary>The endpoint could not be reached (server down, wrong URL, refused connection).</summary>
    Unreachable,
    /// <summary>Any other provider-side failure.</summary>
    ProviderError
}

/// <summary>
/// A classified LLM failure. Carries a short, user-facing <see cref="Exception.Message"/> (the turn
/// path surfaces it to the UI verbatim) plus the machine-readable <see cref="Kind"/> and the raw
/// provider detail for logs. Providers signal context exhaustion inconsistently — OpenAI/vLLM return a
/// clean 400 <c>context_length_exceeded</c>, LM Studio often a 500 with an HTML body — so classification
/// normalizes all of them into one <see cref="LlmErrorKind"/> the rest of the app can act on.
/// </summary>
public sealed class LlmRequestException : Exception
{
    public LlmErrorKind Kind { get; }
    public HttpStatusCode? StatusCode { get; }
    /// <summary>The raw provider body/reason, kept for logs; never shown to the user directly.</summary>
    public string? Detail { get; }

    public LlmRequestException(LlmErrorKind kind, string message, HttpStatusCode? statusCode = null, string? detail = null)
        : base(message)
    {
        Kind = kind;
        StatusCode = statusCode;
        Detail = detail;
    }
}
