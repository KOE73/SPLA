using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json.Nodes;

namespace SPLA.Mcp;

/// <summary>
/// Renders one call's <see cref="ProgressTree"/> as MCP <c>notifications/progress</c>.
/// <para>
/// <b>A tree flattened into a line.</b> MCP carries one scalar and one sentence per tick, where SPLA
/// carries a forest of nodes with their own counters — so the projection has to choose what to say,
/// and it says the path to whatever moved last: <c>agent_spawn › run_command: scanning (12/254)</c>.
/// That is the honest reduction. Inventing an aggregate percentage over a tree whose shape is not
/// known until it finishes would look more informative and be less true.
/// </para>
/// <para>
/// <b>The counter is ours, not the tool's.</b> The spec requires <c>progress</c> to increase on every
/// notification for a token; a tree has no single quantity with that property (nodes open and close,
/// each with its own totals). So the field carries a tick number and <c>total</c> is omitted — the
/// count that matters to a person travels in the message, where it cannot be misread as a fraction of
/// something.
/// </para>
/// <para>
/// <b>Never blocks the tool.</b> Ticks arrive on whatever thread the tool runs on, including several
/// at once from parallel work; sends are chained onto a tail task so they leave in the order their
/// sequence numbers were taken, and the reporting tool waits for none of it.
/// </para>
/// <para>
/// <b>The rate limit coalesces, it does not discard.</b> A tick that arrives too soon is held as the
/// pending message and sent when the floor expires, and <see cref="FinishAsync"/> flushes whatever is
/// still held. Dropping instead was the obvious implementation and the wrong one: a tool that says
/// what it is doing once and then works silently for half a minute would have had that one sentence
/// swallowed by the frame announcing the tool had started, leaving the reader with the least
/// informative of the two.
/// </para>
/// </summary>
internal sealed class McpProgressReporter
{
    /// <summary>Floor on the gap between notifications. A port scan reports per host and a script per
    /// step — without this, a fast tool spends the pipe on frames a person cannot read anyway.</summary>
    private static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(120);

    /// <summary>Depth cap when walking a node's ancestry. The tree cannot be cyclic; this is here so a
    /// bug in one place cannot become a hang in another.</summary>
    private const int MaxPathDepth = 16;

    private readonly ProgressTree _tree;
    private readonly JsonNode _token;
    private readonly Func<JsonObject, Task> _send;

    /// <summary>Every node this reporter has heard of, by id — the only way to resolve
    /// <see cref="ProgressNode.ParentId"/> into a label, since the event carries just the one node.</summary>
    private readonly ConcurrentDictionary<string, ProgressNode> _seen = new();

    private readonly object _gate = new();
    private readonly Timer _flushTimer;
    private Task _tail = Task.CompletedTask;
    private long _sequence;
    private DateTimeOffset _lastSentAt = DateTimeOffset.MinValue;
    private string? _lastMessage;
    private string? _pending;
    private bool _finished;

    /// <param name="tree">The tree opened for this one <c>tools/call</c>.</param>
    /// <param name="token">The client's <c>_meta.progressToken</c>, echoed back verbatim — it may be a
    /// string or a number, and which it is belongs to the client.</param>
    /// <param name="send">Writes one JSON-RPC frame. Expected to serialise its own access to the pipe.</param>
    public McpProgressReporter(ProgressTree tree, JsonNode token, Func<JsonObject, Task> send)
    {
        _tree = tree;
        _token = token;
        _send = send;
        _flushTimer = new Timer(_ => Flush(), null, Timeout.Infinite, Timeout.Infinite);
        _tree.NodeChanged += OnNodeChanged;
    }

    /// <summary>
    /// Stops listening, sends whatever the rate limit was still holding, and waits for every queued
    /// frame to reach the pipe. Awaited before the result is written so the client never sees progress
    /// arrive after the answer it was about.
    /// </summary>
    public async Task FinishAsync()
    {
        _tree.NodeChanged -= OnNodeChanged;

        Task tail;
        lock (_gate)
        {
            if (_finished) return;
            _finished = true;
            if (_pending is { } last) Queue(last);
            _pending = null;
            tail = _tail;
        }

        await _flushTimer.DisposeAsync();
        try { await tail; } catch { /* a dead pipe is the session ending, not a tool failing */ }
    }

