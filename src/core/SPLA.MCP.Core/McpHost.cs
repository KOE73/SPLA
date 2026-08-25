using SPLA.Domain.Models;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.Pipeline;
using SPLA.MCP.Core.Pipeline.Stages;
using SPLA.MCP.Core.Tools;
using SPLA.MCP.Core.ToolSets;
using SPLA.Observability;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core;

public class McpHost : IToolHost
{
    private readonly Dictionary<string, IMcpTool> _tools = new(StringComparer.OrdinalIgnoreCase);
    private readonly IPermissionManager _permissionManager;
    private readonly SPLA.MCP.Core.Plugins.PluginManager? _pluginManager;
    private readonly ILogger<McpHost>? _logger;

    /// <summary>
    /// The chain of concerns wrapped around a tool call, baked once here and re-entered as-is by
    /// nested calls. See <see cref="ToolPipelineStage"/> for what the order guarantees.
    /// </summary>
    private readonly ToolCallDelegate _pipeline;

    /// <summary>What the chain above was folded from, kept so the composition can be inspected — by a
    /// test that guards the order, and by a debug view of which links are standing.</summary>
    public ToolPipelineBlueprint Pipeline { get; }

    /// <summary>What has moved between perimeters in this process, counted while nothing is being
    /// refused. Read by the debug view; the source of the defaults enforcement will eventually be
    /// chosen from.</summary>
    public SPLA.MCP.Core.Security.EdgeLedger Edges { get; } = new();

    public McpHost(
        IPermissionManager permissionManager,
        SPLA.MCP.Core.Plugins.PluginManager? pluginManager = null,
        ILogger<McpHost>? logger = null,
        Func<string?, SPLA.Domain.Security.Zone>? zoneOfPath = null,
        Func<SPLA.Domain.Security.Zone, SPLA.Domain.Security.DataOrigin?>? originOfZone = null)
    {
        _permissionManager = permissionManager;
        _pluginManager = pluginManager;
        _logger = logger;

        Pipeline = new ToolPipelineBlueprint()
            .Use(new ToolResolutionStage(name => _tools.TryGetValue(name, out var t) ? t : null, logger))
            .Use(new PluginAvailabilityStage(pluginManager, logger))
            .Use(new ToolSetDisclosureStage(ToolSetRefusal, logger))
            .Use(new TelemetryStage(logger))
            .Use(new PermissionStage(permissionManager, logger))
            // Beside the permission check, not inside it: both answer "may this happen" from
            // different ends, and when the verdict eventually moves onto the edge it moves into a
            // neighbour rather than into a stranger.
            .Use(new ZoneShadowStage(
                new SPLA.MCP.Core.Security.EdgeClassifier(
                    zoneOfPath ?? (_ => SPLA.Domain.Security.Zone.Unknown)),
                Edges, logger, originOfZone))
            .Use(new AmbientHostStage(this))
            .Use(new BackgroundStage(logger))
            .Use(new ProgressNodeStage())
            .Use(new FaultStage(logger))
            .Use(new AccountingStage(logger));

        // The terminal step: the tool itself. Everything above is the pipeline's business;
        // this is the only line that is the tool's.
        _pipeline = Pipeline.Build((call, ct) => call.Tool!.ExecuteAsync(call.ArgumentsJson, ct));

        if (_pluginManager != null)
        {
            foreach (var tool in _pluginManager.GetDynamicTools())
            {
                RegisterTool(tool);
            }
        }

        // agent_info is registered externally (requires SkillLibrary which McpHost doesn't own)
    }

    public void RegisterTool(IMcpTool tool)
    {
        if (_pluginManager != null && !_pluginManager.IsToolEnabled(tool.Name))
        {
            _logger?.LogInformation("Tool disabled by settings. Tool={ToolName}", tool.Name);
            return; // Tool is disabled via settings
        }

        // A plugin DLL must not self-declare a Scope that bypasses gating. ToolScope.Agent is
        // always-allowed in every mode (including Chat) and ToolScope.Skill drives skill/spawn
        // lifecycle — both are reserved for built-in tools the core ships. Plugin-originated tools
        // (those loaded via PluginManager) are refused if they claim either scope, closing the worst
        // self-declaration vector (arch-review debt #3, step a).
        if (IsPluginTool(tool))
        {
            var scope = tool.GetDefinition().Function.Scope;
            if (scope is ToolScope.Agent or ToolScope.Skill)
            {
                _logger?.LogWarning(
                    "Plugin tool rejected: reserved scope. Tool={ToolName} Scope={Scope}", tool.Name, scope);
                return;
            }
        }

        _tools[tool.Name] = tool;
        _logger?.LogInformation("Tool registered. Tool={ToolName}", tool.Name);
    }

    /// <summary>True when the tool was loaded from a plugin (as opposed to a built-in core/agent
    /// tool). Plugin tools are exactly those the <see cref="Plugins.PluginManager"/> discovered.</summary>
    private bool IsPluginTool(IMcpTool tool)
        => _pluginManager != null && _pluginManager.GetDynamicTools().Contains(tool);

