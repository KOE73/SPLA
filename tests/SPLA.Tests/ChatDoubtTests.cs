using System;
using System.Linq;
using SPLA.Domain.Agent;
using SPLA.Domain.Security;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The flag is only worth having if it stays off during ordinary work. Half of these tests exist to
/// prove it does NOT rise — the failure that kills such a mechanism is not missing a case, it is
/// being on all the time and so telling nobody anything.
/// </summary>
public sealed class ChatDoubtTests
{
    private static IDisposable Scope(AgentSession session) => AgentSessionScope.Begin(session);

    private static AgentSession NewSession() =>
        new(new KeyValueStore("session"), new MarkManager(), new SkillSession());

    [Fact]
    public void The_open_web_raises_it()
    {
        var doubt = new ChatDoubt();

        doubt.Observe(DataOrigin.Internet, "https://example.com/page");

        Assert.True(doubt.IsRaised);
        Assert.Equal("https://example.com/page", Assert.Single(doubt.Causes).What);
    }

    /// <summary>
    /// The discriminator is "did the operator name this source", not "could this text contain an
    /// injected instruction". By the second test everything is suspect — a file on your own server,
    /// a string your own stored procedure returned — and the flag would be on within minutes of any
    /// real work.
    /// </summary>
    [Theory]
    [MemberData(nameof(NamedOrigins))]
    public void A_source_the_operator_named_does_not(DataOrigin origin)
    {
        var doubt = new ChatDoubt();

        doubt.Observe(origin, "whatever it was");

        Assert.False(doubt.IsRaised);
        Assert.Empty(doubt.Causes);
    }

    public static TheoryData<DataOrigin> NamedOrigins() =>
    [
        DataOrigin.Project,
        DataOrigin.Machine,
        DataOrigin.User,
        DataOrigin.Unrecorded,
        DataOrigin.Island(new IslandIdentity("ssh", "abcdef0123456789", "web-01")),
        DataOrigin.Island(new IslandIdentity("sql", "0123456789abcdef", "prod")),
        DataOrigin.Site("wiki.corp.local", listed: true)
    ];

    /// <summary>A domain on the operator's list stops being part of the open web — that is the whole
    /// mechanism by which the vast majority of a real project's fetching stops raising anything.</summary>
    [Fact]
    public void A_listed_domain_is_named_and_an_unlisted_one_is_not()
    {
        var doubt = new ChatDoubt();

        doubt.Observe(DataOrigin.Site("wiki.corp.local", listed: true), "https://wiki.corp.local/x");
        Assert.False(doubt.IsRaised);

        doubt.Observe(DataOrigin.Site("news.example.com", listed: false), "https://news.example.com/y");
        Assert.True(doubt.IsRaised);
    }

    [Fact]
    public void The_same_arrival_twice_is_one_cause()
    {
        var doubt = new ChatDoubt();

        doubt.Observe(DataOrigin.Internet, "https://example.com/page");
        doubt.Observe(DataOrigin.Internet, "https://example.com/page");

        Assert.Single(doubt.Causes);
    }

    /// <summary>Only a person lowers it, and only from the interface. There is deliberately no tool:
    /// a mark that can be removed by what it guards against guards nothing.</summary>
    [Fact]
    public void Only_an_explicit_clear_lowers_it()
    {
        var doubt = new ChatDoubt();
        doubt.Observe(DataOrigin.Internet, "https://example.com/page");

        doubt.Clear();

        Assert.False(doubt.IsRaised);
        Assert.Empty(doubt.Causes);
    }

    [Fact]
    public void Reopening_a_chat_does_not_launder_it()
    {
        var doubt = new ChatDoubt();
        doubt.Observe(DataOrigin.Internet, "https://example.com/page");
        var saved = doubt.Causes;

        var reopened = new ChatDoubt();
        reopened.Restore(saved);

        Assert.True(reopened.IsRaised);
        Assert.Equal("https://example.com/page", reopened.Causes.Single().What);
    }

    /// <summary>
    /// The laundry the project store would otherwise be: text pulled off the open web in one chat,
    /// read back in a fresh one, arriving with a clean flag and dirty contents. Blobs are per-chat
    /// and never had this problem; files still do and cannot be fixed, which is written into the
    /// non-goals.
    /// </summary>
    [Fact]
    public void The_project_store_carries_the_label_between_chats()
    {
        var project = new KeyValueStore("project");

        var first = NewSession();
        using (Scope(first))
        {
            first.Doubt.Observe(DataOrigin.Internet, "https://example.com/page");
            project.Set("note:finding", "…text from that page…", first.Doubt.Causes[^1].Origin);
        }

        var second = NewSession();
        using (Scope(second))
        {
            Assert.False(second.Doubt.IsRaised);

            var entry = project.Entries().Single(e => e.Key == "note:finding");
            second.Doubt.Observe(entry.Origin!, "memory:note:finding");

            Assert.True(second.Doubt.IsRaised);
        }
    }

    [Fact]
    public void Overwriting_an_entry_relabels_it_rather_than_merging()
    {
        var store = new KeyValueStore("project");

        store.Set("k", "from the web", DataOrigin.Internet);
        store.Set("k", "typed by a person", null);

        Assert.Null(store.Entries().Single().Origin);
    }

    [Fact]
    public void A_blob_carries_where_it_came_from()
    {
        var blobs = new BlobStore();

        var handle = blobs.Put(BlobPayload.OfText("page text"), "page", DataOrigin.Internet);

        Assert.Equal(DataOrigin.Internet, blobs.Describe(handle)!.Origin);
        Assert.Equal(DataOrigin.Internet, blobs.List().Single().Origin);
    }

    /// <summary>Vouching is per organisation, not per machine: naming <c>corp.local</c> names its
    /// wiki too, because that is the unit a person actually thinks in.</summary>
    [Theory]
    [InlineData("corp.local", "corp.local", true)]
    [InlineData("corp.local", "wiki.corp.local", true)]
    [InlineData("corp.local", "deep.wiki.corp.local", true)]
    [InlineData("corp.local", "corp.local.evil.com", false)]
    [InlineData("corp.local", "notcorp.local", false)]
    [InlineData("corp.local", "news.example.com", false)]
    public void Vouching_for_a_domain_covers_its_subdomains_and_nothing_else(
        string vouched, string host, bool expected)
    {
        var settings = new SPLA.Domain.Settings.ResolvedSettings { TrustedDomains = { vouched } };

        Assert.Equal(expected, settings.IsTrustedDomain(host));
    }

    [Fact]
    public void An_empty_list_vouches_for_nothing()
    {
        var settings = new SPLA.Domain.Settings.ResolvedSettings();

        Assert.False(settings.IsTrustedDomain("corp.local"));
        Assert.False(settings.IsTrustedDomain(""));
        Assert.False(settings.IsTrustedDomain(null));
    }

    [Fact]
    public void A_blob_with_no_recorded_origin_says_so_rather_than_guessing()
    {
        var blobs = new BlobStore();

        var handle = blobs.Put(BlobPayload.OfText("rows"), "rows");

        Assert.Null(blobs.Describe(handle)!.Origin);
    }
}
