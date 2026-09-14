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
/// Changes what the model is looking at. This is what makes small things placeable: mark the large
/// object, view into it, and the marking on it arrives at a size worth aiming at.
/// <para>
/// Views are never destroyed — going back to <c>source</c> leaves the crop in place, because a view
/// the model has already been shown has to keep meaning what it meant.
/// </para>
/// </summary>
internal sealed class GeometryViewTool(ResolvedSettings projectSettings) : GeometryToolBase(projectSettings)
{
    public override string Name => "geom_view";

    protected override string Description =>
        "Changes which part of the image you are looking at and returns the new picture. Zoom into " +
        "an object by name to place something inside it accurately, into a rectangle of the current " +
        "picture to inspect a corner, or back out to 'source'.";

    protected override string? Details =>
        "rect is [x, y, width, height] in the pixels of the picture you were last shown. After this " +
        "call, every coordinate you pass is in the NEW picture's pixels — the reply states its size. " +
        "Nothing is lost by moving: objects keep their place on the image, and older views stay " +
        "available by id.";

    protected override Dictionary<string, object> Properties => new()
    {
        ["to"] = new
        {
            type = new[] { "string", "null" },
            description = "Where to look: an object's name to zoom into it, 'source' for the whole " +
                          "image, 'parent' to step back out one level, 'current' to redraw what you " +
                          "are looking at, or a view id such as 'crop_1'. Null requires rect."
        },
        ["rect"] = new
        {
            type = new[] { "array", "null" },
            items = new { type = "number" },
            minItems = 4,
            maxItems = 4,
            description = "[x, y, width, height] in the pixels of the picture you were last shown. " +
                          "Null requires 'to'."
        },
        ["padding"] = new
        {
            type = new[] { "number", "null" },
            description = "Margin around the object when zooming to one, as a fraction of its size " +
                          "(0.25 = a quarter of its width on each side). Null uses the project default."
        },
        ["deskew"] = new
        {
            type = new[] { "boolean", "null" },
            description = "When zooming into a rotated box, turn the picture so that box sits " +
                          "horizontally. Null = false."
        },
        ["grid"] = new
        {
            type = new[] { "boolean", "null" },
            description = "Draw the coordinate grid over this view. Null keeps the current setting."
        },
        ["edge_rulers"] = new
        {
            type = new[] { "boolean", "null" },
            description = "Draw a labelled scale along every edge of the box being edited, in that " +
                          "edge's own colour, measuring distance from it in picture pixels — inward " +
                          "and outward. Read a distance straight off it and pass that number as " +
                          "geom_box's 'by'. Null keeps the current setting."
        },
    };

    protected override Task<ToolResult> RunAsync(
        IAgentSession chat, GeometrySettings cfg, JsonElement args, CancellationToken ct)
    {
        var session = GeometrySessionRegistry.TryGet(chat);
        if (session is null) return Task.FromResult(NoSession);

        var current = session.CurrentView;
        var to = Str(args, "to");
        var (rect, rectError) = Rect(args);
        if (rectError is not null) return Task.FromResult(ToolResult.Fail($"geom_view: {rectError}", "bad rect"));

        if (to is null && rect is null)
            return Task.FromResult(ToolResult.Fail(
                "geom_view needs either 'to' (an object name, 'source', 'parent', 'current', or a " +
                "view id) or 'rect' ([x, y, width, height] in the picture you were last shown).",
                "no target"));

        var padding = Number(args, "padding") is { } given ? Math.Clamp(given, 0, 4) : cfg.CropPadding;
        var deskew = ToolJson.GetBoolean(args, "deskew", false);

        var (view, action, error) = Resolve(session, current, to, rect, padding, deskew, cfg);
        if (view is null) return Task.FromResult(ToolResult.Fail($"geom_view: {error}", "unknown target"));

        var isNew = !session.Views.Contains(view);
        if (isNew) session.Views.Add(view);
        session.CurrentViewId = view.Id;

        // A grid asked for once stays on while the model works here: an unmentioned grid keeps what
        // the view had, and a fresh crop inherits the setting from where the model came from.
        view.Grid = Bool(args, "grid") ?? (isNew ? current.Grid : view.Grid);
        view.EdgeRulers = Bool(args, "edge_rulers") ?? (isNew ? current.EdgeRulers : view.EdgeRulers);

        return Task.FromResult(RenderResult(chat, session, view, action!, cfg));
    }

    private static (GeometryView? View, string? Action, string? Error) Resolve(
        GeometrySession session, GeometryView current, string? to, double[]? rect,
        double padding, bool deskew, GeometrySettings cfg)
    {
        if (rect is not null)
        {
            if (rect[2] <= 0 || rect[3] <= 0)
                return (null, null, "rect width and height must be greater than zero.");

            var cropped = ViewBuilder.Crop(session, current, rect[0], rect[1], rect[2], rect[3], cfg);
            return (cropped, $"viewing rectangle [{Text(rect[0])}, {Text(rect[1])}, {Text(rect[2])}, {Text(rect[3])}] of {current.Id}", null);
        }

        switch (to)
        {
            case "current":
                return (current, $"redrawn {current.Id}", null);

            case GeometrySession.RootViewId:
                return (session.Views[0], "back to the whole image", null);

            case "parent":
                if (current.ParentId is not { } parentId || session.View(parentId) is not { } parent)
                    return (session.Views[0], "already at the whole image", null);
                return (parent, $"back to {parent.Id}", null);
        }

        if (session.View(to!) is { } existing)
            return (existing, $"back to {existing.Id}", null);

        if (session.Object(to!) is { } obj)
        {
            var cropped = ViewBuilder.Crop(session, obj, current.Id, padding, deskew, cfg);
            return (cropped, $"zoomed into '{obj.Name}'", null);
        }

        return (null, null,
            $"there is nothing called '{to}'. Pass an object you have marked, 'source', 'parent', " +
            "'current', or a view id you were given.");
    }

    /// <summary>The four numbers of a rectangle, or null when the argument is absent or malformed.
    /// Three numbers is not a rectangle, and reading it as one would crop somewhere arbitrary.</summary>
    private static (double[]? Rect, string? Error) Rect(JsonElement args)
    {
        if (!args.TryGetProperty("rect", out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return (null, null);

        if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() != 4)
            return (null, "rect must be exactly four numbers: [x, y, width, height].");

        var numbers = new double[4];
        var index = 0;
        foreach (var element in value.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Number || !element.TryGetDouble(out var number))
                return (null, "rect must be exactly four numbers: [x, y, width, height].");
            numbers[index++] = number;
        }
        return (numbers, null);
    }

    private static string Text(double value) =>
        Math.Round(value).ToString("0", System.Globalization.CultureInfo.InvariantCulture);
}
