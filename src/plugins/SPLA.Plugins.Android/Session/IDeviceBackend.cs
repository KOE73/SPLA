using SPLA.Plugins.Android.Gestures;

namespace SPLA.Plugins.Android.Session;

internal interface IDeviceBackend : IAsyncDisposable
{
    string Kind { get; }
    (int Width, int Height) ScreenSize { get; }
    bool SupportsMultiTouch { get; }
    bool IsBroken => false;
    Task<Screenshot> CaptureAsync(bool waitStable, CancellationToken ct);
    Task PlayAsync(Gesture gesture, CancellationToken ct);
    Task KeyAsync(int keycode, bool longPress, CancellationToken ct);
    Task<TextEntryResult> TypeTextAsync(string text, CancellationToken ct);
}

internal sealed record Screenshot(byte[] Png, int Width, int Height, string Source, bool Settled, long ElapsedMs);
internal sealed record TextEntryResult(string Method, bool ClipboardOverwritten);
