using SPLA.Domain.Formats;
using SPLA.Domain.Resources;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Composition;
using System.Linq;
using System.Text;

namespace SPLA.Agent.Composition;

/// <summary>
/// Announces the registered resource schemes (<c>file://</c>, <c>sftp://</c>, …) and, per scheme,
/// exactly which verbs it supports.
///
/// <para><b>The verb list is not optional prose.</b> A scheme that can read but not write is
/// unremarkable; a model that has to discover this by calling <c>resource_write</c> and getting a
/// refusal has been taught the same fact the hard way, at the cost of a turn and a bit of its own
/// trust in the tool. Rendering the matrix explicitly is what lets the contract be read instead of
/// probed — see the admission test on <see cref="ResourceVerb"/> for the same idea applied to which
/// verbs exist at all.</para>
///
/// <para>Off entirely, and cheaply so, in the overwhelmingly common case: the master switch
/// (<see cref="ResolvedSettings.UnifiedResources"/>) defaults to false, and even with it on a project
/// that registered no providers, or switched every one of them off, has nothing worth a paragraph of
/// system prompt.</para>
/// </summary>
public sealed class ResourceSchemesContributor : IAgentContributor
{
    public string Id => "resources";

    public AgentContribution Contribute(AgentContributionContext context)
    {
        if (!context.Settings.UnifiedResources) return AgentContribution.None;

        var registry = ResourceRegistry.For(context.Settings);
        var cards = registry.EnabledCards();
        if (cards.Count == 0) return AgentContribution.None;

        var body = new StringBuilder();
        body.Append(
            "--- Resource Addresses ---\n" +
            "Resources beyond ordinary project files are also reachable through scheme://path " +
            "addresses. Each scheme below supports only the verbs listed for it — calling one that is " +
            "not listed will be refused, so check the list rather than trying.");
        foreach (var card in cards)
            body.Append($"\n- {card.Scheme}:// — {card.Summary}; verbs: {string.Join(", ", card.Verbs.Select(VerbWord))}");

        // The conversions, in the same register as the scheme lines: a menu, not a paragraph. Same
        // argument too — a model that discovers "json can become yaml" by asking for it and being
        // refused has paid a turn for a fact that fits on one line. Rendered only when something is
        // actually registered, so a host with no projections spends no context saying so.
        var conversions = FormatConverterRegistry.For(context.Settings).Cards();
        if (conversions.Count > 0)
        {
            body.Append(
                "\nresource_read takes an optional 'as' (a target MIME type) which projects the content " +
                "before returning it. Registered projections:");
            foreach (var card in conversions)
                body.Append($"\n- {card.SourceType} -> {card.TargetType} — {card.Summary}");
        }

        return AgentContribution.FromContext(new ContextItem
        {
            Source = "resources",
            Title = "Resource addresses",
            Body = body.ToString(),
            Prefix = "\n\n"
        });
    }

    /// <summary>Lower-case wire word for a verb. Chosen to be the vocabulary a verb-carrying tool
    /// would use if one is ever exposed, so the prompt and that surface would agree without
    /// translation — no such tool exists yet, and until one does this naming is a promise being kept
    /// in advance rather than a description of something already on the wire.</summary>
    private static string VerbWord(ResourceVerb verb) => verb switch
    {
        ResourceVerb.Read => "read",
        ResourceVerb.Exists => "exists",
        ResourceVerb.List => "list",
        ResourceVerb.Write => "write",
        ResourceVerb.Delete => "delete",
        ResourceVerb.MakeDir => "mkdir",
        _ => verb.ToString().ToLowerInvariant()
    };
}
