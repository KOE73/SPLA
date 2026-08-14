using Microsoft.Extensions.Logging;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Observability;
using System.Diagnostics;
using System.Text;

namespace SPLA.Agent.Guards;

/// <summary>
/// Watches a generation as it streams and abandons it when the model falls into a repetition loop —
/// the same word or paragraph emitted verbatim until the token ceiling stops it. Small local models do
/// this routinely, and cloud reasoning models do it too, so it is treated as an ordinary failure mode
/// of generation rather than as somebody's bug.
///
/// <para>
/// <b>Why a middleware and not a check in the agent loop.</b> What degenerates is the model's output,
/// which is what this stage exists to judge. Sitting here means every caller is covered — the agent
/// loop, a spawned sub-agent, the librarian that queries the model directly — instead of only the one
/// that remembered to look.
/// </para>
///
/// <para>
/// <b>How it can stop anything at all.</b> Middleware is on the call path, never on the data path:
/// chunks go straight from the provider's socket read loop to whatever sink the caller supplied, past
/// every layer. So the guard does not intercept the stream — it <i>observes</i> it, by wrapping those
/// sinks on the way down (see <see cref="LlmTurnContext.ObserveDelta"/>), and stops the generation by
/// cancelling the token the provider is reading with. The provider's read throws, its buffers unwind,
/// the HTTP connection closes, the model stops generating, and the exception surfaces back here.
/// </para>
///
/// <para>
/// <b>Why the partial text lives here.</b> When the read is cancelled the provider never returns, so
/// everything it had accumulated dies with the method. The observers are the only place the abandoned
/// generation still exists — which is why the capture below is not an optimization but the only copy.
/// </para>
///
/// <para>
/// The retry deliberately changes nothing about how the model is asked. Sampling is stochastic
/// (temperature is above zero and no seed is pinned), so the second attempt already walks a different
/// path; turning the repetition penalties up on the way would trade a real loss in answer quality for
/// a coincidence that is unlikely twice.
/// </para>
/// </summary>
public sealed class RepetitionGuardMiddleware : ILlmMiddleware
{
    private readonly ILogger? _logger;

    public RepetitionGuardMiddleware(ILogger? logger = null) => _logger = logger;

    public LlmPipelineStage Stage => LlmPipelineStage.Output;

    /// <summary>Total generations allowed for one call, the abandoned ones included. 2 = one retry.
    /// More than that is rarely worth it: two loops at the same point mean the model is stuck on the
    /// task, not unlucky with sampling, and every attempt costs tokens on a metered connection.</summary>
    public int MaxAttempts { get; set; } = 2;

    /// <summary>How much of an abandoned generation is kept per channel. The interesting parts are the
    /// beginning — where the answer was still going somewhere — and the looping unit, which the
    /// detector reports separately; the kilometres of repetition in between explain nothing and are
    /// dropped rather than carried around.</summary>
    public int PartialCaptureChars { get; set; } = 4000;

