using SPLA.Plugins.Ssh;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// Live SSH terminal channel over the project's <see cref="SshSessionHub"/>. A terminal is a VIEW:
/// <c>terminal.open</c> either ATTACHES to an existing session (<c>session</c> id, replaying recent
/// output) or opens a new session on a configured host; input/close drive the view, while sessions
/// themselves outlive terminals (<c>ssh.session.close</c> ends one for everyone). Host config is
/// read from the project's <c>plugins.ssh.settings</c> and credentials are resolved server-side —
/// the browser never sees one.
/// </summary>
internal sealed class TerminalHandlers : IMessageHandler
{
    public IEnumerable<string> HandledTypes =>
    [
        MessageTypes.TerminalOpen, MessageTypes.TerminalInput,
        MessageTypes.TerminalResize, MessageTypes.TerminalClose,
        MessageTypes.SshSessionsGet, MessageTypes.SshSessionClose,
    ];

    public Task HandleAsync(RequestContext ctx) => ctx.Env.Type switch
    {
        MessageTypes.TerminalOpen    => Open(ctx),
        MessageTypes.TerminalInput   => Input(ctx),
        MessageTypes.TerminalResize  => Resize(ctx),
        MessageTypes.TerminalClose   => Close(ctx),
        MessageTypes.SshSessionsGet  => Sessions(ctx),
        MessageTypes.SshSessionClose => SessionClose(ctx),
        _ => Task.CompletedTask
    };

    private static async Task Open(RequestContext ctx)
    {
        var p = ctx.Payload<TerminalOpenPayload>();
        if (p == null || string.IsNullOrWhiteSpace(p.TerminalId))
            return;

        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var settings = entry.Runtime.Settings;
        var hub = SshSessionHub.For(settings);

        try
        {
            SshLiveSession? session = null;

            // Attach to an existing session by id — the "watch the agent / reattach" path.
            if (!string.IsNullOrWhiteSpace(p.Session))
            {
                session = hub.Get(p.Session);
                if (session == null)
                {
                    await ctx.Send(MessageTypes.TerminalClosed, new TerminalClosedPayload
                    { TerminalId = p.TerminalId, Reason = $"no such session '{p.Session}'" });
                    return;
                }
            }
            else
            {
                var ssh = SshSettings.FromBlob(settings.Plugins.GetValueOrDefault("ssh")?.Settings);
                var name = p.Host ?? ssh.DefaultHost ?? (ssh.Hosts.Count == 1 ? ssh.Hosts.Keys.First() : null);
                if (name == null || !ssh.Hosts.TryGetValue(name, out var cfg))
                {
                    await ctx.Send(MessageTypes.TerminalClosed, new TerminalClosedPayload
                    {
                        TerminalId = p.TerminalId,
                        Reason = name == null ? "no SSH host configured" : $"unknown host '{name}'"
                    });
                    return;
                }

                session = await hub.OpenAsync(name, cfg, ssh.TimeoutSeconds, settings.SecretResolver,
                    openedBy: "human", ctx.HostStopping, (uint)Math.Clamp(p.Cols, 20, 500), (uint)Math.Clamp(p.Rows, 5, 200));
            }

            await ctx.Session.Terminals.AttachAsync(p.TerminalId, session);
        }
        catch (Exception ex)
        {
            await ctx.Send(MessageTypes.TerminalClosed, new TerminalClosedPayload
            {
                TerminalId = p.TerminalId,
                Reason = "open failed: " + ex.Message
            });
        }
    }

    private static Task Input(RequestContext ctx)
    {
        var p = ctx.Payload<TerminalInputPayload>();
        if (p != null && !string.IsNullOrEmpty(p.TerminalId))
            ctx.Session.Terminals.Input(p.TerminalId, p.Data);
        return Task.CompletedTask;
    }

    private static Task Resize(RequestContext ctx)
    {
        var p = ctx.Payload<TerminalResizePayload>();
        if (p != null && !string.IsNullOrEmpty(p.TerminalId))
            ctx.Session.Terminals.Resize(p.TerminalId, p.Cols, p.Rows);
        return Task.CompletedTask;
    }

    private static async Task Close(RequestContext ctx)
    {
        var p = ctx.Payload<TerminalClosePayload>();
        if (p != null && !string.IsNullOrEmpty(p.TerminalId))
            await ctx.Session.Terminals.DetachAsync(p.TerminalId);
    }

    /// <summary>The SSH picker's snapshot: configured hosts (live settings), every live session in
    /// the hub (id, host, opener, viewers), and this connection's terminals. Names only.</summary>
    private static async Task Sessions(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var settings = entry.Runtime.Settings;
        var ssh = SshSettings.FromBlob(settings.Plugins.GetValueOrDefault("ssh")?.Settings);
        var hub = SshSessionHub.For(settings);

        var result = new SshSessionsResultPayload();
        foreach (var (name, cfg) in ssh.Hosts)
            result.Hosts.Add(new SshHostDto
            {
                Name = name, Host = cfg.Host, Port = cfg.Port,
                IsDefault = string.Equals(name, ssh.DefaultHost, StringComparison.OrdinalIgnoreCase),
                Description = cfg.Description
            });

        foreach (var s in hub.List())
            result.Sessions.Add(new SshSessionDto
            {
                Id = s.Id, Host = s.HostName, OpenedBy = s.OpenedBy, Viewers = s.ViewerCount
            });

        foreach (var (terminalId, session) in ctx.Session.Terminals.List())
            result.Terminals.Add(new SshTerminalDto { TerminalId = terminalId, Host = session.HostName, SessionId = session.Id });

        await ctx.Reply(MessageTypes.SshSessionsResult, result);
    }

    /// <summary>Ends a session for everyone (all attached terminals show "session closed").</summary>
    private static Task SessionClose(RequestContext ctx)
    {
        var p = ctx.Payload<SshSessionClosePayload>();
        if (p != null && !string.IsNullOrWhiteSpace(p.SessionId))
        {
            var (entry, _) = ctx.Session.Resolve(ctx.Env);
            SshSessionHub.For(entry.Runtime.Settings).Close(p.SessionId);
        }
        return Task.CompletedTask;
    }
}
