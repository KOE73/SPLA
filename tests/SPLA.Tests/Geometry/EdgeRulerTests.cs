using SPLA.Plugins.Geometry.Render;

namespace SPLA.Tests.Geometry;

/// <summary>
/// The spacing of the edge rulers is ordinary arithmetic and it is the part that decides whether the
/// picture is readable, so it is pinned here rather than left to be judged by eye on a render.
/// <para>
/// Every scale is built out of the finest step the model can resolve — the same lattice the view grid
/// steps on — so the cases below pass it in explicitly rather than assuming the shipped default.
/// </para>
/// </summary>
public sealed class EdgeRulerTests
{
    /// <summary>The step the settings ship with, and the one most of these cases use.</summary>
    private const int Fine = 32;

    /// <summary>Inward is whole inset rectangles: two of them when the box has the room, at the same
    /// inset on every side. Two is the minimum that gives a direction — "between the first ring and
    /// the second" — and the maximum that leaves the print underneath legible.</summary>
    [Theory]
    [InlineData(150, 2)]   // a 300 px shorter side
    [InlineData(64, 2)]    // exactly two rings' worth
    [InlineData(50, 1)]    // room for one
    [InlineData(32, 1)]    // exactly one
    [InlineData(20, 0)]    // narrower than a single inset: no ring at all
    public void A_box_carries_at_most_two_inset_rings(double room, int rings)
    {
        Assert.Equal(rings, EdgeRuler.InwardRings(room, Fine));
    }

    /// <summary>However big the box, the count is capped and never negative: a render is not the place
    /// to discover that a 4000 px box asked for a hundred rings.</summary>
    [Theory]
    [InlineData(16)]
    [InlineData(32)]
    public void The_ring_count_stays_in_range(int fine)
    {
        for (var room = -10; room <= 4000; room += 7)
        {
            var rings = EdgeRuler.InwardRings(room, fine);
            Assert.InRange(rings, 0, EdgeRuler.MaxRings);
            Assert.True(rings * fine <= room || rings == 0, $"{room} px of room took {rings} rings");
        }
    }

    /// <summary>Outward is fine near the edge and then jumps, so a large overshoot is read rather
    /// than counted — and every mark stands on the grid's own lattice.</summary>
    [Fact]
    public void The_outward_scale_starts_fine_and_jumps()
    {
        Assert.Equal([32, 64, 128], EdgeRuler.OutwardScale(1024, 768, Fine));
        Assert.Equal([32, 64], EdgeRuler.OutwardScale(400, 300, Fine));
    }

    /// <summary>It is capped rather than running to the frame's border: a scale that reaches the edge
    /// of the picture stops reading as belonging to an edge of the box.</summary>
    [Fact]
    public void The_outward_scale_never_reaches_the_frame()
    {
        foreach (var side in new[] { 40, 120, 300, 1024, 4096 })
        {
            var marks = EdgeRuler.OutwardScale(side, side, Fine);
            Assert.NotEmpty(marks);
            Assert.True(marks[^1] <= EdgeRuler.OutwardRungs[^1] * Fine, $"{side}px view reached {marks[^1]}");
            if (side >= 1024) Assert.True(marks[^1] <= side * 0.25);
        }
    }
}
