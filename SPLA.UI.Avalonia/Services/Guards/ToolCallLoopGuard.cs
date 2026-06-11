using System;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.UI.Avalonia.Services.Guards;

public static class ToolCallLoopGuard
{
    /// <summary>
    /// Returns true when the last N entries in <paramref name="recentCalls"/> are all identical.
    /// </summary>
    public static bool HasToolCallLoop(Queue<(string name, string args)> recentCalls, int windowSize = 3)
    {
        if (recentCalls.Count < windowSize) return false;
        var arr = recentCalls.TakeLast(windowSize).ToArray();
        var first = arr[0];
        return arr.All(x => x == first);
    }

    /// <summary>
    /// Returns true when the last N entries in <paramref name="recentErrors"/> are the same failing call.
    /// </summary>
    public static bool HasErrorLoop(Queue<(string name, string args, bool isError)> recentErrors, int windowSize = 3)
    {
        if (recentErrors.Count < windowSize) return false;
        var arr = recentErrors.TakeLast(windowSize).ToArray();
        var first = arr[0];
        return arr.All(x => x.isError && x.name == first.name && x.args == first.args);
    }
}
