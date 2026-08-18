using System;
using System.Globalization;

namespace SPLA.Domain.Llm;

/// <summary>How much a turn was asked to think — the person's side of
/// <see cref="Models.ReasoningCapability"/>.</summary>
public enum ReasoningMode
{
    /// <summary>Say nothing; the model's own default wins.</summary>
    Default,

    /// <summary>No thinking at all.</summary>
    Off,

    /// <summary>Think, at whatever depth the model defaults to.</summary>
    On,

    /// <summary>Think at a named depth, in the provider's own vocabulary.</summary>
    Effort,

    /// <summary>Think within a token budget.</summary>
    Budget
}

/// <summary>
/// A reasoning selection, parsed from the single scalar that settings and chat YAML carry
/// (<c>reasoning_level</c>).
/// <para>
/// One string rather than three fields on purpose: the value is layered defaults → project → chat by
/// the same override machinery as temperature and top_p, and a scalar layers cleanly where a nested
/// object needs merge rules nobody wants to define. The grammar is small and open at the interesting
/// end — an unrecognized word is an effort word, so a provider that invents "xhigh" or "ultra"
/// tomorrow travels through without a code change.
/// </para>
/// <list type="bullet">
/// <item><c>""</c> / absent → <see cref="ReasoningMode.Default"/></item>
/// <item><c>off</c>, <c>none</c>, <c>false</c> → <see cref="ReasoningMode.Off"/></item>
/// <item><c>on</c>, <c>true</c>, <c>auto</c> → <see cref="ReasoningMode.On"/></item>
/// <item><c>budget:12000</c>, or a bare number → <see cref="ReasoningMode.Budget"/></item>
/// <item>anything else → <see cref="ReasoningMode.Effort"/>, carrying the word verbatim</item>
/// </list>
/// </summary>
public readonly record struct ReasoningChoice(ReasoningMode Mode, string? Effort, int? TokenBudget)
{
    public static readonly ReasoningChoice Default = new(ReasoningMode.Default, null, null);

    /// <summary>True when the turn should say nothing about reasoning.</summary>
    public bool IsDefault => Mode == ReasoningMode.Default;

    public static ReasoningChoice Parse(string? raw)
    {
        var s = raw?.Trim();
        if (string.IsNullOrEmpty(s)) return Default;

        var lower = s.ToLowerInvariant();
        switch (lower)
        {
            case "off": case "none": case "false": case "no": case "0":
                return new ReasoningChoice(ReasoningMode.Off, null, null);
            case "on": case "true": case "yes": case "auto": case "default":
                return new ReasoningChoice(ReasoningMode.On, null, null);
        }

        if (lower.StartsWith("budget:", StringComparison.Ordinal) &&
            int.TryParse(lower.AsSpan(7), NumberStyles.Integer, CultureInfo.InvariantCulture, out var budget) &&
            budget > 0)
            return new ReasoningChoice(ReasoningMode.Budget, null, budget);

        if (int.TryParse(lower, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bare) && bare > 0)
            return new ReasoningChoice(ReasoningMode.Budget, null, bare);

        // Not a word we know: it is a provider's effort vocabulary, kept verbatim.
        return new ReasoningChoice(ReasoningMode.Effort, s, null);
    }

    /// <summary>The scalar form, as persisted. Round-trips <see cref="Parse"/>.</summary>
    public override string ToString() => Mode switch
    {
        ReasoningMode.Off    => "off",
        ReasoningMode.On     => "on",
        ReasoningMode.Effort => Effort ?? "",
        ReasoningMode.Budget => TokenBudget is { } b ? $"budget:{b.ToString(CultureInfo.InvariantCulture)}" : "",
        _                    => ""
    };
}
