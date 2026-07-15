using System.Collections.Generic;

namespace SPLA.Domain.Models;

public enum ContextRetention
{
    Persistent,
    UntilSuperseded,
    UntilResolved,
    NextStepOnly,
    Never
}

public class ChatMessage
{
    /// <summary>
    /// Stable human-readable identity for this message. Assigned by <see cref="Conversation.Add"/>;
    /// format: role-prefix + sequential number, e.g. U-1, A-2, T-3, S-4.
    /// Stable across message deletions — used as checkpoint/mark anchors.
    /// </summary>
    public string MsgId { get; set; } = string.Empty;

    /// <summary>
    /// Optional named rollback mark attached to this message. Set by the mark_set tool.
    /// At most one mark per message; searching for a mark name walks <see cref="Conversation.Messages"/>.
    /// </summary>
    public string? Mark { get; set; }

    /// <summary>
    /// True for invisible position-anchor messages inserted by checkpoint_save / mark_set.
    /// Labels are never sent to the LLM (filtered by ContextAssembler) and never persisted.
    /// They carry MsgId like L-1, L-2 and optionally a Mark name and resume text in Content.
    /// </summary>
    public bool IsLabel { get; set; }

    /// <summary>UTC creation time. Preserved across save/load so clients can show message age;
    /// defaults to "now" for messages created this session.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ChatRole Role { get; set; }
    public string? Content { get; set; } = string.Empty;

    /// <summary>
    /// Optional attached images for a user message, as data URLs (data:image/...;base64,...). When
    /// present, an OpenAI-compatible vision client emits the message content as a parts array
    /// (text + image_url) instead of a plain string. Null/empty for normal text messages.
    /// </summary>
    public List<string>? Images { get; set; }

    /// <summary>
    /// Separate chain-of-thought / reasoning text emitted by reasoning models
    /// (OpenAI <c>reasoning_content</c> field, or stripped from inline &lt;think&gt; tags).
    /// Null/empty when the model did not reason or reasoning was not exposed.
    /// </summary>
    public string? Reasoning { get; set; }

    // For when the assistant wants to call tools
    public List<ToolCall>? ToolCalls { get; set; }
    
    // For when we send tool results back to the assistant (Role = Tool)
    public string? ToolCallId { get; set; }

    /// <summary>
    /// Tokens the provider reported for the request (prompt) and the answer (completion).
    /// <c>null</c> means the provider does not expose usage — UI must treat absence as "unknown"
    /// and render nothing rather than a misleading 0. See <see cref="ITokenUsageReporter"/>.
    /// </summary>
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }

    public ContextRetention RetentionPolicy { get; set; } = ContextRetention.Persistent;
    public string? ReplacementKey { get; set; }

    /// <summary>
    /// True for messages that are shown to the user but must never be sent to the model
    /// (status notices, guard warnings, command echoes, error bubbles). This is the domain
    /// replacement for the old UI-level <c>is SystemMessageViewModel</c> check: whether a
    /// message reaches the model is decided here, on the message itself, not by its render type.
    /// </summary>
    public bool IsEphemeral { get; set; }
}