    public async Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, LlmTurnDelegate next, CancellationToken ct)
    {
        // The caller's own sinks, kept aside: every attempt re-wraps THESE, never the previous
        // attempt's wrapper, or attempt two would still be feeding attempt one's detectors.
        var callerDelta = ctx.OnDelta;
        var callerReasoning = ctx.OnReasoning;

        try
        {
            for (var attemptNo = 1; ; attemptNo++)
            {
                var content = new Channel("content", PartialCaptureChars);
                var reasoning = new Channel("reasoning", PartialCaptureChars);

                using var attempt = TurnAttempt.Begin(ct);
                var stopwatch = Stopwatch.StartNew();

                ctx.OnDelta = callerDelta;
                ctx.OnReasoning = callerReasoning;
                ctx.ObserveDelta(chunk => Watch(content, chunk, attempt));
                ctx.ObserveReasoning(chunk => Watch(reasoning, chunk, attempt));

                try
                {
                    return await next(ctx, attempt.Token);
                }
                catch (OperationCanceledException) when (attempt.AbortedByMe)
                {
                    stopwatch.Stop();
                    var looped = content.Detector.Detected != null ? content : reasoning;
                    var info = looped.Detector.Detected!;
                    var note = $"repetition in {looped.Name}: period {info.Period} chars, x{info.Repeats}";

                    _logger?.LogWarning(
                        "Generation abandoned — repetition in {Channel}: period={Period} chars, repeats={Repeats}, " +
                        "attempt {Attempt} of {MaxAttempts}. Unit: {Unit}",
                        looped.Name, info.Period, info.Repeats, attemptNo, MaxAttempts, info.Note);

                    Report(ctx, attemptNo, content, reasoning, note, stopwatch.Elapsed, looped.Name);

                    if (attemptNo >= MaxAttempts)
                        return Degenerate(content, reasoning);
                }
            }
        }
        finally
        {
            // Nothing this middleware did to the context outlives it. The context is the caller's.
            ctx.OnDelta = callerDelta;
            ctx.OnReasoning = callerReasoning;
        }
    }

    private static void Watch(Channel channel, string chunk, TurnAttempt attempt)
    {
        channel.Capture(chunk);
        channel.Detector.Feed(chunk);
        if (channel.Detector.Detected != null)
            attempt.Abort("repetition:" + channel.Name);
    }

    /// <summary>
    /// Tells the turn's sink and the telemetry meter about one abandoned generation. Both are
    /// best-effort: a subscriber that throws, or a meter with nobody listening, must never be able to
    /// take the guard down — same discipline as <see cref="LlmTurnContext.ObserveDelta"/>.
    /// </summary>
    private void Report(
        LlmTurnContext ctx, int attemptNo, Channel content, Channel reasoning, string note, TimeSpan duration, string channel)
    {
        try
        {
            ctx.OnAttempt?.Invoke(new GenerationAttempt
            {
                Index = attemptNo,
                Outcome = AttemptOutcome.Repetition,
                Content = content.Text.Length > 0 ? content.Text : null,
                Reasoning = reasoning.Text.Length > 0 ? reasoning.Text : null,
                Note = note,
                Chars = content.TotalChars + reasoning.TotalChars,
                Duration = duration
            });
        }
        catch { /* a bystander must not break the guard */ }

        SplaTelemetry.DiscardedGenerations.Add(1,
            new KeyValuePair<string, object?>("model", ctx.Settings.ModelName),
            new KeyValuePair<string, object?>("channel", channel));
        SplaTelemetry.DiscardedChars.Add(content.TotalChars + reasoning.TotalChars,
            new KeyValuePair<string, object?>("model", ctx.Settings.ModelName),
            new KeyValuePair<string, object?>("channel", channel));
    }

    /// <summary>The turn's outcome once every attempt has looped: not an error — the provider answered
    /// and the tokens were spent — but not an answer either. The last partial generation rides along so
    /// a caller can show what happened; <see cref="LlmTurnStatus.Degenerate"/> is what says it must not
    /// be read as a reply.</summary>
    private static LlmTurnResult Degenerate(Channel content, Channel reasoning) => new()
    {
        Message = new ChatMessage
        {
            Role = ChatRole.Assistant,
            Content = content.Text,
            Reasoning = reasoning.Text.Length > 0 ? reasoning.Text : null
        },
        Status = LlmTurnStatus.Degenerate
    };

    /// <summary>One watched output channel: its detector, plus the bounded copy of what it emitted.</summary>
    private sealed class Channel(string name, int captureLimit)
    {
        private readonly StringBuilder _captured = new();

        public string Name { get; } = name;
        public RepetitionDetector Detector { get; } = new();
        public string Text => _captured.ToString();

        /// <summary>Every character this channel ever saw, uncapped — unlike <see cref="Text"/>, which
        /// stops growing at <paramref name="captureLimit"/>. This is what <see cref="GenerationAttempt.Chars"/>
        /// reports: the capture is deliberately truncated for readability, but the cost of the attempt
        /// is not.</summary>
        public int TotalChars { get; private set; }

        public void Capture(string chunk)
        {
            TotalChars += chunk.Length;
            var room = captureLimit - _captured.Length;
            if (room <= 0) return;
            _captured.Append(chunk.Length <= room ? chunk : chunk[..room]);
        }
    }
}
