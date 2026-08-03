using System.Globalization;
using System.Net.Http.Headers;
using SPLA.Domain.Llm;

namespace SPLA.LLM.OpenAiCompat;

/// <summary>
/// Reads the de-facto standard rate-limit headers off a response.
/// <para>
/// This is the cheapest telemetry there is: the provider already sent it with an answer we already
/// paid for, so collecting it costs no extra request and can never be stale. Before this, the
/// headers were read by nobody and discarded.
/// </para>
/// <para>
/// Lives in the transport because only the transport sees HTTP. It produces
/// <see cref="ProviderFact"/>s and stops there — what to do about a nearly-exhausted budget is
/// policy, and policy is a middleware's business.
/// </para>
/// </summary>
public static class RateLimitSignals
{
    public static IReadOnlyList<ProviderFact> From(HttpResponseHeaders headers)
    {
        var now = DateTimeOffset.UtcNow;
        var facts = new List<ProviderFact>();

        var limit = First(headers, "X-RateLimit-Limit");
        var remaining = First(headers, "X-RateLimit-Remaining");
        var reset = First(headers, "X-RateLimit-Reset");
        var retryAfter = First(headers, "Retry-After");

        var resetsAt = ParseReset(reset, now);

        if (remaining != null)
        {
            // Severity is computed against the limit when there is one; a bare "remaining" says
            // nothing about how close to the edge it is.
            var severity = ProviderFactSeverity.Normal;
            if (long.TryParse(remaining, out var left))
            {
                if (long.TryParse(limit, out var cap) && cap > 0)
                    severity = ((double)left / cap) switch
                    {
                        < 0.05 => ProviderFactSeverity.Critical,
                        < 0.20 => ProviderFactSeverity.Warn,
                        _ => ProviderFactSeverity.Normal
                    };
                else if (left == 0) severity = ProviderFactSeverity.Critical;
            }

            facts.Add(new ProviderFact
            {
                Key = "ratelimit.remaining", Label = "Requests left", Value = remaining,
                Unit = "req", Kind = ProviderFactKind.Counter, Severity = severity,
                ObservedAt = now, ResetsAt = resetsAt
            });
        }

        if (limit != null)
            facts.Add(new ProviderFact
            {
                Key = "ratelimit.limit", Label = "Request limit", Value = limit,
                Unit = "req", Kind = ProviderFactKind.Counter, ObservedAt = now, ResetsAt = resetsAt
            });

        if (resetsAt is { } r)
            facts.Add(new ProviderFact
            {
                Key = "ratelimit.reset", Label = "Window resets", Value = r.ToString("O"),
                Kind = ProviderFactKind.Timestamp, ObservedAt = now, ResetsAt = r
            });

        if (retryAfter != null)
            facts.Add(new ProviderFact
            {
                Key = "ratelimit.retry_after", Label = "Retry after", Value = retryAfter,
                Unit = "s", Kind = ProviderFactKind.Duration, ObservedAt = now
            });

        return facts;
    }

    private static string? First(HttpResponseHeaders headers, string name)
        => headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;

    /// <summary>
    /// X-RateLimit-Reset is either a delta in seconds or an absolute epoch, and providers disagree —
    /// OpenRouter sends milliseconds since the epoch. Distinguished by magnitude rather than by
    /// trusting one convention: anything large enough to be a timestamp is treated as one.
    /// </summary>
    private static DateTimeOffset? ParseReset(string? raw, DateTimeOffset now)
    {
        if (!long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) || v <= 0)
            return null;

        return v switch
        {
            > 100_000_000_000 => DateTimeOffset.FromUnixTimeMilliseconds(v),  // ms epoch
            > 1_000_000_000 => DateTimeOffset.FromUnixTimeSeconds(v),         // s epoch
            _ => now.AddSeconds(v)                                            // delta
        };
    }
}
