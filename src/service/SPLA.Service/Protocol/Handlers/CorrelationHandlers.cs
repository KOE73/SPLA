using System.Linq;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>Round-trip completions and connection-level signals that carry no project scope: focus
/// echo, turn cancel, and the permission/clarify answers that unblock a running turn.</summary>
internal sealed class CorrelationHandlers : IMessageHandler
{
    public IEnumerable<string> HandledTypes =>
    [
        MessageTypes.FocusSet, MessageTypes.Cancel,
        MessageTypes.PermissionDecision, MessageTypes.ClarifyChoice,
    ];

    public Task HandleAsync(RequestContext ctx) => ctx.Env.Type switch
    {
        MessageTypes.FocusSet           => Focus(ctx),
        MessageTypes.Cancel             => Cancel(ctx),
        MessageTypes.PermissionDecision => Permission(ctx),
        MessageTypes.ClarifyChoice      => Clarify(ctx),
        _ => Task.CompletedTask
    };

    private static Task Focus(RequestContext ctx)
    {
        // A window focused a chat; echo to everyone so tear-off windows follow the active chat.
        // Connection-wide, not project-scoped — a debug/tear-off window may follow focus regardless
        // of which project it is itself looking at.
        var p = ctx.Payload<FocusPayload>();
        return p != null && !string.IsNullOrEmpty(p.ChatId)
            ? ctx.Session.Hub.BroadcastAsync(MessageTypes.FocusChanged, new FocusPayload { ChatId = p.ChatId })
            : Task.CompletedTask;
    }

    /// <summary>
    /// Stop = STOP (PLAN_20260825 wave C, ADR §2.4): cancels the running turn AND every live background
    /// task this chat holds, then disarms the pump so a background task's cancellation delivery — which
    /// still lands in the inbox, deliberately, so silence never reads as success — cannot immediately
    /// wake a fresh turn. Without the disarm, Stop would be a half-second pause, not a stop.
    /// <para>
    /// Reached via <see cref="ChatRegistry.Peek"/>, never <c>GetOrOpen</c>: a chat nobody has opened
    /// this process has nothing running to stop, and <c>Peek</c> is the call that says so without
    /// loading it from disk to find out.
    /// </para>
    /// <para>The inbox itself is never touched here — "stop" and "forget what I was told" are different
    /// requests (ADR §2.4); already-delivered results and anything queued ride the next human turn.</para>
    /// </summary>
    private static Task Cancel(RequestContext ctx)
    {
        if (ctx.Env.ChatId == null) return Task.CompletedTask;
        var chatId = ctx.Env.ChatId;

        ctx.Session.TryCancelTurn(chatId);

        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var chat = entry.Chats.Peek(chatId);
        if (chat == null) return Task.CompletedTask;

        // Suppress BEFORE cancelling, not after: a cancelled task delivers its own cancellation into
        // the inbox, and every such delivery pokes the pump. Doing it the other way round leaves a
        // window — narrow, since the pump debounces, but a real one — where Stop is a pause.
        chat.SuppressAutoWake();
        var cancelledTasks = chat.Tasks.CancelAll();

        // A plain Stop with no background work is already visible through TurnComplete{Cancelled=true};
        // "stopped the turn and 0 background tasks" would be noise nobody asked for.
        if (cancelledTasks.Count == 0) return Task.CompletedTask;

        var names = string.Join(", ", cancelledTasks.Select(t => $"{t.Id} ({t.ToolName})"));
        var text = $"Stopped the turn and {cancelledTasks.Count} background task" +
                   (cancelledTasks.Count == 1 ? "" : "s") + $": {names}.";
        return ctx.Session.Hub.BroadcastToWatchersAsync(chatId, MessageTypes.Notice, new NoticePayload { Text = text });
    }

    private static Task Permission(RequestContext ctx)
    {
        var p = ctx.Payload<PermissionDecisionPayload>();
        if (ctx.Env.RequestId != null)
            ctx.Session.CompletePermission(ctx.Env.RequestId, ProtocolMapper.ParseDecision(p?.Decision));
        return Task.CompletedTask;
    }

    private static Task Clarify(RequestContext ctx)
    {
        var p = ctx.Payload<ClarifyChoicePayload>();
        if (ctx.Env.RequestId != null)
            ctx.Session.CompleteClarify(ctx.Env.RequestId, p?.Choice);
        return Task.CompletedTask;
    }
}
