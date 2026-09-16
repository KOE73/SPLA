using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
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
/// Places a box by asking yes/no questions instead of asking for distances. The model says which
/// numbered probes are on the object and which are not; the tool works out where every edge must be
/// and asks again only where it is still unsure (ADR_20260916).
/// <para>
/// This moves the measuring out of the model entirely. <c>geom_box</c> needs the model to judge
/// "how far" — the one thing it does worst; here it only compares a point with what it sees.
/// </para>
/// </summary>
internal sealed class GeometryProbeTool(ResolvedSettings projectSettings) : GeometryToolBase(projectSettings)
{
    /// <summary>The brake on edge rounds. Five is typical for a clean answer; well past that, more
    /// rounds are buying nothing.</summary>
    private const int MaxEdgeRounds = 15;

    public override string Name => "geom_probe";

    protected override ToolEffect Effect => ToolEffect.Write;

    protected override string Description =>
        "Places or refines a named box by yes/no answers: returns the picture with numbered probes, " +
        "takes back which probes lie on the object and which do not, and moves and turns the box itself.";

    protected override string? Details =>
        "A call with only name starts probing: with no box of that name the probes cover the whole " +
        "picture to find the object; with a box they sit around its edges. Every picture carries a " +
        "round number, and the answer is the next call: name, that round, inside = numbers whose " +
        "point lies on the object, outside = numbers whose point does not. A number left out of both " +
        "lists counts as not answered. The box itself is not drawn during probing: the tool moves and " +
        "turns it from the answers, so nothing about its position is judged or passed. When the probes " +
        "fall on several separate objects, the picture outlines each group with a letter and the next call names one letter in pick. The reply says " +
        "when all four edges are settled; the box is then an ordinary box for geom_accept.";

    protected override Dictionary<string, object> Properties => new()
    {
        ["name"] = new
        {
            type = "string",
            description = "The box being placed. An unknown name is found by a scan of the whole picture."
        },
        ["round"] = new
        {
            type = new[] { "integer", "null" },
            description = "The round number printed with the picture being answered. Null or 0 starts " +
                          "a new probing of this name."
        },
        ["inside"] = Numbers("Probe numbers whose point lies on the object. Null when starting."),
        ["outside"] = Numbers("Probe numbers whose point does not lie on the object. Null when starting."),
        ["pick"] = new
        {
            type = new[] { "string", "null" },
            description = "The letter of one outlined group, when the picture asks to pick one. Null otherwise."
        },
    };

    private static object Numbers(string description) => new
    {
        type = new[] { "array", "null" },
        items = new { type = "integer" },
        description
    };

    protected override Task<ToolResult> RunAsync(
        IAgentSession chat, GeometrySettings cfg, JsonElement args, CancellationToken ct)
        => Task.FromResult(Run(chat, cfg, args));

    private static ToolResult Run(IAgentSession chat, GeometrySettings cfg, JsonElement args)
    {
        var session = GeometrySessionRegistry.TryGet(chat);
        if (session is null) return NoSession;

        var name = Str(args, "name");
        if (name is null) return ToolResult.Fail("Error: name is required.", "missing name");

        var existing = session.Object(name);
        if (existing is { Kind: ObjectKind.Point })
            return ToolResult.Fail($"'{name}' is a point; probes place boxes. Pick another name.", "kind mismatch");

        var round = Number(args, "round") is { } given && given != 0 ? (int)given : (int?)null;
        var (inside, insideError) = Ints(args, "inside");
        var (outside, outsideError) = Ints(args, "outside");
        if ((insideError ?? outsideError) is { } listError) return ToolResult.Fail($"geom_probe: {listError}", "bad numbers");
        var pick = Str(args, "pick");

        var answering = round is not null || inside.Count > 0 || outside.Count > 0 || pick is not null;
        if (!answering) return Start(chat, session, name, existing, cfg);

        var state = session.Probe;
        if (state is null || state.Name != name)
            return ToolResult.Fail(
                $"No probe round is open for '{name}'. Start one with just the name (round null).",
                "no probe round");

        if (round != state.Round)
            return ToolResult.Fail(
                round is null
                    ? $"An answer must carry the round it answers: the current round of '{name}' is {state.Round}."
                    : $"Round {round} is not the current round of '{name}' ({state.Round}). Answer only the latest picture.",
                "wrong round");

        if (Interrupted(session, state, existing))
        {
            session.Probe = null;
            return ToolResult.Fail(
                $"Probing of '{name}' was interrupted: the view changed or another call moved the box, so " +
                "these answers describe a picture that no longer exists. Start again with just the name.",
                "probe round stale");
        }

        return state.Mode switch
        {
            ProbeMode.Pick => AnswerPick(chat, session, state, pick, cfg),
            ProbeMode.Scan => AnswerScan(chat, session, state, inside, outside, cfg),
            _ => AnswerEdges(chat, session, state, existing!, inside, outside, cfg),
        };
    }

