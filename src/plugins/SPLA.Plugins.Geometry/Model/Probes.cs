using System;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.Plugins.Geometry.Model;

/// <summary>Which question the current probe round asks. See ADR_20260916 §2.1.</summary>
internal enum ProbeMode
{
    /// <summary>No box yet: probes cover the whole view, the answer finds the object.</summary>
    Scan,

    /// <summary>The scan answer fell apart into several separate groups; the model picks one by letter.</summary>
    Pick,

    /// <summary>A box exists: probes sit only in the edges' remaining bands of uncertainty.</summary>
    Edges
}

/// <summary>
/// One numbered probe, in the pixels of the view the round was started in.
/// </summary>
/// <param name="Edge">The edge this probe answers for (index into the frozen edge palette), or -1 in a scan.</param>
/// <param name="Offset">Distance from the probing frame's origin along that edge's outward normal. Unused in a scan.</param>
/// <param name="Column">Lattice column in a scan — what "neighbour" means when inside probes are grouped.</param>
/// <param name="Row">Lattice row in a scan.</param>
/// <param name="PlateX">Unit direction the number plate is pushed in, away from where the answer is decided.</param>
/// <param name="PlateY">See <paramref name="PlateX"/>.</param>
internal sealed record Probe(
    int Number, double X, double Y, int Edge, double Offset, int Column, int Row, double PlateX, double PlateY);

/// <summary>A connected group of inside probes from a scan, as an axis-aligned rectangle of view pixels.</summary>
internal sealed record ProbeGroup(char Letter, double MinX, double MinY, double MaxX, double MaxY, int Count);

/// <summary>
/// One answer, kept as the point it was about rather than as a distance from an edge — so it stays
/// true when the probing frame turns (ADR_20260916-2).
/// </summary>
/// <param name="Edge">The edge an OUTSIDE answer speaks for, or -1. An inside point is inside the box, so it
/// speaks for all four edges at once; an outside point is only known to be past the edge it was placed
/// for.</param>
/// <param name="Assumed">A starting guess rather than an answer: it counts only until the edge has a real
/// answer on each side.</param>
internal readonly record struct ProbeAnswer(double X, double Y, int Edge, bool Inside, bool Assumed = false);

/// <summary>An answer as one edge sees it in one frame: how far out along that edge's normal, and which side.</summary>
internal readonly record struct Sample(double Offset, bool Inside);

/// <summary>
/// Where the answers put one edge.
/// </summary>
/// <param name="Inner">The farthest offset the chosen cut still counts as inside.</param>
/// <param name="Outer">The nearest offset the chosen cut counts as outside.</param>
/// <param name="Disagreements">How many answers contradict that cut.</param>
/// <param name="BandInner">Where the next round must start probing: <paramref name="Inner"/>, widened to
/// take in every contradicting answer.</param>
/// <param name="BandOuter">Where it must stop, widened the same way.</param>
internal readonly record struct EdgeCut(double Inner, double Outer, int Disagreements, double BandInner, double BandOuter)
{
    public double Estimate => (Inner + Outer) / 2;

    public double Width => Outer - Inner;

    public bool Settled(double tolerance) => Disagreements == 0 && Width <= tolerance;
}

/// <summary>
/// The probing of one object, from the first round to the last. Lives on the session; a round is
/// only valid in the view it was started in (ADR_20260916 §2.7).
/// </summary>
internal sealed class ProbeState
{
    public required string Name { get; init; }

    public required string ViewId { get; init; }

    public ProbeMode Mode { get; set; }

    /// <summary>The number the answer must carry. Session-wide and never reused, so an answer to an
    /// old picture cannot match a new one by accident.</summary>
    public int Round { get; set; }

    public List<Probe> Probes { get; set; } = [];

    public List<ProbeGroup> Groups { get; set; } = [];

    /// <summary>Lattice spacing of the scan, per axis — what half a cell is when a group becomes a box.</summary>
    public double ScanStepX { get; set; }

    public double ScanStepY { get; set; }

    /// <summary>The probing frame, in view pixels: fixed for the life of the state, so every answer ever
    /// given is still an offset in the same coordinates however far the box has moved since.</summary>
    public double OriginX { get; set; }

    public double OriginY { get; set; }

    public double AngleDeg { get; set; }

    /// <summary>Half the range of angles the answers still allow, in degrees. Zero until there are
    /// enough answers to say anything about the angle.</summary>
    public double AngleSpreadDeg { get; set; }

