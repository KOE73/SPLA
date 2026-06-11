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
    
    // For when the assistant wants to call tools
    public List<ToolCall>? ToolCalls { get; set; }
    
    // For when we send tool results back to the assistant (Role = Tool)
    public string? ToolCallId { get; set; }

    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }

    public ContextRetention RetentionPolicy { get; set; } = ContextRetention.Persistent;
    public string? ReplacementKey { get; set; }
}
