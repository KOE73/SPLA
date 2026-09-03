using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Wave 7б of <c>docs/plans/PLAN_20260902_agent_roles-and-correspondence.md</c>
/// (<c>docs/adr/ADR_20260827-2_core_roles.md</c> §2.5's last row): a correspondence now survives a
/// restart, its reply count and estimated volume accumulate as replies cross it, and the project-wide
/// graph is assembled from sessions on disk — never from which <c>ChatRuntime</c>s happen to be open.
/// </summary>
public sealed class CorrespondenceGraphTests
{
    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-corr-graph-{Guid.NewGuid():N}")).FullName;

    private static (AgentRuntime Runtime, ChatRegistry Chats, string Root) BuildProject()
    {
        var root = TempRoot();
        var manifest = Path.Combine(root, "test.spla");
        File.WriteAllText(manifest, """
            version: 1
            name: CorrespondenceGraphTest
            workspace: .
            agent:
              mode: Edit
            """);
        var settings = ConfigLoader.LoadAndResolve(manifest);
        var runtime = new AgentRuntime(settings, NullLoggerFactory.Instance);
        var chats = new ChatRegistry(runtime);
        return (runtime, chats, root);
    }

    /// <summary>A plain <see cref="ChatManager"/> pointed at the same workspace, standing in for
    /// "nothing has this project open" — the same construction <c>ChatSessionActorFieldsTests</c> uses
    /// to prove a field round-trips without needing the rest of the runtime.</summary>
    private static ChatManager FreshManager(string root) => new(new ResolvedSettings
    {
        WorkspacePath = root,
        ProjectFilePath = Path.Combine(root, "test.spla")
    });

    [Fact]
    public void A_correspondence_survives_a_save_and_load()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer", "reviewer");
            var architect = chats.CreateNew("Architect", "architect");

            reviewer.OpenCorrespondence("architect", "design review", architect.ChatId, CorrespondenceInitiator.Self);
            reviewer.SendReply("architect", "design review", "what do you think?");
            reviewer.Save();

            var reloaded = runtime.ChatManager.LoadChat(reviewer.ChatId);

            Assert.NotNull(reloaded);
            var persisted = Assert.Single(reloaded!.Correspondences!);
            Assert.Equal("architect", persisted.Role);
            Assert.Equal("design review", persisted.Topic);
            Assert.Equal(architect.ChatId, persisted.ChatId);
            Assert.Equal("self", persisted.Initiator);
            Assert.Equal(1, persisted.Depth);
            Assert.True(persisted.VolumeEstimate > 0);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_reopened_chat_restores_its_correspondence_with_the_same_tool_name()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer", "reviewer");
            var architect = chats.CreateNew("Architect", "architect");
            var opened = reviewer.OpenCorrespondence("architect", "", architect.ChatId, CorrespondenceInitiator.Self);
            reviewer.SendReply("architect", "", "first message");
            reviewer.Save();

            // A fresh ChatRuntime over the same persisted session, the way reopening a chat after a
            // restart actually happens (ChatRegistry.GetOrOpen loads the session and constructs one).
            var freshSession = runtime.ChatManager.LoadChat(reviewer.ChatId)!;
            var reopened = new ChatRuntime(runtime, freshSession, chats);

            var restored = Assert.Single(reopened.Correspondences);
            Assert.Equal(opened.ToolName, restored.ToolName);
            Assert.Equal(1, restored.Depth);
            Assert.Equal(CorrespondenceInitiator.Self, restored.Initiator);

            // And it keeps working: a second reply accumulates onto the restored record rather than
            // starting a fresh count.
            var result = reopened.SendReply("architect", "", "second message");
            Assert.True(result.Delivered);
            Assert.Equal(2, reopened.Correspondences.Single().Depth);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Reply_count_and_volume_accumulate_across_multiple_sends()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer", "reviewer");
            var architect = chats.CreateNew("Architect", "architect");
            reviewer.OpenCorrespondence("architect", "", architect.ChatId, CorrespondenceInitiator.Self);