    // ── starting ──────────────────────────────────────────────────────────────

    private static ToolResult Start(
        IAgentSession chat, GeometrySession session, string name, GeometryObject? existing, GeometrySettings cfg)
    {
        var view = session.CurrentView;
        var state = new ProbeState { Name = name, ViewId = view.Id };
        session.Probe = state;

        if (existing?.Box is { } box)
        {
            ProbePlanner.StartFromBox(state, box.Transformed(view.SourceToView), view.Width, view.Height);
            state.WrittenBox = existing.Box;
            existing.Status = ObjectStatus.Editing;
            return EdgesRound(chat, session, state, cfg, $"probing the edges of '{name}'");
        }

        state.Mode = ProbeMode.Scan;
        return ScanRound(chat, session, state, cfg, $"scanning for '{name}'");
    }

    private static ToolResult ScanRound(
        IAgentSession chat, GeometrySession session, ProbeState state, GeometrySettings cfg, string action)
    {
        var view = session.CurrentView;
        state.Round = session.NextProbeRound();
        var random = new Random(ProbePlanner.Seed(state.Name, state.Round));
        state.Probes = ProbePlanner.Number(
            ProbePlanner.Scan(state, view.Width, view.Height, cfg.ProbeScanSpacing, cfg.ProbeLayout == "jitter",
                random, Covered(session, view, state.Name)),
            random);
        state.Groups = [];

        var text = new StringBuilder(action).Append('\n');
        RoundHeader(text, state, Style(session, cfg));
        text.Append("question: does the point of each probe lie on an object of the kind you are marking as '")
            .Append(state.Name).Append("'? If several such objects are visible, answer for all of them — they ")
            .Append("are told apart afterwards. Gaps inside one object (between the letters of one inscription) ")
            .Append("count as on it.\n");
        AnswerLine(text, state);

        return RenderResult(chat, session, view, text.ToString(), cfg, Overlay(session, cfg, state.Probes, []));
    }

