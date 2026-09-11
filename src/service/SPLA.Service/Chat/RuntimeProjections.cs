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
        => chat.SnapshotMessages(chat.DisplayMessages);

    /// <summary>Same projection as the parameterless overload, but over a caller-supplied message list
    /// instead of re-reading <see cref="ChatRuntime.DisplayMessages"/> live. Used when the messages must
    /// come from an already-captured <c>ChatFeedSnapshot</c> rather than the conversation as it stands
    /// at call time — e.g. the overflow-resync path in <c>ChatFeedWireSubscriber</c>, where re-reading
    /// live here would reintroduce the exact race <see cref="ChatFeed.SubscribeQueuedWithSnapshot{T}"/>
    /// closes for every other field of the snapshot.</summary>
    public static List<ChatMessageDto> SnapshotMessages(this ChatRuntime chat, IEnumerable<ChatMessage> messages)
        => messages
            .Where(m => m.Role != ChatRole.System)
            .Select(m =>
            {
                var dto = ProtocolMapper.ToDto(m);
                var files = chat.ImageFilesFor(m);
                if (files is { Count: > 0 })
                    dto.Images = files
                        .Select(f => new ImageDto { Url = ChatImages.Url(chat.ChatId, f.File), Label = f.Label })
                        .ToList();
                return dto;
            })
            .ToList();

    /// <summary>The same projection for a chat read straight off the disk, with no runtime behind it
    /// (<see cref="ChatRegistry.ReadArchived"/>). Reads the persisted messages rather than a live
    /// conversation, which is why it cannot simply call the overload above — but it must agree with it
    /// on every visible detail, or an archived chat would render subtly differently from the same chat
    /// before it was archived, and the difference would look like data loss.</summary>
    public static List<ChatMessageDto> SnapshotMessages(this ChatSession chat)
        => chat.Messages
            .Where(m => !string.Equals(m.Role, "system", StringComparison.OrdinalIgnoreCase))
            .Select(m => new ChatMessageDto
            {
                MsgId = m.Id,
                Role = m.Role.ToLowerInvariant(),
                Content = m.Content,
                Reasoning = m.Reasoning,
                CreatedAt = m.CreatedAt.ToString("o"),
                ToolCallId = m.ToolCallId,
                PeerFrom = m.PeerFrom,
                ToolCalls = m.ToolCalls?.Select(ProtocolMapper.ToDto).ToList(),
                Attempts = m.Attempts?.Select(a => new AttemptDto
                {
                    Index = a.Index,
                    Outcome = a.Outcome,
                    Note = a.Note,
                    Chars = a.Chars,
                    DurationMs = a.DurationMs,
                    WaitMs = a.WaitMs,
                    WaitStated = a.WaitStated,
                    Content = a.Content,
                    Reasoning = a.Reasoning
                }).ToList(),
                Images = m.Images is { Count: > 0 }
                    ? m.Images.Select(f => new ImageDto { Url = ChatImages.Url(chat.Id, f.File), Label = f.Label }).ToList()
                    : null
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
        var allChats = manager.ListChatsAndSpawned();
        var spawned = allChats.Where(ChatManager.IsSpawned).ToList();
        var byParent = spawned
            .Where(c => !string.IsNullOrEmpty(c.Parent))
            .GroupBy(c => c.Parent!)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.UpdatedAt).ToList());

        return allChats
            .Where(c => !ChatManager.IsSpawned(c))
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

        var (promptTokens, completionTokens) = ResolveTokenTotals(c);

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
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            Children = children is { Count: > 0 } ? children : null
        };
    }

    /// <summary>
    /// This chat's lifetime token totals: the header's cached figures when it has them, and a sum over
    /// the messages when it does not. Both header fields are written together by
    /// <c>ChatRuntime.Save</c> — either both numbers or both null — so "both null" is exactly the
    /// signal that this file predates the fields and has to be summed the old way.
    /// <para>
    /// Absence stays absence in both paths: null means nobody ever reported usage, and 0 means it was
    /// reported and was zero. Public, and separate from <see cref="ToSummary"/>, so the fallback can be
    /// tested against the real thing — a test that re-implements this rule instead would keep passing
    /// after the rule was deleted.
    /// </para>
    /// </summary>
    public static (int? Prompt, int? Completion) ResolveTokenTotals(SPLA.Domain.Models.ChatSession c)
    {
        if (c.PromptTokensTotal is not null || c.CompletionTokensTotal is not null)
            return (c.PromptTokensTotal, c.CompletionTokensTotal);

        var hasUsage = c.Messages.Any(m => m.PromptTokens is not null || m.CompletionTokens is not null);
        return hasUsage
            ? (c.Messages.Sum(m => m.PromptTokens ?? 0), c.Messages.Sum(m => m.CompletionTokens ?? 0))
            : (null, null);
    }

    /// <summary>The project-wide correspondence graph (PLAN_20260902 wave 7б;
    /// <c>docs/adr/ADR_20260827-2_core_roles.md</c> §2.5's last row), assembled purely from sessions on
    /// disk via <see cref="SPLA.Domain.Settings.CorrespondenceGraph.Build"/> — never from which chats
    /// this or any other window happens to have open (decision 3 of the wave).</summary>
    public static List<CorrespondenceEdgeDto> CorrespondenceGraph(this ChatRegistry chats)
        => SPLA.Domain.Settings.CorrespondenceGraph.Build(chats.Runtime.ChatManager)
            .Select(e => new CorrespondenceEdgeDto
            {
                FromChatId = e.FromChatId,
                FromRole = e.FromRole,
                ToChatId = e.ToChatId,
                ToRole = e.ToRole,
                Topic = e.Topic,
                RepliesFromInitiator = e.RepliesFromInitiator,
                VolumeFromInitiator = e.VolumeFromInitiator,
                RepliesFromCorrespondent = e.RepliesFromCorrespondent,
                VolumeFromCorrespondent = e.VolumeFromCorrespondent
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
