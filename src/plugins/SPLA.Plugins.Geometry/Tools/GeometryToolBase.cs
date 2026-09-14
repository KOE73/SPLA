using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Geometry.Model;
using SPLA.Plugins.Geometry.Render;
using SPLA.Plugins.Geometry.Session;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Geometry.Tools;

/// <summary>
/// What every geometry tool shares: finding the chat's markup session, parsing arguments without
/// trusting them, and — the important one — building the single reply shape.
/// <para>
/// Every reply states the size of the picture it carries and prints every object's absolute
/// coordinates <b>in that picture's pixels</b>. The model is never asked to remember what it sent or
/// to convert anything: the view→source conversion happens here, on the way in and on the way out,
/// and the source-pixel numbers are never shown at all except by <c>geom_result</c>.
/// </para>
/// </summary>
internal abstract class GeometryToolBase(ResolvedSettings projectSettings) : IMcpTool
{
    public abstract string Name { get; }

    protected abstract string Description { get; }

    /// <summary>The tool's own parameters. The base adds <c>required</c>, <c>additionalProperties</c>
    /// and the strict flag, so every property here must be declared with a nullable type unless it is
    /// genuinely mandatory (agents/tool-args.md).</summary>
    protected abstract Dictionary<string, object> Properties { get; }

    protected virtual string? Details => null;

    protected virtual ToolEffect Effect => ToolEffect.Read;

    protected abstract Task<ToolResult> RunAsync(
        IAgentSession chat, GeometrySettings cfg, JsonElement args, CancellationToken ct);

