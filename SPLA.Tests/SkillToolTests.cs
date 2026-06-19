using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.Plugins;
using SPLA.MCP.Core.Tools;

namespace SPLA.Tests;

public class SkillToolTests
{
    private static SkillManager BuildSkillManager()
    {
        // SkillManager with no real files — we'll inject a skill via reflection for testing.
        // For tool tests we only need Find() to work; use a real file-less manager and test
        // the "unknown skill" path, plus a separate test with a stubbed session.
        return new SkillManager();
    }

    /// <summary>Opens an ambient agent session carrying <paramref name="skills"/> so the
    /// scope-based skill tools resolve it.</summary>
    private static IDisposable Scope(SkillSession skills)
        => AgentSessionScope.Begin(new AgentSession(new KeyValueStore("session"), new MarkManager(), skills));

    // ── skill_activate ───────────────────────────────────────────────────────

    [Fact]
    public async Task Activate_unknown_skill_returns_error()
    {
        var session = new SkillSession();
        var skills = BuildSkillManager();
        var tool = new SkillActivateTool(skills);

        using var _ = Scope(session);
        var result = await tool.ExecuteAsync("""{"id":"nonexistent.skill"}""");

        Assert.StartsWith("error: unknown skill", result);
    }

    [Fact]
    public async Task Activate_missing_id_returns_error()
    {
        var session = new SkillSession();
        var skills = BuildSkillManager();
        var tool = new SkillActivateTool(skills);

        using var _ = Scope(session);
        var result = await tool.ExecuteAsync("{}");

        Assert.StartsWith("error: 'id'", result);
    }

    [Fact]
    public async Task Activate_while_skill_active_returns_error()
    {
        var session = new SkillSession();
        session.Activate("first.skill");   // manually put into active state

        var skills = BuildSkillManager();
        var tool = new SkillActivateTool(skills);

        using var _ = Scope(session);
        var result = await tool.ExecuteAsync("""{"id":"second.skill"}""");

        Assert.StartsWith("error: skill 'first.skill' is already active", result);
    }

    [Fact]
    public async Task Activate_invalid_json_returns_error()
    {
        var session = new SkillSession();
        var tool = new SkillActivateTool(BuildSkillManager());

        using var _ = Scope(session);
        var result = await tool.ExecuteAsync("not-json");

        Assert.StartsWith("error: invalid_json", result);
    }

    // ── skill_deactivate ─────────────────────────────────────────────────────

    [Fact]
    public async Task Deactivate_when_active_returns_ok_and_clears_session()
    {
        var session = new SkillSession();
        session.Activate("network.range-audit");
        var tool = new SkillDeactivateTool();

        using var _ = Scope(session);
        var result = await tool.ExecuteAsync("{}");

        Assert.Equal("ok: deactivated 'network.range-audit'", result);
        Assert.Null(session.ActiveSkillId);
    }

    [Fact]
    public async Task Deactivate_when_idle_returns_noop_message()
    {
        var session = new SkillSession();
        var tool = new SkillDeactivateTool();

        using var _ = Scope(session);
        var result = await tool.ExecuteAsync("{}");

        Assert.Equal("ok: no active skill", result);
    }

    // ── PermissionManager — ToolScope.Skill ──────────────────────────────────

    [Fact]
    public void SkillScope_Chat_returns_Ask()
    {
        var pm = new PermissionManager();
        var def = new ToolFunctionDefinition { Name = "skill_activate", Scope = ToolScope.Skill, Effect = ToolEffect.Write, Risk = ToolRisk.Medium };
        Assert.Equal(PermissionResult.Ask, pm.CheckPermission(AgentMode.Chat, def, "{}"));
    }

    [Fact]
    public void SkillScope_Research_returns_Deny()
    {
        var pm = new PermissionManager();
        var def = new ToolFunctionDefinition { Name = "skill_activate", Scope = ToolScope.Skill, Effect = ToolEffect.Write, Risk = ToolRisk.Medium };
        Assert.Equal(PermissionResult.Deny, pm.CheckPermission(AgentMode.Research, def, "{}"));
    }

    [Fact]
    public void SkillScope_Edit_returns_Allow()
    {
        var pm = new PermissionManager();
        var def = new ToolFunctionDefinition { Name = "skill_activate", Scope = ToolScope.Skill, Effect = ToolEffect.Write, Risk = ToolRisk.Medium };
        Assert.Equal(PermissionResult.Allow, pm.CheckPermission(AgentMode.Edit, def, "{}"));
    }

    [Fact]
    public void SkillScope_Agent_returns_Allow()
    {
        var pm = new PermissionManager();
        var def = new ToolFunctionDefinition { Name = "skill_activate", Scope = ToolScope.Skill, Effect = ToolEffect.Write, Risk = ToolRisk.Medium };
        Assert.Equal(PermissionResult.Allow, pm.CheckPermission(AgentMode.Agent, def, "{}"));
    }

    [Fact]
    public void DeactivateTool_AgentScope_always_Allow()
    {
        var pm = new PermissionManager();
        var def = new ToolFunctionDefinition { Name = "skill_deactivate", Scope = ToolScope.Agent, Effect = ToolEffect.Write, Risk = ToolRisk.Low };
        foreach (var mode in Enum.GetValues<AgentMode>())
            Assert.Equal(PermissionResult.Allow, pm.CheckPermission(mode, def, "{}"));
    }
}
