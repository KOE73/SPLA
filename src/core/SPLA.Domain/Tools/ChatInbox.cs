using SPLA.Domain.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SPLA.Domain.Tools;

/// <summary>
/// Who put an item in the inbox, and — by extension — how eager the pump (wave B) should be to wake
/// a turn for it. <see cref="Human"/> wakes immediately and always (it is a person's own words);
/// <see cref="TaskResult"/> wakes subject to policy (watchers present, debounce, self-feeding guard);
/// <see cref="Notice"/> is the pump's own voice back into the chat (e.g. "auto-wake paused") and is
/// never itself a wake trigger — see <c>ChatPump</c>'s comment on why a notice bypasses the inbox
/// entirely rather than carrying this kind through it.
/// <para>Nothing produces <see cref="Human"/> yet — <c>chat.send</c> still calls <c>SendAsync</c>
/// directly (wave D). The branch exists so the type is complete from day one rather than growing a
/// breaking change later.</para>
/// </summary>
public enum InboxItemKind { Human, TaskResult, Notice }

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

    /// <summary>True while at least one item sits undrained — the pump's trap-B.6 check: a signal
    /// that arrives after a turn already drained everything must find nothing here and do nothing.</summary>
    public bool HasPending => !_queue.IsEmpty;

    /// <summary>Raised after an item is queued, carrying its <see cref="InboxItemKind"/>. This is the
    /// pump's wake signal (ADR §2.1) — the pump has nothing else to subscribe to, since "content
    /// arrived" and "a turn should maybe start" are the same event now. Fired outside any lock, so a
    /// handler that itself calls <see cref="DrainAll"/> sees at least this item (queued-then-fired).</summary>
    public event Action<InboxItemKind>? Enqueued;

    /// <summary>Queues a message for delivery at the top of the next loop iteration. Thread-safe:
    /// a background task's completion callback runs on its own thread, outside the turn gate.
    /// <paramref name="kind"/> never reaches the conversation — <see cref="DrainAll"/> still hands
    /// back bare <see cref="ChatMessage"/>s — it exists purely for the pump's wake decision.</summary>
    public void Enqueue(ChatMessage message, InboxItemKind kind)
    {
        _queue.Enqueue(message);
        Enqueued?.Invoke(kind);
    }

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
