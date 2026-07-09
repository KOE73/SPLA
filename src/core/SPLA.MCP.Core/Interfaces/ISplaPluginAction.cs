namespace SPLA.MCP.Core.Interfaces;

/// <summary>
/// Implemented by a plugin that wants to handle ad-hoc actions invoked from its web settings UI
/// (e.g. "Test Connection"). Kept separate from <see cref="ISplaPlugin"/> tools: actions are
/// triggered by a human from the settings editor, not by the agent.
/// </summary>
public interface ISplaPluginAction
{
    /// <summary>
    /// Runs <paramref name="action"/> with the given JSON-serialized value and returns a JSON-
    /// serializable result. Unknown actions should throw; the host turns that into an error reply.
    /// </summary>
    Task<object?> InvokeActionAsync(string action, string? valueJson, CancellationToken ct = default);
}
