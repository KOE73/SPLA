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

    /// <param name="roles">Roles the manifest declares. Empty for the older tests here, which drive
    /// <c>OpenCorrespondence</c> straight and never go through the role check; the wave-4 addressing
    /// tests need declared roles because that check is exactly what tells a role name apart from a
    /// chat's public name.</param>
    private static (AgentRuntime Runtime, ChatRegistry Chats, string Root) BuildProject(params string[] roles)
    {
        var root = TempRoot();
        var manifest = Path.Combine(root, "test.spla");
        var rolesLine = roles.Length == 0 ? "" : $"roles: [{string.Join(", ", roles)}]";
        File.WriteAllText(manifest, $"""
            version: 1
            name: CorrespondenceTest
            workspace: .
            agent:
              mode: Edit
            {rolesLine}
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
    public void Non_ascii_purposes_that_normalise_to_the_same_spelling_still_get_distinct_tool_names()
    {
        // Reproduces PLAN_20260906 wave 0's headline bug in its post-wave-0 shape: the purpose text
        // never enters the address any more (§2.1/2.3), so three Cyrillic purposes cannot collide by
        // themselves — but three INSTANCES of the same role, opened via 'another', still must not
        // collide on a name via ReplyToolNaming.Normalize's "x"/transliteration fallback. Without
        // de-duplication in OpenCorrespondence, the second and third instance could end up sharing a
        // ToolName and ChatToolHost's FirstOrDefault(c => c.ToolName == name) would then silently
        // deliver every reply to whichever one it finds first.
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var first = chats.CreateNew("Architect one");
            var second = chats.CreateNew("Architect two");
            var third = chats.CreateNew("Architect three");

            var c1 = reviewer.OpenCorrespondence("architect", "чайник-носик", first.ChatId, CorrespondenceInitiator.Self);
            var c2 = reviewer.OpenCorrespondence("architect", "носик-чайника", second.ChatId, CorrespondenceInitiator.Self, another: true);
            var c3 = reviewer.OpenCorrespondence("architect", "давление-в-чайнике", third.ChatId, CorrespondenceInitiator.Self, another: true);

            var names = new[] { c1.ToolName, c2.ToolName, c3.ToolName };
            Assert.Equal(names.Length, names.Distinct().Count());
            Assert.Equal(new[] { 1, 2, 3 }, new[] { c1.InstanceNo, c2.InstanceNo, c3.InstanceNo });

            var byChat = new Dictionary<string, string> { [c1.ToolName] = first.ChatId, [c2.ToolName] = second.ChatId, [c3.ToolName] = third.ChatId };
            foreach (var (name, chatId) in byChat)
                Assert.Equal(chatId, reviewer.Correspondences.Single(c => c.ToolName == name).ChatId);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Without_another_opening_the_same_role_again_reuses_the_default_instance()
    {
        var (runtime, chats, root) = BuildProject();
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var architect1 = chats.CreateNew("Architect one");
            var architect2 = chats.CreateNew("Architect two");

            var first = reviewer.OpenCorrespondence("architect", "first purpose", architect1.ChatId, CorrespondenceInitiator.Self);
            var second = reviewer.OpenCorrespondence("architect", "second purpose", architect2.ChatId, CorrespondenceInitiator.Self);

            Assert.Same(first, second);
            Assert.Single(reviewer.Correspondences);
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
            var result = reviewer.SendReply("architect", 1, "what do you think of this API?");

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
            var result = reviewer.SendReply("architect", 1, "hello?");

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
            var result = reviewer.SendReply("architect", 1, "hello?");

            Assert.False(result.Delivered);
            Assert.Equal(ChatRuntime.ReplyOutcome.UnknownCorrespondence, result.Outcome);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    // ---- Wave 4: one address (docs/adr/ADR_20260906_core_one-address.md) -------------------------
    //
    // The number stops being "which of my correspondents this is" and becomes "which chat this is",
    // which is what makes a name worth saying to a third party at all.

    [Fact]
    public void Two_chats_call_the_same_architect_by_the_same_name()
    {
        var (runtime, chats, root) = BuildProject("architect");
        try
        {
            var first = chats.CreateNew("First reviewer");
            var second = chats.CreateNew("Second reviewer");

            // The first one asks for an architect by role and gets a fresh one, architect_1.
            Assert.True(first.Correspond("architect", "api", "look at this").Delivered);
            var architect = Assert.Single(first.Correspondences);
            Assert.Equal("reply_architect_1", architect.ToolName);

            // The second one asks for that same architect BY NAME. Before this ADR there was no way
            // to: naming the role would have minted a second architect, and the id was unspeakable.
            Assert.True(second.Correspond("architect_1", "api", "and by me too").Delivered);

            var seenBySecond = Assert.Single(second.Correspondences);
            Assert.Equal("reply_architect_1", seenBySecond.ToolName);
            Assert.Equal(architect.ChatId, seenBySecond.ChatId);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_public_name_opens_an_address_to_the_existing_chat_and_creates_none()
    {
        var (runtime, chats, root) = BuildProject("architect");
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var bystander = chats.CreateNew("Bystander");
            reviewer.Correspond("architect", "api", "hello");
            var before = runtime.ChatManager.ListChats().Count;

            Assert.True(bystander.Correspond("architect_1", "", "hello from over here").Delivered);

            Assert.Equal(before, runtime.ChatManager.ListChats().Count); // nobody was created
            // And the address exists on BOTH ends, or the edge would be invisible to the graph.
            var architect = chats.GetOrOpen(Assert.Single(bystander.Correspondences).ChatId)!;
            Assert.Contains(architect.Correspondences, c => c.ChatId == bystander.ChatId);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_role_name_still_creates_a_new_correspondent()
    {
        var (runtime, chats, root) = BuildProject("architect");
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            var bystander = chats.CreateNew("Bystander");
            reviewer.Correspond("architect", "api", "hello");

            // The role is a KIND, not a person: asking for one gives you your own, architect_2.
            bystander.Correspond("architect", "something else", "hello");

            Assert.Equal("reply_architect_2", Assert.Single(bystander.Correspondences).ToolName);
            Assert.NotEqual(
                Assert.Single(reviewer.Correspondences).ChatId,
                Assert.Single(bystander.Correspondences).ChatId);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_role_less_chat_is_numbered_only_when_it_becomes_a_correspondent()
    {
        var (runtime, chats, root) = BuildProject("architect");
        try
        {
            var human = chats.CreateNew("Ordinary human chat");
            // Not numbered at creation: most such chats never speak to anyone, and numbering them all
            // would spend the `agent` sequence before the first real correspondent showed up.
            Assert.Null(human.Session.AsInstance);

            human.Correspond("architect", "api", "hello");

            Assert.Equal(1, human.Session.AsInstance);
            // Minted on disk too, not only in memory — the architect just wrote the name down.
            Assert.Equal(1, runtime.ChatManager.LoadChat(human.ChatId)!.AsInstance);

            var architect = chats.GetOrOpen(Assert.Single(human.Correspondences).ChatId)!;
            Assert.Equal("reply_agent_1", Assert.Single(architect.Correspondences).ToolName);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void An_addressee_that_is_neither_a_role_nor_a_chat_is_refused_with_both_lists()
    {
        var (runtime, chats, root) = BuildProject("architect");
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            reviewer.Correspond("architect", "api", "hello");

            var result = reviewer.Correspond("architect_9", "", "anyone there?");

            Assert.False(result.Delivered);
            Assert.Equal(SPLA.Domain.Agent.CorrespondOutcome.UnknownRole, result.Outcome);
            Assert.Contains("architect", result.Message);    // the roles he may create from
            Assert.Contains("architect_1", result.Message);  // the chats he may reach
            Assert.Single(reviewer.Correspondences);         // and nothing was opened
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_role_whose_name_contains_an_underscore_is_matched_never_parsed()
    {
        // fool_ru + fool_ru_2 is the case that kills every "split on the last underscore" shortcut:
        // the demo project really does declare roles like fool_ru and comedian_en.
        var (runtime, chats, root) = BuildProject("fool_ru", "comedian_en");
        try
        {
            var host = chats.CreateNew("Host");
            host.Correspond("fool_ru", "first", "hello");
            host.Correspond("fool_ru", "second", "hello", another: true);

            var byInstance = host.Correspondences.ToDictionary(c => c.InstanceNo);
            Assert.Equal("reply_fool_ru_1", byInstance[1].ToolName);
            Assert.Equal("reply_fool_ru_2", byInstance[2].ToolName);

            // A third chat reaches the SECOND fool by name — not the first, and not a new one.
            var bystander = chats.CreateNew("Bystander");
            Assert.True(bystander.Correspond("fool_ru_2", "", "you specifically").Delivered);
            Assert.Equal(byInstance[2].ChatId, Assert.Single(bystander.Correspondences).ChatId);

            // And the role itself still reads as a role, giving a third fool rather than reaching one.
            bystander.Correspond("fool_ru", "", "any fool will do", another: true);
            Assert.Equal(2, bystander.Correspondences.Count);
            Assert.Contains(bystander.Correspondences, c => c.ToolName == "reply_fool_ru_3");
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }
    // ---- Wave 4: introduction (docs/adr/ADR_20260906_core_one-address.md §2.4/§2.5) --------------
    //
    // A third chat connects two others and stays off the edge. The point of the whole ADR: before
    // public names there was nothing a bystander could pronounce that meant "that one".

    [Fact]
    public void An_introduction_gives_both_parties_the_other_public_name_and_nothing_to_the_introducer()
    {
        var (runtime, chats, root) = BuildProject("architect", "reviewer");
        try
        {
            var moderator = chats.CreateNew("Moderator");
            var architect = chats.CreateNew("Architect", "architect");
            var reviewer = chats.CreateNew("Reviewer", "reviewer");
            architect.EnsureAsInstance();
            reviewer.EnsureAsInstance();

            var result = moderator.Introduce("architect_1", "reviewer_1", "the migration", "settle it between you");

            Assert.True(result.Introduced);
            // Each calls the other by the other's OWN public name — the same string in both tool lists.
            Assert.Equal("reply_reviewer_1", Assert.Single(architect.Correspondences).ToolName);
            Assert.Equal("reply_architect_1", Assert.Single(reviewer.Correspondences).ToolName);
            // And the introducer is on neither edge: he introduced them, he did not join them.
            Assert.Empty(moderator.Correspondences);
            Assert.DoesNotContain(architect.Correspondences, c => c.ChatId == moderator.ChatId);
            Assert.DoesNotContain(reviewer.Correspondences, c => c.ChatId == moderator.ChatId);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void The_introduction_reaches_the_first_party_so_somebody_actually_starts()
    {
        // An opened correspondence touches nobody's mailbox, and a turn is born from the mailbox
        // (ADR_20260825) — an introduction that delivered nothing would leave both parties standing.
        var (runtime, chats, root) = BuildProject("architect", "reviewer");
        try
        {
            var moderator = chats.CreateNew("Moderator");
            var architect = chats.CreateNew("Architect", "architect");
            var reviewer = chats.CreateNew("Reviewer", "reviewer");
            architect.EnsureAsInstance();
            reviewer.EnsureAsInstance();

            moderator.Introduce("architect_1", "reviewer_1", "", "the index question is yours to settle");

            var drained = architect.Inbox.DrainAllWithKinds();
            var (message, kind) = Assert.Single(drained);
            Assert.Equal(InboxItemKind.Peer, kind);
            Assert.Contains("the index question is yours to settle", message.Content);
            // Named out loud rather than passed off as the correspondent's own words: the text is the
            // introducer's, and it travels on the correspondent's half of the edge.
            Assert.Contains(moderator.PublicName(), message.Content);
            // The second party is deliberately left alone — the next thing it hears is the first
            // party's real reply, not a synthetic wake-up.
            Assert.Empty(reviewer.Inbox.DrainAllWithKinds());
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void An_introduced_edge_is_visible_in_the_graph_and_points_at_the_first_named()
    {
        // The test decision 2.5 was made for: a third Initiator value written on both halves would
        // match neither of BuildEdges' two probes and the edge would vanish silently.
        var (runtime, chats, root) = BuildProject("architect", "reviewer");
        try
        {
            var moderator = chats.CreateNew("Moderator");
            var architect = chats.CreateNew("Architect", "architect");
            var reviewer = chats.CreateNew("Reviewer", "reviewer");
            architect.EnsureAsInstance();
            reviewer.EnsureAsInstance();

            // reviewer_1 is named FIRST, so the edge runs reviewer -> architect regardless of who was
            // created first or how the files sort.
            Assert.True(moderator.Introduce("reviewer_1", "architect_1", "", "start us off").Introduced);
            architect.Save();
            reviewer.Save();

            var edge = Assert.Single(CorrespondenceGraph.Build(runtime.ChatManager));
            Assert.Equal("reviewer", edge.FromRole);
            Assert.Equal("architect", edge.ToRole);

            // Saved again and rebuilt: direction is decided once and persisted, never recomputed.
            architect.Save();
            reviewer.Save();
            var again = Assert.Single(CorrespondenceGraph.Build(runtime.ChatManager));
            Assert.Equal("reviewer", again.FromRole);
            Assert.Equal("architect", again.ToRole);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Both_halves_carry_the_introducer_public_name_and_it_survives_a_reload()
    {
        var (runtime, chats, root) = BuildProject("architect", "reviewer");
        try
        {
            var moderator = chats.CreateNew("Moderator");
            var architect = chats.CreateNew("Architect", "architect");
            var reviewer = chats.CreateNew("Reviewer", "reviewer");
            architect.EnsureAsInstance();
            reviewer.EnsureAsInstance();

            moderator.Introduce("architect_1", "reviewer_1", "", "start us off");
            var introducer = moderator.PublicName();

            // One introduction, one group key — this is what the "meeting" view (wave 7) reads.
            Assert.Equal(introducer, Assert.Single(architect.Correspondences).IntroducedBy);
            Assert.Equal(introducer, Assert.Single(reviewer.Correspondences).IntroducedBy);

            architect.Save();
            var stored = runtime.ChatManager.LoadChat(architect.ChatId)!;
            Assert.Equal(introducer, Assert.Single(stored.Correspondences!).IntroducedBy);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void An_ordinary_correspondence_carries_no_introducer()
    {
        var (runtime, chats, root) = BuildProject("architect");
        try
        {
            var reviewer = chats.CreateNew("Reviewer");
            reviewer.Correspond("architect", "api", "hello");
            Assert.Null(Assert.Single(reviewer.Correspondences).IntroducedBy);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_name_that_is_nobody_is_refused_with_the_list_of_chats()
    {
        var (runtime, chats, root) = BuildProject("architect");
        try
        {
            var moderator = chats.CreateNew("Moderator");
            var architect = chats.CreateNew("Architect", "architect");
            architect.EnsureAsInstance();

            var result = moderator.Introduce("architect_1", "reviewer_9", "", "hello");

            Assert.Equal(SPLA.Domain.Agent.IntroduceOutcome.UnknownAddressee, result.Outcome);
            Assert.Contains("architect_1", result.Message);   // who he could have meant
            Assert.Empty(architect.Correspondences);          // and nothing was opened halfway
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Introducing_a_chat_to_itself_is_refused()
    {
        var (runtime, chats, root) = BuildProject("architect");
        try
        {
            var moderator = chats.CreateNew("Moderator");
            var architect = chats.CreateNew("Architect", "architect");
            architect.EnsureAsInstance();

            var result = moderator.Introduce("architect_1", "architect_1", "", "hello");

            Assert.Equal(SPLA.Domain.Agent.IntroduceOutcome.SameChat, result.Outcome);
            Assert.Empty(architect.Correspondences);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Naming_yourself_as_a_party_is_refused_and_told_to_use_agent_correspond()
    {
        // Not a smaller introduction: you end up ON the edge, which is the whole subject of §2.4.
        var (runtime, chats, root) = BuildProject("architect");
        try
        {
            var moderator = chats.CreateNew("Moderator");
            var architect = chats.CreateNew("Architect", "architect");
            architect.EnsureAsInstance();
            var mine = moderator.PublicName();

            var result = moderator.Introduce(mine, "architect_1", "", "hello");

            Assert.Equal(SPLA.Domain.Agent.IntroduceOutcome.IntroducerIsParty, result.Outcome);
            Assert.Contains("agent_correspond", result.Message);
            Assert.Empty(moderator.Correspondences);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void An_archived_chat_cannot_be_introduced()
    {
        var (runtime, chats, root) = BuildProject("architect", "reviewer");
        try
        {
            var moderator = chats.CreateNew("Moderator");
            var architect = chats.CreateNew("Architect", "architect");
            var reviewer = chats.CreateNew("Reviewer", "reviewer");
            architect.EnsureAsInstance();
            reviewer.EnsureAsInstance();
            reviewer.Save();
            chats.Archive(reviewer.ChatId);

            var result = moderator.Introduce("architect_1", "reviewer_1", "", "hello");

            Assert.Equal(SPLA.Domain.Agent.IntroduceOutcome.AddresseeGone, result.Outcome);
            Assert.Contains("archived", result.Message);
            Assert.Empty(architect.Correspondences);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_spawned_run_has_no_public_name_and_cannot_be_introduced()
    {
        // Not a gap: a spawned run's only address is the link to its parent (EnsureAsInstance mints
        // nothing for it), so there is no name for anybody to say.
        var (runtime, chats, root) = BuildProject("architect");
        try
        {
            var moderator = chats.CreateNew("Moderator");
            var architect = chats.CreateNew("Architect", "architect");
            architect.EnsureAsInstance();
            runtime.ChatManager.CreateSpawnedChat(moderator.ChatId, "reviewer", skillId: null, mode: "");

            var result = moderator.Introduce("architect_1", "reviewer_1", "", "hello");

            Assert.Equal(SPLA.Domain.Agent.IntroduceOutcome.UnknownAddressee, result.Outcome);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Two_chats_that_already_correspond_are_not_introduced_a_second_time()
    {
        // Their edge is already theirs; a third party's words delivered onto it would appear inside a
        // conversation he was never part of, attributed to one of them.
        var (runtime, chats, root) = BuildProject("architect", "reviewer");
        try
        {
            var moderator = chats.CreateNew("Moderator");
            var reviewer = chats.CreateNew("Reviewer", "reviewer");
            reviewer.EnsureAsInstance();
            reviewer.Correspond("architect", "api", "hello");
            var architect = chats.GetOrOpen(Assert.Single(reviewer.Correspondences).ChatId)!;
            architect.Inbox.DrainAllWithKinds();

            var result = moderator.Introduce("reviewer_1", architect.PublicName(), "", "hello again");

            Assert.Equal(SPLA.Domain.Agent.IntroduceOutcome.AlreadyLinked, result.Outcome);
            Assert.Single(reviewer.Correspondences);
            Assert.Single(architect.Correspondences);
            Assert.Empty(architect.Inbox.DrainAllWithKinds());   // and nothing was said in their name
            Assert.Empty(reviewer.Inbox.DrainAllWithKinds());
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }
}
