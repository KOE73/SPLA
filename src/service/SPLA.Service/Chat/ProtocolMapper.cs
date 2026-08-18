using SPLA.Runtime;
using SPLA.Domain.Models;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>Maps the agent's domain types to the wire DTOs and back. Keeps serialization concerns
/// out of <see cref="ChatRuntime"/> and the connection bridge.</summary>
public static class ProtocolMapper
{
    public static ChatMessageDto ToDto(ChatMessage m) => new()
    {
        MsgId = m.MsgId,
        Role = m.Role.ToString().ToLowerInvariant(),
        Content = m.Content,
        Reasoning = m.Reasoning,
        CreatedAt = m.CreatedAt.ToString("o"),
        ToolCallId = m.ToolCallId,
        IsEphemeral = m.IsEphemeral,
        ToolCalls = m.ToolCalls?.Select(ToDto).ToList(),
        Attempts = m.Attempts?.Select(ToDto).ToList()
    };

    public static ToolCallDto ToDto(ToolCall tc) => new()
    {
        Id = tc.Id,
        Name = tc.Function.Name,
        Arguments = tc.Function.Arguments
    };

    public static AttemptDto ToDto(SPLA.Domain.Llm.GenerationAttempt a) => new()
    {
        Index = a.Index,
        Outcome = a.Outcome.ToString(),
        Note = a.Note,
        Chars = a.Chars,
        DurationMs = (long)a.Duration.TotalMilliseconds,
        Content = a.Content,
        Reasoning = a.Reasoning
    };

    /// <summary>The reasoning capability as the client sees it. A straight projection: the wire must
    /// carry the provider's own vocabulary, because normalizing it to a fixed enum is exactly what
    /// this design refuses to do.</summary>
    public static ReasoningCapabilityDto ToDto(ReasoningCapability c) => new()
    {
        Known = c.Known,
        Supported = c.Supported,
        Mandatory = c.Mandatory,
        DefaultEnabled = c.DefaultEnabled,
        Efforts = c.Efforts.ToList(),
        DefaultEffort = c.DefaultEffort,
        SupportsTokenBudget = c.SupportsTokenBudget,
        MinTokenBudget = c.MinTokenBudget,
        MaxTokenBudget = c.MaxTokenBudget
    };

    public static PermissionDecision ParseDecision(string? value) => value switch
    {
        "allowOnce" => PermissionDecision.AllowOnce,
        "allowRemember" => PermissionDecision.AllowRemember,
        _ => PermissionDecision.Deny
    };
}
