using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SPLA.Domain.Settings;

namespace SPLA.Domain.Formats;

/// <summary>One registered projection — the row a person and the model both see.</summary>
public sealed record ConversionCard(string SourceType, string TargetType, string Summary);

/// <summary>
/// The project's map of (source type → target type) → converter.
///
/// <para>One instance per running project, reached through <see cref="For"/> exactly the way
/// <c>ResourceRegistry</c> is: <see cref="ResolvedSettings.SharedServices"/> is the existing
/// rendezvous between independently-loaded parties, so a plugin loaded into its own
/// <c>AssemblyLoadContext</c> and the host register into the very same registry, without adding
/// anything to the plugin contract.</para>
///
/// <para><b>A lookup table, with no policy of its own.</b> It composes nothing, guesses nothing and
/// special-cases nothing — not even the identity projection <c>image/png → image/png</c>, which is a
/// registered converter like any other rather than a branch in here. The moment a registry starts
/// answering questions nobody registered an answer for, what it will do stops being readable off the
/// registration list.</para>
/// </summary>
public sealed class FormatConverterRegistry
{
    private const string ServiceKey = "core.formats";

    private readonly ConcurrentDictionary<(string Source, string Target), IFormatConverter> _converters = new();

    /// <summary>The one registry for this project's runtime — created by whichever side touches it first.</summary>
    public static FormatConverterRegistry For(ResolvedSettings settings)
        => (FormatConverterRegistry)settings.SharedServices.GetOrAdd(ServiceKey, _ => new FormatConverterRegistry());

    /// <summary>
    /// Takes a converter on, keyed on its (source, target) pair. Throws on a pair already claimed by a
    /// different implementation — a wiring mistake, discovered at startup by the person who made it,
    /// which is the one moment it is cheap to fix.
    /// </summary>
    public void Register(IFormatConverter converter)
    {
        ArgumentNullException.ThrowIfNull(converter);

        if (string.IsNullOrWhiteSpace(converter.SourceType) || string.IsNullOrWhiteSpace(converter.TargetType))
            throw new ArgumentException("A format converter must name both its source and its target type.", nameof(converter));

        var key = (Normalize(converter.SourceType), Normalize(converter.TargetType));

        if (_converters.TryAdd(key, converter))
            return;

        // Already taken — but by what? Two DIFFERENT converters on one pair is the mistake worth
        // shouting about: which one answers would then depend on load order. The same converter type
        // arriving twice is not that mistake, it is re-entry — a second runtime built over one
        // ResolvedSettings (tests do this routinely), or a plugin reload. Treating re-entry as a
        // conflict would turn a benign sequence into a startup crash, so only a genuine clash throws.
        var incumbent = _converters[key];
        if (incumbent.GetType() == converter.GetType())
            return;

        throw new InvalidOperationException(
            $"The conversion '{key.Item1}' -> '{key.Item2}' is already served by {incumbent.GetType().Name}, " +
            $"and {converter.GetType().Name} claims it too. Two converters for one pair would make which " +
            "one answers depend on load order.");
    }

    /// <summary>
    /// Finds the converter for one hop, or says why there is none.
    ///
    /// <para>The failure text LISTS the targets actually reachable from <paramref name="sourceType"/>,
    /// because the alternative — "no such conversion" — leaves a model to guess a second time. This is
    /// how the menu is learned at the point of need instead of by triggering errors.</para>
    /// </summary>
    public bool TryResolve(string sourceType, string targetType, out IFormatConverter converter, out string? error)
    {
        converter = null!;
        error = null;

        var source = Normalize(sourceType);
        var target = Normalize(targetType);

        var match = _converters
            .Where(kv => Matches(kv.Key.Source, source) && Matches(kv.Key.Target, target))
            // Exact registrations win over wildcard ones; otherwise a registered 'image/*' would
            // shadow the specific converter somebody wrote for 'image/png'.
            .OrderBy(kv => Specificity(kv.Key.Source) + Specificity(kv.Key.Target))
            .Select(kv => kv.Value)
            .FirstOrDefault();

        if (match is not null)
        {
            converter = match;
            return true;
        }

        var reachable = TargetsFor(source);
        error = reachable.Count == 0
            ? $"No conversion is registered from '{source}' to '{target}'. Nothing can be produced from '{source}' at all."
            : $"No conversion is registered from '{source}' to '{target}'. From '{source}' you can reach: {string.Join(", ", reachable)}.";
        return false;
    }

    /// <summary>What this source can become, wildcards included, in a stable order.</summary>
    public IReadOnlyList<string> TargetsFor(string sourceType)
    {
        var source = Normalize(sourceType);
        return _converters.Keys
            .Where(k => Matches(k.Source, source))
            .Select(k => k.Target)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(t => t, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>Every registered pair, for rendering into the system prompt.</summary>
    public IReadOnlyList<ConversionCard> Cards()
        => _converters
            .OrderBy(kv => kv.Key.Source, StringComparer.Ordinal)
            .ThenBy(kv => kv.Key.Target, StringComparer.Ordinal)
            .Select(kv => new ConversionCard(kv.Key.Source, kv.Key.Target, kv.Value.Summary))
            .ToList();

    /// <summary>
    /// A MIME type as this registry compares it: trimmed, lowercased, and with the parameters after the
    /// first ';' dropped — <c>text/plain; charset=utf-8</c> is the same type as <c>text/plain</c>, and a
    /// charset is not a different format. Kept in one place so registration and lookup can never drift.
    /// </summary>
    private static string Normalize(string? mime)
    {
        if (string.IsNullOrWhiteSpace(mime)) return string.Empty;
        var semicolon = mime.IndexOf(';');
        if (semicolon >= 0) mime = mime[..semicolon];
        return mime.Trim().ToLowerInvariant();
    }

    /// <summary>Whether a registered type answers for a requested one. Equal, or a registered family
    /// wildcard (<c>image/*</c>) over the same family.</summary>
    private static bool Matches(string registered, string requested)
    {
        if (string.Equals(registered, requested, StringComparison.Ordinal)) return true;
        if (!registered.EndsWith("/*", StringComparison.Ordinal)) return false;

        var family = registered[..^1]; // "image/"
        return requested.StartsWith(family, StringComparison.Ordinal);
    }

    private static int Specificity(string registered)
        => registered.EndsWith("/*", StringComparison.Ordinal) ? 1 : 0;
}
