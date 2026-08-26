using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace SPLA.Mcp.Client;

/// <summary>Where a connection stands. Read by the UI, and by a proxy tool deciding whether it can
/// run at all.</summary>
public enum McpSessionState
{
    Disconnected,
    Connecting,
    Ready,

    /// <summary>Gave up after repeated failures. Nothing further is attempted until somebody asks —
    /// a person pressing "reconnect", or the settings changing. A server started with a mistyped
    /// command must not retry forever and fill the log with the same line.</summary>
    Failed
}

/// <summary>
/// One conversation with one foreign MCP server: the handshake, the tool list, the calls, and the
/// bookkeeping that turns a stream of frames back into replies to particular requests.
///
/// <para><b>Capabilities we declare: none.</b> Not <c>sampling</c>, not <c>elicitation</c>, not
/// <c>roots</c>. A well-behaved server therefore never asks, and one that asks anyway is answered
/// with <c>-32601</c> — a protocol-shaped refusal it can act on, rather than silence it would wait
/// out. Wave one deliberately stops here: the two-way channel those features need is one thing that
/// arrives once, whole, with the turn pump (ADR_20260826_service_mcp-client §2).</para>
/// </summary>
public sealed class McpServerSession : IAsyncDisposable
{
    private const int MaxConsecutiveFailures = 5;

    private readonly McpServerSpec _spec;
    private readonly Func<McpServerSpec, IMcpTransport> _transportFactory;
    private readonly ILogger? _logger;

    /// <summary>Requests we are waiting on, by id. A reply that matches nothing here is a reply to a
    /// call that was already cancelled or timed out — dropped, not an error.</summary>
    private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonNode>> _pending = new();

    /// <summary>Progress callbacks by the token we minted for the call. Removed when the call ends,
    /// so a server that keeps reporting after answering reports into nothing.</summary>
    private readonly ConcurrentDictionary<string, Action<McpProgress>> _progress = new();

    private readonly SemaphoreSlim _connectGate = new(1, 1);
    private readonly CancellationTokenSource _disposing = new();

    private IMcpTransport? _transport;
    private long _nextRequestId;
    private int _consecutiveFailures;
    private volatile McpSessionState _state = McpSessionState.Disconnected;

    public McpServerSession(
        McpServerSpec spec,
        Func<McpServerSpec, IMcpTransport>? transportFactory = null,
        ILogger? logger = null)
    {
        _spec = spec;
        _logger = logger;
        _transportFactory = transportFactory ?? DefaultTransport;
    }

    public string Id => _spec.Id;
    public McpSessionState State => _state;

    /// <summary>What the last attempt failed with, for the UI to show instead of a bare red dot.</summary>
    public string? LastError { get; private set; }

    /// <summary>What the server called itself in the handshake.</summary>
    public string? ServerName { get; private set; }

    /// <summary>The tools this server offered, in its own vocabulary. Replaced wholesale on every
    /// list — a server that dropped a tool must not leave it behind here.</summary>
    public IReadOnlyList<McpToolInfo> Tools { get; private set; } = [];

    /// <summary>Raised whenever <see cref="State"/> changes, including into <see cref="McpSessionState.Failed"/>.</summary>
    public event Action<McpServerSession>? StateChanged;

    /// <summary>Raised when <see cref="Tools"/> has been replaced — after a connect, and after the
    /// server said its list changed. Whoever registers tools with the host listens here.</summary>
    public event Action<McpServerSession>? ToolsChanged;

    private IMcpTransport DefaultTransport(McpServerSpec spec) => spec.Transport switch
    {
        McpTransportKind.Stdio => new StdioTransport(spec, _logger),
        McpTransportKind.Http => new HttpTransport(spec, _logger),
        _ => throw new NotSupportedException($"Unknown MCP transport for server '{spec.Id}'.")
    };

