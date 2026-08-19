using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Project;

namespace SPLA.Instances;

/// <summary>
/// The instance side of registration: announces this process to a hub and keeps telling it what it
/// is doing, for as long as the process lives.
///
/// <para><b>Why the instance registers, rather than the hub discovering.</b> A hub cannot look at
/// another machine's disk, and a pid it was told about means nothing there. Self-registration is the
/// only mechanism that works locally and remotely without being two mechanisms, and it makes the
/// connection itself the liveness signal: the hub sees the instance leave the moment the socket
/// drops, however it drops.</para>
///
/// <para><b>Failure is never fatal.</b> An unreachable hub must not stop an instance from serving —
/// the registry is a view, not a dependency. So the loop reconnects with a backoff and says nothing
/// louder than a log line: the project still has its lock, clients still connect straight to the
/// endpoint, and the only thing lost while the hub is away is the hub's view of this instance.</para>
/// </summary>
public sealed class InstanceRegistrar : IAsyncDisposable
{
    private readonly Uri _channel;
    private readonly string? _token;
    private readonly RegisterFrame _registration;
    private readonly Func<StatusFrame> _readStatus;
    private readonly Func<bool, Task> _onStopRequested;
    private readonly ILogger _log;
    private readonly CancellationTokenSource _cts = new();
    private Task? _loop;

    /// <param name="readStatus">Called to sample what this instance is doing. A pull rather than a
    /// push so the registrar cannot hold a stale copy of state somebody forgot to update.</param>
    /// <param name="onStopRequested">Invoked when the hub relays a stop. The instance decides whether
    /// it may go — the hub only passes the request on.</param>
    public InstanceRegistrar(
        string hubUrl,
        string? token,
        RegisterFrame registration,
        Func<StatusFrame> readStatus,
        Func<bool, Task> onStopRequested,
        ILogger log)
    {
        var baseUri = new Uri(hubUrl.TrimEnd('/') + RegistryRoutes.Channel);
        _channel = new Uri(baseUri.ToString().Replace("http://", "ws://").Replace("https://", "wss://"));
        _token = token;
        _registration = registration;
        _readStatus = readStatus;
        _onStopRequested = onStopRequested;
        _log = log;
    }

    public void Start() => _loop ??= Task.Run(() => RunAsync(_cts.Token));

    private async Task RunAsync(CancellationToken ct)
    {
        // Backoff that gives up on being clever: a few seconds, then a minute. A hub that is down for
        // an hour should not be retried every second, and one that just restarted should be found
        // again quickly.
        var delays = new[] { 2, 5, 15, 30, 60 };
        var attempt = 0;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await SessionAsync(ct);
                attempt = 0;   // a session that ran at all resets the backoff
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { return; }
            catch (Exception ex)
            {
                _log.LogDebug(ex, "Registry channel to {Hub} failed; will retry.", _channel);
            }

            var wait = TimeSpan.FromSeconds(delays[Math.Min(attempt++, delays.Length - 1)]);
            try { await Task.Delay(wait, ct); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task SessionAsync(CancellationToken ct)
    {
        using var socket = new ClientWebSocket();
        if (!string.IsNullOrWhiteSpace(_token))
            socket.Options.SetRequestHeader("Authorization", "Bearer " + _token);

        await socket.ConnectAsync(_channel, ct);
        _log.LogInformation("Registered with registry hub at {Hub}.", _channel);

        await SendAsync(socket, RegistryFrames.Register, _registration, ct);

        // State is pushed on a slow tick rather than on every change: the states that matter to a
        // watcher (waiting, stalled) are held for minutes, and a chatty feed would spend a socket on
        // a badge nobody is looking at yet. A change the person actually waits on — a question — is
        // still visible within one tick.
        var status = Task.Run(() => PushStatusAsync(socket, ct), ct);
        await ReceiveAsync(socket, ct);
        await status;
    }

    private async Task PushStatusAsync(WebSocket socket, CancellationToken ct)
    {
        string? last = null;
        while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            var current = _readStatus();
            var key = current.State + ":" + current.Clients;
            if (key != last)
            {
                last = key;
                try { await SendAsync(socket, RegistryFrames.Status, current, ct); }
                catch { return; }
            }

            try { await Task.Delay(TimeSpan.FromSeconds(2), ct); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task ReceiveAsync(WebSocket socket, CancellationToken ct)
    {
        var buffer = new byte[8 * 1024];
        while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            var result = await socket.ReceiveAsync(buffer, ct);
            if (result.MessageType == WebSocketMessageType.Close) return;

            var frame = JsonSerializer.Deserialize<RegistryFrame>(
                buffer.AsSpan(0, result.Count), RegistryJson.Options);

            if (frame?.Type != RegistryFrames.Stop) continue;

            var body = frame.Body?.Deserialize<StopFrame>(RegistryJson.Options) ?? new StopFrame();
            await _onStopRequested(body.Force);
        }
    }

    private static Task SendAsync(WebSocket socket, string type, object body, CancellationToken ct)
    {
        var frame = new RegistryFrame
        {
            Type = type,
            Body = JsonSerializer.SerializeToElement(body, RegistryJson.Options)
        };
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(frame, RegistryJson.Options));
        return socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        if (_loop is not null)
        {
            try { await _loop.WaitAsync(TimeSpan.FromSeconds(2)); } catch { /* shutting down anyway */ }
        }
        _cts.Dispose();
    }
}
