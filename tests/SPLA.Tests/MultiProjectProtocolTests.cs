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
/// Phase 2.2 (../../docs/plans/PLAN_20260701_core_host-abstraction.md), variant B: every chat/project-scoped envelope carries
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

    /// <summary>
    /// Two projects are two connections, and neither can see the other's chats.
    ///
    /// <para>This used to be "two projects over one socket stay isolated", and the isolation was
    /// enforced by every message naming its own project. That put the burden on the sender: forget
    /// the field once and a settings write landed in whichever project the connection defaulted to.
    /// Binding the project to the connection removes the class of bug rather than testing for it —
    /// so what is worth proving now is that a second connection really is a second project, and that
    /// nothing about the first one leaks into it.</para>
    /// </summary>
    [Fact]
    public async Task Two_connections_hold_two_projects_and_neither_sees_the_other()
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
                using var alpha = new ClientWebSocket();
                await alpha.ConnectAsync(new Uri($"ws://127.0.0.1:{port}/ws"), CancellationToken.None);
                await SendAsync(alpha, MessageTypes.Hello, new HelloPayload());
                var welcome = await ReceiveAsync<WelcomePayload>(alpha, MessageTypes.Welcome);
                Assert.Equal(alphaManifest, welcome.ProjectId);
                Assert.Equal("Alpha", welcome.ProjectName);

                // A second socket, moved to Beta by project.open — the only thing that rebinds a
                // connection. Locally this is a second window; on a server it is one user walking
                // between their own projects.
                using var beta = new ClientWebSocket();
                await beta.ConnectAsync(new Uri($"ws://127.0.0.1:{port}/ws"), CancellationToken.None);
                await SendAsync(beta, MessageTypes.Hello, new HelloPayload());
                await ReceiveAsync<WelcomePayload>(beta, MessageTypes.Welcome);

                await SendAsync(beta, MessageTypes.ProjectOpen, new ProjectOpenPayload { ProjectId = betaManifest });
                var context = await ReceiveAsync<ProjectContextPayload>(beta, MessageTypes.ProjectContext);
                Assert.Equal(betaManifest, context.ProjectId);

                await SendAsync(beta, MessageTypes.ChatNew, new ChatNewPayload { Title = "Beta chat" });
                var opened = await ReceiveAsync<ChatOpenedPayload>(beta, MessageTypes.ChatOpened);
                Assert.Equal("Beta chat", opened.Title);

                // chat.new also broadcasts an unsolicited chat.list.result to that project's watchers
                // (sidebar auto-refresh) — correlate by RequestId so broadcast noise cannot be
                // mistaken for the reply to an explicit query.
                await SendAsync(beta, MessageTypes.ChatList, null, requestId: "beta-list");
                var betaList = await ReceiveAsync<ChatListResultPayload>(beta, MessageTypes.ChatListResult, "beta-list");
                Assert.Single(betaList.Chats);

                // The point of the test: Alpha's connection was never moved and sees nothing of Beta.
                await SendAsync(alpha, MessageTypes.ChatList, null, requestId: "alpha-list");
                var alphaList = await ReceiveAsync<ChatListResultPayload>(alpha, MessageTypes.ChatListResult, "alpha-list");
                Assert.Empty(alphaList.Chats);

                await alpha.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
                await beta.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
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
                // No project named on the message: project.create already moved this connection into
                // the project it made, which is the whole point of binding it to the connection.
                await SendAsync(socket, MessageTypes.ChatList, null, requestId: "h1");
                var chats = await ReceiveAsync<ChatListResultPayload>(socket, MessageTypes.ChatListResult, "h1");
                Assert.Empty(chats.Chats);

                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
            }
            finally { await host.StopAsync(); }
        }
        finally { try { Directory.Delete(root, recursive: true); } catch { } }
    }

    [Fact]
    public async Task Initial_chat_is_created_once_and_first_message_is_streamed_to_the_web_client()
    {
        var root = TempRoot();
        try
        {
            var provider = new LocalProjectProvider(Path.Combine(root, "state"));
            var manifest = Path.Combine(root, "demo", "RemoteWeb.spla");
            using var registry = new AgentRuntimeRegistry(NullLoggerFactory.Instance, provider);
            registry.Create(new ProjectDescriptor { Id = manifest, ManifestPath = manifest, Name = "Remote Web" });
            registry.DefaultProjectId = manifest;

            var port = FreePort();
            var host = SplaServiceHost.Build(registry, new ServiceOptions
            {
                Port = port,
                InitialChatMessage = "Hello from startup"
            });
            await host.StartAsync();
            try
            {
                using var first = new ClientWebSocket();
                await first.ConnectAsync(new Uri($"ws://127.0.0.1:{port}/ws"), CancellationToken.None);
                await SendAsync(first, MessageTypes.Hello, new HelloPayload());
                await ReceiveAsync<WelcomePayload>(first, MessageTypes.Welcome);

                var opened = await ReceiveAsync<ChatOpenedPayload>(first, MessageTypes.ChatOpened);
                Assert.Equal("Hello from startup", opened.Title);

                var userMessage = await ReceiveAsync<UserMessagePayload>(first, MessageTypes.UserMessage);
                Assert.Equal("Hello from startup", userMessage.Text);

                using var second = new ClientWebSocket();
                await second.ConnectAsync(new Uri($"ws://127.0.0.1:{port}/ws"), CancellationToken.None);
                await SendAsync(second, MessageTypes.Hello, new HelloPayload());
                await ReceiveAsync<WelcomePayload>(second, MessageTypes.Welcome);
                await SendAsync(second, MessageTypes.ChatList, null, requestId: "startup-list");
                var chats = await ReceiveAsync<ChatListResultPayload>(second, MessageTypes.ChatListResult, "startup-list");
                Assert.Single(chats.Chats);

                await first.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
                await second.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
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
