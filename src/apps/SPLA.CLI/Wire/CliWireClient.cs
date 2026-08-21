using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using SPLA.Service.Contracts;

namespace SPLA.CLI.Wire;

/// <summary>
/// A console-shaped client for the same WebSocket protocol the windows speak.
///
/// <para>It exists because the alternative is worse. A project may have only one writer, so a
/// <c>chat run</c> started while a window (or another run) already holds the project cannot simply
/// open its own runtime — and refusing outright would break the most ordinary thing anyone does:
/// leave the app open and fire a scripted run at the same project. Connecting instead makes the run
/// appear in the open window rather than racing it for the same files.</para>
///
/// <para>Deliberately small. It knows how to say hello, start a chat, send one message and read the
/// turn back; it is not a second implementation of the web client, and anything it does not
/// understand it ignores rather than guesses at.</para>
/// </summary>
internal sealed class CliWireClient : IAsyncDisposable
{
    private readonly ClientWebSocket _socket;
    private readonly Channel<ProtocolEnvelope> _inbox = Channel.CreateUnbounded<ProtocolEnvelope>();
    private readonly CancellationTokenSource _pumpCts = new();
    private readonly Task _pump;
    private readonly string? _token;

    private CliWireClient(ClientWebSocket socket, string? token)
    {
        _socket = socket;
        _token = token;
        _pump = Task.Run(() => ReceiveLoopAsync(_pumpCts.Token));
    }

    /// <summary>Connects and completes the handshake. <paramref name="baseUrl"/> is the service's HTTP
    /// base (what the instance lock publishes); the socket path is appended here so callers never
    /// carry two spellings of the same address.</summary>
    public static async Task<CliWireClient> ConnectAsync(string baseUrl, string? token, CancellationToken ct)
    {
        var ws = new Uri(baseUrl.TrimEnd('/').Replace("https://", "wss://").Replace("http://", "ws://") + "/ws");
        var socket = new ClientWebSocket();
        await socket.ConnectAsync(ws, ct);

        var client = new CliWireClient(socket, token);
        await client.SendAsync(MessageTypes.Hello, new HelloPayload { ClientName = "spla-cli" }, ct: ct);
        await client.WaitForAsync(MessageTypes.Welcome, ct);
        return client;
    }

    /// <summary>
    /// Sends a request and waits for the correlated <see cref="MessageTypes.InstanceStatusResult"/>
    /// answer — the shape both <see cref="MessageTypes.InstanceStatus"/> and
    /// <see cref="MessageTypes.InstanceStop"/> reply with. General over the two on purpose: `spla ps`
    /// and `spla stop` differ only in what they send, not in how they read the answer back.
    /// </summary>
    public async Task<InstanceStatusPayload?> RequestInstanceStatusAsync(string type, object? payload, CancellationToken ct)
    {
        await SendAsync(type, payload, ct: ct);
        var reply = await WaitForAsync(MessageTypes.InstanceStatusResult, ct);
        return Payload<InstanceStatusPayload>(reply);
    }

    /// <summary>Starts a chat and returns its id.</summary>
    public async Task<string> NewChatAsync(string? title, CancellationToken ct)
    {
        await SendAsync(MessageTypes.ChatNew, new ChatNewPayload { Title = title }, ct: ct);
        var opened = await WaitForAsync(MessageTypes.ChatOpened, ct);
        return Payload<ChatOpenedPayload>(opened)?.ChatId
            ?? throw new InvalidOperationException("The service opened a chat without giving it an id.");
    }

