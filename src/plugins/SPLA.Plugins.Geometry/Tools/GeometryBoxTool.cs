using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Geometry.Model;
using SPLA.Plugins.Geometry.Session;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Geometry.Tools;

/// <summary>
/// Places or corrects one oriented box. This is the tool the loop turns on: a rough guess, then
/// <c>dx/dy/dw/dh/dangle</c> corrections read off the returned picture, then <c>geom_accept</c>.
/// </summary>
internal sealed class GeometryBoxTool(ResolvedSettings projectSettings) : GeometryToolBase(projectSettings)
{
    private static readonly string[] AbsoluteFields = ["cx", "cy", "width", "height", "angle"];
    private static readonly string[] DeltaFields = ["dx", "dy", "dw", "dh", "dangle"];

    public override string Name => "geom_box";

    protected override ToolEffect Effect => ToolEffect.Write;

    protected override string Description =>
        "Places a named box on the open image, or corrects one already placed, and returns the " +
        "picture with it drawn. Guess roughly first, look at the result, then call again with " +
        "dx/dy/dw/dh/dangle to nudge it. Two or three corrections are normal.";

    protected override string? Details =>
        "Coordinates are in the pixels of the picture you were last shown, never the original " +
        "image's. Give either the absolute fields (cx, cy, width, height, angle) or the deltas " +
        "(dx, dy, dw, dh, dangle) in one call, never both. A new box needs cx, cy, width and " +
        "height. angle is degrees, positive clockwise. delete:true removes the box.";

    protected override Dictionary<string, object> Properties => new()
    {
        ["name"] = new
        {
            type = "string",
            description = "Your name for this box. An unknown name creates it; a known one corrects it."
        },
        ["cx"] = Field("Centre x, in the pixels of the picture you were last shown."),
        ["cy"] = Field("Centre y, in the pixels of the picture you were last shown."),
        ["width"] = Field("Width across the box's own long axis, in the pixels of that picture."),
        ["height"] = Field("Height across the box's own short axis, in the pixels of that picture."),
        ["angle"] = Field("Rotation in degrees, positive clockwise. Null = 0 on a new box, unchanged on an existing one."),
        ["dx"] = Field("Move right by this many pixels (negative moves left)."),
        ["dy"] = Field("Move down by this many pixels (negative moves up)."),
        ["dw"] = Field("Widen by this many pixels (negative narrows)."),
        ["dh"] = Field("Heighten by this many pixels (negative shortens)."),
        ["dangle"] = Field("Turn by this many degrees, positive clockwise."),
        ["delete"] = new
        {
            type = new[] { "boolean", "null" },
            description = "True removes this box entirely. Null = false."
        },
    };

    private static object Field(string description) => new
    {
        type = new[] { "number", "null" },
        description
    };

    protected override Task<ToolResult> RunAsync(
        IAgentSession chat, GeometrySettings cfg, JsonElement args, CancellationToken ct)
    {
        var session = GeometrySessionRegistry.TryGet(chat);
        if (session is null) return Task.FromResult(NoSession);

        var name = ToolJson.GetStringTrimmed(args, "name");
        if (name is null) return Task.FromResult(ToolResult.Fail("Error: name is required.", "missing name"));

        var view = session.CurrentView;
        var existing = session.Object(name);

        if (existing is { Kind: ObjectKind.Point })
            return Task.FromResult(ToolResult.Fail(
                $"'{name}' is a point, not a box. Use geom_point for it, or pick another name.",
                "kind mismatch"));

        if (ToolJson.GetBoolean(args, "delete", false))
        {
            if (existing is null)
                return Task.FromResult(ToolResult.Fail($"There is no box called '{name}'.", "unknown box"));
            session.Objects.Remove(existing);
            return Task.FromResult(RenderResult(chat, session, view, $"deleted '{name}'", cfg));
        }

        var absolute = AnyOf(args, AbsoluteFields);
        var relative = AnyOf(args, DeltaFields);

        // Mixing the two is not a preference: "cx=400, dx=-20" has no single meaning, and guessing
        // one leaves the model correcting a box that moved somewhere it did not ask for.
        if (absolute && relative)
            return Task.FromResult(ToolResult.Fail(
                $"geom_box got both absolute fields ({Present(args, AbsoluteFields)}) and deltas " +
                $"({Present(args, DeltaFields)}). Send one or the other: absolute fields set the box " +
                "where you say, deltas move it from where it is.",
                "mixed absolute and delta"));

        var toView = view.SourceToView;
        Obb box;
        string action;

        if (existing is null)
        {
            if (relative)
                return Task.FromResult(ToolResult.Fail(
                    $"There is no box called '{name}' yet, so there is nothing for " +
                    $"{Present(args, DeltaFields)} to move. Create it first with cx, cy, width and height.",
                    "delta without box"));

            if (Number(args, "cx") is not { } cx || Number(args, "cy") is not { } cy ||
                Number(args, "width") is not { } width || Number(args, "height") is not { } height)
                return Task.FromResult(ToolResult.Fail(
                    $"A new box needs cx, cy, width and height. Guess roughly — you will see it drawn " +
                    "and can correct it with dx/dy/dw/dh.",
                    "incomplete box"));

            box = new Obb(cx, cy, Size(width), Size(height), Number(args, "angle") ?? 0);
            action = $"created '{name}'";
        }
        else if (!absolute && !relative)
        {
            return Task.FromResult(ToolResult.Fail(
                $"Nothing to change on '{name}'. Pass absolute fields, or deltas, or delete:true. " +
                "To look at the current state again, call geom_view with to:'current'.",
                "no change requested"));
        }
        else
        {
            var seen = existing.Box!.Transformed(toView);
            box = absolute
                ? new Obb(
                    Number(args, "cx") ?? seen.Cx,
                    Number(args, "cy") ?? seen.Cy,
                    Size(Number(args, "width") ?? seen.Width),
                    Size(Number(args, "height") ?? seen.Height),
                    Number(args, "angle") ?? seen.AngleDeg)
                : new Obb(
                    seen.Cx + (Number(args, "dx") ?? 0),
                    seen.Cy + (Number(args, "dy") ?? 0),
                    Size(seen.Width + (Number(args, "dw") ?? 0)),
                    Size(seen.Height + (Number(args, "dh") ?? 0)),
                    seen.AngleDeg + (Number(args, "dangle") ?? 0));
            action = $"updated '{name}'";
        }

        // The model spoke in the picture it was shown; the session keeps source coordinates.
        var inSource = box.Transformed(view.ViewToSource);

        if (existing is null)
        {
            session.Objects.Add(new GeometryObject
            {
                Name = name,
                Kind = ObjectKind.Box,
                Box = inSource,
            });
        }
        else
        {
            existing.Box = inSource;
            // Correcting an accepted box puts it back in play: it is being placed again, and the
            // picture should say so in the only way the model reads — the colour.
            existing.Status = ObjectStatus.Editing;
        }

        return Task.FromResult(RenderResult(chat, session, view, action, cfg));
    }

    /// <summary>A box with no area cannot be seen or corrected, so a size never goes below one pixel.</summary>
    private static double Size(double value) => Math.Max(1, Math.Abs(value));

    /// <summary>Which of <paramref name="fields"/> the call actually carried — an error that names
    /// them is one the model can act on without guessing.</summary>
    private static string Present(JsonElement args, string[] fields) =>
        string.Join(", ", Array.FindAll(fields, f => Number(args, f) is not null));
}
