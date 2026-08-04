using System.Collections.Generic;
using System.Linq;

namespace SPLA.MCP.Core.Skills;

/// <summary>
/// Answers "does this agent currently have that capability?" for requirement resolution. Kept as an
/// interface so <see cref="SkillLibrary"/> does not depend on the tool host or the feature catalog —
/// it only needs the answers, and tests can supply them directly.
/// </summary>
public interface ISkillCapabilityProbe
{
    bool HasTool(string toolName);
    bool HasFeature(string featureId);

    /// <summary>Plugin that owns a tool, when it came from one. Used only to phrase the reason a
    /// skill is unavailable ("needs port_scan — from plugin 'network'").</summary>
    string? PluginOfTool(string toolName);
}

/// <summary>Ready-made probes for hosts that do no gating and for tests.</summary>
public sealed class SkillCapabilityProbe : ISkillCapabilityProbe
{
    private readonly HashSet<string>? _tools;
    private readonly HashSet<string>? _features;
    private readonly IReadOnlyDictionary<string, string>? _toolOwners;

    /// <summary>Treats every requirement as satisfied. For hosts that have no tool host to ask —
    /// NOT a default: a caller that can gate should say so explicitly.</summary>
    public static SkillCapabilityProbe AllAvailable { get; } = new(null, null, null);

    /// <param name="tools">Tool names present, or null for "all present".</param>
    /// <param name="features">Feature ids enabled, or null for "all enabled".</param>
    /// <param name="toolOwners">Tool name → owning plugin id, for reason text.</param>
    public SkillCapabilityProbe(
        IEnumerable<string>? tools,
        IEnumerable<string>? features,
        IReadOnlyDictionary<string, string>? toolOwners = null)
    {
        _tools = tools is null ? null : new HashSet<string>(tools, System.StringComparer.OrdinalIgnoreCase);
        _features = features is null ? null : new HashSet<string>(features, System.StringComparer.Ordinal);
        _toolOwners = toolOwners;
    }

    public bool HasTool(string toolName) => _tools is null || _tools.Contains(toolName);
    public bool HasFeature(string featureId) => _features is null || _features.Contains(featureId);

    public string? PluginOfTool(string toolName)
    {
        if (_toolOwners is null) return null;
        return _toolOwners.TryGetValue(toolName, out var owner) ? owner
            : _toolOwners.FirstOrDefault(kvp => kvp.Key.Equals(toolName, System.StringComparison.OrdinalIgnoreCase)).Value;
    }
}
