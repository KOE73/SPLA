namespace SPLA.MCP.Core.Interfaces;

/// <summary>Outcome of a plugin's load-time self-check.</summary>
public enum PluginHealthStatus
{
    /// <summary>The plugin verified its runtime prerequisites; its tools should work.</summary>
    Ok,
    /// <summary>The plugin loaded but a prerequisite is missing or broken; some/all tools may fail.
    /// The plugin stays registered so the rest of the app keeps working — the state is a diagnostic.</summary>
    Degraded
}

/// <summary>A self-check result: a status plus, when degraded, a human-readable reason.</summary>
public sealed record PluginHealth(PluginHealthStatus Status, string? Message = null)
{
    public static readonly PluginHealth Ok = new(PluginHealthStatus.Ok);
    public static PluginHealth Degraded(string message) => new(PluginHealthStatus.Degraded, message);
}

/// <summary>
/// Optional plugin contract for a load-time self-check. After <see cref="ISplaPlugin.Initialize"/>
/// registers a plugin's tools, the host calls <see cref="CheckHealth"/> so the plugin can verify the
/// things a static load cannot prove: a native dependency is present, an external binary is on PATH,
/// a scripting/interop runtime actually executes, a required service answers. This turns silent,
/// only-visible-when-the-model-calls-it breakage into an explicit state the UI can show.
/// <para>
/// Contract: <b>do not throw.</b> Probe what you need and return <see cref="PluginHealth.Degraded"/>
/// with a clear message on failure. The host still guards the call (a thrown exception is treated as
/// degraded), but a plugin that returns a good message gives the user something actionable.
/// Keep it fast — it runs synchronously during startup for every enabled plugin that implements it.
/// </para>
/// </summary>
public interface ISplaPluginSelfCheck
{
    PluginHealth CheckHealth();
}
