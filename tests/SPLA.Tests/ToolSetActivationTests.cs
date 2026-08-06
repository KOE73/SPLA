using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.Tools;
using SPLA.MCP.Core.ToolSets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Activation is the other half of a level: the level says what the project allows, activation says
/// what is armed in THIS chat. These tests pin the two properties that make the split worth having —
/// a set raised in one chat is not raised in the next, and a level the user set is a ceiling the
/// agent cannot lift.
/// </summary>
public sealed class ToolSetActivationTests
{
    private sealed class FakeTool : IMcpTool
    {
        public string Name { get; init; } = "fake_tool";
        public ToolDefinition GetDefinition() => new()
        {
            Type = "function",
            Function = new ToolFunctionDefinition { Name = Name, Description = "fake" }
        };
        public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken ct = default) =>
            Task.FromResult(ToolResult.Text("ran"));
    }

    /// <summary>A registry with one set, levelled as asked, owning one tool. Built by hand rather
    /// than from a plugin directory: the behaviour under test is the gate, not discovery.</summary>
    private static (McpHost Host, ToolSetRegistry Sets) HostWith(ToolSetLevel level, string toolName = "ssh_run")
    {
        var settings = new ResolvedSettings();
        settings.ToolSets["ssh"] = ToolSetRegistry.Format(level);

        var sets = new ToolSetRegistry(settings, features:
        [
            new SPLA.MCP.Core.Agent.AgentFeature("ssh", tools: [new FakeTool { Name = toolName }])
        ]);

        var host = new McpHost(new PermissionManager(settings: settings)) { ToolSets = sets };
        host.RegisterTool(new FakeTool { Name = toolName });
        return (host, sets);
    }

    private static AgentSession Session() =>
        new(new KeyValueStore("session"), new MarkManager(), new SkillSession());

    [Fact]
    public void A_fully_enabled_set_is_disclosed_without_activation()
    {
        var (host, _) = HostWith(ToolSetLevel.Enabled);
        Assert.Contains(host.GetToolDefinitions(), d => d.Function.Name == "ssh_run");
    }

    [Fact]
    public void A_set_levelled_off_is_neither_disclosed_nor_permitted()
    {
        var (host, _) = HostWith(ToolSetLevel.Disabled);

        Assert.DoesNotContain(host.GetToolDefinitions(), d => d.Function.Name == "ssh_run");
        Assert.DoesNotContain("ssh_run", host.GetPermittedToolNames());
    }

    /// <summary>The distinction the skill probe depends on: waiting on a skill means permitted but
    /// not disclosed, so a skill that requires the set still resolves as available.</summary>
    [Fact]
    public void A_set_waiting_on_a_skill_is_permitted_but_not_disclosed()
    {
        var (host, _) = HostWith(ToolSetLevel.SkillDemand);

        Assert.DoesNotContain(host.GetToolDefinitions(), d => d.Function.Name == "ssh_run");
        Assert.Contains("ssh_run", host.GetPermittedToolNames());
    }

    [Fact]
    public async Task An_unraised_set_refuses_execution_and_says_who_can_raise_it()
    {
        var (host, _) = HostWith(ToolSetLevel.AgentDemand);
        using var scope = AgentSessionScope.Begin(Session());

        var result = (await host.ExecuteToolAsync(AgentMode.Agent, "ssh_run", "{}")).TextContent;

        Assert.Contains("toolset_activate", result);
        Assert.Contains("ssh", result);
    }

    /// <summary>A set levelled off must not even admit it exists — that is the one refusal that
    /// deliberately reveals nothing.</summary>
    [Fact]
    public async Task A_set_levelled_off_denies_existence()
    {
        var (host, _) = HostWith(ToolSetLevel.Disabled);
        using var scope = AgentSessionScope.Begin(Session());

        var result = (await host.ExecuteToolAsync(AgentMode.Agent, "ssh_run", "{}")).TextContent;

        Assert.Equal("Error: Tool 'ssh_run' not found.", result);
        Assert.DoesNotContain("toolset", result);
    }

    [Fact]
    public async Task Activation_discloses_the_set_in_this_chat_only()
    {
        var (host, sets) = HostWith(ToolSetLevel.AgentDemand);
        var chatA = Session();
        var chatB = Session();

        using (AgentSessionScope.Begin(chatA))
        {
            await new ToolSetActivateTool(sets).ExecuteAsync("""{"setId":"ssh"}""");
            Assert.Contains(host.GetToolDefinitions(), d => d.Function.Name == "ssh_run");
        }

        using (AgentSessionScope.Begin(chatB))
        {
            Assert.DoesNotContain(host.GetToolDefinitions(), d => d.Function.Name == "ssh_run");
        }
    }

    /// <summary>The ceiling: the agent may only raise what the user placed within its reach.</summary>
    [Fact]
    public async Task The_agent_cannot_raise_a_set_reserved_for_skills()
    {
        var (host, sets) = HostWith(ToolSetLevel.SkillDemand);
        using var scope = AgentSessionScope.Begin(Session());

        var result = (await new ToolSetActivateTool(sets).ExecuteAsync("""{"setId":"ssh"}""")).TextContent;

        Assert.StartsWith("error:", result);
        Assert.DoesNotContain(host.GetToolDefinitions(), d => d.Function.Name == "ssh_run");
    }

    [Fact]
    public async Task The_agent_cannot_release_a_set_a_skill_raised()
    {
        var (_, sets) = HostWith(ToolSetLevel.SkillDemand);
        var session = Session();
        session.ToolSets.Activate("ssh", ToolSetActivationBy.Skill, "required by skill 'x'");
        using var scope = AgentSessionScope.Begin(session);

        var result = (await new ToolSetDeactivateTool().ExecuteAsync("""{"setId":"ssh"}""")).TextContent;

        Assert.StartsWith("error:", result);
        Assert.True(session.ToolSets.IsActive("ssh"));
    }

    [Fact]
    public void A_skill_takes_only_its_own_sets_down()
    {
        var session = Session();
        session.ToolSets.Activate("ssh", ToolSetActivationBy.Skill);
        session.ToolSets.Activate("network", ToolSetActivationBy.Agent);

        session.ToolSets.DeactivateAllBy(ToolSetActivationBy.Skill);

        Assert.False(session.ToolSets.IsActive("ssh"));
        Assert.True(session.ToolSets.IsActive("network"));
    }
}
