using SPLA.Domain.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SPLA.Domain.Tools;

/// <summary>
/// Delivers messages a chat did not ask for at the moment it asked for them, at the one boundary
/// where inserting one is safe: the top of the agent loop, before the next LLM call is assembled.
/// <para>
/// Inserting anywhere inside a turn — between an assistant message's <c>tool_calls</c> and their
/// matching <c>tool_result</c>s — breaks the pairing every provider requires; see
/// <c>docs/adr/ADR_20260824-2_core_background-tool-calls.md</c> §3 ("вставка в середину пачки рвёт
/// разговор"). A background task's result is exactly this shape: it finished on its own schedule,
/// with nobody currently waiting on an LLM response to append it to, so it cannot ride any of the
/// existing per-call injection points (checkpoint rollback, the tool-image sink) — both of those fire
/// from inside a tool-call loop iteration that is already running. The inbox exists for the delivery
/// that has no turn to attach to at all.
/// </para>
/// <para>
/// One per chat, owned by <c>ChatRuntime</c> alongside <see cref="ProgressHub"/>. Producers so far:
/// none — this is the queue, not yet fed by anything. The first feed is the background-task delivery
/// (plan step 1.4).
/// </para>
/// </summary>
public sealed class ChatInbox
{
    private readonly ConcurrentQueue<ChatMessage> _queue = new();

    /// <summary>Queues a message for delivery at the top of the next loop iteration. Thread-safe:
    /// a background task's completion callback runs on its own thread, outside the turn gate.</summary>
    public void Enqueue(ChatMessage message) => _queue.Enqueue(message);

    /// <summary>
    /// Removes and returns everything queued so far, in arrival order. Called from inside the turn
    /// gate, at the top of the loop — the caller is expected to append each one to the conversation
    /// before assembling the next LLM call.
    /// <para>Draining rather than peeking is deliberate: a message delivered here is delivered once,
    /// the same way a <c>tool_result</c> is never replayed into a later call.</para>
    /// </summary>
    public IReadOnlyList<ChatMessage> DrainAll()
    {
        if (_queue.IsEmpty) return [];

        var drained = new List<ChatMessage>();
        while (_queue.TryDequeue(out var message)) drained.Add(message);
        return drained;
    }
}
