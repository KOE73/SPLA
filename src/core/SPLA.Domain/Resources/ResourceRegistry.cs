using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SPLA.Domain.Settings;

namespace SPLA.Domain.Resources;

/// <summary>What one scheme is and what may be done to it — the row a person and the model both see.</summary>
/// <param name="Verbs">Derived from the provider's implemented interfaces, never from a self-declared
/// flag, so it cannot claim what the code does not do.</param>
public sealed record SchemeCard(string Scheme, string Summary, IReadOnlyList<ResourceVerb> Verbs, bool Enabled);

/// <summary>
/// The project's map of URI scheme → storage.
///
/// <para>One instance per running project, reached through <see cref="For"/> the same way
/// <c>SshSessionHub</c> is: <see cref="ResolvedSettings.SharedServices"/> is the existing rendezvous
/// between independently-loaded parties, so a plugin loaded into its own <c>AssemblyLoadContext</c>
/// and the host register into the very same registry. That is what lets registration happen inside
/// <c>ISplaPlugin.Initialize</c> without adding anything to the plugin contract.</para>
///
/// <para><b>The registry refuses nothing on security grounds and classifies no edges.</b> It is a
/// lookup table. Which movement a call is, and whether it is allowed, stay with the permission
/// pipeline where every other tool's answer is decided — a second place that can say no is a second
/// place that can disagree.</para>
/// </summary>
public sealed class ResourceRegistry
{
    private const string ServiceKey = "core.resources";

    private readonly ConcurrentDictionary<string, IResourceProvider> _providers =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Schemes switched off by the operator. Absent means on — a scheme someone bothered to
    /// register is useful by default, and the switch exists to run the experiment, not to gate.</summary>
    private readonly ConcurrentDictionary<string, bool> _disabled = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The one registry for this project's runtime — created by whichever side touches it first.</summary>
    public static ResourceRegistry For(ResolvedSettings settings)
        => (ResourceRegistry)settings.SharedServices.GetOrAdd(ServiceKey, _ => new ResourceRegistry());

    /// <summary>
    /// Takes a provider on. Throws on a provider that cannot serve the base verbs or on a scheme
    /// already taken — both are wiring mistakes, discovered at startup by the person who made them,
    /// which is the one moment they are cheap to fix.
    /// </summary>
    public void Register(IResourceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        if (string.IsNullOrWhiteSpace(provider.Scheme))
            throw new ArgumentException("A resource provider must name its scheme.", nameof(provider));

        var scheme = provider.Scheme.Trim().ToLowerInvariant();

        if (_providers.TryAdd(scheme, provider))
            return;

        // Already taken — but by what? Two DIFFERENT providers claiming one scheme is the mistake
        // worth shouting about: which storage answers would then depend on load order. The same
        // provider type arriving twice is not that mistake, it is re-entry — a second AgentRuntime
        // built over one ResolvedSettings (tests do this routinely), or a plugin reload. Treating
        // re-entry as a conflict would turn a benign sequence into a startup crash, so only a
        // genuine clash throws.
        var incumbent = _providers[scheme];
        if (incumbent.GetType() == provider.GetType())
            return;

        throw new InvalidOperationException(
            $"Scheme '{scheme}' is already served by {incumbent.GetType().Name}, and " +
            $"{provider.GetType().Name} claims it too. Two providers for one scheme would make which " +
            "storage answers depend on load order.");
    }

    /// <summary>Every scheme, on and off alike, busiest question first: what can I address here.</summary>
    public IReadOnlyList<SchemeCard> Cards()
        => _providers.Values
            .OrderBy(p => p.Scheme, StringComparer.Ordinal)
            .Select(p => new SchemeCard(p.Scheme, p.Summary, VerbsOf(p), IsEnabled(p.Scheme)))
            .ToList();

    /// <summary>Only what is actually usable right now — what the model is told about.</summary>
    public IReadOnlyList<SchemeCard> EnabledCards()
        => Cards().Where(c => c.Enabled).ToList();

    /// <summary>
    /// Which verbs a provider really serves. The base three come from <see cref="IResourceProvider"/>
    /// itself; each extended verb is one line, which is the whole cost of adding one.
    /// </summary>
    public static IReadOnlyList<ResourceVerb> VerbsOf(IResourceProvider provider)
    {
        var verbs = new List<ResourceVerb> { ResourceVerb.Read, ResourceVerb.Exists, ResourceVerb.List };

        if (provider is IResourceWriter) verbs.Add(ResourceVerb.Write);
        if (provider is IResourceRemover) verbs.Add(ResourceVerb.Delete);
        if (provider is IResourceContainerMaker) verbs.Add(ResourceVerb.MakeDir);

        return verbs;
    }

    public bool IsEnabled(string scheme)
        => !_disabled.ContainsKey(scheme);

    /// <summary>Operator switch, per scheme. Off means the scheme resolves to a refusal and is not
    /// mentioned to the model — the point being to be able to measure with and without.</summary>
    public void SetEnabled(string scheme, bool enabled)
    {
        if (enabled) _disabled.TryRemove(scheme, out _);
        else _disabled[scheme] = true;
    }

    /// <summary>Applies the operator's whole set of per-scheme switches at once, replacing any
    /// previous ones. Anything not named is on.</summary>
    public void ApplySwitches(IEnumerable<KeyValuePair<string, bool>>? switches)
    {
        _disabled.Clear();
        if (switches is null) return;

        foreach (var (scheme, enabled) in switches)
            if (!enabled) _disabled[scheme] = true;
    }

    /// <summary>
    /// Finds who serves an address, or says why nobody does.
    ///
    /// <para>The failure text names the schemes that ARE served, because the alternative — "unknown
    /// scheme" — leaves a model to guess a second time, and guessing is what the addresses were
    /// supposed to end.</para>
    /// </summary>
    public bool TryResolve(
        string? address,
        out IResourceProvider provider,
        out ResourceUri uri,
        out string? error)
    {
        provider = null!;

        if (!ResourceUri.TryParse(address, out uri, out error))
            return false;

        if (!_providers.TryGetValue(uri.Scheme, out provider!))
        {
            error = $"No storage is registered for scheme '{uri.Scheme}'. Available: {Available()}.";
            return false;
        }

        if (!IsEnabled(uri.Scheme))
        {
            provider = null!;
            error = $"Scheme '{uri.Scheme}' is switched off in this project's settings. Available: {Available()}.";
            return false;
        }

        return true;
    }

    /// <summary>Checks a verb before the call rather than after — the matrix has to be answerable up
    /// front, or a scheme that cannot list teaches the model by failing.</summary>
    public static bool Supports(IResourceProvider provider, ResourceVerb verb)
        => VerbsOf(provider).Contains(verb);

    private string Available()
    {
        var live = _providers.Keys.Where(IsEnabled).OrderBy(s => s, StringComparer.Ordinal).ToList();
        return live.Count == 0 ? "(none)" : string.Join(", ", live.Select(s => s + "://"));
    }
}
