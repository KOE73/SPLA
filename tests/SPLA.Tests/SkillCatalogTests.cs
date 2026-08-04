using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Library;
using SPLA.Library.Catalog;
using SPLA.Library.Format;
using SPLA.Library.Sources;
using SPLA.Agent.Composition;
using SPLA.MCP.Core.Composition;
using SPLA.Tests.Fakes;

namespace SPLA.Tests;

/// <summary>
/// The catalog: tags, the vocabulary they form, and how much of the holdings the model is told about.
///
/// <para>The thing under test is a price, not a feature. A skill may exist without the model knowing,
/// and the cost of the catalog must stop tracking the size of the fond — everything here is an
/// assertion about one of those two.</para>
/// </summary>
public class SkillCatalogTests
{
    // ── Tag normalisation: one subject, one word ─────────────────────────────

    [Theory]
    [InlineData("SSH", "ssh")]
    [InlineData("SSH Access", "ssh-access")]
    [InlineData("ssh_access", "ssh-access")]
    [InlineData("  SSH---Access  ", "ssh-access")]
    [InlineData("linux/host", "linux-host")]
    [InlineData("1C", "1c")]
    public void Tags_normalise_to_one_spelling(string raw, string expected)
    {
        Assert.Equal(expected, SkillTag.Normalize(raw));
    }

    /// <summary>A tag that normalises to nothing is dropped rather than becoming an empty string —
    /// an empty tag would match every query.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")]
    [InlineData("---")]
    [InlineData(null)]
    public void Tags_that_normalise_to_nothing_are_dropped(string? raw)
    {
        Assert.Null(SkillTag.Normalize(raw));
    }

    [Fact]
    public void Tag_list_deduplicates_and_keeps_the_authors_order()
    {
        Assert.Equal(["ssh", "linux"], SkillTag.NormalizeAll(["SSH", "linux", "ssh_", "  ssh  "]));
    }

    [Fact]
    public void Frontmatter_normalises_tags_on_the_way_in()
    {
        var entry = SkillFrontmatter.Parse(
            "---\nid: x\ntags: [SSH, \"Linux Host\", ssh]\n---\nBody.", "fallback", "x.md");

        Assert.Equal(["ssh", "linux-host"], entry.Tags);
    }

    [Fact]
    public void A_skill_without_tags_has_none_and_that_is_not_an_error()
    {
        Assert.Empty(SkillFrontmatter.Parse("---\nid: x\n---\nBody.", "fallback", "x.md").Tags);
    }

    // ── Vocabulary ───────────────────────────────────────────────────────────

    [Fact]
    public void Vocabulary_counts_every_tag_across_the_cards()
    {
        var vocabulary = TagVocabulary.From(Library(
            ("a", ["ssh", "linux"]), ("b", ["ssh"]), ("c", ["windows"])).Catalog());

        Assert.Equal(3, vocabulary.Count);
        Assert.Equal(2, vocabulary.CountOf("ssh"));
        Assert.True(vocabulary.Knows("SSH"));            // query normalises like storage
        Assert.False(vocabulary.Knows("nonexistent"));
    }

    /// <summary>Commonest first: the big subjects are the ones worth asking about. Alphabetical within
    /// a tie so the prompt does not churn between rebuilds.</summary>
    [Fact]
    public void Vocabulary_orders_by_frequency_then_alphabetically()
    {
        var vocabulary = TagVocabulary.From(Library(
            ("a", ["ssh", "zebra"]), ("b", ["ssh", "apple"]), ("c", ["ssh"])).Catalog());

        Assert.Equal(["ssh", "apple", "zebra"], vocabulary.Tags());
    }

    // ── Levels decide who is told ────────────────────────────────────────────

    [Fact]
    public void Out_of_catalog_and_findable_reach_the_model_not_at_all()
    {
        var view = CatalogView.Build(Library(
            (SourceLevel.OutOfCatalog, "hidden", ["secret"]),
            (SourceLevel.Findable, "findable", ["secret"])).Catalog());

        Assert.True(view.IsEmpty);
    }

    [Fact]
    public void In_catalog_contributes_tags_but_not_descriptions()
    {
        var view = CatalogView.Build(Library(
            (SourceLevel.InCatalog, "a", ["ssh"])).Catalog());

        Assert.Empty(view.Shelf);
        Assert.Equal(["ssh"], view.Cloud.Tags());
    }

    [Fact]
    public void On_shelf_is_listed_in_full()
    {
        var view = CatalogView.Build(Library((SourceLevel.OnShelf, "a", ["ssh"])).Catalog());

        Assert.Equal("a", Assert.Single(view.Shelf).Id);
        Assert.True(view.Cloud.IsEmpty);
    }

    /// <summary>A source that says nothing about its level keeps behaving the way sources always
    /// have — a handful of local skills listed in full.</summary>
    [Fact]
    public void Level_defaults_to_on_shelf()
    {
        Assert.Equal(SourceLevel.OnShelf, new FakeSkillSource().Level);
        Assert.Equal(SourceLevel.OnShelf, SkillSourceRegistryProbe.Level(null));
        Assert.Equal(SourceLevel.OnShelf, SkillSourceRegistryProbe.Level("nonsense"));
        Assert.Equal(SourceLevel.InCatalog, SkillSourceRegistryProbe.Level("in-catalog"));
        Assert.Equal(SourceLevel.OutOfCatalog, SkillSourceRegistryProbe.Level("out_of_catalog"));
    }

    // ── The shelf collapses under its own weight ─────────────────────────────

