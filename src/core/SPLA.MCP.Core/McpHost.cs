using SPLA.Domain.Models;
using SPLA.Domain.Interfaces;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Permissions;
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

    public McpHost(
        IPermissionManager permissionManager,
        SPLA.MCP.Core.Plugins.PluginManager? pluginManager = null,
        ILogger<McpHost>? logger = null)
    {
        _permissionManager = permissionManager;
        _pluginManager = pluginManager;
        _logger = logger;

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
            .Select(GetDefinitionForModel);
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

    public async Task<ToolResult> ExecuteToolAsync(AgentMode mode, string name, string argumentsJson, CancellationToken cancellationToken = default)
    {
        if (_tools.TryGetValue(name, out var tool))
        {
            if (_pluginManager != null && !_pluginManager.IsToolAvailable(tool))
            {
                _logger?.LogWarning("Tool refused: owning plugin is disabled. Tool={ToolName}", name);
                return ToolResult.Refuse(
                    $"Error: tool '{name}' belongs to a plugin that is currently disabled.",
                    "plugin disabled");
            }

            if (ToolSetRefusal(name) is { } refusal)
            {
                _logger?.LogInformation("Tool refused: its set is not raised. Tool={ToolName}", name);
                return ToolResult.Refuse(refusal, "tool set not raised");
            }

            using var activity = SplaTelemetry.StartActivity("mcp.tool.execute");
            activity?.SetTag("spla.tool.name", name);
            activity?.SetTag("spla.agent.mode", mode.ToString());
            var started = Stopwatch.GetTimestamp();
            _logger?.LogInformation("Tool execution started. Tool={ToolName} Mode={Mode}", name, mode);

            var def = tool.GetDefinition();
            var verdict = _permissionManager.CheckPermission(mode, def.Function, argumentsJson);

            if (verdict.Result == PermissionResult.Deny)
            {
                _logger?.LogWarning(
                    "Tool execution denied by policy. Tool={ToolName} Mode={Mode} Reason={Reason}",
                    name, mode, verdict.Reason);
                return ToolResult.Refuse(
                    $"Error: Execution of tool '{name}' was denied by permission policy in {mode} mode ({verdict.Reason}).",
                    verdict.Reason);
            }

            if (verdict.Result == PermissionResult.Ask)
            {
                _logger?.LogInformation(
                    "Tool requires user confirmation — awaiting permission. Tool={ToolName} Mode={Mode} Scope={Scope} Effect={Effect} Risk={Risk}",
                    name, mode, def.Function.Scope, def.Function.Effect, def.Function.Risk);
                var pending = PermissionScope.RequestAsync(def.Function, argumentsJson);
                if (pending != null)
                {
                    var decision = await pending;
                    _logger?.LogInformation(
                        "Permission decision received. Tool={ToolName} Decision={Decision}", name, decision);
                    if (decision == PermissionDecision.Deny)
                    {
                        _logger?.LogWarning("Tool execution denied by user. Tool={ToolName} Mode={Mode}", name, mode);
                        return ToolResult.Refuse(
                            $"Error: Execution of tool '{name}' was denied by the user.",
                            "denied by the user");
                    }
                }
                else
                {
                    _logger?.LogWarning(
                        "Tool requires confirmation but NO permission handler is attached — execution blocked. Tool={ToolName} Mode={Mode}",
                        name, mode);
                    return ToolResult.Refuse(
                        $"Error: Tool '{name}' requires user confirmation, but no permission handler is attached.",
                        "no permission handler attached");
                }
            }

            // Make this host + mode ambiently available so a tool can invoke other tools by name
            // (e.g. a script's ctx.Run) under the same mode, with permissions and progress intact.
            using var hostScope = SPLA.Domain.Tools.ToolHostScope.Begin(this, mode);
            // Open a progress-tree node for this tool call. This is the single place nodes are created,
            // so tools the model calls directly and tools a script invokes via the host both nest
            // correctly — the script's call sits above its parallel children with no extra wiring.
            using var progressNode = SPLA.Domain.Tools.ProgressScope.BeginNode(name);
            try
            {
                var result = await tool.ExecuteAsync(argumentsJson, cancellationToken);
                // A tool that reports failure is a failure, whether it said so by throwing or by
                // returning. Before the result carried an outcome only the throw was visible here,
                // so a returned "error: …" was recorded as a successful call.
                if (result.IsError)
                {
                    progressNode.Fail();
                    activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, result.Reason);
                }
                RecordToolCall(name, started, result);
                return result;
            }
            catch (OperationCanceledException)
            {
                progressNode.Fail();
                _logger?.LogWarning("Tool execution canceled. Tool={ToolName} Mode={Mode}", name, mode);
                throw;
            }
            catch (Exception ex)
            {
                progressNode.Fail();
                activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
                SplaTelemetry.ToolErrors.Add(1);
                _logger?.LogError(ex, "Tool execution failed. Tool={ToolName} Mode={Mode}", name, mode);
                return ToolResult.Fail($"Error executing tool {name}: {ex.Message}", ex.GetType().Name);
            }
        }
        _logger?.LogWarning("Tool not found. Tool={ToolName} Mode={Mode}", name, mode);
        return ToolResult.Fail($"Error: Tool '{name}' not found.", "tool not found");
    }

    private void RecordToolCall(string name, long started, ToolResult result)
    {
        var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        SplaTelemetry.ToolCalls.Add(1);
        SplaTelemetry.ToolDurationMs.Record(elapsedMs);
        if (result.IsError) SplaTelemetry.ToolErrors.Add(1);
        _logger?.LogInformation(
            "Tool execution finished. Tool={ToolName} Outcome={Outcome} DurationMs={DurationMs:F1} ResultLength={ResultLength}",
            name,
            result.Outcome,
            elapsedMs,
            result.TextContent.Length);
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
    private static ToolDefinition GetDefinitionForModel(IMcpTool tool)
    {
        var definition = tool.GetDefinition();
        var details = definition.Function.Details;
        if (string.IsNullOrWhiteSpace(details)) return definition;

        if (!definition.Function.Description.Contains(details.Trim(), StringComparison.Ordinal))
            definition.Function.Description =
                definition.Function.Description.TrimEnd() + Environment.NewLine + Environment.NewLine + details.Trim();

        return definition;
    }

    private bool IsToolAvailableInMode(IMcpTool tool, AgentMode mode)
    {
        var permission = _permissionManager.CheckPermission(mode, tool.GetDefinition().Function, "{}");
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
