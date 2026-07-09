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

    private static Task Cancel(RequestContext ctx)
    {
        if (ctx.Env.ChatId != null) ctx.Session.TryCancelTurn(ctx.Env.ChatId);
        return Task.CompletedTask;
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
