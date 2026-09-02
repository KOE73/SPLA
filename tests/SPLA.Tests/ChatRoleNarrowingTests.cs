using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.ToolSets;
using SPLA.Runtime;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Wave 5б of <c>docs/plans/PLAN_20260902_agent_roles-and-correspondence.md</c>: a STANDING chat
/// opened <c>as: &lt;role&gt;</c> must have its own resolved settings, not just a per-chat mode
/// override — <see cref="ChatRuntime"/> resolves them once, at open, and narrows its tool surface
/// through <see cref="ChatToolHost"/> (<see cref="ChatRuntime.AvailableToolNames"/>) exactly the way
/// <c>SpawnedAgentRunner</c> already narrows a spawned run's (wave 3).
/// <para>
/// Real <see cref="AgentRuntime"/>/<see cref="ChatRegistry"/> against a temp project throughout — same
/// harness <c>ReplyToolTests.BuildProject</c> and <c>AgentSpawnToolTests</c>'s role-file tests use —
/// because the point being proven is the actual wiring (<c>ChatRuntime</c>'s constructor, the shared
/// <see cref="ToolSetRegistry"/>, <see cref="ConfigLoader.LoadRole"/>), not <see cref="ChatToolHost"/>'s
/// own filtering logic in isolation (that is <c>ChatToolHostTests</c>'s job).
/// </para>
/// </summary>
public sealed class ChatRoleNarrowingTests
{
    private sealed class FakeTool : IMcpTool
    {
        public string Name { get; init; } = "net_tool";

        public ToolDefinition GetDefinition() => new()
        {
            Type = "function",
            Function = new ToolFunctionDefinition { Name = Name, Description = "fake" }
        };

