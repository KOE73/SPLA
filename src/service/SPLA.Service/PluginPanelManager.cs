using System.Collections.Concurrent;
using System.Text.Json;
using SPLA.MCP.Core.Interfaces;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>Connection-owned viewers over plugin-owned sessions. A disconnect disposes every
/// session; the manager routes only opaque events and inputs and contains no plugin-specific logic.</summary>
internal sealed class PluginPanelManager(Func<string, object, Task> send) : IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, ISplaPluginPanelSession> _sessions = new();

    public async Task OpenAsync(AgentRuntime runtime, PluginPanelOpenPayload payload, CancellationToken ct)
    {
        await CloseAsync(payload.PanelId);
        var provider = runtime.PluginManager.GetPanelProvider(payload.PanelType)
            ?? throw new InvalidOperationException($"No enabled plugin provides panel type '{payload.PanelType}'.");

        var session = await provider.OpenAsync(
            payload.PanelId,
            payload.Parameters,
            panelEvent => new ValueTask(send(MessageTypes.PluginPanelEvent, new PluginPanelEventPayload
            {
                PanelId = payload.PanelId,
                EventType = panelEvent.Type,
                Data = panelEvent.Payload
            })),
            ct);
        _sessions[payload.PanelId] = session;
        await send(MessageTypes.PluginPanelOpened, new PluginPanelOpenedPayload { PanelId = payload.PanelId });
    }

    public Task InputAsync(PluginPanelInputPayload payload, CancellationToken ct) =>
        _sessions.TryGetValue(payload.PanelId, out var session)
            ? session.HandleInputAsync(payload.InputType, payload.Data, ct)
            : Task.CompletedTask;

    public async Task CloseAsync(string panelId)
    {
        if (_sessions.TryRemove(panelId, out var session)) await session.DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var panelId in _sessions.Keys) await CloseAsync(panelId);
    }
}
