using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Plugins;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Universal capability lookup for the agent — resolves tools, skills, and plugins by id.
/// Replaces the former tool.help + skill.load pair.
/// </summary>
public sealed class AgentInfoTool : IMcpTool, IToolHelpProvider
{
    private readonly McpHost _host;
    private readonly SkillManager? _skillManager;

    public AgentInfoTool(McpHost host, SkillManager? skillManager = null)
    {
        _host = host;
        _skillManager = skillManager;
    }

    public string Name => "agent_info";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Universal capability lookup. Returns tool schema+help, skill instructions, or full index. id = tool name OR skill id. Omit id for full index.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    id = new
                    {
                        type = new[] { "string", "null" },
                        description = "Tool name or skill id. Partial names return suggestions. Null = full index."
                    }
                },
                required = new[] { "id" }
            }
        }
    };

    // Called by McpHost for mode-aware execution
    public Task<string> ExecuteAsync(AgentMode mode, string argumentsJson, CancellationToken cancellationToken = default)
        => ResolveAsync(argumentsJson, mode);

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
        => ResolveAsync(argumentsJson, null);

    private Task<string> ResolveAsync(string argumentsJson, AgentMode? mode)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var id = ToolJson.GetStringTrimmed(doc.RootElement, "id");

            if (string.IsNullOrEmpty(id))
                return Task.FromResult(BuildIndex(mode));

            // Exact skill match takes priority (skill ids look like tool names and would confuse tool lookup)
            if (_skillManager != null)
            {
                var body = _skillManager.LoadBody(id);
                if (body != null)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("found: true");
                    sb.AppendLine("kind: skill");
                    sb.AppendLine($"skill: {id}");
                    sb.AppendLine("---");
                    sb.Append(body);
                    return Task.FromResult(sb.ToString());
                }
            }

            // Tool lookup (exact + partial suggestions)
            var toolResult = _host.GetToolHelp(id, mode);

            // If tool not found either, append skill suggestions too
            if (toolResult.StartsWith("found: false", StringComparison.Ordinal) && _skillManager != null)
            {
                var skillHits = _skillManager.GetEnabled()
                    .Where(s => s.Id.Contains(id, StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.Id)
                    .Take(5)
                    .ToArray();

                if (skillHits.Length > 0)
                {
                    toolResult += "\nskill_suggestions:";
                    foreach (var s in skillHits)
                        toolResult += $"\n  - {s}";
                }
            }

            return Task.FromResult(toolResult);
        }
        catch (JsonException)
        {
            return Task.FromResult("found: false\nreason: invalid_json");
        }
    }

    private string BuildIndex(AgentMode? mode)
    {
        var sb = new StringBuilder();
        sb.AppendLine("kind: index");
        sb.AppendLine("tools:");
        foreach (var def in _host.GetToolDefinitions())
            sb.AppendLine($"  - {def.Function.Name}: {def.Function.Description}");

        if (_skillManager != null)
        {
            var skills = _skillManager.GetEnabled().ToList();
            if (skills.Count > 0)
            {
                sb.AppendLine("skills:");
                foreach (var skill in skills)
                    sb.AppendLine($"  - {skill.Id}: {skill.Description}");
            }
        }

        return sb.ToString();
    }

    public string? GetHelpText() => """
        tool: agent_info

        summary: Universal capability lookup. Resolves by kind automatically.
                 Use for: tool usage help, skill instructions, or full capability index.

        arguments:
          id:
            required: false
            formats:
              - exact_tool_name   → tool schema + help text
              - exact_skill_id    → full skill instruction body
              - partial_name      → suggestions from tools + skills
              - (omit)            → full index of all tools and skills
            examples:
              - network_scan_tcp_ports
              - network.range-audit
              - scan

        behavior:
          skill_match:   found: true, kind: skill — returns full skill body.
          tool_match:    found: true, kind: tool  — returns schema + help.
          partial_match: found: false + suggestions from tools and skills.
          empty_id:      kind: index — all tools and skills listed.

        examples:
          - request:
              id: network.range-audit
          - request:
              id: network_scan_tcp_ports
          - request: {}
        """;
}
