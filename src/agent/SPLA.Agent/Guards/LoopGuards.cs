using System;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.Agent.Guards;

/// <summary>
/// Anti-loop guards for the agent loop. Canonical home for logic that previously lived
/// (duplicated, and only in the UI) under SPLA.UI.Avalonia/Services/Guards.
/// </summary>
public static class LoopGuards
{
    /// <summary>Returns true when the last <paramref name="windowSize"/> tool calls are identical.</summary>
    public static bool HasToolCallLoop(IReadOnlyCollection<(string name, string args)> recentCalls, int windowSize = 3)
    {
        if (recentCalls.Count < windowSize) return false;
        var arr = recentCalls.TakeLast(windowSize).ToArray();
        var first = arr[0];
        return arr.All(x => x == first);
    }

    /// <summary>Returns true when the last <paramref name="windowSize"/> entries are the same failing call.</summary>
    public static bool HasErrorLoop(IReadOnlyCollection<(string name, string args, bool isError)> recentErrors, int windowSize = 3)
    {
        if (recentErrors.Count < windowSize) return false;
        var arr = recentErrors.TakeLast(windowSize).ToArray();
        var first = arr[0];
        return arr.All(x => x.isError && x.name == first.name && x.args == first.args);
    }

    /// <summary>
    /// Returns true when the tail of <paramref name="text"/> contains a suffix of at least
    /// <paramref name="minSuffixLen"/> chars repeating at least <paramref name="minRepeats"/> times.
    /// Examines only the last 3000 chars to stay cheap.
    /// </summary>
    public static bool HasTextLoop(string text, int minSuffixLen = 40, int minRepeats = 5)
    {
        if (text.Length < minSuffixLen * minRepeats) return false;

        var tail = text.Length > 3000 ? text.AsSpan(text.Length - 3000) : text.AsSpan();
        var suffix = tail.Slice(tail.Length - minSuffixLen);

        int count = 0;
        int pos = tail.Length - minSuffixLen;
        while (pos >= minSuffixLen)
        {
            pos -= minSuffixLen;
            if (tail.Slice(pos, minSuffixLen).SequenceEqual(suffix))
            {
                count++;
                if (count >= minRepeats - 1) return true;
            }
            else
            {
                count = 0;
            }
        }
        return false;
    }
}
