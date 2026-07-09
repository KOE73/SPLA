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

    public IReadOnlyList<ChatMessage> Messages => _messages;
    public int Count => _messages.Count;

    public void Add(ChatMessage message)
    {
        if (string.IsNullOrEmpty(message.MsgId))
            message.MsgId = GenerateMsgId(message.Role);
        _messages.Add(message);
    }

    private string GenerateMsgId(ChatRole role, bool isLabel = false)
    {
        if (isLabel) return $"L-{Interlocked.Increment(ref _labelSeq)}";
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

    public bool Remove(ChatMessage message) => _messages.Remove(message);

    public void Clear() => _messages.Clear();

    /// <summary>
    /// True if <paramref name="msg"/> should be written to the persistent chat file.
    /// System prompts (rebuilt from config on load) and Tool messages (transient call/result pairs)
    /// are excluded; only the human-readable exchange is saved.
    /// </summary>
    public static bool ShouldPersist(ChatMessage msg) =>
        !msg.IsEphemeral &&
        !msg.IsLabel &&
        msg.Role != ChatRole.System &&
        msg.Role != ChatRole.Tool &&
        !string.IsNullOrWhiteSpace(msg.Content);

    /// <summary>
    /// Truncates the message history to <paramref name="messageCount"/> entries, removing everything after.
    /// No-op if <paramref name="messageCount"/> is already &gt;= current count.
    /// </summary>
    public void TruncateTo(int messageCount)
    {
        if (messageCount < _messages.Count)
            _messages.RemoveRange(messageCount, _messages.Count - messageCount);
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
    public IEnumerable<ChatMessage> Persistable => _messages.Where(ShouldPersist);
}
