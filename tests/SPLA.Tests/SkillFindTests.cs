using SPLA.Library;
using SPLA.Library.Librarians;
using SPLA.Library.Sources;
using SPLA.MCP.Core.Tools;
using SPLA.Tests.Fakes;

namespace SPLA.Tests;

/// <summary>
/// The tag librarian and <c>skill_find</c>.
///
/// <para>Two properties matter more than the ranking details. A librarian must never reveal a source
/// the level said not to reveal, and it must hand back annotations rather than procedures — otherwise
/// searching costs exactly what listing everything cost, and the whole design is pointless.</para>
/// </summary>
public class SkillFindTests
{
    private static SkillLibrary Fond()
    {
        var listed = new FakeSkillSource("listed", level: SourceLevel.OnShelf)
            .With("net.dns", description: "Diagnoses DNS resolution problems.", tags: ["network", "dns"]);

        var catalogued = new FakeSkillSource("catalogued", level: SourceLevel.InCatalog)
            .With("linux.capture", description: "Captures a Linux host over SSH into a package.",
                  tags: ["ssh", "linux", "backup"])
            .With("linux.restore", description: "Restores a captured Linux host.", tags: ["ssh", "linux"]);

        var findable = new FakeSkillSource("findable", level: SourceLevel.Findable)
            .With("win.audit", description: "Audits a Windows server.", tags: ["windows", "audit"]);

        var hidden = new FakeSkillSource("hidden", level: SourceLevel.OutOfCatalog)
            .With("secret.payroll", description: "Touches payroll.", tags: ["ssh", "payroll"]);

        return new SkillLibrary([listed, catalogued, findable, hidden]);
    }

    private static TagLibrarian Librarian() => new(Fond());

    private static string[] Ids(IReadOnlyList<SkillMatch> matches) =>
        matches.Select(m => m.Card.Id).ToArray();

    // ── What the librarian will and will not reveal ──────────────────────────

    /// <summary>The level's whole meaning. A source the model is not told about must not become
    /// discoverable by asking — otherwise "out of catalog" is a label, not a boundary.</summary>
    [Fact]
    public void Out_of_catalog_is_never_returned()
    {
        var matches = Librarian().Find(new SkillQuery(Tags: ["ssh"]), limit: 10);

        Assert.DoesNotContain("secret.payroll", Ids(matches));
        Assert.Equal(["linux.capture", "linux.restore"], Ids(matches).Order().ToArray());
    }

    [Fact]
    public void Out_of_catalog_tags_are_absent_from_the_vocabulary_too()
    {
        // Otherwise the model could infer the hidden source's subjects from the word list alone.
        Assert.False(Librarian().Vocabulary.Knows("payroll"));
        Assert.True(Librarian().Vocabulary.Knows("ssh"));
    }

    /// <summary>Findable exists precisely to be reachable here and nowhere else: absent from the
    /// prompt, present at the desk.</summary>
    [Fact]
    public void Findable_is_reachable_by_search_though_absent_from_the_prompt()
    {
        Assert.Equal(["win.audit"], Ids(Librarian().Find(new SkillQuery(Tags: ["windows"]))));
    }

    // ── Selection ────────────────────────────────────────────────────────────

    [Fact]
    public void Several_tags_narrow_rather_than_widen()
    {
        var matches = Librarian().Find(new SkillQuery(Tags: ["ssh", "backup"]), limit: 10);

        // Both carry ssh; only one carries backup, and it outranks the other.
        Assert.Equal("linux.capture", matches[0].Card.Id);
        Assert.Equal(["ssh", "backup"], matches[0].MatchedTags);
        Assert.True(matches[0].Score > matches[1].Score);
    }

    [Fact]
    public void Query_tags_normalise_like_stored_ones()
    {
        Assert.Equal(["win.audit"], Ids(Librarian().Find(new SkillQuery(Tags: ["Windows"]))));
    }

    [Fact]
    public void Free_text_reaches_what_nobody_tagged()
    {
        Assert.Equal(["net.dns"], Ids(Librarian().Find(new SkillQuery(Text: "resolution problems"))));
    }

    /// <summary>A tag match is a fact, a text match is a guess — the fact ranks first.</summary>
    [Fact]
    public void A_tag_match_outranks_a_text_match()
    {
        var matches = Librarian().Find(new SkillQuery(Tags: ["dns"], Text: "Restores"), limit: 10);

        Assert.Equal("net.dns", matches[0].Card.Id);
    }

    /// <summary>
    /// Found by measuring, not by reading: the text pass used <c>Contains</c>, so <c>our</c> matched
    /// inside <c>behaviour</c> and "our outgoing email" hit an SMTP skill for no reason.
    ///
    /// <para>A pass that always succeeds is worse than one that never does — it reports a false hit
    /// AND stops the model-backed librarian behind it from ever being reached.</para>
    /// </summary>
    [Fact]
    public void A_term_never_matches_inside_a_longer_word()
    {
        var source = new FakeSkillSource(level: SourceLevel.InCatalog)
            .With("mail.probe", description: "Checks an SMTP server: relay behaviour.");
        var librarian = new TagLibrarian(new SkillLibrary([source]));

        Assert.Empty(librarian.Find(new SkillQuery(Text: "our outgoing email")));
        // …but a real prefix still works, so plurals and stems are not lost.
        Assert.Single(librarian.Find(new SkillQuery(Text: "relays")));
    }

