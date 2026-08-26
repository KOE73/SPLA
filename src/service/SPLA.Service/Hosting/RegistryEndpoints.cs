using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SPLA.Instances;

namespace SPLA.Service;

/// <summary>
/// The hub's three endpoints. They live here, not in <c>SPLA.Instances</c>, and that is the whole
/// reason the split exists: the desktop shell references the registry to draw a tray and must not
/// acquire ASP.NET doing it. Hosting is this layer's business; the hub itself is a plain object.
///
/// <para>Mapped by whoever is playing host — <c>spla hub</c>, a <c>serve</c> that was asked to also
/// be one, or <c>SPLA.Server</c>. One deployment, one set of routes, no second implementation for
/// the "without a server" case, because that case is a different scenario rather than a smaller
/// one.</para>
/// </summary>
public static class RegistryEndpoints
{
    /// <param name="token">Required of every caller when set. An open registration endpoint is not
    /// "no encryption": it lets anyone who can reach the port enumerate somebody's agents and ask
    /// them to stop. Loopback-only deployments are the one place leaving it null is defensible.</param>
    /// <param name="spawner">How this deployment starts agents, or null when it may not. Null is a
    /// real configuration, not a missing one: the route then answers 501, and a hub that only watches
    /// stays a hub that only watches.</param>
    public static void MapRegistry(
        this WebApplication app, RegistryHub hub, string? token, IInstanceSpawner? spawner = null)
    {
        app.MapGet(RegistryRoutes.Instances, (HttpContext ctx) =>
            Authorized(ctx, token)
                ? Results.Json(new RegistryListResponse { Instances = [.. hub.List()] }, RegistryJson.Options)
                : Results.Unauthorized());

        // A stop is relayed, never performed here: only the instance knows whether it may go, and an
        // index that could kill processes it does not own would be a different kind of thing entirely.
        app.MapPost(RegistryRoutes.Stop, async (HttpContext ctx, string instance, bool force = false) =>
            !Authorized(ctx, token)
                ? Results.Unauthorized()
                : await hub.RequestStopAsync(instance, force)
                    ? Results.Ok()
                    : Results.NotFound());

        // "Close the project": the agent and every window on it, asked together. Relayed exactly like
        // a single stop — each participant still decides — but addressed by project, because closing
        // only the agent is what left windows pointed at a service that would never answer.
        app.MapPost(RegistryRoutes.StopProject, async (HttpContext ctx, string project, bool force = false) =>
        {
            if (!Authorized(ctx, token)) return Results.Unauthorized();
            var asked = await hub.RequestStopProjectAsync(project, force);
            return asked > 0 ? Results.Ok(new { asked }) : Results.NotFound();
        });

        // What a manager opens with: everything this machine remembers, running or not. The instance
        // listing cannot answer this — it only knows what is up, and a project with nothing running is
        // precisely the row somebody came to press Start on.
        app.MapGet(RegistryRoutes.Projects, (HttpContext ctx) =>
            Authorized(ctx, token)
                ? Results.Json(new KnownProjectsResponse { Projects = KnownProjects(hub) }, RegistryJson.Options)
                : Results.Unauthorized());

        // Forgets the memory of a project, never the project. See ConfigLoader.RemoveRecentProject.
        app.MapPost(RegistryRoutes.Forget, (HttpContext ctx, string project) =>
            !Authorized(ctx, token)
                ? Results.Unauthorized()
                : SPLA.Domain.Settings.ConfigLoader.RemoveRecentProject(project)
                    ? Results.Ok()
                    : Results.NotFound());

        // The one place the hub acts on its own rather than passing a request along. Stopping has an
        // owner to ask; starting does not, and refusing to start would mean a machine with no desktop
        // could not bring a project up at all. See ADR_20260820_apps_project-hub §4.
        app.MapPost(RegistryRoutes.Start, async (HttpContext ctx, string project) =>
        {
            if (!Authorized(ctx, token)) return Results.Unauthorized();
            if (spawner is null)
                return Results.Json(new { error = "This hub does not start instances." }, statusCode: 501);

            // Plain Start: whatever the project says about MCP is left exactly as the project says it.
            var result = await spawner.StartAsync(project, enableMcp: false, ctx.RequestAborted);
            if (result.AlreadyRunning) return Results.Json(new { started = false, alreadyRunning = true });
            return result.Started
                ? Results.Json(new { started = true })
                : Results.Json(new { started = false, error = result.Error }, statusCode: 400);
        });

        app.MapPost(RegistryRoutes.Focus, async (HttpContext ctx, string instance) =>
            !Authorized(ctx, token)
                ? Results.Unauthorized()
                : await hub.RequestFocusAsync(instance)
                    ? Results.Ok()
                    : Results.NotFound());

        // The Projects window's own scheme. Not project-scoped — the hub holds no project to save it
        // through — so it goes straight to a file next to the registry rather than through `hub`.
        app.MapGet(RegistryRoutes.Appearance, (HttpContext ctx) =>
            Authorized(ctx, token)
                ? Results.Json(new { theme = SPLA.Domain.Settings.HubAppearanceStore.LoadTheme() })
                : Results.Unauthorized());

        app.MapPost(RegistryRoutes.Appearance, (HttpContext ctx, string theme) =>
        {
            if (!Authorized(ctx, token)) return Results.Unauthorized();
            SPLA.Domain.Settings.HubAppearanceStore.SaveTheme(theme);
            return Results.Ok();
        });

        app.Map(RegistryRoutes.Watch, async (HttpContext ctx) =>
        {
            if (!ctx.WebSockets.IsWebSocketRequest) { ctx.Response.StatusCode = 400; return; }
            if (!Authorized(ctx, token)) { ctx.Response.StatusCode = 401; return; }

            using var socket = await ctx.WebSockets.AcceptWebSocketAsync();
            await WatchAsync(hub, socket, ctx.RequestAborted);
        });

        app.Map(RegistryRoutes.Channel, async (HttpContext ctx) =>
        {
            if (!ctx.WebSockets.IsWebSocketRequest) { ctx.Response.StatusCode = 400; return; }
            if (!Authorized(ctx, token)) { ctx.Response.StatusCode = 401; return; }

            using var socket = await ctx.WebSockets.AcceptWebSocketAsync();
            await ServeChannelAsync(hub, socket, ctx.RequestAborted);
        });

        // The "front door" route: a client that only knows the hub's fixed port, never an instance's
        // ephemeral one, dials this exactly like it would dial an instance's own POST /mcp — same path,
        // same body, same Accept-driven choice between plain JSON and SSE. Guarded by the same token as
        // every other route here, which matters more for this one than for most: an unauthenticated hub
        // reachable off loopback would hand out a route that starts arbitrary-code-running agents on
        // request. See MapRegistry's own remarks on `token` for when that guard is (and is not) in
        // place; this route adds no separate authorization scheme, deliberately, so there is exactly one
        // door to reason about instead of two that could drift apart.
        app.Map(RegistryRoutes.Mcp, async (HttpContext ctx) =>
        {
            if (ctx.Request.Method != HttpMethods.Post) { ctx.Response.StatusCode = 405; return; }
            if (!Authorized(ctx, token)) { ctx.Response.StatusCode = 401; return; }

            await HandleMcpProxyAsync(ctx, hub, spawner);
        });

        // The hub's own MCP surface: no `?project=`, answered here rather than relayed, offering
        // tools that describe the machine instead of one project. Read-only for now — see
        // HubToolHost's remarks on why start/stop are left to the routes above rather than folded
        // in here.
        app.Map(RegistryRoutes.HubMcp, async (HttpContext ctx) =>
        {
            if (ctx.Request.Method != HttpMethods.Post) { ctx.Response.StatusCode = 405; return; }
            if (!Authorized(ctx, token)) { ctx.Response.StatusCode = 401; return; }

            await HandleHubMcpAsync(ctx, hub);
        });
    }

