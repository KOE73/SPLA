using SPLA.Runtime;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Project;
using SPLA.Service;
using SPLA.Service.Contracts;

namespace SPLA.Tests;

/// <summary>
/// Phase 2.2 (docs/host-abstraction-plan.md), variant B: every chat/project-scoped envelope carries
/// its own ProjectId (null = the connection's default project); the server keeps no "current
/// project" state on the socket. This is the end-to-end proof the earlier unit tests couldn't give:
/// a REAL WebSocket connection touching two different projects gets genuinely isolated results —
/// not just that the registry object graph looks right in isolation.
/// </summary>
public sealed class MultiProjectProtocolTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-ws-{Guid.NewGuid():N}")).FullName;

    /// <summary>Grabs a free loopback TCP port from the OS (bind to port 0, read the assigned port,
    /// release it) so the suite never collides with whatever else is listening on this machine —
    /// license daemons, another `spla serve`, a previous run's lingering socket. There is a tiny race
    /// between releasing the port here and the host binding it, but on loopback in a test it is
    /// effectively never lost, and a fresh port per test keeps runs independent.</summary>
    private static int FreePort()
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    [Fact]
    public async Task Two_projects_over_one_socket_stay_isolated()
    {
        var root = TempRoot();
        try
        {
            var provider = new LocalProjectProvider(Path.Combine(root, "state"));
            var alphaManifest = Path.Combine(root, "alpha", "alpha.spla");
            var betaManifest = Path.Combine(root, "beta", "beta.spla");

            using var registry = new AgentRuntimeRegistry(NullLoggerFactory.Instance, provider);
            registry.Create(new ProjectDescriptor { Id = alphaManifest, ManifestPath = alphaManifest, Name = "Alpha" });
            registry.Create(new ProjectDescriptor { Id = betaManifest, ManifestPath = betaManifest, Name = "Beta" });
            registry.DefaultProjectId = alphaManifest;

            var port = FreePort();
            var host = SplaServiceHost.Build(registry, new ServiceOptions { Port = port });
            await host.StartAsync();
            try
            {
                using var socket = new ClientWebSocket();
                await socket.ConnectAsync(new Uri($"ws://127.0.0.1:{port}/ws"), CancellationToken.None);

                await SendAsync(socket, MessageTypes.Hello, new HelloPayload());
                var welcome = await ReceiveAsync<WelcomePayload>(socket, MessageTypes.Welcome);
                Assert.Equal(alphaManifest, welcome.ProjectId);
                Assert.Equal("Alpha", welcome.ProjectName);

                // Create a chat explicitly in Beta (not the default project).
                await SendAsync(socket, MessageTypes.ChatNew, new ChatNewPayload { Title = "Beta chat" }, projectId: betaManifest);
                var opened = await ReceiveAsync<ChatOpenedPayload>(socket, MessageTypes.ChatOpened);
                Assert.Equal("Beta chat", opened.Title);

                // chat.new also broadcasts an unsolicited chat.list.result to this project's watchers
                // (sidebar auto-refresh) — correlate by RequestId so that broadcast noise can't be
                // mistaken for the reply to an explicit query below.
                const string betaReqId = "beta-list";
                const string alphaReqId = "alpha-list";

                // Beta's list shows the new chat.
                await SendAsync(socket, MessageTypes.ChatList, null, projectId: betaManifest, requestId: betaReqId);
                var betaList = await ReceiveAsync<ChatListResultPayload>(socket, MessageTypes.ChatListResult, betaReqId);
                Assert.Single(betaList.Chats);

                // The default project (Alpha, ProjectId omitted) was never touched — still empty.
                await SendAsync(socket, MessageTypes.ChatList, null, requestId: alphaReqId);
                var alphaList = await ReceiveAsync<ChatListResultPayload>(socket, MessageTypes.ChatListResult, alphaReqId);
                Assert.Empty(alphaList.Chats);

                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
            }
            finally
            {
                await host.StopAsync();
            }
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Server_mode_lands_a_user_in_their_own_area()
    {
        var root = TempRoot();
        try
        {
            // Registry provider rooted in temp so the test never touches the real ~/.spla registry.
            var provider = new LocalProjectProvider(Path.Combine(root, "state"));
            using var registry = new AgentRuntimeRegistry(NullLoggerFactory.Instance, provider);

            // Server mode: a per-user root. Auth is off here, so the connection's identity is the
            // implicit single local user (UserKey "local") — enough to prove routing without Negotiate.
            var serverRoot = new ServerProjectRoot(Path.Combine(root, "srv"));
            var port = FreePort();
            var host = SplaServiceHost.Build(registry, new ServiceOptions { Port = port }, null, serverRoot);
            await host.StartAsync();
            try
            {
                using var socket = new ClientWebSocket();
                await socket.ConnectAsync(new Uri($"ws://127.0.0.1:{port}/ws"), CancellationToken.None);

                await SendAsync(socket, MessageTypes.Hello, new HelloPayload());
                var welcome = await ReceiveAsync<WelcomePayload>(socket, MessageTypes.Welcome);

                // Landed in the user's OWN default project, auto-provisioned under their area — NOT a
                // shared or server-owned project. This is the fix for "fell into the server's projects".
                var expected = Path.Combine(root, "srv", "users", "local", "default.spla");
                Assert.Equal(expected, welcome.ProjectId);
                Assert.True(File.Exists(expected));

                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
            }
            finally { await host.StopAsync(); }
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Negotiate_login_issues_cookie_then_ws_authenticates_and_routes_to_user_area()
    {
        var root = TempRoot();
        try
        {
            var provider = new LocalProjectProvider(Path.Combine(root, "state"));
            using var registry = new AgentRuntimeRegistry(NullLoggerFactory.Instance, provider);
            var serverRoot = new ServerProjectRoot(Path.Combine(root, "srv"));
            var port = FreePort();
            var host = SplaServiceHost.Build(registry,
                new ServiceOptions { Port = port, RequireAuthentication = true }, null, serverRoot);
            await host.StartAsync();
            try
            {
                // 1) A normal HTTP GET does the Negotiate handshake (loopback → the current domain user)
                //    and comes back with the auth cookie — exactly what the browser does on page load.
                var cookies = new CookieContainer();
                using (var handler = new HttpClientHandler
                       {
                           Credentials = CredentialCache.DefaultCredentials,
                           AllowAutoRedirect = true,
                           CookieContainer = cookies,
                           PreAuthenticate = true
                       })
                using (var http = new HttpClient(handler))
                {
                    // Hit /login directly so Negotiate runs on the first request (loopback → current
                    // domain user); it signs the cookie and 302s back to / which returns 200.
                    var page = await http.GetAsync($"http://127.0.0.1:{port}/login");
                    Assert.Equal(HttpStatusCode.OK, page.StatusCode);
                }

                // 2) The WebSocket carries that cookie (as a browser would) — no Negotiate on the WS.
                using var socket = new ClientWebSocket();
                socket.Options.Cookies = cookies;
                await socket.ConnectAsync(new Uri($"ws://127.0.0.1:{port}/ws"), CancellationToken.None);

                await SendAsync(socket, MessageTypes.Hello, new HelloPayload());
                var welcome = await ReceiveAsync<WelcomePayload>(socket, MessageTypes.Welcome);

                // The real domain user was resolved and routed into their OWN provisioned area.
                Assert.False(string.IsNullOrEmpty(welcome.UserName));
                Assert.Contains(Path.Combine("users"), welcome.ProjectId);
                Assert.True(File.Exists(welcome.ProjectId));

                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
            }
            finally { await host.StopAsync(); }
        }
        finally { try { Directory.Delete(root, recursive: true); } catch { } }
    }

    [Fact]
    public async Task Server_project_create_lands_in_user_area_lists_and_is_empty()
    {
        var root = TempRoot();
        try
        {
            var provider = new LocalProjectProvider(Path.Combine(root, "state"));
            using var registry = new AgentRuntimeRegistry(NullLoggerFactory.Instance, provider);
            var serverRoot = new ServerProjectRoot(Path.Combine(root, "srv"));
            var port = FreePort();
            var host = SplaServiceHost.Build(registry, new ServiceOptions { Port = port }, null, serverRoot);
            await host.StartAsync();
            try
            {
                using var socket = new ClientWebSocket();
                await socket.ConnectAsync(new Uri($"ws://127.0.0.1:{port}/ws"), CancellationToken.None);
                await SendAsync(socket, MessageTypes.Hello, new HelloPayload());
                await ReceiveAsync<WelcomePayload>(socket, MessageTypes.Welcome);

                // Create by NAME only (no path) — the server places it in the user's own area.
                await SendAsync(socket, MessageTypes.ProjectCreate, new ProjectCreatePayload { Name = "Demo" }, requestId: "c1");
                var ctx = await ReceiveAsync<ProjectContextPayload>(socket, MessageTypes.ProjectContext, "c1");

                var expected = Path.Combine(root, "srv", "users", "local", "Demo", "Demo.spla");
                Assert.Equal(Path.GetFullPath(expected), Path.GetFullPath(ctx.ProjectId));
                Assert.True(File.Exists(ctx.ProjectId));

                // Shows up in the user's project list — the "no projects after refresh" bug.
                await SendAsync(socket, MessageTypes.ProjectList, null, requestId: "l1");
                var list = await ReceiveAsync<ProjectListResultPayload>(socket, MessageTypes.ProjectListResult, "l1");
                Assert.Contains(list.Projects, p => Path.GetFullPath(p.Id) == Path.GetFullPath(ctx.ProjectId));

                // Its chats are ITS OWN (empty) — not another project's ("strange foreign chats" bug).
                await SendAsync(socket, MessageTypes.ChatList, null, projectId: ctx.ProjectId, requestId: "h1");
                var chats = await ReceiveAsync<ChatListResultPayload>(socket, MessageTypes.ChatListResult, "h1");
                Assert.Empty(chats.Chats);

                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
            }
            finally { await host.StopAsync(); }
        }
        finally { try { Directory.Delete(root, recursive: true); } catch { } }
    }

    private static async Task SendAsync(
        ClientWebSocket socket, string type, object? payload, string? projectId = null, string? requestId = null)
    {
        var env = new ProtocolEnvelope
        {
            Type = type,
            ProjectId = projectId,
            RequestId = requestId,
            Payload = payload == null ? null : JsonSerializer.SerializeToElement(payload, Json)
        };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(env, Json);
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    /// <summary>Reads envelopes until one matches <paramref name="expectedType"/> (and, when given,
    /// <paramref name="expectedRequestId"/>) — skips broadcasts this call isn't waiting on, e.g. the
    /// sidebar-refresh chat.list.result a chat.new triggers for every watcher of that project.</summary>
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
