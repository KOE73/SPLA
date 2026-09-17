using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SPLA.Domain.Models;

/// <summary>
/// The single source of truth for a chat session's message history. Owns
/// <see cref="ChatMessage"/> objects; all UI projections and persistence decisions
/// are derived from this list — never the other way around.
/// </summary>
public sealed class Conversation
{
    private readonly List<ChatMessage> _messages = new();

    // Per-role sequential counters for stable, readable MsgIds (U-1, A-2, T-3, S-4, L-5).
    private int _userSeq;
    private int _assistantSeq;
    private int _toolSeq;
    private int _systemSeq;
    private int _labelSeq;
    private int _scopeMarkerSeq;
    private int _summarySeq;

    public IReadOnlyList<ChatMessage> Messages => _messages;
    public int Count => _messages.Count;

    public void Add(ChatMessage message)
    {
        if (string.IsNullOrEmpty(message.MsgId))
            message.MsgId = GenerateMsgId(message.Role,
                isScopeMarker: message.ScopeMarker != null, isSummary: message.CompactSummary);
        _messages.Add(message);
    }

    private string GenerateMsgId(ChatRole role, bool isLabel = false, bool isScopeMarker = false, bool isSummary = false)
    {
        if (isLabel) return $"L-{Interlocked.Increment(ref _labelSeq)}";
        if (isScopeMarker) return $"R-{Interlocked.Increment(ref _scopeMarkerSeq)}";
        if (isSummary) return $"C-{Interlocked.Increment(ref _summarySeq)}";
        return role switch
        {
            ChatRole.User      => $"U-{Interlocked.Increment(ref _userSeq)}",
            ChatRole.Assistant => $"A-{Interlocked.Increment(ref _assistantSeq)}",
            ChatRole.Tool      => $"T-{Interlocked.Increment(ref _toolSeq)}",
            ChatRole.System    => $"S-{Interlocked.Increment(ref _systemSeq)}",
            _                  => $"X-{Interlocked.Increment(ref _userSeq)}"
        };
    }

    /// <summary>
    /// Inserts an invisible label anchor immediately before <paramref name="assistantMsg"/>
    /// (which must already be the last message in the list). The label gets a stable L-* MsgId,
    /// carries an optional mark name and resume note, and is never sent to the LLM.
    /// Returns the created label.
    /// </summary>
    public ChatMessage InsertLabelBefore(ChatMessage assistantMsg, string? markName = null, string? resume = null)
    {
        var label = new ChatMessage
        {
            IsLabel   = true,
            IsEphemeral = true,
            Role      = ChatRole.System,
            Mark      = markName,
            Content   = resume ?? string.Empty
        };
        label.MsgId = GenerateMsgId(label.Role, isLabel: true);

        var idx = _messages.IndexOf(assistantMsg);
        if (idx < 0)
            _messages.Add(label);   // fallback: append
        else
            _messages.Insert(idx, label);

        return label;
    }

    /// <summary>
    /// Appends a scope-marker message recording that <paramref name="scope"/>'s project rules have
    /// been (or are about to be) loaded into the prompt — see
    /// <c>docs/adr/ADR_20260911-2_agent_agents-md-scopes.md</c> §2.5. Always appended at the end of
    /// the history (unlike a label, a marker is not a position anchor for something else — it is its
    /// own event). <c>Role</c> is <see cref="ChatRole.User"/>: any neutral role would do, since
    /// <see cref="Context.ContextAssembler.ShouldSend"/> excludes markers from the model regardless —
    /// <c>User</c> was picked simply because it is not treated specially anywhere else in
    /// <see cref="ShouldPersist"/> or assembly (unlike <see cref="ChatRole.System"/> or
    /// <see cref="ChatRole.Tool"/>).
    /// </summary>
    public ChatMessage AddScopeMarker(string scope)
    {
        var marker = new ChatMessage
        {
            Role = ChatRole.User,
            Content = string.Empty,
            ScopeMarker = scope
        };
        marker.MsgId = GenerateMsgId(marker.Role, isScopeMarker: true);
        _messages.Add(marker);
        return marker;
    }

    /// <summary>
    /// Inserts a compaction summary record immediately before <paramref name="beforeMsg"/> (the first
    /// message of the kept tail — see <c>ChatRuntime.CompactAsync</c> and
    /// <c>docs/adr/ADR_20260911-3_agent_compaction.md</c> §2.2/§2.6). Mirrors
    /// <see cref="InsertLabelBefore"/>'s insert-before-a-given-message shape, but the record itself is
    /// the opposite of a label in every way that matters: it is not ephemeral, it IS sent to the model
    /// (as an ordinary <see cref="ChatRole.User"/> turn) and it DOES get persisted. Gets a stable
    /// <c>C-*</c> MsgId. Returns the created summary message.
    /// </summary>
    public ChatMessage InsertSummaryBefore(ChatMessage beforeMsg, string content)
    {
        var summary = new ChatMessage
        {
            Role = ChatRole.User,
            Content = content,
            CompactSummary = true
        };
        summary.MsgId = GenerateMsgId(summary.Role, isSummary: true);

        var idx = _messages.IndexOf(beforeMsg);
        if (idx < 0)
            _messages.Add(summary);   // fallback: append
        else
            _messages.Insert(idx, summary);

        return summary;
    }

    public bool Remove(ChatMessage message) => _messages.Remove(message);

    public void Clear() => _messages.Clear();

