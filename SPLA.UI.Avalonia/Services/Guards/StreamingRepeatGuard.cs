using System;

namespace SPLA.UI.Avalonia.Services.Guards;

public static class StreamingRepeatGuard
{
    /// <summary>
    /// Returns true when the tail of <paramref name="text"/> contains a suffix of at least
    /// <paramref name="minSuffixLen"/> characters that repeats at least <paramref name="minRepeats"/> times.
    /// Only examines the last 3000 characters to keep it O(1) in practice.
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
                if (count >= minRepeats - 1) return true; // -1 because we already have the last occurrence
            }
            else
            {
                count = 0;
            }
        }
        return false;
    }
}