    /// <summary>Every answer so far, as points in view pixels.</summary>
    public List<ProbeAnswer> Answers { get; } = [];

    /// <summary>The scan's answers, held until a group is chosen: they become the first answers about its edges.</summary>
    public List<(Probe Probe, bool Inside)> ScanAnswers { get; set; } = [];

    /// <summary>How many edge rounds this probing has drawn — the brake when answers never settle.</summary>
    public int EdgeRounds { get; set; }

    /// <summary>Answers refused in a row. Reset by any accepted answer; the brake on a model that keeps
    /// sending answers the tool cannot use.</summary>
    public int Refusals { get; set; }

    /// <summary>The size of the view the probing runs in, for the border that stands in for a missing
    /// outside answer (<see cref="ProbePlanner.Samples"/>).</summary>
    public int ViewWidth { get; set; }

    public int ViewHeight { get; set; }

    /// <summary>The box this state last wrote, by reference. Anything else found on the object means
    /// another call moved it and the answers no longer describe it.</summary>
    public Obb? WrittenBox { get; set; }
}

/// <summary>
/// The arithmetic of probing — layout, grouping, cuts, the box that follows from them. Pure: no
/// session, no rendering, so every number here can be pinned by a test.
/// </summary>
internal static class ProbePlanner
{
    /// <summary>Each edge's outward normal in the probing frame's own axes (y grows downward).</summary>
    private static readonly (double U, double V)[] Normals = [(0, -1), (1, 0), (0, 1), (-1, 0)];

    /// <summary>How far a jittered probe may wander from its lattice slot, as a share of one cell.</summary>
    private const double JitterShare = 0.6;

    // ── frame ─────────────────────────────────────────────────────────────────

    public static (double X, double Y) ToView(ProbeState state, double u, double v)
    {
        var rad = state.AngleDeg * Math.PI / 180.0;
        double cos = Math.Cos(rad), sin = Math.Sin(rad);
        return (state.OriginX + u * cos - v * sin, state.OriginY + u * sin + v * cos);
    }

    private static (double X, double Y) NormalInView(ProbeState state, int edge) => NormalInView(state.AngleDeg, edge);

    private static (double X, double Y) NormalInView(double angle, int edge)
    {
        var rad = angle * Math.PI / 180.0;
        double cos = Math.Cos(rad), sin = Math.Sin(rad);
        var (u, v) = Normals[edge];
        return (u * cos - v * sin, u * sin + v * cos);
    }

    /// <summary>How far the frame's origin is from the view's border along an edge's normal — the
    /// farthest an edge can be and still be seen.</summary>
    public static double BorderOffset(ProbeState state, int edge, int viewWidth, int viewHeight) =>
        BorderOffset(state, state.AngleDeg, edge, viewWidth, viewHeight);

    private static double BorderOffset(ProbeState state, double angle, int edge, int viewWidth, int viewHeight)
    {
        var (nx, ny) = NormalInView(angle, edge);
        var reach = double.PositiveInfinity;
        if (nx > 1e-9) reach = Math.Min(reach, (viewWidth - state.OriginX) / nx);
        if (nx < -1e-9) reach = Math.Min(reach, -state.OriginX / nx);
        if (ny > 1e-9) reach = Math.Min(reach, (viewHeight - state.OriginY) / ny);
        if (ny < -1e-9) reach = Math.Min(reach, -state.OriginY / ny);
        return Math.Max(1, reach);
    }

    // ── starting a state ──────────────────────────────────────────────────────

    /// <summary>
    /// Edges probing around a box the model already placed. The box is a guess of unknown quality, so
    /// each edge starts from two assumptions — halfway to the centre is inside, the view's border is
    /// outside — that the first real answers on either side replace.
    /// </summary>
    public static void StartFromBox(ProbeState state, Obb inView, int viewWidth, int viewHeight)
    {
        state.Mode = ProbeMode.Edges;
        state.OriginX = inView.Cx;
        state.OriginY = inView.Cy;
        state.AngleDeg = inView.AngleDeg;
        (state.ViewWidth, state.ViewHeight) = (viewWidth, viewHeight);
        state.Answers.Clear();

        double[] extents = [inView.Height / 2, inView.Width / 2, inView.Height / 2, inView.Width / 2];
        for (var edge = 0; edge < 4; edge++)
        {
            var border = BorderOffset(state, edge, viewWidth, viewHeight);
            var (ix, iy) = AlongNormal(state, edge, Math.Min(extents[edge], border) / 2);
            var (ox, oy) = AlongNormal(state, edge, border);
            state.Answers.Add(new(ix, iy, -1, Inside: true, Assumed: true));
            state.Answers.Add(new(ox, oy, edge, Inside: false, Assumed: true));
        }
    }