    /// <summary>
    /// True if <paramref name="msg"/> should be written to the persistent chat file.
    /// System prompts (rebuilt from config on load) are always excluded. Tool messages (transient
    /// call/result pairs) are excluded unless <paramref name="saveToolCalls"/> asks to keep them —
    /// off by default, since most of that trace is diagnostic noise nobody re-reads.
    /// <para>
    /// A message with blank <see cref="ChatMessage.Content"/> normally does not survive either — that
    /// is what makes the degenerate-turn placeholder (empty content, a non-empty
    /// <see cref="ChatMessage.Attempts"/>) invisible by default. <paramref name="saveAttempts"/> is the
    /// one thing that keeps it: with the setting off, dropping it is correct — a reload would otherwise
    /// show an empty bubble that explains nothing, since the very thing it exists to explain was never
    /// written down. With it on, the message is the only record of what happened and must stay.
    /// </para>
    /// </summary>
    /// <remarks>
    /// A scope marker (<see cref="ChatMessage.ScopeMarker"/> non-null) always persists, regardless of
    /// every other flag here — it is the only record of which folders' rules a session has already
    /// been shown, and without it a reopened chat would re-trigger every write refusal it already
    /// paid for. See <c>docs/adr/ADR_20260911-2_agent_agents-md-scopes.md</c> §2.5.
    /// </remarks>
    public static bool ShouldPersist(ChatMessage msg, bool saveToolCalls = false, bool saveAttempts = false) =>
        msg.ScopeMarker != null ||
        (!msg.IsEphemeral &&
        !msg.IsLabel &&
        msg.Role != ChatRole.System &&
        (msg.Role != ChatRole.Tool || saveToolCalls) &&
        (msg.Role == ChatRole.Tool
            ? (msg.ToolCalls?.Count > 0 || !string.IsNullOrWhiteSpace(msg.Content))
            : (!string.IsNullOrWhiteSpace(msg.Content) || (saveAttempts && msg.Attempts?.Count > 0))));

    /// <summary>
    /// Truncates the message history to <paramref name="messageCount"/> entries, removing everything after.
    /// No-op if <paramref name="messageCount"/> is already &gt;= current count.
    /// <para>
    /// The one centralized place a rollback that crosses a compaction boundary is repaired
    /// (<c>docs/adr/ADR_20260911-3_agent_compaction.md</c> §2.5): if truncation removes a message with
    /// <see cref="ChatMessage.CompactSummary"/> set, every remaining message whose
    /// <see cref="ChatMessage.CompactedBy"/> names that summary's <see cref="ChatMessage.MsgId"/> gets
    /// its <see cref="ChatMessage.RetentionPolicy"/> handed back to
    /// <see cref="ContextRetention.Persistent"/> and <see cref="ChatMessage.CompactedBy"/> cleared —
    /// exactly undoing what that one compaction did. Bookmark rollback, checkpoint rollback,
    /// <c>chat.rewind</c> and branching all funnel through this method (or its <c>string</c> overload,
    /// which calls it), so none of them need their own copy of this repair.
    /// </para>
    /// </summary>
    public void TruncateTo(int messageCount)
    {
        if (messageCount >= _messages.Count) return;

        var removed = _messages.GetRange(messageCount, _messages.Count - messageCount);
        _messages.RemoveRange(messageCount, _messages.Count - messageCount);

        foreach (var summary in removed)
        {
            if (!summary.CompactSummary) continue;
            foreach (var m in _messages)
            {
                if (m.CompactedBy != summary.MsgId) continue;
                m.RetentionPolicy = ContextRetention.Persistent;
                m.CompactedBy = null;
            }
        }
    }

    /// <summary>
    /// Truncates so that the message with <paramref name="anchorMsgId"/> is the last entry,
    /// removing everything after it. No-op if the message is not found or is already last.
    /// Returns true if the truncation was applied.
    /// </summary>
    public bool TruncateTo(string anchorMsgId)
    {
        var idx = _messages.FindIndex(m => m.MsgId == anchorMsgId);
        if (idx < 0) return false;
        TruncateTo(idx + 1);
        return true;
    }

    /// <summary>
    /// Finds the most-recent message that has <paramref name="markName"/> assigned to its
    /// <see cref="ChatMessage.Mark"/> property.  Returns null if no such message exists.
    /// </summary>
    public ChatMessage? FindByMark(string markName) =>
        _messages.LastOrDefault(m => m.Mark == markName);

    /// <summary>
    /// Returns the last non-ephemeral, non-label message, or null if the history is empty.
    /// </summary>
    public ChatMessage? LastReal() =>
        _messages.LastOrDefault(m => !m.IsEphemeral && !m.IsLabel);

    /// <summary>
    /// Finds the most-recent label carrying <paramref name="markName"/> in its Mark property.
    /// </summary>
    public ChatMessage? FindLabel(string markName) =>
        _messages.LastOrDefault(m => m.IsLabel && m.Mark == markName);

    /// <summary>Messages that should be written to the persistent chat file.</summary>
    public IEnumerable<ChatMessage> Persistable => _messages.Where(m => ShouldPersist(m));

    /// <summary>Messages that should be written to the persistent chat file, given the project's
    /// full-tool-trace and save-attempts preferences.</summary>
    public IEnumerable<ChatMessage> PersistableWith(bool saveToolCalls, bool saveAttempts = false) =>
        _messages.Where(m => ShouldPersist(m, saveToolCalls, saveAttempts));
}