    public ToolDefinition GetDefinition()
    {
        var properties = Properties;
        return new ToolDefinition
        {
            Type = "function",
            Function = new ToolFunctionDefinition
            {
                Name = Name,
                Description = Description,
                Scope = ToolScope.Project,
                Effect = Effect,
                Risk = ToolRisk.Low,
                StrictSchema = true,
                Details = Details,
                Parameters = new
                {
                    type = "object",
                    properties,
                    required = properties.Keys.ToArray(),
                    additionalProperties = false
                }
            }
        };
    }

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var chat = AgentSessionScope.Current;
        if (chat is null) return ToolResult.Refuse("Error: no active chat session.", "no chat session");

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
        }
        catch (JsonException) { return ToolResult.Fail("Error: invalid JSON arguments.", "invalid json"); }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return ToolResult.Fail("Error: arguments must be a JSON object.", "invalid arguments");

            try
            {
                return await RunAsync(chat, Settings(chat.Settings ?? projectSettings),
                    document.RootElement, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex) { return ToolResult.Fail($"{Name}: {ex.Message}", "geometry operation failed"); }
        }
    }

    /// <summary>The project's settings for this plugin, or the defaults when the file says nothing.</summary>
    internal static GeometrySettings Settings(ResolvedSettings settings)
    {
        settings.Plugins.TryGetValue("geometry", out var section);
        return GeometrySettings.FromBlob(section?.Settings);
    }

    /// <summary>The resolved settings a tool needs for an address lookup (<c>ResourceRegistry.For</c>).</summary>
    protected ResolvedSettings Resolved(IAgentSession chat) => chat.Settings ?? projectSettings;

    protected static ToolResult NoSession => ToolResult.Refuse(
        "No image is open in this chat. Call geom_open with an image address first.", "no geometry session");

    // ── the shared reply ──────────────────────────────────────────────────────

    /// <summary>
    /// Renders <paramref name="view"/> and wraps it the way every tool but <c>geom_result</c> answers:
    /// the picture as a <see cref="ToolImage"/> so the model sees it on its next turn, the same bytes
    /// in the chat's blob store so anything else can take them without going through the context, and
    /// the text that says which pixel space the model is now speaking in.
    /// </summary>
    protected static ToolResult RenderResult(
        IAgentSession chat, GeometrySession session, GeometryView view, string action, GeometrySettings cfg)
    {
        var bytes = GeometryRenderer.Render(session, view, view.Grid, view.BoxGrid, cfg);
        var mime = GeometryRenderer.MimeType(cfg);

        // The frame is other people's content whatever it depicts, and the render is the frame.
        // Named, and the name rotates: every step of the loop used to leave a fresh auto-named blob
        // of about a megabyte that nothing ever released (see GeometrySession.NextRenderName).
        var handle = chat.Blobs.Put(
            BlobPayload.OfBytes(bytes, mime), session.NextRenderName(cfg.RenderHistory), session.Origin);

        // One step of the loop is done and stored: the panel, if anyone has it open, redraws now.
        GeometrySessionRegistry.NotifyUpdated(chat);

        return ToolResult.From(
            new ToolText(Report(session, view, action, handle, cfg)),
            new ToolImage(Convert.ToBase64String(bytes), mime));
    }

    /// <summary>The reply text. Absolute values are printed every time — the model is not required to
    /// remember what it last sent, and re-deriving them is exactly the arithmetic this tool exists to
    /// take off it.</summary>
    private static string Report(
        GeometrySession session, GeometryView view, string action, string handle, GeometrySettings cfg)
    {
        var text = new StringBuilder();
        text.Append(action).Append('\n');

        text.Append("view: ").Append(view.Id).Append(" — ")
            .Append(view.Width).Append('x').Append(view.Height).Append(" px");
        if (view.FromBox is { } from) text.Append(", from box '").Append(from).Append('\'');
        if (view.Deskewed) text.Append(", deskewed");
        text.Append('\n');

        text.Append("ALL coordinates you pass are in this ")
            .Append(view.Width).Append('x').Append(view.Height).Append(" space.\n");

        var here = session.Objects.Where(o => GeometryRenderer.IsVisible(o, view)).ToList();
        var elsewhere = session.Objects.Where(o => !GeometryRenderer.IsVisible(o, view)).ToList();

        if (here.Count == 0)
        {
            text.Append("objects here: none\n");
        }
        else
        {
            text.Append("objects here:\n");
            var nameWidth = Math.Max(4, here.Max(o => o.Name.Length));
            var lines = here.ToDictionary(o => o, o => DescribeInView(o, view));
            var lineWidth = Math.Max(46, lines.Values.Max(line => line.Length));
            foreach (var obj in here)
            {
                text.Append("  ").Append(obj.Name.PadRight(nameWidth)).Append("  ")
                    .Append(obj.Kind == ObjectKind.Box ? "box   " : "point ").Append(' ')
                    .Append(lines[obj].PadRight(lineWidth)).Append("  ")
                    .Append(obj.Status == ObjectStatus.Accepted ? "accepted" : "editing").Append('\n');
                Legend(text, obj, nameWidth);
            }
        }

        if (elsewhere.Count > 0)
            text.Append("outside this view: ").Append(string.Join(", ", elsewhere.Select(o => o.Name))).Append('\n');

        foreach (var obj in here)
        {
            Oversize(text, obj, view);
            Small(text, obj, view, cfg);
        }

        text.Append("stored as ").Append(handle).Append('.');
        return text.ToString();
    }

    /// <summary>
    /// Which colour is which edge, written out rather than left to be inferred from the picture.
    /// <para>
    /// The link "saw a colour → named it in the call" has to be stated somewhere: drawing the colours
    /// and hoping is the same gamble as documenting a convention and hoping, just visual instead of
    /// verbal (ADR_20260914-3 §3.3). It also means the human reading the reply and the human looking
    /// at the panel see the same thing, and that nothing important rests on colour alone.
    /// </para>
    /// <para>
    /// Only for the box being edited: an accepted one has no edge colours to name, and a point has no
    /// edges at all.
    /// </para>
    /// </summary>
    private static void Legend(StringBuilder text, GeometryObject obj, int nameWidth)
    {
        if (obj.Kind != ObjectKind.Box || obj.Status == ObjectStatus.Accepted) return;

        text.Append("  ").Append(new string(' ', nameWidth)).Append("  edges: ")
            .Append(string.Join(", ", GeometryRenderer.EdgeNames.Select(
                (name, i) => $"{name}={GeometryRenderer.EdgeSides[i]}")))
            .Append(" — the box's own sides, so they turn with it; ")
            .Append("white dot in a ring = its centre, the point dx/dy move. ")
            .Append("Name a colour in geom_box's edge to move just that side.\n");
    }

    /// <summary>
    /// Says when a box runs past the edge of the picture it is being judged in.
    /// <para>
    /// This is the trap a live run fell into: a box grew to 1100 px inside a 1024 px view, so its
    /// left and right edges were off-screen and the model could not see that the text was already
    /// enclosed. It widened six more times, repeating its own previous message each turn rather than
    /// reading the new picture.
    /// </para>
    /// <para>
    /// A <b>notice, never an error</b>: a box legitimately larger than the frame is real — a sack
    /// running off the top and bottom of a photograph is correctly marked that way. The tool says
    /// what cannot be seen from here and leaves the judgement where it belongs.
    /// </para>
    /// </summary>
    private static void Oversize(StringBuilder text, GeometryObject obj, GeometryView view)
    {
        if (obj.Box is not { } box) return;

        var corners = box.Transformed(view.SourceToView).Corners();
        var width = corners.Max(c => c.X) - corners.Min(c => c.X);
        var height = corners.Max(c => c.Y) - corners.Min(c => c.Y);

        bool wider = width > view.Width, taller = height > view.Height;
        if (!wider && !taller) return;

        var what = wider && taller ? "wider and taller" : wider ? "wider" : "taller";
        var numbers = wider && taller
            ? $"{Round(width)} > {view.Width} across, {Round(height)} > {view.Height} down"
            : wider ? $"{Round(width)} > {view.Width} across" : $"{Round(height)} > {view.Height} down";
        var edges = wider && taller ? "left, right, top and bottom" : wider ? "left and right" : "top and bottom";

        text.Append("note: '").Append(obj.Name).Append("' is ").Append(what)
            .Append(" than this view (").Append(numbers).Append(") — its ").Append(edges)
            .Append(" edges are off-screen, so you cannot judge them here. That is fine if it really ")
            .Append("runs off the picture; if not, geom_view {to:'").Append(obj.Name)
            .Append("'} shows the whole box.\n");
    }

    /// <summary>A box small enough in this view that it is not worth aiming at is below this share of
    /// the view's shorter side. At the default 1024 px working size that is roughly 150 px — a box
    /// smaller than that is a few percent of the picture's area, where a one-pixel judgement by eye
    /// costs many pixels on the source image, and cropping to it is what the crop stack is for.</summary>
    private const double SmallInView = 0.15;

    /// <summary>A nudge is only worth printing when the crop would genuinely change what the model can
    /// see; below this it is noise on every reply.</summary>
    private const double WorthZooming = 2;

    /// <summary>
    /// The mirror of <see cref="Oversize"/>: the box fits, but it is small enough here that the model
    /// is placing it by eye at a scale it cannot judge. Accuracy in this plugin comes from working
    /// zoomed — in the first live run the model placed five objects in the flat view, never zoomed
    /// once, and only the large ones came out usable — so the reply says so and names the call.
    /// </summary>
    private static void Small(StringBuilder text, GeometryObject obj, GeometryView view, GeometrySettings cfg)
    {
        if (obj.Box is not { } box) return;

        var corners = box.Transformed(view.SourceToView).Corners();
        var width = corners.Max(c => c.X) - corners.Min(c => c.X);
        var height = corners.Max(c => c.Y) - corners.Min(c => c.Y);

        var longest = Math.Max(width, height);
        if (longest <= 0 || longest >= SmallInView * Math.Min(view.Width, view.Height)) return;

        // What geom_view would actually deliver: a crop's longest side becomes render_max_side, and
        // the box occupies all of it but the padding (ViewBuilder.Zoom).
        var zoomed = cfg.RenderMaxSide / (1 + cfg.CropPadding * 2) / longest;
        if (zoomed < WorthZooming) return;

        text.Append("note: '").Append(obj.Name).Append("' is small in this view (")
            .Append(Round(width)).Append('x').Append(Round(height)).Append(" px of ")
            .Append(view.Width).Append('x').Append(view.Height)
            .Append(") — placing it accurately by eye at this size is guesswork. geom_view {to:'")
            .Append(obj.Name).Append("'} shows it about ")
            .Append(Round(zoomed)).Append("x larger, and coordinates there are that much finer.\n");
    }

    /// <summary>One object's numbers, in the pixels of the view the model is looking at. Internal
    /// rather than private because the panel prints the same line the model is told.</summary>
    internal static string DescribeInView(GeometryObject obj, GeometryView view)
    {
        var t = view.SourceToView;
        if (obj.Kind == ObjectKind.Point)
        {
            var (x, y) = t.Apply(obj.Point.X, obj.Point.Y);
            return $"x={Round(x)} y={Round(y)}";
        }

        if (obj.Box is not { } box) return "(no box)";

        var inView = box.Transformed(t);
        return $"cx={Round(inView.Cx)} cy={Round(inView.Cy)} w={Round(inView.Width)} " +
               $"h={Round(inView.Height)} angle={Angle(inView.AngleDeg)}";
    }

    /// <summary>Whole pixels. A tenth of a pixel is below what the model can judge from the picture,
    /// and printing it invites corrections that are noise.</summary>
    private static string Round(double value) =>
        Math.Round(value).ToString("0", CultureInfo.InvariantCulture);

    /// <summary>The angle, and — the part that carries the weight — what it looks like. "Positive is
    /// clockwise" is a convention the model has to hold against a trained prior that says the
    /// opposite (mathematics puts y upward; an image puts it downward), and in a live run the model
    /// lost that bet and never revisited the sign. Naming the tilt on every line printed makes the
    /// sign a consequence the model can check against the picture in front of it.</summary>
    private static string Angle(double degrees)
    {
        var normalised = degrees % 360;
        if (normalised > 180) normalised -= 360;
        if (normalised < -180) normalised += 360;

        var rounded = Math.Round(normalised, 1);
        var number = rounded.ToString("+0.#;-0.#;0", CultureInfo.InvariantCulture);
        return rounded switch
        {
            > 0 => $"{number} (tilted down to the right)",
            < 0 => $"{number} (tilted up to the right)",
            _ => number
        };
    }

    // ── argument helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// An optional string argument, where the string <c>"null"</c> also counts as not supplied.
    /// <para>
    /// Every geometry tool declares <c>StrictSchema</c>, so the model must send every property on
    /// every call — and a small model fills the ones it does not want with the four characters
    /// <c>"null"</c> rather than a JSON null. A live log carries
    /// <c>geom_view {"to":"null","rect":[…]}</c>. It happened to work there because nothing read
    /// <c>to</c>; it would not have if an object were actually named that, and it would not in a
    /// field that is read.
    /// </para>
    /// <para>
    /// An object genuinely called "null" is unreachable through this, and that is the cheaper of the
    /// two mistakes: naming one costs a rename, while reading the placeholder as a name puts a tool
    /// to work on something the model never asked about.
    /// </para>
    /// </summary>
    protected static string? Str(JsonElement args, string name)
    {
        var value = ToolJson.GetStringTrimmed(args, name);
        return value is null || value.Equals("null", StringComparison.OrdinalIgnoreCase) ? null : value;
    }

    /// <summary>An optional boolean argument: absent, JSON-null and anything that is not a JSON
    /// boolean all read as "not supplied", which is what lets a flag fall through to the project
    /// setting instead of being forced to a value by StrictSchema.</summary>
    protected static bool? Bool(JsonElement args, string name)
        => args.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;

    /// <summary>A JSON number as a double. <c>ToolJson</c> has no double reader and lives in the core,
    /// which this plugin does not get to extend; the null-safety contract is the same — absent,
    /// JSON-null and a non-number all read as absent.</summary>
    protected static double? Number(JsonElement args, string name)
        => args.TryGetProperty(name, out var value)
           && value.ValueKind == JsonValueKind.Number
           && value.TryGetDouble(out var number)
           && !double.IsNaN(number) && !double.IsInfinity(number)
            ? number
            : null;

    /// <summary>True when at least one of <paramref name="names"/> was actually supplied as a number.
    /// Use for fields where zero is a real value — a coordinate of 0 or an angle of 0 means something.</summary>
    protected static bool AnyOf(JsonElement args, params string[] names)
        => names.Any(name => Number(args, name) is not null);

    /// <summary>A displacement this call actually asks for, or null. Zero is read as "not supplied".
    /// <para>
    /// StrictSchema puts every property in <c>required</c>, so a model cannot omit the fields it is
    /// not using — it has to put something there, and what it puts is 0. Reading that 0 as intent
    /// makes the mutually-exclusive argument groups impossible to satisfy: the call is refused for
    /// carrying a group it never meant to use, and no rewriting of it can help, because the schema
    /// forbids leaving the group out. A live model deadlocked here until the loop guard killed the
    /// turn. Zero displacement is no displacement, so absence is the honest reading.
    /// </para></summary>
    protected static double? Move(JsonElement args, string name)
        => Number(args, name) is { } value && value != 0 ? value : null;

    /// <summary>True when at least one of <paramref name="names"/> asks for a non-zero displacement.</summary>
    protected static bool AnyMove(JsonElement args, params string[] names)
        => names.Any(name => Move(args, name) is not null);
}