    private static ToolResult EdgesRound(
        IAgentSession chat, GeometrySession session, ProbeState state, GeometrySettings cfg, string action)
    {
        var view = session.CurrentView;
        var cuts = ProbePlanner.Cuts(state);

        var text = new StringBuilder(action).Append('\n');
        EdgeStatus(text, state, cuts, cfg);

        if (Enumerable.Range(0, 4).All(edge => ProbePlanner.Settled(state, cuts, edge, cfg.ProbeTolerance)))
            return Finish(chat, session, state, cfg, text, $"all four edges of '{state.Name}' are settled");

        // Answers that never agree — a model contradicting itself, or an object no box describes — would
        // otherwise keep the rounds coming for ever. Stop, keep the best box so far, and say which edges
        // did not settle.
        if (++state.EdgeRounds > MaxEdgeRounds)
            return Finish(chat, session, state, cfg, text,
                $"'{state.Name}' is left where the answers put it after {MaxEdgeRounds} rounds: " +
                string.Join(", ", Enumerable.Range(0, 4).Where(edge => !ProbePlanner.Settled(state, cuts, edge, cfg.ProbeTolerance))
                    .Select(edge => GeometryRenderer.EdgeNames[edge])) + " never settled");

        state.Round = session.NextProbeRound();
        var random = new Random(ProbePlanner.Seed(state.Name, state.Round));
        var layout = new ProbeLayout(cfg.ProbeSpacing, cfg.ProbeScanSpacing, cfg.ProbeRows, cfg.ProbeTolerance,
            cfg.ProbeLayout == "jitter", Style(session, cfg).FontSize * 1.6);
        state.Probes = ProbePlanner.Number(
            ProbePlanner.Edges(state, view.Width, view.Height, layout, random, Covered(session, view, state.Name)),
            random);
        state.Groups = [];

        // Every open band lies outside the picture: nothing left that can be asked about here.
        if (state.Probes.Count == 0)
            return Finish(chat, session, state, cfg, text,
                $"the open edges of '{state.Name}' run off this view, so they are left where the answers put them");

        RoundHeader(text, state, Style(session, cfg));
        text.Append("question: does the point of each probe lie on the area the box '").Append(state.Name)
            .Append("' has to enclose? ")
            .Append(cfg.ProbeShowBox ? "The coloured box is the current estimate, not the answer — judge the picture under it. " : "")
            .Append("Gaps inside the object (between the letters of one inscription) count as on it.\n");
        AnswerLine(text, state);

        return RenderResult(chat, session, view, text.ToString(), cfg, Overlay(session, cfg, state.Probes, []));
    }

    private static ToolResult Finish(
        IAgentSession chat, GeometrySession session, ProbeState state, GeometrySettings cfg, StringBuilder text, string why)
    {
        session.Probe = null;
        text.Append(why).Append(" (to within ").Append(cfg.ProbeTolerance).Append(" px). No more probes; '")
            .Append(state.Name).Append("' is an ordinary box now — geom_accept it when it sits right, or ")
            .Append("geom_probe it again with just the name to re-check.");
        return RenderResult(chat, session, session.CurrentView, text.ToString(), cfg);
    }

    // ── answers ───────────────────────────────────────────────────────────────

    private static ToolResult AnswerScan(
        IAgentSession chat, GeometrySession session, ProbeState state,
        HashSet<int> inside, HashSet<int> outside, GeometrySettings cfg)
    {
        if (Validate(state, inside, outside) is { } invalid) return invalid;

        var found = state.Probes.Where(p => inside.Contains(p.Number)).ToList();
        state.ScanAnswers =
        [
            .. state.Probes.Where(p => inside.Contains(p.Number)).Select(p => (p, true)),
            .. state.Probes.Where(p => outside.Contains(p.Number)).Select(p => (p, false)),
        ];
        if (found.Count == 0)
            return ScanRound(chat, session, state, cfg,
                $"no probe was answered as on '{state.Name}'. If it is there but smaller than the gaps between " +
                "probes, geom_view closer to it and start again; the probes below are renumbered");

        var groups = ProbePlanner.Groups(found);
        if (groups.Count == 1) return Found(chat, session, state, groups[0], cfg, $"found '{state.Name}'");

        state.Mode = ProbeMode.Pick;
        state.Groups = groups;
        state.Probes = [];
        state.Round = session.NextProbeRound();

        var text = new StringBuilder()
            .Append("the probes on '").Append(state.Name).Append("' form ").Append(groups.Count)
            .Append(" separate groups, outlined and lettered ")
            .Append(string.Join(", ", groups.Select(g => g.Letter))).Append(".\n")
            .Append("round ").Append(state.Round).Append(": pick the one to mark as '").Append(state.Name)
            .Append("' — geom_probe {name:'").Append(state.Name).Append("', round:").Append(state.Round)
            .Append(", pick:'A'}. The others can be marked afterwards under their own names; once accepted ")
            .Append("they are veiled and get no probes.");

        return RenderResult(chat, session, session.CurrentView, text.ToString(), cfg, Overlay(session, cfg, [], groups));
    }

