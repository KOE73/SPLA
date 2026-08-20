using SPLA.Domain.Tools;
using SPLA.Agent;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Plugins;
using SPLA.Library;
using SPLA.MCP.Core.Tools;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Tests;

/// <summary>Stub gateway that returns a fixed response without making network calls.</summary>
file sealed class StubLlmService : SPLA.Domain.Llm.ILlmGateway
{
    private readonly string _response;
    public StubLlmService(string response = "stub result") => _response = response;

    /// <summary>What the "provider" reports having been sent. Null is the honest default — plenty of
    /// endpoints report nothing, and the progress tick has to cope with that.</summary>
    public int? PromptTokens { get; init; }

    public Task<SPLA.Domain.Llm.LlmTurnResult> InvokeAsync(
        SPLA.Domain.Llm.LlmTurnContext ctx, CancellationToken ct = default)
    {
        ctx.OnDelta?.Invoke(_response);
        return Task.FromResult(new SPLA.Domain.Llm.LlmTurnResult
        {
            Message = new ChatMessage
            {
                Role = ChatRole.Assistant,
                Content = _response,
                PromptTokens = PromptTokens
            }
        });
    }
}

/// <summary>Stub tool host that has no tools and always returns empty.</summary>
file sealed class StubToolHost : IToolHost
{
    public IEnumerable<ToolDefinition> GetToolDefinitions() => [];

    public Task<ToolResult> ExecuteToolAsync(AgentMode mode, string name, string argumentsJson,
        CancellationToken cancellationToken = default, ToolCallContext? context = null)
        => Task.FromResult(ToolResult.Fail($"tool not found: {name}", "tool not found"));
}

public class AgentSpawnToolTests
{
    private static SpawnedAgentRunner BuildRunner(string llmResponse = "done")
    {
        var llm = new StubLlmService(llmResponse);
        var tools = new StubToolHost();
        var skills = new SkillLibrary([new SPLA.Tests.Fakes.FakeSkillSource()
            .With("test.skill", body: "Step 1: Do the thing.\nStep 2: Report done.", description: "A test skill")]);

        var settings = new ResolvedSettings { Mode = AgentMode.Edit };
        var plugins = new PluginManager(settings);

        return new SpawnedAgentRunner(llm, tools, skills, plugins, settings);
    }

    [Fact]
    public async Task Spawn_unknown_skill_returns_error()
    {
        var tool = new AgentSpawnTool(BuildRunner());
        var result = (await tool.ExecuteAsync("""{"skill":"no.such","input":"go"}""")).TextContent;
        Assert.StartsWith("error:", result);
    }

    [Fact]
    public async Task Spawn_without_skill_runs_the_input_as_a_free_form_task()
    {
        var tool = new AgentSpawnTool(BuildRunner("host is Ubuntu 24.04"));
        var result = (await tool.ExecuteAsync("""{"input":"report the OS of host X"}""")).TextContent;
        Assert.Contains("host is Ubuntu 24.04", result);
    }

    [Fact]
    public async Task Spawn_with_null_skill_runs_the_input_as_a_free_form_task()
    {
        // Strict schema keeps 'skill' in required, so a model with nothing to pin sends an explicit
        // null. It has to mean the same as leaving the property out.
        var tool = new AgentSpawnTool(BuildRunner("done"));
        var result = (await tool.ExecuteAsync("""{"input":"do a thing","skill":null,"mode":null}""")).TextContent;
        Assert.Contains("done", result);
    }

    [Fact]
    public async Task Spawn_missing_input_param_returns_error()
    {
        var tool = new AgentSpawnTool(BuildRunner());
        var result = (await tool.ExecuteAsync("""{"skill":"test.skill"}""")).TextContent;
        Assert.StartsWith("error: 'input'", result);
    }

    [Fact]
    public async Task Spawn_invalid_json_returns_error()
    {
        var tool = new AgentSpawnTool(BuildRunner());
        var result = (await tool.ExecuteAsync("not-json")).TextContent;
        Assert.StartsWith("error: invalid_json", result);
    }

    [Fact]
    public async Task Spawn_valid_skill_returns_llm_response()
    {
        var tool = new AgentSpawnTool(BuildRunner("skill completed successfully"));
        var result = (await tool.ExecuteAsync("""{"skill":"test.skill","input":"run it","mode":"Research"}""")).TextContent;
        Assert.Contains("skill completed successfully", result);
    }

    [Fact]
    public async Task Spawn_without_skill_still_refuses_an_empty_input()
    {
        // The one thing a free-form spawn cannot do without: with no skill and no brief there is
        // nothing to run at all.
        var tool = new AgentSpawnTool(BuildRunner());
        var result = (await tool.ExecuteAsync("""{"skill":null,"input":"  "}""")).TextContent;
        Assert.StartsWith("error: 'input'", result);
    }

    [Fact]
    public async Task Spawn_does_not_affect_parent_skill_session()
    {
        // SpawnedAgentRunner creates its own SkillSession — parent has none.
        // After spawn, parent session remains untouched (no ActiveSkillId set externally).
        var parentSession = new SPLA.Domain.Agent.SkillSession();
        Assert.Null(parentSession.ActiveSkillId);

        var tool = new AgentSpawnTool(BuildRunner("ok"));
        await tool.ExecuteAsync("""{"skill":"test.skill","input":"go"}""");

        Assert.Null(parentSession.ActiveSkillId); // unchanged
    }

