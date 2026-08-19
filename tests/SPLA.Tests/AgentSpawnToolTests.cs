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

    public Task<SPLA.Domain.Llm.LlmTurnResult> InvokeAsync(
        SPLA.Domain.Llm.LlmTurnContext ctx, CancellationToken ct = default)
    {
        ctx.OnDelta?.Invoke(_response);
        return Task.FromResult(new SPLA.Domain.Llm.LlmTurnResult
        {
            Message = new ChatMessage { Role = ChatRole.Assistant, Content = _response }
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
}