    /// <summary>
    /// Sends one message and reads the turn to its end, reporting as it goes.
    /// </summary>
    /// <param name="onText">Assistant text as it streams.</param>
    /// <param name="onNote">Everything else worth a line on the console — tool calls, notices.</param>
    /// <param name="onPermission">Answers a permission question. The turn is running on the other
    /// side, and the question may equally well be answered in the window that is also watching; the
    /// first answer wins and the loser simply never sees it asked again.</param>
    /// <returns>The turn's error, or null when it finished cleanly.</returns>
    public async Task<string?> SendAndStreamAsync(
        string chatId,
        string text,
        Action<string> onText,
        Action<string> onNote,
        Func<PermissionRequestPayload, PermissionDecisionPayload>? onPermission,
        CancellationToken ct)
    {
        await SendAsync(MessageTypes.ChatSend, new ChatSendPayload { ChatId = chatId, Text = text }, chatId, ct: ct);

        while (true)
        {
            var env = await ReadAsync(ct);
            switch (env.Type)
            {
                case MessageTypes.Delta:
                    if (Payload<DeltaPayload>(env)?.Text is { Length: > 0 } chunk) onText(chunk);
                    break;

                case MessageTypes.ToolStarted:
                    if (Payload<ToolStartedPayload>(env)?.ToolCall?.Name is { Length: > 0 } tool)
                        onNote($"[tool] {tool}");
                    break;

                case MessageTypes.Notice:
                    if (Payload<NoticePayload>(env)?.Text is { Length: > 0 } note) onNote($"[note] {note}");
                    break;

                case MessageTypes.PermissionRequest when onPermission != null && env.RequestId != null:
                    var ask = Payload<PermissionRequestPayload>(env) ?? new PermissionRequestPayload();
                    await SendAsync(MessageTypes.PermissionDecision, onPermission(ask), chatId, env.RequestId, ct);
                    break;

                case MessageTypes.Error:
                    return Payload<ErrorPayload>(env)?.Message ?? "unknown error";

                case MessageTypes.TurnComplete:
                    var done = Payload<TurnCompletePayload>(env);
                    return done?.Error ?? (done?.Cancelled == true ? "cancelled" : null);
            }
        }
    }

    // ── plumbing ──────────────────────────────────────────────────────────────

    private async Task SendAsync(
        string type, object? payload, string? chatId = null, string? requestId = null, CancellationToken ct = default)
    {
        var env = new ProtocolEnvelope
        {
            Type = type,
            ChatId = chatId,
            RequestId = requestId,
            Auth = _token is null ? null : new AuthInfo { Token = _token },
            Payload = payload is null ? null : JsonSerializer.SerializeToElement(payload, ServiceJson.Options)
        };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(env, ServiceJson.Options);
        await _socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
    }

    private async Task<ProtocolEnvelope> ReadAsync(CancellationToken ct) => await _inbox.Reader.ReadAsync(ct);

    /// <summary>Reads until a frame of <paramref name="type"/> arrives, discarding what comes before —
    /// the server is free to volunteer chat lists and health results at any moment, and none of them
    /// are answers to what was just asked.</summary>
    private async Task<ProtocolEnvelope> WaitForAsync(string type, CancellationToken ct)
    {
        while (true)
        {
            var env = await ReadAsync(ct);
            if (env.Type == type) return env;
            if (env.Type == MessageTypes.Error)
                throw new InvalidOperationException(Payload<ErrorPayload>(env)?.Message ?? "the service refused the request");
        }
    }

    private static T? Payload<T>(ProtocolEnvelope env) where T : class
        => env.Payload is null ? null : env.Payload.Value.Deserialize<T>(ServiceJson.Options);

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[64 * 1024];
        var assembled = new MemoryStream();

        try
        {
            while (_socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await _socket.ReceiveAsync(buffer, ct);
                if (result.MessageType == WebSocketMessageType.Close) break;

                assembled.Write(buffer, 0, result.Count);
                if (!result.EndOfMessage) continue;

                var json = Encoding.UTF8.GetString(assembled.ToArray());
                assembled.SetLength(0);

                try
                {
                    if (JsonSerializer.Deserialize<ProtocolEnvelope>(json, ServiceJson.Options) is { } env)
                        await _inbox.Writer.WriteAsync(env, ct);
                }
                catch (JsonException) { /* a frame this client does not understand is not its business */ }
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException) { }
        finally
        {
            _inbox.Writer.TryComplete();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _pumpCts.CancelAsync();
        try
        {
            if (_socket.State == WebSocketState.Open)
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
        }
        catch { /* the other side may already be gone */ }

        try { await _pump; } catch { /* pump cancellation is expected */ }
        _socket.Dispose();
        _pumpCts.Dispose();
    }
}