    /// <summary>
    /// Edges probing from a scan group. The scan's own answers carry over: every probe of the group is
    /// inside for all four edges, and every other answered probe is outside for the edge whose side of
    /// the group it lies on — unless it lies off a corner, where it cannot be told which edge it is past.
    /// </summary>
    public static void StartFromGroup(ProbeState state, ProbeGroup group, int viewWidth, int viewHeight)
    {
        state.Mode = ProbeMode.Edges;
        state.OriginX = (group.MinX + group.MaxX) / 2;
        state.OriginY = (group.MinY + group.MaxY) / 2;
        state.AngleDeg = 0;
        (state.ViewWidth, state.ViewHeight) = (viewWidth, viewHeight);
        state.Answers.Clear();

        double halfX = state.ScanStepX / 2, halfY = state.ScanStepY / 2;
        foreach (var (probe, inside) in state.ScanAnswers)
        {
            var member = inside && probe.X >= group.MinX && probe.X <= group.MaxX && probe.Y >= group.MinY && probe.Y <= group.MaxY;
            if (member) { state.Answers.Add(new(probe.X, probe.Y, -1, Inside: true)); continue; }

            var withinX = probe.X >= group.MinX - halfX && probe.X <= group.MaxX + halfX;
            var withinY = probe.Y >= group.MinY - halfY && probe.Y <= group.MaxY + halfY;
            int? edge = (withinX, withinY) switch
            {
                (true, _) when probe.Y < group.MinY => 0,
                (_, true) when probe.X > group.MaxX => 1,
                (true, _) when probe.Y > group.MaxY => 2,
                (_, true) when probe.X < group.MinX => 3,
                _ => null,
            };
            if (edge is { } side) state.Answers.Add(new(probe.X, probe.Y, side, Inside: false));
        }

        // An object touching the frame has no outside answer on that side; the border stands in for one.
        for (var edge = 0; edge < 4; edge++)
        {
            if (state.Answers.Any(a => a.Edge == edge)) continue;
            var (x, y) = AlongNormal(state, edge, BorderOffset(state, edge, viewWidth, viewHeight));
            state.Answers.Add(new(x, y, edge, Inside: false, Assumed: true));
        }
    }

    private static (double X, double Y) AlongNormal(ProbeState state, int edge, double offset)
    {
        var (u, v) = Normals[edge];
        return ToView(state, u * offset, v * offset);
    }

    // ── layouts ───────────────────────────────────────────────────────────────

    /// <summary>A lattice over the whole view at <paramref name="spacing"/>, skipping what is already
    /// accepted. Numbers are left at zero; <see cref="Number"/> assigns them.</summary>
    public static List<Probe> Scan(
        ProbeState state, int viewWidth, int viewHeight, double spacing, bool jitter, Random random,
        IReadOnlyList<Obb> covered)
    {
        var columns = Math.Max(1, (int)Math.Round(viewWidth / spacing));
        var rows = Math.Max(1, (int)Math.Round(viewHeight / spacing));
        double stepX = (double)viewWidth / columns, stepY = (double)viewHeight / rows;
        state.ScanStepX = stepX;
        state.ScanStepY = stepY;

        var probes = new List<Probe>();
        for (var row = 0; row < rows; row++)
        for (var column = 0; column < columns; column++)
        {
            var x = (column + 0.5 + Jitter(jitter, random)) * stepX;
            var y = (row + 0.5 + Jitter(jitter, random)) * stepY;
            if (covered.Any(box => Contains(box, x, y))) continue;
            probes.Add(new Probe(0, x, y, -1, 0, column, row, 1, 0));
        }
        return probes;
    }