    [Fact]
    public void Shelf_below_the_limit_stays_listed()
    {
        var view = CatalogView.Build(ManySkills(10, tagged: true), shelfLimit: 25);

        Assert.False(view.Collapsed);
        Assert.Equal(10, view.Shelf.Count);
    }

    [Fact]
    public void Shelf_above_the_limit_collapses_into_the_cloud()
    {
        var view = CatalogView.Build(ManySkills(100, tagged: true), shelfLimit: 25);

        Assert.True(view.Collapsed);
        Assert.Empty(view.Shelf);
        Assert.Equal(100, view.CloudedSkills.Count);
    }

    /// <summary>An untagged skill cannot be found by subject, so demoting it would not summarise it —
    /// it would delete it. It keeps its place and keeps costing what it costs, which is the visible
    /// price of not having tagged the fond.</summary>
    [Fact]
    public void Untagged_skills_are_never_demoted()
    {
        var cards = ManySkills(60, tagged: true).Concat(ManySkills(5, tagged: false, prefix: "bare")).ToList();

        var view = CatalogView.Build(cards, shelfLimit: 25);

        Assert.True(view.Collapsed);
        Assert.Equal(5, view.Shelf.Count);
        Assert.All(view.Shelf, c => Assert.StartsWith("bare", c.Id));
    }

    [Fact]
    public void A_shelf_of_only_untagged_skills_does_not_collapse_at_all()
    {
        var view = CatalogView.Build(ManySkills(100, tagged: false), shelfLimit: 25);

        Assert.False(view.Collapsed);
        Assert.Equal(100, view.Shelf.Count);
    }

    // ── The stage's own check ────────────────────────────────────────────────

    /// <summary>
    /// A hundred skills must cost the prompt less than 200 tokens of catalog. Measured on the rendered
    /// section minus its fixed preamble, at the ~4 chars/token rule of thumb — the point is the order
    /// of magnitude, not the exact figure: 100 descriptions is 12-15k, and this has to be nothing like
    /// it.
    /// </summary>
    [Fact]
    public void A_hundred_skills_cost_under_two_hundred_tokens_of_catalog()
    {
        var source = new FakeSkillSource(level: SourceLevel.OnShelf);
        var subjects = new[] { "ssh", "linux", "windows", "network", "database", "backup", "docker", "1c" };
        for (var i = 0; i < 100; i++)
            source.With($"skill.{i:D3}",
                description: "A long description of the kind real skills carry, with trigger phrases " +
                             "so a model can match a request against it semantically rather than by name.",
                tags: [subjects[i % subjects.Length], subjects[(i * 3) % subjects.Length]]);

        var section = RenderSkillsSection(new SkillLibrary([source]));
        var variablePart = section[section.IndexOf("catalogued by subject", StringComparison.Ordinal)..];

        Assert.True(variablePart.Length / 4 < 200,
            $"catalog cost ~{variablePart.Length / 4} tokens:\n{variablePart}");
        // And the descriptions really are gone, not merely shortened.
        Assert.DoesNotContain("trigger phrases", section);
    }

    [Fact]
    public void The_cloud_prints_counts_so_the_model_can_judge_whether_asking_is_worth_a_turn()
    {
        var source = new FakeSkillSource(level: SourceLevel.InCatalog)
            .With("a", tags: ["ssh"]).With("b", tags: ["ssh"]).With("c", tags: ["windows"]);

        var section = RenderSkillsSection(new SkillLibrary([source]));

        Assert.Contains("ssh (2)", section);
        Assert.Contains("windows (1)", section);
        Assert.DoesNotContain("Available skills:", section);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static ResolvedSettings Settings() => new()
    {
        Mode = AgentMode.Edit,
        Instructions = [],
        CustomPrompt = null,
        Skills = new Dictionary<string, SplaSkillSection>()
    };

    /// <summary>The skills section as the model would receive it.</summary>
    private static string RenderSkillsSection(SkillLibrary library)
    {
        var composed = new AgentContextComposer([new SkillsContributor(library)])
            .Compose(Settings(), Directory.GetCurrentDirectory());
        return composed.SystemPrompt;
    }

    private static SkillLibrary Library(params (string Id, string[] Tags)[] skills)
    {
        var source = new FakeSkillSource();
        foreach (var (id, tags) in skills) source.With(id, tags: tags);
        return new SkillLibrary([source]);
    }

    private static SkillLibrary Library(params (SourceLevel Level, string Id, string[] Tags)[] skills)
    {
        var sources = skills
            .GroupBy(s => s.Level)
            .Select(g =>
            {
                var source = new FakeSkillSource($"src-{g.Key}", level: g.Key);
                foreach (var skill in g) source.With(skill.Id, tags: skill.Tags);
                return (ISkillSource)source;
            });

        return new SkillLibrary(sources);
    }

    private static List<SkillCard> ManySkills(int count, bool tagged, string prefix = "skill")
    {
        var source = new FakeSkillSource();
        for (var i = 0; i < count; i++)
            source.With($"{prefix}.{i:D3}", tags: tagged ? ["subject"] : null);

        return new SkillLibrary([source]).Catalog().ToList();
    }
}

/// <summary>Reaches the registry's internal level parser — the yaml spelling is part of the contract
/// with a person editing .spla, so it is worth a test even though the method is not public.</summary>
internal static class SkillSourceRegistryProbe
{
    public static SourceLevel Level(string? value)
    {
        var method = typeof(SkillSourceRegistry).GetMethod(
            "ParseLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        return (SourceLevel)method.Invoke(null, [value])!;
    }
}
