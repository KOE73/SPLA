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

    // ── Wave 5б: a chat's own role narrowing, layered under the reply-tool mixing above ──────────

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

    /// <summary>The invariant the whole wave depends on: passing a <see cref="ToolSetRegistry"/> but
    /// no role selection (<c>roleToolSets: null</c>) must leave the surface exactly as it always was —
    /// a chat with no <c>as:</c> narrows nothing, not "narrows against an empty set".</summary>
    [Fact]
    public void GetToolDefinitions_with_no_role_selection_does_not_narrow_even_with_a_registry_present()
    {
        var settings = new ResolvedSettings();
        var registry = BuildRegistry(settings, ("net", "net_tool"));
        var inner = new FakeToolHost
        {
            Definitions = { new() { Function = new ToolFunctionDefinition { Name = "net_tool" } } }
        };

        var host = new ChatToolHost(inner, owner: null, toolSets: registry, roleToolSets: null);

        Assert.Contains("net_tool", host.GetToolDefinitions().Select(d => d.Function.Name));
    }

    /// <summary>A role's own <c>toolsets:</c> selection, once resolved into a dictionary, genuinely
    /// hides a tool its set disables — even though the shared registry's own baseline leaves that set
    /// enabled. This is the actual narrowing the leak test elsewhere in the suite depends on.</summary>
    [Fact]
    public void GetToolDefinitions_hides_a_tool_whose_set_the_role_disables()
    {
        var settings = new ResolvedSettings(); // "net" has no toolsets: entry -> Enabled by default
        var registry = BuildRegistry(settings, ("net", "net_tool"));
        var inner = new FakeToolHost
        {
            Definitions =
            {
                new() { Function = new ToolFunctionDefinition { Name = "net_tool" } },
                new() { Function = new ToolFunctionDefinition { Name = "file_tool" } }
            }
        };
        var roleToolSets = new Dictionary<string, string> { ["net"] = "disabled" };

        var host = new ChatToolHost(inner, owner: null, toolSets: registry, roleToolSets: roleToolSets);
        var names = host.GetToolDefinitions().Select(d => d.Function.Name).ToList();

        Assert.DoesNotContain("net_tool", names);
        Assert.Contains("file_tool", names);
    }

    /// <summary>A role that names no selection for a set inherits the registry's own standing
    /// decision — inheritance, not an empty set, for a set the role's dictionary never mentions.</summary>
    [Fact]
    public void GetToolDefinitions_inherits_the_registrys_level_for_a_set_the_role_never_mentions()
    {
        var settings = new ResolvedSettings();
        settings.ToolSets["net"] = "disabled"; // the project's own standing decision
        var registry = BuildRegistry(settings, ("net", "net_tool"));
        var inner = new FakeToolHost
        {
            Definitions = { new() { Function = new ToolFunctionDefinition { Name = "net_tool" } } }
        };
        var roleToolSets = new Dictionary<string, string>(); // role says nothing about "net"

        var host = new ChatToolHost(inner, owner: null, toolSets: registry, roleToolSets: roleToolSets);

        Assert.DoesNotContain("net_tool", host.GetToolDefinitions().Select(d => d.Function.Name));
    }

    /// <summary>A tool no set claims is nobody's to gate and always survives the narrowing, same rule
    /// the registry itself uses.</summary>
    [Fact]
    public void GetToolDefinitions_never_hides_a_tool_no_set_claims()
    {
        var settings = new ResolvedSettings();
        var registry = BuildRegistry(settings); // no sets at all
        var inner = new FakeToolHost
        {
            Definitions = { new() { Function = new ToolFunctionDefinition { Name = "unclaimed_tool" } } }
        };
        var roleToolSets = new Dictionary<string, string> { ["net"] = "disabled" };

        var host = new ChatToolHost(inner, owner: null, toolSets: registry, roleToolSets: roleToolSets);

        Assert.Contains("unclaimed_tool", host.GetToolDefinitions().Select(d => d.Function.Name));
    }

    /// <summary>Role narrowing and reply-tool mixing are independent layers: a virtual reply tool is
    /// never a member of any <see cref="ToolSetRegistry"/> set, so even the harshest narrowing must
    /// not be able to remove it.</summary>
    [Fact]
    public void Role_narrowing_never_removes_a_virtual_reply_tool()
    {
        var settings = new ResolvedSettings();
        var registry = BuildRegistry(settings, ("net", "net_tool"));
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
        var roleToolSets = new Dictionary<string, string> { ["net"] = "disabled" };

        var host = new ChatToolHost(inner, source, registry, roleToolSets);
        var names = host.GetToolDefinitions().Select(d => d.Function.Name).ToList();

        Assert.DoesNotContain("net_tool", names);
        Assert.Contains("reply_architect", names);
    }

    private sealed class ReplyToolTests_FakeReplySource : IReplyToolSource
    {
        public List<Correspondence> Correspondences { get; init; } = new();
        IReadOnlyCollection<Correspondence> IReplyToolSource.Correspondences => Correspondences;
        public ChatRuntime.ReplyResult SendReply(string role, int instanceNo, string text) =>
            new(ChatRuntime.ReplyOutcome.Delivered, null);
    }
}
