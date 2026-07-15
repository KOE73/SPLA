using SPLA.Domain.Models;
using SPLA.Runtime;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// Wire projections of the transport-neutral runtime objects. The runtime (SPLA.Runtime) knows
/// nothing about DTOs or URLs — this is the service-side seam that maps its state onto the
/// WebSocket protocol, kept as extensions so call sites read exactly as before the split.
/// </summary>
public static class RuntimeProjections
{
    /// <summary>The chat's display messages projected to wire DTOs (system prompt hidden).
    /// Persisted image filenames are surfaced as /chat-image URLs so reopened chats show their pictures.</summary>
    public static List<ChatMessageDto> SnapshotMessages(this ChatRuntime chat)
        => chat.DisplayMessages
            .Select(m =>
            {
                var dto = ProtocolMapper.ToDto(m);
                var files = chat.ImageFilesFor(m);
                if (files is { Count: > 0 })
                    dto.Images = files.Select(f => ChatImages.Url(chat.ChatId, f)).ToList();
                return dto;
            })
            .ToList();

    /// <summary>All chats on disk, most-recent first, as wire summaries.</summary>
    public static List<ChatSummaryDto> List(this ChatRegistry chats)
        => chats.Runtime.ChatManager.ListChats()
            .Select(c => new ChatSummaryDto
            {
                Id = c.Id,
                Title = c.Title,
                UpdatedAt = c.UpdatedAt.ToString("o")
            })
            .ToList();
}
