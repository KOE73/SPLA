using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>LLM connection settings and diagnostics: list/save connections, ping/models/test probes,
/// and the load-new/unload-old model swap. Health checks re-broadcast to the project's watchers.</summary>
internal sealed class ConnectionHandlers : IMessageHandler
{
    public IEnumerable<string> HandledTypes =>
    [
        MessageTypes.ConnectionsGet, MessageTypes.ConnectionPing, MessageTypes.ConnectionModels,
        MessageTypes.ConnectionTest, MessageTypes.ConnectionSwapModel, MessageTypes.ConnectionsSave,
    ];

    public Task HandleAsync(RequestContext ctx) => ctx.Env.Type switch
    {
        MessageTypes.ConnectionsGet       => Get(ctx),
        MessageTypes.ConnectionPing        => Ping(ctx),
        MessageTypes.ConnectionModels      => Models(ctx),
        MessageTypes.ConnectionTest        => Test(ctx),
        MessageTypes.ConnectionSwapModel   => Swap(ctx),
        MessageTypes.ConnectionsSave       => Save(ctx),
        _ => Task.CompletedTask
    };

    private static async Task Get(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        await ctx.Reply(MessageTypes.ConnectionsResult, SettingsOps.GetConnections(entry.Runtime));
        // Send cached health immediately so dots render without waiting for the network check.
        await ctx.Send(MessageTypes.ConnectionsHealth,
            ConnectionDiagOps.GetCachedHealth(entry.Runtime.Settings.Connections, entry.Runtime.ConnectionHealth));
        // Re-ping all in background (settings panel just opened) → broadcast to this project's clients.
        PingAllAndBroadcast(ctx, entry.Runtime, projectId);
    }

    private static async Task Ping(RequestContext ctx)
        => await ctx.Reply(MessageTypes.ConnectionPingResult,
            await ConnectionDiagOps.PingAsync(ctx.Payload<ConnectionDiagRequest>()));

    private static async Task Models(RequestContext ctx)
        => await ctx.Reply(MessageTypes.ConnectionModelsResult,
            await ConnectionDiagOps.GetModelsAsync(ctx.Payload<ConnectionDiagRequest>()));

    private static async Task Test(RequestContext ctx)
        => await ctx.Reply(MessageTypes.ConnectionTestResult,
            await ConnectionDiagOps.TestChatAsync(ctx.Payload<ConnectionDiagRequest>()));

    private static async Task Swap(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var req = ctx.Payload<ConnectionSwapModelRequest>();
        if (req == null) return;
        var swapResult = await SwapModelAsync(entry.Runtime, req, ctx.HostStopping);
        await ctx.Reply(MessageTypes.ConnectionSwapModelResult, swapResult);
        if (swapResult.Error == null)
            await ctx.Session.Hub.BroadcastToProjectAsync(projectId, MessageTypes.ConnectionsResult, SettingsOps.GetConnections(entry.Runtime));
    }

    private static async Task Save(RequestContext ctx)
    {
        var (entry, projectId) = ctx.Session.Resolve(ctx.Env);
        var p = ctx.Payload<ConnectionsPayload>();
        var result = SettingsOps.SaveConnections(entry.Runtime, p?.Connections ?? new());
        // Everyone on this project refreshes pickers/editors against the new list.
        await ctx.Session.Hub.BroadcastToProjectAsync(projectId, MessageTypes.ConnectionsResult, result);
        // Re-ping after save — new or changed endpoints need a fresh check.
        PingAllAndBroadcast(ctx, entry.Runtime, projectId);
    }

    private static void PingAllAndBroadcast(RequestContext ctx, AgentRuntime runtime, string projectId)
        => _ = Task.Run(async () =>
        {
            try
            {
                var health = await ConnectionDiagOps.PingAllAsync(runtime.Settings.Connections, runtime.ConnectionHealth);
                await ctx.Session.Hub.BroadcastToProjectAsync(projectId, MessageTypes.ConnectionsHealth, health);
            }
            catch { }
        });

    private static async Task<ConnectionSwapModelResult> SwapModelAsync(
        AgentRuntime runtime, ConnectionSwapModelRequest req, CancellationToken ct)
    {
        var result = new ConnectionSwapModelResult { Id = req.Id };
        try
        {
            var endpoint = req.Endpoint ?? "";
            var apiKey   = req.ApiKey   ?? "lm-studio";

            // Unload every currently loaded model instance before loading the new one.
            var models = await runtime.ModelManagement.GetModelDetailsAsync(endpoint, apiKey, ct);
            foreach (var m in models.Where(m => m.IsLoaded))
                await runtime.ModelManagement.UnloadModelAsync(endpoint, apiKey, m.UnloadId, ct);

            await runtime.ModelManagement.LoadModelAsync(endpoint, apiKey, req.ModelKey, ct);

            // Update the live connection so chats immediately use the new model.
            var conn = runtime.Settings.Connections.FirstOrDefault(c => c.Id == req.Id);
            if (conn != null) conn.Model = req.ModelKey;

            result.Model = req.ModelKey;
        }
        catch (Exception ex)
        {
            result.Error = ex.Message;
        }
        return result;
    }
}
