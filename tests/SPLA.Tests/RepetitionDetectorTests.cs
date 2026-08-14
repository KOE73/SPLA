using System.Linq;
using System.Text;
using SPLA.Agent.Guards;

namespace SPLA.Tests;

/// <summary>
/// Covers <see cref="RepetitionDetector"/> against the failures that made the old fixed-block guard
/// useless: a period that doesn't divide 40 (6, 128 — neither does), and chunk boundaries changing the
/// answer. Also pins both sides of the false-positive line: an ordinary markdown table must survive,
/// and a tail of identical blocks must trip whatever the chunking.
/// </summary>
public class RepetitionDetectorTests
{
    /// <summary>Builds a block of <paramref name="length"/> chars with (for test purposes) no internal
    /// repetition. A fixed seed keeps this deterministic; random content over 26 letters makes an
    /// accidental smaller period astronomically unlikely — unlike a closed-form generator such as
    /// char(i) = 'a' + (7*i+3) % 26, which looks aperiodic but is secretly period-26 (the formula only
    /// depends on i mod 26), silently defeating any test built on top of it.</summary>
    private static string BuildUnit(int length)
    {
        var rng = new System.Random(20260814 + length);
        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
            sb.Append((char)('a' + rng.Next(26)));
        return sb.ToString();
    }

    /// <summary>Ordinary, non-repeating "prose": distinct numbered tokens, so no substring can ever
    /// recur verbatim by construction. <paramref name="tokenPrefix"/> lets two calls in the same test
    /// produce visibly different text.</summary>
    private static string BuildProse(int minLength, string tokenPrefix = "word")
    {
        var sb = new StringBuilder();
        var i = 0;
        while (sb.Length < minLength)
            sb.Append(tokenPrefix).Append(i++).Append(' ');
        return sb.ToString();
    }

    [Fact]
    public void Period_below_the_floor_is_never_reported()
    {
        // "right " has a true period of 6. MinPeriod = 8 is a deliberate choice, not an oversight —
        // short periods like this are overwhelmingly markup/indentation/whitespace, not a loop — so
        // this must NOT be elevated to some larger valid multiple either; it is simply ignored.
        var det = new RepetitionDetector();
        det.Feed(string.Concat(Enumerable.Repeat("right ", 200)));

        Assert.Null(det.Detected);
    }

    [Fact]
    public void Period_at_the_floor_is_detected()
    {
        // Deliberately not built from two copies of "right " ("right right "): that string's true
        // minimal period is 6 (identical to the previous test's input), so no detector examining
        // character content alone could tell the two apart or justify a different verdict. This unit
        // has no internal repetition at all, so its only valid period is its own length, 12.
        var unit = BuildUnit(12);
        var det = new RepetitionDetector();
        det.Feed(string.Concat(Enumerable.Repeat(unit, 200)));

        Assert.NotNull(det.Detected);
        Assert.Equal(12, det.Detected!.Period);
    }

    [Fact]
    public void Long_period_paragraph_is_detected()
    {
        // 128 does not divide the old guard's fixed 40-char block size — exactly the case it missed.
        var unit = BuildUnit(128);
        var det = new RepetitionDetector();
        det.Feed(string.Concat(Enumerable.Repeat(unit, 30)));

        Assert.NotNull(det.Detected);
        Assert.Equal(128, det.Detected!.Period);
        Assert.True(det.Detected!.Repeats >= 6);
    }

    [Fact]
    public void Chunk_boundaries_do_not_change_the_verdict()
    {
        var unit = BuildUnit(128);
        var full = string.Concat(Enumerable.Repeat(unit, 30));

        var whole = new RepetitionDetector();
        whole.Feed(full);

        var chunked = new RepetitionDetector();
        for (var i = 0; i < full.Length; i += 7)
            chunked.Feed(full.Substring(i, System.Math.Min(7, full.Length - i)));

        Assert.NotNull(whole.Detected);
        Assert.NotNull(chunked.Detected);
        Assert.Equal(whole.Detected!.Period, chunked.Detected!.Period);
        Assert.Equal(128, chunked.Detected!.Period);
    }

    [Fact]
    public void Ordinary_prose_is_never_detected()
    {
        var det = new RepetitionDetector();
        det.Feed(BuildProse(5000));

        Assert.Null(det.Detected);
    }

    [Fact]
    public void An_ordinary_markdown_table_is_not_detected()
    {
        // The shape most likely to worry someone reading this class: a table is visually repetitive.
        // It is not textually repetitive — real rows differ — so every candidate period the row
        // rhythm suggests is rejected on the first character that disagrees.
        var sb = new StringBuilder("| name | value |\n| --- | --- |\n");
        for (var i = 0; i < 20; i++) sb.Append("| word").Append(i).Append(" | value").Append(i).Append(" |\n");

        var det = new RepetitionDetector();
        foreach (var c in sb.ToString()) det.Feed(c.ToString());

        Assert.Null(det.Detected);
    }

    [Fact]
    public void Identical_tail_blocks_trip_whatever_the_chunking()
    {
        // The accepted false positive, pinned so it stays a decision rather than a surprise: text that
        // ENDS in MinRepeats identical blocks trips, even when a human would call it legitimate output
        // (here, an empty table skeleton). Pinned for both feed shapes, because the earlier version of
        // this detector only checked at chunk ends and so answered differently for the same text
        // depending on how the network sliced it — passing when fed whole, tripping when fed by token.
        var full = BuildProse(200, "pre") + string.Concat(Enumerable.Repeat("|      |      |\n", 8));

        var whole = new RepetitionDetector();
        whole.Feed(full);

        var perChar = new RepetitionDetector();
        foreach (var c in full) perChar.Feed(c.ToString());

        Assert.NotNull(whole.Detected);
        Assert.NotNull(perChar.Detected);
        Assert.Equal(whole.Detected!.Period, perChar.Detected!.Period);
    }

    [Fact]
    public void Repetition_below_the_repeat_threshold_is_not_detected()
    {
        // Three copies is not a loop yet — MinRepeats is 6. Note what this test canNOT be: "the text
        // repeated ten times earlier and then moved on". In a live stream that situation never exists,
        // because the tenth copy was the tail at the moment it was written and would have tripped the
        // guard right there. Only a run that never reaches the threshold survives to be followed.
        var unit = BuildUnit(128);
        var full = string.Concat(Enumerable.Repeat(unit, 3)) + BuildProse(400, "fresh");

        var det = new RepetitionDetector();
        foreach (var c in full) det.Feed(c.ToString());

        Assert.Null(det.Detected);
    }

    [Fact]
    public void Single_chunk_and_char_by_char_agree()
    {
        var unit = BuildUnit(128);
        var full = string.Concat(Enumerable.Repeat(unit, 30));

        var whole = new RepetitionDetector();
        whole.Feed(full);

        var perChar = new RepetitionDetector();
        foreach (var c in full)
            perChar.Feed(c.ToString());

        Assert.NotNull(whole.Detected);
        Assert.NotNull(perChar.Detected);
        Assert.Equal(whole.Detected!.Period, perChar.Detected!.Period);
    }
}
