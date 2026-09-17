using System.Text;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// A minimal vt100/xterm screen: raw pty bytes in, settled TEXT out. This is what the agent branch
/// of a session reads instead of the byte stream, and the difference is not cosmetic.
///
/// <para>A pty stream is not a transcript — it is a sequence of instructions for painting one
/// rectangle. <c>docker pull</c>, <c>apt</c>, <c>npm</c>, <c>curl</c> and every other progress bar
/// repaint the SAME lines thousands of times, with CR and cursor-up saying "go back and overwrite".
/// Accumulating that stream keeps every frame ever painted: ten minutes of a docker pull is half a
/// megabyte of text describing about twenty lines of actual state.</para>
///
/// <para>Stripping the escape codes and keeping the text — which is what this session did before —
/// is the worst of the two: it removes exactly the instructions that said "this replaces that" and
/// leaves the payload of every frame glued together (<c>103.8MB/113.2MBabc: Downloading</c>).
/// Structure destroyed, volume untouched. So the codes must be EXECUTED, not deleted.</para>
///
/// <para>Only the agent branch runs through here. Human viewers keep getting the raw stream — they
/// have a real terminal on the other end that does this job properly, and a second emulator in the
/// path could only lose fidelity (mouse reporting, alt-screen, 24-bit colour, every sequence this
/// class is content to ignore).</para>
///
/// <para>Deliberately partial: the sequences that move the cursor, erase, and scroll — the ones that
/// make progress output collapse. Everything else (colour, mode sets, alt-screen, mouse) is
/// recognised well enough to be CONSUMED and dropped, which is all the agent needs. A full-screen
/// program like <c>htop</c> renders as a plausible screen rather than a correct one, and that is the
/// accepted limit: the agent should not be driving htop.</para>
/// </summary>
internal sealed class TerminalScreen
{
    /// <summary>Lines that scrolled off the top are kept for the agent to read, but not forever:
    /// a long build log would otherwise re-create the unbounded buffer this class exists to remove.
    /// Dropped lines are counted, never silently forgotten (see <see cref="DroppedLines"/>).</summary>
    private const int MaxScrollback = 4000;

    private readonly List<string> _scrollback = new();
    private readonly List<StringBuilder> _screen = new();
    private int _cols;
    private int _height;
    private int _row;
    private int _col;

    /// <summary>Set when a glyph lands in the last column: the wrap happens on the NEXT glyph, not
    /// immediately, which is what keeps a line exactly <c>_cols</c> wide from eating a blank line
    /// after it. Real terminals all do this, and progress bars sized to the window depend on it.</summary>
    private bool _wrapPending;

    public TerminalScreen(int cols, int rows)
    {
        _cols = Math.Clamp(cols, 20, 500);
        _height = Math.Clamp(rows, 5, 200);
        _screen.Add(new StringBuilder());
    }

    /// <summary>How many scrolled-off lines were discarded to stay inside <see cref="MaxScrollback"/>.
    /// A reader's cursor is expressed in absolute line numbers, so this is what tells it the window
    /// moved out from under it.</summary>
    public int DroppedLines { get; private set; }

    /// <summary>Absolute number of lines that have left the screen — the coordinate a reader's cursor
    /// lives in. Monotonic; unaffected by scrollback trimming.</summary>
    public int SettledCount => DroppedLines + _scrollback.Count;

    /// <summary>The live rectangle, trailing blank rows removed. These lines are still being painted
    /// and may change with the next chunk.</summary>
    public IReadOnlyList<string> Live
    {
        get
        {
            var end = _screen.Count;
            while (end > 0 && _screen[end - 1].Length == 0) end--;
            var live = new List<string>(end);
            for (var i = 0; i < end; i++) live.Add(_screen[i].ToString().TrimEnd());
            return live;
        }
    }

    /// <summary>Settled lines from absolute position <paramref name="from"/> onwards. Lines already
    /// trimmed from scrollback cannot be returned; the caller learns that from
    /// <see cref="DroppedLines"/> rather than from a silently short answer.</summary>
    public IReadOnlyList<string> SettledFrom(int from)
    {
        var start = Math.Clamp(from - DroppedLines, 0, _scrollback.Count);
        return _scrollback.GetRange(start, _scrollback.Count - start);
    }

    /// <summary>Everything currently readable — settled lines plus the live rectangle. Used for
    /// whole-output matching (an <c>until</c> pattern), where the point is to see what a human
    /// watching the terminal would see.</summary>
    public string Text()
    {
        var sb = new StringBuilder();
        foreach (var line in _scrollback) sb.Append(line).Append('\n');
        foreach (var line in Live) sb.Append(line).Append('\n');
        return sb.ToString();
    }

    /// <summary>The line the cursor is on, as painted so far. What a prompt waiting for input looks
    /// like — <c>[sudo] password for x:</c> with no newline after it.</summary>
    public string CursorLine => _row >= 0 && _row < _screen.Count ? _screen[_row].ToString().TrimEnd() : "";

