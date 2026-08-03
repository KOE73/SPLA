using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.ToolSets;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Raises a tool set in this chat: its tools appear in the next request with their full definitions.
/// Only sets the user levelled "on agent demand" can be raised this way — the agent never widens the
/// boundary the project drew, it only chooses from inside it.
/// </summary>
public sealed class ToolSetActivateTool : IMcpTool
{
    private readonly ToolSetRegistry _sets;

    public ToolSetActivateTool(ToolSetRegistry sets) => _sets = sets;

    public string Name => "toolset_activate";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Activates a tool set announced in the tool sets index, making its tools available in this chat.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    setId = new
                    {
                        type = "string",
                        description = "Id of the tool set to activate, exactly as announced (e.g. ssh)."
                    }
                },
                required = new[] { "setId" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var session = AgentSessionScope.Current?.ToolSets;
        if (session is null) return Task.FromResult("error: no active chat session");

        using var doc = JsonDocument.Parse(argumentsJson);
        var setId = ToolJson.GetStringTrimmed(doc.RootElement, "setId");
        if (string.IsNullOrEmpty(setId))
            return Task.FromResult("error: 'setId' parameter is required");

        var set = _sets.Find(setId);
        if (set is null)
            return Task.FromResult($"error: unknown tool set '{setId}'");

        var level = _sets.LevelOf(set.Id);
        if (level != ToolSetLevel.AgentDemand)
            return Task.FromResult(level switch
            {
                ToolSetLevel.Enabled => $"tool set '{set.Id}' is already fully available — no activation needed",
                ToolSetLevel.SkillDemand =>
                    $"error: tool set '{set.Id}' can only be activated by a skill that requires it, or by the user",
                _ => $"error: unknown tool set '{setId}'"
            });

        if (session.IsActive(set.Id))
            return Task.FromResult($"tool set '{set.Id}' is already active");

        session.Activate(set.Id, ToolSetActivationBy.Agent);

        var tools = set.ToolNames.Count > 0
            ? $" Tools: {string.Join(", ", set.ToolNames)}."
            : string.Empty;
        return Task.FromResult(
            $"tool set '{set.Id}' activated for this chat.{tools} Their full definitions are available from the next message on.");
    }
}

/// <summary>
/// Lowers a tool set the agent raised. Deliberately a permission, not a duty: nothing in the prompt
/// tells the model it must release a set, because "decide whether to give tools back" is exactly the
/// kind of bookkeeping decision this design removes. The user's control is the status bar.
/// </summary>
public sealed class ToolSetDeactivateTool : IMcpTool
{
    public string Name => "toolset_deactivate";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Deactivates a tool set activated earlier in this chat, removing its tools from the context.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    setId = new
                    {
                        type = "string",
                        description = "Id of the tool set to deactivate."
                    }
                },
                required = new[] { "setId" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var session = AgentSessionScope.Current?.ToolSets;
        if (session is null) return Task.FromResult("error: no active chat session");

        using var doc = JsonDocument.Parse(argumentsJson);
        var setId = ToolJson.GetStringTrimmed(doc.RootElement, "setId");
        if (string.IsNullOrEmpty(setId))
            return Task.FromResult("error: 'setId' parameter is required");

        // A set a skill raised is the skill's to drop: releasing it mid-procedure would break the
        // very requirements the skill declared.
        var activation = session.Active.FirstOrDefault(a =>
            string.Equals(a.SetId, setId, System.StringComparison.OrdinalIgnoreCase));
        if (activation.SetId is null)
            return Task.FromResult($"tool set '{setId}' is not active");
        if (activation.By != ToolSetActivationBy.Agent)
            return Task.FromResult(
                $"error: tool set '{setId}' was activated by {activation.By.ToString().ToLowerInvariant()} and cannot be released here");

        session.Deactivate(setId);
        return Task.FromResult($"tool set '{setId}' deactivated");
    }
}
