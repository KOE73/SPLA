using SPLA.Plugins.Ssh;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The case this class was written for: a ten-minute <c>docker pull</c> came back to the model as
/// 460 000 characters, the turn exceeded the endpoint's context window, and the turn died. Nothing
/// in that half-megabyte was information — it was one screen of progress, repainted ten thousand
/// times, with the escape codes that said "this replaces that" stripped out and the payload of every
/// frame kept.
/// </summary>
public sealed class TerminalScreenTests
{
    private const string Esc = "\x1b";

    [Fact]
    public void Carriage_return_repaints_a_line_instead_of_appending_it()
    {
        var screen = new TerminalScreen(80, 24);

        // A progress bar: same line, rewritten in place.
        screen.Feed("Downloading 1MB/100MB\rDownloading 50MB/100MB\rDownloading 100MB/100MB\r\n");

        Assert.Equal(new[] { "Downloading 100MB/100MB" }, screen.SettledOrLive());
    }

    [Fact]
    public void Cursor_up_repaints_the_block_a_docker_pull_owns()
    {
        var screen = new TerminalScreen(80, 24);
        screen.Feed("layer-a: Downloading 10MB\nlayer-b: Downloading 20MB\n");

        // What docker actually emits between frames: go back up over its own block and rewrite it.
        for (var mb = 30; mb <= 900; mb += 10)
            screen.Feed($"{Esc}[2A{Esc}[2Klayer-a: Downloading {mb}MB\n{Esc}[2Klayer-b: Downloading {mb * 2}MB\n");

        var text = screen.Text();
        Assert.Contains("layer-a: Downloading 900MB", text);
        Assert.Contains("layer-b: Downloading 1800MB", text);

        // The point of the whole exercise: 88 repaints of two lines are two lines, not 88 of them.
        Assert.DoesNotContain("Downloading 500MB", text);
        Assert.Equal(2, screen.SettledOrLive().Count);
    }

    [Fact]
    public void A_long_progress_stream_stays_small()
    {
        var screen = new TerminalScreen(120, 30);
        for (var i = 0; i < 20_000; i++)
            screen.Feed($"\r48dd761b6bfe: Downloading  {i}MB/113.2MB");

        // The regression in one number. The raw stream fed in here is about a megabyte.
        Assert.True(screen.Text().Length < 200, $"screen kept {screen.Text().Length} chars");
        Assert.Contains("19999MB/113.2MB", screen.Text());
    }

    [Fact]
    public void Scrolled_lines_settle_and_are_handed_out_once()
    {
        var screen = new TerminalScreen(80, 5);
        for (var i = 1; i <= 12; i++) screen.Feed($"line {i}\n");

        var first = screen.SettledFrom(0);
        Assert.Equal("line 1", first[0]);

        // Read again from where the first read ended: nothing repeats.
        var cursor = screen.SettledCount;
        screen.Feed("line 13\n");
        Assert.Equal(new[] { "line 9" }, screen.SettledFrom(cursor));
    }

    [Fact]
    public void Erase_line_shortens_what_was_there()
    {
        var screen = new TerminalScreen(80, 24);
        screen.Feed("a very long status line indeed");
        screen.Feed($"\rshort{Esc}[K");

        Assert.Equal(new[] { "short" }, screen.SettledOrLive());
    }

    [Fact]
    public void Full_clear_drops_the_frame_rather_than_settling_it()
    {
        var screen = new TerminalScreen(80, 24);
        screen.Feed("old screen\n");
        screen.Feed($"{Esc}[2J{Esc}[Hnew screen");

        var text = screen.Text();
        Assert.DoesNotContain("old screen", text);
        Assert.Contains("new screen", text);
    }

    [Fact]
    public void Text_wraps_at_the_pty_width()
    {
        var screen = new TerminalScreen(20, 24);
        screen.Feed(new string('x', 25));

        var lines = screen.SettledOrLive();
        Assert.Equal(2, lines.Count);
        Assert.Equal(new string('x', 20), lines[0]);
        Assert.Equal(new string('x', 5), lines[1]);
    }

    [Fact]
    public void Colour_and_mode_sequences_leave_no_residue()
    {
        var screen = new TerminalScreen(80, 24);
        screen.Feed($"{Esc}[?25l{Esc}[1;32mPULL DONE{Esc}[0m{Esc}[?25h\n");

        Assert.Equal(new[] { "PULL DONE" }, screen.SettledOrLive());
    }

    [Fact]
    public void A_prompt_without_a_newline_is_the_cursor_line()
    {
        var screen = new TerminalScreen(80, 24);
        screen.Feed("Reading package lists...\n[sudo] password for oleg: ");

        Assert.Equal("[sudo] password for oleg:", screen.CursorLine);
    }

    [Fact]
    public void Scrollback_is_bounded_and_says_how_much_it_dropped()
    {
        var screen = new TerminalScreen(80, 24);
        for (var i = 0; i < 6000; i++) screen.Feed($"line {i}\n");

        Assert.True(screen.DroppedLines > 0);
        // The cursor coordinate stays absolute, so a reader can tell it lost ground.
        Assert.Equal(screen.DroppedLines + screen.SettledFrom(screen.DroppedLines).Count, screen.SettledCount);
    }
}

internal static class TerminalScreenTestExtensions
{
    /// <summary>Every readable line — settled plus live — with trailing blanks gone. What a person
    /// looking at the terminal would read off it.</summary>
    public static IReadOnlyList<string> SettledOrLive(this TerminalScreen screen)
    {
        var lines = new List<string>(screen.SettledFrom(0));
        lines.AddRange(screen.Live);
        while (lines.Count > 0 && lines[^1].Length == 0) lines.RemoveAt(lines.Count - 1);
        return lines;
    }
}
