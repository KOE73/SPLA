using SPLA.Domain.Settings;
using SPLA.MCP.Core.Agent;
using SPLA.MCP.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.MCP.Core.ToolSets;

/// <summary>
/// The process-wide catalogue of tool sets and their levels: what exists, who shipped it, and how
/// far the user allows it to reach the model. Composition (which set is raised right now) is a
/// property of a chat and lives elsewhere — this type answers only "allowed how far", never
/// "raised now".
///
/// <para>Levels are resolved on every query rather than captured, so toggling a plugin or editing
/// the <c>toolsets:</c> section takes effect without a reload — the same live-gating rule the
/// plugin flag already follows.</para>
/// </summary>
public sealed class ToolSetRegistry
{
    private readonly ResolvedSettings _settings;
    private readonly PluginManager? _plugins;
    private readonly List<ToolSetDescriptor> _sets = [];
    private readonly Dictionary<string, string> _setOfTool = new(StringComparer.OrdinalIgnoreCase);

    public ToolSetRegistry(
        ResolvedSettings settings,
        PluginManager? plugins = null,
        IEnumerable<IAgentFeature>? features = null)
    {
        _settings = settings;
        _plugins = plugins;

        foreach (var feature in features ?? [])
            Add(new ToolSetDescriptor
            {
                Id = feature.Id,
                Origin = ToolSetOrigin.Core,
                OriginId = "core",
                ToolNames = feature.Tools.Select(t => t.Name).ToList()
            });

        if (plugins == null) return;

        var toolsByPlugin = plugins.GetToolOwners()
            .GroupBy(pair => pair.Value, pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var descriptor in plugins.GetPlugins())
            Add(new ToolSetDescriptor
            {
                Id = descriptor.Meta.Id,
                Origin = ToolSetOrigin.Plugin,
                OriginId = descriptor.Meta.Id,
                Description = descriptor.Meta.Description,
                Summon = descriptor.Meta.Summon,
                ToolNames = toolsByPlugin.GetValueOrDefault(descriptor.Meta.Id) ?? []
            });
    }

    private void Add(ToolSetDescriptor set)
    {
        _sets.Add(set);
        foreach (var toolName in set.ToolNames)
            _setOfTool[toolName] = set.Id;
    }

    /// <summary>Every known set, core first, then plugins in discovery order.</summary>
    public IReadOnlyList<ToolSetDescriptor> All => _sets;

    public ToolSetDescriptor? Find(string setId) =>
        _sets.FirstOrDefault(s => string.Equals(s.Id, setId, StringComparison.OrdinalIgnoreCase));

    /// <summary>Which set a tool belongs to, or null for a tool no set claims (a built-in tool that
    /// belongs to no feature). An unclaimed tool is not gated by this mechanism at all.</summary>
    public string? SetOfTool(string toolName) => _setOfTool.GetValueOrDefault(toolName);

    /// <summary>
    /// How far this set may reach the model. An explicit <c>toolsets:</c> entry wins; without one the
    /// level is <b>derived</b> from the supplier's existing on/off flag, which is what keeps every
    /// project that predates this mechanism behaving exactly as before — an enabled plugin is a fully
    /// disclosed set, a disabled one does not exist.
    /// </summary>
    public ToolSetLevel LevelOf(string setId)
    {
        if (_settings.ToolSets.TryGetValue(setId, out var configured) && TryParseLevel(configured, out var level))
            return level;

        var set = Find(setId);
        if (set is { Origin: ToolSetOrigin.Plugin } && _plugins != null)
            return _plugins.IsPluginEnabled(set.OriginId) ? ToolSetLevel.Enabled : ToolSetLevel.Disabled;

        // Core sets are already gated by agent.capabilities before they get here: a feature that is
        // off has no descriptor at all, so a descriptor means the capability is on.
        return ToolSetLevel.Enabled;
    }

    /// <summary>Level of the set owning this tool. Tools no set claims are <see cref="ToolSetLevel.Enabled"/> —
    /// the mechanism gates sets, and an unclaimed tool is nobody's.</summary>
    public ToolSetLevel LevelOfTool(string toolName) =>
        SetOfTool(toolName) is { } setId ? LevelOf(setId) : ToolSetLevel.Enabled;

    /// <summary>Accepts the written form (<c>disabled</c>, <c>skill_demand</c>, <c>agent_demand</c>,
    /// <c>enabled</c>) plus the spellings a person is likely to type by hand. Deliberately does not
    /// accept <c>on</c>/<c>off</c>: YAML 1.1 reads those as booleans, and a level that changes meaning
    /// depending on whether it was quoted is a trap.</summary>
    public static bool TryParseLevel(string? text, out ToolSetLevel level)
    {
        level = ToolSetLevel.Disabled;
        if (string.IsNullOrWhiteSpace(text)) return false;

        switch (text.Trim().Replace('-', '_').ToLowerInvariant())
        {
            case "disabled":
            case "false":
                level = ToolSetLevel.Disabled;
                return true;
            case "skill_demand":
            case "skill":
                level = ToolSetLevel.SkillDemand;
                return true;
            case "agent_demand":
            case "agent":
                level = ToolSetLevel.AgentDemand;
                return true;
            case "enabled":
            case "true":
                level = ToolSetLevel.Enabled;
                return true;
            default:
                return false;
        }
    }

    /// <summary>The written form, for settings and for the UI.</summary>
    public static string Format(ToolSetLevel level) => level switch
    {
        ToolSetLevel.Disabled => "disabled",
        ToolSetLevel.SkillDemand => "skill_demand",
        ToolSetLevel.AgentDemand => "agent_demand",
        _ => "enabled"
    };
}
