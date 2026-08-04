using SPLA.Agent.Composition;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Library;
using SPLA.Library.Sources;
using SPLA.MCP.Core.Composition;
using SPLA.Tests.Fakes;

namespace SPLA.Tests;

/// <summary>
/// Handing a skill to a chat — the person choosing instead of the model.
///
/// <para>The property that matters is a price: a chat with a handed-out skill must carry no catalog
/// at all. That is what closes the "weak model, small context" case, and it is also why the level can
/// afford to hide things — whatever the model is not told, its owner can still hand over.</para>
/// </summary>
public class SkillHandoutTests
{
    private static ResolvedSettings Settings() => new()
    {
        Mode = AgentMode.Edit,
        Instructions = [],
        CustomPrompt = null,
        Skills = new Dictionary<string, SplaSkillSection>()
    };

    private static string Prompt(SkillLibrary library, ISkillSession session) =>
        new AgentContextComposer([new SkillsContributor(library, session)])
            .Compose(Settings(), Directory.GetCurrentDirectory()).SystemPrompt;

    /// <summary>The stage's own check. Not "the index is shorter" — absent.</summary>
    [Fact]
    public void A_chat_with_a_handed_out_skill_carries_no_catalog_at_all()
    {
        var shelf = new FakeSkillSource("shelf", level: SourceLevel.OnShelf)
            .With("listed.one", description: "UNIQUE-SHELF-MARKER");
        var cloud = new FakeSkillSource("cloud", level: SourceLevel.InCatalog)
            .With("clouded.one", tags: ["unique-cloud-marker"]);
        var library = new SkillLibrary([shelf, cloud]);

        var idle = new SkillSession();
        var before = Prompt(library, idle);
        Assert.Contains("UNIQUE-SHELF-MARKER", before);
        Assert.Contains("unique-cloud-marker", before);

        var handed = new SkillSession();
        handed.Activate("clouded.one", "THE PROCEDURE");
        var after = Prompt(library, handed);

        Assert.Contains("THE PROCEDURE", after);
        Assert.DoesNotContain("UNIQUE-SHELF-MARKER", after);   // no shelf
        Assert.DoesNotContain("unique-cloud-marker", after);   // and no tag cloud either
        Assert.DoesNotContain("skill_find", after);            // nor the instructions for using one
    }

    /// <summary>
    /// A skill is loaded or it is not — there is no third state that bypasses the catalog and lands
    /// in the prompt anyway.
    ///
    /// <para>That third state used to exist: <c>preloaded: true</c> put a body into the base prompt
    /// forever, outside the index, outside activation, outside the source level. It was
    /// <c>agent.instructions</c> wearing a skill's clothes, and it leaked past every rule the library
    /// has — including into a chat that already had a skill running, which is the case this pins.</para>
    /// </summary>
    [Fact]
    public void No_skill_can_reach_the_prompt_without_being_loaded()
    {
        var source = new FakeSkillSource()
            .With("other.one", body: "OTHER-BODY-MARKER", description: "not the one running");
        var library = new SkillLibrary([source]);

        var session = new SkillSession();
        session.Activate("running.one", "THE RUNNING PROCEDURE");
        var prompt = Prompt(library, session);

        Assert.Contains("THE RUNNING PROCEDURE", prompt);
        Assert.DoesNotContain("OTHER-BODY-MARKER", prompt);
    }

    /// <summary>Handing over is cheaper than being told: the same fond costs a fraction once a skill
    /// is running, and the remainder is the procedure the user asked for.</summary>
    [Fact]
    public void Handing_a_skill_over_removes_the_catalogs_cost_rather_than_adding_to_it()
    {
        var source = new FakeSkillSource(level: SourceLevel.OnShelf);
        for (var i = 0; i < 40; i++)
            source.With($"skill.{i:D2}",
                description: "A description long enough to matter when forty of them are printed.",
                tags: ["subject"]);

        var library = new SkillLibrary([source]);

        var idle = Prompt(library, new SkillSession());
        var handed = new SkillSession();
        handed.Activate("skill.00", "short body");

        Assert.True(Prompt(library, handed).Length < idle.Length);
    }
}
