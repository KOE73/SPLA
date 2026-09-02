using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.Runtime;

namespace SPLA.Tests;

/// <summary>
/// Wave 0 of <c>docs/plans/PLAN_20260902_agent_roles-and-correspondence.md</c>: <c>ChatToolHost</c>
/// is the seam Wave 5 will use to mix a chat's virtual <c>reply_*</c> tools into the surface. This
/// wave proves the seam empty — every member must delegate to the inner host verbatim, with nothing
/// added, dropped or altered along the way.
/// </summary>
public sealed class ChatToolHostTests
{
    private sealed class FakeToolHost : IToolHost
    {
        public List<(AgentMode mode, string name, string args, ToolCallContext? context)> Executed { get; } = new();
        public List<ToolDefinition> Definitions { get; init; } = new();

        public IEnumerable<ToolDefinition> GetToolDefinitions() => Definitions;

        public Task<ToolResult> ExecuteToolAsync(
            AgentMode mode, string name, string argumentsJson,
            CancellationToken cancellationToken = default, ToolCallContext? context = null)
        {
            Executed.Add((mode, name, argumentsJson, context));
            return Task.FromResult(ToolResult.Text($"result of {name}"));
        }
    }

    [Fact]
    public void GetToolDefinitions_passes_through_the_inner_hosts_definitions_unchanged()
    {
        var definitions = new List<ToolDefinition>
        {
            new() { Function = new ToolFunctionDefinition { Name = "tool_one", Description = "first" } },
            new() { Function = new ToolFunctionDefinition { Name = "tool_two", Description = "second" } }
        };
        var inner = new FakeToolHost { Definitions = definitions };
        var host = new ChatToolHost(inner);

        var seen = host.GetToolDefinitions().ToList();

        Assert.Equal(definitions, seen);
    }

    [Fact]
    public async Task ExecuteToolAsync_delegates_every_argument_verbatim_and_returns_the_inner_result()
    {
        var inner = new FakeToolHost();
        var host = new ChatToolHost(inner);
        var context = new ToolCallContext { Source = "test-caller" };
        using var cts = new CancellationTokenSource();

        var result = await host.ExecuteToolAsync(
            AgentMode.Agent, "some_tool", """{"x":1}""", cts.Token, context);

        var call = Assert.Single(inner.Executed);
        Assert.Equal(AgentMode.Agent, call.mode);
        Assert.Equal("some_tool", call.name);
        Assert.Equal("""{"x":1}""", call.args);
        Assert.Same(context, call.context);
        Assert.Equal("result of some_tool", result.ToString());
    }

    [Fact]
    public async Task ExecuteToolAsync_works_with_no_context_passed_the_same_as_the_inner_host()
    {
        var inner = new FakeToolHost();
        var host = new ChatToolHost(inner);

        await host.ExecuteToolAsync(AgentMode.Chat, "another_tool", "{}");

        var call = Assert.Single(inner.Executed);
        Assert.Null(call.context);
    }
}
