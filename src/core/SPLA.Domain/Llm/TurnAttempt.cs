namespace SPLA.Domain.Llm;

/// <summary>
/// One attempt at a model call that the caller may abandon mid-flight.
///
/// <para>
/// Abandoning a streamed generation is not "stop reading" — it is cancelling the token the provider
/// reads the socket with, so the HTTP connection closes and the model actually stops generating. That
/// part is one line. The part worth a type is what comes after: the cancellation surfaces as an
/// <see cref="OperationCanceledException"/> that is <b>indistinguishable</b> from the user pressing
/// stop, and a guard that confuses the two will helpfully restart a generation the human just killed.
/// <see cref="AbortedByMe"/> is the discrimination, and it is the reason this exists as a type instead
/// of a linked token source created inline.
/// </para>
///
/// <para>
/// Usage is always the same shape — cancel from wherever the decision is made (a stream observer, a
/// timer), then catch at the frame that owns the attempt:
/// </para>
/// <code>
/// using var attempt = TurnAttempt.Begin(ct);
/// try { return await next(ctx, attempt.Token); }
/// catch (OperationCanceledException) when (attempt.AbortedByMe) { /* mine — decide what to do */ }
/// </code>
/// </summary>
public sealed class TurnAttempt : IDisposable
{
    private readonly CancellationTokenSource _cts;
    private readonly CancellationToken _outer;

    private TurnAttempt(CancellationToken outer)
    {
        _outer = outer;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(outer);
    }

    /// <summary>Starts an attempt whose token also honours <paramref name="outer"/>, so the user's own
    /// cancellation still stops everything immediately.</summary>
    public static TurnAttempt Begin(CancellationToken outer) => new(outer);

    /// <summary>The token to pass down. Cancelled either by <see cref="Abort"/> or by the outer token.</summary>
    public CancellationToken Token => _cts.Token;

    /// <summary>Why this attempt was abandoned, or null if it was not abandoned here.</summary>
    public string? AbortReason { get; private set; }

    /// <summary>
    /// True only when the cancellation came from <see cref="Abort"/> AND the outer token is still
    /// alive. Both halves matter: a user who presses stop during an abort must win, because the
    /// alternative is a guard that treats "the human said no" as "let me try that again".
    /// </summary>
    public bool AbortedByMe => AbortReason != null && !_outer.IsCancellationRequested;

    /// <summary>Abandons this attempt. Safe to call from a stream observer and safe to call twice —
    /// the first reason is the one that stands, since it is the one that caused the stop.</summary>
    public void Abort(string reason)
    {
        AbortReason ??= reason;
        if (!_cts.IsCancellationRequested) _cts.Cancel();
    }

    public void Dispose() => _cts.Dispose();
}
