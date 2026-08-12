using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.MCP.BasicTools.FileSystem;
using SPLA.MCP.Core.Pipeline;
using SPLA.MCP.Core.Pipeline.Stages;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// What the boundary does to a real tool call, as opposed to what it computes. Two rules with
/// different force live here, and the pair is the point: the cutout refuses today, the root rule
/// only counts.
/// </summary>
public sealed class WorkspaceBoundaryTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "spla-ws-" + Guid.NewGuid().ToString("N"));
    private readonly string _outside = Path.Combine(Path.GetTempPath(), "spla-out-" + Guid.NewGuid().ToString("N"));
    private readonly List<BoundaryObservation> _observed = [];

    public WorkspaceBoundaryTests()
    {
        Directory.CreateDirectory(Path.Combine(_root, ".spla"));
        Directory.CreateDirectory(_outside);
        File.WriteAllText(Path.Combine(_root, ".spla", "secrets.yaml"), "password: hunter2");
        File.WriteAllText(Path.Combine(_root, "app.cs"), "// project file");
        File.WriteAllText(Path.Combine(_outside, "notes.txt"), "someone else's file");
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { /* best effort */ }
        try { Directory.Delete(_outside, recursive: true); } catch { /* best effort */ }
    }

    private IDisposable Scope(BoundaryMode mode = BoundaryMode.Shadow)
    {
        var workspace = new LocalWorkspace(new PathBoundary(_root, [".spla"]), mode, _observed.Add);

        return AgentSessionScope.Begin(new AgentSession(
            new KeyValueStore("session"), new MarkManager(), new SkillSession(),
            sandbox: new PassthroughSandbox(workspace, shell: null)));
    }

    private static string ReadArgs(string path) =>
        $$"""{"path":{{JsonSerializer.Serialize(path)}},"start_line":null,"line_count":null,"output":null,"output_name":null}""";

    private static async Task<string> Read(string path) =>
        (await new FsReadTool().ExecuteAsync(ReadArgs(path))).ToString();

    [Fact]
    public async Task A_project_file_reads_as_before()
    {
        using var _ = Scope();

        Assert.Contains("// project file", await Read(Path.Combine(_root, "app.cs")));
    }

    /// <summary>The decided half. <c>.spla/</c> is the runtime's own — chats, secrets, accounting,
    /// grants — and it is closed whatever mode the root rule is in.</summary>
    [Fact]
    public async Task The_cutout_is_refused_even_in_shadow()
    {
        using var _ = Scope();

        var refusal = await Assert.ThrowsAsync<PathBoundaryException>(
            () => Read(Path.Combine(_root, ".spla", "secrets.yaml")));

        Assert.Equal(PathRefusal.Cutout, refusal.Refusal);
        Assert.DoesNotContain("hunter2", refusal.Message);
        Assert.Empty(_observed);   // a refusal, not an observation
    }

    /// <summary>
    /// The half still under evaluation. Reading outside the root still works — that is the whole
    /// point of shadow — but it no longer happens unseen, and the record is what the decision to
    /// enforce will be made on.
    /// </summary>
    [Fact]
    public async Task Outside_the_root_is_allowed_in_shadow_but_recorded()
    {
        using var _ = Scope();

        var answer = await Read(Path.Combine(_outside, "notes.txt"));

        Assert.Contains("someone else's file", answer);
        Assert.NotEmpty(_observed);
        Assert.All(_observed, o => Assert.Equal(PathRefusal.OutsideRoot, o.Refusal));

        // One call, several observations: the tool asks whether the file exists before reading it,
        // and every touch of the disk goes through the boundary. The channel counts traffic, not
        // distinct paths — whoever presents it groups.
        Assert.Single(_observed.Select(o => o.Path).Distinct());
    }

    [Fact]
    public async Task Outside_the_root_is_refused_once_enforcing()
    {
        using var _ = Scope(BoundaryMode.Enforce);

        var refusal = await Assert.ThrowsAsync<PathBoundaryException>(
            () => Read(Path.Combine(_outside, "notes.txt")));

        Assert.Equal(PathRefusal.OutsideRoot, refusal.Refusal);
    }

    /// <summary>Writing into the cutout is refused as firmly as reading it, and leaves nothing
    /// behind — a boundary that guarded only reads would be decorative.</summary>
    [Fact]
    public async Task Writing_into_the_cutout_leaves_nothing_behind()
    {
        using var _ = Scope();
        var target = Path.Combine(_root, ".spla", "grants.yaml");

        await Assert.ThrowsAsync<PathBoundaryException>(() => new FsWriteTool().ExecuteAsync(
            $$"""{"path":{{JsonSerializer.Serialize(target)}},"content":"edge: allow"}"""));

        Assert.False(File.Exists(target));
    }

    /// <summary>
    /// The tool must let the refusal past its own catch-all. This is not a detail: every one of these
    /// tools ends in <c>catch (Exception)</c> returning "Error reading file", and a decision flattened
    /// into a fault sends the model off to repair something it was simply not allowed to touch.
    /// </summary>
    [Fact]
    public async Task The_tools_own_catch_all_does_not_swallow_the_decision()
    {
        using var _ = Scope();
        var inCutout = Path.Combine(_root, ".spla", "secrets.yaml");

        await Assert.ThrowsAsync<PathBoundaryException>(() => new FsReadTool().ExecuteAsync(ReadArgs(inCutout)));
        await Assert.ThrowsAsync<PathBoundaryException>(() => new FsDeleteTool().ExecuteAsync(
            $$"""{"path":{{JsonSerializer.Serialize(inCutout)}}}"""));
    }

    /// <summary>And what the model finally sees: a refusal with its reason, never a fault. The
    /// conversion is the pipeline's, in one place, for all twelve tools on the seam.</summary>
    [Fact]
    public async Task The_pipeline_turns_the_refusal_into_a_refusal_not_a_fault()
    {
        var call = new ToolCallInvocation(AgentMode.Agent, "system_read_file", "{}");

        var result = await new FaultStage(null).InvokeAsync(
            call,
            (_, _) => throw new PathBoundaryException(PathRefusal.Cutout, "'.spla' is the application's own folder"),
            CancellationToken.None);

        Assert.Equal(ToolOutcome.Refused, result.Outcome);
        Assert.Contains(".spla", result.ToString());
        Assert.DoesNotContain("not found", result.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
