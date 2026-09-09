namespace SPLA.Domain.Llm.Middleware;

/// <summary>
/// The next moment each connection is allowed to be called, and the queue that forms in front of it.
/// <para>
/// Keyed by connection rather than by model or by chat, because that is what the provider counts: a
/// rate limit belongs to the credential, so five models and three chats sharing one key share one
/// budget. A gate per model would let two individually well-behaved callers exceed together what the
/// key allows — which is the whole failure this exists to prevent.
/// </para>
/// <para>
/// Shared and long-lived, unlike everything else on the call path. It is the one piece of pipeline
/// state that must outlive a turn: pacing that forgot the previous request would pace nothing.
/// </para>
/// </summary>
public sealed class RequestPacer
{
    private readonly Dictionary<string, DateTimeOffset> _nextFree = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _gate = new();

    /// <summary>
    /// Claims the next free slot on a connection and returns how long the caller must wait for it.
    /// <para>
    /// The slot is taken immediately rather than after the wait, which is what makes concurrent
    /// callers queue instead of colliding: two chats asking at the same instant get consecutive slots,
    /// not the same one. The cost of that choice is a slot burned when a caller is cancelled while
    /// waiting — the next one then waits out a turn that never happened. Accepted deliberately:
    /// releasing slots would mean tracking every waiter, and the failure mode is one extra pause.
    /// </para>
    /// <para><paramref name="now"/> is a parameter and not <see cref="DateTimeOffset.UtcNow"/> so the
    /// queueing can be tested without spending real seconds on it.</para>
    /// </summary>
    public TimeSpan Claim(string connectionId, TimeSpan interval, DateTimeOffset now)
    {
        if (interval <= TimeSpan.Zero) return TimeSpan.Zero;

        lock (_gate)
        {
            var slot = _nextFree.TryGetValue(connectionId, out var free) && free > now ? free : now;
            _nextFree[connectionId] = slot + interval;
            return slot - now;
        }
    }
}

/// <summary>
/// Holds requests apart so the provider's rate limit is not tripped in the first place.
/// <para>
/// The counterpart to <see cref="RateLimitRetryMiddleware"/> and deliberately not the same layer:
/// that one reacts to a refusal and lives for one turn, this one prevents refusals and has to keep
/// working on the turns that succeed. A single layer doing both would only start working after it had
/// stopped being needed.
/// </para>
/// <para>
/// Off by default (<c>min_request_interval: 0</c>) — the mechanism exists and costs nothing until
/// someone has a provider's demand or a measurement to raise it with. A second per turn is not the
/// small price it sounds like: an agent making fifty tool calls pays most of a minute for it.
/// </para>
/// </summary>
public sealed class RequestPacingMiddleware : ILlmMiddleware
{
    private readonly RequestPacer _pacer;

    public RequestPacingMiddleware(RequestPacer pacer) => _pacer = pacer;

    public LlmPipelineStage Stage => LlmPipelineStage.Pacing;

    public async Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, LlmTurnDelegate next, CancellationToken ct)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(0, ctx.Settings.MinRequestInterval));
        var connectionId = ctx.Settings.ConnectionId;

        // No connection id means nothing to pace against — a bare turn with no configured account.
        if (interval > TimeSpan.Zero && !string.IsNullOrEmpty(connectionId))
        {
            var wait = _pacer.Claim(connectionId, interval, DateTimeOffset.UtcNow);
            // The turn's own token: Stop ends the wait at once instead of after it.
            if (wait > TimeSpan.Zero) await Task.Delay(wait, ct);
        }

        return await next(ctx, ct);
    }
}