    /// <summary>
    /// The next round of an edges probing: rows across each unsettled edge's band, columns along the
    /// edge's current length. Settled edges get nothing — attention goes where the answer is still open.
    /// <para>
    /// A band wider than <paramref name="rows"/> rows at the scan spacing is sampled at that spacing
    /// (density, not a count); a narrower one gets exactly <paramref name="rows"/> rows, which is what
    /// halves it round after round. Rows are staggered along the edge, so a band thinner than a marker
    /// still does not stack markers on top of each other.
    /// </para>
    /// </summary>
    public static List<Probe> Edges(
        ProbeState state, int viewWidth, int viewHeight, ProbeLayout layout, Random random,
        IReadOnlyList<Obb> covered)
    {
        var cuts = Cuts(state);
        double[] estimates = [.. cuts.Select(c => Math.Max(0.5, c.Estimate))];
        var probes = new List<Probe>();

        for (var edge = 0; edge < 4; edge++)
        {
            var cut = cuts[edge];
            if (Settled(state, cuts, edge, layout.Tolerance)) continue;

            // A band the answers have closed but the angle still leaves open is probed across the swing.
            var swing = Swing(state, cuts, edge);
            var (bandInner, bandOuter) = cut.Settled(layout.Tolerance)
                ? (cut.Estimate - swing, cut.Estimate + swing)
                : (cut.BandInner, cut.BandOuter);
            var offsets = RowOffsets(bandInner, bandOuter, layout);
            if (offsets.Count == 0) continue;

            // Along the edge: only as far as the perpendicular edges are CONFIRMED inside, not as far as
            // they are estimated. A probe past a perpendicular edge is outside because of that edge, and
            // filed under this one it would drag this edge inward — a probe at a corner answers for two
            // edges at once, and the answer cannot be split between them (ADR_20260916 §2.1).
            var (before, after) = edge is 0 or 2 ? (3, 1) : (0, 2);
            double from = -cuts[before].Inner, to = cuts[after].Inner;
            var inset = Math.Min(layout.Spacing / 2, (to - from) * 0.15);
            from += inset;
            to -= inset;
            var length = Math.Max(0, to - from);
            var columns = length <= 0 ? 1 : Math.Max(2, (int)Math.Round(length / layout.Spacing));

            var (nx, ny) = NormalInView(state, edge);
            for (var row = 0; row < offsets.Count; row++)
            {
                var offset = offsets[row];
                var outward = offset > estimates[edge];
                for (var column = 0; column < columns; column++)
                {
                    var share = (column + (row + 0.5) / offsets.Count + Jitter(layout.Jitter, random)) / columns;
                    var along = length <= 0 ? from : from + Math.Clamp(share, 0, 1) * length;
                    var (u, v) = edge switch
                    {
                        0 => (along, -offset),
                        1 => (offset, along),
                        2 => (along, offset),
                        _ => (-offset, along),
                    };
                    var (x, y) = ToView(state, u, v);
                    if (x < 0 || y < 0 || x > viewWidth || y > viewHeight) continue;
                    if (covered.Any(box => Contains(box, x, y))) continue;
                    if (probes.Any(p => Math.Abs(p.X - x) < layout.Footprint && Math.Abs(p.Y - y) < layout.Footprint)) continue;

                    probes.Add(new Probe(0, x, y, edge, offset, column, row,
                        outward ? nx : -nx, outward ? ny : -ny));
                }
            }
        }
        return probes;
    }

    private static List<double> RowOffsets(double inner, double outer, ProbeLayout layout)
    {
        var width = outer - inner;
        var offsets = new List<double>();
        if (width <= 0) return offsets;

        if (width > (layout.Rows + 1) * layout.ScanSpacing)
        {
            for (var offset = inner + layout.ScanSpacing; offset < outer - layout.ScanSpacing * 0.25; offset += layout.ScanSpacing)
                offsets.Add(offset);
        }
        else
        {
            for (var row = 0; row < layout.Rows; row++)
                offsets.Add(inner + width * (row + 1) / (layout.Rows + 1));
        }
        return offsets;
    }

    private static double Jitter(bool on, Random random) => on ? (random.NextDouble() - 0.5) * JitterShare : 0;

    /// <summary>Assigns the numbers 1..n in a shuffled order. A number must say nothing about where its
    /// probe is: numbered in sequence along an edge, an answer could be completed from the order
    /// instead of the picture (ADR_20260916 §2.4).</summary>
    public static List<Probe> Number(List<Probe> probes, Random random)
    {
        var numbers = Enumerable.Range(1, probes.Count).ToArray();
        random.Shuffle(numbers);
        return [.. probes.Select((probe, i) => probe with { Number = numbers[i] })];
    }