        public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken ct = default) =>
            Task.FromResult(ToolResult.Text("ran"));
    }

    private static string TempProjectDir() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-chatrole-{Guid.NewGuid():N}")).FullName;

    private static void WriteRoleFile(string projectDir, string roleName, string yaml)
    {
        var rolesDir = Directory.CreateDirectory(Path.Combine(projectDir, "roles")).FullName;
        File.WriteAllText(Path.Combine(rolesDir, roleName + ".yaml"), yaml);
    }

    /// <summary>Builds a real project declaring <paramref name="roles"/>, with a "net_tool" registered
    /// under a "net" tool set — the fixture every test below narrows (or doesn't) via a role's
    /// <c>toolsets:</c> selection. Role bodies (if any) must be written with <see cref="WriteRoleFile"/>
    /// into <paramref name="root"/>/roles BEFORE a chat naming that role is created — <c>ChatRuntime</c>
    /// resolves a role's settings once, in its constructor.</summary>
    private static (AgentRuntime Runtime, ChatRegistry Chats, string Root) BuildProject(params string[] roles)
    {
        var root = TempProjectDir();
        var manifest = Path.Combine(root, "test.spla");
        File.WriteAllText(manifest, $"""
            version: 1
            name: ChatRoleNarrowingTest
            workspace: .
            agent:
              mode: Edit
            roles: [{string.Join(", ", roles)}]
            """);
        var settings = ConfigLoader.LoadAndResolve(manifest);
        var runtime = new AgentRuntime(settings, NullLoggerFactory.Instance);

        runtime.McpHost.RegisterTool(new FakeTool { Name = "net_tool" });
        runtime.ToolSets.AddDynamic(new ToolSetDescriptor
        {
            Id = "net", Origin = ToolSetOrigin.Core, OriginId = "core", ToolNames = ["net_tool"]
        });

        var chats = new ChatRegistry(runtime);
        return (runtime, chats, root);
    }

    /// <summary>THE LEAK TEST. Two chats open at once in the same project, sharing the same
    /// <see cref="AgentRuntime.McpHost"/>/<see cref="AgentRuntime.ToolSets"/>: one under a role that
    /// disables the "net" set, one with no role at all. The role-less chat must still see the full
    /// surface — proving the narrowing lives entirely in the narrowed chat's own
    /// <see cref="ChatToolHost"/> instance and never touches the shared registry the other chat reads
    /// from. Getting this backwards (mutating the shared registry) would strip "net_tool" from every
    /// chat in the project, not just the one under the role.</summary>
    [Fact]
    public void A_role_that_disables_a_tool_set_narrows_only_its_own_chat_not_a_sibling_chat()
    {
        var (runtime, chats, root) = BuildProject("narrow");
        try
        {
            WriteRoleFile(root, "narrow", "toolsets:\n  net: disabled\n");

            var narrowed = chats.CreateNew("Narrowed chat", role: "narrow");
            var plain = chats.CreateNew("Plain chat");

            Assert.DoesNotContain("net_tool", narrowed.AvailableToolNames());
            Assert.Contains("net_tool", plain.AvailableToolNames()); // the leak check
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    /// <summary>The negative half of the same claim, isolated: a role with no <c>toolsets:</c> entry
    /// at all inherits the project's full surface rather than silently narrowing to nothing.</summary>
    [Fact]
    public void A_role_with_no_tool_selection_still_sees_the_full_set()
    {
        var (runtime, chats, root) = BuildProject("generalist");
        try
        {
            WriteRoleFile(root, "generalist", "mode: Edit\n"); // no toolsets: at all

            var chat = chats.CreateNew("Generalist chat", role: "generalist");

            Assert.Contains("net_tool", chat.AvailableToolNames());
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    /// <summary>A role's own <c>mode:</c> governs the chat outright — even when the chat's own
    /// per-chat agent-section override says something else — the same rule
    /// <c>SpawnedAgentRunner.RunAsync</c> already applies to a spawned run's <c>mode</c> argument.</summary>
    [Fact]
    public void A_chat_whose_role_declares_a_mode_runs_in_that_mode_even_over_its_own_override()
    {
        var (runtime, chats, root) = BuildProject("agent-role");
        try
        {
            WriteRoleFile(root, "agent-role", "mode: Agent\n");

            var chat = chats.CreateNew("Agent-mode chat", role: "agent-role");
            // The chat's own override says Chat; the role says Agent. The role must win.
            chat.ApplySettings(mode: "Chat", modelId: null);

            Assert.Equal(AgentMode.Agent.ToString(), chat.ModeName);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    /// <summary>An ordinary chat with no <c>as:</c> at all is completely unaffected by any of this —
    /// its own per-chat mode override still governs, and its tool surface is the project's full one.</summary>
    [Fact]
    public void A_chat_with_no_role_is_completely_unaffected()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var chat = chats.CreateNew("Ordinary chat");
            chat.ApplySettings(mode: "Chat", modelId: null);

            Assert.Equal(AgentMode.Chat.ToString(), chat.ModeName);
            Assert.Contains("net_tool", chat.AvailableToolNames());
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    /// <summary>A role named on <c>agent_correspond</c>'s on-demand correspondent chat must narrow it
    /// from its very first turn — the exact scenario the wave's own motivation names. Proves
    /// <c>ChatRegistry.CreateNew(title, role)</c>/<c>ChatManager.CreateNewChat(title, role)</c> stamp
    /// <c>as:</c> before the correspondent's <see cref="ChatRuntime"/> constructor ever runs, rather
    /// than a caller patching <c>Session.As</c> onto an already-open runtime (which would narrow
    /// nothing until the chat was closed and reopened).</summary>
    [Fact]
    public void Agent_correspond_narrows_the_on_demand_correspondent_chat_from_its_first_turn()
    {
        var (runtime, chats, root) = BuildProject("narrow");
        try
        {
            WriteRoleFile(root, "narrow", "toolsets:\n  net: disabled\n");

            var reviewer = chats.CreateNew("Reviewer");

            var result = reviewer.Correspond("narrow", "design review", "hello");
            Assert.True(result.Delivered);

            var correspondence = Assert.Single(reviewer.Correspondences);
            var correspondentChat = chats.GetOrOpen(correspondence.ChatId);
            Assert.NotNull(correspondentChat);
            Assert.Equal("narrow", correspondentChat!.Session.As);
            Assert.DoesNotContain("net_tool", correspondentChat.AvailableToolNames());
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }
}
