using System;
using System.Collections.Generic;

namespace SPLA.Agent.Guards;

/// <summary>
/// Detects a degenerate generation loop — the same word or paragraph repeated verbatim until the
/// model hits the token ceiling — by watching one streamed channel for a tail that is an exact
/// repetition of some unit, of ANY period, not only ones that happen to divide a fixed block size.
///
/// <para>
/// Replaces the agent loop's old streamed-text guard (<c>LoopGuards.HasTextLoop</c>), which sliced the
/// tail into fixed 40-char blocks and so only ever caught loops whose period divides 40 — a period-6
/// loop ("right " repeated) or a 128-char paragraph looping never tripped it, because neither period
/// lines up with 40. Knows nothing about who is watching: it is fed by
/// <see cref="RepetitionGuardMiddleware"/>, one instance per output channel per model call.
/// </para>
///
/// <para>
/// <b>Tail-anchored, always.</b> This never scans from the start of the text looking for a periodic
/// run somewhere inside it — the only question it ever asks is "does the text ending right now, at
/// the very end of everything fed so far, consist of a unit repeating?" A run that has already ended
/// and been followed by ordinary prose is therefore invisible: it was the tail once, and by the time
/// the prose arrives it no longer is.
/// </para>
///
/// <para>
/// <b>Bounded memory, by construction.</b> Retained history is a fixed-capacity ring buffer holding at
/// most <see cref="MaxPeriod"/> × <see cref="MinRepeats"/> characters (~8400 with the defaults) — the
/// smallest span that can still prove <see cref="MinRepeats"/> copies of the longest unit this
/// detector is willing to catch. The whole generation is never retained; a period longer than
/// <see cref="MaxPeriod"/> is deliberately out of reach, not a missed detection.
/// </para>
///
/// <para>
/// <b>Incremental by construction.</b> The old guard re-scanned the whole streamed buffer on every
/// chunk (<c>streamed.ToString()</c> inside the socket read loop — O(n²) over a long generation).
/// This one keeps a Rabin-Karp rolling hash over a small anchor at the tail; each character updates
/// the hash and a hash→last-position map in O(1), and eviction from the ring buffer is O(1) per
/// character too (no periodic reshuffle). A hash match is only ever a CANDIDATE period — it is
/// confirmed (or rejected) by direct character comparison before anything is reported, so an
/// accidental hash collision can never produce a false trip.
/// </para>
///
/// <para>
/// <b>Checked after every character — for determinism, not for sensitivity.</b> Catching a live loop
/// never depended on where the stream was cut: a periodic string truncated at any point is still
/// periodic with the same period, so a loop in progress shows a periodic tail at every chunk boundary,
/// mid-unit or not. Checking only at chunk ends cost nothing in detection — what it cost was meaning.
/// It silently skipped the moments where a periodic run had just ENDED, so the same text answered
/// differently depending on how the provider happened to slice it, and <see cref="MinRepeats"/> stopped
/// denoting anything a person could tune: a threshold that fires or not according to network packet
/// boundaries is not a threshold. Per character it costs almost nothing — a candidate outside
/// <see cref="MinPeriod"/>..<see cref="MaxPeriod"/> is one comparison, and a wrong candidate dies on
/// its first mismatching character.
/// </para>
///
/// <para>
/// <b>The false positive this accepts.</b> Anything that ends with <see cref="MinRepeats"/> identical
/// blocks of at least <see cref="MinPeriod"/> characters trips it, including output a human would call
/// legitimate — an empty table skeleton, a separator line repeated, a column of identical rows. That
/// is judged rare enough in prose to be worth the cost of catching real loops, and
/// <see cref="MinRepeats"/> is the knob if it turns out otherwise. Note the shape of the risk: a false
/// trip does not corrupt anything, it abandons one generation.
/// </para>
/// </summary>
public sealed class RepetitionDetector
{
    /// <summary>Periods shorter than this are rejected outright — never elevated to a nearby valid
    /// multiple. Short periods are usually markup, indentation, or whitespace, not a loop.</summary>
    public int MinPeriod { get; set; } = 8;

    /// <summary>Minimum number of consecutive, exactly-matching copies of the unit required before
    /// reporting. Confirmed by direct comparison, never by hash alone.</summary>
    public int MinRepeats { get; set; } = 6;

    /// <summary>No detection work runs at all until the channel holds at least this many characters —
    /// avoids spending anything on the very start of a turn, where there is nothing to say yet.</summary>
    public int MinTailChars { get; set; } = 300;

