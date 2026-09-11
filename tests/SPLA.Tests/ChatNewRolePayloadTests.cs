using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Project;
using SPLA.Runtime;
using SPLA.Service;
using SPLA.Service.Contracts;

namespace SPLA.Tests;

/// <summary>
/// PLAN_20260911 wave 1.1: <c>chat.new</c> accepts an optional <c>Role</c>, validated case-insensitively
/// against the project manifest's declared <c>roles:</c> list — the same source
/// <c>agent_spawn</c>/<c>SpawnedAgentRunner.GetAvailableRoles</c> uses. A known role stamps <c>as:</c>
/// on the new chat before it is ever opened; an unknown one refuses the whole request, chat count
/// unchanged, and names what was available.
///
/// <para>Real end-to-end over a WebSocket, same harness as <see cref="MultiProjectProtocolTests"/> —
/// the point being proven is the wire round trip (<c>ChatHandlers.New</c>), not <c>ChatRegistry.CreateNew</c>
/// in isolation (that is already covered by <c>ChatRoleNarrowingTests</c>).</para>
/// </summary>
public sealed class ChatNewRolePayloadTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-chatnewrole-{Guid.NewGuid():N}")).FullName;

    private static int FreePort()
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    /// <summary>Writes a manifest declaring <paramref name="roles"/> and registers it, exactly as
    /// <c>ChatRoleNarrowingTests.BuildProject</c> does for the non-wire path.</summary>
    private static (string Root, string Manifest) BuildProject(params string[] roles)
    {
        var root = TempRoot();
        var manifest = Path.Combine(root, "test.spla");
        File.WriteAllText(manifest, $"""
            version: 1
            name: ChatNewRoleTest
            workspace: .
            agent:
              mode: Edit
            roles: [{string.Join(", ", roles)}]
            """);
        return (root, manifest);
    }

    [Fact]
    public async Task Chat_new_with_a_known_role_stamps_as_on_the_chat_and_its_summary()
    {
        var (root, manifest) = BuildProject("scout");
        try
        {
            var provider = new LocalProjectProvider(Path.Combine(root, "state"));
            using var registry = new AgentRuntimeRegistry(NullLoggerFactory.Instance, provider);
            registry.Create(new ProjectDescriptor { Id = manifest, ManifestPath = manifest, Name = "Test" });
            registry.DefaultProjectId = manifest;

            var port = FreePort();
            var host = SplaServiceHost.Build(registry, new ServiceOptions { Port = port });
            await host.StartAsync();
            try
            {
                using var socket = new ClientWebSocket();
                await socket.ConnectAsync(new Uri($"ws://127.0.0.1:{port}/ws"), CancellationToken.None);
                await SendAsync(socket, MessageTypes.Hello, new HelloPayload());
                await ReceiveAsync<WelcomePayload>(socket, MessageTypes.Welcome);

                // Case-insensitive on purpose — proves the match, not just an exact echo.
                await SendAsync(socket, MessageTypes.ChatNew, new ChatNewPayload { Title = "Scouted", Role = "SCOUT" });
                var opened = await ReceiveAsync<ChatOpenedPayload>(socket, MessageTypes.ChatOpened);
                Assert.Equal("Scouted", opened.Title);

                await SendAsync(socket, MessageTypes.ChatList, null, requestId: "list");
                var list = await ReceiveAsync<ChatListResultPayload>(socket, MessageTypes.ChatListResult, "list");
                var summary = Assert.Single(list.Chats);
                Assert.Equal("scout", summary.As);

                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
            }
            finally { await host.StopAsync(); }
        }
        finally { try { Directory.Delete(root, recursive: true); } catch { } }
    }

    [Fact]
    public async Task Chat_new_with_an_unknown_role_is_refused_and_creates_no_chat()
    {
        var (root, manifest) = BuildProject("scout", "planner");
        try
        {
            var provider = new LocalProjectProvider(Path.Combine(root, "state"));
            using var registry = new AgentRuntimeRegistry(NullLoggerFactory.Instance, provider);
            registry.Create(new ProjectDescriptor { Id = manifest, ManifestPath = manifest, Name = "Test" });
            registry.DefaultProjectId = manifest;

            var port = FreePort();
            var host = SplaServiceHost.Build(registry, new ServiceOptions { Port = port });
            await host.StartAsync();
            try
            {
                using var socket = new ClientWebSocket();
                await socket.ConnectAsync(new Uri($"ws://127.0.0.1:{port}/ws"), CancellationToken.None);
                await SendAsync(socket, MessageTypes.Hello, new HelloPayload());
                await ReceiveAsync<WelcomePayload>(socket, MessageTypes.Welcome);

                await SendAsync(socket, MessageTypes.ChatNew, new ChatNewPayload { Title = "Ghost", Role = "nope" });
                var error = await ReceiveAsync<ErrorPayload>(socket, MessageTypes.Error);
                Assert.Contains("nope", error.Message);
                Assert.Contains("scout", error.Message);
                Assert.Contains("planner", error.Message);

                await SendAsync(socket, MessageTypes.ChatList, null, requestId: "list");
                var list = await ReceiveAsync<ChatListResultPayload>(socket, MessageTypes.ChatListResult, "list");
                Assert.Empty(list.Chats);

                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
            }
            finally { await host.StopAsync(); }
        }
        finally { try { Directory.Delete(root, recursive: true); } catch { } }
    }

    private static async Task SendAsync(
        ClientWebSocket socket, string type, object? payload, string? requestId = null)
    {
        var env = new ProtocolEnvelope
        {
            Type = type,
            RequestId = requestId,
            Payload = payload == null ? null : JsonSerializer.SerializeToElement(payload, Json)
        };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(env, Json);
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private static async Task<T> ReceiveAsync<T>(ClientWebSocket socket, string expectedType, string? expectedRequestId = null)
    {
        var buffer = new byte[64 * 1024];
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        while (true)
        {
            var result = await socket.ReceiveAsync(buffer, cts.Token);
            var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var env = JsonSerializer.Deserialize<ProtocolEnvelope>(text, Json)!;
            if (env.Type == expectedType && (expectedRequestId == null || env.RequestId == expectedRequestId))
                return env.Payload!.Value.Deserialize<T>(Json)!;
        }
    }
}
