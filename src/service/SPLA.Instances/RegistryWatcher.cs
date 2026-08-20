using System.Net.WebSockets;
using System.Text.Json;
using SPLA.Domain.Project;

namespace SPLA.Instances;

/// <summary>
/// An observer's live view of a hub: the list, pushed whenever it changes.
///
/// <para><b>Why not poll the HTTP listing.</b> The thing a watcher exists for is
/// <see cref="InstanceState.Waiting"/> — the agent is blocked on a person and wants their attention
/// now. A tray icon that notices thirty seconds late is not an attention signal, and one that polls
/// every second to avoid that is a socket's worth of traffic spent pretending not to be a socket.
/// The hub already has a channel to every instance; giving observers one costs nothing new.</para>
///
/// <para>Reconnects on its own and never throws at the caller: a watcher that dies because the hub
/// restarted would leave a tray showing a frozen list forever, which is worse than an empty one.</para>
/// </summary>
public sealed class RegistryWatcher : IAsyncDisposable
{
    private readonly Uri _url;
    private readonly string? _token;
    private readonly CancellationTokenSource _cts = new();
    private Task? _loop;

    public RegistryWatcher(string hubUrl, string? token = null)
    {
        var http = hubUrl.TrimEnd('/') + RegistryRoutes.Watch;
        _url = new Uri(http.Replace("http://", "ws://").Replace("https://", "wss://"));
        _token = token;
    }

    /// <summary>The current list, replaced wholesale on every push. Whole list rather than a diff on
    /// purpose: it is small, and a watcher that reconciles deltas is a watcher that can drift from
    /// what the hub actually holds.</summary>
    public IReadOnlyList<InstanceRecord> Current { get; private set; } = [];

    /// <summary>Raised after <see cref="Current"/> changes, on the receive loop's thread. A UI handler
    /// must marshal to its own thread — this class knows nothing about one.</summary>
    public event Action<IReadOnlyList<InstanceRecord>>? Changed;

    /// <summary>Raised when the connection to the hub comes up or goes down, so a view can say "no
    /// hub" rather than silently showing an empty list that looks like "nothing running".</summary>
    public event Action<bool>? Connected;

    public void Start() => _loop ??= Task.Run(() => RunAsync(_cts.Token));

    private async Task RunAsync(CancellationToken ct)
    {
        var delays = new[] { 1, 3, 10, 30 };
        var attempt = 0;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await SessionAsync(ct);
                attempt = 0;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { return; }
            catch { /* hub down or restarting; the retry below is the whole response */ }

            Connected?.Invoke(false);
            try { await Task.Delay(TimeSpan.FromSeconds(delays[Math.Min(attempt++, delays.Length - 1)]), ct); }
            catch (OperationCanceledException) { return; }
        }
    }

    private async Task SessionAsync(CancellationToken ct)
    {
        using var socket = new ClientWebSocket();
        if (!string.IsNullOrWhiteSpace(_token))
            socket.Options.SetRequestHeader("Authorization", "Bearer " + _token);

        await socket.ConnectAsync(_url, ct);
        Connected?.Invoke(true);

        var buffer = new byte[64 * 1024];
        using var message = new MemoryStream();

        while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            message.SetLength(0);
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(buffer, ct);
                if (result.MessageType == WebSocketMessageType.Close) return;
                message.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            var listing = JsonSerializer.Deserialize<RegistryListResponse>(
                message.ToArray(), RegistryJson.Options);
            if (listing is null) continue;

            Current = listing.Instances.Select(ToRecord).ToList();
            Changed?.Invoke(Current);
        }
    }

    private static InstanceRecord ToRecord(RegisteredInstanceDto dto) => dto.ToRecord();

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        if (_loop is not null)
        {
            try { await _loop.WaitAsync(TimeSpan.FromSeconds(2)); } catch { /* going away anyway */ }
        }
        _cts.Dispose();
    }
}
