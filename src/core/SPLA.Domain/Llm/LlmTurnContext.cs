using SPLA.Domain.Models;
using SPLA.Domain.Settings;

namespace SPLA.Domain.Llm;

/// <summary>
/// Everything one call to one model needs, and the only thing that varies from turn to turn.
/// <para>
/// This is the counterpart to the pipeline's <i>composition</i>: the set of middleware is baked once
/// per runtime and never changes, so all per-turn variation — messages, tools, which connection,
/// where the streamed chunks go — travels here. If you ever feel the need to assemble a different
/// pipeline for a particular turn, something is missing from this context instead.
/// </para>
/// </summary>
public sealed class LlmTurnContext
{
    /// <summary>The conversation as the model should see it, already assembled and filtered.</summary>
    public required IReadOnlyList<ChatMessage> Messages { get; init; }

    /// <summary>Tools offered for this turn (already mode-filtered). Empty = none.</summary>
    public IReadOnlyList<ToolDefinition> Tools { get; init; } = [];

    /// <summary>Endpoint, model and sampling knobs for this turn.</summary>
    public required LLMSettings Settings { get; init; }

    /// <summary>Which model entry this turn runs on. Null = the project's first/default entry.</summary>
    public string? ModelId { get; init; }

    // ── Sinks ────────────────────────────────────────────────────────────────────────────────────
    // Streaming is not a separate operation: it is the presence of these. The provider calls them
    // straight from its read loop, so middleware is in the CALL path and absent from the DATA path —
    // live output is unaffected by how many layers are composed.
    //
    // A middleware that needs to observe the output WRAPS the sink on the way down (never buffers the
    // result on the way up — by then the user has already read the text). A wrapper MUST forward the
    // chunk immediately and do only cheap, non-blocking work: the sink is awaited inside the socket
    // read loop.

    /// <summary>Called for each text chunk as it arrives. Null = don't stream text.</summary>
    public Func<string, Task>? OnDelta { get; set; }

    /// <summary>Called for each reasoning chunk as it arrives. Null = don't stream reasoning.</summary>
    public Func<string, Task>? OnReasoning { get; set; }

    /// <summary>
    /// Called once per generation a guard ABANDONS — never for the successful one. The successful
    /// attempt is already the message this call returns, so a record of it here would just be the same
    /// answer twice; this sink exists for the attempts that never became the answer, which otherwise
    /// leave no trace at all.
    /// </summary>
    public Action<GenerationAttempt>? OnAttempt { get; set; }

    /// <summary>
    /// Adds an observer to the text sink: <paramref name="observe"/> is called with every chunk, after
    /// the existing sink has already been given it.
    /// <para>
    /// Order is the whole point. Forwarding first means the reader's latency is untouched no matter how
    /// many observers are attached, and an observer that throws can never stop a chunk from reaching
    /// the screen — its exception is swallowed here, because the caller of this sink is a socket read
    /// loop that must not die over a bystander. An observer that needs to stop the turn does so by
    /// cancelling, not by throwing.
    /// </para>
    /// <para>
    /// Attaching works even when no sink exists: a run with no UI still gets its observers, which is
    /// what keeps a guard effective in a headless host.
    /// </para>
    /// </summary>
    public void ObserveDelta(Action<string> observe) => OnDelta = Wrap(OnDelta, observe);

    /// <summary>Adds an observer to the reasoning sink. See <see cref="ObserveDelta"/>.</summary>
    public void ObserveReasoning(Action<string> observe) => OnReasoning = Wrap(OnReasoning, observe);

    private static Func<string, Task> Wrap(Func<string, Task>? inner, Action<string> observe)
        => async chunk =>
        {
            if (inner != null) await inner(chunk);
            try { observe(chunk); } catch { /* a bystander must not break the stream */ }
        };

    /// <summary>
    /// Free-form bag for middleware that needs to hand something to a later stage. Deliberately the
    /// exception, not the rule: anything that crosses more than one hop belongs on a typed property
    /// of this class. A middleware whose precondition is missing must fail loudly, never no-op.
    /// </summary>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>(StringComparer.Ordinal);
}
