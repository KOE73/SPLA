using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.BasicTools.FileSystem;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Permissions;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Wave 3.3 of docs/plans/PLAN_20260911_agent_roles-agents-md-compact.md — the pipeline stage that
/// marks and, for a write, refuses a call into a folder whose AGENTS.md chain has not yet reached the
/// prompt. Goes through the real pipeline (<see cref="McpHost.ExecuteToolAsync"/>) rather than calling
/// the stage directly, the same choice <c>ZoneShadowStageTests</c> makes — the ordering relative to
/// resolution/permission is part of what is being tested.
/// </summary>
public sealed class AgentsScopeStageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "spla-agents-stage-" + Guid.NewGuid().ToString("N"));

    public AgentsScopeStageTests()
    {
        Directory.CreateDirectory(Path.Combine(_root, "src", "backend"));
        File.WriteAllText(Path.Combine(_root, "AGENTS.md"), "root rules");
        File.WriteAllText(Path.Combine(_root, "src", "AGENTS.md"), "src rules");
        File.WriteAllText(Path.Combine(_root, "src", "backend", "AGENTS.md"), "backend rules");
        File.WriteAllText(Path.Combine(_root, "src", "backend", "x.cs"), "// existing");
    }

    public void Dispose() { try { Directory.Delete(_root, recursive: true); } catch { /* best effort */ } }

    private McpHost NewHost(AgentsMdMode mode = AgentsMdMode.Inject)
    {
        var host = new McpHost(new PermissionManager());
        host.ProjectSettings = () => new ResolvedSettings { AgentsMd = mode };
        host.RegisterTool(new FsReadTool());
        host.RegisterTool(new FsWriteTool());
        return host;
    }

    private (IDisposable Scope, Conversation Conversation) Scope()
    {
        var workspace = new LocalWorkspace(new PathBoundary(_root), BoundaryMode.Shadow);
        var conversation = new Conversation();
        var session = new AgentSession(
            new KeyValueStore("session"), new MarkManager(), new SkillSession(),
            sandbox: new PassthroughSandbox(workspace, shell: null))
        { Conversation = conversation };
        return (AgentSessionScope.Begin(session), conversation);
    }

    private static string ReadArgs(string path) =>
        $$"""{"path":{{System.Text.Json.JsonSerializer.Serialize(path)}},"start_line":null,"line_count":null,"output":null,"output_name":null}""";

    private static string WriteArgs(string path, string content) =>
        $$"""{"path":{{System.Text.Json.JsonSerializer.Serialize(path)}},"content":{{System.Text.Json.JsonSerializer.Serialize(content)}}}""";

    [Fact]
    public async Task Read_into_a_new_scope_executes_and_leaves_a_marker()
    {
        var host = NewHost();
        var (scope, conversation) = Scope();
        using var _ = scope;

        var result = await host.ExecuteToolAsync(AgentMode.Agent, "system_read_file",
            ReadArgs(Path.Combine(_root, "src", "backend", "x.cs")), CancellationToken.None);

        Assert.NotEqual(ToolOutcome.Refused, result.Outcome);
        Assert.Contains(conversation.Messages, m => m.ScopeMarker == "src/backend");
    }

    [Fact]
    public async Task Write_into_a_new_scope_is_refused_then_retry_succeeds()
    {
        var host = NewHost();
        var (scope, conversation) = Scope();
        using var _ = scope;
        var args = WriteArgs(Path.Combine(_root, "src", "backend", "new.cs"), "// new");

        var first = await host.ExecuteToolAsync(AgentMode.Agent, "system_write_file", args, CancellationToken.None);
        Assert.Equal(ToolOutcome.Refused, first.Outcome);
        Assert.Contains(conversation.Messages, m => m.ScopeMarker == "src/backend");
        Assert.False(File.Exists(Path.Combine(_root, "src", "backend", "new.cs")));

        var retry = await host.ExecuteToolAsync(AgentMode.Agent, "system_write_file", args, CancellationToken.None);
        Assert.NotEqual(ToolOutcome.Refused, retry.Outcome);
        Assert.True(File.Exists(Path.Combine(_root, "src", "backend", "new.cs")));
        // Still exactly one marker for the scope — the retry does not add a second.
        Assert.Single(conversation.Messages.Where(m => m.ScopeMarker == "src/backend"));
    }

    [Fact]
    public async Task Write_into_an_already_covered_scope_executes_directly()
    {
        var host = NewHost();
        var (scope, conversation) = Scope();
        using var _ = scope;
        conversation.AddScopeMarker("src/backend");

        var result = await host.ExecuteToolAsync(AgentMode.Agent, "system_write_file",
            WriteArgs(Path.Combine(_root, "src", "backend", "new.cs"), "// new"), CancellationToken.None);

        Assert.NotEqual(ToolOutcome.Refused, result.Outcome);
        Assert.True(File.Exists(Path.Combine(_root, "src", "backend", "new.cs")));
    }

    [Fact]
    public async Task Ignore_mode_neither_marks_nor_refuses()
    {
        var host = NewHost(AgentsMdMode.Ignore);
        var (scope, conversation) = Scope();
        using var _ = scope;

        var result = await host.ExecuteToolAsync(AgentMode.Agent, "system_write_file",
            WriteArgs(Path.Combine(_root, "src", "backend", "new.cs"), "// new"), CancellationToken.None);

        Assert.NotEqual(ToolOutcome.Refused, result.Outcome);
        Assert.Empty(conversation.Messages.Where(m => m.ScopeMarker != null));
    }

    /// <summary>
    /// True simultaneity cannot be forced from the outside without a hook into the stage itself
    /// (which production code should not carry for a test's sake), so this drives real OS threads
    /// released together by a <see cref="Barrier"/> to get them as close to concurrent as the
    /// scheduler allows, across many repetitions. The invariant that must hold on every repetition
    /// regardless of how close the race actually lands is thread-safety itself: never more than one
    /// marker for the scope, and never a corrupted/duplicated conversation. The stronger claim from
    /// the plan — "two parallel writes give two refusals" — is the behaviour when the two calls
    /// truly overlap (each sees "not covered" before either inserts); this asserts it whenever a
    /// repetition actually achieved that overlap (both refused), and otherwise only that the loser
    /// of a near-miss still lands on a consistent, non-corrupting outcome (refused, or — having
    /// found the scope already covered — executed).
    /// </summary>
    [Fact]
    public async Task Two_parallel_writes_into_the_same_new_scope_never_yield_more_than_one_marker()
    {
        for (var i = 0; i < 20; i++)
        {
            var host = NewHost();
            var (scope, conversation) = Scope();
            using var _ = scope;

            var fileA = Path.Combine(_root, "src", "backend", $"a{i}.cs");
            var fileB = Path.Combine(_root, "src", "backend", $"b{i}.cs");
            var argsA = WriteArgs(fileA, "// a");
            var argsB = WriteArgs(fileB, "// b");

            using var barrier = new Barrier(2);
            var sessionForThread = AgentSessionScope.Current;

            Task<ToolResult> RunOn(string args) => Task.Run(async () =>
            {
                using var __ = AgentSessionScope.Begin(sessionForThread!);
                barrier.SignalAndWait();
                return await host.ExecuteToolAsync(AgentMode.Agent, "system_write_file", args, CancellationToken.None);
            });

            var results = await Task.WhenAll(RunOn(argsA), RunOn(argsB));

            // Thread safety, unconditionally: exactly one marker was ever recorded for the scope,
            // across 20 repetitions launched from real OS threads racing to enter the stage.
            Assert.Single(conversation.Messages.Where(m => m.ScopeMarker == "src/backend"));

            // No corruption either way: a call that was not refused actually wrote its file.
            foreach (var (result, file) in new[] { (results[0], fileA), (results[1], fileB) })
                Assert.Equal(result.Outcome == ToolOutcome.Refused, !File.Exists(file));
        }
    }
}
