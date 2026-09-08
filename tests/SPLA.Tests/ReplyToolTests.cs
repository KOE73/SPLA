using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Domain.Tools;
using SPLA.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Wave 5 (docs/plans/PLAN_20260902_agent_roles-and-correspondence.md §"Волна 5",
/// docs/adr/ADR_20260827-2_core_roles.md §2.3): the virtual <c>reply_&lt;role&gt;[_&lt;topic&gt;]</c>
/// tools <see cref="ChatToolHost"/> mixes into a chat's own tool surface.
/// <para>
/// Two harnesses on purpose. Tests that need a real address book (naming, cross-chat isolation,
/// actual delivery) drive a real <see cref="ChatRegistry"/> against a temp project, same as
/// <c>CorrespondenceTests</c>. Tests about <see cref="ChatToolHost"/>'s OWN mixing/gating logic use a
/// minimal <see cref="FakeReplySource"/> instead — that logic never touches a real chat's disk state
/// or its capability gate, and <see cref="IReplyToolSource"/> exists precisely so proving it does not
/// need either.
/// </para>
/// </summary>
public sealed class ReplyToolTests
{
    // ── fakes for the ChatToolHost-only tests ───────────────────────────────────────────────────

    private sealed class FakeToolHost : IToolHost
    {
        public List<ToolDefinition> Definitions { get; init; } = new();
        public List<(AgentMode mode, string name, string args)> Executed { get; } = new();

        public IEnumerable<ToolDefinition> GetToolDefinitions() => Definitions;

        public Task<ToolResult> ExecuteToolAsync(
            AgentMode mode, string name, string argumentsJson,
            CancellationToken cancellationToken = default, ToolCallContext? context = null)
        {
            Executed.Add((mode, name, argumentsJson));
            return Task.FromResult(ToolResult.Text($"result of {name}"));
        }
    }

    private sealed class FakeReplySource : IReplyToolSource
    {
        public List<Correspondence> Correspondences { get; init; } = new();
        IReadOnlyCollection<Correspondence> IReplyToolSource.Correspondences => Correspondences;

        /// <summary>What the next <see cref="SendReply"/> call answers with — the seam that lets a
        /// test stand in for a denied capability gate without a real <see cref="ISandbox"/>.</summary>
        public Func<string, int, string, ChatRuntime.ReplyResult>? OnSendReply { get; set; }
        public List<(string role, int instanceNo, string text)> SentReplies { get; } = new();

        public ChatRuntime.ReplyResult SendReply(string role, int instanceNo, string text)
        {
            SentReplies.Add((role, instanceNo, text));
            return OnSendReply?.Invoke(role, instanceNo, text)
                   ?? new ChatRuntime.ReplyResult(ChatRuntime.ReplyOutcome.Delivered, null);
        }
    }

    private static Correspondence MakeCorrespondence(string role, string purpose, string toolName) => new()
    {
        Role = role, Purpose = purpose, InstanceNo = 1, ChatId = "some-chat-id",
        Initiator = CorrespondenceInitiator.Self, ToolName = toolName
    };

    // ── ChatToolHost's own mixing/gating logic (fake source) ────────────────────────────────────

    [Fact]
    public void GetToolDefinitions_mixes_in_one_virtual_reply_tool_per_open_correspondence()
    {
        var source = new FakeReplySource
        {
            Correspondences = { MakeCorrespondence("architect", "design review", "reply_architect") }
        };
        var host = new ChatToolHost(new FakeToolHost(), source);

        var names = host.GetToolDefinitions().Select(d => d.Function.Name).ToList();

        Assert.Contains("reply_architect", names);
    }

    [Fact]
    public void GetToolDefinitions_with_no_source_or_no_correspondences_adds_nothing()
    {
        var inner = new FakeToolHost
        {
            Definitions = { new() { Function = new ToolFunctionDefinition { Name = "some_tool" } } }
        };

        var withNullSource = new ChatToolHost(inner);
        var withEmptySource = new ChatToolHost(inner, new FakeReplySource());

        Assert.Equal(new[] { "some_tool" }, withNullSource.GetToolDefinitions().Select(d => d.Function.Name));
        Assert.Equal(new[] { "some_tool" }, withEmptySource.GetToolDefinitions().Select(d => d.Function.Name));
    }

    [Fact]
    public async Task Calling_a_reply_tool_by_name_routes_to_SendReply_and_never_reaches_the_inner_host()
    {
        var source = new FakeReplySource
        {
            Correspondences = { MakeCorrespondence("architect", "design review", "reply_architect") }
        };
        var inner = new FakeToolHost();
        var host = new ChatToolHost(inner, source);

        var result = await host.ExecuteToolAsync(
            AgentMode.Agent, "reply_architect", """{"text":"what do you think?"}""");

        Assert.False(result.IsError);
        Assert.Single(source.SentReplies);
        Assert.Equal(("architect", 1, "what do you think?"), source.SentReplies[0]);
        Assert.Empty(inner.Executed); // never delegated — this is the whole point of the seam
    }

    [Fact]
    public async Task The_receipt_never_carries_the_correspondents_answer()
    {
        var source = new FakeReplySource
        {
            Correspondences = { MakeCorrespondence("architect", "design review", "reply_architect") }
        };
        var host = new ChatToolHost(new FakeToolHost(), source);

        var sentText = "this is a very specific piece of content only the correspondent should see";
        var result = await host.ExecuteToolAsync(
            AgentMode.Agent, "reply_architect", $$"""{"text":"{{sentText}}"}""");

        Assert.False(result.IsError);
        // The receipt must not echo back what was sent, and must not read as if it WERE an answer —
        // trap 10. A model that sees its own words reflected back, or prose that looks like a
        // response, stops waiting for the real one.
        Assert.DoesNotContain(sentText, result.TextContent);
        Assert.Contains("receipt", result.TextContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not", result.TextContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task A_toolset_levelled_off_does_not_hide_a_reply_tool()
    {
        // The inner host stands in for McpHost with some unrelated (or even the same) tool set
        // disabled — agent_correspond itself is gone from its definitions, the way a levelled-off
        // set's tools always are. The virtual reply tool must still be there: ChatToolHost never
        // consults ToolSetRegistry/ToolSetSession for these at all (agents/toolsets.md, "Virtual
        // reply tools are outside this system") — it is gated on the correspondence edge, not the set.
        var inner = new FakeToolHost(); // agent_correspond intentionally absent, as if levelled off
        var source = new FakeReplySource
        {
            Correspondences = { MakeCorrespondence("architect", "design review", "reply_architect") }
        };
        var host = new ChatToolHost(inner, source);

        var names = host.GetToolDefinitions().Select(d => d.Function.Name).ToList();

        Assert.DoesNotContain("agent_correspond", names); // the level-off is real
        Assert.Contains("reply_architect", names);          // but does not hide the reply tool
    }

    [Fact]
    public async Task A_missing_edge_grant_refuses_the_reply_tool()
    {
        var source = new FakeReplySource
        {
            Correspondences = { MakeCorrespondence("architect", "design review", "reply_architect") },
            OnSendReply = (_, _, _) => new ChatRuntime.ReplyResult(
                ChatRuntime.ReplyOutcome.Denied, "correspondence is not permitted for this chat")
        };
        var host = new ChatToolHost(new FakeToolHost(), source);

        var result = await host.ExecuteToolAsync(AgentMode.Agent, "reply_architect", """{"text":"hi"}""");

        Assert.Equal(ToolOutcome.Refused, result.Outcome);
        Assert.Contains("not permitted", result.TextContent);
    }

    // ── real project harness: naming, cross-chat isolation, actual delivery ────────────────────

    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-replytool-{Guid.NewGuid():N}")).FullName;

    private static (AgentRuntime Runtime, ChatRegistry Chats, string Root) BuildProject()
    {
        var root = TempRoot();
        var manifest = Path.Combine(root, "test.spla");
        File.WriteAllText(manifest, """
            version: 1
            name: ReplyToolTest
            workspace: .
            agent:
              mode: Edit
            roles: [architect]
            """);
        var settings = ConfigLoader.LoadAndResolve(manifest);
        var runtime = new AgentRuntime(settings, NullLoggerFactory.Instance);
        var chats = new ChatRegistry(runtime);
        return (runtime, chats, root);
    }

    [Fact]
    public void The_virtual_tool_appears_only_in_the_chat_that_owns_the_correspondence()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var architect = chats.CreateNew("Architect");
            var bystander = chats.CreateNew("Bystander");

            reviewer.OpenCorrespondence("architect", "design review", architect.ChatId, CorrespondenceInitiator.Self);

            var reviewerHost = new ChatToolHost(new FakeToolHost(), reviewer);
            var bystanderHost = new ChatToolHost(new FakeToolHost(), bystander);

            Assert.Contains("reply_architect_1", reviewerHost.GetToolDefinitions().Select(d => d.Function.Name));
            Assert.DoesNotContain("reply_architect_1", bystanderHost.GetToolDefinitions().Select(d => d.Function.Name));
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void The_tool_name_stays_stable_when_a_second_instance_of_the_same_role_is_opened()
    {
        // PLAN_20260906 wave 0: the address is a system-issued instance number, never the caller's
        // purpose text — a second instance only ever appears via the explicit 'another' flag, and its
        // number never depends on what either side's purpose happened to say.
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var architect1 = chats.CreateNew("Architect1");
            var architect2 = chats.CreateNew("Architect2");

            var first = reviewer.OpenCorrespondence(
                "architect", "api design", architect1.ChatId, CorrespondenceInitiator.Self);
            // Every public name carries its number, first instance included (ADR_20260906 §2.2 and
            // ReplyToolNaming.BuildPublicName): a bare "architect" would be the role, not a chat.
            Assert.Equal("reply_architect_1", first.ToolName);

            var second = reviewer.OpenCorrespondence(
                "architect", "db schema", architect2.ChatId, CorrespondenceInitiator.Self, another: true);
            Assert.Equal("reply_architect_2", second.ToolName); // a second instance: ordinal joins

            // The first one's name must not have moved just because a second one showed up later
            // (plan trap 11).
            var stillFirst = reviewer.Correspondences.Single(c => c.ChatId == architect1.ChatId);
            Assert.Equal("reply_architect_1", stillFirst.ToolName);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_purpose_is_optional_to_open_a_correspondence()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");

            var result = reviewer.Correspond("architect", "   ", "hello");

            Assert.True(result.Delivered);
            var correspondence = Assert.Single(reviewer.Correspondences);
            Assert.Equal("", correspondence.Purpose);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_role_is_required_to_open_a_correspondence()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");

            var result = reviewer.Correspond("   ", "purpose", "hello");

            Assert.False(result.Delivered);
            Assert.Equal(SPLA.Domain.Agent.CorrespondOutcome.InvalidArgument, result.Outcome);
            Assert.Empty(reviewer.Correspondences); // nothing was opened
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task The_reply_tool_executes_through_ChatToolHost_and_delivers_a_Peer_item()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var architect = chats.CreateNew("Architect");
            reviewer.OpenCorrespondence("architect", "design review", architect.ChatId, CorrespondenceInitiator.Self);

            var host = new ChatToolHost(new FakeToolHost(), reviewer);
            var result = await host.ExecuteToolAsync(
                AgentMode.Agent, "reply_architect_1", """{"text":"what do you think of this API?"}""");

            Assert.False(result.IsError);

            var drained = architect.Inbox.DrainAllWithKinds();
            var (message, kind) = Assert.Single(drained);
            Assert.Equal(InboxItemKind.Peer, kind);
            Assert.Equal(ChatRole.User, message.Role);
            Assert.Equal("what do you think of this API?", message.Content);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Agent_correspond_finds_or_creates_the_correspondents_chat_and_delivers_the_first_message()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");

            var result = reviewer.Correspond("architect", "design review", "hello, got a minute?");

            Assert.True(result.Delivered);
            var correspondence = Assert.Single(reviewer.Correspondences);
            Assert.Equal("architect", correspondence.Role);

            var correspondentChat = chats.GetOrOpen(correspondence.ChatId);
            Assert.NotNull(correspondentChat);
            Assert.Equal("architect", correspondentChat!.Session.As);

            var drained = correspondentChat.Inbox.DrainAllWithKinds();
            var (message, kind) = Assert.Single(drained);
            Assert.Equal(InboxItemKind.Peer, kind);
            Assert.Equal("hello, got a minute?", message.Content);

            // Calling it again with the same address reuses the correspondence rather than creating
            // a second correspondent chat.
            var second = reviewer.Correspond("architect", "design review", "still there?");
            Assert.True(second.Delivered);
            Assert.Single(reviewer.Correspondences);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Agent_correspond_refuses_an_undeclared_role()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");

            var result = reviewer.Correspond("nonexistent-role", "topic", "hello");

            Assert.False(result.Delivered);
            Assert.Equal(SPLA.Domain.Agent.CorrespondOutcome.UnknownRole, result.Outcome);
            Assert.Contains("architect", result.Message); // lists what IS available
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }
}
