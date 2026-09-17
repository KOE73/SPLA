using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Geometry;

namespace SPLA.Tests.Geometry;

/// <summary>Accepting settles an object without moving it, and correcting it afterwards unsettles it.</summary>
public sealed class GeometryAcceptToolTests
{
    private static Dictionary<string, IMcpTool> Tools() =>
        new GeometryPlugin().Initialize(new ResolvedSettings()).ToDictionary(t => t.Name);

    private static (AgentSession Chat, Dictionary<string, IMcpTool> Tools, IDisposable Scope) Begin()
    {
        var chat = new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession());
        var scope = AgentSessionScope.Begin(chat);
        return (chat, Tools(), scope);
    }

    private static async Task OpenFrame(AgentSession chat, Dictionary<string, IMcpTool> tools)
    {
        var opened = await tools["geom_open"].ExecuteAsync($$"""{"image":"{{GeometryToolsTests.Handle(chat)}}"}""");
        Assert.False(opened.IsError, opened.TextContent);
    }

    [Fact]
    public async Task Accepting_by_name_settles_that_object_only()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");
        await tools["geom_point"].ExecuteAsync("""{"name":"mark","x":418,"y":203}""");

        var result = await tools["geom_accept"].ExecuteAsync("""{"name":"bag"}""");

        Assert.False(result.IsError, result.TextContent);
        Assert.Single(result.Content.OfType<ToolImage>());
        Assert.Contains("accepted 'bag'", result.TextContent);

        var lines = result.TextContent.Split('\n');
        Assert.Contains(lines, l => l.Contains("bag") && l.TrimEnd().EndsWith("accepted"));
        Assert.Contains(lines, l => l.Contains("mark") && l.TrimEnd().EndsWith("editing"));
    }

    [Fact]
    public async Task Accepting_without_a_name_settles_everything_still_being_edited()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");
        await tools["geom_point"].ExecuteAsync("""{"name":"mark","x":418,"y":203}""");

        var result = await tools["geom_accept"].ExecuteAsync("{}");

        Assert.Contains("accepted 'bag', 'mark'", result.TextContent);
        Assert.DoesNotContain("editing", result.TextContent);

        var again = await tools["geom_accept"].ExecuteAsync("{}");
        Assert.True(again.IsError);
        Assert.Contains("already accepted", again.TextContent);
    }

    [Fact]
    public async Task Accepting_does_not_move_anything()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":401,"cy":299,"width":400,"height":300,"angle":7}""");
        var result = await tools["geom_accept"].ExecuteAsync("""{"name":"bag"}""");

        Assert.Contains("cx=401 cy=299 w=400 h=300 angle=+7 (tilted down to the right)", result.TextContent);
    }

    [Fact]
    public async Task Correcting_an_accepted_object_puts_it_back_into_editing()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");
        await tools["geom_accept"].ExecuteAsync("""{"name":"bag"}""");

        var corrected = await tools["geom_box"].ExecuteAsync("""{"name":"bag","dx":5}""");
        Assert.Contains("editing", corrected.TextContent);
    }

    [Fact]
    public async Task Nothing_marked_yet_is_said_plainly()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        var empty = await tools["geom_accept"].ExecuteAsync("{}");
        Assert.True(empty.IsError);
        Assert.Contains("Nothing has been marked", empty.TextContent);

        var unknown = await tools["geom_accept"].ExecuteAsync("""{"name":"ghost"}""");
        Assert.True(unknown.IsError);
        Assert.Contains("ghost", unknown.TextContent);
    }

    /// <summary>The placeholder the model sends for an argument it does not want means "not supplied"
    /// here too — geom_accept reads no name and settles everything, rather than hunting for an object
    /// called "null".</summary>
    [Fact]
    public async Task The_string_null_as_a_name_accepts_everything()
    {
        var (chat, tools, scope) = Begin();
        using var _scope = scope;
        await OpenFrame(chat, tools);

        await tools["geom_box"].ExecuteAsync("""{"name":"bag","cx":400,"cy":300,"width":400,"height":300}""");
        await tools["geom_point"].ExecuteAsync("""{"name":"mark","x":418,"y":203}""");

        var result = await tools["geom_accept"].ExecuteAsync("""{"name":"null"}""");

        Assert.False(result.IsError, result.TextContent);
        Assert.Contains("accepted 'bag', 'mark'", result.TextContent);
    }
}
