using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Domain.Tools;
using SPLA.Runtime;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Archiving is a headstone, not a deletion (<c>docs/adr/ADR_20260904_core_history-vs-current.md</c>
/// §2.1). Before this, striking a correspondence removed it from the live dictionary, and the save path
/// rebuilds the persisted list wholesale from that dictionary — so the surviving side's own file lost
/// every trace that the two had ever corresponded, permanently, while the tool surface gained nothing
/// from the loss.
/// <para>
/// Each test below names the property that erasure broke. The last two are the ones that would have
/// caught the two bugs a naive "just keep the entry and flag it" fix introduces instead: a dead address
/// still offering its reply tool, and the liveness pass announcing the same death on every later turn.
/// </para>
/// </summary>
public sealed class CorrespondenceTombstoneTests
{
    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-tomb-{Guid.NewGuid():N}")).FullName;

    private static (AgentRuntime Runtime, ChatRegistry Chats, string Root) BuildProject()
    {
        var root = TempRoot();
        var manifest = Path.Combine(root, "test.spla");
        File.WriteAllText(manifest, """
            version: 1
            name: TombstoneTest
            workspace: .
            agent:
              mode: Edit
            """);
        var settings = ConfigLoader.LoadAndResolve(manifest);
        var runtime = new AgentRuntime(settings, NullLoggerFactory.Instance);
        var chats = new ChatRegistry(runtime);
        return (runtime, chats, root);
    }

    // ── The loss this ADR exists to stop ──────────────────────────────────────

    [Fact]
    public void An_ended_correspondence_survives_in_the_surviving_sides_own_file()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var architect = chats.CreateNew("Architect");
            var architectId = architect.ChatId;

            reviewer.OpenCorrespondence("architect", "api", architectId, CorrespondenceInitiator.Self);
            chats.Archive(architectId);
            reviewer.RefreshCorrespondences();
            reviewer.Save();

            // The point: the SURVIVOR's file still says this happened. Before the fix this list came
            // back null — the archived correspondent's own file held the only remaining half.
            var persisted = runtime.ChatManager.LoadChat(reviewer.ChatId);
            Assert.NotNull(persisted);
            var entry = Assert.Single(persisted!.Correspondences!);
            Assert.Equal("architect", entry.Role);
            Assert.Equal("api", entry.Topic);
            Assert.NotNull(entry.EndedAt);
            Assert.Equal("archived", entry.EndedReason);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_deleted_correspondent_is_told_apart_from_an_archived_one_in_the_record()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var writer = chats.CreateNew("Writer");
            var writerId = writer.ChatId;

            reviewer.OpenCorrespondence("writer", "draft", writerId, CorrespondenceInitiator.Self);
            chats.Delete(writerId);
            reviewer.RefreshCorrespondences();

            var tomb = Assert.Single(reviewer.EndedCorrespondences);
            Assert.Equal("deleted", tomb.EndedReason);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    // ── What the live surface must NOT gain from keeping the record ───────────

    [Fact]
    public void An_ended_correspondence_is_not_offered_as_a_reply_tool()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var architect = chats.CreateNew("Architect");
            var architectId = architect.ChatId;

            reviewer.OpenCorrespondence("architect", "api", architectId, CorrespondenceInitiator.Self);
            Assert.Single(reviewer.Correspondences);   // offered while open

            chats.Archive(architectId);
            reviewer.RefreshCorrespondences();

            // Keeping the record must not keep the address alive: ChatToolHost builds reply_* tools
            // from Correspondences, so a tombstone leaking in here would re-offer a dead address.
            Assert.Empty(reviewer.Correspondences);
            Assert.Single(reviewer.EndedCorrespondences);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void The_liveness_pass_announces_an_ending_once_not_on_every_later_turn()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var architect = chats.CreateNew("Architect");
            var architectId = architect.ChatId;

            reviewer.OpenCorrespondence("architect", "api", architectId, CorrespondenceInitiator.Self);
            chats.Archive(architectId);

            reviewer.RefreshCorrespondences();
            var first = reviewer.Inbox.DrainAllWithKinds();
            Assert.Single(first);
            Assert.Equal(InboxItemKind.Notice, first[0].Kind);

            // The pass runs once per turn. A tombstone left in the live dictionary would be located as
            // "archived" again and again, queueing the same notice for the rest of the chat's life.
            reviewer.RefreshCorrespondences();
            reviewer.RefreshCorrespondences();
            Assert.Empty(reviewer.Inbox.DrainAllWithKinds());
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    // ── Across a restart ──────────────────────────────────────────────────────

    [Fact]
    public void An_ended_correspondence_comes_back_as_a_tombstone_not_as_a_live_address()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var architect = chats.CreateNew("Architect");
            var architectId = architect.ChatId;

            reviewer.OpenCorrespondence("architect", "api", architectId, CorrespondenceInitiator.Self);
            chats.Archive(architectId);
            reviewer.RefreshCorrespondences();
            reviewer.Save();

            // A fresh ChatRuntime over the same persisted session — how reopening after a restart
            // actually happens, and the same harness the wave 7б graph tests use.
            var freshSession = runtime.ChatManager.LoadChat(reviewer.ChatId)!;
            var reopened = new ChatRuntime(runtime, freshSession, chats);
            Assert.Empty(reopened.Correspondences);
            var tomb = Assert.Single(reopened.EndedCorrespondences);
            Assert.Equal("archived", tomb.EndedReason);
            Assert.NotNull(tomb.EndedAt);

            // And it stays quiet: restoring it live would make the first turn after every restart
            // re-announce a death the chat was already told about.
            reopened.RefreshCorrespondences();
            Assert.Empty(reopened.Inbox.DrainAllWithKinds());
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void The_same_address_can_be_opened_again_after_it_ended_without_reviving_the_old_one()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var first = chats.CreateNew("Architect one");
            var firstId = first.ChatId;

            reviewer.OpenCorrespondence("architect", "api", firstId, CorrespondenceInitiator.Self);
            chats.Archive(firstId);
            reviewer.RefreshCorrespondences();

            var second = chats.CreateNew("Architect two");
            var reopened = reviewer.OpenCorrespondence(
                "architect", "api", second.ChatId, CorrespondenceInitiator.Self);

            // A fresh correspondence, not the tombstone handed back: same address, new chat, counters
            // starting from zero rather than continuing a dead exchange.
            Assert.True(reopened.IsOpen);
            Assert.Equal(second.ChatId, reopened.ChatId);
            Assert.Equal(0, reopened.Depth);
            Assert.Single(reviewer.Correspondences);
            Assert.Single(reviewer.EndedCorrespondences);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }
}
