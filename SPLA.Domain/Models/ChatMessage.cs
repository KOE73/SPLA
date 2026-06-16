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
    public ChatRole Role { get; set; }
    public string? Content { get; set; } = string.Empty;

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

    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }

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
