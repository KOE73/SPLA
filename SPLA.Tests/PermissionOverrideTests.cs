using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Permissions;

namespace SPLA.Tests;

/// <summary>
/// Phase 3.0 (docs/host-abstraction-plan.md): ResolvedSettings.PermRead/PermWrite/PermShell/
/// PermInternet were already round-tripped through the settings UI (SettingsOps.GetAgent/SaveAgent,
/// persisted to the .spla project) but PermissionManager never consulted them — a project could be
/// configured to deny shell entirely and the agent would ignore it completely. This closes that gap:
/// an explicit project override is a hard floor/ceiling for its category, applying in every mode
/// (including Agent) and read live off the same ResolvedSettings instance the settings UI mutates,
/// so an in-session save takes effect on the very next tool call.
/// </summary>
public sealed class PermissionOverrideTests
{
    private static ToolFunctionDefinition Def(ToolScope scope, ToolEffect effect) =>
        new() { Name = "t", Scope = scope, Effect = effect };

    [Fact]
    public void Explicit_deny_override_blocks_shell_even_in_agent_mode()
    {
        var settings = new ResolvedSettings { PermShell = "deny" };
        var pm = new PermissionManager(settings: settings);
        var shellTool = Def(ToolScope.Shell, ToolEffect.Execute);

        // Agent mode would otherwise Allow (or Ask for Danger risk) shell tools — the override wins.
        Assert.Equal(PermissionResult.Deny, pm.CheckPermission(AgentMode.Agent, shellTool, "{}"));
    }

    [Fact]
    public void Explicit_allow_override_skips_the_ask_prompt_in_edit_mode()
    {
        var settings = new ResolvedSettings { PermWrite = "allow" };
        var pm = new PermissionManager(settings: settings);
        var writeTool = Def(ToolScope.Project, ToolEffect.Write);

        // Edit mode normally Asks for project writes — the override bypasses that.
        Assert.Equal(PermissionResult.Allow, pm.CheckPermission(AgentMode.Edit, writeTool, "{}"));
    }

    [Fact]
    public void Override_is_read_live_from_the_same_settings_instance()
    {
        var settings = new ResolvedSettings();
        var pm = new PermissionManager(settings: settings);
        var writeTool = Def(ToolScope.Project, ToolEffect.Write);

        // No override yet — falls through to Edit mode's default (Ask).
        Assert.Equal(PermissionResult.Ask, pm.CheckPermission(AgentMode.Edit, writeTool, "{}"));

        // Settings UI mutates the SAME ResolvedSettings instance in place (SettingsOps.SaveAgent) —
        // no PermissionManager reconstruction needed for the change to take effect.
        settings.PermWrite = "deny";
        Assert.Equal(PermissionResult.Deny, pm.CheckPermission(AgentMode.Edit, writeTool, "{}"));
    }

    [Fact]
    public void Unset_override_falls_through_to_mode_defaults_unchanged()
    {
        var pm = new PermissionManager(settings: new ResolvedSettings());
        var readTool = Def(ToolScope.Project, ToolEffect.Read);

        Assert.Equal(PermissionResult.Allow, pm.CheckPermission(AgentMode.Edit, readTool, "{}"));
        Assert.Equal(PermissionResult.Deny, pm.CheckPermission(AgentMode.Chat, readTool, "{}"));
    }

    [Fact]
    public void Null_settings_behaves_exactly_like_the_no_override_constructor()
    {
        var withNullSettings = new PermissionManager(settings: null);
        var legacy = new PermissionManager();
        var writeTool = Def(ToolScope.Project, ToolEffect.Write);

        Assert.Equal(
            legacy.CheckPermission(AgentMode.Edit, writeTool, "{}"),
            withNullSettings.CheckPermission(AgentMode.Edit, writeTool, "{}"));
    }
}
