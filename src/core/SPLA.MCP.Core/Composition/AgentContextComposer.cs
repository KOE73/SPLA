using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;

namespace SPLA.MCP.Core.Composition;

/// <summary>
/// The assembled surface: every item, the text they make, and the manifest saying where each came
/// from. One object so a caller cannot look at the prompt without being able to explain it.
/// </summary>
public sealed record ComposedContext(IReadOnlyList<ContextItem> Items, CompositionManifest Manifest)
{
    public static readonly ComposedContext Empty = new([], CompositionManifest.Empty);

    /// <summary>The single system message: every <see cref="ContextPlacement.SystemPrompt"/> item's
    /// <see cref="ContextItem.Text"/>, in composition order.</summary>
    public string SystemPrompt =>
        string.Concat(Items.Where(i => i.Placement == ContextPlacement.SystemPrompt).Select(i => i.Text));

    /// <summary>Items that travel as their own message, in composition order.</summary>
    public IReadOnlyList<ContextItem> TurnMessages =>
        Items.Where(i => i.Placement == ContextPlacement.TurnMessage).ToList();

    /// <summary>A surface someone else already rendered. For hosts that build their prompt outside
    /// the composer (a spawned sub-agent renders once, up front) and for tests.</summary>
    public static ComposedContext FromSystemPrompt(string text) =>
        new([new ContextItem { Source = "prerendered", Title = "System prompt", Body = text, Contributor = "host" }],
            new CompositionManifest([
                new ManifestEntry("host", "prerendered", "System prompt",
                    ContextPlacement.SystemPrompt, TokenEstimate.Of(text))]));
}

/// <summary>
/// Runs the contributors in order and folds their contributions into one surface. This is the whole
/// composition mechanism: it knows how to collect and attribute, and nothing about what any
/// particular contributor means — which is the difference between this and the builder it replaces,
/// where the list of sources was the code.
///
/// <para>Order is the order the contributors were supplied. It is load-bearing (the prompt reads
/// top-down as authority) and is therefore fixed by whoever composes the list, never by the
/// contributors themselves.</para>
/// </summary>
public sealed class AgentContextComposer
{
    private readonly IReadOnlyList<IAgentContributor> _contributors;
    private readonly ILogger? _logger;

    public AgentContextComposer(IEnumerable<IAgentContributor> contributors, ILogger? logger = null)
    {
        _contributors = contributors.ToList();
        _logger = logger;
    }

    public IReadOnlyList<IAgentContributor> Contributors => _contributors;

    public ComposedContext Compose(ResolvedSettings settings, string workingDirectory)
    {
        var context = new AgentContributionContext(settings, workingDirectory);
        var items = new List<ContextItem>();
        var entries = new List<ManifestEntry>();

        foreach (var contributor in _contributors)
        {
            AgentContribution contribution;
            try
            {
                contribution = contributor.Contribute(context);
            }
            catch (Exception ex)
            {
                // A broken contributor must not take the turn down with it — but it must not
                // disappear either: the manifest carries the reason the text is not there.
                _logger?.LogWarning(ex, "Agent contributor failed. Contributor={Contributor}", contributor.Id);
                entries.Add(new ManifestEntry(contributor.Id, "(error)", "(contribution failed)",
                    ContextPlacement.SystemPrompt, 0, ex.Message));
                continue;
            }

            foreach (var item in contribution.Context)
            {
                // The contributor id is stamped here rather than trusted from the item: attribution
                // that a contributor could spoof would not be worth reading.
                var stamped = item with { Contributor = contributor.Id };
                items.Add(stamped);
                entries.Add(new ManifestEntry(
                    contributor.Id, stamped.Source, stamped.Title, stamped.Placement, stamped.ApproxTokens));
            }
        }

        var manifest = new CompositionManifest(entries);

        // Per-composition, so Debug on purpose: this runs on every iteration of the agent loop. The
        // full table is one level below that — when you need it, you need all of it.
        _logger?.LogDebug("Agent surface composed. Contributions={Count} ApproxTokens={ApproxTokens}",
            entries.Count, manifest.ApproxTokens);
        if (_logger?.IsEnabled(LogLevel.Trace) == true)
            _logger.LogTrace("Agent surface manifest:\n{Manifest}", manifest.ToText());

        return new ComposedContext(items, manifest);
    }
}
