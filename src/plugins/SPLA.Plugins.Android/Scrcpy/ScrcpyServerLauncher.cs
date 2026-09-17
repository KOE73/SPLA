using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Runtime;

namespace SPLA.Plugins.Android.Scrcpy;

internal sealed class ScrcpyConnection(AdbRunner adb, string serial, int port, Process process,
    TcpClient video, TcpClient control, string deviceName, ConcurrentQueue<string> output) : IAsyncDisposable
{
    public NetworkStream Video => video.GetStream();
    public NetworkStream Control => control.GetStream();
    public string DeviceName => deviceName;
    public string OutputTail => string.Join('\n', output);
    public async ValueTask DisposeAsync()
    {
        video.Dispose(); control.Dispose();
        try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
        process.Dispose();
        try { await adb.CheckedAsync(serial, ["forward", "--remove", $"tcp:{port}"], CancellationToken.None, 5000); }
        catch (Exception) { /* Device unplugging also removes the forward. */ }
    }
}

internal static class ScrcpyServerLauncher
{
    internal static string[] ServerArguments(string version, string scid, AndroidSettings settings) =>
    ["shell", "CLASSPATH=/data/local/tmp/scrcpy-server.jar", "app_process", "/", "com.genymobile.scrcpy.Server", version,
        $"scid={scid}", "log_level=info", "tunnel_forward=true", "audio=false", "video=true", "control=true", "video_codec=h264",
        $"max_size={settings.MaxSize}", $"max_fps={settings.MaxFps}", $"video_bit_rate={settings.VideoBitRate}",
        $"stay_awake={settings.StayAwake.ToString().ToLowerInvariant()}", "cleanup=true", "power_off_on_close=false", "clipboard_autosync=false"];

    public static async Task<ScrcpyConnection> StartAsync(AdbRunner adb, string serial, string folder, AndroidSettings settings, CancellationToken ct)
    {
        var receipt = RuntimeReceipt.TryRead(folder);
        if (receipt?.Component != "scrcpy" || receipt.Version is not { } version || !Regex.IsMatch(version, @"^\d+\.\d+(?:\.\d+)?$"))
            throw new IOException("scrcpy needs an installation receipt with its server version. Install it from the Android plugin page.");
        var server = RuntimePaths.ScrcpyServer(folder) ?? throw new FileNotFoundException("scrcpy-server is missing.");
        var scid = Random.Shared.Next(int.MaxValue).ToString("x8");
        await adb.CheckedAsync(serial, ["push", server, "/data/local/tmp/scrcpy-server.jar"], ct);
        var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop();
        await adb.CheckedAsync(serial, ["forward", $"tcp:{port}", $"localabstract:scrcpy_{scid}"], ct);
        Process? process = null; TcpClient? video = null; TcpClient? control = null;
        ConcurrentQueue<string> output = new();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct); deadline.CancelAfter(TimeSpan.FromSeconds(15));
        try
        {
            process = adb.StartLongRunning(serial, ServerArguments(version, scid, settings), line =>
            { output.Enqueue(line); while (output.Count > 40) output.TryDequeue(out _); });
            for (var attempt = 0; attempt < 100; attempt++)
            {
                if (process.HasExited) throw new IOException($"scrcpy server exited: {string.Join('\n', output)}");
                video = new TcpClient();
                try
                {
                    await video.ConnectAsync(IPAddress.Loopback, port, deadline.Token);
                    await video.GetStream().ReadExactlyAsync(new byte[1], deadline.Token);
                    break;
                }
                catch (Exception ex) when (ex is IOException or SocketException)
                { video.Dispose(); video = null; await Task.Delay(100, deadline.Token); }
            }
            if (video is null) throw new IOException("scrcpy server did not open its video socket.");
            control = new TcpClient(); await control.ConnectAsync(IPAddress.Loopback, port, deadline.Token);
            var name = new byte[64]; await video.GetStream().ReadExactlyAsync(name, deadline.Token);
            var codec = new byte[4]; await video.GetStream().ReadExactlyAsync(codec, deadline.Token);
            if (BinaryPrimitives.ReadUInt32BigEndian(codec) != 0x68323634) throw new IOException("scrcpy did not select H.264 video.");
            return new(adb, serial, port, process, video, control, Encoding.UTF8.GetString(name).TrimEnd('\0'), output);
        }
        catch
        {
            video?.Dispose(); control?.Dispose();
            if (process is not null)
            {
                try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
                process.Dispose();
            }
            try { await adb.CheckedAsync(serial, ["forward", "--remove", $"tcp:{port}"], CancellationToken.None, 5000); } catch (Exception) { }
            ct.ThrowIfCancellationRequested();
            throw;
        }
    }
}
