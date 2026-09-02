using SPLA.Domain.Models;
using SPLA.Domain.Settings;
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

    /// <summary>All human-visible chats, most-recent first, each with its spawned descendants nested
    /// under it — the tree PLAN_20260902 wave 7 asks for ("список чатов становится деревом роль →
    /// чат"; ADR_20260827-2 §2.5, ADR_20260902 §2.3: "видны в дереве роль → чат, под своим
    /// родителем"). A spawned session never appears at the top level; it only ever shows up as
    /// someone's <see cref="ChatSummaryDto.Children"/>, however deep the spawn chain went (the
    /// recursion-depth-3 cap bounds this in practice, so the recursion below needs no cap of its own).
    /// A spawned session whose parent has itself been deleted or archived becomes unreachable from any
    /// root — the same "orphan" outcome the flat list already had, just no longer silently dropped
    /// from disk; retention (<c>agent.spawned_retention</c>) is still what reclaims it.</summary>
    public static List<ChatSummaryDto> List(this ChatRegistry chats)
    {
        var manager = chats.Runtime.ChatManager;
        var spawned = manager.ListSpawnedChats();
        var byParent = spawned
            .Where(c => !string.IsNullOrEmpty(c.Parent))
            .GroupBy(c => c.Parent!)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.UpdatedAt).ToList());

        return manager.ListChats()
            .Select(c => ToSummary(c, chats, byParent))
            .ToList();
    }

    private static ChatSummaryDto ToSummary(
        SPLA.Domain.Models.ChatSession c, ChatRegistry chats, Dictionary<string, List<SPLA.Domain.Models.ChatSession>> byParent)
    {
        var children = byParent.TryGetValue(c.Id, out var kids)
            ? kids.Select(k => ToSummary(k, chats, byParent)).ToList()
            : null;

        // A spawned session's one run does not go through ChatPump/Turns — SpawnedAgentRunner drives
        // it directly (ADR_20260902 §2.1) — so StateOf's activity lookup never sees it and would
        // otherwise report "idle" for a run that is very much in progress. Spawn.Outcome is null for
        // exactly that duration (see ChatSessionSpawnInfo's own remark), so it stands in here.
        var stillRunning = ChatManager.IsSpawned(c) && c.Spawn?.Outcome is null;

        // Summed here rather than carried per message onto the wire: nobody downstream needs the
        // per-message figure, only the chat's running total, and the messages are already in memory —
        // ListChats/ListSpawnedChats already deserialized the whole file to build `c` itself.
        var promptTokens = c.Messages.Sum(m => m.PromptTokens ?? 0);
        var completionTokens = c.Messages.Sum(m => m.CompletionTokens ?? 0);
        var hasUsage = c.Messages.Any(m => m.PromptTokens is not null || m.CompletionTokens is not null);

        return new ChatSummaryDto
        {
            Id = c.Id,
            Title = c.Title,
            UpdatedAt = c.UpdatedAt.ToString("o"),
            TurnActive = stillRunning || (chats.Peek(c.Id)?.IsTurnRunning ?? false),
            State = stillRunning
                ? SPLA.Domain.Project.InstanceStates.Name(SPLA.Domain.Project.InstanceState.Working)
                : SPLA.Domain.Project.InstanceStates.Name(chats.Runtime.StateOf(c.Id, StallAfter)),
            As = c.As,
            Origin = c.Origin,
            Parent = c.Parent,
            ModelId = c.ModelId,
            PromptTokens = hasUsage ? promptTokens : null,
            CompletionTokens = hasUsage ? completionTokens : null,
            Children = children is { Count: > 0 } ? children : null
        };
    }

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