    /// <summary>
    /// A run gets a branch of its own, named after the task. Without one a batch is a lie: its tasks
    /// run on parallel flows that all inherit the same current node, so several sub-agents would hang
    /// their tool calls off the batch as one undifferentiated row — the tree would show what was done
    /// and lose who did it. A pinned skill names the branch after itself.
    /// </summary>
    [Theory]
    [InlineData("""{"skill":null,"input":"count the adr files"}""", "count the adr files")]
    [InlineData("""{"skill":"test.skill","input":"go"}""", "test.skill")]
    public async Task A_spawned_run_gets_a_branch_named_after_its_task(string arguments, string expected)
    {
        var tree = new ProgressTree();
        var tool = new AgentSpawnTool(BuildRunner("ok"));

        using (ProgressScope.BeginTree(tree))
        using (ProgressScope.BeginNode("agent_spawn"))
        {
            await tool.ExecuteAsync(arguments);
        }

        var caller = Assert.Single(tree.Nodes, n => n.ParentId == null);
        var run = Assert.Single(tree.Nodes, n => n.ParentId == caller.Id);
        Assert.Equal(expected, run.Label);
    }

    /// <summary>
    /// How full the run's context is, on the tick. This is the number that makes a runaway sub-agent
    /// legible before it is expensive: there is no "percent done" for an agent, but there is a ceiling
    /// it is walking towards, and nothing else about a long run distinguishes working from filling up.
    /// </summary>
    [Fact]
    public async Task A_tick_carries_how_much_context_the_run_has_used()
    {
        var tree = new ProgressTree();
        var settings = new ResolvedSettings { Mode = AgentMode.Edit };
        var runner = new SpawnedAgentRunner(
            new StubLlmService("done") { PromptTokens = 1234 },
            new StubToolHost(),
            new SkillLibrary([new SPLA.Tests.Fakes.FakeSkillSource()]),
            new PluginManager(settings),
            settings);

        using (ProgressScope.BeginTree(tree))
        using (ProgressScope.BeginNode("agent_spawn"))
        {
            // Two turns: the first reports the count, the second is where it can appear — the figure
            // arrives with the call that has already been made.
            await runner.RunAsync(null, "go", AgentMode.Edit);
        }

        var run = Assert.Single(tree.Nodes, n => n.Label == "go");

        // No window declared and nothing to ask, so the figure travels in the message and nothing is
        // set for a bar: there is no fraction without a denominator, and Current alone would only put
        // the same number on the line twice. Invariant formatting — the line is English and also goes
        // out over MCP, so a machine with a comma separator must not turn it into "1,2k".
        Assert.Contains("ctx 1.2k", run.Latest!.Message);
        Assert.Null(run.Latest.Current);
        Assert.Null(run.Latest.Total);
    }

    /// <summary>With a window to compare against, the figure becomes a percentage and a bar. The
    /// lookup is off the critical path — a run must not wait on a provider that may be slow or gone —
    /// so the test lets it land before reading.</summary>
    [Fact]
    public async Task A_known_window_turns_the_figure_into_a_percentage()
    {
        var tree = new ProgressTree();
        var settings = new ResolvedSettings { Mode = AgentMode.Edit };
        var runner = new SpawnedAgentRunner(
            new StubLlmService("done") { PromptTokens = 4096 },
            new StubToolHost(),
            new SkillLibrary([new SPLA.Tests.Fakes.FakeSkillSource()]),
            new PluginManager(settings),
            settings,
            contextWindow: (_, _) => Task.FromResult<int?>(16384));

        using (ProgressScope.BeginTree(tree))
        using (ProgressScope.BeginNode("agent_spawn"))
        {
            await runner.RunAsync(null, "go", AgentMode.Edit);
        }

        var run = Assert.Single(tree.Nodes, n => n.Label == "go");

        // The sentence says what the numbers mean; the numbers themselves are in the fields, where
        // every renderer already knows how to show them. Saying both printed the figure twice.
        Assert.Contains("ctx 25%", run.Latest!.Message);
        Assert.DoesNotContain("4.1k", run.Latest.Message);
        Assert.Equal(4096, run.Latest.Current);
        Assert.Equal(16384, run.Latest.Total);
        Assert.Equal(0.25, run.Latest.Fraction);
    }

    /// <summary>Nothing to say before the first call comes back, and nothing invented when the provider
    /// stays silent about it — which many endpoints do.</summary>
    [Fact]
    public async Task No_context_figure_when_the_provider_reports_none()
    {
        var tree = new ProgressTree();

        using (ProgressScope.BeginTree(tree))
        using (ProgressScope.BeginNode("agent_spawn"))
        {
            await BuildRunner("done").RunAsync(null, "go", AgentMode.Edit);
        }

        var run = Assert.Single(tree.Nodes, n => n.Label == "go");
        Assert.Null(run.Latest!.Current);
        Assert.DoesNotContain("ctx", run.Latest.Message);
    }

    /// <summary>Concurrent runs must not collide: each opens its node on its own flow, and AsyncLocal
    /// forking is what keeps them siblings rather than a chain.</summary>
    [Fact]
    public async Task Concurrent_spawns_each_get_their_own_branch()
    {
        var tree = new ProgressTree();
        var tool = new AgentSpawnTool(BuildRunner("ok"));

        using (ProgressScope.BeginTree(tree))
        using (ProgressScope.BeginNode("agent_spawn_batch"))
        {
            await Task.WhenAll(Enumerable.Range(0, 5).Select(i =>
                tool.ExecuteAsync($$"""{"skill":null,"input":"task number {{i}}"}""")));
        }

        var batch = Assert.Single(tree.Nodes, n => n.ParentId == null);
        var runs = tree.Nodes.Where(n => n.ParentId == batch.Id).Select(n => n.Label).ToList();

        Assert.Equal(5, runs.Count);
        Assert.Equal(5, runs.Distinct().Count());
    }
}
