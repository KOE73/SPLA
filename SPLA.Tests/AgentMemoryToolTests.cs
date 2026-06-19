using SPLA.Domain.Agent;
using SPLA.MCP.Core.Tools;

namespace SPLA.Tests;

public sealed class AgentMemoryToolTests
{
    [Fact]
    public async Task Clear_without_scope_clears_session_only()
    {
        var session = new KeyValueStore("session");
        var project = new KeyValueStore("project");
        session.Set("context:plan", "session");
        project.Set("context:plan", "project");

        var tool   = new AgentMemoryClearTool(session, project);
        var result = await tool.ExecuteAsync("""{"filter":"context:","scope":null}""");

        Assert.Contains("ok: cleared [session]", result);
        Assert.DoesNotContain("[project]", result);
        Assert.Null(session.Get("context:plan"));
        Assert.Equal("project", project.Get("context:plan"));
    }

    [Fact]
    public async Task List_without_scope_reads_session_only()
    {
        var session = new KeyValueStore("session");
        var project = new KeyValueStore("project");
        session.Set("task:state", "session");
        project.Set("task:state", "project");

        var tool   = new AgentMemoryListTool(session, project);
        var result = await tool.ExecuteAsync("""{"filter":"task:","top":null,"skip":null,"scope":null}""");

        Assert.Contains("task:state = session", result);
        Assert.DoesNotContain("task:state = project", result);
    }

    [Fact]
    public async Task Set_then_get_roundtrip()
    {
        var session = new KeyValueStore("session");
        var project = new KeyValueStore("project");

        var setter = new AgentMemorySetTool(session, project);
        var getter = new AgentMemoryGetTool(session, project);

        await setter.ExecuteAsync("""{"key":"context:plan","value":"step 1","scope":null}""");
        var value = await getter.ExecuteAsync("""{"key":"context:plan","scope":null}""");

        Assert.Equal("step 1", value);
    }

    [Fact]
    public async Task Delete_removes_key()
    {
        var session = new KeyValueStore("session");
        var project = new KeyValueStore("project");
        session.Set("note:x", "hello");

        var tool   = new AgentMemoryDeleteTool(session, project);
        var result = await tool.ExecuteAsync("""{"key":"note:x","scope":null}""");

        Assert.Contains("ok: deleted [session]", result);
        Assert.Null(session.Get("note:x"));
    }

    [Fact]
    public async Task Get_returns_not_found_for_missing_key()
    {
        var session = new KeyValueStore("session");
        var project = new KeyValueStore("project");

        var tool   = new AgentMemoryGetTool(session, project);
        var result = await tool.ExecuteAsync("""{"key":"missing:key","scope":null}""");

        Assert.Contains("not_found", result);
    }

    [Fact]
    public async Task Set_writes_to_project_scope_when_specified()
    {
        var session = new KeyValueStore("session");
        var project = new KeyValueStore("project");

        var setter = new AgentMemorySetTool(session, project);
        var getter = new AgentMemoryGetTool(session, project);

        await setter.ExecuteAsync("""{"key":"project:build-cmd","value":"dotnet build","scope":"project"}""");

        Assert.Null(session.Get("project:build-cmd"));
        var value = await getter.ExecuteAsync("""{"key":"project:build-cmd","scope":"project"}""");
        Assert.Equal("dotnet build", value);
    }
}
