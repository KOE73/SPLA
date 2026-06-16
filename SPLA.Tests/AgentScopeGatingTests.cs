using SPLA.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Permissions;

namespace SPLA.Tests;

/// <summary>
/// Fundamental agent capabilities (ToolScope.Agent) must bypass both mode filtering and the
/// permission policy — always available, in every mode including Chat.
/// </summary>
public class AgentScopeGatingTests
{
    private static ToolDefinition Tool(ToolScope scope, ToolEffect effect = ToolEffect.Read) => new()
    {
        Function = new ToolFunctionDefinition { Name = "t", Scope = scope, Effect = effect }
    };

    [Theory]
    [InlineData(AgentMode.Chat)]
    [InlineData(AgentMode.Research)]
    [InlineData(AgentMode.Inspect)]
    [InlineData(AgentMode.Edit)]
    [InlineData(AgentMode.Agent)]
    public void Filter_always_allows_agent_scope(AgentMode mode)
    {
        // Even a Write-effect agent tool (like agent.memory) passes in every mode.
        Assert.True(ToolModeFilter.IsAllowed(Tool(ToolScope.Agent, ToolEffect.Write), mode));
    }

    [Fact]
    public void Filter_still_blocks_non_agent_tools_in_chat()
    {
        Assert.False(ToolModeFilter.IsAllowed(Tool(ToolScope.Local, ToolEffect.Read), AgentMode.Chat));
    }

    [Fact]
    public void Filter_keeps_only_agent_tools_in_chat()
    {
        var tools = new[]
        {
            Tool(ToolScope.Agent),
            Tool(ToolScope.Local, ToolEffect.Read),
            Tool(ToolScope.Internet)
        };
        var allowed = ToolModeFilter.Filter(tools, AgentMode.Chat);
        Assert.Single(allowed);
        Assert.Equal(ToolScope.Agent, allowed[0].Function.Scope);
    }

    [Theory]
    [InlineData(AgentMode.Chat)]
    [InlineData(AgentMode.Research)]
    [InlineData(AgentMode.Agent)]
    public void Permission_always_allows_agent_scope(AgentMode mode)
    {
        var pm = new PermissionManager();
        var def = new ToolFunctionDefinition { Name = "agent.memory", Scope = ToolScope.Agent, Effect = ToolEffect.Write };
        Assert.Equal(PermissionResult.Allow, pm.CheckPermission(mode, def, "{}"));
    }

    [Fact]
    public void Permission_denies_local_write_in_chat()
    {
        var pm = new PermissionManager();
        var def = new ToolFunctionDefinition { Name = "fs.write", Scope = ToolScope.Local, Effect = ToolEffect.Write };
        Assert.Equal(PermissionResult.Deny, pm.CheckPermission(AgentMode.Chat, def, "{}"));
    }
}