    private void OnNodeChanged(ProgressNode node)
    {
        _seen[node.Id] = node;
        Offer(Render(node));
    }

    private void Offer(string message)
    {
        lock (_gate)
        {
            if (_finished) return;

            // The same sentence twice says nothing new — true whether it is on the wire already or
            // merely waiting to be.
            if (string.Equals(message, _lastMessage, StringComparison.Ordinal)) return;
            if (string.Equals(message, _pending, StringComparison.Ordinal)) return;

            var now = DateTimeOffset.UtcNow;
            var due = _lastSentAt + MinInterval;

            if (now >= due)
            {
                _pending = null;
                _flushTimer.Change(Timeout.Infinite, Timeout.Infinite);
                Send(message, now);
                return;
            }

            // Too soon. Held rather than dropped, and the timer is armed for the remainder so a tool
            // that goes quiet after this tick still gets it delivered.
            _pending = message;
            _flushTimer.Change(due - now, Timeout.InfiniteTimeSpan);
        }
    }

    private void Flush()
    {
        lock (_gate)
        {
            if (_finished || _pending is not { } message) return;
            _pending = null;
            Send(message, DateTimeOffset.UtcNow);
        }
    }

    /// <summary>Records the send and queues it. Callers hold <see cref="_gate"/>.</summary>
    private void Send(string message, DateTimeOffset at)
    {
        _lastMessage = message;
        _lastSentAt = at;
        Queue(message);
    }

    /// <summary>Puts one frame on the tail. Callers hold <see cref="_gate"/>.
    /// <para>Ordering, not throughput, is why this is a chain: the spec requires the sequence to
    /// increase on the wire, and a number taken under the lock says nothing about when the write that
    /// carries it would otherwise have started.</para></summary>
    private void Queue(string message)
    {
        var frame = Frame(++_sequence, message);

        _tail = _tail
            .ContinueWith(_ => _send(frame), CancellationToken.None,
                TaskContinuationOptions.None, TaskScheduler.Default)
            .Unwrap();
    }

    private JsonObject Frame(long sequence, string message) => new()
    {
        ["jsonrpc"] = "2.0",
        ["method"] = "notifications/progress",
        ["params"] = new JsonObject
        {
            ["progressToken"] = _token.DeepClone(),
            ["progress"] = sequence,
            ["message"] = message
        }
    };

    /// <summary>
    /// One line for one node: where it sits, and the last thing it said. The path is what makes a tick
    /// from four levels down legible — <c>run_command</c> alone could be anything, whereas
    /// <c>agent_spawn › roslyn_script_run › run_command</c> is a place.
    /// </summary>
    private string Render(ProgressNode node)
    {
        var path = new List<string>(4);
        var current = node;
        for (var depth = 0; current is not null && depth < MaxPathDepth; depth++)
        {
            path.Add(current.Label);
            current = current.ParentId is { } parentId && _seen.TryGetValue(parentId, out var parent)
                ? parent
                : null;
        }
        path.Reverse();

        var text = new StringBuilder(string.Join(" › ", path));

        // A terminal node has nothing left to report; saying how it ended is more useful than
        // repeating its last tick.
        if (node.State != ProgressState.Running)
            return text.Append(node.State == ProgressState.Completed ? " ✓" : " ✗").ToString();

        var latest = node.Latest;
        if (latest is null) return text.Append(" …").ToString();

        if (!string.IsNullOrWhiteSpace(latest.Message))
            text.Append(": ").Append(latest.Message);

        if (latest.Current is long done)
            text.Append(latest.Total is long total ? $" ({done}/{total})" : $" ({done})");

        if (latest.Details is { Count: > 0 } details)
        {
            text.Append(" [");
            for (var i = 0; i < details.Count; i++)
            {
                if (i > 0) text.Append(", ");
                text.Append(details[i].Label).Append(": ").Append(details[i].Value);
            }
            text.Append(']');
        }

        return text.ToString();
    }
}