            reviewer.SendReply("architect", "", "short");
            reviewer.SendReply("architect", "", "a somewhat longer second message than the first");

            var c = reviewer.Correspondences.Single();
            Assert.Equal(2, c.Depth);
            var expectedVolume = SPLA.MCP.Core.Composition.TokenEstimate.Of("short")
                + SPLA.MCP.Core.Composition.TokenEstimate.Of("a somewhat longer second message than the first");
            Assert.Equal(expectedVolume, c.VolumeEstimate);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void The_graph_is_assembled_from_disk_with_no_runtime_open()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer", "reviewer");
            var architect = chats.CreateNew("Architect", "architect");

            reviewer.OpenCorrespondence("architect", "design review", architect.ChatId, CorrespondenceInitiator.Self);
            architect.OpenCorrespondence("reviewer", "design review", reviewer.ChatId, CorrespondenceInitiator.Correspondent);
            reviewer.SendReply("architect", "design review", "please look at this");
            architect.SendReply("reviewer", "design review", "looks fine");
            reviewer.Save();
            architect.Save();

            // A brand-new ChatManager over the same workspace — nothing in this test holds a
            // ChatRegistry or a ChatRuntime, only files on disk (decision 3 of wave 7б).
            var manager = FreshManager(root);
            var edges = CorrespondenceGraph.Build(manager);

            var edge = Assert.Single(edges);
            Assert.Equal("reviewer", edge.FromRole);
            Assert.Equal("architect", edge.ToRole);
            Assert.Equal(1, edge.RepliesFromInitiator);
            Assert.Equal(1, edge.RepliesFromCorrespondent);
            Assert.True(edge.VolumeFromInitiator > 0);
            Assert.True(edge.VolumeFromCorrespondent > 0);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void An_edge_points_from_the_initiator_not_from_whoever_is_asked_first()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var writer = chats.CreateNew("Writer", "writer");
            var architect = chats.CreateNew("Architect", "architect");

            // The architect opens the correspondence — writer is Initiator.Correspondent on its own
            // side, exactly mirroring ChatRuntime.Correspond's own wiring.
            architect.OpenCorrespondence("writer", "", writer.ChatId, CorrespondenceInitiator.Self);
            writer.OpenCorrespondence("architect", "", architect.ChatId, CorrespondenceInitiator.Correspondent);
            architect.SendReply("writer", "", "please draft the section");
            architect.Save();
            writer.Save();

            var edges = CorrespondenceGraph.Build(FreshManager(root));

            var edge = Assert.Single(edges);
            Assert.Equal("architect", edge.FromRole); // the initiator, not "writer" even though writer's
                                                        // session sorts first alphabetically
            Assert.Equal("writer", edge.ToRole);
            Assert.Equal(1, edge.RepliesFromInitiator);
            Assert.Equal(0, edge.RepliesFromCorrespondent);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_session_file_predating_the_correspondences_block_still_loads()
    {
        var root = TempRoot();
        try
        {
            var chats = FreshManager(root);
            var chatsDir = Directory.CreateDirectory(Path.Combine(root, ".spla", "chats")).FullName;
            const string id = "legacy-session";
            File.WriteAllText(Path.Combine(chatsDir, id + ".yaml"), """
                version: 1
                id: legacy-session
                title: Old Chat
                created_at: 2026-01-01T00:00:00Z
                updated_at: 2026-01-01T00:00:00Z
                workspace: ""
                messages: []
                """);

            var reloaded = chats.LoadChat(id);

            Assert.NotNull(reloaded);
            Assert.Equal("Old Chat", reloaded!.Title);
            Assert.Null(reloaded.Correspondences);

            // And the graph builder tolerates it same as any other session with nothing to say.
            var edges = CorrespondenceGraph.Build(chats);
            Assert.Empty(edges);
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
