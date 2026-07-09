using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>Chat lifecycle for a project: list/new/rename/delete, open/watch, send a turn, and per-chat
/// settings. Chat-list mutations broadcast to the whole project so every window stays in sync.</summary>
internal sealed class ChatHandlers : IMessageHandler
{
    public IEnumerable<string> HandledTypes =>
    [
        MessageTypes.ChatList, MessageTypes.ChatNew, MessageTypes.ChatRename, MessageTypes.ChatDelete,
        MessageTypes.ChatOpen, MessageTypes.ChatWatch, MessageTypes.ChatSend, MessageTypes.ChatSettings,
    ];

    public Task HandleAsync(RequestContext ctx) => ctx.Env.Type switch
    {
        MessageTypes.ChatList     => List(ctx),
        MessageTypes.ChatNew      => New(ctx),
        MessageTypes.ChatRename   => Rename(ctx),
        MessageTypes.ChatDelete   => Delete(ctx),
        MessageTypes.ChatOpen     => Open(ctx),
        MessageTypes.ChatWatch    => Watch(ctx),
        MessageTypes.ChatSend     => Send(ctx),
        MessageTypes.ChatSettings => Settings(ctx),
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
        if (p != null) { entry.Chats.Delete(p.ChatId); await BroadcastChatList(ctx, projectId, entry.Chats); }
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
        chat.ApplySettings(p.Mode, p.ConnectionId);
        await ctx.Session.SendOpenedAsync(chat);   // echo back the applied settings
    }

    private static Task BroadcastChatList(RequestContext ctx, string projectId, ChatRegistry chats)
        => ctx.Session.Hub.BroadcastToProjectAsync(projectId, MessageTypes.ChatListResult,
            new ChatListResultPayload { Chats = chats.List() });
}