    /// <summary>Follows a pty resize so later output wraps at the width the remote is using. Existing
    /// content is not re-flowed — a repaint after a resize replaces it anyway, and re-wrapping
    /// settled lines would rewrite text the agent may already have read.</summary>
    public void Resize(int cols, int rows)
    {
        _cols = Math.Clamp(cols, 20, 500);
        _height = Math.Clamp(rows, 5, 200);
        if (_col >= _cols) _col = _cols - 1;
    }

    // ── Feeding ────────────────────────────────────────────────────────────────

    public void Feed(string chunk)
    {
        for (var i = 0; i < chunk.Length; i++)
        {
            var ch = chunk[i];
            switch (ch)
            {
                case '\x1b':
                    i = Escape(chunk, i);
                    break;
                case '\r':
                    _col = 0;
                    _wrapPending = false;
                    break;
                case '\n':
                    LineFeed();
                    break;
                case '\b':
                    if (_col > 0) _col--;
                    _wrapPending = false;
                    break;
                case '\t':
                    do { Put(' '); } while (_col % 8 != 0);
                    break;
                case '\a':
                    break; // bell — nothing to show
                default:
                    if (!char.IsControl(ch)) Put(ch);
                    break;
            }
        }
    }

    /// <summary>Handles the escape sequence starting at <paramref name="start"/> and returns the index
    /// of its last consumed character. An incomplete sequence at the end of a chunk is dropped rather
    /// than buffered: the loss is one repaint out of thousands, and carrying partial-sequence state
    /// across chunks is where this class would start to need a real parser.</summary>
    private int Escape(string s, int start)
    {
        if (start + 1 >= s.Length) return s.Length - 1;
        var next = s[start + 1];

        if (next == '[') return Csi(s, start + 2);

        // OSC and the other string-introducers run until BEL or ST.
        if (next is ']' or 'P' or '^' or '_')
        {
            for (var i = start + 2; i < s.Length; i++)
            {
                if (s[i] == '\a') return i;
                if (s[i] == '\x1b' && i + 1 < s.Length && s[i + 1] == '\\') return i + 1;
            }
            return s.Length - 1;
        }

        switch (next)
        {
            case 'M': // reverse index
                if (_row > 0) _row--;
                return start + 1;
            case 'D': // index
                LineFeed();
                return start + 1;
            case 'E': // next line
                _col = 0;
                LineFeed();
                return start + 1;
            case 'c': // full reset
                _screen.Clear();
                _screen.Add(new StringBuilder());
                _row = _col = 0;
                _wrapPending = false;
                return start + 1;
            default:
                // Two-character sequences with an intermediate byte (charset selection etc).
                return next is >= ' ' and <= '/' && start + 2 < s.Length ? start + 2 : start + 1;
        }
    }

    /// <summary>CSI — the sequences that actually matter here. Returns the index of the final byte.</summary>
    private int Csi(string s, int from)
    {
        var i = from;
        var isPrivate = i < s.Length && (s[i] == '?' || s[i] == '<' || s[i] == '=' || s[i] == '>');
        if (isPrivate) i++;
        var paramStart = i;
        while (i < s.Length && (char.IsAsciiDigit(s[i]) || s[i] == ';')) i++;
        var parameters = s[paramStart..i];
        while (i < s.Length && s[i] is >= ' ' and <= '/') i++; // intermediate bytes
        if (i >= s.Length) return s.Length - 1;
        var final = s[i];

        // Private modes (cursor hide, alt screen, mouse reporting …) are consumed and ignored: they
        // change how a screen is presented, not what it says.
        if (isPrivate) return i;

        int P(int index, int fallback)
        {
            if (parameters.Length == 0) return fallback;
            var parts = parameters.Split(';');
            if (index >= parts.Length || !int.TryParse(parts[index], out var v) || v == 0) return fallback;
            return v;
        }

        switch (final)
        {
            case 'A': _row = Math.Max(0, _row - P(0, 1)); _wrapPending = false; break;
            case 'B': MoveDown(P(0, 1)); break;
            case 'C': _col = Math.Min(_cols - 1, _col + P(0, 1)); _wrapPending = false; break;
            case 'D': _col = Math.Max(0, _col - P(0, 1)); _wrapPending = false; break;
            case 'E': _col = 0; MoveDown(P(0, 1)); break;
            case 'F': _col = 0; _row = Math.Max(0, _row - P(0, 1)); _wrapPending = false; break;
            case 'G': _col = Math.Clamp(P(0, 1) - 1, 0, _cols - 1); _wrapPending = false; break;
            case 'H':
            case 'f':
                _row = Math.Clamp(P(0, 1) - 1, 0, _height - 1);
                _col = Math.Clamp(P(1, 1) - 1, 0, _cols - 1);
                EnsureRow(_row);
                _wrapPending = false;
                break;
            case 'J': EraseDisplay(parameters.Length == 0 ? 0 : P(0, 0)); break;
            case 'K': EraseLine(parameters.Length == 0 ? 0 : P(0, 0)); break;
            case 'L': InsertLines(P(0, 1)); break;
            case 'M': DeleteLines(P(0, 1)); break;
            case 'P': DeleteChars(P(0, 1)); break;
            case 'X': // erase characters in place
            {
                var row = EnsureRow(_row);
                for (var k = 0; k < P(0, 1) && _col + k < row.Length; k++) row[_col + k] = ' ';
                break;
            }
            case '@': // insert blanks
            {
                var row = EnsureRow(_row);
                row.Insert(Math.Min(_col, row.Length), new string(' ', P(0, 1)));
                break;
            }
            // 'm' (colour), 'r' (scroll region), 'n' (device status), 's'/'u' (cursor save) and the
            // rest: consumed, no effect on the text.
        }
        return i;
    }