    private static ToolResult AnswerPick(
        IAgentSession chat, GeometrySession session, ProbeState state, string? pick, GeometrySettings cfg)
    {
        var letters = string.Join(", ", state.Groups.Select(g => $"'{g.Letter}'"));
        if (pick is null)
            return ToolResult.Fail($"Round {state.Round} asks to pick one group: {letters}.", "pick missing");

        var group = state.Groups.FirstOrDefault(g => char.ToUpperInvariant(pick[0]) == g.Letter && pick.Length == 1);
        if (group is null)
            return ToolResult.Fail($"'{pick}' is not one of the groups on the picture: {letters}.", "unknown group");

        return Found(chat, session, state, group, cfg, $"picked group {group.Letter} as '{state.Name}'");
    }

    /// <summary>A group becomes the box and the first round of edges begins at once — there is nothing
    /// for the model to look at in between.</summary>
    private static ToolResult Found(
        IAgentSession chat, GeometrySession session, ProbeState state, ProbeGroup group, GeometrySettings cfg, string action)
    {
        var view = session.CurrentView;
        ProbePlanner.StartFromGroup(state, group, view.Width, view.Height);

        var box = ProbePlanner.Box(state).Transformed(view.ViewToSource);
        if (session.Object(state.Name) is { } existing) existing.Box = box;
        else session.Objects.Add(new GeometryObject { Name = state.Name, Kind = ObjectKind.Box, Box = box });
        state.WrittenBox = box;

        return EdgesRound(chat, session, state, cfg, action);
    }

    private static ToolResult AnswerEdges(
        IAgentSession chat, GeometrySession session, ProbeState state, GeometryObject target,
        HashSet<int> inside, HashSet<int> outside, GeometrySettings cfg)
    {
        if (Validate(state, inside, outside) is { } invalid) return invalid;
        if (inside.Count == 0 && outside.Count == 0)
            return ToolResult.Fail(
                $"Round {state.Round} got no numbers. List at least one probe in inside or outside.", "empty answer");

        ProbePlanner.Record(state, inside, outside);

        var view = session.CurrentView;
        target.Box = ProbePlanner.Box(state).Transformed(view.ViewToSource);
        target.Status = ObjectStatus.Editing;
        state.WrittenBox = target.Box;

        var answered = inside.Count + outside.Count;
        return EdgesRound(chat, session, state, cfg,
            $"moved the edges of '{target.Name}' by {answered} answer{(answered == 1 ? "" : "s")} " +
            $"({state.Probes.Count - answered} left unanswered)");
    }

    /// <summary>Every number must be one on the picture, and none may be both inside and outside.</summary>
    private static ToolResult? Validate(ProbeState state, HashSet<int> inside, HashSet<int> outside)
    {
        var known = state.Probes.Select(p => p.Number).ToHashSet();
        var unknown = inside.Concat(outside).Where(n => !known.Contains(n)).Distinct().Order().ToList();
        if (unknown.Count > 0)
            return ToolResult.Fail(
                $"Round {state.Round} has no probe numbered {string.Join(", ", unknown)}; its probes are 1..{state.Probes.Count}.",
                "unknown probe");

        var both = inside.Intersect(outside).Order().ToList();
        if (both.Count > 0)
            return ToolResult.Fail(
                $"{string.Join(", ", both)} listed as both inside and outside. Put each number in one list, " +
                "or leave it out if unsure.",
                "contradictory answer");

        return null;
    }

    /// <summary>Answers are offsets in the view the round began in, from a box this state wrote. A
    /// different view or a box moved by anyone else makes them describe something that is gone.</summary>
    private static bool Interrupted(GeometrySession session, ProbeState state, GeometryObject? target)
    {
        if (session.CurrentViewId != state.ViewId) return true;
        return state.Mode == ProbeMode.Edges
            ? target?.Box is null || !ReferenceEquals(target.Box, state.WrittenBox)
            : target is not null;
    }