    /// <summary>
    /// The tool sets this host knows and how far each may reach the model. Assigned after
    /// construction because a set knows its tools only once they are registered, and this host is
    /// what registers them. Null (tests, hosts that predate tool sets) means no set gating at all.
    /// </summary>
    public ToolSetRegistry? ToolSets { get; set; }

    public IEnumerable<ToolDefinition> GetToolDefinitions()
    {
        // Live plugin gating: a disabled plugin's tools drop out of the offered list immediately
        // (no restart) — the assemblies stay loaded, only exposure is gated.
        return _tools.Values
            .Where(t => _pluginManager == null || _pluginManager.IsToolAvailable(t))
            .Where(t => IsDisclosed(t.Name))
            .Select(t => GetDefinitionForModel(t, exposeBackgroundFlag: true));
    }

    /// <summary>
    /// Every tool the project PERMITS, whether or not it is disclosed right now — i.e. everything
    /// except sets levelled off entirely.
    ///
    /// <para>This is what skill requirements resolve against, and it must not be the disclosed list:
    /// a skill's whole job at the "on skill demand" level is to raise the set it needs, so judging it
    /// by what is raised before it runs would mark exactly those skills unavailable.</para>
    /// </summary>
    public IEnumerable<string> GetPermittedToolNames()
    {
        return _tools.Values
            .Where(t => _pluginManager == null || _pluginManager.IsToolAvailable(t))
            .Where(t => ToolSets == null || ToolSets.LevelOfTool(t.Name) != ToolSetLevel.Disabled)
            .Select(t => t.Name);
    }

    /// <summary>
    /// What to serve a caller that is not this chat's own head — a second head over a wire, a queued
    /// job, anything with a conversation of its own or none at all.
    ///
    /// <para>Built on <see cref="GetPermittedToolNames"/> rather than on the disclosed list, and for
    /// the same reason: disclosure is about how much a set costs the model's context in a chat, which
    /// is nobody else's question. A foreign caller is offered what the project permits, less what is
    /// bound to a conversation it does not have.</para>
    /// </summary>
    public IEnumerable<ToolDefinition> GetToolDefinitionsFor(ToolExposure exposure)
    {
        var permitted = GetPermittedToolNames().ToHashSet(StringComparer.OrdinalIgnoreCase);

        // The background flag is never raised here: a foreign head has no chat to deliver a detached
        // result to (plan pitfall 11, "фон наружу через MCP"). BackgroundStage would run the call
        // synchronously regardless — that guard lives in the pipeline, not the schema — but there is
        // no reason to advertise a capability that degrades on first use.
        return _tools.Values
            .Where(t => permitted.Contains(t.Name))
            .Where(t => exposure.Allows(t.GetDefinition().Function))
            .Select(t => GetDefinitionForModel(t, exposeBackgroundFlag: false));
    }

    /// <summary>True when the model may see this tool right now: its set is fully enabled, or it is
    /// raised in the chat currently running. A tool no set claims is always disclosed.</summary>
    private bool IsDisclosed(string toolName)
    {
        if (ToolSets == null) return true;

        var setId = ToolSets.SetOfTool(toolName);
        if (setId == null) return true;

        return ToolSets.LevelOf(setId) switch
        {
            ToolSetLevel.Enabled => true,
            ToolSetLevel.Disabled => false,
            _ => SPLA.Domain.Agent.AgentSessionScope.Current?.ToolSets.IsActive(setId) == true
        };
    }

    /// <summary>
    /// Why this call cannot run right now, or null when the tool's set permits it.
    ///
    /// <para>The wording is deliberate. A set the user levelled off does not exist — saying anything
    /// else would leak what the project holds. A set that is merely not raised does exist, and saying
    /// so is the useful answer: a dead end costs more than the disclosure, because a model told "no
    /// such tool" about a tool it can see in the history just tries again (IDEA_20260802_core §6.8).</para>
    /// </summary>
    private string? ToolSetRefusal(string toolName)
    {
        if (ToolSets == null) return null;
        if (ToolSets.SetOfTool(toolName) is not { } setId) return null;

        return ToolSets.LevelOf(setId) switch
        {
            ToolSetLevel.Enabled => null,
            ToolSetLevel.Disabled => $"Error: Tool '{toolName}' not found.",
            _ when SPLA.Domain.Agent.AgentSessionScope.Current?.ToolSets.IsActive(setId) == true => null,
            ToolSetLevel.AgentDemand =>
                $"Error: tool '{toolName}' belongs to tool set '{setId}', which is not active in this chat. "
                + $"Call toolset_activate with setId '{setId}' first.",
            _ =>
                $"Error: tool '{toolName}' belongs to tool set '{setId}', which only a skill or the user "
                + "can activate. Ask the user, or run a skill that requires it."
        };
    }

