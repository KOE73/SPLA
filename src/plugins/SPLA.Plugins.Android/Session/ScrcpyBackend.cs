using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Scrcpy;
using SPLA.Plugins.Android.Video;

namespace SPLA.Plugins.Android.Session;

internal sealed class ScrcpyBackend : IDeviceBackend
{
    private readonly ScrcpyConnection connection;
    private readonly H264Decoder decoder;
    private readonly AndroidSettings settings;
    private readonly CancellationTokenSource lifetime = new();
    private readonly SemaphoreSlim writer = new(1);
    private readonly LatestFrame frames = new();
    private readonly Task videoTask, drainTask;
    private Exception? failure;
    private long afterActionSequence = -1;
    public string Kind => "scrcpy";
    public bool SupportsMultiTouch => true;
    public bool IsBroken => Volatile.Read(ref failure) is not null;
    public (int Width, int Height) ScreenSize => frames.Current is { } frame ? (frame.Width, frame.Height) : (0, 0);

    private ScrcpyBackend(ScrcpyConnection connection, H264Decoder decoder, AndroidSettings settings)
    {
        this.connection = connection; this.decoder = decoder; this.settings = settings;
        videoTask = GuardAsync(async () =>
        {
            await foreach (var item in VideoDemuxer.ReadAsync(connection.Video, lifetime.Token))
                if (item is VideoPacket packet) foreach (var frame in decoder.Decode(packet)) frames.Publish(frame);
            throw new EndOfStreamException("scrcpy video stream ended.");
        });
        drainTask = GuardAsync(() => DeviceMessageDrain.RunAsync(connection.Control, lifetime.Token));
    }

    public static async Task<ScrcpyBackend> StartAsync(AdbRunner adb, string serial, string folder, AndroidSettings settings, CancellationToken ct)
    {
        var decoder = new H264Decoder(folder);
        ScrcpyBackend? backend = null;
        try
        {
            var connection = await ScrcpyServerLauncher.StartAsync(adb, serial, folder, settings, ct);
            backend = new(connection, decoder, settings);
            await backend.frames.WaitNextAsync(-1, TimeSpan.FromSeconds(5), ct);
            return backend;
        }
        catch { if (backend is not null) await backend.DisposeAsync(); else decoder.Dispose(); throw; }
    }

    private async Task GuardAsync(Func<Task> action)
    {
        try { await action(); }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
        catch (Exception ex) { Volatile.Write(ref failure, ex); frames.Fail(ex); }
    }

    public async Task<Screenshot> CaptureAsync(bool waitStable, CancellationToken ct)
    {
        if (failure is { } error) throw new IOException("scrcpy stream is broken.", error);
        if (afterActionSequence >= 0)
        {
            try { await frames.WaitNextAsync(afterActionSequence, TimeSpan.FromMilliseconds(150), ct); }
            catch (TimeoutException)
            {
                // Some hardware encoders stop repeating once an unchanged image converges.
                // Request fresh capture evidence without touching the UI or inventing stability.
                await RefreshAsync(ct);
                await frames.WaitNextAsync(afterActionSequence, TimeSpan.FromSeconds(5), ct);
            }
            afterActionSequence = -1;
        }
        var result = waitStable ? await frames.WaitStableAsync(settings.SettleMs, settings.SettleTimeoutMs, ct, RefreshAsync)
            : (await frames.WaitNextAsync(-1, TimeSpan.FromSeconds(5), ct), false, 0L);
        var frame = result.Item1;
        return new(PngEncoder.Encode(YuvToRgb.ToRgb24(frame), frame.Width, frame.Height), frame.Width, frame.Height, Kind, result.Item2, result.Item3);
    }
    private Task SendAsync(byte[] message, CancellationToken ct) => connection.Control.WriteAsync(message, ct).AsTask();
    private async Task RefreshAsync(CancellationToken ct)
    {
        await writer.WaitAsync(ct);
        try { await SendAsync([17], ct); } // RESET_VIDEO in scrcpy 4.1
        finally { writer.Release(); }
    }
    private async Task WriteAsync(Func<Task> action, CancellationToken ct)
    {
        await writer.WaitAsync(ct);
        try { frames.InvalidateStability(); await action(); }
        catch (IOException ex) { Volatile.Write(ref failure, ex); throw; }
        finally
        {
            frames.InvalidateStability();
            afterActionSequence = frames.Current?.Sequence ?? -1;
            writer.Release();
        }
    }
    public Task PlayAsync(Gesture gesture, CancellationToken ct) => WriteAsync(() =>
        GesturePlayer.PlayAsync(gesture, ScreenSize.Width, ScreenSize.Height, SendAsync, ct), ct);
    public Task KeyAsync(int keycode, bool longPress, CancellationToken ct) => WriteAsync(async () =>
    {
        await SendAsync(ControlMessages.Key(0, keycode), ct);
        try { if (longPress) await Task.Delay(800, ct); }
        finally
        {
            using var release = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await SendAsync(ControlMessages.Key(1, keycode), release.Token);
        }
    }, ct);
    public async Task<TextEntryResult> TypeTextAsync(string text, CancellationToken ct)
    {
        var clipboard = text.Any(c => c > 127);
        await WriteAsync(async () =>
        {
            if (clipboard) await SendAsync(ControlMessages.SetClipboard(text, true), ct);
            else for (var start = 0; start < text.Length; start += 300)
                await SendAsync(ControlMessages.Text(text.Substring(start, Math.Min(300, text.Length - start))), ct);
        }, ct);
        return new(clipboard ? "clipboard paste" : "scrcpy text", clipboard);
    }
    public async ValueTask DisposeAsync()
    {
        await lifetime.CancelAsync(); await connection.DisposeAsync();
        await Task.WhenAll(videoTask, drainTask);
        decoder.Dispose(); lifetime.Dispose(); writer.Dispose();
    }
}