    /// <summary>
    /// The longest repeating unit this detector can catch, in characters (roughly 20 lines of text).
    /// Expressed as a unit length rather than a raw buffer size on purpose: everything here is
    /// anchored at the tail, so the only knob that means anything is "how long a unit are we willing
    /// to recognize," not "how far back do we scan." The retained ring buffer's capacity
    /// (<see cref="MinRepeats"/> × this) follows from it rather than being configured separately.
    /// </summary>
    public int MaxPeriod { get; set; } = 1400;

    /// <summary>Non-null once a repeat has been confirmed. Sticky: once set it is never cleared or
    /// replaced, matching how the caller is expected to use it — see it non-null once, stop the turn.</summary>
    public RepetitionInfo? Detected { get; private set; }

    // Truncation length for the human-readable unit carried on RepetitionInfo — long enough to
    // recognize the looping text, short enough not to dump a multi-kilobyte run into a log line.
    private const int NoteMaxChars = 120;

    // Odd multiplier for the Rabin-Karp rolling hash over the anchor window. Not a detection
    // threshold — purely an internal mixing constant, so it stays private and unnamed-by-the-brief.
    private const ulong HashBase = 1000003uL;

    // Ring buffer of the retained tail: fixed capacity, O(1) push-and-evict, never grows past it and
    // never needs a periodic reshuffle the way trimming a StringBuilder would. Capacity is locked in
    // lazily on first use from MaxPeriod/MinRepeats as they stood then — mutating either mid-stream
    // (nothing here does, but nothing should crash over it) does not resize an already-running ring.
    private char[] _ring = Array.Empty<char>();
    private int _ringCapacity;
    private int _ringHead;   // logical index 0 (oldest retained char), if _ringCount > 0
    private int _ringCount;  // characters currently retained, <= _ringCapacity

    private long _totalLength;  // every char ever fed, including ones long since evicted
    private long _bufferStart;  // absolute channel offset of the oldest character still retained

    // Rolling hash of the last _anchorLen characters (the window ending at the current tail).
    // _anchorLen is fixed to MinPeriod the first time the ring reaches it — locking it in keeps the
    // hash's algebra self-consistent even if MinPeriod is mutated mid-stream.
    private int _anchorLen;
    private ulong _anchorHash;
    private ulong _anchorHighPow = 1; // HashBase ^ (anchorLen - 1), used to drop the char leaving the window

    // Most recent absolute start-position seen for each anchor hash. Only the latest occurrence is
    // kept: if the tail is genuinely periodic with period P, the position P characters back is always
    // the most recently recorded occurrence of that same hash, so "last wins" recovers exactly the
    // true recurrence distance without needing to remember every historical occurrence.
    private readonly Dictionary<ulong, long> _lastAnchorPos = new();

    // The candidate period implied by the anchor window ending at the most recently appended
    // character: a lookup made before that character's own position overwrites the map. Written and
    // read once per character, which is what makes the verdict independent of how the stream was cut.
    private long _pendingCandidatePeriod;

    // Batches the hash-map prune (drop entries the ring has since evicted) to once every _ringCapacity
    // characters instead of every character, so it stays amortized O(1) like the ring eviction itself.
    private int _sinceLastPrune;

    /// <summary>
    /// Feeds one chunk of streamed text. Chunk boundaries carry no meaning — including a surrogate
    /// pair or grapheme split across two calls — because the buffer is a flat sequence of UTF-16 code
    /// units and never interprets one; nothing here can throw over how a chunk happened to be cut.
    /// </summary>
    public void Feed(string chunk)
    {
        if (Detected != null || string.IsNullOrEmpty(chunk)) return;

        foreach (var c in chunk)
        {
            AppendChar(c);
            if (_totalLength < MinTailChars) continue;
            TryDetect();
            if (Detected != null) return;
        }
    }

    private void AppendChar(char c)
    {
        _totalLength++;
        RingPush(c);
        _bufferStart = _totalLength - _ringCount;

        if (_anchorLen == 0)
        {
            var wanted = Math.Max(1, MinPeriod);
            if (_ringCount < wanted) return; // still filling the very first anchor window
            _anchorLen = wanted;
            SeedAnchorHash();
        }
        else
        {
            // Roll the hash forward by one: drop the char that just left the window, fold in the new one.
            var leaving = RingCharAtRelative(_ringCount - 1 - _anchorLen);
            _anchorHash = unchecked((_anchorHash - (ulong)leaving * _anchorHighPow) * HashBase + c);
        }

        var currentAnchorStart = _totalLength - _anchorLen;
        _pendingCandidatePeriod = _lastAnchorPos.TryGetValue(_anchorHash, out var prevStart)
            ? currentAnchorStart - prevStart
            : 0;
        _lastAnchorPos[_anchorHash] = currentAnchorStart;

        if (++_sinceLastPrune >= _ringCapacity)
        {
            _sinceLastPrune = 0;
            PruneStaleAnchors();
        }
    }

