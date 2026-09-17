using System.Buffers.Binary;
using System.Diagnostics;
using System.Text;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;

namespace SPLA.Plugins.Android.Session;

internal sealed class AdbBackend(AdbRunner adb, string serial, AndroidSettings settings) : IDeviceBackend
{
    public string Kind => "adb";
    public bool SupportsMultiTouch => false;
    public (int Width, int Height) ScreenSize { get; private set; }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public async Task<Screenshot> CaptureAsync(bool waitStable, CancellationToken ct)
    {
        var watch = Stopwatch.StartNew();
        if (waitStable) await Task.Delay(settings.SettleMs, ct);
        var result = await adb.RunBinaryAsync(serial, ["exec-out", "screencap", "-p"], TimeSpan.FromSeconds(30), ct);
        var png = result.StdOut;
        if (result.ExitCode != 0 || png.Length < 24 || !png.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            throw new IOException($"screencap did not return a PNG: {result.StdErr}");
        var width = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16));
        var height = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20));
        if (width <= 0 || height <= 0) throw new IOException("Invalid screenshot dimensions.");
        ScreenSize = (width, height);
        return new(png, width, height, Kind, false, watch.ElapsedMilliseconds);
    }

    public async Task PlayAsync(Gesture gesture, CancellationToken ct)
    {
        GestureBuilders.Validate(gesture, ScreenSize.Width, ScreenSize.Height);
        if (gesture.Pointers.Count != 1) throw new NotSupportedException("Multi-touch needs scrcpy — install it on the Android plugin page.");
        var path = gesture.Pointers[0].Path;
        if (path[0].TMs > 0) await Task.Delay(path[0].TMs, ct);
        var first = path[0]; var last = path[^1];
        var duration = last.TMs - first.TMs;
        if (path.Any(p => p.X != first.X || p.Y != first.Y) || duration > 400)
            await adb.CheckedAsync(serial, ["shell", "input", "swipe", $"{first.X}", $"{first.Y}", $"{last.X}", $"{last.Y}", $"{Math.Max(1, duration)}"], ct, 70000);
        else await adb.CheckedAsync(serial, ["shell", "input", "tap", $"{first.X}", $"{first.Y}"], ct);
    }

    public async Task KeyAsync(int keycode, bool longPress, CancellationToken ct)
    {
        List<string> args = ["shell", "input", "keyevent"];
        if (longPress) args.Add("--longpress");
        args.Add($"{keycode}");
        await adb.CheckedAsync(serial, args, ct);
    }

    internal static string EscapeText(string text)
    {
        if (text.Any(c => c < 32 || c > 126)) throw new NotSupportedException("ADB text input supports printable ASCII only. Install scrcpy for Unicode text.");
        // input text converts %s to spaces before injecting; refuse literal %s instead of silently changing it.
        if (text.Contains("%s", StringComparison.Ordinal)) throw new NotSupportedException("Literal %s needs scrcpy text input.");
        return AdbRunner.Quote(text.Replace(" ", "%s"));
    }

    public async Task<TextEntryResult> TypeTextAsync(string text, CancellationToken ct)
    {
        await adb.CheckedAsync(serial, ["shell", "input", "text", EscapeText(text)], ct);
        return new("adb input", false);
    }
}
