using SPLA.Service.Contracts;

namespace SPLA.Service;

internal sealed class PluginPanelHandlers : IMessageHandler
{
    public IEnumerable<string> HandledTypes =>
        [MessageTypes.PluginPanelOpen, MessageTypes.PluginPanelInput, MessageTypes.PluginPanelClose];

    public async Task HandleAsync(RequestContext ctx)
    {
        switch (ctx.Env.Type)
        {
            case MessageTypes.PluginPanelOpen:
            {
                var payload = ctx.Payload<PluginPanelOpenPayload>() ?? throw new InvalidOperationException("Missing plugin panel payload.");
                var (entry, _) = ctx.Session.Resolve(ctx.Env);
                await ctx.Session.PluginPanels.OpenAsync(entry.Runtime, payload, ctx.HostStopping);
                break;
            }
            case MessageTypes.PluginPanelInput:
                await ctx.Session.PluginPanels.InputAsync(ctx.Payload<PluginPanelInputPayload>()!, ctx.HostStopping);
                break;
            case MessageTypes.PluginPanelClose:
                await ctx.Session.PluginPanels.CloseAsync(ctx.Payload<PluginPanelClosePayload>()!.PanelId);
                break;
        }
    }
}