    /// <summary>
    /// Opens the connection and reads the tool list. Safe to call again on a session that is already
    /// connected — it returns immediately — and on one that has failed, which is what a person
    /// pressing "reconnect" does.
    /// </summary>
    public async Task ConnectAsync(CancellationToken ct = default)
    {
        await _connectGate.WaitAsync(ct);
        try
        {
            if (_state == McpSessionState.Ready) return;

            await TearDownTransportAsync();
            SetState(McpSessionState.Connecting);

            var transport = _transportFactory(_spec);
            transport.FrameReceived += OnFrame;
            transport.Closed += OnClosed;
            _transport = transport;

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct, _disposing.Token);
            timeout.CancelAfter(_spec.Timeout);

            await transport.StartAsync(timeout.Token);
            await HandshakeAsync(transport, timeout.Token);
            await RefreshToolsAsync(timeout.Token);

            _consecutiveFailures = 0;
            LastError = null;
            SetState(McpSessionState.Ready);
            _logger?.LogInformation(
                "MCP server ready. Server={ServerId} Name={ServerName} Tools={ToolCount}",
                _spec.Id, ServerName, Tools.Count);
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            await TearDownTransportAsync();
            SetState(McpSessionState.Failed);
            _logger?.LogWarning(ex, "Could not connect to MCP server. Server={ServerId}", _spec.Id);
            throw;
        }
        finally
        {
            _connectGate.Release();
        }
    }

    private async Task HandshakeAsync(IMcpTransport transport, CancellationToken ct)
    {
        var reply = await RequestAsync("initialize", new JsonObject
        {
            ["protocolVersion"] = JsonRpc.ProtocolVersion,
            // Empty on purpose — see the class remarks. An empty object rather than an absent key:
            // "I support nothing" is a statement, "I did not say" invites a server to guess.
            ["capabilities"] = new JsonObject(),
            ["clientInfo"] = new JsonObject
            {
                ["name"] = "spla",
                ["version"] = typeof(McpServerSession).Assembly.GetName().Version?.ToString() ?? "0"
            }
        }, ct);

        ServerName = reply["serverInfo"]?["name"]?.GetValue<string>();

        var serverVersion = reply["protocolVersion"]?.GetValue<string>();
        if (serverVersion is not null && serverVersion != JsonRpc.ProtocolVersion)
            // Logged, never refused. A server that is merely newer than us still answers
            // tools/list and tools/call; failing the connection over a version string would rule
            // out working servers for a difference we have not actually hit yet.
            _logger?.LogInformation(
                "MCP protocol version differs. Server={ServerId} Ours={Ours} Theirs={Theirs}",
                _spec.Id, JsonRpc.ProtocolVersion, serverVersion);

        if (transport is HttpTransport http) http.RememberNegotiatedVersion(serverVersion);

        await transport.SendAsync(JsonRpc.Notification("notifications/initialized"), ct);
    }

    /// <summary>Re-reads the server's tool list and announces it. Called after the handshake and
    /// whenever the server says the list changed.</summary>
    public async Task RefreshToolsAsync(CancellationToken ct = default)
    {
        var reply = await RequestAsync("tools/list", null, ct);

        var tools = new List<McpToolInfo>();
        if (reply["tools"] is JsonArray array)
            foreach (var entry in array)
                if (entry is not null)
                {
                    var tool = McpToolInfo.FromJson(entry);
                    if (!string.IsNullOrWhiteSpace(tool.Name)) tools.Add(tool);
                }

        Tools = tools;
        Raise(ToolsChanged);
    }

    /// <summary>
    /// Runs one tool and returns the server's <c>result</c> object verbatim. Mapping it onto a
    /// <c>ToolResult</c> — content blocks, <c>isError</c>, images, resources — belongs to the proxy
    /// tool above (step 3); this layer stays a transcript.
    /// </summary>
    /// <param name="onProgress">Called for every progress notification the server sends about this
    /// call. Passing null asks for no progress token at all, so a server that would report says
    /// nothing instead of reporting into a void.</param>
    public async Task<JsonNode> CallToolAsync(
        string toolName,
        string argumentsJson,
        Action<McpProgress>? onProgress = null,
        CancellationToken ct = default)
    {
        if (_state != McpSessionState.Ready)
            throw new InvalidOperationException(
                $"MCP server '{_spec.Id}' is {_state.ToString().ToLowerInvariant()}" +
                (LastError is null ? "." : $": {LastError}"));

        JsonNode? arguments;
        try
        {
            arguments = string.IsNullOrWhiteSpace(argumentsJson) ? new JsonObject() : JsonNode.Parse(argumentsJson);
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new ArgumentException($"Arguments for '{toolName}' are not valid JSON.", nameof(argumentsJson), ex);
        }

        var parameters = new JsonObject
        {
            ["name"] = toolName,
            ["arguments"] = arguments ?? new JsonObject()
        };

        return await RequestAsync("tools/call", parameters, ct, onProgress);
    }

    /// <summary>Sends a request and waits for the reply with the matching id. Every outgoing request
    /// in this class goes through here, so correlation, timeout and cleanup are written once.</summary>
    private async Task<JsonNode> RequestAsync(
        string method,
        JsonObject? parameters,
        CancellationToken ct,
        Action<McpProgress>? onProgress = null)
    {
        var transport = _transport ?? throw new InvalidOperationException(
            $"MCP server '{_spec.Id}' has no open transport.");

        var id = Interlocked.Increment(ref _nextRequestId);
        var completion = new TaskCompletionSource<JsonNode>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[id] = completion;

        string? progressToken = null;
        if (onProgress is not null)
        {
            progressToken = $"{_spec.Id}:{id}";
            _progress[progressToken] = onProgress;
            (parameters ??= [])["_meta"] = new JsonObject { ["progressToken"] = progressToken };
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct, _disposing.Token);
        timeout.CancelAfter(_spec.Timeout);

        try
        {
            await transport.SendAsync(JsonRpc.Request(id, method, parameters), timeout.Token);
            return await completion.Task.WaitAsync(timeout.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested && !_disposing.IsCancellationRequested)
        {
            // Our own deadline, not the caller's cancellation and not shutdown. Told apart because
            // the three are the same exception type and mean very different things to whoever is
            // waiting — and because a timeout must read as a timeout in the log.
            throw new TimeoutException(
                $"MCP server '{_spec.Id}' did not answer '{method}' within {_spec.Timeout.TotalSeconds:0}s.");
        }
        finally
        {
            _pending.TryRemove(id, out _);
            if (progressToken is not null) _progress.TryRemove(progressToken, out _);
        }
    }

    /// <summary>Everything the server said, sorted into the three things it can be. Runs on the
    /// transport's reader thread and must never throw — the transport logs and carries on, but a
    /// handler that throws on every frame is a connection that delivers nothing.</summary>
    private void OnFrame(JsonNode frame)
    {
        if (JsonRpc.IsServerRequest(frame))
        {
            _ = RefuseServerRequestAsync(frame);
            return;
        }

        if (JsonRpc.IsNotification(frame))
        {
            OnNotification(frame);
            return;
        }

        // A reply. Ids are ours and always numbers, so anything else is a frame for somebody else.
        long id;
        try { id = frame["id"]!.GetValue<long>(); }
        catch { return; }

        if (!_pending.TryRemove(id, out var completion)) return;   // already timed out or cancelled

        if (frame["error"] is { } error)
        {
            var code = error["code"]?.GetValue<int>() ?? 0;
            var message = error["message"]?.GetValue<string>() ?? "no message";
            completion.TrySetException(new McpServerException(code, message));
            return;
        }

        completion.TrySetResult(frame["result"]?.DeepClone() ?? new JsonObject());
    }

    private void OnNotification(JsonNode frame)
    {
        switch (frame["method"]?.GetValue<string>())
        {
            case "notifications/progress":
            {
                var parameters = frame["params"];
                var token = parameters?["progressToken"]?.ToJsonString().Trim('"');
                if (token is null || !_progress.TryGetValue(token, out var report)) return;

                try
                {
                    report(new McpProgress(
                        parameters?["progress"]?.GetValue<double>() ?? 0,
                        TryDouble(parameters?["total"]),
                        parameters?["message"]?.GetValue<string>()));
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "Progress handler threw. Server={ServerId}", _spec.Id);
                }
                return;
            }

            case "notifications/tools/list_changed":
                // Off this thread: re-listing is a round trip, and doing it here would block the
                // reader that the reply has to come back through — a deadlock, not a slowdown.
                _ = Task.Run(async () =>
                {
                    try { await RefreshToolsAsync(_disposing.Token); }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Could not re-read tools. Server={ServerId}", _spec.Id);
                    }
                });
                return;
        }
    }

    /// <summary>
    /// The answer to <c>elicitation/create</c>, <c>sampling/createMessage</c>, <c>roots/list</c> and
    /// anything else a server asks of us in wave one.
    ///
    /// <para>Logged at Warning rather than Debug, and that is the point of the method: this line is
    /// the only evidence that a server wanted something we cannot give, and it is what decides
    /// whether the second wave is worth building.</para>
    /// </summary>
    private async Task RefuseServerRequestAsync(JsonNode frame)
    {
        var method = frame["method"]?.GetValue<string>() ?? "(none)";
        _logger?.LogWarning(
            "MCP server asked for something we do not support and was refused. Server={ServerId} Method={Method}",
            _spec.Id, method);

        try
        {
            var transport = _transport;
            if (transport is null) return;
            await transport.SendAsync(
                JsonRpc.ErrorReply(frame["id"], JsonRpc.MethodNotFound,
                    $"spla does not implement '{method}'."),
                _disposing.Token);
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "Could not refuse server request. Server={ServerId}", _spec.Id);
        }
    }

    private void OnClosed(Exception? cause)
    {
        if (_disposing.IsCancellationRequested) return;

        LastError = cause?.Message;
        FailPending(cause);
        SetState(McpSessionState.Disconnected);

        _ = Task.Run(ReconnectAsync);
    }

    /// <summary>Nothing that was waiting can ever be answered now — the channel it would have come
    /// back through is gone. Failing them here is what turns "the server died" into an error the
    /// caller sees, instead of a call that hangs until its own deadline.</summary>
    private void FailPending(Exception? cause)
    {
        foreach (var id in _pending.Keys)
            if (_pending.TryRemove(id, out var completion))
                completion.TrySetException(cause
                    ?? new IOException($"MCP server '{_spec.Id}' closed the connection."));
        _progress.Clear();
    }

    private async Task ReconnectAsync()
    {
        while (!_disposing.IsCancellationRequested)
        {
            if (_consecutiveFailures >= MaxConsecutiveFailures)
            {
                SetState(McpSessionState.Failed);
                _logger?.LogWarning(
                    "Giving up on MCP server after {Attempts} attempts; reconnect manually. Server={ServerId}",
                    _consecutiveFailures, _spec.Id);
                return;
            }

            // 1, 2, 4, 8, 16 seconds. Bounded by the attempt cap above rather than by a ceiling on
            // the delay: a server that is going to come back does so early, and one that is not
            // should stop being retried rather than be retried slowly forever.
            var delay = TimeSpan.FromSeconds(Math.Pow(2, _consecutiveFailures));
            try { await Task.Delay(delay, _disposing.Token); }
            catch (OperationCanceledException) { return; }

            _consecutiveFailures++;
            try
            {
                await ConnectAsync(_disposing.Token);
                return;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Reconnect attempt failed. Server={ServerId} Attempt={Attempt}",
                    _spec.Id, _consecutiveFailures);
            }
        }
    }

    private void SetState(McpSessionState state)
    {
        if (_state == state) return;
        _state = state;
        Raise(StateChanged);
    }

    private void Raise(Action<McpServerSession>? handler)
    {
        try { handler?.Invoke(this); }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "MCP session event handler threw. Server={ServerId}", _spec.Id);
        }
    }

    private async Task TearDownTransportAsync()
    {
        var transport = _transport;
        _transport = null;
        if (transport is null) return;

        transport.FrameReceived -= OnFrame;
        transport.Closed -= OnClosed;
        try { await transport.DisposeAsync(); }
        catch (Exception ex) { _logger?.LogDebug(ex, "Transport dispose threw. Server={ServerId}", _spec.Id); }
    }

    public async ValueTask DisposeAsync()
    {
        await _disposing.CancelAsync();
        FailPending(new ObjectDisposedException(nameof(McpServerSession)));
        await TearDownTransportAsync();
        _state = McpSessionState.Disconnected;
        _connectGate.Dispose();
        _disposing.Dispose();
    }

    private static double? TryDouble(JsonNode? node)
    {
        try { return node?.GetValue<double>(); }
        catch { return null; }
    }
}