    private void MoveDown(int n)
    {
        _wrapPending = false;
        for (var k = 0; k < n; k++)
        {
            if (_row + 1 >= _height) { Scroll(); continue; }
            _row++;
            EnsureRow(_row);
        }
    }

    /// <summary>Moves to the start of the next line — a newline, not a strict vt100 index.
    /// <para>A real terminal's LF drops a row and keeps the column, because the pty's ONLCR has
    /// already sent the CR that returns it. This class is fed by things that are not always a pty:
    /// a chunk carrying a bare newline is a line ending everywhere it comes from, and treating it as
    /// "down one, stay in column 47" silently indents every line after the first. Nothing is lost on
    /// a real stream — the CR arrives first and has already zeroed the column.</para></summary>
    private void LineFeed()
    {
        _col = 0;
        _wrapPending = false;
        if (_row + 1 >= _height) { Scroll(); return; }
        _row++;
        EnsureRow(_row);
    }

    /// <summary>The top line leaves the screen and becomes settled text. This — not the newline — is
    /// the moment a line stops being able to change, which is what makes "new output since I last
    /// looked" a well-defined thing to return.</summary>
    private void Scroll()
    {
        if (_screen.Count == 0) { _screen.Add(new StringBuilder()); return; }
        Settle(_screen[0].ToString().TrimEnd());
        _screen.RemoveAt(0);
        while (_screen.Count < _height) _screen.Add(new StringBuilder());
        _row = _height - 1;
    }

    private void Settle(string line)
    {
        _scrollback.Add(line);
        if (_scrollback.Count <= MaxScrollback) return;
        var excess = _scrollback.Count - MaxScrollback;
        _scrollback.RemoveRange(0, excess);
        DroppedLines += excess;
    }

    private StringBuilder EnsureRow(int index)
    {
        while (_screen.Count <= index && _screen.Count < _height) _screen.Add(new StringBuilder());
        if (index >= _screen.Count) index = _screen.Count - 1;
        if (_row >= _screen.Count) _row = _screen.Count - 1;
        return _screen[index];
    }

    private void Put(char ch)
    {
        if (_wrapPending)
        {
            _col = 0;
            LineFeed();
        }
        var row = EnsureRow(_row);
        while (row.Length < _col) row.Append(' ');
        if (_col < row.Length) row[_col] = ch;
        else row.Append(ch);
        if (_col + 1 >= _cols) _wrapPending = true;
        else _col++;
    }

    private void EraseLine(int mode)
    {
        var row = EnsureRow(_row);
        switch (mode)
        {
            case 0: if (_col < row.Length) row.Length = _col; break;
            case 1: for (var k = 0; k <= _col && k < row.Length; k++) row[k] = ' '; break;
            default: row.Clear(); break;
        }
    }

    private void EraseDisplay(int mode)
    {
        switch (mode)
        {
            case 0:
                EraseLine(0);
                for (var k = _row + 1; k < _screen.Count; k++) _screen[k].Clear();
                break;
            case 1:
                for (var k = 0; k < _row && k < _screen.Count; k++) _screen[k].Clear();
                EraseLine(1);
                break;
            default:
                // A full clear discards the screen WITHOUT settling it: the program is repainting
                // from scratch, and keeping the erased frame is exactly the duplication this class
                // exists to remove.
                foreach (var row in _screen) row.Clear();
                _row = _col = 0;
                break;
        }
        _wrapPending = false;
    }

    private void InsertLines(int n)
    {
        for (var k = 0; k < n && _row < _screen.Count; k++)
        {
            _screen.Insert(_row, new StringBuilder());
            if (_screen.Count > _height) _screen.RemoveAt(_screen.Count - 1);
        }
        _col = 0;
        _wrapPending = false;
    }

    private void DeleteLines(int n)
    {
        for (var k = 0; k < n && _row < _screen.Count; k++)
        {
            _screen.RemoveAt(_row);
            _screen.Add(new StringBuilder());
        }
        _col = 0;
        _wrapPending = false;
    }

    private void DeleteChars(int n)
    {
        var row = EnsureRow(_row);
        var count = Math.Min(n, Math.Max(0, row.Length - _col));
        if (count > 0) row.Remove(_col, count);
    }
}
