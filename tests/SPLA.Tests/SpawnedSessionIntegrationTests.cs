using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Wave 2 (docs/plans/PLAN_20260902_agent_roles-and-correspondence.md §"Волна 2",
/// docs/adr/ADR_20260902_core_session-unification.md): a spawned run gets a real
/// <see cref="ChatRegistry"/>-backed session — same on-disk format as a human chat. These drive
/// <see cref="ChatRegistry"/>'s own <c>ISpawnSessionHost</c> surface directly against a real temp
/// project, which is the only way to prove trap 5 (retention never touches a live session) and trap 9
/// (recursion now writes real files) actually hold on disk, and that a session's write-lock signal
/// really does flip once its run finishes.
/// </summary>
public sealed class SpawnedSessionIntegrationTests
{
    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-spawn-{Guid.NewGuid():N}")).FullName;

    private static (AgentRuntime Runtime, ChatRegistry Chats, string Root) BuildProject()
    {
        var root = TempRoot();
        var manifest = Path.Combine(root, "test.spla");
        File.WriteAllText(manifest, """
            version: 1
            name: SpawnTest
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

    /// <summary>Requirement 1/2: a spawned run leaves a readable session on disk with the right
    /// origin/parent — the entire difference wave 0 declared and this wave fills in.</summary>
    [Fact]
    public void A_spawned_session_is_a_real_file_with_origin_and_parent()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var parent = chats.CreateNew("Parent chat");
            var session = chats.OpenSpawnedSession(parent.ChatId, role: null);
            session.Finish(
                new[]
                {
                    new ChatMessage { Role = ChatRole.User, Content = "hi" },
                    new ChatMessage { Role = ChatRole.Assistant, Content = "done" }
                },
                skillId: null, mode: "Edit", startedAt: DateTimeOffset.UtcNow,
                outcome: "completed", error: null);
            session.Dispose();

            var reloaded = runtime.ChatManager.LoadChat(session.ChatId);
            Assert.NotNull(reloaded);
            Assert.Equal("spawned", reloaded!.Origin);
            Assert.Equal(parent.ChatId, reloaded.Parent);
            Assert.Null(reloaded.As); // wave 3 wires a real role; null is correct here
            Assert.Equal("completed", reloaded.Spawn?.Outcome);
            Assert.Equal(2, reloaded.Messages.Count);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    /// <summary>Requirement 4: a batch of spawns must not pollute the human chat list.</summary>
    [Fact]
    public void Chat_list_excludes_spawned_sessions_but_ListSpawnedChats_sees_them()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var human = chats.CreateNew("Human chat");
            var spawned = Enumerable.Range(0, 3)
                .Select(_ => chats.OpenSpawnedSession(human.ChatId, null)).ToList();
            foreach (var s in spawned)
            {
                s.Finish(Array.Empty<ChatMessage>(), null, "Edit", DateTimeOffset.UtcNow, "completed", null);
                s.Dispose();
            }

            var visible = runtime.ChatManager.ListChats().Select(c => c.Id).ToList();
            Assert.Contains(human.ChatId, visible);
            foreach (var s in spawned) Assert.DoesNotContain(s.ChatId, visible);

            var spawnedIds = runtime.ChatManager.ListSpawnedChats().Select(c => c.Id).ToList();
            Assert.Equal(3, spawnedIds.Count);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    /// <summary>Requirement 5 / trap 5: the ring trims finished spawned sessions and never a live one.</summary>
    [Fact]
    public void Retention_trims_finished_sessions_but_never_a_live_one()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var finishedIds = new System.Collections.Generic.List<string>();
            for (var i = 0; i < 3; i++)
            {
                var s = chats.OpenSpawnedSession(null, null);
                // UpdatedAt is what ordering keys off — a real gap keeps the "most recent" comparison
                // meaningful even on filesystems/clocks with coarse resolution.
                Thread.Sleep(15);
                s.Finish(Array.Empty<ChatMessage>(), null, "Edit", DateTimeOffset.UtcNow, "completed", null);
                s.Dispose();
                finishedIds.Add(s.ChatId);
            }

            // A run "in progress" — never Finish()ed, so Spawn.Outcome stays null.
            var live = chats.OpenSpawnedSession(null, null);

            chats.TrimSpawnedRetention(keep: 1);

            var remaining = runtime.ChatManager.ListSpawnedChats().Select(c => c.Id).ToHashSet();
            Assert.Contains(live.ChatId, remaining);          // never touched
            Assert.Contains(finishedIds[^1], remaining);      // newest finished kept
            Assert.DoesNotContain(finishedIds[0], remaining); // oldest finished trimmed
            Assert.DoesNotContain(finishedIds[1], remaining);
            Assert.Equal(2, remaining.Count); // 1 finished kept + 1 live spared

            live.Dispose();
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Retention_zero_keeps_no_finished_session_but_still_spares_a_live_one()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var s = chats.OpenSpawnedSession(null, null);
            s.Finish(Array.Empty<ChatMessage>(), null, "Edit", DateTimeOffset.UtcNow, "completed", null);
            s.Dispose();
            var live = chats.OpenSpawnedSession(null, null);

            chats.TrimSpawnedRetention(keep: 0);

            var remaining = runtime.ChatManager.ListSpawnedChats().Select(c => c.Id).ToHashSet();
            Assert.DoesNotContain(s.ChatId, remaining);
            Assert.Contains(live.ChatId, remaining);

            live.Dispose();
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Negative_retention_never_trims_anything()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var ids = Enumerable.Range(0, 5).Select(_ =>
            {
                var s = chats.OpenSpawnedSession(null, null);
                s.Finish(Array.Empty<ChatMessage>(), null, "Edit", DateTimeOffset.UtcNow, "completed", null);
                s.Dispose();
                return s.ChatId;
            }).ToList();

            chats.TrimSpawnedRetention(keep: -1);

            var remaining = runtime.ChatManager.ListSpawnedChats().Select(c => c.Id).ToHashSet();
            foreach (var id in ids) Assert.Contains(id, remaining);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    /// <summary>
    /// Requirement 6 (ADR §2.2): the exact signal <c>ChatHandlers.Send</c> checks before accepting a
    /// message into a spawned chat flips from refused to accepted once the run finishes — including
    /// through a runtime that was already cached by an earlier <c>chat.open</c>/<c>chat.watch</c>
    /// (the stale-cache hazard <see cref="ChatRegistry.EvictCachedRuntime"/> exists to close).
    /// </summary>
    [Fact]
    public void Write_gate_signal_is_refused_mid_run_and_accepted_once_finished()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var session = chats.OpenSpawnedSession(null, null);

            // A client peeked at (opened/watched) the chat while the run was still going — exactly the
            // scenario that would go stale without the cache eviction on Finish.
            var openedMidRun = chats.GetOrOpen(session.ChatId)!;
            Assert.True(IsRefused(openedMidRun));

            session.Finish(Array.Empty<ChatMessage>(), null, "Edit", DateTimeOffset.UtcNow, "completed", null);
            session.Dispose();

            // The same call chat.send makes for every message — must now see the finished session,
            // not the stale one a mid-run peek cached.
            var afterFinish = chats.GetOrOpen(session.ChatId)!;
            Assert.False(IsRefused(afterFinish));
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }

        // The exact predicate ChatHandlers.Send uses.
        static bool IsRefused(ChatRuntime chat) =>
            chat.Session.Origin == "spawned" && chat.Session.Spawn?.Outcome is null;
    }

    /// <summary>A human chat is never refused by the spawned write-gate — only Origin == "spawned"
    /// with a run still in progress trips it.</summary>
    [Fact]
    public void A_human_chat_is_never_write_gated()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var human = chats.CreateNew("Human chat");
            Assert.False(human.Session.Origin == "spawned" && human.Session.Spawn?.Outcome is null);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    /// <summary>Trap 9, re-proven on disk: three levels of spawn-from-a-spawn create three real
    /// sessions, each parented at the one that made it.</summary>
    [Fact]
    public void Nested_spawned_sessions_chain_their_parent_ids_on_disk()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var top = chats.OpenSpawnedSession(null, null);
            var mid = chats.OpenSpawnedSession(top.ChatId, null);
            var leaf = chats.OpenSpawnedSession(mid.ChatId, null);

            foreach (var s in new[] { leaf, mid, top })
            {
                s.Finish(Array.Empty<ChatMessage>(), null, "Edit", DateTimeOffset.UtcNow, "completed", null);
                s.Dispose();
            }

            var topChat = runtime.ChatManager.LoadChat(top.ChatId)!;
            var midChat = runtime.ChatManager.LoadChat(mid.ChatId)!;
            var leafChat = runtime.ChatManager.LoadChat(leaf.ChatId)!;

            Assert.Null(topChat.Parent);
            Assert.Equal(top.ChatId, midChat.Parent);
            Assert.Equal(mid.ChatId, leafChat.Parent);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }
}
