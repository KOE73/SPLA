using System.Text.Json;

namespace SPLA.MCP.Core.Interfaces;

/// <summary>A narrow plugin-owned interactive panel transport. The host routes opaque input and
/// events; it never knows the plugin's browser, terminal, database, or other domain state.</summary>
public interface ISplaPluginPanelProvider
{
    string PanelType { get; }
    Task<ISplaPluginPanelSession> OpenAsync(
        string panelId,
        IReadOnlyDictionary<string, string?> parameters,
        Func<SplaPluginPanelEvent, ValueTask> publish,
        CancellationToken cancellationToken);
}

public interface ISplaPluginPanelSession : IAsyncDisposable
{
    Task HandleInputAsync(string inputType, JsonElement payload, CancellationToken cancellationToken);
}

public sealed record SplaPluginPanelEvent(string Type, object? Payload = null);