    private void RingPush(char c)
    {
        if (_ringCapacity == 0)
        {
            // At least MinPeriod, too, so a pathological MaxPeriod/MinRepeats configuration can never
            // make the ring smaller than one anchor window — the seed step below needs that much.
            _ringCapacity = Math.Max(
                Math.Max(1, MaxPeriod) * Math.Max(1, MinRepeats),
                Math.Max(1, MinPeriod));
            _ring = new char[_ringCapacity];
        }

        if (_ringCount < _ringCapacity)
        {
            _ring[(_ringHead + _ringCount) % _ringCapacity] = c;
            _ringCount++;
        }
        else
        {
            _ring[_ringHead] = c;
            _ringHead = (_ringHead + 1) % _ringCapacity;
        }
    }

    private char RingCharAtRelative(int relativeIndex) => _ring[(_ringHead + relativeIndex) % _ringCapacity];

    private void SeedAnchorHash()
    {
        ulong h = 0;
        ulong pow = 1;
        var start = _ringCount - _anchorLen; // == 0 the first time this runs
        for (var i = 0; i < _anchorLen; i++)
        {
            h = unchecked(h * HashBase + RingCharAtRelative(start + i));
            if (i < _anchorLen - 1) pow = unchecked(pow * HashBase);
        }
        _anchorHash = h;
        _anchorHighPow = pow;
    }

    /// <summary>Drops hash-map entries the ring buffer has already evicted — they can never be
    /// confirmed by direct comparison again, so keeping them would leak memory over a long stream.</summary>
    private void PruneStaleAnchors()
    {
        if (_lastAnchorPos.Count == 0) return;
        List<ulong>? stale = null;
        foreach (var kv in _lastAnchorPos)
            if (kv.Value < _bufferStart)
                (stale ??= new List<ulong>()).Add(kv.Key);
        if (stale != null)
            foreach (var key in stale)
                _lastAnchorPos.Remove(key);
    }

    private void TryDetect()
    {
        if (Detected != null) return;
        if (_pendingCandidatePeriod < MinPeriod || _pendingCandidatePeriod > MaxPeriod) return;

        Detected = Verify((int)_pendingCandidatePeriod);
    }

    /// <summary>
    /// Confirms a hash-derived candidate period by direct character comparison, walking backward from
    /// the current tail. A hash match alone is never sufficient — this is the only place that decides.
    /// </summary>
    private RepetitionInfo? Verify(int period)
    {
        var end = _ringCount; // everything currently retained, logical index space
        if (period <= 0 || period > MaxPeriod || period >= end) return null;

        var pos = end - period; // logical start of the last (rightmost) candidate block
        var repeats = 1;
        while (pos - period >= 0)
        {
            if (!BlocksEqual(pos - period, pos, period)) break;
            pos -= period;
            repeats++;
        }

        if (repeats < MinRepeats) return null;

        var unitLen = Math.Min(period, NoteMaxChars);
        var unit = ExtractUnit(pos, unitLen);
        var note = period > NoteMaxChars ? unit + "…" : unit;

        return new RepetitionInfo
        {
            Period = period,
            Repeats = repeats,
            StartOffset = _bufferStart + pos,
            Note = note
        };
    }

    private bool BlocksEqual(int aStart, int bStart, int length)
    {
        for (var i = 0; i < length; i++)
            if (RingCharAtRelative(aStart + i) != RingCharAtRelative(bStart + i)) return false;
        return true;
    }

    private string ExtractUnit(int relativeStart, int length)
    {
        var chars = new char[length];
        for (var i = 0; i < length; i++) chars[i] = RingCharAtRelative(relativeStart + i);
        return new string(chars);
    }
}

/// <summary>One confirmed repeating run, as found by <see cref="RepetitionDetector"/>.</summary>
public sealed record RepetitionInfo
{
    /// <summary>Length of the repeating unit, in characters.</summary>
    public required int Period { get; init; }

    /// <summary>How many consecutive, exactly-matching copies were confirmed by direct comparison.
    /// Capped by how much history the ring buffer retains (<see cref="RepetitionDetector.MaxPeriod"/> ×
    /// <see cref="RepetitionDetector.MinRepeats"/> characters), so a run longer than that is reported
    /// with this under-counted rather than not at all.</summary>
    public required int Repeats { get; init; }

    /// <summary>Absolute character index into the channel (0-based, counting every character ever fed
    /// to the detector) where the confirmed repeating run begins.</summary>
    public required long StartOffset { get; init; }

    /// <summary>The repeated unit itself, truncated to ~120 chars — enough for a person to recognize
    /// what is looping without the note carrying a multi-kilobyte run.</summary>
    public required string Note { get; init; }
}
