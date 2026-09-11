namespace SPLA.Plugins.Android.Gestures;

internal static class GestureBuilders
{
    public static Gesture Tap(int x, int y) => LongPress(x, y, 50);
    public static Gesture LongPress(int x, int y, int ms) => Swipe(x, y, x, y, ms);
    public static Gesture Swipe(int x1, int y1, int x2, int y2, int ms) => new([new([new(x1, y1, 0), new(x2, y2, ms)])]);
    public static Gesture Pinch(int cx, int cy, int fromDistance, int toDistance, double angleDeg, int ms)
    {
        if (fromDistance < 0 || toDistance < 0 || !double.IsFinite(angleDeg)) throw new ArgumentException("Invalid pinch distances or angle.");
        var radians = angleDeg * Math.PI / 180;
        GesturePoint Point(int sign, int distance, int time) => new(
            checked(cx + (int)Math.Round(sign * distance * Math.Cos(radians) / 2)),
            checked(cy + (int)Math.Round(sign * distance * Math.Sin(radians) / 2)), time);
        return new([new([Point(-1, fromDistance, 0), Point(-1, toDistance, ms)]), new([Point(1, fromDistance, 0), Point(1, toDistance, ms)])]);
    }

    public static void Validate(Gesture gesture, int width, int height)
    {
        if (gesture.Pointers.Count is < 1 or > 10) throw new ArgumentException("Use 1 to 10 pointers.");
        foreach (var pointer in gesture.Pointers)
        {
            if (pointer.Path.Count is < 1 or > 4096) throw new ArgumentException("Each pointer needs 1 to 4096 points.");
            var previous = -1;
            foreach (var point in pointer.Path)
            {
                if (point.X < 0 || point.Y < 0 || point.X >= width || point.Y >= height)
                    throw new ArgumentException($"Point ({point.X},{point.Y}) is outside the {width}x{height} screen.");
                if (point.TMs < previous || point.TMs is < 0 or > 60000) throw new ArgumentException("Times must be ascending, between 0 and 60000 ms.");
                previous = point.TMs;
            }
        }
    }
}
