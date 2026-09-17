using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.ToolSets;
using SPLA.Runtime;
using System.Collections.Generic;
using System.Linq;

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

    // ── A role's tool-set levels: one rule, read from the running session's own settings ─────────

    private static ToolSetRegistry BuildRegistry(ResolvedSettings settings, params (string setId, string toolName)[] members)
    {
        var registry = new ToolSetRegistry(settings);
        foreach (var group in members.GroupBy(m => m.setId))
            registry.AddDynamic(new ToolSetDescriptor
            {
                Id = group.Key,
                Origin = ToolSetOrigin.Core,
                OriginId = "core",
                ToolNames = group.Select(m => m.toolName).ToList()
            });
        return registry;
    }

    private static IDisposable SessionUnder(ResolvedSettings? settings) =>
        SPLA.Domain.Agent.AgentSessionScope.Begin(new SPLA.Domain.Agent.AgentSession(
            new SPLA.Domain.Agent.KeyValueStore("session"), new SPLA.Domain.Agent.MarkManager(),
            new SPLA.Domain.Agent.SkillSession(), settings: settings));

    /// <summary>A session with no settings of its own (a plain chat) answers from the project's.</summary>
    [Fact]
    public void A_session_without_its_own_settings_gets_the_projects_levels()
    {
        var project = new ResolvedSettings();
        var registry = BuildRegistry(project, ("net", "net_tool"));

        using (SessionUnder(null))
            Assert.True(registry.IsDisclosed("net_tool"));
    }

    /// <summary>A role's <c>toolsets:</c> hides a set the project leaves on — inside the role's
    /// session only; the same registry asked outside it still answers for the project.</summary>
    [Fact]
    public void A_roles_session_hides_a_set_the_role_disables_and_nobody_elses()
    {
        var project = new ResolvedSettings();
        var registry = BuildRegistry(project, ("net", "net_tool"));
        var role = new ResolvedSettings();
        role.ToolSets["net"] = "disabled";

        using (SessionUnder(role))
            Assert.False(registry.IsDisclosed("net_tool"));
        Assert.True(registry.IsDisclosed("net_tool"));
    }

    /// <summary>The widening half — what the old role filter could never do: a role enables a set the
    /// project keeps off, and inside its session the set is there.</summary>
    [Fact]
    public void A_roles_session_can_enable_a_set_the_project_keeps_off()
    {
        var project = new ResolvedSettings();
        project.ToolSets["net"] = "disabled";
        var registry = BuildRegistry(project, ("net", "net_tool"));
        var role = new ResolvedSettings();
        role.ToolSets["net"] = "enabled";

        using (SessionUnder(role))
            Assert.True(registry.IsDisclosed("net_tool"));
        Assert.False(registry.IsDisclosed("net_tool"));
    }

    /// <summary>A tool no set claims is nobody's to gate.</summary>
    [Fact]
    public void A_tool_no_set_claims_is_always_disclosed()
    {
        var registry = BuildRegistry(new ResolvedSettings());
        var role = new ResolvedSettings();
        role.ToolSets["net"] = "disabled";

        using (SessionUnder(role))
            Assert.True(registry.IsDisclosed("unclaimed_tool"));
    }

    /// <summary>A virtual reply tool is mixed in by the chat's own host and is never a member of any
    /// set, so no level can remove it.</summary>
    [Fact]
    public void A_virtual_reply_tool_is_always_offered()
    {
        var inner = new FakeToolHost
        {
            Definitions = { new() { Function = new ToolFunctionDefinition { Name = "net_tool" } } }
        };
        var source = new ReplyToolTests_FakeReplySource
        {
            Correspondences = { new Correspondence
            {
                Role = "architect", Purpose = "design review", InstanceNo = 1, ChatId = "some-id",
                Initiator = CorrespondenceInitiator.Self, ToolName = "reply_architect"
            }}
        };

        var host = new ChatToolHost(inner, source);

        Assert.Contains("reply_architect", host.GetToolDefinitions().Select(d => d.Function.Name));
    }

    private sealed class ReplyToolTests_FakeReplySource : IReplyToolSource
    {
        public List<Correspondence> Correspondences { get; init; } = new();
        IReadOnlyCollection<Correspondence> IReplyToolSource.Correspondences => Correspondences;
        public ChatRuntime.ReplyResult SendReply(string role, int instanceNo, string text) =>
            new(ChatRuntime.ReplyOutcome.Delivered, null);
    }
}
