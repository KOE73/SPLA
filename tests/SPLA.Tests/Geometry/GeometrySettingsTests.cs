using SPLA.Plugins.Geometry;

namespace SPLA.Tests.Geometry;

/// <summary>
/// The settings blob is hand-edited YAML: an absent key must produce the documented default and an
/// out-of-range one must land on the nearest bound instead of reaching the renderer.
/// </summary>
public sealed class GeometrySettingsTests
{
    [Fact]
    public void Empty_blob_gives_the_documented_defaults()
    {
        var settings = GeometrySettings.FromBlob(null);

        Assert.Equal(1024, settings.RenderMaxSide);
        Assert.Equal(3, settings.LineWidth);
        Assert.Equal(16, settings.FontSize);
        Assert.Equal(0.05, settings.CropPadding, 9);
        Assert.Equal(0, settings.JpegQuality);
        Assert.Equal(5, settings.RenderHistory);
    }

    [Fact]
    public void Values_out_of_range_are_clamped_to_the_nearest_bound()
    {
        var settings = GeometrySettings.FromBlob(new Dictionary<string, object>
        {
            ["render_max_side"] = 99999,
            ["line_width"] = 0,
            ["font_size"] = 200,
            ["crop_padding"] = 5.0,
            ["jpeg_quality"] = 7,
            ["render_history"] = 99,
        });

        Assert.Equal(4096, settings.RenderMaxSide);
        Assert.Equal(1, settings.LineWidth);
        Assert.Equal(48, settings.FontSize);
        Assert.Equal(1.0, settings.CropPadding, 9);
        Assert.Equal(30, settings.JpegQuality);
        Assert.Equal(20, settings.RenderHistory);
    }

    [Fact]
    public void Values_inside_the_range_are_kept()
    {
        var settings = GeometrySettings.FromBlob(new Dictionary<string, object>
        {
            ["render_max_side"] = 800,
            ["jpeg_quality"] = 85,
        });

        Assert.Equal(800, settings.RenderMaxSide);
        Assert.Equal(85, settings.JpegQuality);
        Assert.Equal(3, settings.LineWidth);
    }
}
