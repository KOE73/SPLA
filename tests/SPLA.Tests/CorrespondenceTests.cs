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
/// Wave 4 (docs/plans/PLAN_20260902_agent_roles-and-correspondence.md §"Волна 4",
/// docs/adr/ADR_20260827-2_core_roles.md §2.2/§2.4): the correspondence mailbox and addressing
/// machinery. These drive a real <see cref="ChatRegistry"/> against a temp project — the same harness
/// <c>SpawnedSessionIntegrationTests</c> uses — because the soft-link liveness check
/// (<see cref="ChatRuntime.RefreshCorrespondences"/>) reads real files on disk (active/archived/missing).
/// </summary>
public sealed class CorrespondenceTests
{
    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-corr-{Guid.NewGuid():N}")).FullName;

    private static (AgentRuntime Runtime, ChatRegistry Chats, string Root) BuildProject()
    {
        var root = TempRoot();
        var manifest = Path.Combine(root, "test.spla");
        File.WriteAllText(manifest, """
            version: 1
            name: CorrespondenceTest
            workspace: .
            agent:
              mode: Edit
            """);
        var settings = ConfigLoader.LoadAndResolve(manifest);
        var runtime = new AgentRuntime(settings, NullLoggerFactory.Instance);
        var chats = new ChatRegistry(runtime);
        return (runtime, chats, root);
    }

    [Fact]
    public void Peer_kind_travels_through_the_inbox_like_any_other()
    {
        var inbox = new ChatInbox();
        var message = new ChatMessage { Role = ChatRole.User, Content = "reply from the architect" };

        inbox.Enqueue(message, InboxItemKind.Peer);
        var drained = inbox.DrainAllWithKinds();

        Assert.Single(drained);
        Assert.Equal(InboxItemKind.Peer, drained[0].Kind);
        Assert.Same(message, drained[0].Message);
    }

    [Fact]
    public void A_correspondence_keeps_its_initiator_and_the_set_is_not_a_single_link()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var architect = chats.CreateNew("Architect");
            var writer = chats.CreateNew("Writer");

            reviewer.OpenCorrespondence("architect", "design review", architect.ChatId, CorrespondenceInitiator.Self);
            reviewer.OpenCorrespondence("writer", "design review", writer.ChatId, CorrespondenceInitiator.Correspondent);

            var set = reviewer.Correspondences;
            Assert.Equal(2, set.Count);

            var withArchitect = set.Single(c => c.Role == "architect");
            Assert.Equal(CorrespondenceInitiator.Self, withArchitect.Initiator);
            Assert.Equal(architect.ChatId, withArchitect.ChatId);

            var withWriter = set.Single(c => c.Role == "writer");
            Assert.Equal(CorrespondenceInitiator.Correspondent, withWriter.Initiator);
            Assert.Equal(writer.ChatId, withWriter.ChatId);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Opening_the_same_address_twice_does_not_reset_an_already_running_exchange()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var architect = chats.CreateNew("Architect");

            var first = reviewer.OpenCorrespondence("architect", "", architect.ChatId, CorrespondenceInitiator.Self);
            first.Depth = 3;

            var second = reviewer.OpenCorrespondence("architect", "", architect.ChatId, CorrespondenceInitiator.Correspondent);

            Assert.Same(first, second);
            Assert.Equal(3, second.Depth);
            Assert.Equal(CorrespondenceInitiator.Self, second.Initiator); // unchanged by the second open
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_sleeping_correspondent_is_reached_not_struck()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");

            // Sleeping: on disk (written straight through ChatManager, the way an unopened chat any
            // client has never looked at also is), but not currently held open by the registry.
            var architect = runtime.ChatManager.CreateNewChat("Architect");
            Assert.Null(chats.Peek(architect.Id));

            reviewer.OpenCorrespondence("architect", "", architect.Id, CorrespondenceInitiator.Self);
            reviewer.RefreshCorrespondences();

