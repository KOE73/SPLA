using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Geometry.Model;
using SPLA.Plugins.Geometry.Session;
using SPLA.Plugins.Geometry.Render;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Geometry.Tools;

/// <summary>
/// Places or corrects one oriented box. This is the tool the loop turns on: a rough guess, then
/// corrections read off the returned picture — <c>dx/dy/dw/dh/dangle</c> for the box as a whole, or
/// <c>edge</c>+<c>by</c> for one side named by the colour it is drawn in — then <c>geom_accept</c>.
/// </summary>
internal sealed class GeometryBoxTool(ResolvedSettings projectSettings) : GeometryToolBase(projectSettings)
{
    private static readonly string[] AbsoluteFields = ["cx", "cy", "width", "height", "angle"];
    private static readonly string[] DeltaFields = ["dx", "dy", "dw", "dh", "dangle"];

    public override string Name => "geom_box";

    protected override ToolEffect Effect => ToolEffect.Write;

    protected override string Description =>
        "Places a named box on the open image, or corrects one already placed, and returns the " +
        "picture with it drawn. Guess roughly first, then look at the returned picture and correct " +
        "what you see. Stop when the outline sits on the thing and you cannot name a side that is " +
        "still wrong — then call geom_accept. If a correction did not make the picture better, do " +
        "not repeat it smaller: change approach instead — move one side with edge/by rather than the " +
        "whole size, check the tilt words against the picture in case the angle's sign is wrong, or " +
        "geom_view onto the box and work larger.";

    protected override string? Details =>
        "Coordinates are in the pixels of the picture you were last shown, never the original " +
        "image's. One call does one of three things, never two: the absolute fields " +
        "(cx, cy, width, height, angle), the deltas (dx, dy, dw, dh, dangle), or one edge " +
        "(edge + by). A new box needs cx, cy, width and height. delete:true removes the box. " +
        "angle is degrees and turns the box clockwise on the picture: angle=10 leaves it tilted " +
        "DOWN to the right, angle=-10 tilted UP to the right. Every reply names the tilt in words " +
        "next to the number — read it back and check it against what you see before nudging the size. " +
        "The box being placed is drawn with a colour on each of its four sides and the reply says " +
        "which is which; edge takes one of those colour names. Worked example: the picture shows the " +
        "text running past the green (left) side and stopping short of the magenta (right) one, so " +
        "{edge:'green', by:30} pushes the left side 30 px OUT to take the text in, and " +
        "{edge:'magenta', by:-15} pulls the right side 15 px IN off the empty space. Positive is " +
        "always outward, whichever side you name and however the box is turned; dw/dh are still there " +
        "for when both opposite sides need moving at once.";

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
        ["angle"] = Field(
            "Rotation in degrees, clockwise on the picture: 10 tilts the box down to the right, " +
            "-10 tilts it up to the right. Null = 0 on a new box, unchanged on an existing one."),
        ["dx"] = Field("Move right by this many pixels (negative moves left)."),
        ["dy"] = Field("Move down by this many pixels (negative moves up)."),
        ["dw"] = Field("Widen by this many pixels (negative narrows)."),
        ["dh"] = Field("Heighten by this many pixels (negative shortens)."),
        ["dangle"] = Field(
            "Turn by this many degrees, clockwise on the picture: dangle=5 drops the right-hand end " +
            "further down, dangle=-5 lifts it. If the box leans the wrong way, the sign is wrong — " +
            "flip it rather than turning further the same way."),
        ["edge"] = new
        {
            type = new[] { "string", "null" },
            description =
                "Move ONE side of the box, named by its colour in the picture: 'cyan' (top), " +
                "'magenta' (right), 'yellow' (bottom) or 'green' (left). The colours belong to the " +
                "box, not to the screen, so they stay with the same side however the box is turned. " +
                "Needs 'by'."
        },
        ["by"] = Field(
            "How far to move the edge named in 'edge'. POSITIVE moves it OUTWARD, away from the " +
            "box's centre, so the box covers more; negative moves it inward and the box covers less. " +
            "Outward is the same direction for all four edges and does not depend on the rotation."),
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

        var name = Str(args, "name");
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
        var edgeName = Str(args, "edge");
        var edgeBy = Number(args, "by");

