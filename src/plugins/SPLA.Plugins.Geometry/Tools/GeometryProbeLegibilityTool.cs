using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Plugins.Geometry.Render;
using SPLA.Plugins.Geometry.Session;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Geometry.Tools;

/// <summary>
/// Finds the smallest probe look a model actually reads, by making it read (ADR_20260916-3). One call
/// draws every look at every size over the current picture, each with its own random number; the
/// answer lists the numbers read, and <c>geom_probe</c> draws that look for the rest of the session.
/// <para>
/// Probe numbers that are too large hide the picture the question is about; too small, and they are
/// read anyway, wrongly. Which size is right depends on the model and on the picture, so the tool
/// measures it rather than having a setting guessed.
/// </para>
/// </summary>
internal sealed class GeometryProbeLegibilityTool(ResolvedSettings projectSettings) : GeometryToolBase(projectSettings)
{
    public override string Name => "geom_probe_legibility";

    protected override string Description =>
        "Checks which probe numbers can be read on this picture: draws a chart of numbers in several " +
        "looks and sizes, takes back the numbers read, and makes geom_probe use the smallest look read.";

    protected override string? Details =>
        "A call with round null draws the chart over the current view; every cell is a different " +
        "two-digit number. The answer is the next call with that round and read = every number read " +
        "with certainty. A number guessed wrong counts against nothing, but a number guessed right " +
        "counts as read, and probes are then drawn that small. The choice holds for this image's session.";

    protected override Dictionary<string, object> Properties => new()
    {
        ["round"] = new
        {
            type = new[] { "integer", "null" },
            description = "The round printed with the chart being answered. Null or 0 draws a new chart."
        },
        ["read"] = new
        {
            type = new[] { "array", "null" },
            items = new { type = "integer" },
            description = "Every number on the chart that can be read with certainty. Null when drawing."
        },
    };

    protected override Task<ToolResult> RunAsync(
        IAgentSession chat, GeometrySettings cfg, JsonElement args, CancellationToken ct)
        => Task.FromResult(Run(chat, cfg, args));

    private static ToolResult Run(IAgentSession chat, GeometrySettings cfg, JsonElement args)
    {
        var session = GeometrySessionRegistry.TryGet(chat);
        if (session is null) return NoSession;

        var round = Number(args, "round") is { } given && given != 0 ? (int)given : (int?)null;
        var (read, error) = GeometryProbeTool.Ints(args, "read");
        if (error is not null) return ToolResult.Fail($"geom_probe_legibility: {error}", "bad numbers");

        if (round is null && read.Count == 0) return Draw(chat, session, cfg);

        if (session.Chart is not { } chart)
            return ToolResult.Fail("No legibility chart is waiting for an answer. Draw one with round null.", "no chart");
        if (round != chart.Round)
            return ToolResult.Fail(
                $"Round {(round is null ? "null" : round)} is not the chart's round ({chart.Round}). Answer only the latest chart.",
                "wrong round");

        return Answer(session, chart, read, cfg);
    }

    private static ToolResult Draw(IAgentSession chat, GeometrySession session, GeometrySettings cfg)
    {
        var view = session.CurrentView;
        var chart = LegibilityChart.Build(session.NextProbeRound(), view.Width, view.Height, cfg.ProbeColor);
        session.Chart = chart;

        var text = new StringBuilder()
            .Append("legibility chart, round ").Append(chart.Round).Append(": ").Append(chart.Probes.Count)
            .Append(" two-digit numbers in different looks and sizes are drawn over the picture.\n")
            .Append("answer: geom_probe_legibility {round:").Append(chart.Round)
            .Append(", read:[…]} with every number you can read with certainty. Leave out any you would ")
            .Append("have to guess: the smallest numbers read decide how small probes are drawn.");

        return RenderResult(chat, session, view, text.ToString(), cfg,
            new ProbeOverlay(chart.Probes, [], ProbeStyle.From(cfg), Styles: chart.Styles));
    }

    private static ToolResult Answer(GeometrySession session, LegibilityChart chart, HashSet<int> read, GeometrySettings cfg)
    {
        session.Chart = null;
        var onChart = chart.Probes.Select(p => p.Number).ToHashSet();
        var misread = read.Where(n => !onChart.Contains(n)).Order().ToList();
        var thresholds = chart.Thresholds(read);

        var text = new StringBuilder("read ").Append(read.Count - misread.Count).Append(" of ").Append(chart.Probes.Count);
        if (misread.Count > 0)
            text.Append("; not on the chart, so misread: ").Append(string.Join(", ", misread));
        text.Append('\n');

        for (var row = 0; row < LegibilityChart.Looks.Length; row++)
        {
            var (marker, label) = LegibilityChart.Looks[row];
            var name = marker == "badge" ? marker : $"{marker} + {label}";
            text.Append("  ").Append(name.PadRight(16))
                .Append(thresholds[row] is { } size ? $"read from {size}px up" : "not read at every size")
                .Append('\n');
        }

        if (chart.Choose(read, cfg.ProbeColor) is not { } chosen)
            return ToolResult.Text(text.Append("No look was read at all sizes up to the largest; geom_probe keeps ")
                .Append(session.ProbeStyle ?? ProbeStyle.From(cfg)).Append('.').ToString());

        session.ProbeStyle = chosen;
        return ToolResult.Text(text
            .Append("geom_probe now draws ").Append(chosen).Append(" for this image. ")
            .Append("Settings to keep it: probe_marker=").Append(chosen.Marker)
            .Append(", probe_label=").Append(chosen.Label)
            .Append(", probe_font_size=").Append(chosen.FontSize).Append('.').ToString());
    }
}
