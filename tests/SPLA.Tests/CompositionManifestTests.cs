using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Composition;

namespace SPLA.Tests;

/// <summary>
/// The composition mechanism itself: contributors are folded in order, every item is attributed to
/// the contributor that actually produced it, and a contributor that fails is reported rather than
/// silently missing.
/// </summary>
public sealed class CompositionManifestTests
{
    private static ResolvedSettings Settings() => new()
    {
        Mode = AgentMode.Edit,
        Instructions = [],
        CustomPrompt = null,
        Skills = new Dictionary<string, SplaSkillSection>()
    };

    private sealed class StubContributor(string id, params ContextItem[] items) : IAgentContributor
    {
        public string Id => id;
        public AgentContribution Contribute(AgentContributionContext context) => AgentContribution.FromContext(items);
    }

    private sealed class ThrowingContributor(string id, string message) : IAgentContributor
    {
        public string Id => id;
        public AgentContribution Contribute(AgentContributionContext context) => throw new InvalidOperationException(message);
    }

    private static ContextItem Item(string source, string body, string prefix = "", ContextPlacement placement = ContextPlacement.SystemPrompt)
        => new() { Source = source, Title = source, Body = body, Prefix = prefix, Placement = placement };

    [Fact]
    public void System_prompt_is_the_concatenation_of_its_items_in_order()
    {
        var composer = new AgentContextComposer([
            new StubContributor("first", Item("a", "AAA")),
            new StubContributor("second", Item("b", "BBB", prefix: "\n\n--- B ---\n"))
        ]);

        var composed = composer.Compose(Settings(), Directory.GetCurrentDirectory());

        Assert.Equal("AAA\n\n--- B ---\nBBB", composed.SystemPrompt);
        Assert.Equal(["a", "b"], composed.Items.Select(i => i.Source));
    }

    /// <summary>Turn-message items never reach the prompt, and prompt items never reach the turn
    /// message list — the placement is what addresses a contribution, not the order it arrived in.</summary>
    [Fact]
    public void Placement_decides_where_an_item_goes()
    {
        var composer = new AgentContextComposer([
            new StubContributor("prompt-side", Item("p", "IN PROMPT")),
            new StubContributor("turn-side", Item("t", "PER TURN", placement: ContextPlacement.TurnMessage))
        ]);

        var composed = composer.Compose(Settings(), Directory.GetCurrentDirectory());

        Assert.Equal("IN PROMPT", composed.SystemPrompt);
        var turn = Assert.Single(composed.TurnMessages);
        Assert.Equal("PER TURN", turn.Body);
    }

    /// <summary>Attribution a contributor could write itself would not be worth reading, so the
    /// composer stamps it — whatever the item claimed is overwritten.</summary>
    [Fact]
    public void Contributor_attribution_is_stamped_by_the_composer()
    {
        var lying = new ContextItem { Source = "s", Title = "t", Body = "b", Contributor = "someone-else" };
        var composer = new AgentContextComposer([new StubContributor("real", lying)]);

        var composed = composer.Compose(Settings(), Directory.GetCurrentDirectory());

        Assert.Equal("real", Assert.Single(composed.Items).Contributor);
        Assert.Equal("real", Assert.Single(composed.Manifest.Entries).Contributor);
    }

    [Fact]
    public void Manifest_reports_size_per_item_and_per_contributor()
    {
        var composer = new AgentContextComposer([
            new StubContributor("core", Item("core.memory", new string('x', 400)), Item("core.web", new string('y', 40))),
            new StubContributor("mode", Item("Edit", new string('z', 80)))
        ]);

        var manifest = composer.Compose(Settings(), Directory.GetCurrentDirectory()).Manifest;

        Assert.Equal(3, manifest.Entries.Count);
        Assert.Equal(130, manifest.ApproxTokens);          // (400 + 40 + 80) / 4
        var core = Assert.Single(manifest.ByContributor, c => c.Contributor == "core");
        Assert.Equal(2, core.Items);
        Assert.Equal(110, core.ApproxTokens);
    }

    /// <summary>A broken contributor must not take the turn down — and must not vanish either. The
    /// manifest is where "this text is missing, and here is why" gets answered.</summary>
    [Fact]
    public void A_failing_contributor_is_recorded_and_the_rest_still_compose()
    {
        var composer = new AgentContextComposer([
            new StubContributor("before", Item("a", "AAA")),
            new ThrowingContributor("broken", "disk on fire"),
            new StubContributor("after", Item("b", "BBB"))
        ]);

        var composed = composer.Compose(Settings(), Directory.GetCurrentDirectory());

        Assert.Equal("AAABBB", composed.SystemPrompt);
        var failure = Assert.Single(composed.Manifest.Entries, e => e.Problem is not null);
        Assert.Equal("broken", failure.Contributor);
        Assert.Contains("disk on fire", failure.Problem);
        Assert.Contains("broken", composed.Manifest.ToText());
    }
}
