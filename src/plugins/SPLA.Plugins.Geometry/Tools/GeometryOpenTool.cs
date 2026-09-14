using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Security;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Geometry.Image;
using SPLA.Plugins.Geometry.Session;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Geometry.Tools;

/// <summary>
/// Opens a frame for markup. One frame per chat: opening another replaces this one, because the
/// working unit is a single frame marked up and handed on (ADR §3.5).
/// </summary>
internal sealed class GeometryOpenTool(ResolvedSettings projectSettings) : GeometryToolBase(projectSettings)
{
    public override string Name => "geom_open";

    protected override string Description =>
        "Opens an image for markup and returns it as a picture. Start here: every other geom_* tool " +
        "works on the image this opened. Accepts a blob:<handle> from another tool, a URI " +
        "(file:///…), or a path inside the project workspace.";

    protected override string? Details =>
        "Opening a second image in the same chat replaces the first one and everything marked on it. " +
        "The picture you get back may be smaller than the original file; its size is the coordinate " +
        "space for every coordinate you pass afterwards.";

    protected override Dictionary<string, object> Properties => new()
    {
        ["image"] = new
        {
            type = "string",
            description = "Where the image is: a blob:<handle> from another tool, a URI such as " +
                          "file:///C:/frames/01.png, or a path relative to the project workspace."
        },
        ["grid"] = new
        {
            type = new[] { "boolean", "null" },
            description = "Draw a coordinate grid over every render of this view. Null = false. " +
                          "Useful only to check that the picture reached you at the size stated."
        },
    };

    protected override async Task<ToolResult> RunAsync(
        IAgentSession chat, GeometrySettings cfg, JsonElement args, CancellationToken ct)
    {
        var address = Str(args, "image");
        if (address is null) return ToolResult.Fail("Error: image is required.", "missing image");

        var loaded = await ImageSource.LoadAsync(address, Resolved(chat), ct).ConfigureAwait(false);
        if (!loaded.Ok) return ToolResult.Fail($"geom_open: {loaded.Error}", "image not readable");

        var origin = loaded.Origin ?? OriginOf(address);

        // A picture is content like any other: whatever the frame shows, it arrived from somewhere,
        // and the chat is told where before anything is drawn on it.
        chat.Doubt.Observe(origin, address);

        var session = GeometrySession.Open(loaded.Bytes, address, origin, cfg, out var error);
        if (session is null) return ToolResult.Fail($"geom_open: {error}", "not an image");

        GeometrySessionRegistry.Set(chat, session);

        var view = session.CurrentView;
        view.Grid = ToolJson.GetBoolean(args, "grid", false);

        return RenderResult(chat, session, view,
            $"opened '{address}' — source image {session.Source.Width}x{session.Source.Height} px", cfg);
    }

    /// <summary>
    /// What to label an address the source itself said nothing about. A resource provider hands back
    /// content without a <c>DataOrigin</c>, so the label is read off the address: anything fetched
    /// over http(s) is somebody's site and untrusted, anything else is a file the project already
    /// holds. Labelling a project file as web content would raise doubt on every ordinary frame and
    /// teach the reader to ignore the label.
    /// </summary>
    private static DataOrigin OriginOf(string address) =>
        Uri.TryCreate(address, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https"
            ? DataOrigin.Site(uri.Host, listed: false)
            : DataOrigin.Project;
}
