using SPLA.Plugins.Geometry.Render;

namespace SPLA.Tests.Geometry;

/// <summary>
/// The spacing of the edge rulers is ordinary arithmetic and it is the part that decides whether the
/// picture is readable, so it is pinned here rather than left to be judged by eye on a render.
/// </summary>
public sealed class EdgeRulerTests
{
    /// <summary>An ordinary box lands on four to six lines a side: enough to read a distance off,
    /// few enough that the labels do not collide.</summary>
    [Theory]
    [InlineData(150, 25, 6)]   // a 300 px side
    [InlineData(280, 50, 5)]   // a 560 px side
    [InlineData(62, 10, 6)]    // a 125 px side — the finest the ladder goes
    [InlineData(35, 10, 3)]    // a 70 px side: below target, and nothing coarser would help
    [InlineData(500, 100, 5)]  // a 1000 px side
    public void An_inward_scale_carries_four_to_six_lines(double depth, int step, int count)
    {
        Assert.Equal((step, count), EdgeRuler.InwardScale(depth));
    }

    /// <summary>Whatever the box, the step is one a reader adds in their head — never an arithmetic
    /// problem of its own.</summary>
    [Fact]
    public void Every_step_comes_off_the_ladder()
    {
        for (var depth = 1; depth <= 600; depth++)
        {
            var (step, count) = EdgeRuler.InwardScale(depth);
            Assert.Contains(step, EdgeRuler.Ladder);
            Assert.True(count <= EdgeRuler.MaxInward, $"depth {depth} gave {count} lines");
            Assert.True(step * count <= depth || count == 1, $"depth {depth} overran with {count}x{step}");
        }
    }

    /// <summary>A box too narrow for a scale gets one honest line at the finest step instead of a
    /// crowd of unreadable ones — and nothing at all when it cannot even hold that.</summary>
    [Theory]
    [InlineData(12, 1)]   // 24 px tall: one line at 10
    [InlineData(6, 1)]    // 12 px tall: still holds a single 10
    [InlineData(4, 0)]    // 8 px tall: the line would be outside the box
    public void A_narrow_box_degrades_to_one_line_or_none(double depth, int count)
    {
        var (step, got) = EdgeRuler.InwardScale(depth);
        Assert.Equal(EdgeRuler.MinStep, step);
        Assert.Equal(count, got);
    }

    /// <summary>Outward is fine near the edge and then jumps, so a large overshoot is read rather
    /// than counted.</summary>
    [Fact]
    public void The_outward_scale_starts_fine_and_jumps()
    {
        Assert.Equal([10, 20, 50, 100], EdgeRuler.OutwardScale(1024, 768));
        Assert.Equal([10, 20, 50], EdgeRuler.OutwardScale(400, 300));
    }

    /// <summary>It is capped rather than running to the frame's border: a scale that reaches the edge
    /// of the picture stops reading as belonging to an edge of the box.</summary>
    [Fact]
    public void The_outward_scale_never_reaches_the_frame()
    {
        foreach (var side in new[] { 40, 120, 300, 1024, 4096 })
        {
            var marks = EdgeRuler.OutwardScale(side, side);
            Assert.NotEmpty(marks);
            Assert.True(marks[^1] <= 100, $"{side}px view reached {marks[^1]}");
            if (side >= 400) Assert.True(marks[^1] <= side * 0.25);
        }
    }
}
