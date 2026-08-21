using System.Collections.Generic;
using System.Linq;
using SPLA.Agent.Composition;
using SPLA.Domain.Host;
using SPLA.Domain.Resources;
using SPLA.Domain.Resources.Providers;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Composition;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// What the operator's switches actually buy, pinned as behaviour rather than as intent.
///
/// <para>The claim this file exists to defend is that the address space ships <b>inert</b>: with the
/// master switch off, an agent is byte-for-byte the agent it was before any of this was written. That
/// is the whole basis for shipping it at all — the usefulness of URI addressing depends on the model,
/// so the answer has to be obtainable by flipping a switch and measuring. A switch that leaks a
/// paragraph of system prompt while off would make the two arms of that comparison differ by more
/// than the thing being compared, and the measurement would be worthless.</para>
/// </summary>
public sealed class ResourceSchemesContributorTests
{
    private static ResolvedSettings SettingsWithFileScheme(bool unifiedResources)
    {
        var settings = new ResolvedSettings { UnifiedResources = unifiedResources };
        ResourceRegistry.For(settings).Register(new FileResourceProvider(() => new LocalWorkspace()));
        return settings;
    }

    private static AgentContribution Contribute(ResolvedSettings settings)
        => new ResourceSchemesContributor().Contribute(new AgentContributionContext(settings, "."));

    [Fact]
    public void Says_nothing_at_all_while_the_master_switch_is_off()
    {
        // Registered and perfectly usable — the point is that registration alone must not reach the
        // model. Providers exist for the host's sake before they exist for the model's.
        var contribution = Contribute(SettingsWithFileScheme(unifiedResources: false));

        Assert.Empty(contribution.Context);
    }

    [Fact]
    public void Announces_the_scheme_and_its_verbs_once_switched_on()
    {
        var contribution = Contribute(SettingsWithFileScheme(unifiedResources: true));

        var body = Assert.Single(contribution.Context).Body;
        Assert.Contains("file://", body);

        // The verb matrix is the part that must not be vague: the whole reason for printing it is so
        // support is read rather than discovered by triggering a refusal.
        foreach (var verb in new[] { "read", "exists", "list", "write", "delete", "mkdir" })
            Assert.Contains(verb, body);
    }

    /// <summary>
    /// A scheme switched off is not a scheme mentioned as unavailable — it is a scheme the model is
    /// never told about. Naming it would spend context describing something that cannot be used, and
    /// would invite the model to ask for it.
    /// </summary>
    [Fact]
    public void A_scheme_switched_off_is_not_mentioned_and_leaves_nothing_behind()
    {
        var settings = SettingsWithFileScheme(unifiedResources: true);
        ResourceRegistry.For(settings).SetEnabled("file", false);

        var contribution = Contribute(settings);

        Assert.Empty(contribution.Context);
    }

    /// <summary>
    /// The switches arrive as a map where absence means enabled. Getting that inversion wrong would
    /// silently disable every scheme nobody had an opinion about, so it is pinned rather than trusted.
    /// </summary>
    [Fact]
    public void Absence_from_the_switch_map_means_enabled()
    {
        var settings = SettingsWithFileScheme(unifiedResources: true);

        ResourceRegistry.For(settings).ApplySwitches(new Dictionary<string, bool> { ["sftp"] = false });

        var body = Assert.Single(Contribute(settings).Context).Body;
        Assert.Contains("file://", body);
    }
}
