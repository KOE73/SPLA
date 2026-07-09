using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>Read-side surfaces over a project's workspace and live agent: the debug snapshot, schema
/// lookup, and the file browser/editor (browse/read/write, each path-guarded in <see cref="WorkspaceOps"/>).</summary>
internal sealed class WorkspaceHandlers : IMessageHandler
{
    public IEnumerable<string> HandledTypes =>
    [
        MessageTypes.DebugRequest, MessageTypes.SchemaGet,
        MessageTypes.FsBrowse, MessageTypes.FsRead, MessageTypes.FsWrite,
    ];

    public Task HandleAsync(RequestContext ctx) => ctx.Env.Type switch
    {
        MessageTypes.DebugRequest => Debug(ctx),
        MessageTypes.SchemaGet    => Schema(ctx),
        MessageTypes.FsBrowse     => FsBrowse(ctx),
        MessageTypes.FsRead       => FsRead(ctx),
        MessageTypes.FsWrite      => FsWrite(ctx),
        _ => Task.CompletedTask
    };

    private static Task Debug(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<DebugRequestPayload>();
        var chat = ctx.Env.ChatId != null ? entry.Chats.GetOrOpen(ctx.Env.ChatId) : null;
        var snap = new LiveAgentInspector(entry.Runtime).Snapshot(p?.Kind ?? "", chat);
        return ctx.Session.SendAsync(MessageTypes.DebugSnapshot, snap, ctx.Env.ChatId, ctx.Env.RequestId);
    }

    private static Task Schema(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<SchemaGetPayload>();
        if (string.IsNullOrWhiteSpace(p?.Name))
            return ctx.Reply(MessageTypes.SchemaResult, new SchemaResultPayload { Error = "Name is required." });
        return ctx.Reply(MessageTypes.SchemaResult, SchemaOps.Get(entry.Runtime.SchemaRegistry, p.Name));
    }

    private static Task FsBrowse(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<FsBrowsePayload>();
        var root = entry.Runtime.Settings.WorkspacePath;
        return ctx.Reply(MessageTypes.FsBrowseResult, WorkspaceOps.Browse(root, p?.ParentRef));
    }

    private static Task FsRead(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<FsReadPayload>();
        if (string.IsNullOrWhiteSpace(p?.Ref))
            return ctx.Reply(MessageTypes.FsReadResult, new FsReadResultPayload { Error = "Ref is required." });
        var root = entry.Runtime.Settings.WorkspacePath;
        return ctx.Reply(MessageTypes.FsReadResult, WorkspaceOps.Read(root, p.Ref));
    }

    private static Task FsWrite(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<FsWritePayload>();
        if (string.IsNullOrWhiteSpace(p?.Ref))
            return ctx.Reply(MessageTypes.FsWriteResult, new FsWriteResultPayload { Error = "Ref is required." });
        var root = entry.Runtime.Settings.WorkspacePath;
        return ctx.Reply(MessageTypes.FsWriteResult, WorkspaceOps.Write(root, p.Ref, p.Text ?? ""));
    }
}
