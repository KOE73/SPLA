using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
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

            var result = await spawner.StartAsync(project, ctx.RequestAborted);
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
    private static List<KnownProjectDto> KnownProjects(RegistryHub hub)
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
                Name = participants.FirstOrDefault()?.ProjectName
                       ?? Path.GetFileNameWithoutExtension(id),
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

    private static bool Authorized(HttpContext ctx, string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return true;

        var header = ctx.Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) &&
               string.Equals(header["Bearer ".Length..].Trim(), token, StringComparison.Ordinal);
    }
}
