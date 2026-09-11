using System.Diagnostics;
using SPLA.Plugins.Android.Scrcpy;

namespace SPLA.Plugins.Android.Gestures;

internal static class GesturePlayer
{
    internal sealed record TouchEvent(int Time, byte Action, int Pointer, int X, int Y);
    internal static IReadOnlyList<TouchEvent> Timeline(Gesture gesture)
    {
        List<TouchEvent> events = [];
        for (var pointer = 0; pointer < gesture.Pointers.Count; pointer++)
        {
            var path = gesture.Pointers[pointer].Path;
            var first = path[0]; var last = path[^1];
            events.Add(new(first.TMs, 0, pointer, first.X, first.Y));
            for (var segment = 1; segment < path.Count; segment++)
            {
                var from = path[segment - 1]; var to = path[segment];
                for (var time = from.TMs + 16; time < to.TMs; time += 16)
                {
                    var fraction = (double)(time - from.TMs) / (to.TMs - from.TMs);
                    events.Add(new(time, 2, pointer, (int)Math.Round(from.X + (to.X - from.X) * fraction), (int)Math.Round(from.Y + (to.Y - from.Y) * fraction)));
                }
                if (segment < path.Count - 1) events.Add(new(to.TMs, 2, pointer, to.X, to.Y));
            }
            events.Add(new(last.TMs, 1, pointer, last.X, last.Y));
        }
        return events.OrderBy(e => e.Time).ThenBy(e => e.Action == 0 ? 0 : e.Action == 2 ? 1 : 2)
            .ThenBy(e => e.Action == 1 ? -e.Pointer : e.Pointer).ToArray();
    }

    public static async Task PlayAsync(Gesture gesture, int width, int height, Func<byte[], CancellationToken, Task> send, CancellationToken ct)
    {
        GestureBuilders.Validate(gesture, width, height);
        var watch = Stopwatch.StartNew(); Dictionary<int, TouchEvent> pressed = [];
        try
        {
            foreach (var item in Timeline(gesture))
            {
                var delay = item.Time - watch.ElapsedMilliseconds;
                if (delay > 0) await Task.Delay(TimeSpan.FromMilliseconds(delay), ct);
                await send(ControlMessages.Touch(item.Action, item.Pointer, item.X, item.Y, width, height, item.Action == 1 ? 0 : 1), ct);
                if (item.Action == 1) pressed.Remove(item.Pointer); else pressed[item.Pointer] = item;
            }
        }
        finally
        {
            using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            foreach (var item in pressed.Values.OrderByDescending(e => e.Pointer))
                try { await send(ControlMessages.Touch(1, item.Pointer, item.X, item.Y, width, height, 0), cleanup.Token); } catch (Exception) { }
        }
    }
}
