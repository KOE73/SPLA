using System.Linq;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>Read-side surfaces over a project's workspace and live agent: the debug snapshot, schema
/// lookup, the file browser/editor (browse/read/write, each path-guarded in <see cref="WorkspaceOps"/>),
/// and reading back one finished spawned run.</summary>
internal sealed class WorkspaceHandlers : IMessageHandler
{
    public IEnumerable<string> HandledTypes =>
    [
        MessageTypes.DebugRequest, MessageTypes.SchemaGet,
        MessageTypes.FsBrowse, MessageTypes.FsRead, MessageTypes.FsWrite,
        MessageTypes.SubagentGet,
    ];

    public Task HandleAsync(RequestContext ctx) => ctx.Env.Type switch
    {
        MessageTypes.DebugRequest => Debug(ctx),
        MessageTypes.SchemaGet    => Schema(ctx),
        MessageTypes.FsBrowse     => FsBrowse(ctx),
        MessageTypes.FsRead       => FsRead(ctx),
        MessageTypes.FsWrite      => FsWrite(ctx),
        MessageTypes.SubagentGet  => SubagentGet(ctx),
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
        var boundary = BoundaryOf(entry);
        return ctx.Reply(MessageTypes.FsBrowseResult, WorkspaceOps.Browse(boundary, p?.ParentRef));
    }

    private static Task FsRead(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<FsReadPayload>();
        if (string.IsNullOrWhiteSpace(p?.Ref))
            return ctx.Reply(MessageTypes.FsReadResult, new FsReadResultPayload { Error = "Ref is required." });
        var boundary = BoundaryOf(entry);
        return ctx.Reply(MessageTypes.FsReadResult, WorkspaceOps.Read(boundary, p.Ref));
    }

    private static Task FsWrite(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<FsWritePayload>();
        if (string.IsNullOrWhiteSpace(p?.Ref))
            return ctx.Reply(MessageTypes.FsWriteResult, new FsWriteResultPayload { Error = "Ref is required." });
        var boundary = BoundaryOf(entry);
        return ctx.Reply(MessageTypes.FsWriteResult, WorkspaceOps.Write(boundary, p.Ref, p.Text ?? ""));
    }

    /// <summary>
    /// Reads one spawned session's transcript back off disk by chat id (<c>SpawnedRunLog</c> is gone —
    /// see <c>docs/adr/ADR_20260902_core_session-unification.md</c> §2.1). A miss is a normal answer
    /// (<c>found: false</c>), not an error: the id may never have existed, may name a human chat (this
    /// must not become a generic chat-by-id reader), or may have been trimmed by
    /// <c>agent.spawned_retention</c> — the on-disk ring's own way of being bounded.
    /// </summary>
    private static Task SubagentGet(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<SubagentGetPayload>();
        var chat = string.IsNullOrWhiteSpace(p?.RunId) ? null : entry.Runtime.ChatManager.LoadChat(p.RunId);
        if (chat != null && chat.Origin != "spawned") chat = null;

        var result = chat is null
            ? new SubagentResultPayload { Found = false }
            : new SubagentResultPayload
            {
                Found = true,
                RunId = chat.Id,
                Label = chat.Title,
                SkillId = chat.Spawn?.SkillId,
                Mode = chat.Spawn?.Mode ?? "",
                StartedAt = (chat.Spawn?.StartedAt ?? chat.CreatedAt).ToString("o"),
                // Still running: there is no finish time yet, so the last update stands in for it —
                // an honest "as of now", not a fabricated end.
                FinishedAt = (chat.Spawn?.FinishedAt ?? chat.UpdatedAt).ToString("o"),
                Outcome = chat.Spawn?.Outcome ?? "running",
                Error = chat.Spawn?.Error,
                Result = chat.Messages.LastOrDefault(m => m.Role == "assistant")?.Content ?? "",
                Messages = chat.Messages.Select(ToTranscriptDto).ToList()
            };

        return ctx.Reply(MessageTypes.SubagentResult, result);
    }

    /// <summary>
    /// Persisted spawned-session messages, straight off disk, into the same DTO a live chat's
    /// transcript uses — a reader that already renders one chat renders this without new code. Small,
    /// deliberate subset of <see cref="SPLA.Runtime.ChatRuntime"/>'s own hydration (system prompt is
    /// never persisted, so there is nothing to strip here; image sidecars are omitted — a spawned
    /// session's transcript is read for its text, not replayed as a chat).
    /// </summary>
    private static ChatMessageDto ToTranscriptDto(SPLA.Domain.Models.ChatSessionMessage m) => new()
    {
        MsgId = m.Id,
        Role = m.Role,
        Content = m.Content,
        Reasoning = m.Reasoning,
        CreatedAt = m.CreatedAt.ToString("o"),
        ToolCallId = m.ToolCallId,
        ToolCalls = m.ToolCalls?.Select(ProtocolMapper.ToDto).ToList(),
        Compacted = m.Retention == "never" && m.CompactedBy != null,
        CompactSummary = m.CompactSummary
    };

    /// <summary>The project's own boundary. Without a manifest there is no project and no boundary to
    /// ask for — but this surface has always been bounded by the launch directory, and taking that
    /// away would open a human surface while closing an agent one.</summary>
    private static SPLA.Domain.Host.PathBoundary BoundaryOf(SPLA.Runtime.RuntimeEntry entry)
    {
        var settings = entry.Runtime.Settings;
        var boundary = settings.Project.GetBoundary();
        return boundary.IsBounded ? boundary : new SPLA.Domain.Host.PathBoundary(settings.WorkspacePath);
    }
}
