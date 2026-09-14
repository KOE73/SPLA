using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Tools;
using SPLA.Plugins.Geometry.Model;
using SPLA.Plugins.Geometry.Render;
using SPLA.Plugins.Geometry.Session;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Geometry.Tools;

/// <summary>
/// Hands the finished markup to whoever asked for it, in the original image's coordinates.
/// <para>
/// The only geometry tool that returns no picture, and deliberately so: this is data for the caller,
/// not something to look at. Everything else in the plugin exists to keep source-pixel numbers away
/// from the model's eyes; here they are the whole point.
/// </para>
/// </summary>
internal sealed class GeometryResultTool(ResolvedSettings projectSettings) : GeometryToolBase(projectSettings)
{
    public override string Name => "geom_result";

    protected override string Description =>
        "Returns everything marked, in the ORIGINAL image's coordinates, as JSON. This one returns " +
        "no picture — it is the answer, not something to look at.";

    protected override string? Details =>
        "Each box also carries its four corners in source coordinates, so the result is unambiguous " +
        "however the reader interprets the angle. Use output:'blob' to hand the JSON to another tool " +
        "without reading it yourself.";

    protected override Dictionary<string, object> Properties => new()
    {
        ["output"] = SchemaParts.Output,
        ["output_name"] = SchemaParts.OutputName,
    };

    protected override Task<ToolResult> RunAsync(
        IAgentSession chat, GeometrySettings cfg, JsonElement args, CancellationToken ct)
    {
        var session = GeometrySessionRegistry.TryGet(chat);
        if (session is null) return Task.FromResult(NoSession);

        var json = JsonSerializer.Serialize(Build(session), new JsonSerializerOptions { WriteIndented = true });

        var target = DataChannel.ParseTarget(Str(args, "output"));
        var summary = $"{session.Objects.Count} object(s) in source coordinates " +
                      $"({session.Source.Width}x{session.Source.Height} px), from '{session.SourceAddress}'.";

        return Task.FromResult(ToolResult.Text(DataChannel.Route(
            target, BlobPayload.OfText(json, "application/json"), summary,
            Str(args, "output_name"), session.Origin)));
    }

    private static object Build(GeometrySession session) => new
    {
        session = new
        {
            current_view = session.CurrentViewId,
            objects = session.Objects.Count,
        },
        source = new
        {
            address = session.SourceAddress,
            width = session.Source.Width,
            height = session.Source.Height,
        },
        objects = session.Objects.ConvertAll(obj => Describe(obj, session)),
        views = session.Views.ConvertAll(view => (object)new
        {
            id = view.Id,
            parent = view.ParentId,
            from_box = view.FromBox,
            deskewed = view.Deskewed,
            width = view.Width,
            height = view.Height,
            // The transform itself, so a caller that wants to reproduce a view can, without having
            // to reverse-engineer how the crop was chosen.
            source_to_view = Matrix(view.SourceToView),
        }),
    };

    private static object Describe(GeometryObject obj, GeometrySession session)
    {
        var coordinates = new Dictionary<string, object>
        {
            [GeometrySession.RootViewId] = InSource(obj),
        };

        // Every view the object can actually be seen in, so a caller working from a crop does not
        // have to redo the transform the tool already knows.
        foreach (var view in session.Views)
        {
            if (view.Id == GeometrySession.RootViewId || !GeometryRenderer.IsVisible(obj, view)) continue;
            coordinates[view.Id] = InView(obj, view);
        }

        return new
        {
            name = obj.Name,
            kind = obj.Kind == ObjectKind.Box ? "box" : "point",
            status = obj.Status == ObjectStatus.Accepted ? "accepted" : "editing",
            coordinates,
        };
    }

    /// <summary>Source coordinates, with a box's corners spelled out. Angle conventions differ
    /// between libraries; four corners cannot be misread.</summary>
    private static object InSource(GeometryObject obj)
    {
        if (obj.Kind == ObjectKind.Point || obj.Box is not { } box)
            return new { x = obj.Point.X, y = obj.Point.Y };

        var corners = box.Corners();
        return new
        {
            cx = box.Cx,
            cy = box.Cy,
            width = box.Width,
            height = box.Height,
            angle = box.AngleDeg,
            corners = System.Array.ConvertAll(corners, c => new[] { c.X, c.Y }),
        };
    }

    private static object InView(GeometryObject obj, GeometryView view)
    {
        if (obj.Kind == ObjectKind.Point || obj.Box is not { } box)
        {
            var (x, y) = view.SourceToView.Apply(obj.Point.X, obj.Point.Y);
            return new { x, y };
        }

        var seen = box.Transformed(view.SourceToView);
        return new { cx = seen.Cx, cy = seen.Cy, width = seen.Width, height = seen.Height, angle = seen.AngleDeg };
    }

    private static object Matrix(in Affine t) => new { a = t.A, b = t.B, c = t.C, d = t.D, e = t.E, f = t.F };
}
