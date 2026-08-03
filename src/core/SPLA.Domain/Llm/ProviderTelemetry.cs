namespace SPLA.Domain.Llm;

/// <summary>
/// How a fact should be rendered and aggregated. Without this the UI cannot tell 20 requests from
/// $20 from 20 seconds, and would have to special-case each provider — exactly what the generic
/// layer exists to avoid.
/// </summary>
public enum ProviderFactKind
{
    /// <summary>A plain count (requests, tokens).</summary>
    Counter,
    /// <summary>An amount of money. The unit carries the currency.</summary>
    Money,
    /// <summary>A ratio already expressed as 0–100.</summary>
    Percent,
    /// <summary>A duration in seconds.</summary>
    Duration,
    /// <summary>A point in time, ISO-8601.</summary>
    Timestamp,
    /// <summary>Anything else — shown verbatim.</summary>
    Text
}

/// <summary>How loudly to show it. The provider decides, because only it knows that "3 credits left"
/// is bad while "3 requests used" is fine.</summary>
public enum ProviderFactSeverity { Normal, Warn, Critical }

/// <summary>
/// One provider-reported fact, in the shape every provider must produce even for counters this build
/// has never heard of.
/// <para>
/// This is the <i>middle</i> of three layers. Below it is the raw, untranslated record
/// (<see cref="LlmTurnResult.RawUsage"/>) which never loses anything; above it are the typed
/// canonical figures that can be compared across providers. A fact that does not map onto a canonical
/// concept lands here rather than being forced into the nearest one — a wrong mapping reads as
/// authoritative, while a missing canonical value correctly reads as "not known".
/// </para>
/// </summary>
public sealed class ProviderFact
{
    /// <summary>Stable identifier, for ordering and de-duplication. Provider-scoped.</summary>
    public required string Key { get; init; }

    /// <summary>What a human should see.</summary>
    public required string Label { get; init; }

    public required string Value { get; init; }

    /// <summary>Unit or currency ("USD", "tokens", "req/min"). Empty when meaningless.</summary>
    public string Unit { get; init; } = string.Empty;

    public ProviderFactKind Kind { get; init; } = ProviderFactKind.Text;

    public ProviderFactSeverity Severity { get; init; } = ProviderFactSeverity.Normal;

    /// <summary>
    /// When this was observed. Mandatory because most of these are <i>last seen</i> rather than
    /// <i>current</i> — a balance read three hours ago must not be presented as live.
    /// </summary>
    public DateTimeOffset ObservedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>When the window this belongs to resets, for anything windowed.</summary>
    public DateTimeOffset? ResetsAt { get; init; }
}

/// <summary>A titled group of facts. Sections exist so a UI can say which half belongs to the
/// connection (balance, key limits) and which to the model (price, context) — merged into one list
/// they would read as claims about the same thing.</summary>
public sealed class ProviderFactSection
{
    public required string Title { get; init; }
    public IReadOnlyList<ProviderFact> Facts { get; init; } = [];

    /// <summary>Where a human can see the full picture. Cheaper and more truthful than
    /// reimplementing a provider's dashboard.</summary>
    public string? DeepLink { get; init; }
}

/// <summary>
/// Optional capability for a provider that can report account-level figures — balance, key limits,
/// usage. Detected with a type check, exactly like <see cref="Interfaces.ITokenUsageReporter"/>: a
/// provider with nothing to report simply does not implement it, and no caller needs a null case per
/// provider.
/// <para>
/// Deliberately account-scoped, not turn-scoped: what this returns is a property of the credential,
/// so two connections sharing one provider still get two independent answers.
/// </para>
/// </summary>
public interface IProviderAccountInfo
{
    /// <summary>
    /// Reads what the provider will tell us about this credential.
    /// </summary>
    /// <param name="endpoint">The connection's endpoint.</param>
    /// <param name="apiKey">The inference credential, already materialized.</param>
    /// <param name="adminKey">The account-management credential, when configured. Null is normal:
    /// most figures come from the inference key, and only some providers need a second one.</param>
    Task<IReadOnlyList<ProviderFactSection>> GetAccountInfoAsync(
        string endpoint, string? apiKey, string? adminKey, CancellationToken ct = default);
}
