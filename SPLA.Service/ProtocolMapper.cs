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
        ToolCallId = m.ToolCallId,
        IsEphemeral = m.IsEphemeral,
        ToolCalls = m.ToolCalls?.Select(ToDto).ToList()
    };

    public static ToolCallDto ToDto(ToolCall tc) => new()
    {
        Id = tc.Id,
        Name = tc.Function.Name,
        Arguments = tc.Function.Arguments
    };

    public static PermissionDecision ParseDecision(string? value) => value switch
    {
        "allowOnce" => PermissionDecision.AllowOnce,
        "allowRemember" => PermissionDecision.AllowRemember,
        _ => PermissionDecision.Deny
    };
}
