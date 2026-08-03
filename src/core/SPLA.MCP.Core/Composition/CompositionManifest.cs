using System.Text;

namespace SPLA.MCP.Core.Composition;

/// <summary>One line of the manifest: a single contribution, and who is answerable for it.</summary>
/// <param name="Problem">Null when the item is in the surface. Non-null when the contributor failed —
/// the entry is kept anyway, because "this text is missing and here is why" is the answer the
/// manifest exists to give, and a contributor that vanishes silently is exactly the failure mode.</param>
public sealed record ManifestEntry(
    string Contributor,
    string Source,
    string Title,
    ContextPlacement Placement,
    int ApproxTokens,
    string? Problem = null);

/// <summary>
/// The report of what the agent's current surface is made of — the side product that answers
/// "why is this text in the prompt?" without reading four classes.
///
/// <para>Token figures are estimates (<see cref="TokenEstimate"/>) and are here for attribution:
/// they say which contributor holds which share, not what may be sent. What may be sent is decided
/// by the provider — the model's real context window comes from its catalog and the real size of a
/// request comes back as <c>prompt_tokens</c>. Guessing that locally would only be a second, worse
/// answer to a question already answered upstream.</para>
/// </summary>
public sealed record CompositionManifest(IReadOnlyList<ManifestEntry> Entries)
{
    public static readonly CompositionManifest Empty = new([]);

    public int ApproxTokens => Entries.Sum(e => e.ApproxTokens);

    /// <summary>Per-contributor totals, in composition order — the view that shows who is expensive.</summary>
    public IReadOnlyList<ContributorTotal> ByContributor =>
        Entries.GroupBy(e => e.Contributor, StringComparer.Ordinal)
            .Select(g => new ContributorTotal(g.Key, g.Count(), g.Sum(e => e.ApproxTokens)))
            .ToList();

    /// <summary>Compact table for logs and the CLI.</summary>
    public string ToText()
    {
        var text = new StringBuilder();
        text.AppendLine($"Agent surface — {Entries.Count} contribution(s), ~{ApproxTokens} tokens (estimate)");
        foreach (var entry in Entries)
        {
            var placement = entry.Placement == ContextPlacement.SystemPrompt ? "prompt" : "turn";
            text.AppendLine(entry.Problem is null
                ? $"  {entry.Contributor,-16} {entry.Source,-24} {placement,-6} ~{entry.ApproxTokens,6} — {entry.Title}"
                : $"  {entry.Contributor,-16} {entry.Source,-24} {placement,-6} {"FAILED",7} — {entry.Problem}");
        }
        return text.ToString();
    }
}

public sealed record ContributorTotal(string Contributor, int Items, int ApproxTokens);
