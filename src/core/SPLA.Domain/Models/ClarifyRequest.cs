using System.Collections.Generic;

namespace SPLA.Domain.Models;

/// <summary>
/// A structured question the agent sends to the user via <see cref="SPLA.Domain.Tools.ClarifyScope"/>.
/// Contains a single question and an ordered list of options to choose from.
/// The host (UI or CLI) renders this however it likes; the agent receives the chosen option text.
/// </summary>
public sealed class ClarifyRequest
{
    /// <summary>The question text shown to the user.</summary>
    public required string Question { get; init; }

    /// <summary>Ordered list of selectable options.</summary>
    public required IReadOnlyList<ClarifyOption> Options { get; init; }
}

/// <summary>One selectable option in a <see cref="ClarifyRequest"/>.</summary>
public sealed class ClarifyOption
{
    /// <summary>Short label shown as the choice (e.g. "Yes", "No", skill name).</summary>
    public required string Label { get; init; }

    /// <summary>Optional description shown alongside the label.</summary>
    public string? Description { get; init; }
}