    /// <summary>Answers one JSON-RPC line against <see cref="HubToolHost"/> instead of relaying it —
    /// the hub-scoped analogue of <c>SplaServiceHost.HandleMcpAsync</c>. No SSE: every hub tool today
    /// is an instant in-memory read, so there is nothing a streamed progress frame would ever carry.
    /// </summary>
    private static async Task HandleHubMcpAsync(HttpContext ctx, RegistryHub hub)
    {
        string line;
        using (var bodyReader = new StreamReader(ctx.Request.Body))
        {
            line = await bodyReader.ReadToEndAsync(ctx.RequestAborted);
        }
        if (string.IsNullOrWhiteSpace(line)) { ctx.Response.StatusCode = 400; return; }

        System.Text.Json.Nodes.JsonNode? request;
        try { request = System.Text.Json.Nodes.JsonNode.Parse(line); }
        catch (System.Text.Json.JsonException) { ctx.Response.StatusCode = 400; return; }

        var requestId = request?["id"];
        var expectsReply = requestId is not null;

        var host = new HubToolHost(hub);
        var server = new SPLA.Mcp.McpStdioServer(
            host,
            host.GetToolDefinitions,
            log: TextWriter.Null,
            source: $"hub-mcp {ctx.Connection.RemoteIpAddress}");

        var reader = new McpHttpFraming.PendingLineReader(line);
        var writer = new McpHttpFraming.CapturingWriter(requestId);
        var running = server.RunAsync(reader, writer, ctx.RequestAborted);

        string? responseLine = null;
        if (expectsReply)
        {
            var finished = await Task.WhenAny(writer.ResponseWritten, running);
            if (finished == running)
                await running; // surfaces the fault, if any, instead of swallowing it
            else
                responseLine = await writer.ResponseWritten;
        }

        reader.SignalEof();
        await running;

        if (responseLine is null) { ctx.Response.StatusCode = 204; return; }
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(responseLine, ctx.RequestAborted);
    }

