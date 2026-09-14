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
/// Places or corrects one point. A separate tool from <c>geom_box</c> on purpose: a point takes two
/// numbers where a box takes five, and one schema covering both shapes is a reliable way to get a
/// box with no size out of a small model. The loop is the same — place, look, nudge, accept.
/// </summary>
internal sealed class GeometryPointTool(ResolvedSettings projectSettings) : GeometryToolBase(projectSettings)
{
    public override string Name => "geom_point";

    protected override ToolEffect Effect => ToolEffect.Write;

    protected override string Description =>
        "Marks a named point on the open image, or corrects one already marked, and returns the " +
        "picture with it drawn as a crosshair. Guess roughly first, look at the result, then call " +
        "again with dx/dy to nudge it.";

    protected override string? Details =>
        "Coordinates are in the pixels of the picture you were last shown, never the original " +
        "image's. Give either x and y or dx and dy in one call, never both. delete:true removes " +
        "the point. Use geom_box for anything that has a size.";

    protected override Dictionary<string, object> Properties => new()
    {
        ["name"] = new
        {
            type = "string",
            description = "Your name for this point. An unknown name creates it; a known one corrects it."
        },
        ["x"] = Field("Position across, in the pixels of the picture you were last shown."),
        ["y"] = Field("Position down, in the pixels of the picture you were last shown."),
        ["dx"] = Field("Move right by this many pixels (negative moves left)."),
        ["dy"] = Field("Move down by this many pixels (negative moves up)."),
        ["delete"] = new
        {
            type = new[] { "boolean", "null" },
            description = "True removes this point entirely. Null = false."
        },
    };

    private static object Field(string description) => new { type = new[] { "number", "null" }, description };

    protected override Task<ToolResult> RunAsync(
        IAgentSession chat, GeometrySettings cfg, JsonElement args, CancellationToken ct)
    {
        var session = GeometrySessionRegistry.TryGet(chat);
        if (session is null) return Task.FromResult(NoSession);

        var name = ToolJson.GetStringTrimmed(args, "name");
        if (name is null) return Task.FromResult(ToolResult.Fail("Error: name is required.", "missing name"));

        var view = session.CurrentView;
        var existing = session.Object(name);

        if (existing is { Kind: ObjectKind.Box })
            return Task.FromResult(ToolResult.Fail(
                $"'{name}' is a box, not a point. Use geom_box for it, or pick another name.",
                "kind mismatch"));

        if (ToolJson.GetBoolean(args, "delete", false))
        {
            if (existing is null)
                return Task.FromResult(ToolResult.Fail($"There is no point called '{name}'.", "unknown point"));
            session.Objects.Remove(existing);
            return Task.FromResult(RenderResult(chat, session, view, $"deleted '{name}'", cfg));
        }

        var absolute = AnyOf(args, "x", "y");
        var relative = AnyOf(args, "dx", "dy");

        if (absolute && relative)
            return Task.FromResult(ToolResult.Fail(
                "geom_point got both absolute coordinates (x, y) and deltas (dx, dy). Send one or " +
                "the other: x and y put the point where you say, dx and dy move it from where it is.",
                "mixed absolute and delta"));

        double px, py;
        string action;

        if (existing is null)
        {
            if (relative)
                return Task.FromResult(ToolResult.Fail(
                    $"There is no point called '{name}' yet, so there is nothing for dx/dy to move. " +
                    "Create it first with x and y.",
                    "delta without point"));

            if (Number(args, "x") is not { } x || Number(args, "y") is not { } y)
                return Task.FromResult(ToolResult.Fail(
                    "A new point needs x and y. Guess roughly — you will see it drawn and can " +
                    "correct it with dx/dy.",
                    "incomplete point"));

            (px, py) = (x, y);
            action = $"created '{name}'";
        }
        else if (!absolute && !relative)
        {
            return Task.FromResult(ToolResult.Fail(
                $"Nothing to change on '{name}'. Pass x and y, or dx and dy, or delete:true. " +
                "To look at the current state again, call geom_view with to:'current'.",
                "no change requested"));
        }
        else
        {
            var seen = view.SourceToView.Apply(existing.Point.X, existing.Point.Y);
            px = absolute ? Number(args, "x") ?? seen.X : seen.X + (Number(args, "dx") ?? 0);
            py = absolute ? Number(args, "y") ?? seen.Y : seen.Y + (Number(args, "dy") ?? 0);
            action = $"updated '{name}'";
        }

        var inSource = view.ViewToSource.Apply(px, py);

        if (existing is null)
        {
            session.Objects.Add(new GeometryObject
            {
                Name = name,
                Kind = ObjectKind.Point,
                Point = inSource,
            });
        }
        else
        {
            existing.Point = inSource;
            existing.Status = ObjectStatus.Editing;
        }

        return Task.FromResult(RenderResult(chat, session, view, action, cfg));
    }
}