    /// <summary>A seed that depends only on the object's name and the round, so a render can be
    /// reproduced — <c>string.GetHashCode</c> changes from one process to the next.</summary>
    public static int Seed(string name, int round)
    {
        unchecked
        {
            var hash = (int)2166136261;
            foreach (var character in name) hash = (hash ^ character) * 16777619;
            return hash ^ (round * (int)0x9E3779B1);
        }
    }

    // ── answers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Connected groups of inside probes on the scan lattice, eight-neighbour. Letters go in reading
    /// order — top to bottom, then left to right — so the same picture always gets the same letters.
    /// </summary>
    public static List<ProbeGroup> Groups(IReadOnlyList<Probe> inside)
    {
        var bySlot = inside.ToDictionary(p => (p.Column, p.Row));
        var seen = new HashSet<(int, int)>();
        var groups = new List<List<Probe>>();

        foreach (var start in inside)
        {
            if (!seen.Add((start.Column, start.Row))) continue;
            var group = new List<Probe>();
            var queue = new Queue<Probe>([start]);
            while (queue.TryDequeue(out var probe))
            {
                group.Add(probe);
                for (var dc = -1; dc <= 1; dc++)
                for (var dr = -1; dr <= 1; dr++)
                {
                    var slot = (probe.Column + dc, probe.Row + dr);
                    if (bySlot.TryGetValue(slot, out var next) && seen.Add(slot)) queue.Enqueue(next);
                }
            }
            groups.Add(group);
        }

        return [.. groups
            .Select(g => (MinX: g.Min(p => p.X), MinY: g.Min(p => p.Y), MaxX: g.Max(p => p.X), MaxY: g.Max(p => p.Y), g.Count))
            .OrderByDescending(g => g.Count).Take(26)
            .OrderBy(g => g.MinY).ThenBy(g => g.MinX)
            .Select((g, i) => new ProbeGroup((char)('A' + i), g.MinX, g.MinY, g.MaxX, g.MaxY, g.Count))];
    }

    /// <summary>Files the round's answers: inside ones for every edge, outside ones for the edge the
    /// probe was placed for. Then turns the frame to the angle the answers now agree on best.</summary>
    public static void Record(ProbeState state, IReadOnlySet<int> inside, IReadOnlySet<int> outside)
    {
        foreach (var probe in state.Probes.Where(p => p.Edge >= 0))
        {
            if (inside.Contains(probe.Number)) state.Answers.Add(new(probe.X, probe.Y, -1, Inside: true));
            else if (outside.Contains(probe.Number)) state.Answers.Add(new(probe.X, probe.Y, probe.Edge, Inside: false));
        }
        FitAngle(state);
    }

    /// <summary>How far to look either side of the current angle, and how finely.</summary>
    private const double AngleReach = 45, AngleStep = 1, AngleFineStep = 0.1;

    /// <summary>
    /// Turns the frame to the angle the answers contradict least, summed over the four edges, and
    /// records how wide a range of angles does equally well.
    /// <para>
    /// This is what makes a tilted object converge. At the wrong angle a true edge is not parallel to
    /// its probes, so correct answers along it disagree with each other and no cut exists; the band
    /// never narrows and the probing never ends. At the right angle they agree.
    /// </para>
    /// <para>
    /// Answers rarely pin one angle: a whole run of angles is usually contradicted equally little. The
    /// middle of that run is taken — not the end nearest the current angle, which on a live render
    /// stopped a 25° inscription at 19° — and half its width is kept as the spread, because an edge is
    /// not settled while the angle can still swing its far end past the tolerance.
    /// </para>
    /// </summary>
    public static void FitAngle(ProbeState state)
    {
        if (state.Answers.Count(a => !a.Assumed) < 8) return;

        var coarse = Run(state, state.AngleDeg - AngleReach, state.AngleDeg + AngleReach, AngleStep, state.AngleDeg);
        var middle = (coarse.From + coarse.To) / 2;
        var fine = Run(state, coarse.From - AngleStep, coarse.To + AngleStep, AngleFineStep, middle);

        state.AngleDeg = (fine.From + fine.To) / 2;
        state.AngleSpreadDeg = (fine.To - fine.From) / 2;
    }

