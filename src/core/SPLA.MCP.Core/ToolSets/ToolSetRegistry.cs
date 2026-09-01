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

    // _sets and _setOfTool used to be write-once (constructor only). An MCP server now adds and
    // removes its set after construction, from its own connection thread, while call threads read
    // All/Find/SetOfTool/LevelOf concurrently. A single lock around every mutation and every read is
    // the simplest correct answer: the two collections must change together (RemoveDynamic drops
    // entries from both), so a lock-free scheme would need to reason about the moment between them
    // anyway, and these operations are short and infrequent enough that lock contention is not a
    // real cost. Copy-on-read (`All`) keeps an enumerator a caller holds onto from ever observing a
    // torn state.
    private readonly object _gate = new();

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

    /// <summary>
    /// Adds a set after construction — the mechanism a connected MCP server needs
    /// (PLAN_20260826_service_mcp-client, step 1): a server's tool list is unknown until the
    /// handshake completes, which is well after this registry was built. <see cref="Add"/> stays
    /// private and is still what the constructor calls for the sets known up front; this is the same
    /// operation opened up for later callers.
    /// </summary>
    public void AddDynamic(ToolSetDescriptor set)
    {
        lock (_gate)
            Add(set);
    }

    /// <summary>
    /// Removes a dynamically-added set and every tool-name mapping it owned. Returns whether a set by
    /// that id was found. Both collections are dropped under the same lock — leaving <c>_setOfTool</c>
    /// pointing at a set id <see cref="Find"/> can no longer resolve would turn "the server
    /// disconnected" into a dangling reference instead of "this tool is no longer gated by anything".
    /// </summary>
    public bool RemoveDynamic(string setId)
    {
        lock (_gate)
        {
            var index = _sets.FindIndex(s => string.Equals(s.Id, setId, StringComparison.OrdinalIgnoreCase));
            if (index < 0) return false;

            var set = _sets[index];
            _sets.RemoveAt(index);
            foreach (var toolName in set.ToolNames)
                _setOfTool.Remove(toolName);
            return true;
        }
    }

    /// <summary>Every known set, core first, then plugins and connected MCP servers in discovery/connect
    /// order. Copied under the lock so an enumerator a caller holds onto never observes a set removed
    /// out from under it mid-iteration.</summary>
    public IReadOnlyList<ToolSetDescriptor> All
    {
        get { lock (_gate) return _sets.ToList(); }
    }

    public ToolSetDescriptor? Find(string setId)
    {
        lock (_gate)
            return _sets.FirstOrDefault(s => string.Equals(s.Id, setId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Which set a tool belongs to, or null for a tool no set claims (a built-in tool that
    /// belongs to no feature). An unclaimed tool is not gated by this mechanism at all.</summary>
    public string? SetOfTool(string toolName)
    {
        lock (_gate)
            return _setOfTool.GetValueOrDefault(toolName);
    }

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
        //
        // An Origin: Mcp set falls through to the same Enabled default, and deliberately has no
        // "is this server enabled" delegate the way the plugin branch does. A plugin can be disabled
        // while its descriptor still exists (its assembly stays loaded, only exposure is gated) — an
        // MCP server cannot: disabling one removes its ToolSetDescriptor and unregisters its tools
        // outright (ADR_20260826_service_mcp-client), there is no "descriptor for an off server" state
        // to ask about. The descriptor existing already means the server is connected and on.
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
