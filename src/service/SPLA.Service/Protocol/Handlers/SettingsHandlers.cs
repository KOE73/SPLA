using SPLA.Runtime;
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
        MessageTypes.McpGet, MessageTypes.McpSave,
        MessageTypes.McpServersGet, MessageTypes.McpServersSave, MessageTypes.McpServersReconnect,
        MessageTypes.PluginsGet, MessageTypes.PluginsSave, MessageTypes.PluginAction,
        MessageTypes.SkillsGet, MessageTypes.SkillsSave,
        MessageTypes.SkillSourcesGet, MessageTypes.SkillSourcesSave, MessageTypes.SkillSourceTrust,
        MessageTypes.FeaturesGet, MessageTypes.FeaturesSave,
        MessageTypes.UsageGet, MessageTypes.AppearanceSave, MessageTypes.SystemRegisterAssociation,
    ];

    public Task HandleAsync(RequestContext ctx) => ctx.Env.Type switch
    {
        MessageTypes.AgentGet                  => AgentGet(ctx),
        MessageTypes.AgentSave                 => AgentSave(ctx),
        MessageTypes.McpGet                    => McpGet(ctx),
        MessageTypes.McpSave                   => McpSave(ctx),
        MessageTypes.McpServersGet              => McpServersGet(ctx),
        MessageTypes.McpServersSave             => McpServersSave(ctx),
        MessageTypes.McpServersReconnect        => McpServersReconnect(ctx),
        MessageTypes.PluginsGet                => PluginsGet(ctx),
        MessageTypes.PluginsSave               => PluginsSave(ctx),
        MessageTypes.PluginAction              => PluginAction(ctx),
        MessageTypes.SkillsGet                 => SkillsGet(ctx),
        MessageTypes.SkillsSave                => SkillsSave(ctx),
        MessageTypes.SkillSourcesGet           => SkillSourcesGet(ctx),
        MessageTypes.SkillSourcesSave          => SkillSourcesSave(ctx),
        MessageTypes.SkillSourceTrust          => SkillSourceTrust(ctx),
        MessageTypes.FeaturesGet               => FeaturesGet(ctx),
        MessageTypes.FeaturesSave              => FeaturesSave(ctx),
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

    private static Task McpGet(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        return ctx.Reply(MessageTypes.McpResult, SettingsOps.GetMcp(entry.Runtime));
    }

    private static async Task McpSave(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<McpSettingsPayload>();
        if (p != null)
            await ctx.Session.Hub.BroadcastToProjectAsync(projectId, MessageTypes.McpResult, SettingsOps.SaveMcp(entry.Runtime, p));
    }

    private static Task McpServersGet(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        return ctx.Reply(MessageTypes.McpServersResult, SettingsOps.GetMcpServers(entry.Runtime));
    }

    private static async Task McpServersSave(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<McpServersPayload>();
        if (p != null)
            await ctx.Session.Hub.BroadcastToProjectAsync(projectId, MessageTypes.McpServersResult, SettingsOps.SaveMcpServers(entry.Runtime, p));
    }

    // Unicast, like PluginAction: this answers the one click that asked for it. A state change from a
    // successful retry already reaches every window on its own — McpClientManager publishes
    // McpServersChanged on every connect/disconnect, which the host's domain-event subscriber (see
    // agents/protocol.md, "Domain events") broadcasts as this same mcp.servers.result to the whole
    // project. Broadcasting here too would just be the same frame sent twice.
    private static async Task McpServersReconnect(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<McpServerActionPayload>();
        var result = await SettingsOps.ReconnectMcpServer(entry.Runtime, p?.ServerId ?? "");
        await ctx.Reply(MessageTypes.McpServersResult, result);
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

    private static Task SkillsGet(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        return ctx.Reply(MessageTypes.SkillsResult, SettingsOps.GetSkills(entry.Runtime));
    }

    private static async Task SkillsSave(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<SkillsPayload>();
        await ctx.Session.Hub.BroadcastToProjectAsync(projectId, MessageTypes.SkillsResult,
            SettingsOps.SaveSkills(entry.Runtime, p?.Skills ?? new()));
    }

    private static Task SkillSourcesGet(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        return ctx.Reply(MessageTypes.SkillSourcesResult, SettingsOps.GetSkillSources(entry.Runtime));
    }

    private static async Task SkillSourcesSave(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<SkillSourcesPayload>();
        // Broadcast the SKILLS, not the source list: the point of adding a branch is the books that
        // appear, and every open panel has to see them without asking again.
        await ctx.Session.Hub.BroadcastToProjectAsync(projectId, MessageTypes.SkillsResult,
            SettingsOps.SaveSkillSources(entry.Runtime, p?.Sources ?? new()));
    }

    private static async Task SkillSourceTrust(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<SkillSourceTrustPayload>();
        if (p == null || string.IsNullOrWhiteSpace(p.SourceId)) return;

        await ctx.Session.Hub.BroadcastToProjectAsync(projectId, MessageTypes.SkillsResult,
            SettingsOps.SetSkillSourceTrust(entry.Runtime, p.SourceId, p.Trusted));
    }

    private static Task FeaturesGet(RequestContext ctx)
    {
        var (entry, _) = ctx.Session.Resolve(ctx.Env);
        return ctx.Reply(MessageTypes.FeaturesResult, SettingsOps.GetFeatures(entry.Runtime));
    }

    private static async Task FeaturesSave(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<FeaturesPayload>();
        await ctx.Session.Hub.BroadcastToProjectAsync(projectId, MessageTypes.FeaturesResult,
            SettingsOps.SaveFeatures(entry.Runtime, p?.Features ?? new()));
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