    /// <summary>The contiguous run of least-contradicted angles in a sweep, the one nearest
    /// <paramref name="near"/> when several tie.</summary>
    private static (double From, double To) Run(ProbeState state, double from, double to, double step, double near)
    {
        var angles = new List<double>();
        for (var angle = from; angle <= to + 1e-9; angle += step) angles.Add(angle);
        var errors = angles.Select(a => Errors(state, a)).ToArray();
        var least = errors.Min();

        (double From, double To)? best = null;
        for (var i = 0; i < angles.Count; i++)
        {
            if (errors[i] != least || i > 0 && errors[i - 1] == least) continue;
            var j = i;
            while (j + 1 < angles.Count && errors[j + 1] == least) j++;
            var run = (angles[i], angles[j]);
            if (best is not { } current || Distance(run, near) < Distance(current, near)) best = run;
        }
        return best!.Value;
    }

    private static double Distance((double From, double To) run, double angle) =>
        angle < run.From ? run.From - angle : angle > run.To ? angle - run.To : 0;

    private static int Errors(ProbeState state, double angle)
    {
        var total = 0;
        for (var edge = 0; edge < 4; edge++) total += Cut(Samples(state, edge, angle)).Disagreements;
        return total;
    }

    /// <summary>
    /// What <paramref name="edge"/> knows, measured in the frame turned to <paramref name="angle"/>.
    /// <para>
    /// Every inside answer counts for every edge. An outside answer counts for the edge it lies past
    /// <b>in this frame</b> — not the edge it was placed for: once the frame turns, a point placed beside
    /// one edge can sit past its neighbour instead, and filed under the old edge it would contradict a
    /// correct cut for ever. A point off a corner of the inside answers' extent is past two edges at once
    /// and counts for neither.
    /// </para>
    /// <para>Assumptions drop out once the edge has a real answer on each side.</para>
    /// </summary>
    public static List<Sample> Samples(ProbeState state, int edge, double angle)
    {
        var rad = angle * Math.PI / 180.0;
        double cos = Math.Cos(rad), sin = Math.Sin(rad);
        var local = state.Answers.Select(a =>
        {
            double dx = a.X - state.OriginX, dy = a.Y - state.OriginY;
            return (Answer: a, Offsets: Offsets(dx * cos + dy * sin, -dx * sin + dy * cos));
        }).ToList();

        // How far the inside answers reach towards each edge — what "beside an edge" is measured against.
        var reach = new double[4];
        foreach (var (answer, offsets) in local.Where(l => l.Answer.Inside))
            for (var k = 0; k < 4; k++) reach[k] = Math.Max(reach[k], offsets[k]);

        var owned = local.Where(l => l.Answer.Inside || Owner(l.Offsets, reach) == edge).ToList();
        var confirmed = owned.Any(l => !l.Answer.Assumed && l.Answer.Inside)
                        && owned.Any(l => !l.Answer.Assumed && !l.Answer.Inside);

        List<Sample> samples = [.. owned
            .Where(l => !(confirmed && l.Answer.Assumed))
            .Select(l => new Sample(l.Offsets[edge], l.Answer.Inside))];

        // An edge nothing outside is filed under is open as far as the border. Without this its band
        // closed on its own farthest inside answer and it read as settled: a tilted inscription stopped
        // 47 px short, because after the turn every outside answer near that end sat off a corner.
        // The assumed border answer placed at the start does not do this job — it is a point, and a
        // turn moves it off a corner too.
        if (!samples.Any(sample => !sample.Inside) && state.ViewWidth > 0 && state.ViewHeight > 0)
            samples.Add(new Sample(
                Math.Max(BorderOffset(state, angle, edge, state.ViewWidth, state.ViewHeight),
                    samples.Count == 0 ? 0 : samples.Max(sample => sample.Offset)), Inside: false));

        return samples;
    }

    /// <summary>A point's distance past each edge's line through the origin: top, right, bottom, left.</summary>
    private static double[] Offsets(double u, double v) => [-v, u, v, -u];

    /// <summary>
    /// Which edge an outside point is past. Beside an edge means within the span the perpendicular
    /// edges' inside answers already cover; past a corner means neither span, and then no edge can
    /// claim it. A point inside both spans is a contradiction, charged to the edge it is nearest to
    /// breaking.
    /// </summary>
    private static int? Owner(double[] offsets, double[] reach)
    {
        var besideHorizontal = offsets[1] <= reach[1] && offsets[3] <= reach[3];
        var besideVertical = offsets[0] <= reach[0] && offsets[2] <= reach[2];
        return (besideHorizontal, besideVertical) switch
        {
            (true, false) => offsets[0] > reach[0] ? 0 : 2,
            (false, true) => offsets[1] > reach[1] ? 1 : 3,
            (true, true) => Enumerable.Range(0, 4).MaxBy(k => offsets[k] - reach[k]),
            _ => null,
        };
    }

