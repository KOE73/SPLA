using Microsoft.Playwright;
using System.Collections.Generic;

namespace SPLA.Plugins.Browser;

/// <summary>
/// Maps the short <c>ref</c> ids that <see cref="BrowserSnapshotService"/> stamps onto elements
/// (e.g. <c>e3.12</c>) back to a live <see cref="ILocator"/>, and rejects refs from a stale
/// snapshot. A ref's own text encodes the snapshot generation it came from (<c>e&lt;gen&gt;.&lt;n&gt;</c>),
/// so staleness is a cheap integer comparison against the tab's current generation — no DOM probing
/// needed, and (unlike a bare counter) refs from different generations can never collide.
/// Generation is tracked per tab id (each tab snapshots independently).
/// </summary>
internal sealed class ElementRefRegistry
{
    private readonly object _lock = new();
    private readonly Dictionary<string, int> _generationByTab = new();

    /// <summary>Advances and returns the next snapshot generation for a tab. Call once per
    /// <c>browser_snapshot</c>; the returned value is embedded into every ref the snapshot mints.</summary>
    public int NextGeneration(string tabId)
    {
        lock (_lock)
        {
            var gen = _generationByTab.TryGetValue(tabId, out var g) ? g + 1 : 1;
            _generationByTab[tabId] = gen;
            return gen;
        }
    }

    public int? CurrentGeneration(string tabId)
    {
        lock (_lock) return _generationByTab.TryGetValue(tabId, out var g) ? g : null;
    }

    /// <summary>Resolves a ref like <c>e3.12</c> to a locator scoped to <paramref name="page"/>,
    /// after checking it belongs to the tab's current snapshot generation.</summary>
    public (ILocator? Locator, string? Error) Resolve(IPage page, string tabId, string refId)
    {
        if (!TryParseGeneration(refId, out var refGen))
            return (null, $"Error: '{refId}' is not a valid ref (expected a form like 'e3.12' from browser_snapshot).");

        var current = CurrentGeneration(tabId);
        if (current is null)
            return (null, "Error: no snapshot taken yet on this tab. Call browser_snapshot first.");
        if (refGen != current)
            return (null, $"Error: ref '{refId}' is from an older snapshot (gen {refGen}, current gen {current}). Call browser_snapshot again and retry with a fresh ref.");

        return (page.Locator($"[data-spla-ref='{refId}']"), null);
    }

    private static bool TryParseGeneration(string refId, out int generation)
    {
        generation = 0;
        if (string.IsNullOrEmpty(refId) || refId[0] != 'e') return false;
        var dot = refId.IndexOf('.');
        if (dot <= 1) return false;
        return int.TryParse(refId.AsSpan(1, dot - 1), out generation);
    }
}
