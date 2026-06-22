using SPLA.Domain.Models;
using SPLA.Domain.Interfaces;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.Tools;
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

        // agent_info is registered externally (requires SkillManager which McpHost doesn't own)
    }

    public void RegisterTool(IMcpTool tool)
    {
        if (_pluginManager != null && !_pluginManager.IsToolEnabled(tool.Name))
        {
            _logger?.LogInformation("Tool disabled by settings. Tool={ToolName}", tool.Name);
            return; // Tool is disabled via settings
        }
        _tools[tool.Name] = tool;
        _logger?.LogInformation("Tool registered. Tool={ToolName}", tool.Name);
    }

    public IEnumerable<ToolDefinition> GetToolDefinitions()
    {
        return _tools.Values.Select(GetDefinitionForModel);
    }

    public string GetToolHelp(string? name, AgentMode? mode = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "found: false\nreason: missing_name";
        }

        if (_tools.TryGetValue(name, out var tool))
        {
            if (mode.HasValue && !IsToolAvailableInMode(tool, mode.Value))
            {
                return "found: false\nreason: tool_not_available_in_current_mode";
            }

            var definition = tool.GetDefinition().Function;
            var pluginId = GetPluginId(definition.Name);
            var helpText = tool is IToolHelpProvider helpProvider
                ? helpProvider.GetHelpText()
                : null;

            var sb = new StringBuilder();
            sb.AppendLine("found: true");
            sb.AppendLine($"tool: {definition.Name}");
            if (!string.IsNullOrWhiteSpace(pluginId))
            {
                sb.AppendLine($"plugin: {pluginId}");
            }

            sb.AppendLine($"description: {definition.Description}");
            sb.AppendLine("parameters:");
            sb.AppendLine(Indent(JsonSerializer.Serialize(definition.Parameters ?? new { type = "object", properties = new { } }, new JsonSerializerOptions
            {
                WriteIndented = true
            }), "  "));

            if (!string.IsNullOrWhiteSpace(helpText))
            {
                sb.AppendLine("help: |");
                sb.AppendLine(Indent(helpText.Trim(), "  "));
            }
            else
            {
                sb.AppendLine("help: null");
                sb.AppendLine("note: This tool does not provide extended help yet. Use the schema and description above.");
            }

            return sb.ToString();
        }

        var suggestions = _tools.Keys
            .Where(toolName => !mode.HasValue || IsToolAvailableInMode(_tools[toolName], mode.Value))
            .Where(toolName => toolName.Contains(name, StringComparison.OrdinalIgnoreCase))
            .OrderBy(toolName => toolName, StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .ToArray();

        if (suggestions.Length == 0)
        {
            return "found: false\nreason: tool_not_registered_or_plugin_disabled";
        }

        var suggestionBuilder = new StringBuilder();
        suggestionBuilder.AppendLine("found: false");
        suggestionBuilder.AppendLine("reason: exact_tool_not_found");
        suggestionBuilder.AppendLine("suggestions:");
        foreach (var suggestion in suggestions)
        {
            suggestionBuilder.AppendLine($"  - {suggestion}");
        }

        return suggestionBuilder.ToString();
    }

    public async Task<string> ExecuteToolAsync(AgentMode mode, string name, string argumentsJson, CancellationToken cancellationToken = default)
    {
        if (_tools.TryGetValue(name, out var tool))
        {
            using var activity = SplaTelemetry.StartActivity("mcp.tool.execute");
            activity?.SetTag("spla.tool.name", name);
            activity?.SetTag("spla.agent.mode", mode.ToString());
            var started = Stopwatch.GetTimestamp();
            _logger?.LogInformation("Tool execution started. Tool={ToolName} Mode={Mode}", name, mode);

            var def = tool.GetDefinition();
            var permission = _permissionManager.CheckPermission(mode, def.Function, argumentsJson);

            if (permission == PermissionResult.Deny)
            {
                _logger?.LogWarning("Tool execution denied by policy. Tool={ToolName} Mode={Mode}", name, mode);
                return $"Error: Execution of tool '{name}' was denied by permission policy in {mode} mode.";
            }

            if (permission == PermissionResult.Ask)
            {
                var pending = PermissionScope.RequestAsync(def.Function, argumentsJson);
                if (pending != null)
                {
                    var decision = await pending;
                    if (decision == PermissionDecision.Deny)
                    {
                        _logger?.LogWarning("Tool execution denied by user. Tool={ToolName} Mode={Mode}", name, mode);
                        return $"Error: Execution of tool '{name}' was denied by the user.";
                    }
                }
                else
                {
                    return $"Error: Tool '{name}' requires user confirmation, but no permission handler is attached.";
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
                if (tool is AgentInfoTool agentInfoTool)
                {
                    var helpResult = await agentInfoTool.ExecuteAsync(mode, argumentsJson, cancellationToken);
                    RecordToolSuccess(name, started, helpResult.Length);
                    return helpResult;
                }

                var result = await tool.ExecuteAsync(argumentsJson, cancellationToken);
                RecordToolSuccess(name, started, result.Length);
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
                SplaTelemetry.ToolErrors.Add(1);
                _logger?.LogError(ex, "Tool execution failed. Tool={ToolName} Mode={Mode}", name, mode);
                return $"Error executing tool {name}: {ex.Message}";
            }
        }
        _logger?.LogWarning("Tool not found. Tool={ToolName} Mode={Mode}", name, mode);
        return $"Error: Tool '{name}' not found.";
    }

    private void RecordToolSuccess(string name, long started, int resultLength)
    {
        var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        SplaTelemetry.ToolCalls.Add(1);
        SplaTelemetry.ToolDurationMs.Record(elapsedMs);
        _logger?.LogInformation(
            "Tool execution finished. Tool={ToolName} DurationMs={DurationMs:F1} ResultLength={ResultLength}",
            name,
            elapsedMs,
            resultLength);
    }

    private static string? GetPluginId(string toolName)
    {
        var dot = toolName.IndexOf('.', StringComparison.Ordinal);
        return dot > 0 ? toolName[..dot] : null;
    }

    private static ToolDefinition GetDefinitionForModel(IMcpTool tool)
    {
        var definition = tool.GetDefinition();
        if (tool is not IToolHelpProvider helpProvider || string.IsNullOrWhiteSpace(helpProvider.GetHelpText()))
        {
            return definition;
        }

        if (!definition.Function.Description.StartsWith("[H]", StringComparison.Ordinal))
        {
            definition.Function.Description = $"[H] {definition.Function.Description}";
        }

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