    /// <summary>
    /// The machine's remembered projects, each merged with whatever is registered against it.
    ///
    /// <para>Two sources because they answer different halves: the recent list knows what exists and
    /// survives everything being shut down, while the hub knows what is up right now and survives
    /// nothing. Neither alone is a project manager.</para>
    ///
    /// <para>A project that is running but not remembered is still listed. It would otherwise be
    /// invisible here while plainly visible in <c>spla ps</c> — and something started by hand in a
    /// folder nobody has opened in the desktop app is a perfectly ordinary way to work.</para>
    /// </summary>
    /// <summary>
    /// The name a project is listed and addressed under, read from its manifest rather than taken from
    /// whichever registration happens to be up.
    ///
    /// <para><b>It has to be the same string in both states.</b> This used to prefer a live
    /// registration's <c>ProjectName</c> and fall back to the file name, which meant a project whose
    /// manifest calls it "Тест" but whose file is <c>1C.spla</c> was listed as <c>1C</c> while stopped
    /// and as <c>Тест</c> while running. That is not cosmetic: the row jumps to a different place in an
    /// alphabetical list, and the MCP address copied from it changes, at the moment somebody presses
    /// Start. Reading the manifest answers identically either way — and it is the same source the
    /// registration itself was quoting, so nothing is lost by not waiting for one.</para>
    /// </summary>
    private static string StableName(string manifestPath)
    {
        try
        {
            if (SPLA.Domain.Settings.ConfigLoader.LoadProjectRaw(manifestPath).Name is { } name
                && !string.IsNullOrWhiteSpace(name))
                return name;
        }
        catch { /* an unreadable or half-written manifest still has a file name to be known by */ }

        return Path.GetFileNameWithoutExtension(manifestPath);
    }

