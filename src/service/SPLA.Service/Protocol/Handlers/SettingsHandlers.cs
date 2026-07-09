using System.Text.Json;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>Project settings surfaces: agent config, plugins (get/save/action), token usage, appearance,
/// and the machine-level file-association registration. Mutations broadcast to the project's watchers.</summary>
internal sealed class SettingsHandlers : IMessageHandler
{
    public IEnumerable<string> HandledTypes =>
    [
        MessageTypes.AgentGet, MessageTypes.AgentSave,
        MessageTypes.PluginsGet, MessageTypes.PluginsSave, MessageTypes.PluginAction,
        MessageTypes.UsageGet, MessageTypes.AppearanceSave, MessageTypes.SystemRegisterAssociation,
    ];

    public Task HandleAsync(RequestContext ctx) => ctx.Env.Type switch
    {
        MessageTypes.AgentGet                  => AgentGet(ctx),
        MessageTypes.AgentSave                 => AgentSave(ctx),
        MessageTypes.PluginsGet                => PluginsGet(ctx),
        MessageTypes.PluginsSave               => PluginsSave(ctx),
        MessageTypes.PluginAction              => PluginAction(ctx),
        MessageTypes.UsageGet                  => UsageGet(ctx),
        MessageTypes.AppearanceSave            => AppearanceSave(ctx),
        MessageTypes.SystemRegisterAssociation => RegisterAssociation(ctx),
        _ => Task.CompletedTask
    };

    private static Task AgentGet(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        return ctx.Reply(MessageTypes.AgentResult, SettingsOps.GetAgent(entry.Runtime));
    }

    private static async Task AgentSave(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<AgentSettingsPayload>();
        if (p != null)
            await ctx.Session.Hub.BroadcastToProjectAsync(projectId, MessageTypes.AgentResult, SettingsOps.SaveAgent(entry.Runtime, p));
    }

    private static Task PluginsGet(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        return ctx.Reply(MessageTypes.PluginsResult, SettingsOps.GetPlugins(entry.Runtime));
    }

    private static async Task PluginsSave(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<PluginsPayload>();
        await ctx.Session.Hub.BroadcastToProjectAsync(projectId, MessageTypes.PluginsResult,
            SettingsOps.SavePlugins(entry.Runtime, p?.Plugins ?? new()));
    }

    private static async Task PluginAction(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<PluginActionPayload>();
        PluginActionResultPayload result;
        try
        {
            var value = await entry.Runtime.PluginManager.InvokeActionAsync(p?.PluginId ?? "", p?.Action ?? "", p?.ValueJson);
            result = new PluginActionResultPayload { Ok = true, ResultJson = JsonSerializer.Serialize(value) };
        }
        catch (Exception ex)
        {
            result = new PluginActionResultPayload { Ok = false, Error = ex.Message };
        }
        await ctx.Reply(MessageTypes.PluginActionResult, result);
    }

    private static Task UsageGet(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        return ctx.Reply(MessageTypes.UsageResult, SettingsOps.GetUsage(entry.Runtime));
    }

    private static Task AppearanceSave(RequestContext ctx)
    {
        // Appearance auto-saves on change. SaveAppearance persists + publishes AppearanceChanged;
        // the host's event subscriber fans appearance.changed out to this project's windows.
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<AppearanceChangedPayload>();
        if (p != null) SettingsOps.SaveAppearance(entry.Runtime, p.Theme, p.Density);
        return Task.CompletedTask;
    }

    private static Task RegisterAssociation(RequestContext ctx)
        => ctx.Reply(MessageTypes.SystemRegisterAssociationResult, SystemOps.RegisterFileAssociation());
}
