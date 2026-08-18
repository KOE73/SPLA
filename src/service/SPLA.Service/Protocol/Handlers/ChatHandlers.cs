using SPLA.MCP.Core.ToolSets;
using SPLA.Runtime;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>Chat lifecycle for a project: list/new/rename/delete, open/watch, send a turn, and per-chat
/// settings. Chat-list mutations broadcast to the whole project so every window stays in sync.</summary>
internal sealed class ChatHandlers : IMessageHandler
{
    public IEnumerable<string> HandledTypes =>
    [
        MessageTypes.ChatList, MessageTypes.ChatNew, MessageTypes.ChatRename, MessageTypes.ChatDelete,
        MessageTypes.ChatOpen, MessageTypes.ChatWatch, MessageTypes.ChatUnwatch,
        MessageTypes.ChatSend, MessageTypes.ChatSettings, MessageTypes.ChatReasoningGet,
        MessageTypes.ChatRewind, MessageTypes.ChatFork,
        MessageTypes.ChatSkillActivate, MessageTypes.ChatSkillDeactivate,
        MessageTypes.ChatToolSetDeactivate, MessageTypes.ChatDoubtClear,
    ];

    public Task HandleAsync(RequestContext ctx) => ctx.Env.Type switch
    {
        MessageTypes.ChatList     => List(ctx),
        MessageTypes.ChatNew      => New(ctx),
        MessageTypes.ChatRename   => Rename(ctx),
        MessageTypes.ChatDelete   => Delete(ctx),
        MessageTypes.ChatOpen     => Open(ctx),
        MessageTypes.ChatWatch    => Watch(ctx),
        MessageTypes.ChatUnwatch  => Unwatch(ctx),
        MessageTypes.ChatSend     => Send(ctx),
        MessageTypes.ChatSettings => Settings(ctx),
        MessageTypes.ChatReasoningGet => ReasoningGet(ctx),
        MessageTypes.ChatRewind   => Rewind(ctx),
        MessageTypes.ChatFork     => Fork(ctx),
        MessageTypes.ChatSkillActivate => SkillActivate(ctx),
        MessageTypes.ChatSkillDeactivate => SkillDeactivate(ctx),
        MessageTypes.ChatToolSetDeactivate => ToolSetDeactivate(ctx),
        MessageTypes.ChatDoubtClear => DoubtClear(ctx),
        _ => Task.CompletedTask
    };

    private static Task List(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        return ctx.Reply(MessageTypes.ChatListResult, new ChatListResultPayload { Chats = entry.Chats.List() });
    }

    private static async Task New(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ChatNewPayload>() ?? new ChatNewPayload();
        var chat = entry.Chats.CreateNew(p.Title);
        await ctx.Session.SendOpenedAsync(chat);
        await BroadcastChatList(ctx, projectId, entry.Chats);
    }

    private static async Task Rename(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ChatRenamePayload>();
        if (p != null) { entry.Chats.Rename(p.ChatId, p.Title); await BroadcastChatList(ctx, projectId, entry.Chats); }
    }

    private static async Task Delete(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ChatDeletePayload>();
        if (p != null)
        {
            entry.Chats.Delete(p.ChatId);
            ctx.Session.MarkChatClosed(p.ChatId);   // nothing left to watch
            await BroadcastChatList(ctx, projectId, entry.Chats);
        }
    }

    private static async Task Open(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ChatOpenPayload>();
        var chat = p != null ? entry.Chats.GetOrOpen(p.ChatId) : null;
        if (chat == null) { await ctx.Send(MessageTypes.Error, new ErrorPayload { Message = $"Chat not found: {p?.ChatId}" }); return; }
        await ctx.Session.SendOpenedAsync(chat);
    }

