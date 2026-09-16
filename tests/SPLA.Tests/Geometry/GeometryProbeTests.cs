using SkiaSharp;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Geometry;
using SPLA.Plugins.Geometry.Model;
using SPLA.Plugins.Geometry.Render;
using SPLA.Plugins.Geometry.Session;

namespace SPLA.Tests.Geometry;

/// <summary>
/// geom_probe (ADR_20260916): the model only says which probes are on the object, and the tool has to
/// turn that into edges. The loop tests answer as a perfect model would — from a rectangle known by
/// construction — and check the box lands on it; the planner tests pin what the loop rests on.
/// </summary>
public sealed class GeometryProbeTests
{
    private static (AgentSession Chat, Dictionary<string, IMcpTool> Tools, IDisposable Scope) Begin()
    {
        var chat = new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession());
        var scope = AgentSessionScope.Begin(chat);
        var tools = new GeometryPlugin().Initialize(new ResolvedSettings()).ToDictionary(t => t.Name);
        return (chat, tools, scope);
    }

    /// <summary>A 1024x768 frame — the working size, so view pixels are source pixels — with the given
    /// axis-aligned rectangles on a grey ground.</summary>
    private static byte[] Frame(params SKRect[] objects)
    {
        using var bitmap = new SKBitmap(1024, 768);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(new SKColor(0x80, 0x80, 0x80));
            using var paint = new SKPaint { Color = new SKColor(0x1E, 0x64, 0xC8) };
            foreach (var rect in objects) canvas.DrawRect(rect, paint);
        }
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static async Task Open(AgentSession chat, Dictionary<string, IMcpTool> tools, byte[] png)
    {
        var opened = await tools["geom_open"].ExecuteAsync($$"""{"image":"{{GeometryToolsTests.Handle(chat, png)}}"}""");
        Assert.False(opened.IsError, opened.TextContent);
    }

    /// <summary>What a model that never errs would send for the round currently on the picture.</summary>
    private static string PerfectAnswer(AgentSession chat, string name, SKRect truth)
    {
        var state = GeometrySessionRegistry.TryGet(chat)!.Probe!;
        var inside = state.Probes.Where(p => truth.Contains((float)p.X, (float)p.Y)).Select(p => p.Number);
        var outside = state.Probes.Where(p => !truth.Contains((float)p.X, (float)p.Y)).Select(p => p.Number);
        return $$"""{"name":"{{name}}","round":{{state.Round}},"inside":[{{string.Join(",", inside)}}],"outside":[{{string.Join(",", outside)}}]}""";
    }

    [Fact]
    public async Task A_perfect_model_finds_an_unplaced_object_and_the_edges_settle_on_it()
    {
        var truth = new SKRect(300, 200, 620, 430);
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await Open(chat, tools, Frame(truth));

        var result = await tools["geom_probe"].ExecuteAsync("""{"name":"bag"}""");
        Assert.False(result.IsError, result.TextContent);
        Assert.Contains("round ", result.TextContent);

        var rounds = 0;
        while (GeometrySessionRegistry.TryGet(chat)!.Probe is not null)
        {
            Assert.True(++rounds <= 12, $"did not settle in 12 rounds:\n{result.TextContent}");
            result = await tools["geom_probe"].ExecuteAsync(PerfectAnswer(chat, "bag", truth));
            Assert.False(result.IsError, result.TextContent);
        }

        Assert.Contains("all four edges of 'bag' are settled", result.TextContent);
        var box = GeometrySessionRegistry.TryGet(chat)!.Object("bag")!.Box!;
        const double slack = 4 + 1;
        Assert.InRange(box.Cx - box.Width / 2, truth.Left - slack, truth.Left + slack);
        Assert.InRange(box.Cx + box.Width / 2, truth.Right - slack, truth.Right + slack);
        Assert.InRange(box.Cy - box.Height / 2, truth.Top - slack, truth.Top + slack);
        Assert.InRange(box.Cy + box.Height / 2, truth.Bottom - slack, truth.Bottom + slack);
    }

    [Fact]
    public async Task A_rough_box_placed_by_hand_is_refined_the_same_way()
    {
        var truth = new SKRect(300, 200, 620, 430);
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await Open(chat, tools, Frame(truth));
        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":480,"cy":300,"width":250,"height":300}""");

        var result = await tools["geom_probe"].ExecuteAsync("""{"name":"bag"}""");
        for (var rounds = 0; GeometrySessionRegistry.TryGet(chat)!.Probe is not null; rounds++)
        {
            Assert.True(rounds < 12, result.TextContent);
            result = await tools["geom_probe"].ExecuteAsync(PerfectAnswer(chat, "bag", truth));
            Assert.False(result.IsError, result.TextContent);
        }

        var box = GeometrySessionRegistry.TryGet(chat)!.Object("bag")!.Box!;
        Assert.InRange(box.Width, truth.Width - 10, truth.Width + 10);
        Assert.InRange(box.Height, truth.Height - 10, truth.Height + 10);
    }

    [Fact]
    public async Task Two_look_alike_objects_are_split_into_lettered_groups_and_one_is_picked()
    {
        SKRect left = new(100, 250, 330, 500), right = new(620, 250, 850, 500);
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await Open(chat, tools, Frame(left, right));
        await tools["geom_probe"].ExecuteAsync("""{"name":"bag"}""");

        var state = GeometrySessionRegistry.TryGet(chat)!.Probe!;
        var inside = state.Probes.Where(p => left.Contains((float)p.X, (float)p.Y) || right.Contains((float)p.X, (float)p.Y));
        var grouped = await tools["geom_probe"].ExecuteAsync(
            $$"""{"name":"bag","round":{{state.Round}},"inside":[{{string.Join(",", inside.Select(p => p.Number))}}],"outside":null}""");

        Assert.False(grouped.IsError, grouped.TextContent);
        Assert.Contains("2 separate groups", grouped.TextContent);

        state = GeometrySessionRegistry.TryGet(chat)!.Probe!;
        var picked = await tools["geom_probe"].ExecuteAsync($$"""{"name":"bag","round":{{state.Round}},"pick":"b"}""");
        Assert.False(picked.IsError, picked.TextContent);

        // Reading order: B is the right-hand one.
        var box = GeometrySessionRegistry.TryGet(chat)!.Object("bag")!.Box!;
        Assert.InRange(box.Cx, right.Left, right.Right);
    }

    [Fact]
    public async Task The_legibility_chart_makes_probes_the_smallest_look_read_at_every_larger_size()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await Open(chat, tools, Frame(new SKRect(300, 200, 620, 430)));

        var drawn = await tools["geom_probe_legibility"].ExecuteAsync("""{"round":null,"read":null}""");
        Assert.False(drawn.IsError, drawn.TextContent);
        Assert.Equal(ImageKeep.Once, Assert.Single(drawn.Content.OfType<ToolImage>()).Keep);

        var chart = GeometrySessionRegistry.TryGet(chat)!.Chart!;
        int Cell(string marker, string label, int size) => chart.Probes.Single(p =>
            LegibilityChart.Looks[p.Row] == (marker, label) && LegibilityChart.Sizes[p.Column] == size).Number;

        // A dot with an outlined number is read from 10 px up; a ring with one from 12 px up, and its
        // 8 px cell too — luck, since 10 px was missed. A misread number is on no cell.
        int[] read =
        [
            .. new[] { 10, 12, 14, 16, 20 }.Select(size => Cell("dot", "outline", size)),
            .. new[] { 8, 12, 14, 16, 20 }.Select(size => Cell("ring", "outline", size)),
            .. Enumerable.Range(10, 90).Where(n => chart.Probes.All(p => p.Number != n)).Take(1),
        ];
        var answered = await tools["geom_probe_legibility"].ExecuteAsync(
            $$"""{"round":{{chart.Round}},"read":[{{string.Join(",", read)}}]}""");

        Assert.False(answered.IsError, answered.TextContent);
        Assert.Contains("misread", answered.TextContent);
        Assert.Equal(new ProbeStyle("dot", "outline", 10, "mono"), GeometrySessionRegistry.TryGet(chat)!.ProbeStyle);

        var probe = await tools["geom_probe"].ExecuteAsync("""{"name":"bag"}""");
        Assert.Contains("small dot", probe.TextContent);
    }

    [Fact]
    public async Task A_probe_picture_is_kept_for_one_turn_whatever_the_chat_setting()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await Open(chat, tools, Frame(new SKRect(300, 200, 620, 430)));

        var result = await tools["geom_probe"].ExecuteAsync("""{"name":"bag"}""");

        Assert.Equal(ImageKeep.Once, Assert.Single(result.Content.OfType<ToolImage>()).Keep);
    }

    [Fact]
    public async Task An_answer_to_an_old_round_or_about_a_moved_box_is_refused()
    {
        var truth = new SKRect(300, 200, 620, 430);
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await Open(chat, tools, Frame(truth));
        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":460,"cy":315,"width":300,"height":200}""");
        await tools["geom_probe"].ExecuteAsync("""{"name":"bag"}""");
        var round = GeometrySessionRegistry.TryGet(chat)!.Probe!.Round;

        var stale = await tools["geom_probe"].ExecuteAsync($$"""{"name":"bag","round":{{round + 7}},"inside":[1],"outside":null}""");
        Assert.True(stale.IsError);
        Assert.Contains("not the current round", stale.TextContent);

        var both = await tools["geom_probe"].ExecuteAsync($$"""{"name":"bag","round":{{round}},"inside":[1],"outside":[1]}""");
        Assert.True(both.IsError);
        Assert.Contains("both inside and outside", both.TextContent);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","dx":20}""");
        var moved = await tools["geom_probe"].ExecuteAsync($$"""{"name":"bag","round":{{round}},"inside":[1],"outside":null}""");
        Assert.True(moved.IsError);
        Assert.Contains("interrupted", moved.TextContent);
    }

    [Fact]
    public async Task Accepted_objects_get_no_probes()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await Open(chat, tools, Frame());
        await tools["geom_box"].ExecuteAsync("""{"name":"done","cx":512,"cy":384,"width":400,"height":300}""");
        await tools["geom_accept"].ExecuteAsync("""{"name":"done"}""");

        await tools["geom_probe"].ExecuteAsync("""{"name":"next"}""");

        var probes = GeometrySessionRegistry.TryGet(chat)!.Probe!.Probes;
        Assert.NotEmpty(probes);
        Assert.DoesNotContain(probes, p => p.X is > 312 and < 712 && p.Y is > 234 and < 534);
    }

    /// <summary>A tilted object: the answers alone have to turn the box. Truth is an oriented box;
    /// the frame only needs to be the working size, since the oracle answers from geometry.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("""{"name":"label","cx":530,"cy":385,"width":300,"height":140,"angle":18}""")]
    public async Task A_tilted_object_is_found_with_its_angle(string? roughBox)
    {
        var truth = new Obb(512, 400, 287, 86, 25);
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await Open(chat, tools, Frame());
        if (roughBox is not null) await tools["geom_box"].ExecuteAsync(roughBox);

        var result = await tools["geom_probe"].ExecuteAsync("""{"name":"label"}""");
        for (var rounds = 0; GeometrySessionRegistry.TryGet(chat)!.Probe is { } state; rounds++)
        {
            Assert.True(rounds < 12, result.TextContent);
            var inside = state.Probes.Where(p => ProbePlanner.Contains(truth, p.X, p.Y)).Select(p => p.Number);
            var outside = state.Probes.Where(p => !ProbePlanner.Contains(truth, p.X, p.Y)).Select(p => p.Number);
            result = await tools["geom_probe"].ExecuteAsync(
                $$"""{"name":"label","round":{{state.Round}},"inside":[{{string.Join(",", inside)}}],"outside":[{{string.Join(",", outside)}}]}""");
            Assert.False(result.IsError, result.TextContent);
        }

        Assert.Contains("all four edges of 'label' are settled", result.TextContent);
        var box = GeometrySessionRegistry.TryGet(chat)!.Object("label")!.Box!;
        Assert.InRange(box.AngleDeg, truth.AngleDeg - 1.5, truth.AngleDeg + 1.5);
        Assert.InRange(box.Cx, truth.Cx - 4, truth.Cx + 4);
        Assert.InRange(box.Cy, truth.Cy - 4, truth.Cy + 4);
        Assert.InRange(box.Width, truth.Width - 8, truth.Width + 8);
        Assert.InRange(box.Height, truth.Height - 8, truth.Height + 8);
    }

    /// <summary>A model whose answers never agree must not keep the rounds coming for ever.</summary>
    [Fact]
    public async Task Answers_that_never_agree_stop_after_a_bounded_number_of_rounds()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await Open(chat, tools, Frame());
        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":512,"cy":384,"width":300,"height":200}""");
        var random = new Random(7);

        var result = await tools["geom_probe"].ExecuteAsync("""{"name":"bag"}""");
        for (var rounds = 0; GeometrySessionRegistry.TryGet(chat)!.Probe is { } state; rounds++)
        {
            Assert.True(rounds < 40, "probing never stopped");
            var coin = state.Probes.ToLookup(_ => random.Next(2) == 0, p => p.Number);
            result = await tools["geom_probe"].ExecuteAsync(
                $$"""{"name":"bag","round":{{state.Round}},"inside":[{{string.Join(",", coin[true])}}],"outside":[{{string.Join(",", coin[false])}}]}""");
            Assert.False(result.IsError, result.TextContent);
        }

        Assert.Contains("never settled", result.TextContent);
    }

    // ── planner ───────────────────────────────────────────────────────────────

    [Fact]
    public void A_single_wrong_answer_does_not_move_the_cut_but_is_reported()
    {
        Sample[] answers =
        [
            new(10, true), new(20, true), new(30, true), new(40, false), new(50, false),
            new(60, true), // the model got this one wrong
            new(70, false),
        ];

        var cut = ProbePlanner.Cut(answers);

        Assert.Equal(30, cut.Inner);
        Assert.Equal(40, cut.Outer);
        Assert.Equal(1, cut.Disagreements);
        Assert.Equal(60, cut.BandOuter);
        Assert.False(cut.Settled(64));
    }

    [Fact]
    public void Numbers_are_a_shuffled_permutation()
    {
        var probes = Enumerable.Range(0, 40).Select(i => new Probe(0, i * 10, 0, 0, i, i, 0, 1, 0)).ToList();

        var numbered = ProbePlanner.Number(probes, new Random(ProbePlanner.Seed("bag", 1)));

        Assert.Equal(Enumerable.Range(1, 40), numbered.Select(p => p.Number).Order());
        Assert.NotEqual(Enumerable.Range(1, 40), numbered.Select(p => p.Number));
    }

    [Fact]
    public void Diagonal_neighbours_are_one_group_and_a_gap_makes_two()
    {
        Probe At(int column, int row) => new(0, column * 10, row * 10, -1, 0, column, row, 1, 0);

        var groups = ProbePlanner.Groups([At(0, 0), At(1, 1), At(2, 2), At(5, 0), At(5, 1)]);

        Assert.Equal(2, groups.Count);
        Assert.Equal(['A', 'B'], groups.Select(g => g.Letter));
    }
}