    internal static List<KnownProjectDto> KnownProjects(RegistryHub hub)
    {
        var live = hub.List();
        var remembered = SPLA.Domain.Settings.ConfigLoader.LoadRecentProjects();

        var ids = new List<string>(remembered);
        foreach (var entry in live)
        {
            if (!ids.Any(id => string.Equals(id, entry.ProjectId, StringComparison.OrdinalIgnoreCase)))
                ids.Add(entry.ProjectId);
        }

        return ids.Select(id =>
        {
            var participants = live
                .Where(e => string.Equals(e.ProjectId, id, StringComparison.OrdinalIgnoreCase))
                .ToList();
            var agent = participants.FirstOrDefault(e => e.Role == ParticipantRoles.Agent);

            return new KnownProjectDto
            {
                ProjectId = id,
                Name = StableName(id),
                Exists = File.Exists(id),
                State = agent?.State,
                InstanceId = agent?.Info.InstanceId,
                Windows = participants.Count(e => e.Role == ParticipantRoles.Window),
                McpAvailable = !string.IsNullOrEmpty(agent?.Info.Endpoint)
            };
        }).ToList();
    }

    /// <summary>
    /// One instance's registration channel: its first frame says who it is, every later frame says
    /// what it is doing, and the socket closing says it is gone. Liveness is the connection —
    /// nothing here checks a pid or a timestamp, because those cannot answer the question across a
    /// network and the transport already can.
    /// </summary>
    private static async Task ServeChannelAsync(RegistryHub hub, WebSocket socket, CancellationToken ct)
    {
        var buffer = new byte[16 * 1024];
        IDisposable? registration = null;
        var instanceId = "";

        try
        {
            while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await socket.ReceiveAsync(buffer, ct);
                if (result.MessageType == WebSocketMessageType.Close) break;

                var frame = JsonSerializer.Deserialize<RegistryFrame>(
                    buffer.AsSpan(0, result.Count), RegistryJson.Options);
                if (frame is null) continue;

                switch (frame.Type)
                {
                    case RegistryFrames.Register when registration is null:
                        var body = frame.Body?.Deserialize<RegisterFrame>(RegistryJson.Options);
                        if (body is null) break;

                        registration = hub.Register(body, (type, stop) => SendAsync(socket, type, stop, ct));
                        instanceId = RegistryHub.IdOf(registration);
                        await SendAsync(socket, RegistryFrames.Accepted, new { }, ct);
                        break;

                    case RegistryFrames.Status when registration is not null:
                        var status = frame.Body?.Deserialize<StatusFrame>(RegistryJson.Options);
                        if (status is not null) hub.Report(instanceId, status);
                        break;
                }
            }
        }
        catch (OperationCanceledException) { /* host stopping, or the instance went away */ }
        catch (WebSocketException) { /* the same, less politely */ }
        finally
        {
            // The only place an instance leaves the hub. However the socket ended — closed, dropped,
            // process killed — this runs, which is what makes the channel the liveness signal.
            registration?.Dispose();
        }
    }

    /// <summary>
    /// One observer's live view: the current listing immediately, then again on every change.
    ///
    /// <para>Sends the whole list rather than a delta. It is small, and an observer that has to
    /// reconcile deltas is an observer that can end up showing something the hub does not hold —
    /// which for a tray icon means an instance that looks alive after it left.</para>
    /// </summary>
    private static async Task WatchAsync(RegistryHub hub, WebSocket socket, CancellationToken ct)
    {
        // A gate rather than sending straight from the event: Changed fires on whichever thread
        // caused it, and a socket may only have one send in flight. Coalescing is a feature here —
        // three changes in a millisecond are one thing to look at.
        var pending = new SemaphoreSlim(1, 1);
        void OnChanged() { try { pending.Release(); } catch (SemaphoreFullException) { /* already due */ } }

        hub.Changed += OnChanged;
        try
        {
            while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var listing = new RegistryListResponse { Instances = [.. hub.List()] };
                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(listing, RegistryJson.Options));
                await socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct);

                await pending.WaitAsync(ct);
            }
        }
        catch (OperationCanceledException) { /* observer went away, or the host is stopping */ }
        catch (WebSocketException) { /* the same, less politely */ }
        finally
        {
            hub.Changed -= OnChanged;
            pending.Dispose();
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
        return socket.State == WebSocketState.Open
            ? socket.SendAsync(bytes, WebSocketMessageType.Text, true, ct)
            : Task.CompletedTask;
    }

    /// <summary>One reusable client for every relayed MCP call. No per-request timeout — a tool call
    /// behind an SSE stream can legitimately run for as long as <c>ssh_run</c> or an agent-spawn does,
    /// and this proxy has no more business cutting that off than the instance's own handler does.
    /// Cancellation instead rides <see cref="HttpContext.RequestAborted"/>, exactly as it does on the
    /// instance side.</summary>
    private static readonly HttpClient ProxyHttp = new() { Timeout = Timeout.InfiniteTimeSpan };

    /// <summary>
    /// The front-door handler: resolve which instance owns the project, bring it up if nothing does,
    /// then relay the call verbatim. Every failure mode gets its own status code rather than a bare
    /// 500, because the caller on the other end of this is meant to be an MCP client with no other way
    /// to find out *why* — there is no human at a console reading hub logs on the other side of a
    /// stable URL the way there would be for `spla hub` run by hand.
    /// </summary>
    private static async Task HandleMcpProxyAsync(HttpContext ctx, RegistryHub hub, IInstanceSpawner? spawner)
    {
        var project = ctx.Request.Query["project"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(project))
        {
            ctx.Response.StatusCode = 400;
            await ctx.Response.WriteAsync("?project=<project name, or a manifest path> is required.");
            return;
        }

        var resolved = ResolveProjectId(hub, project);
        if (resolved.Kind == ResolveKind.NotFound)
        {
            ctx.Response.StatusCode = 404;
            await ctx.Response.WriteAsync(
                $"No project named or at '{project}' is known to this machine. Name lookup covers " +
                "projects that are running, have registered here, or are in the remembered list the " +
                "Projects window shows. A manifest path always works.");
            return;
        }
        if (resolved.Kind == ResolveKind.Ambiguous)
        {
            ctx.Response.StatusCode = 409;
            await ctx.Response.WriteAsJsonAsync(new
            {
                error = $"'{project}' matches more than one project this hub has seen. Use the manifest path instead.",
                candidates = resolved.Candidates.Select(c => new { name = c.ProjectName, path = c.ProjectId })
            }, RegistryJson.Options, ctx.RequestAborted);
            return;
        }

        var projectId = resolved.ProjectId!;
        var agent = hub.FindAgent(projectId);

        if (agent is null || string.IsNullOrEmpty(agent.Info.Endpoint))
        {
            if (spawner is null)
            {
                ctx.Response.StatusCode = 501;
                await ctx.Response.WriteAsync(
                    "Nothing is running this project and this hub was not given a spawner, so it cannot start one.");
                return;
            }

            // --mcp forced on: this child is being started for the sole purpose of answering the
            // request in hand, and one that came up without the route mapped would answer it with the
            // static-asset catch-all's 405 — the exact confusing failure this argument exists to stop.
            var spawn = await spawner.StartAsync(projectId, enableMcp: true, ctx.RequestAborted);
            if (!spawn.Started && !spawn.AlreadyRunning)
            {
                ctx.Response.StatusCode = 502;
                await ctx.Response.WriteAsync($"Could not start an instance for '{projectId}': {spawn.Error}");
                return;
            }

            // Covers both branches above: a fresh process still has to connect and register before it
            // has an endpoint to proxy to, and "already running" can still race a registration that
            // simply has not reached the hub yet (the channel and this HTTP request are independent
            // round trips). Either way the only honest thing to do is wait for the hub to actually see
            // it, not assume StartAsync returning means the endpoint exists yet.
            agent = await WaitForAgentAsync(hub, projectId, TimeSpan.FromSeconds(30), ctx.RequestAborted);
            if (agent is null)
            {
                ctx.Response.StatusCode = 504;
                await ctx.Response.WriteAsync(
                    $"'{projectId}' was asked to start but did not register an MCP endpoint within 30s.");
                return;
            }
        }

        await ProxyAsync(ctx, agent.Info.Endpoint!);
    }

    private enum ResolveKind { Found, Ambiguous, NotFound }

    private sealed record ResolvedProject(ResolveKind Kind, string? ProjectId, IReadOnlyList<RegistryHub.ProjectMatch> Candidates);

    /// <summary>
    /// Turns whatever the caller typed after <c>?project=</c> into the manifest path everything else
    /// here keys on.
    ///
    /// <para><b>Path first, always.</b> A path that exists on disk is accepted outright and never even
    /// consults the name index — which matters because it is the *only* way to address a project this
    /// hub has never seen register (see <see cref="RegistryHub"/>'s remarks on <c>_knownNames</c>). A
    /// name that happens to collide with a real file path would be a strange config to write, but
    /// resolving the path first means that config does what it looks like it does rather than silently
    /// preferring a name match instead.</para>
    ///
    /// <para><b>Name second, over both halves of what the machine knows.</b> Live registrations
    /// (<c>_knownNames</c>) answer for anything running or recently run; the remembered-projects list
    /// answers for everything else, and is the same source <see cref="KnownProjects"/> builds the
    /// Projects window from — deliberately so. A name the person can read in that window has to be a
    /// name this route accepts, or the "copy MCP address" button beside it would hand out addresses
    /// that 404. It also fixes the case the whole front door exists for: after the hub itself restarts,
    /// nothing has registered yet, and name lookup limited to live registrations would answer nothing
    /// at all until something else happened to start first.</para>
    ///
    /// <para>Still not a disk scan — see the field doc on <c>_knownNames</c> for why a filesystem crawl
    /// was rejected. The remembered list is a short, explicit record of projects this machine has
    /// actually opened, which is a different thing from hunting the disk for manifests. Entries whose
    /// manifest has since been deleted are dropped here rather than carried: they cannot be started
    /// anyway, and keeping them would let a stale row turn a perfectly unambiguous name ambiguous.</para>
    ///
    /// <para>A collision between two different manifests sharing a display name is reported back to the
    /// caller as <see cref="ResolveKind.Ambiguous"/> rather than guessed at, per the owner's explicit
    /// call: an ambiguous route should say so, not pick one.</para>
    /// </summary>
    private static ResolvedProject ResolveProjectId(RegistryHub hub, string project)
    {
        if (File.Exists(project))
            return new ResolvedProject(ResolveKind.Found, project, []);

        var matches = new List<RegistryHub.ProjectMatch>(hub.FindByName(project));

        foreach (var id in SPLA.Domain.Settings.ConfigLoader.LoadRecentProjects())
        {
            if (!File.Exists(id)) continue;

            // The very same name the Projects window lists it under (see StableName), so what a person
            // reads off a row and what this route accepts agree by construction rather than by luck.
            var name = StableName(id);
            if (!string.Equals(name, project, StringComparison.OrdinalIgnoreCase)) continue;
            if (matches.Any(m => string.Equals(m.ProjectId, id, StringComparison.OrdinalIgnoreCase)))
                continue;

            matches.Add(new RegistryHub.ProjectMatch(id, name));
        }

        return matches.Count switch
        {
            0 => new ResolvedProject(ResolveKind.NotFound, null, []),
            1 => new ResolvedProject(ResolveKind.Found, matches[0].ProjectId, []),
            _ => new ResolvedProject(ResolveKind.Ambiguous, null, matches)
        };
    }

    /// <summary>
    /// Blocks until <paramref name="projectId"/> has a live agent with a published endpoint, or gives
    /// up after <paramref name="timeout"/>. Driven by <see cref="RegistryHub.Changed"/> rather than a
    /// poll loop with a fixed delay — the same event the watch socket rides — so this notices the
    /// registration on the same tick everything else watching the hub does, instead of adding its own
    /// polling latency on top of however long the child actually takes to start.
    /// </summary>
    private static async Task<RegisteredInstanceDto?> WaitForAgentAsync(
        RegistryHub hub, string projectId, TimeSpan timeout, CancellationToken ct)
    {
        var found = hub.FindAgent(projectId);
        if (found is { Info.Endpoint.Length: > 0 }) return found;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        var signal = new SemaphoreSlim(0, int.MaxValue);
        void OnChanged() { try { signal.Release(); } catch (SemaphoreFullException) { /* already due */ } }

        hub.Changed += OnChanged;
        try
        {
            while (true)
            {
                found = hub.FindAgent(projectId);
                if (found is { Info.Endpoint.Length: > 0 }) return found;

                try { await signal.WaitAsync(cts.Token); }
                catch (OperationCanceledException) { return null; }
            }
        }
        finally { hub.Changed -= OnChanged; }
    }

    /// <summary>
    /// Relays one MCP call to the instance now holding the project: same body, same <c>Accept</c>
    /// (which is what the instance's own <c>HandleMcpAsync</c> reads to decide plain-JSON vs SSE), and
    /// the response streamed back a chunk at a time rather than buffered.
    ///
    /// <para><b>Why not <c>response.Content.CopyToAsync(ctx.Response.Body)</c>.</b> That method reads
    /// through its own internal buffer and only writes onward when it fills, which for a slow trickle
    /// of SSE progress frames means the client would see nothing until enough of them piled up to fill
    /// that buffer — precisely the bundled-at-the-end behavior <c>SseWriter</c> on the instance side
    /// exists to avoid, reintroduced one hop later. The manual read/write/flush loop below instead
    /// forwards whatever came off the wire in one read as one write-and-flush, so a frame the instance
    /// pushed the moment it was produced is not held back here either.</para>
    /// </summary>
    private static async Task ProxyAsync(HttpContext ctx, string endpoint)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint.TrimEnd('/') + RegistryRoutes.Mcp)
        {
            Content = new StreamContent(ctx.Request.Body)
        };
        if (ctx.Request.ContentType is { } contentType)
            request.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);
        foreach (var accept in ctx.Request.Headers.Accept)
            if (accept is not null)
                request.Headers.TryAddWithoutValidation("Accept", accept);

        HttpResponseMessage response;
        try
        {
            response = await ProxyHttp.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ctx.RequestAborted);
        }
        catch (HttpRequestException ex)
        {
            ctx.Response.StatusCode = 502;
            await ctx.Response.WriteAsync($"Could not reach the instance at '{endpoint}': {ex.Message}");
            return;
        }

        using (response)
        {
            // An instance that was already up when this request arrived may have been started without
            // MCP — by a desktop window, or by hand — and then /mcp is simply not mapped on it. What
            // comes back is whatever its static-asset catch-all makes of a POST, which is a bare 405
            // (or a 404) that says nothing about the actual problem or its fix. Nothing here can turn
            // MCP on for a process already running, so the useful thing is to name the situation.
            if (response.StatusCode is HttpStatusCode.MethodNotAllowed or HttpStatusCode.NotFound)
            {
                ctx.Response.StatusCode = 409;
                await ctx.Response.WriteAsync(
                    $"The instance already serving this project (at '{endpoint}') does not answer /mcp: " +
                    "it was started without MCP enabled. Enable mcp in the project's settings, or stop " +
                    "it and let this route start one for itself.");
                return;
            }

            ctx.Response.StatusCode = (int)response.StatusCode;
            if (response.Content.Headers.ContentType is { } responseContentType)
                ctx.Response.ContentType = responseContentType.ToString();

            await using var upstream = await response.Content.ReadAsStreamAsync(ctx.RequestAborted);
            var buffer = new byte[8 * 1024];
            int read;
            try
            {
                while ((read = await upstream.ReadAsync(buffer, ctx.RequestAborted)) > 0)
                {
                    await ctx.Response.Body.WriteAsync(buffer.AsMemory(0, read), ctx.RequestAborted);
                    await ctx.Response.Body.FlushAsync(ctx.RequestAborted);
                }
            }
            catch (OperationCanceledException) { /* the caller went away mid-stream — same as any other client disconnect */ }
        }
    }

    private static bool Authorized(HttpContext ctx, string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return true;

        var header = ctx.Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) &&
               string.Equals(header["Bearer ".Length..].Trim(), token, StringComparison.Ordinal);
    }
}