    public async Task<ToolResult> ExecuteToolAsync(
        AgentMode mode,
        string name,
        string argumentsJson,
        CancellationToken cancellationToken = default,
        ToolCallContext? context = null)
    {
        // The call states whose it is, or says nothing and means "the same as the surrounding flow".
        // Entering is what makes the context the source: from here down, the ambient scopes tools
        // read are the ones this context named. When it came from ambient state in the first place,
        // entering re-establishes the values already in effect and changes nothing.
        var effectiveContext = context ?? ToolCallContext.FromAmbient();
        using var callScope = effectiveContext.Enter();

        // Nothing is assembled per call — only the invocation, which is what the links write their
        // findings on. That is what makes a nested ctx.Run safe: it walks the same chain with its own
        // invocation and cannot disturb the call waiting above it.
        return await _pipeline(
            new ToolCallInvocation(mode, name, argumentsJson, effectiveContext.Source), cancellationToken);
    }

    private static string? GetPluginId(string toolName)
    {
        var dot = toolName.IndexOf('.', StringComparison.Ordinal);
        return dot > 0 ? toolName[..dot] : null;
    }

    /// <summary>
    /// The definition as the model sees it: description plus the tool's own details, folded into one
    /// text. A tool is disclosed with everything it has to say about itself or not at all — the model
    /// never has to decide whether to go and read more, which is the decision the old help tool and
    /// its [H] marker cost on every call.
    /// </summary>
    private static ToolDefinition GetDefinitionForModel(IMcpTool tool, bool exposeBackgroundFlag)
    {
        var definition = tool.GetDefinition();
        var details = definition.Function.Details;
        if (!string.IsNullOrWhiteSpace(details) &&
            !definition.Function.Description.Contains(details.Trim(), StringComparison.Ordinal))
            definition.Function.Description =
                definition.Function.Description.TrimEnd() + Environment.NewLine + Environment.NewLine + details.Trim();

        if (exposeBackgroundFlag && definition.Function.SupportsBackground)
            definition.Function.Parameters =
                WithBackgroundParameter(definition.Function.Parameters, definition.Function.StrictSchema);

        return definition;
    }

    /// <summary>
    /// Adds the optional <c>background</c> boolean to a tool's parameter schema — only reached for a
    /// tool that declared <see cref="ToolFunctionDefinition.SupportsBackground"/>, so every other
    /// tool's schema, and every request to a model, costs nothing for this (ADR §2: "Флаг во всех
    /// схемах — это токены в каждом запросе").
    /// <para>
    /// Nullable rather than plain <c>boolean</c>, and added to <c>required</c> when the tool declares
    /// <see cref="ToolFunctionDefinition.StrictSchema"/>: OpenAI strict mode requires every property
    /// to be listed in <c>required</c>, so an optional field is expressed as present-but-nullable —
    /// the same shape <c>cwd</c>/<c>code_page</c> already use in <c>RunCommandTool</c>. Getting this
    /// wrong is plan pitfall 13; it is handled once, here, rather than by every tool that opts in.
    /// </para>
    /// </summary>
    private static object WithBackgroundParameter(object? parameters, bool strictSchema)
    {
        var node = parameters is null
            ? new System.Text.Json.Nodes.JsonObject { ["type"] = "object", ["properties"] = new System.Text.Json.Nodes.JsonObject() }
            : System.Text.Json.Nodes.JsonNode.Parse(System.Text.Json.JsonSerializer.Serialize(parameters))!;

        var properties = node["properties"] as System.Text.Json.Nodes.JsonObject
            ?? (System.Text.Json.Nodes.JsonObject)(node["properties"] = new System.Text.Json.Nodes.JsonObject());

        properties["background"] = new System.Text.Json.Nodes.JsonObject
        {
            ["type"] = new System.Text.Json.Nodes.JsonArray("boolean", "null"),
            ["description"] = "Run this call detached from the turn: get a task id back immediately, " +
                "the result arrives as a message once it finishes. Null/omitted = run normally and wait."
        };

        if (strictSchema)
        {
            var required = node["required"] as System.Text.Json.Nodes.JsonArray
                ?? (System.Text.Json.Nodes.JsonArray)(node["required"] = new System.Text.Json.Nodes.JsonArray());
            if (!required.Any(r => r?.GetValue<string>() == "background"))
                required.Add("background");
        }

        return node;
    }

    /// <summary>
    /// Whether to show the model this tool at all. Asks the ceiling, not the call: disclosure happens
    /// before there are any arguments, and a verdict computed from a stand-in <c>"{}"</c> would let a
    /// domain policy — which has no statement to judge — delete the tool from the list entirely.
    /// </summary>
    private bool IsToolAvailableInMode(IMcpTool tool, AgentMode mode)
    {
        var permission = _permissionManager.CheckToolCeiling(mode, tool.GetDefinition().Function);
        return permission != PermissionResult.Deny;
    }

    private static string Indent(string text, string prefix)
    {
        return string.Join(Environment.NewLine, text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n')
            .Select(line => prefix + line));
    }
}