    private static Task Watch(RequestContext ctx)
    {
        // Registers this connection as a watcher of both the chat (for turn events) and the project
        // (for settings/usage broadcasts) without the side effects of ChatOpen.
        ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ChatOpenPayload>();
        if (!string.IsNullOrEmpty(p?.ChatId)) ctx.Session.MarkChatOpen(p.ChatId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops sending this chat's turn events to this connection.
    ///
    /// <para>Deliberately explicit rather than implied by opening another chat: a chat whose turn is
    /// still running keeps streaming into the client's background session for that chat, which is the
    /// entire point of being able to switch away mid-answer. The client is the only side that knows
    /// when it has stopped keeping that session — so the client says so, here.</para>
    /// </summary>
    private static Task Unwatch(RequestContext ctx)
    {
        var p = ctx.Payload<ChatOpenPayload>();
        if (!string.IsNullOrEmpty(p?.ChatId)) ctx.Session.MarkChatClosed(p.ChatId);
        return Task.CompletedTask;
    }

    private static async Task Send(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ChatSendPayload>();
        if (p == null) return;
        var chat = entry.Chats.GetOrOpen(p.ChatId);
        if (chat == null) { await ctx.Send(MessageTypes.Error, new ErrorPayload { Message = $"Chat not found: {p.ChatId}" }); return; }

        // The sender must watch this chat, otherwise the turn's stream (which fans out to watchers
        // only) would never reach the very client that started it.
        ctx.Session.MarkChatOpen(p.ChatId);
        ctx.Session.StartTurn(entry.Runtime, projectId, chat, p.Text, p.Images, ctx.HostStopping);
    }

    private static async Task Settings(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ChatSettingsPayload>();
        if (p == null) return;
        var chat = entry.Chats.GetOrOpen(p.ChatId);
        if (chat == null) return;
        chat.ApplySettings(p.Mode, p.ModelId, p.Temperature, p.Reasoning);
        await ctx.Session.SendOpenedAsync(chat);   // echo back the applied settings
    }

    /// <summary>
    /// Answers what this chat's model can do with its reasoning channel. Best-effort by construction:
    /// a provider that is down, or one that describes nothing, yields an unknown capability rather
    /// than an error — the status bar then says the lever is unavailable, which is the truth.
    /// </summary>
    private static async Task ReasoningGet(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ChatReasoningRequest>();
        if (p == null) return;
        var chat = entry.Chats.GetOrOpen(p.ChatId);
        if (chat == null) return;

        var caps = await chat.GetReasoningAsync(ctx.HostStopping);
        await ctx.Reply(MessageTypes.ChatReasoningResult, new ChatReasoningResult
        {
            ChatId = p.ChatId,
            ModelId = chat.ModelId,
            Reasoning = ProtocolMapper.ToDto(caps)
        });
    }

    private static async Task Rewind(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ChatRewindPayload>();
        var chat = p != null ? entry.Chats.GetOrOpen(p.ChatId) : null;
        if (chat == null || p == null) return;
        if (!chat.Rewind(p.MsgId, p.Before))
        {
            await ctx.Send(MessageTypes.Error, new ErrorPayload { Message = "Rewind failed — message not found or a turn is running." });
            return;
        }
        await ctx.Session.SendOpenedAsync(chat);   // re-render the truncated log
    }

    private static async Task Fork(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ChatForkPayload>();
        var fork = p != null ? entry.Chats.Fork(p.ChatId, p.MsgId) : null;
        if (fork == null)
        {
            await ctx.Send(MessageTypes.Error, new ErrorPayload { Message = "Fork failed — chat not found or a turn is running." });
            return;
        }
        await ctx.Session.SendOpenedAsync(fork);   // switches this client to the fork
        await BroadcastChatList(ctx, projectId, entry.Chats);
    }

    /// <summary>
    /// Hands a skill to the chat because a person picked it — the loan desk, and the cheapest of the
    /// three ways the ADR names of taking a book out.
    ///
    /// <para>The catalog costs nothing here: it is suppressed while a skill is active, so a chat with
    /// a handed-out skill carries no index at all. That closes the "weak model, small context" case
    /// outright, and it sidesteps the failure the stage-4 runs found — nothing has to be chosen by
    /// the model, so no competing tool can distract it from choosing.</para>
    /// </summary>
    private static async Task SkillActivate(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ChatSkillActivatePayload>();
        var chat = p != null ? entry.Chats.GetOrOpen(p.ChatId) : null;
        if (chat == null || p == null) return;

        // Refusals are reported, not swallowed: the user picked this from a list, so "nothing
        // happened" would be indistinguishable from a lost message.
        if (chat.ActivateSkill(p.SkillId) is { } reason)
        {
            await ctx.Send(MessageTypes.Error, new ErrorPayload { Message = $"Cannot use skill: {reason}" });
            return;
        }

        await ctx.Session.Hub.BroadcastToWatchersAsync(chat.ChatId, MessageTypes.ChatSkillState,
            new ChatSkillStatePayload { ChatId = chat.ChatId, ActiveSkillId = chat.ActiveSkillId });
    }

    /// <summary>
    /// Ends the chat's skill on the user's say-so.
    ///
    /// <para>Exists because <c>skill_deactivate</c> is the model's decision, and a model that never
    /// makes it leaves the chat unable to recover on its own: the skills index is suppressed while a
    /// skill is active, so it cannot be pointed at another one, and <c>skill_activate</c> refuses a
    /// second. The CLI's <c>/skills unload</c> only reaches chats inside that one process — this is
    /// the same exit for every window on the service.</para>
    /// </summary>
    private static async Task SkillDeactivate(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ChatSkillDeactivatePayload>();
        var chat = p != null ? entry.Chats.GetOrOpen(p.ChatId) : null;
        if (chat == null) return;

        chat.DeactivateSkill();

        // To watchers, not just the requester: two windows on one chat must not disagree about
        // whether a skill is running.
        await ctx.Session.Hub.BroadcastToWatchersAsync(chat.ChatId, MessageTypes.ChatSkillState,
            new ChatSkillStatePayload { ChatId = chat.ChatId, ActiveSkillId = chat.ActiveSkillId });
    }

    /// <summary>
    /// Clears the chat's doubt flag on the person's say-so.
    ///
    /// <para>Only a person can: the model has no tool for this, and it must not get one, because a
    /// mark that whatever it guards against can remove guards nothing. Clearing does not un-send
    /// anything that already left — it only affects what gets asked from here on, which is what
    /// bounds the cost of clearing it wrongly.</para>
    /// </summary>
    private static async Task DoubtClear(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ChatDoubtClearPayload>();
        var chat = p != null ? entry.Chats.GetOrOpen(p.ChatId) : null;
        if (chat == null) return;

        chat.Doubt.Clear();

        // Persisted immediately, not at the next turn: a person who cleared the flag and closed the
        // window would otherwise find it back, and a decision that does not stick is not one.
        chat.Save();

        await ctx.Session.Hub.BroadcastToWatchersAsync(chat.ChatId, MessageTypes.ChatDoubtState,
            new ChatDoubtStatePayload { ChatId = chat.ChatId, Doubt = DoubtDto(chat) });
    }

    /// <summary>The chat's flag as the wire carries it, causes included: a bare red dot with no
    /// account of itself gets dismissed on reflex.</summary>
    public static ChatDoubtDto DoubtDto(SPLA.Runtime.ChatRuntime chat) => new()
    {
        Raised = chat.Doubt.IsRaised,
        Causes = chat.Doubt.Causes
            .Select(c => new ChatDoubtCauseDto
            {
                Zone = c.Origin.Zone,
                What = c.What,
                At = c.At.ToString("o")
            })
            .ToList()
    };

    /// <summary>
    /// Lowers a tool set the model (or a skill) raised in this chat. The person's control over what
    /// the agent can reach — and what lets <c>toolset_deactivate</c> stay a permission for the model
    /// instead of a duty it has to remember.
    /// </summary>
    private static async Task ToolSetDeactivate(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ChatToolSetDeactivatePayload>();
        var chat = p != null ? entry.Chats.GetOrOpen(p.ChatId) : null;
        if (chat == null || p == null) return;

        chat.DeactivateToolSet(p.SetId);

        await ctx.Session.Hub.BroadcastToWatchersAsync(chat.ChatId, MessageTypes.ChatToolSetState,
            new ChatToolSetStatePayload { ChatId = chat.ChatId, Sets = ToolSetDtos(entry, chat) });
    }

    /// <summary>
    /// Every set this chat can see — raised or merely announced — with what raised it. Sets levelled
    /// off are omitted: for this chat they do not exist, and listing them would leak what the project
    /// holds into a window that is not allowed to use it.
    /// </summary>
    internal static List<ToolSetStateDto> ToolSetDtos(RuntimeEntry entry, ChatRuntime chat)
    {
        var registry = entry.Runtime.ToolSets;
        var raised = chat.ActiveToolSets.ToDictionary(a => a.SetId, StringComparer.OrdinalIgnoreCase);

        return registry.All
            .Select(set => (Set: set, Level: registry.LevelOf(set.Id)))
            .Where(x => x.Level != ToolSetLevel.Disabled)
            .Select(x =>
            {
                raised.TryGetValue(x.Set.Id, out var activation);
                var isRaised = raised.ContainsKey(x.Set.Id);
                return new ToolSetStateDto
                {
                    SetId = x.Set.Id,
                    By = isRaised ? activation.By.ToString().ToLowerInvariant() : string.Empty,
                    Reason = isRaised ? activation.Reason : null,
                    Level = ToolSetRegistry.Format(x.Level),
                    Disclosed = isRaised || x.Level == ToolSetLevel.Enabled
                };
            })
            // A set waiting on a skill costs nothing and is invisible to the model until it runs —
            // showing it would turn a cost readout into a settings list.
            .Where(dto => dto.Disclosed || dto.Level == ToolSetRegistry.Format(ToolSetLevel.AgentDemand))
            .ToList();
    }

    private static Task BroadcastChatList(RequestContext ctx, string projectId, ChatRegistry chats)
        => ctx.Session.Hub.BroadcastToProjectAsync(projectId, MessageTypes.ChatListResult,
            new ChatListResultPayload { Chats = chats.List() });
}