    /// <summary>The other half of the same defect: words long enough to survive a length rule that
    /// still carry no subject. "recipe for borscht" matched a DNS skill through <c>for</c>.</summary>
    [Fact]
    public void Stopwords_do_not_match_anything()
    {
        var source = new FakeSkillSource(level: SourceLevel.InCatalog)
            .With("net.dns", description: "Diagnoses DNS resolution failures for a domain.");
        var librarian = new TagLibrarian(new SkillLibrary([source]));

        Assert.Empty(librarian.Find(new SkillQuery(Text: "recipe for borscht")));
        Assert.Single(librarian.Find(new SkillQuery(Text: "domain resolution")));
    }

    [Fact]
    public void Short_words_do_not_flatten_the_ranking()
    {
        // "a", "on", "to" would otherwise match every description.
        Assert.Empty(Librarian().Find(new SkillQuery(Text: "a on to")));
    }

    [Fact]
    public void An_empty_query_returns_nothing_rather_than_everything()
    {
        Assert.Empty(Librarian().Find(new SkillQuery()));
        Assert.Empty(Librarian().Find(new SkillQuery(Tags: [], Text: "  ")));
    }

    [Fact]
    public void Unavailable_skills_never_surface()
    {
        var source = new FakeSkillSource(level: SourceLevel.InCatalog)
            .With("off", tags: ["ssh"], enabled: false)
            .With("on", tags: ["ssh"]);

        Assert.Equal(["on"], Ids(new TagLibrarian(new SkillLibrary([source])).Find(new SkillQuery(Tags: ["ssh"]))));
    }

    // ── The tool ─────────────────────────────────────────────────────────────

    private static async Task<string> Find(string args) => (await new SkillFindTool(Librarian()).ExecuteAsync(args)).TextContent;

    /// <summary>Cards, not bodies. Returning procedures would put back exactly what the two-step
    /// selection exists to keep out of the context.</summary>
    [Fact]
    public async Task Find_returns_descriptions_and_never_a_procedure()
    {
        var result = await Find("""{"tags":["ssh"]}""");

        Assert.Contains("linux.capture", result);
        Assert.Contains("Captures a Linux host over SSH", result);
        Assert.DoesNotContain("Step 1", result);       // the fake's body text
        Assert.Contains("skill_activate", result);      // and it says what to do next
    }

    /// <summary>"Nobody wrote a skill for this" and "that is not a word here" are different answers,
    /// and only the second is fixable by asking again.</summary>
    [Fact]
    public async Task An_unknown_subject_is_reported_as_unknown_with_the_real_vocabulary()
    {
        var result = await Find("""{"tags":["kubernetes"]}""");

        Assert.Contains("not subjects in this catalogue", result);
        Assert.Contains("kubernetes", result);
        Assert.Contains("ssh", result);
    }

    [Fact]
    public async Task A_known_subject_with_no_match_does_not_claim_the_word_is_wrong()
    {
        var result = await Find("""{"tags":["dns"],"text":"zzzz"}""");

        Assert.DoesNotContain("not subjects in this catalogue", result);
    }

    [Fact]
    public async Task An_empty_call_is_told_what_it_could_have_asked()
    {
        var result = await Find("{}");

        Assert.StartsWith("error:", result);
        Assert.Contains("subjects in this catalogue:", result);
    }

    [Fact]
    public async Task Malformed_arguments_do_not_throw()
    {
        Assert.Equal("error: invalid_json", await Find("not json"));
    }

    /// <summary>Read-effect, so asking the catalogue a question never prompts the user — activation
    /// stays the gate that does.</summary>
    /// <summary>Found live: a weak model guesses an id from a subject word. The refusal must not turn
    /// into an enumeration channel — suggesting ids the catalog withheld would make a wrong guess the
    /// cheapest way to list the fond.</summary>
    [Fact]
    public async Task A_wrong_guess_never_suggests_what_the_level_withheld()
    {
        var session = new SPLA.Domain.Agent.SkillSession();
        using var _ = SPLA.Domain.Agent.AgentSessionScope.Begin(
            new SPLA.Domain.Agent.AgentSession(
                new SPLA.Domain.Agent.KeyValueStore("session"),
                new SPLA.Domain.Agent.MarkManager(), session));

        var result = (await new SkillActivateTool(Fond()).ExecuteAsync("""{"id":"secret"}""")).TextContent;

        Assert.StartsWith("error:", result);
        Assert.DoesNotContain("payroll", result);
    }

    /// <summary>And when there is nothing to suggest, say what to do instead of leaving a dead end —
    /// the observed failure was thrashing on invented tool names after a refused guess.</summary>
    [Fact]
    public async Task A_wrong_guess_is_pointed_at_skill_find()
    {
        var session = new SPLA.Domain.Agent.SkillSession();
        using var _ = SPLA.Domain.Agent.AgentSessionScope.Begin(
            new SPLA.Domain.Agent.AgentSession(
                new SPLA.Domain.Agent.KeyValueStore("session"),
                new SPLA.Domain.Agent.MarkManager(), session));

        var result = (await new SkillActivateTool(Fond()).ExecuteAsync("""{"id":"zzzz"}""")).TextContent;

        Assert.Contains("skill_find", result);
    }

    [Fact]
    public void Find_is_a_read_and_does_not_ask()
    {
        var definition = new SkillFindTool(Librarian()).GetDefinition().Function!;

        Assert.Equal(SPLA.Domain.Models.ToolScope.Skill, definition.Scope);
        Assert.Equal(SPLA.Domain.Models.ToolEffect.Read, definition.Effect);
        Assert.Equal(SPLA.Domain.Models.PermissionResult.Allow,
            new SPLA.MCP.Core.Permissions.PermissionManager()
                .CheckPermission(SPLA.Domain.Models.AgentMode.Chat, definition, "{}"));
    }
}
