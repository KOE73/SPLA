using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Security;
using SPLA.MCP.BasicTools.FileSystem;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Permissions;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The shadow step through the real pipeline, rather than the classifier on its own: a call goes in,
/// the ledger has an edge afterwards, and nothing was refused on the way. The last part is the
/// point — a shadow that quietly changes an outcome is not a shadow.
/// </summary>
public sealed class ZoneShadowStageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "spla-edge-" + Guid.NewGuid().ToString("N"));
    private readonly string _outside = Path.Combine(Path.GetTempPath(), "spla-edge-out-" + Guid.NewGuid().ToString("N"));

    public ZoneShadowStageTests()
    {
        Directory.CreateDirectory(_root);
        Directory.CreateDirectory(_outside);
        File.WriteAllText(Path.Combine(_root, "inside.txt"), "in the project");
        File.WriteAllText(Path.Combine(_outside, "outside.txt"), "somewhere else");
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { /* best effort */ }
        try { Directory.Delete(_outside, recursive: true); } catch { /* best effort */ }
    }

    private McpHost NewHost()
    {
        var boundary = new PathBoundary(_root);
        var host = new McpHost(
            new PermissionManager(),
            zoneOfPath: p => string.IsNullOrWhiteSpace(p)
                ? Zone.Unknown
                : boundary.Contains(p) ? Zone.Project : Zone.Machine);

        host.RegisterTool(new FsReadTool());
        return host;
    }

    private IDisposable Scope()
    {
        var workspace = new LocalWorkspace(new PathBoundary(_root), BoundaryMode.Shadow);
        return AgentSessionScope.Begin(new AgentSession(
            new KeyValueStore("session"), new MarkManager(), new SkillSession(),
            sandbox: new PassthroughSandbox(workspace, shell: null)));
    }

    private static string ReadArgs(string path) =>
        $$"""{"path":{{System.Text.Json.JsonSerializer.Serialize(path)}},"start_line":null,"line_count":null,"output":null,"output_name":null}""";

    [Fact]
    public async Task A_call_leaves_its_movement_in_the_ledger()
    {
        var host = NewHost();
        using var _ = Scope();

        await host.ExecuteToolAsync(AgentMode.Agent, "system_read_file",
            ReadArgs(Path.Combine(_root, "inside.txt")), CancellationToken.None);

        var traffic = Assert.Single(host.Edges.List());
        Assert.Equal(Zone.Project, traffic.Edge.Source);
        Assert.Equal(Zone.Context, traffic.Edge.Sink);
        Assert.Equal("system_read_file", traffic.LastTool);
        Assert.Equal(1, traffic.Calls);
    }

    /// <summary>
    /// The same tool, two destinations, two rows. This is the whole reason for classifying by path
    /// rather than by declaration — one row would have said "project read" for both and the record
    /// would have been worthless as evidence.
    /// </summary>
    [Fact]
    public async Task Inside_and_outside_the_root_are_two_different_rows()
    {
        var host = NewHost();
        using var _ = Scope();

        await host.ExecuteToolAsync(AgentMode.Agent, "system_read_file",
            ReadArgs(Path.Combine(_root, "inside.txt")), CancellationToken.None);
        await host.ExecuteToolAsync(AgentMode.Agent, "system_read_file",
            ReadArgs(Path.Combine(_outside, "outside.txt")), CancellationToken.None);

        var rows = host.Edges.List();
        Assert.Equal(2, rows.Count);
        Assert.Contains(rows, r => r.Edge.Source == Zone.Project);
        Assert.Contains(rows, r => r.Edge.Source == Zone.Machine);
    }

    /// <summary>Shadow means shadow: reading outside the root is recorded and still succeeds. If this
    /// ever fails, the collection step has quietly become the enforcement step.</summary>
    [Fact]
    public async Task Recording_a_movement_does_not_refuse_it()
    {
        var host = NewHost();
        using var _ = Scope();

        var result = await host.ExecuteToolAsync(AgentMode.Agent, "system_read_file",
            ReadArgs(Path.Combine(_outside, "outside.txt")), CancellationToken.None);

        Assert.NotEqual(ToolOutcome.Refused, result.Outcome);
        Assert.Contains("somewhere else", result.ToString());
        Assert.Single(host.Edges.List());
    }

    [Fact]
    public async Task Repeats_of_one_movement_are_counted_not_listed()
    {
        var host = NewHost();
        using var _ = Scope();
        var args = ReadArgs(Path.Combine(_root, "inside.txt"));

        for (var i = 0; i < 3; i++)
            await host.ExecuteToolAsync(AgentMode.Agent, "system_read_file", args, CancellationToken.None);

        Assert.Equal(3, host.Edges.List().Single().Calls);
    }

    /// <summary>A refused call still moved nothing, but it is still a thing that was attempted, and
    /// the record is of attempts. Ordering in the pipeline is what decides this, so it is pinned.</summary>
    [Fact]
    public async Task An_unknown_tool_records_nothing()
    {
        var host = NewHost();
        using var _ = Scope();

        await host.ExecuteToolAsync(AgentMode.Agent, "no_such_tool", "{}", CancellationToken.None);

        Assert.Empty(host.Edges.List());
    }
}
