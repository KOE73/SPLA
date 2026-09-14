using SPLA.Domain.Agent;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Geometry.Model;
using SPLA.Plugins.Geometry.Render;
using SPLA.Plugins.Geometry.Session;
using SPLA.Plugins.Geometry.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Geometry;

/// <summary>
/// The panel half of the plugin: a read-only view on one chat's markup session, so a human can watch
/// the box walk towards the object instead of reading coordinates off tool replies
/// (<c>docs/adr/ADR_20260914-2_web_plugin-panels.md</c>).
/// <para>
/// It is declared on <see cref="GeometryPlugin"/> rather than as a class of its own because the
/// plugin loader looks for <see cref="ISplaPluginPanelProvider"/> on the <see cref="ISplaPlugin"/>
/// instance it created and nowhere else; the panel code still lives in its own file.
/// </para>
/// <para>
/// <b>The panel owns nothing.</b> Every answer is read from the chat's markup session and its blob
/// store at the moment it is asked; the panel holds a chat id and no other state. That is deliberate:
/// nobody tells a plugin when a chat closes (<c>ADR_20260828</c> is open), so a panel with state of
/// its own would be a second thing with nobody to free it (ADR §3.5).
/// </para>
/// </summary>
public sealed partial class GeometryPlugin : ISplaPluginPanelProvider
{
    public string PanelType => "geometry.session";

    public Task<ISplaPluginPanelSession> OpenAsync(
        string panelId,
        IReadOnlyDictionary<string, string?> parameters,
        Func<SplaPluginPanelEvent, ValueTask> publish,
        CancellationToken cancellationToken)
    {
        var session = new GeometryPanelSession(publish, parameters.GetValueOrDefault("chatId"));
        return Task.FromResult<ISplaPluginPanelSession>(session);
    }
}

/// <summary>
/// One open geometry panel. Frames are <b>pushed</b>: the markup loop fires one event per step —
/// rare enough that polling would be a timer burning for nothing (plan §2.1) — and the push carries
/// only the text and the names of the renders in the ring. The pictures themselves are asked for by
/// name, once each, because the ring holds several megabytes and re-sending all of it on every step
/// of the loop would put the whole history on the wire five times over.
/// </summary>
internal sealed class GeometryPanelSession : ISplaPluginPanelSession
{
    private const string RenderPrefix = "geom_render_";

    private readonly Func<SplaPluginPanelEvent, ValueTask> _publish;
    private string? _chatId;

    public GeometryPanelSession(Func<SplaPluginPanelEvent, ValueTask> publish, string? chatId)
    {
        _publish = publish;
        _chatId = chatId;
        GeometrySessionRegistry.Updated += OnUpdated;
        // The chat may already hold a marked-up frame from before the panel was opened.
        _ = PublishStateAsync();
    }

    public Task HandleInputAsync(string inputType, JsonElement payload, CancellationToken cancellationToken)
    {
        switch (inputType)
        {
            case "bind":
                // The window switched chats. Unconditional, with no "is the panel visible" flag —
                // such a flag is exactly what froze the debug panel on the first chat's data.
                _chatId = Text(payload, "chatId");
                return PublishStateAsync().AsTask();
            case "refresh":
                return PublishStateAsync().AsTask();
            case "render":
                return PublishRenderAsync(Text(payload, "name")).AsTask();
            default:
                return Task.CompletedTask;
        }
    }

    public ValueTask DisposeAsync()
    {
        GeometrySessionRegistry.Updated -= OnUpdated;
        return ValueTask.CompletedTask;
    }

    /// <summary>Raised on the tool's own thread, one step of the markup loop at a time. The state is
    /// read here, where the session is quiet, and only the sending is left to run on.</summary>
    private void OnUpdated(string chatId)
    {
        if (!string.Equals(chatId, _chatId, StringComparison.Ordinal)) return;
        var state = ReadState();
        _ = SafePublishAsync(new("state", state));
    }

    private ValueTask PublishStateAsync() => SafePublishAsync(new("state", ReadState()));

    private async ValueTask PublishRenderAsync(string? name)
    {
        if (string.IsNullOrEmpty(name) || !name.StartsWith(RenderPrefix, StringComparison.Ordinal)) return;
        if (Resolve() is not { } found)
        {
            await SafePublishAsync(new("render", new { name, missing = true }));
            return;
        }

        var blobs = found.Owner.Blobs;
        var entry = blobs.Describe(name);
        var payload = blobs.Get(name);
        if (entry is null || payload?.Bytes is not { } bytes)
        {
            await SafePublishAsync(new("render", new { name, missing = true }));
            return;
        }

        await SafePublishAsync(new("render", new
        {
            name,
            at = entry.CreatedAt,
            mimeType = payload.ContentType ?? "image/png",
            base64 = Convert.ToBase64String(bytes)
        }));
    }

    /// <summary>Everything the panel shows except the pixels: which chat, which view, how big it is,
    /// what is marked on it, and which renders the ring currently holds.</summary>
    private object ReadState()
    {
        if (Resolve() is not { } found)
            return new { chatId = _chatId, hasSession = false };

        var (owner, geometry) = found;
        var view = geometry.CurrentView;
        var objects = geometry.Objects.ToArray();

        var renders = owner.Blobs.List()
            .Where(e => e.Name is { } n && n.StartsWith(RenderPrefix, StringComparison.Ordinal))
            .OrderBy(e => e.CreatedAt)
            .Select(e => new { name = e.Name, at = e.CreatedAt, size = e.Size })
            .ToArray();

        return new
        {
            chatId = _chatId,
            hasSession = true,
            source = geometry.SourceAddress,
            sourceWidth = geometry.Source.Width,
            sourceHeight = geometry.Source.Height,
            view = new
            {
                id = view.Id,
                width = view.Width,
                height = view.Height,
                fromBox = view.FromBox,
                deskewed = view.Deskewed,
            },
            objects = objects.Select(o => new
            {
                name = o.Name,
                kind = o.Kind == ObjectKind.Box ? "box" : "point",
                status = o.Status == ObjectStatus.Accepted ? "accepted" : "editing",
                visible = GeometryRenderer.IsVisible(o, view),
                where = GeometryToolBase.DescribeInView(o, view),
            }).ToArray(),
            renders,
            current = geometry.LastRenderName,
        };
    }

    private (IAgentSession Owner, GeometrySession Geometry)? Resolve() =>
        _chatId is { Length: > 0 } id ? GeometrySessionRegistry.TryGetForChat(id) : null;

    /// <summary>A viewer that has gone away must not take the markup loop down with it: the manager
    /// that owns the connection disposes the session on its own.</summary>
    private async ValueTask SafePublishAsync(SplaPluginPanelEvent panelEvent)
    {
        try { await _publish(panelEvent); }
        catch { /* disconnected viewer */ }
    }

    private static string? Text(JsonElement payload, string name)
        => payload.ValueKind == JsonValueKind.Object
           && payload.TryGetProperty(name, out var value)
           && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
