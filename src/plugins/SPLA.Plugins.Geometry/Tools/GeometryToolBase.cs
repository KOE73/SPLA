using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
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
        var bytes = GeometryRenderer.Render(session, view, view.Grid, cfg);
        var mime = GeometryRenderer.MimeType(cfg);

        // The frame is other people's content whatever it depicts, and the render is the frame.
        // Named, and the name rotates: every step of the loop used to leave a fresh auto-named blob
        // of about a megabyte that nothing ever released (see GeometrySession.NextRenderName).
        var handle = chat.Blobs.Put(
            BlobPayload.OfBytes(bytes, mime), session.NextRenderName(cfg.RenderHistory), session.Origin);

        return ToolResult.From(
            new ToolText(Report(session, view, action, handle)),
            new ToolImage(Convert.ToBase64String(bytes), mime));
    }

    /// <summary>The reply text. Absolute values are printed every time — the model is not required to
    /// remember what it last sent, and re-deriving them is exactly the arithmetic this tool exists to
    /// take off it.</summary>
    private static string Report(GeometrySession session, GeometryView view, string action, string handle)
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
            foreach (var obj in here)
                text.Append("  ").Append(obj.Name.PadRight(nameWidth)).Append("  ")
                    .Append(obj.Kind == ObjectKind.Box ? "box   " : "point ").Append(' ')
                    .Append(Describe(obj, view).PadRight(46))
                    .Append(obj.Status == ObjectStatus.Accepted ? "accepted" : "editing").Append('\n');
        }

        if (elsewhere.Count > 0)
            text.Append("outside this view: ").Append(string.Join(", ", elsewhere.Select(o => o.Name))).Append('\n');

        text.Append("stored as ").Append(handle).Append('.');
        return text.ToString();
    }

    /// <summary>One object's numbers, in the pixels of the view the model is looking at.</summary>
    private static string Describe(GeometryObject obj, GeometryView view)
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

    private static string Angle(double degrees)
    {
        var normalised = degrees % 360;
        if (normalised > 180) normalised -= 360;
        if (normalised < -180) normalised += 360;
        return Math.Round(normalised, 1).ToString("0.#", CultureInfo.InvariantCulture);
    }

    // ── argument helpers ──────────────────────────────────────────────────────

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

    /// <summary>True when at least one of <paramref name="names"/> was actually supplied as a number.</summary>
    protected static bool AnyOf(JsonElement args, params string[] names)
        => names.Any(name => Number(args, name) is not null);
}
