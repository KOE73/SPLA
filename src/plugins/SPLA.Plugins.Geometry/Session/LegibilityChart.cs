using SPLA.Plugins.Geometry.Model;
using SPLA.Plugins.Geometry.Render;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.Plugins.Geometry.Session;

/// <summary>
/// A chart of probe looks at several sizes, each cell a different random number, and the arithmetic
/// that turns "these are the numbers I could read" into the smallest look this model reads
/// (ADR_20260916-3).
/// <para>
/// The model is not asked which look it prefers — asked that, a model says "all of them". It is asked
/// to read the numbers, and a number it did not read, or read as another, is the answer.
/// </para>
/// </summary>
internal sealed class LegibilityChart
{
    /// <summary>Digit sizes across, smallest first.</summary>
    public static readonly int[] Sizes = [8, 10, 12, 14, 16, 20];

    /// <summary>Looks down, in the order a tie at the same size is broken: what covers least of the
    /// point being asked about comes first.</summary>
    public static readonly (string Marker, string Label)[] Looks =
    [
        ("ring", "outline"), ("dot", "outline"), ("ring", "plate"), ("dot", "plate"), ("bare", "outline"), ("bare", "dark"), ("badge", "plate"),
    ];

    public required int Round { get; init; }

    /// <summary>One probe per cell, row by row; <see cref="Probe.Row"/> is the look, <see cref="Probe.Column"/> the size.</summary>
    public required IReadOnlyList<Probe> Probes { get; init; }

    public required IReadOnlyList<ProbeStyle> Styles { get; init; }

    public static LegibilityChart Build(int round, int width, int height, string colour)
    {
        var random = new Random(unchecked(round * 7919 + 17));
        // Two digits everywhere, so every cell is the same reading task and only the look and size differ.
        var numbers = Enumerable.Range(10, 90).OrderBy(_ => random.Next()).ToArray();

        var probes = new List<Probe>();
        var styles = new List<ProbeStyle>();
        double cellWidth = (double)width / Sizes.Length, cellHeight = (double)height / Looks.Length;
        for (var row = 0; row < Looks.Length; row++)
        for (var column = 0; column < Sizes.Length; column++)
        {
            // Left of the cell's middle: a label goes to the right of its marker.
            var x = (column + 0.35) * cellWidth;
            var y = (row + 0.5) * cellHeight;
            probes.Add(new Probe(numbers[probes.Count], x, y, -1, 0, column, row, 1, 0));
            styles.Add(new ProbeStyle(Looks[row].Marker, Looks[row].Label, Sizes[column], colour));
        }

        return new LegibilityChart { Round = round, Probes = probes, Styles = styles };
    }

    /// <summary>
    /// For each look, the smallest size from which every larger size was read too, or null. A small cell
    /// read while a larger one of the same look was not is taken for luck, not for legibility.
    /// </summary>
    public IReadOnlyList<int?> Thresholds(IReadOnlySet<int> read)
    {
        var result = new List<int?>();
        for (var row = 0; row < Looks.Length; row++)
        {
            int? threshold = null;
            for (var column = Sizes.Length - 1; column >= 0; column--)
            {
                if (!read.Contains(Probes[row * Sizes.Length + column].Number)) break;
                threshold = Sizes[column];
            }
            result.Add(threshold);
        }
        return result;
    }

    /// <summary>The look to use: the smallest threshold, ties to the earlier look; null when nothing was read.</summary>
    public ProbeStyle? Choose(IReadOnlySet<int> read, string colour)
    {
        var thresholds = Thresholds(read);
        var candidates = Enumerable.Range(0, Looks.Length).Where(row => thresholds[row] is not null).ToList();
        if (candidates.Count == 0) return null;

        var best = candidates.OrderBy(row => thresholds[row]).ThenBy(row => row).First();
        return new ProbeStyle(Looks[best].Marker, Looks[best].Label, thresholds[best]!.Value, colour);
    }
}
