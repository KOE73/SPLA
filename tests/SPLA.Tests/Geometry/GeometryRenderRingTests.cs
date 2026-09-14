using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Geometry;

namespace SPLA.Tests.Geometry;

/// <summary>
/// Renders go into the chat's blob store under rotating names. What is pinned here is the bound:
/// marking one frame used to leave one auto-named blob of about a megabyte per step of the loop, and
/// nothing ever released them.
/// </summary>
public sealed class GeometryRenderRingTests
{
    private static Dictionary<string, IMcpTool> Tools() =>
        new GeometryPlugin().Initialize(new ResolvedSettings()).ToDictionary(t => t.Name);

    /// <summary>AgentSessionScope is an AsyncLocal: a scope begun inside an awaited helper does not
    /// flow back out of it, so the chat has to be opened in the test method itself.</summary>
    private static (AgentSession Chat, Dictionary<string, IMcpTool> Tools, IDisposable Scope) Begin()
    {
        var chat = new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession());
        return (chat, Tools(), AgentSessionScope.Begin(chat));
    }

    [Fact]
    public async Task The_store_holds_at_most_render_history_renders_however_long_the_loop_runs()
    {
        var history = new GeometrySettings().RenderHistory;
        var (chat, tools, scope) = Begin();
        using var _scope = scope;

        var opened = await tools["geom_open"].ExecuteAsync(
            $$"""{"image":"{{GeometryToolsTests.Handle(chat)}}"}""");
        Assert.False(opened.IsError, opened.TextContent);

        // geom_open already rendered once, so history + 1 more steps take the ring past a full turn.
        for (var i = 0; i <= history; i++)
        {
            var step = await tools["geom_box"].ExecuteAsync(
                $$"""{"name":"bag","cx":{{400 + i}},"cy":300,"width":400,"height":300}""");
            Assert.False(step.IsError, step.TextContent);
        }

        var renders = chat.Blobs.List().Where(b => b.Name?.StartsWith("geom_render_") == true).ToList();
        Assert.Equal(history, renders.Count);

        // The newest render is the one the last reply pointed at — the head of the ring, which the
        // numbers alone do not give away once they wrap.
        var last = await tools["geom_box"].ExecuteAsync(
            """{"name":"bag","dx":5}""");
        Assert.Contains("stored as blob:geom_render_", last.TextContent);
        Assert.Equal(history, chat.Blobs.List().Count(b => b.Name?.StartsWith("geom_render_") == true));
    }

    [Fact]
    public async Task Names_rotate_through_the_ring_and_start_over()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;

        var opened = await tools["geom_open"].ExecuteAsync(
            $$"""{"image":"{{GeometryToolsTests.Handle(chat)}}"}""");
        Assert.Contains("stored as blob:geom_render_1.", opened.TextContent);

        var history = new GeometrySettings().RenderHistory;
        string? text = null;
        for (var i = 0; i < history; i++)
            text = (await tools["geom_box"].ExecuteAsync(
                """{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""")).TextContent;

        // Slot 1 again: the ring wrapped and the oldest render was overwritten, not added to.
        Assert.Contains("stored as blob:geom_render_1.", text);
    }
}