    /// <summary>How far the far end of an edge can still swing under the remaining angle spread, in view px.</summary>
    public static double Swing(ProbeState state, EdgeCut[] cuts, int edge)
    {
        double[] e = [.. cuts.Select(c => Math.Max(0.5, c.Estimate))];
        var halfLength = edge is 0 or 2 ? (e[1] + e[3]) / 2 : (e[0] + e[2]) / 2;
        return halfLength * Math.Tan(state.AngleSpreadDeg * Math.PI / 180);
    }

    /// <summary>An edge is settled when its band is within tolerance, nothing contradicts it, and the angle
    /// can no longer swing its ends further than the tolerance either.</summary>
    public static bool Settled(ProbeState state, EdgeCut[] cuts, int edge, double tolerance) =>
        cuts[edge].Settled(tolerance) && Swing(state, cuts, edge) <= tolerance;

    public static EdgeCut[] Cuts(ProbeState state) =>
        [.. Enumerable.Range(0, 4).Select(edge => Cut(Samples(state, edge, state.AngleDeg)))];

    /// <summary>
    /// The cut that the fewest answers contradict. The model is wrong sometimes, so no single probe
    /// decides an edge; a contradiction widens the band to be probed again instead of being believed
    /// or thrown away (ADR_20260916 §2.2). Among equally good cuts the middle one is taken.
    /// </summary>
    public static EdgeCut Cut(IReadOnlyList<Sample> answers)
    {
        if (answers.Count == 0) return new(0, 0, 0, 0, 0);

        var sorted = answers.OrderBy(a => a.Offset).ToArray();
        var count = sorted.Length;

        // errors(k): the cut sits after the first k answers — outside ones among them are wrong, and
        // so are inside ones after them.
        var insideAfter = sorted.Count(a => a.Inside);
        var outsideBefore = 0;
        var best = int.MaxValue;
        var bestCuts = new List<int>();
        for (var k = 0; k <= count; k++)
        {
            var errors = outsideBefore + insideAfter;
            if (errors < best) { best = errors; bestCuts.Clear(); }
            if (errors == best) bestCuts.Add(k);
            if (k == count) break;
            if (sorted[k].Inside) insideAfter--; else outsideBefore++;
        }

        var cut = bestCuts[bestCuts.Count / 2];
        var inner = cut > 0 ? sorted[cut - 1].Offset : 0;
        var outer = cut < count ? sorted[cut].Offset : sorted[count - 1].Offset;

        double bandInner = inner, bandOuter = outer;
        for (var i = 0; i < count; i++)
        {
            var wrong = i < cut ? !sorted[i].Inside : sorted[i].Inside;
            if (!wrong) continue;
            bandInner = Math.Min(bandInner, sorted[i].Offset);
            bandOuter = Math.Max(bandOuter, sorted[i].Offset);
        }

        return new(inner, outer, best, bandInner, bandOuter);
    }

    /// <summary>The box the answers describe, in view pixels: each edge at the middle of its cut.</summary>
    public static Obb Box(ProbeState state)
    {
        double[] e = [.. Cuts(state).Select(c => Math.Max(0.5, c.Estimate))];
        var (cx, cy) = ToView(state, (e[1] - e[3]) / 2, (e[2] - e[0]) / 2);
        return new Obb(cx, cy, e[1] + e[3], e[0] + e[2], state.AngleDeg);
    }

    public static bool Contains(Obb box, double x, double y)
    {
        var rad = -box.AngleDeg * Math.PI / 180.0;
        double dx = x - box.Cx, dy = y - box.Cy;
        var u = dx * Math.Cos(rad) - dy * Math.Sin(rad);
        var v = dx * Math.Sin(rad) + dy * Math.Cos(rad);
        return Math.Abs(u) <= box.Width / 2 && Math.Abs(v) <= box.Height / 2;
    }
}

/// <summary>The settings a layout needs, lifted out of <see cref="GeometrySettings"/> so the planner
/// stays free of the plugin's configuration type.</summary>
/// <param name="Footprint">How close two probes may be, in view pixels, before the second is dropped.</param>
internal readonly record struct ProbeLayout(
    double Spacing, double ScanSpacing, int Rows, double Tolerance, bool Jitter, double Footprint);