    // ── text ──────────────────────────────────────────────────────────────────

    private static void RoundHeader(StringBuilder text, ProbeState state, ProbeStyle style)
    {
        text.Append("round ").Append(state.Round).Append(": ").Append(state.Probes.Count).Append(" numbered probes; ")
            .Append(style.Describe())
            .Append(" Numbers are shuffled and say nothing about position.\n");
    }

    private static void AnswerLine(StringBuilder text, ProbeState state) =>
        text.Append("answer: geom_probe {name:'").Append(state.Name).Append("', round:").Append(state.Round)
            .Append(", inside:[…], outside:[…]}. Leave out any number you are not sure of — a missing answer ")
            .Append("costs a probe, a wrong one costs a round.\n");

    private static void EdgeStatus(StringBuilder text, ProbeState state, EdgeCut[] cuts, GeometrySettings cfg)
    {
        text.Append("edges: ");
        text.Append(string.Join("; ", cuts.Select((cut, i) =>
        {
            var edge = $"{GeometryRenderer.EdgeNames[i]} ({GeometryRenderer.EdgeSides[i]})";
            if (ProbePlanner.Settled(state, cuts, i, cfg.ProbeTolerance)) return $"{edge} settled";
            if (cut.Settled(cfg.ProbeTolerance)) return $"{edge} open, its ends still depend on the tilt";
            return cut.Disagreements > 0
                ? $"{edge} open, {cut.Disagreements} answer{(cut.Disagreements == 1 ? "" : "s")} disagree — re-checking"
                : $"{edge} open, within {Math.Ceiling(cut.Width).ToString(CultureInfo.InvariantCulture)} px";
        })));
        text.Append('\n');
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    /// <summary>The style proven legible in this session by <c>geom_probe_legibility</c>, else the settings'.</summary>
    private static ProbeStyle Style(GeometrySession session, GeometrySettings cfg) =>
        session.ProbeStyle ?? ProbeStyle.From(cfg);

    private static ProbeOverlay Overlay(
        GeometrySession session, GeometrySettings cfg, IReadOnlyList<Probe> probes, IReadOnlyList<ProbeGroup> groups) =>
        new(probes, groups, Style(session, cfg), cfg.ProbeShowBox);

    /// <summary>Accepted boxes in this view, other than the one being probed: veiled, and never probed.</summary>
    private static List<Obb> Covered(GeometrySession session, GeometryView view, string name) =>
        [.. session.Objects
            .Where(o => o.Status == ObjectStatus.Accepted && o.Name != name && o.Box is not null)
            .Select(o => o.Box!.Transformed(view.SourceToView))];

    /// <summary>A list of probe numbers. Absent, null, the string "null" and an empty array are all an
    /// empty set; a whole-number double or a numeric string is accepted, since a model writes both.</summary>
    internal static (HashSet<int> Numbers, string? Error) Ints(JsonElement args, string name)
    {
        var numbers = new HashSet<int>();
        if (!args.TryGetProperty(name, out var value) || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return (numbers, null);
        if (value.ValueKind == JsonValueKind.String && value.GetString()?.Trim().ToLowerInvariant() is "null" or "")
            return (numbers, null);
        if (value.ValueKind != JsonValueKind.Array)
            return (numbers, $"{name} must be an array of probe numbers, e.g. [3, 17].");

        foreach (var element in value.EnumerateArray())
        {
            double? number = element.ValueKind switch
            {
                JsonValueKind.Number when element.TryGetDouble(out var d) => d,
                JsonValueKind.String when double.TryParse(element.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) => d,
                _ => null,
            };
            if (number is not { } n || n != Math.Floor(n))
                return (numbers, $"{name} must hold whole probe numbers only; got {element.GetRawText()}.");
            numbers.Add((int)n);
        }
        return (numbers, null);
    }
}
