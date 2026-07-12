using SPLA.Plugins.Ssh;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// Live SSH terminal channel (phase B of the live console). Bridges the browser xterm to an SSH pty:
/// <c>terminal.open/input/resize/close</c> from the client drive the connection's
/// <see cref="SshTerminalManager"/>, which streams raw output back as <c>terminal.data</c>. Host
/// config is read from the project's <c>plugins.ssh.settings</c> and the password is resolved
/// server-side from the secret store — the browser never sees a credential.
/// </summary>
internal sealed class TerminalHandlers : IMessageHandler
{
    public IEnumerable<string> HandledTypes =>
    [
        MessageTypes.TerminalOpen, MessageTypes.TerminalInput,
        MessageTypes.TerminalResize, MessageTypes.TerminalClose,
    ];

    public Task HandleAsync(RequestContext ctx) => ctx.Env.Type switch
    {
        MessageTypes.TerminalOpen   => Open(ctx),
        MessageTypes.TerminalInput  => Input(ctx),
        MessageTypes.TerminalResize => Resize(ctx),
        MessageTypes.TerminalClose  => Close(ctx),
        _ => Task.CompletedTask
    };

    private static async Task Open(RequestContext ctx)
    {
        var p = ctx.Payload<TerminalOpenPayload>();
        if (p == null || string.IsNullOrWhiteSpace(p.TerminalId))
            return;

        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var settings = entry.Runtime.Settings;
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

        try
        {
            await ctx.Session.Terminals.OpenAsync(
                p.TerminalId, name, cfg, ssh.TimeoutSeconds, settings.SecretResolver, p.Cols, p.Rows, ctx.HostStopping);
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
            await ctx.Session.Terminals.CloseAsync(p.TerminalId);
    }
}
