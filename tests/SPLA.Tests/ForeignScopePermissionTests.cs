using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Permissions;

namespace SPLA.Tests;

/// <summary>
/// Шаг 0 (../../docs/plans/PLAN_20260826_service_mcp-client.md): the new <see cref="ToolScope.Foreign"/>
/// axis for tools executed by a foreign MCP server, which declared none of our Scope/Effect/Risk.
/// The whole server is one basket — the grant is taken on the basket, never derived from a
/// description a stranger wrote (ADR_20260826_service_mcp-client §2/§3).
/// <para>
/// The regression that matters: <c>Decide</c>'s existing mode branches all end in Deny or Ask for an
/// unrecognised tool, so a forgotten <c>Foreign</c> branch would not open access — in Agent mode the
/// fallthrough is <c>Ask</c> too, indistinguishable by <see cref="PermissionResult"/> alone. Only
/// <see cref="PermissionVerdict.Category"/> == "foreign" proves the real branch ran.
/// </para>
/// </summary>
public sealed class ForeignScopePermissionTests
{
    private static ToolFunctionDefinition ForeignTool(string name = "t") =>
        new() { Name = name, Scope = ToolScope.Foreign, Effect = ToolEffect.Write };

    [Fact]
    public void Foreign_tool_is_denied_in_chat_mode()
    {
        var pm = new PermissionManager();
        var verdict = pm.CheckPermission(AgentMode.Chat, ForeignTool(), "{}");

        Assert.Equal(PermissionResult.Deny, verdict.Result);
    }

    [Theory]
    [InlineData(AgentMode.Research)]
    [InlineData(AgentMode.Inspect)]
    [InlineData(AgentMode.Edit)]
    [InlineData(AgentMode.Agent)]
    public void Foreign_tool_asks_for_confirmation_in_every_mode_other_than_chat(AgentMode mode)
    {
        var pm = new PermissionManager();
        var verdict = pm.CheckPermission(mode, ForeignTool(), "{}");

        Assert.Equal(PermissionResult.Ask, verdict.Result);
        // Agent mode's own fallthrough for an unclassified tool is also Ask — this is the assertion
        // that actually distinguishes the real Foreign branch from a forgotten one.
        Assert.Equal("foreign", verdict.Category);
    }

    [Fact]
    public void Project_override_allow_lets_a_foreign_tool_through_even_in_inspect_mode()
    {
        var settings = new ResolvedSettings { PermForeign = "allow" };
        var pm = new PermissionManager(settings: settings);

        var verdict = pm.CheckPermission(AgentMode.Inspect, ForeignTool(), "{}");

        Assert.Equal(PermissionResult.Allow, verdict.Result);
    }

    [Fact]
    public void Project_override_deny_blocks_a_foreign_tool_even_in_agent_mode()
    {
        var settings = new ResolvedSettings { PermForeign = "deny" };
        var pm = new PermissionManager(settings: settings);

        var verdict = pm.CheckPermission(AgentMode.Agent, ForeignTool(), "{}");

        Assert.Equal(PermissionResult.Deny, verdict.Result);
    }

    [Fact]
    public void A_remembered_allow_with_wildcard_arguments_turns_the_second_call_into_allow()
    {
        var pm = new PermissionManager();
        var tool = ForeignTool();

        // First call: no standing decision yet — asks.
        var first = pm.CheckPermission(AgentMode.Edit, tool, "{\"x\":1}");
        Assert.Equal(PermissionResult.Ask, first.Result);

        // The host records the user's confirmation exactly the way "ask once, remember" already
        // works for every other category — no new mechanism for Foreign.
        pm.Remember(new RememberedToolPermission
        {
            Tool = tool.Name,
            Arguments = "*",
            Decision = PermissionDecision.AllowRemember
        });

        var second = pm.CheckPermission(AgentMode.Edit, tool, "{\"x\":2}");
        Assert.Equal(PermissionResult.Allow, second.Result);
    }

    [Fact]
    public void Permissions_internet_allow_does_not_open_a_foreign_tool()
    {
        // The entire reason Foreign is its own category rather than reusing Internet: an internet
        // override set for web_fetch must not silently widen into "any foreign MCP server allowed".
        var settings = new ResolvedSettings { PermInternet = "allow" };
        var pm = new PermissionManager(settings: settings);

        var verdict = pm.CheckPermission(AgentMode.Agent, ForeignTool(), "{}");

        Assert.Equal(PermissionResult.Ask, verdict.Result);
        Assert.Equal("foreign", verdict.Category);
    }

    [Fact]
    public void A_remembered_confirmation_works_in_agent_mode_too()
    {
        // Agent mode skips the remembered-permissions lookup for every other scope, and correctly so:
        // there everything is already Allow, and a stale remembered denial must not override the mode.
        // Foreign breaks that premise — it is Ask in Agent mode as well — so the lookup has to stay
        // reachable for it. Without this, "confirm the first call to each tool" would fire on every
        // single call in the mode people actually work in, and a prompt that always fires is a prompt
        // nobody reads.
        var tool = ForeignTool("ghmcp_create_issue");
        var pm = new PermissionManager();

        Assert.Equal(PermissionResult.Ask, pm.CheckPermission(AgentMode.Agent, tool, "{\"x\":1}").Result);

        pm.Remember(new RememberedToolPermission
        {
            Tool = tool.Name,
            Arguments = "*",
            Decision = PermissionDecision.AllowRemember
        });

        var second = pm.CheckPermission(AgentMode.Agent, tool, "{\"x\":2}");
        Assert.Equal(PermissionResult.Allow, second.Result);
    }

    [Fact]
    public void A_remembered_denial_is_honoured_for_a_foreign_tool_in_agent_mode()
    {
        // The other half of reaching the lookup: for every other scope a remembered denial is ignored
        // in Agent mode because the mode's own rules are authoritative and permissive. Here the mode
        // says Ask, so a person who already said no must not be asked the same question again.
        var tool = ForeignTool("ghmcp_delete_repo");
        var pm = new PermissionManager();
        pm.Remember(new RememberedToolPermission
        {
            Tool = tool.Name,
            Arguments = "*",
            Decision = PermissionDecision.Deny
        });

        var verdict = pm.CheckPermission(AgentMode.Agent, tool, "{}");

        Assert.Equal(PermissionResult.Deny, verdict.Result);
    }
}
