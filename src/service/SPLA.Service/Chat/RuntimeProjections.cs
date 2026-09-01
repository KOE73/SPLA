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

    /// <summary>All chats on disk, most-recent first, as wire summaries. <c>TurnActive</c> is read via
    /// <see cref="ChatRegistry.Peek"/>, which never loads a chat: a chat nobody has opened cannot be
    /// running a turn, so "not open" and "not running" are the same answer here.</summary>
    /// <summary>The same threshold the instance handlers use. A judgement about how long silence
    /// means "stuck", not something a deployment tunes — so a literal matching the default beats
    /// threading an option through every projection.</summary>
    private static readonly TimeSpan StallAfter = TimeSpan.FromMinutes(10);

    public static List<ChatSummaryDto> List(this ChatRegistry chats)
        => chats.Runtime.ChatManager.ListChats()
            .Select(c => new ChatSummaryDto
            {
                Id = c.Id,
                Title = c.Title,
                UpdatedAt = c.UpdatedAt.ToString("o"),
                TurnActive = chats.Peek(c.Id)?.IsTurnRunning ?? false,
                State = SPLA.Domain.Project.InstanceStates.Name(
                    chats.Runtime.StateOf(c.Id, StallAfter))
            })
            .ToList();

    /// <summary>Archived chats as wire summaries. An archived chat can never have an open runtime
    /// (<see cref="ChatRegistry.Archive"/> closes it first), so <c>TurnActive</c>/<c>State</c> are
    /// always the idle defaults — nothing to peek at.</summary>
    public static List<ChatSummaryDto> ListArchived(this ChatRegistry chats)
        => chats.Runtime.ChatManager.ListArchivedChats()
            .Select(c => new ChatSummaryDto
            {
                Id = c.Id,
                Title = c.Title,
                UpdatedAt = c.UpdatedAt.ToString("o")
            })
            .ToList();
}
