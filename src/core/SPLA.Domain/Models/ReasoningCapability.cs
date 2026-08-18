using System;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.Domain.Models;

/// <summary>
/// What one model will let a caller do with its reasoning channel, as the provider describes it.
/// <para>
/// There is no cross-vendor standard here, and this type does not invent one. Three incompatible
/// shapes exist in the wild — a plain on/off switch (Qwen3-style <c>enable_thinking</c>), a graded
/// effort word (<c>reasoning_effort</c>), and a token budget (Anthropic <c>budget_tokens</c>, Gemini
/// <c>thinkingBudget</c>) — plus models that reason unconditionally and models that cannot reason at
/// all. A model may offer several at once: LM Studio reports Qwen3.8 as
/// <c>["off","low","medium","xhigh","on"]</c>, which is a switch and an effort scale in one list.
/// This type therefore carries all three axes side by side rather than a single "kind" enum, and the
/// UI shows whichever axes the model actually has.
/// </para>
/// <para>
/// <see cref="Efforts"/> holds the provider's own words in the provider's own order. They are never
/// normalized to a fixed enum: LM Studio's Qwen3.8 offers "xhigh" and no "high", OpenRouter's
/// Nemotron offers only "high" and "medium", and OpenAI adds "minimal". A closed enum would have to
/// drop, rename or invent values on every one of those.
/// </para>
/// </summary>
public sealed class ReasoningCapability
{
    /// <summary>Nothing was advertised. Distinct from "no reasoning": most OpenAI-compatible servers
    /// describe no capabilities at all, and silence is not a denial.</summary>
    public static readonly ReasoningCapability Unknown = new();

    /// <summary>A model the provider explicitly describes as having no reasoning channel.</summary>
    public static readonly ReasoningCapability None = new() { Known = true, Supported = false };

    /// <summary>True when a provider actually described this model. False = we simply have not been
    /// told, and the UI must offer the lever permissively rather than hide it.</summary>
    public bool Known { get; init; }

    /// <summary>Whether the model has a reasoning channel at all.</summary>
    public bool Supported { get; init; }

    /// <summary>The model always reasons and cannot be silenced (DeepSeek-R1, Ministral-reasoning,
    /// which LM Studio advertises as the single option "on").</summary>
    public bool Mandatory { get; init; }

    /// <summary>Whether the model reasons when nothing is asked of it.</summary>
    public bool DefaultEnabled { get; init; } = true;

    /// <summary>Graded effort words, in the provider's own vocabulary and order. Empty when the model
    /// is a plain switch.</summary>
    public IReadOnlyList<string> Efforts { get; init; } = Array.Empty<string>();

    /// <summary>The effort the model uses when none is given, when advertised.</summary>
    public string? DefaultEffort { get; init; }

    /// <summary>Whether reasoning depth can be given as a token budget instead of a word.</summary>
    public bool SupportsTokenBudget { get; init; }

    /// <summary>Bounds on that budget, when the provider states them.</summary>
    public int? MinTokenBudget { get; init; }

    /// <inheritdoc cref="MinTokenBudget"/>
    public int? MaxTokenBudget { get; init; }

    /// <summary>Whether "no thinking at all" is on offer.</summary>
    public bool CanDisable => Supported && !Mandatory;

    /// <summary>Whether there is any lever worth drawing — an off switch, a scale, or a budget.</summary>
    public bool HasLever => Supported && (CanDisable || Efforts.Count > 0 || SupportsTokenBudget);

    /// <summary>
    /// Reads LM Studio's shape: one flat <c>allowed_options</c> list plus a default, where "off" and
    /// "on" are membership words and everything else is an effort. The list is mixed for models that
    /// have both, which is why the two are separated here rather than classified as one or the other.
    /// </summary>
    public static ReasoningCapability FromOptions(IEnumerable<string>? allowed, string? @default)
    {
        var options = (allowed ?? Enumerable.Empty<string>())
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Select(o => o.Trim())
            .ToList();

        if (options.Count == 0) return Unknown;

        bool IsWord(string o, string w) => string.Equals(o, w, StringComparison.OrdinalIgnoreCase);
        var hasOff = options.Any(o => IsWord(o, "off") || IsWord(o, "none"));
        var efforts = options.Where(o => !IsWord(o, "off") && !IsWord(o, "none") && !IsWord(o, "on")).ToList();

        var def = string.IsNullOrWhiteSpace(@default) ? null : @default.Trim();

        return new ReasoningCapability
        {
            Known = true,
            Supported = true,
            Mandatory = !hasOff,
            DefaultEnabled = def == null || !(IsWord(def, "off") || IsWord(def, "none")),
            Efforts = efforts,
            DefaultEffort = def != null && efforts.Any(e => IsWord(e, def)) ? def : null
        };
    }
}
