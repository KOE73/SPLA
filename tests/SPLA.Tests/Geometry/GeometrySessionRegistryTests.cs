using SkiaSharp;
using SPLA.Domain.Agent;
using SPLA.Plugins.Geometry;
using SPLA.Plugins.Geometry.Session;

namespace SPLA.Tests.Geometry;

/// <summary>
/// One markup session per chat, and no chat can see another's. Two chats sharing a frame would mean
/// one chat's correction moving the other chat's box.
/// </summary>
public sealed class GeometrySessionRegistryTests
{
    /// <summary>A real encoded PNG — this also proves SkiaSharp's native library loads at all.</summary>
    private static byte[] Png(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.DarkGray);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static GeometrySession Open(int width = 400, int height = 300, GeometrySettings? cfg = null)
    {
        var session = GeometrySession.Open(
            Png(width, height), "test://frame.png", null, cfg ?? new GeometrySettings(), out var error);
        Assert.Null(error);
        Assert.NotNull(session);
        return session!;
    }

    private static IAgentSession Chat() =>
        new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession());

    [Fact]
    public void Two_chats_do_not_see_each_others_sessions()
    {
        var first = Chat();
        var second = Chat();
        var geometry = Open();

        GeometrySessionRegistry.Set(first, geometry);
        try
        {
            Assert.Same(geometry, GeometrySessionRegistry.TryGet(first));
            Assert.Null(GeometrySessionRegistry.TryGet(second));
        }
        finally
        {
            GeometrySessionRegistry.Remove(first);
        }
    }

    [Fact]
    public void Opening_a_frame_in_a_busy_chat_replaces_the_previous_one()
    {
        var chat = Chat();
        var first = Open();
        var second = Open(200, 200);

        GeometrySessionRegistry.Set(chat, first);
        GeometrySessionRegistry.Set(chat, second);
        try
        {
            Assert.Same(second, GeometrySessionRegistry.TryGet(chat));
        }
        finally
        {
            GeometrySessionRegistry.Remove(chat);
        }
    }

    [Fact]
    public void Remove_forgets_the_session_and_says_whether_there_was_one()
    {
        var chat = Chat();
        GeometrySessionRegistry.Set(chat, Open());

        Assert.True(GeometrySessionRegistry.Remove(chat));
        Assert.False(GeometrySessionRegistry.Remove(chat));
        Assert.Null(GeometrySessionRegistry.TryGet(chat));
    }

    [Fact]
    public void A_fresh_session_starts_in_the_root_view_with_nothing_marked()
    {
        using var geometry = Open();

        Assert.Equal("source", geometry.CurrentViewId);
        Assert.Empty(geometry.Objects);
        Assert.Single(geometry.Views);
        Assert.Equal("source", geometry.CurrentView.Id);
        Assert.Null(geometry.CurrentView.ParentId);
    }

    [Fact]
    public void The_root_view_is_scaled_down_to_the_working_size_and_is_not_the_identity()
    {
        // 2048x1024 into a 1024 working size: exactly half.
        using var geometry = Open(2048, 1024, new GeometrySettings());
        var root = geometry.CurrentView;

        Assert.Equal(1024, root.Width);
        Assert.Equal(512, root.Height);
        Assert.Equal(0.5, root.SourceToView.ScaleFactor, 9);

        // A source pixel maps into the view, and a view pixel maps back out.
        var inView = root.SourceToView.Apply(1000, 600);
        Assert.Equal(500, inView.X, 9);
        Assert.Equal(300, inView.Y, 9);

        var back = root.ViewToSource.Apply(inView.X, inView.Y);
        Assert.Equal(1000, back.X, 9);
        Assert.Equal(600, back.Y, 9);
    }

    [Fact]
    public void An_image_smaller_than_the_working_size_is_not_enlarged()
    {
        using var geometry = Open(400, 300);
        var root = geometry.CurrentView;

        Assert.Equal(400, root.Width);
        Assert.Equal(300, root.Height);
        Assert.Equal(1.0, root.SourceToView.ScaleFactor, 9);
    }

    [Fact]
    public void Bytes_that_are_not_an_image_answer_with_text()
    {
        var session = GeometrySession.Open(
            [0, 1, 2, 3], "test://not-a-frame", null, new GeometrySettings(), out var error);

        Assert.Null(session);
        Assert.Contains("not-a-frame", error);
    }
}
