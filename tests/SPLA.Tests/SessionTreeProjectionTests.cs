using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using SPLA.Service;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Wave 7 (docs/plans/PLAN_20260902_agent_roles-and-correspondence.md, commit 549e8c2,
/// <see cref="RuntimeProjections.List"/>): the chat list becomes a tree instead of a flat list that
/// either hides spawned sessions or dumps them in with human ones. Two properties must both hold —
/// <see cref="ChatManager.ListChats"/> already proved the flat exclusion half
/// (<c>SpawnedSessionIntegrationTests</c>); this proves the tree-nesting half built on top of it in
/// <c>RuntimeProjections.List</c>/<c>ToSummary</c>.
/// </summary>
public sealed class SessionTreeProjectionTests
{
    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-tree-{Guid.NewGuid():N}")).FullName;

    private static (AgentRuntime Runtime, ChatRegistry Chats, string Root) BuildProject()
    {
        var root = TempRoot();
        var manifest = Path.Combine(root, "test.spla");
        File.WriteAllText(manifest, """
            version: 1
            name: TreeTest
            workspace: .
            agent:
              mode: Edit
            """);
        var settings = ConfigLoader.LoadAndResolve(manifest);
        var runtime = new AgentRuntime(settings, NullLoggerFactory.Instance);
        var chats = new ChatRegistry(runtime);
        runtime.SpawnedRunner.AttachSessionHost(chats);
        return (runtime, chats, root);
    }

    /// <summary>A spawned session must never appear as a top-level entry of the human list — only as
    /// someone's <see cref="Contracts.ChatSummaryDto.Children"/> — while still being fully reachable
    /// through the tree, role and parent id intact.</summary>
    [Fact]
    public void A_spawned_session_is_nested_under_its_parent_and_absent_from_the_top_level_list()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var parent = chats.CreateNew("Parent chat");
            var spawned = chats.OpenSpawnedSession(parent.ChatId, role: "reviewer");
            spawned.Finish(Array.Empty<SPLA.Domain.Models.ChatMessage>(), skillId: null, mode: "Edit",
                startedAt: DateTimeOffset.UtcNow, outcome: "completed", error: null);
            spawned.Dispose();

            var list = chats.List();

            // Top level: exactly the human chat, never the spawned one.
            Assert.Single(list);
            var parentSummary = list.Single();
            Assert.Equal(parent.ChatId, parentSummary.Id);
            Assert.DoesNotContain(list, s => s.Id == spawned.ChatId);

            // Nested: the spawned session shows up as the parent's own child, carrying its role and
            // parent id.
            Assert.NotNull(parentSummary.Children);
            var child = Assert.Single(parentSummary.Children!);
            Assert.Equal(spawned.ChatId, child.Id);
            Assert.Equal("reviewer", child.As);
            Assert.Equal("spawned", child.Origin);
            Assert.Equal(parent.ChatId, child.Parent);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    /// <summary>A plain human chat with no spawned descendants reports no children at all — the
    /// overwhelming majority case, and the one the DTO's own doc comment calls out.</summary>
    [Fact]
    public void A_chat_with_no_spawned_descendants_has_null_children()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            chats.CreateNew("Solo chat");
            var summary = chats.List().Single();
            Assert.Null(summary.Children);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    /// <summary>Nesting goes as deep as the spawn chain actually reached — a spawn-of-a-spawn is the
    /// grandchild of the human root, not a second top-level or a flat child of it.</summary>
    [Fact]
    public void Nested_spawns_chain_two_levels_deep_in_the_tree()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var human = chats.CreateNew("Human chat");
            var mid = chats.OpenSpawnedSession(human.ChatId, role: "planner");
            mid.Finish(Array.Empty<SPLA.Domain.Models.ChatMessage>(), null, "Edit", DateTimeOffset.UtcNow, "completed", null);
            var leaf = chats.OpenSpawnedSession(mid.ChatId, role: "coder");
            leaf.Finish(Array.Empty<SPLA.Domain.Models.ChatMessage>(), null, "Edit", DateTimeOffset.UtcNow, "completed", null);
            mid.Dispose();
            leaf.Dispose();

            var root_ = chats.List().Single(c => c.Id == human.ChatId);
            var midSummary = Assert.Single(root_.Children!);
            Assert.Equal(mid.ChatId, midSummary.Id);

            var leafSummary = Assert.Single(midSummary.Children!);
            Assert.Equal(leaf.ChatId, leafSummary.Id);
            Assert.Equal(mid.ChatId, leafSummary.Parent);

            // The tree only ever shows one top-level entry — the human root.
            Assert.Single(chats.List());
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    /// <summary>A batch of several spawned children off the same parent all nest under it — none of
    /// them leak to the top level, matching <c>ChatManager</c>'s own "a batch of spawns must not
    /// pollute the human chat list" requirement, now proven at the tree-projection layer too.</summary>
    [Fact]
    public void A_batch_of_spawned_children_all_nest_under_the_same_parent()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var parent = chats.CreateNew("Parent");
            var children = Enumerable.Range(0, 3).Select(i =>
            {
                var s = chats.OpenSpawnedSession(parent.ChatId, role: $"role{i}");
                s.Finish(Array.Empty<SPLA.Domain.Models.ChatMessage>(), null, "Edit", DateTimeOffset.UtcNow, "completed", null);
                s.Dispose();
                return s.ChatId;
            }).ToList();

            var list = chats.List();
            Assert.Single(list); // only the parent at the top level
            var summary = list.Single();
            Assert.Equal(3, summary.Children?.Count);
            foreach (var id in children)
                Assert.Contains(summary.Children!, c => c.Id == id && c.Parent == parent.ChatId);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }
}
