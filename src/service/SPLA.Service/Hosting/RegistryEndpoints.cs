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
    public static void MapRegistry(this WebApplication app, RegistryHub hub, string? token)
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