            Assert.Single(reviewer.Correspondences); // not struck
            Assert.NotNull(chats.Peek(architect.Id)); // woken by the soft-link check
            Assert.Empty(reviewer.Inbox.DrainAllWithKinds()); // no notice for a merely sleeping correspondent
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_deleted_correspondent_is_struck_and_produces_a_notice()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var architect = chats.CreateNew("Architect");
            var architectId = architect.ChatId;
            chats.Delete(architectId);

            reviewer.OpenCorrespondence("architect", "", architectId, CorrespondenceInitiator.Self);
            reviewer.RefreshCorrespondences();

            Assert.Empty(reviewer.Correspondences);
            var drained = reviewer.Inbox.DrainAllWithKinds();
            Assert.Single(drained);
            Assert.Equal(InboxItemKind.Notice, drained[0].Kind);
            Assert.Contains("deleted", drained[0].Message.Content);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Archived_and_deleted_correspondents_produce_different_notice_text()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var architect = chats.CreateNew("Architect");
            var writer = chats.CreateNew("Writer");
            var architectId = architect.ChatId;
            var writerId = writer.ChatId;

            chats.Archive(architectId);
            chats.Delete(writerId);

            reviewer.OpenCorrespondence("architect", "", architectId, CorrespondenceInitiator.Self);
            reviewer.OpenCorrespondence("writer", "", writerId, CorrespondenceInitiator.Self);
            reviewer.RefreshCorrespondences();

            Assert.Empty(reviewer.Correspondences);
            var texts = reviewer.Inbox.DrainAllWithKinds()
                .Select(d => d.Message.Content!)
                .ToList();

            Assert.Equal(2, texts.Count);
            Assert.Contains(texts, t => t.Contains("architect") && t.Contains("archived"));
            Assert.Contains(texts, t => t.Contains("writer") && t.Contains("deleted"));
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void ChatManager_Locate_distinguishes_active_archived_and_missing()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var active = chats.CreateNew("Active");
            var archived = chats.CreateNew("Archived");
            chats.Archive(archived.ChatId);

            Assert.Equal(ChatLocation.Active, runtime.ChatManager.Locate(active.ChatId));
            Assert.Equal(ChatLocation.Archived, runtime.ChatManager.Locate(archived.ChatId));
            Assert.Equal(ChatLocation.Missing, runtime.ChatManager.Locate("no-such-chat"));
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void SendReply_delivers_as_an_ordinary_persistent_user_message()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var architect = chats.CreateNew("Architect");

            reviewer.OpenCorrespondence("architect", "", architect.ChatId, CorrespondenceInitiator.Self);
            var result = reviewer.SendReply("architect", "", "what do you think of this API?");

            Assert.True(result.Delivered);

            var drained = architect.Inbox.DrainAllWithKinds();
            Assert.Single(drained);
            var (message, kind) = drained[0];
            Assert.Equal(InboxItemKind.Peer, kind);
            Assert.Equal(ChatRole.User, message.Role);
            Assert.Equal(ContextRetention.Persistent, message.RetentionPolicy);
            Assert.Equal("what do you think of this API?", message.Content);

            var updated = reviewer.Correspondences.Single();
            Assert.Equal(1, updated.Depth);
            Assert.NotNull(updated.LastReplyAt);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void SendReply_to_a_deleted_correspondent_strikes_it_and_reports_gone()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var architect = chats.CreateNew("Architect");
            var architectId = architect.ChatId;
            chats.Delete(architectId);

            reviewer.OpenCorrespondence("architect", "", architectId, CorrespondenceInitiator.Self);
            var result = reviewer.SendReply("architect", "", "hello?");

            Assert.False(result.Delivered);
            Assert.Equal(ChatRuntime.ReplyOutcome.CorrespondentGone, result.Outcome);
            Assert.Empty(reviewer.Correspondences);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void SendReply_with_no_open_correspondence_is_refused()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var result = reviewer.SendReply("architect", "", "hello?");

            Assert.False(result.Delivered);
            Assert.Equal(ChatRuntime.ReplyOutcome.UnknownCorrespondence, result.Outcome);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }
}
