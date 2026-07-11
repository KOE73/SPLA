using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SPLA.Observability.Collection;
using SPLA.Service.Auth;

namespace SPLA.Service.Observability;

/// <summary>Maps the local stats dashboard (`/stats`), its JSON API (`/stats/api/*`), and the real-time
/// firehose (`/stats/live`). Any authenticated user may open it; the data is scoped server-side — an
/// admin sees the whole server, an ordinary user only their own activity. This is the observability
/// plane (viewing), gated by the ambient auth (fallback policy), never a host-configuration surface.</summary>
internal static class StatsEndpoints
{
    public static void Map(WebApplication app, StatsHub hub, bool authEnabled)
    {
        app.MapGet("/stats", (HttpContext ctx) =>
            TelemetryCollector.Current is null
                ? Results.NotFound()
                : Results.Text(StatsPage.Render(ctx.User.Identity?.Name ?? ""), "text/html; charset=utf-8"));

        var api = app.MapGroup("/stats/api");

        api.MapGet("/snapshot", (HttpContext ctx) =>
        {
            if (TelemetryCollector.Current is not { } c) return Results.NoContent();
            var (userKey, isAdmin) = Scope(ctx, authEnabled);
            return Results.Json(c.SnapshotFor(userKey, isAdmin));
        });

        // The per-metric time series is server-wide aggregate data → admin only. Ordinary users get
        // their own live feed + counters via the snapshot, not the server-wide charts.
        api.MapGet("/metrics", (HttpContext ctx) =>
            IsAdmin(ctx, authEnabled) ? Results.Json(TelemetryCollector.Current?.Instruments() ?? []) : Results.Json(Array.Empty<string>()));

        api.MapGet("/series", (HttpContext ctx, string metric, int? minutes) =>
        {
            if (!IsAdmin(ctx, authEnabled)) return Results.Json(Array.Empty<BucketPoint>(), statusCode: StatusCodes.Status403Forbidden);
            return Results.Json(TelemetryCollector.Current?.Series(metric, Math.Clamp(minutes ?? 60, 1, 1440)) ?? []);
        });

        // Real-time push socket. Requires the same ambient auth as the page (fallback policy); the
        // subscriber's scope is fixed from its identity at connect time.
        app.Map("/stats/live", (HttpContext ctx) => HandleLiveAsync(ctx, hub, authEnabled));
    }

    private static async Task HandleLiveAsync(HttpContext ctx, StatsHub hub, bool authEnabled)
    {
        if (!ctx.WebSockets.IsWebSocketRequest) { ctx.Response.StatusCode = StatusCodes.Status400BadRequest; return; }
        if (TelemetryCollector.Current is not { } collector) { ctx.Response.StatusCode = StatusCodes.Status404NotFound; return; }

        var (userKey, isAdmin) = Scope(ctx, authEnabled);
        if (authEnabled && userKey is null) { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return; }

        using var socket = await ctx.WebSockets.AcceptWebSocketAsync();
        var subscriber = new StatsSubscriber { Socket = socket, UserKey = userKey, IsAdmin = isAdmin };
        hub.Add(subscriber);
        try
        {
            await hub.SendInitialAsync(subscriber, collector);

            // Hold the socket open; we only read to observe the client closing it.
            var buffer = new byte[512];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, ctx.RequestAborted);
                if (result.MessageType == WebSocketMessageType.Close) break;
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException) { }
        finally { hub.Remove(subscriber); }
    }

    private static (string? UserKey, bool IsAdmin) Scope(HttpContext ctx, bool authEnabled)
    {
        if (!authEnabled) return (null, true);   // single-user / loopback: full server view
        var userKey = ctx.User.FindFirst(AuthClaims.UserKey)?.Value;
        return (userKey, ctx.User.IsInRole("admin"));
    }

    private static bool IsAdmin(HttpContext ctx, bool authEnabled)
        => !authEnabled || ctx.User.IsInRole("admin");
}
