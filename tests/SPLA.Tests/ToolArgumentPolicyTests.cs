using SPLA.Domain.Models;
using SPLA.MCP.Core.Permissions;
using System;
using System.Collections.Generic;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The seat for domain argument checks inside the single verdict. No module ships yet — these guard
/// the rules the seat imposes, which is the part that would be expensive to discover wrong later:
/// a module that could widen its own rights, or one whose bug silently deletes a tool.
/// </summary>
public class ToolArgumentPolicyTests
{
    private static ToolFunctionDefinition ShellTool() => new()
    {
        Name = "system_run_shell",
        Scope = ToolScope.Shell,
        Effect = ToolEffect.Write,
        Risk = ToolRisk.Medium
    };

    private static ToolFunctionDefinition AgentTool() => new()
    {
        Name = "agent_memory_get",
        Scope = ToolScope.Agent,
        Effect = ToolEffect.Read
    };

    private sealed class FixedPolicy : IToolArgumentPolicy
    {
        private readonly PermissionVerdict? _verdict;
        public int Calls { get; private set; }

        public FixedPolicy(PermissionVerdict? verdict) => _verdict = verdict;

        public bool AppliesTo(ToolFunctionDefinition tool) => true;

        public PermissionVerdict? Evaluate(ToolFunctionDefinition tool, string argumentsJson)
        {
            Calls++;
            return _verdict;
        }
    }

    private sealed class ThrowingPolicy : IToolArgumentPolicy
    {
        public bool AppliesTo(ToolFunctionDefinition tool) => true;
        public PermissionVerdict? Evaluate(ToolFunctionDefinition tool, string argumentsJson)
            => throw new InvalidOperationException("bad parser");
    }

    private static PermissionManager With(params IToolArgumentPolicy[] policies)
        => new(argumentPolicies: policies);

    [Fact]
    public void Abstaining_module_leaves_the_verdict_alone()
    {
        var pm = With(new FixedPolicy(null));

        // Agent mode allows shell outright; an abstention must not disturb that.
        Assert.Equal(
            PermissionResult.Allow,
            pm.CheckPermission(AgentMode.Agent, ShellTool(), "{\"command\":\"ls\"}").Result);
    }

    [Fact]
    public void Module_narrows_allow_to_ask()
    {
        var pm = With(new FixedPolicy(PermissionVerdict.Ask("touches production")));

        var verdict = pm.CheckPermission(AgentMode.Agent, ShellTool(), "{}");

        Assert.Equal(PermissionResult.Ask, verdict.Result);
        Assert.Equal("touches production", verdict.Reason);
    }

    [Fact]
    public void Module_narrows_allow_to_deny()
    {
        var pm = With(new FixedPolicy(PermissionVerdict.Deny("DROP is never allowed")));

        Assert.Equal(
            PermissionResult.Deny,
            pm.CheckPermission(AgentMode.Agent, ShellTool(), "{}").Result);
    }

    [Fact]
    public void Module_cannot_widen_a_denial()
    {
        // The rule that matters most: otherwise registering a module becomes a way to grant yourself
        // rights the mode never gave.
        var pm = With(new FixedPolicy(PermissionVerdict.Allow("I say it is fine")));

        // Chat mode denies every tool call.
        Assert.Equal(
            PermissionResult.Deny,
            pm.CheckPermission(AgentMode.Chat, ShellTool(), "{}").Result);
    }

    [Fact]
    public void Module_cannot_widen_an_ask()
    {
        var pm = With(new FixedPolicy(PermissionVerdict.Allow("I say it is fine")));

        // Edit mode asks before a project write.
        var writeTool = new ToolFunctionDefinition
        {
            Name = "system_write_file", Scope = ToolScope.Project, Effect = ToolEffect.Write
        };

        Assert.Equal(
            PermissionResult.Ask,
            pm.CheckPermission(AgentMode.Edit, writeTool, "{}").Result);
    }

    [Fact]
    public void Strictest_module_wins_regardless_of_registration_order()
    {
        var lenient = PermissionVerdict.Ask("wants a look");
        var strict = PermissionVerdict.Deny("absolutely not");

        var forwards = With(new FixedPolicy(lenient), new FixedPolicy(strict));
        var backwards = With(new FixedPolicy(strict), new FixedPolicy(lenient));

        Assert.Equal(
            PermissionResult.Deny, forwards.CheckPermission(AgentMode.Agent, ShellTool(), "{}").Result);
        Assert.Equal(
            PermissionResult.Deny, backwards.CheckPermission(AgentMode.Agent, ShellTool(), "{}").Result);
    }

    [Fact]
    public void Throwing_module_abstains_rather_than_denies()
    {
        // A bug in a parser must not present as "the tool stopped working".
        var pm = With(new ThrowingPolicy());

        Assert.Equal(
            PermissionResult.Allow,
            pm.CheckPermission(AgentMode.Agent, ShellTool(), "{}").Result);
    }

    [Fact]
    public void Modules_are_not_asked_about_agent_scoped_capabilities()
    {
        var policy = new FixedPolicy(PermissionVerdict.Deny("no"));
        var pm = With(policy);

        Assert.Equal(
            PermissionResult.Allow,
            pm.CheckPermission(AgentMode.Chat, AgentTool(), "{}").Result);
        Assert.Equal(0, policy.Calls);
    }

    [Fact]
    public void Modules_are_not_asked_when_the_verdict_is_already_deny()
    {
        var policy = new FixedPolicy(PermissionVerdict.Deny("no"));
        var pm = With(policy);

        pm.CheckPermission(AgentMode.Chat, ShellTool(), "{}");

        Assert.Equal(0, policy.Calls);
    }

    // ---- disclosure ---------------------------------------------------------------------------

    [Fact]
    public void Disclosure_never_consults_modules()
    {
        // The bug this prevents: a domain module handed "{}" has no statement to judge, and its
        // denial would delete the tool from the model's list in every mode.
        var policy = new FixedPolicy(PermissionVerdict.Deny("cannot parse an empty statement"));
        var pm = With(policy);

        Assert.NotEqual(
            PermissionResult.Deny, pm.CheckToolCeiling(AgentMode.Agent, ShellTool()).Result);
        Assert.Equal(0, policy.Calls);
    }

    [Fact]
    public void Ceiling_still_honours_mode_and_project_policy()
    {
        // Skipping the modules must not turn the ceiling into "everything is visible".
        var pm = With(new FixedPolicy(null));

        Assert.Equal(
            PermissionResult.Deny, pm.CheckToolCeiling(AgentMode.Chat, ShellTool()).Result);
    }

    [Fact]
    public void With_no_modules_the_two_entries_agree()
    {
        // Nothing registered is the shipping configuration; the split must be invisible there.
        var pm = new PermissionManager();

        foreach (var mode in Enum.GetValues<AgentMode>())
        {
            Assert.Equal(
                pm.CheckPermission(mode, ShellTool(), "{}").Result,
                pm.CheckToolCeiling(mode, ShellTool()).Result);
        }
    }
}