        if (edgeName is not null || edgeBy is not null)
        {
            // One side or the whole box, never both in one call: "edge:'cyan', by:20, dh:-10" has no
            // single meaning, and the same reasoning already refuses absolute fields mixed with deltas.
            if (absolute || relative)
                return Task.FromResult(ToolResult.Fail(
                    "geom_box got edge/by together with " +
                    $"{Present(args, absolute ? AbsoluteFields : DeltaFields)}. Moving one side is its " +
                    "own call: send edge and by alone, or the absolute fields, or the deltas.",
                    "mixed edge and other fields"));

            var moved = MoveEdge(view, existing, name, edgeName, edgeBy);
            return Task.FromResult(moved.Error ?? RenderResult(chat, session, view, moved.Action!, cfg));
        }

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

    /// <summary>
    /// Moves one side of the box, named by the colour it is drawn in.
    /// <para>
    /// This is the arithmetic the tool exists to take off the model. In a live run it wrote, six times
    /// word for word, that one marking was cut off on the left and another on the right — it thinks in
    /// edges — while the only size control was <c>dw</c>, symmetric about the centre. Moving one side
    /// through <c>dw</c> means working out "widen by 40 and shift by 20", which is exactly the
    /// translation we took away everywhere else (ADR_20260914-3 §1).
    /// </para>
    /// <para>
    /// The side moves along the <b>box's own axis</b>, so it is still the same side after a rotation,
    /// and "outward" is the only convention left: it means the same thing for all four edges and owes
    /// nothing to where the screen's top is.
    /// </para>
    /// </summary>
    private static (ToolResult? Error, string? Action) MoveEdge(
        GeometryView view, GeometryObject? existing, string name, string? edgeName, double? by)
    {
        var colours = string.Join(", ", GeometryRenderer.EdgeNames.Select(
            (c, i) => $"'{c}' ({GeometryRenderer.EdgeSides[i]})"));

        if (edgeName is null)
            return (ToolResult.Fail(
                $"geom_box got by={Text(by)} but no edge to move. Name the side by its colour: {colours}.",
                "by without edge"), null);

        var index = Array.FindIndex(
            GeometryRenderer.EdgeNames, c => c.Equals(edgeName, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return (ToolResult.Fail(
                $"'{edgeName}' is not an edge of the box. The four are {colours}, and each reply " +
                "names which colour is which side.",
                "unknown edge"), null);

        if (by is not { } amount || amount == 0)
            return (ToolResult.Fail(
                $"geom_box got edge:'{edgeName}' but no distance. Pass by: positive moves that side " +
                "outward, away from the centre; negative moves it inward.",
                "edge without by"), null);

        if (existing?.Box is null)
            return (ToolResult.Fail(
                $"There is no box called '{name}' yet, so it has no {edgeName} edge to move. " +
                "Create it first with cx, cy, width and height.",
                "edge without box"), null);

        var seen = existing.Box.Transformed(view.SourceToView);
        var vertical = index is 0 or 2;
        var size = vertical ? seen.Height : seen.Width;

        // Pulling a side in past the opposite one would turn the box inside out; it stops at one pixel.
        var delta = Math.Max(amount, 1 - size);

        // Which way "outward" points in the box's own frame, before the rotation is applied.
        (double X, double Y) outward = index switch
        {
            0 => (0, -1),
            1 => (1, 0),
            2 => (0, 1),
            _ => (-1, 0),
        };

        var rad = seen.AngleDeg * Math.PI / 180.0;
        double cos = Math.Cos(rad), sin = Math.Sin(rad);
        var shift = delta / 2;
        var dx = (outward.X * cos - outward.Y * sin) * shift;
        var dy = (outward.X * sin + outward.Y * cos) * shift;

        var box = new Obb(
            seen.Cx + dx,
            seen.Cy + dy,
            Size(vertical ? seen.Width : size + delta),
            Size(vertical ? size + delta : seen.Height),
            seen.AngleDeg);

        existing.Box = box.Transformed(view.ViewToSource);
        existing.Status = ObjectStatus.Editing;

        var direction = delta > 0 ? "outward" : "inward";
        return (null,
            $"moved the {GeometryRenderer.EdgeNames[index]} ({GeometryRenderer.EdgeSides[index]}) edge " +
            $"of '{name}' {direction} by {Text(Math.Abs(delta))} px");
    }

    private static string Text(double? value) =>
        value is { } v ? v.ToString("0.#", CultureInfo.InvariantCulture) : "nothing";

    /// <summary>A box with no area cannot be seen or corrected, so a size never goes below one pixel.</summary>
    private static double Size(double value) => Math.Max(1, Math.Abs(value));

    /// <summary>Which of <paramref name="fields"/> the call actually carried — an error that names
    /// them is one the model can act on without guessing.</summary>
    private static string Present(JsonElement args, string[] fields) =>
        string.Join(", ", Array.FindAll(fields, f => Number(args, f) is not null));
}
