namespace SPLA.Plugins.Android.Gestures;

internal sealed record GesturePoint(int X, int Y, int TMs);
internal sealed record GesturePointer(IReadOnlyList<GesturePoint> Path);
internal sealed record Gesture(IReadOnlyList<GesturePointer> Pointers);
