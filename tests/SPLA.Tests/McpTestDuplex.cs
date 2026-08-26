using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using SPLA.Mcp.Client;

namespace SPLA.Tests;

/// <summary>
/// The wire, in memory: two line pipes, one per direction, so a real <see cref="McpServerSession"/>
/// can sit at one end and either our own <c>McpStdioServer</c> or a hand-scripted counterpart can sit
/// at the other — nothing spawned, no port opened. Shared by <c>McpClientSessionTests</c> and
/// <c>McpProxyToolTests</c>, which is the whole reason it lives in its own file rather than as a
/// private nested class of either.
/// </summary>
internal static class McpTestDuplex
{
    /// <summary>One direction of a line-oriented connection. Both ends of MCP over stdio are already
    /// <see cref="TextReader"/>/<see cref="TextWriter"/>, so nothing here descends to bytes.</summary>
    public sealed class LinePipe
    {
        private readonly Channel<string> _lines = Channel.CreateUnbounded<string>();

        public TextReader Reader { get; }
        public TextWriter Writer { get; }

        public LinePipe()
        {
            Reader = new ChannelReaderText(_lines.Reader);
            Writer = new ChannelWriterText(_lines.Writer);
        }

        /// <summary>Ends the stream — what a closed pipe or a dead process looks like to the reader
        /// on the other side.</summary>
        public void Close() => _lines.Writer.TryComplete();

        private sealed class ChannelReaderText(ChannelReader<string> lines) : TextReader
        {
            public override async ValueTask<string?> ReadLineAsync(CancellationToken ct)
            {
                try { return await lines.ReadAsync(ct); }
                catch (ChannelClosedException) { return null; }
            }

            public override Task<string?> ReadLineAsync() => ReadLineAsync(default).AsTask();
        }

        /// <summary>Buffers until a newline, then publishes one line. Callers reach a
        /// <see cref="TextWriter"/> through half a dozen overloads and only the newline is common to
        /// all of them.</summary>
        private sealed class ChannelWriterText(ChannelWriter<string> lines) : TextWriter
        {
            private readonly StringBuilder _buffer = new();

            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(char value)
            {
                lock (_buffer)
                {
                    if (value != '\n') { _buffer.Append(value); return; }
                    var line = _buffer.ToString().TrimEnd('\r');
                    _buffer.Clear();
                    if (line.Length > 0) lines.TryWrite(line);
                }
            }

            public override void Write(string? value)
            {
                if (value is null) return;
                foreach (var c in value) Write(c);
            }

            public override Task WriteLineAsync(char[] buffer, int index, int count)
            {
                Write(new string(buffer, index, count));
                Write('\n');
                return Task.CompletedTask;
            }

            public override Task WriteLineAsync(string? value)
            {
                Write(value);
                Write('\n');
                return Task.CompletedTask;
            }

            public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken ct = default)
            {
                Write(buffer.ToString());
                Write('\n');
                return Task.CompletedTask;
            }

            public override Task WriteAsync(string? value) { Write(value); return Task.CompletedTask; }

            public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken ct = default)
            {
                Write(buffer.ToString());
                return Task.CompletedTask;
            }

            public override Task FlushAsync() => Task.CompletedTask;
        }
    }

    /// <summary>Builds a session whose transport is one end of a duplex pair, and hands back the
    /// other end for whoever plays the server.</summary>
    public static (McpServerSession Session, LinePipe ToServer, LinePipe ToClient) Session(
        string serverId = "probe", TimeSpan? timeout = null)
    {
        var toServer = new LinePipe();
        var toClient = new LinePipe();

        var spec = new McpServerSpec(serverId, McpTransportKind.Stdio)
        {
            Timeout = timeout ?? TimeSpan.FromSeconds(10)
        };

        var session = new McpServerSession(
            spec,
            _ => new StreamTransport(toClient.Reader, toServer.Writer, serverId));

        return (session, toServer, toClient);
    }

    /// <summary>Plays a server by hand: reads what the client sent and writes back whatever the test
    /// says. Exists for the things our own server never does to us — sending a request of its own,
    /// reporting unsolicited progress, hanging up mid-call.</summary>
    public sealed class ScriptedServer(LinePipe fromClient, LinePipe toClient)
    {
        public List<JsonNode> Received { get; } = [];

        public async Task<JsonNode> ReadAsync(CancellationToken ct = default)
        {
            var line = await fromClient.Reader.ReadLineAsync(ct) ?? throw new IOException("client closed");
            var frame = JsonNode.Parse(line)!;
            Received.Add(frame);
            return frame;
        }

        public void Send(JsonObject frame) => toClient.Writer.WriteLine(frame.ToJsonString());

        public void Reply(JsonNode request, JsonObject result) => Send(new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = request["id"]!.DeepClone(),
            ["result"] = result
        });

        /// <summary>Answers initialize and tools/list so a test can get straight to the interesting
        /// part. <paramref name="tools"/> lets a caller hand back a specific tool list.</summary>
        public async Task HandshakeAsync(JsonArray? tools = null, CancellationToken ct = default)
        {
            Reply(await ReadAsync(ct), new JsonObject
            {
                ["protocolVersion"] = "2025-06-18",
                ["capabilities"] = new JsonObject(),
                ["serverInfo"] = new JsonObject { ["name"] = "scripted" }
            });

            await ReadAsync(ct);   // notifications/initialized — nothing to answer

            Reply(await ReadAsync(ct), new JsonObject
            {
                ["tools"] = tools ?? new JsonArray(new JsonObject
                {
                    ["name"] = "slow",
                    ["description"] = "takes a while",
                    ["inputSchema"] = new JsonObject { ["type"] = "object" }
                })
            });
        }
    }
}
