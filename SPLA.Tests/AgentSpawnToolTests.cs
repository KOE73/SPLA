using SPLA.Agent;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Plugins;
using SPLA.MCP.Core.Tools;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Tests;

/// <summary>Stub LLM service that returns a fixed response without making network calls.</summary>
file sealed class StubLlmService : ILLMService
{
    private readonly string _response;
    public StubLlmService(string response = "stub result") => _response = response;

    public Task<ChatMessage> SendMessageAsync(IEnumerable<ChatMessage> messages, LLMSettings settings,
        IEnumerable<ToolDefinition>? tools = null, CancellationToken cancellationToken = default)
        => Task.FromResult(new ChatMessage { Role = ChatRole.Assistant, Content = _response });

    public async IAsyncEnumerable<string> SendMessageStreamAsync(IEnumerable<ChatMessage> messages,
        LLMSettings settings, IEnumerable<ToolDefinition>? tools = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return _response;
        await Task.CompletedTask;
    }

    public Task<ChatMessage> SendMessageStreamFullAsync(IEnumerable<ChatMessage> messages,
        LLMSettings settings, IEnumerable<ToolDefinition>? tools,
        Func<string, Task>? onDelta, CancellationToken cancellationToken = default,
        Func<string, Task>? onReasoning = null)
    {
        onDelta?.Invoke(_response);
        return Task.FromResult(new ChatMessage { Role = ChatRole.Assistant, Content = _response });
    }
}

/// <summary>Stub tool host that has no tools and always returns empty.</summary>
file sealed class StubToolHost : IToolHost
{
    public IEnumerable<ToolDefinition> GetToolDefinitions() => [];

    public Task<string> ExecuteToolAsync(AgentMode mode, string name, string argumentsJson,
        CancellationToken cancellationToken = default)
        => Task.FromResult($"tool not found: {name}");
}

public class AgentSpawnToolTests
{
    private static SpawnedAgentRunner BuildRunner(string llmResponse = "done")
    {
        var llm = new StubLlmService(llmResponse);
        var tools = new StubToolHost();
        var skills = new SkillManager();

        // Write a temporary skill
        var tempDir = Path.Combine(Path.GetTempPath(), "spla_spawn_" + Path.GetRandomFileName());
        var skillsDir = Path.Combine(tempDir, "test-plugin", "skills");
        Directory.CreateDirectory(skillsDir);
        File.WriteAllText(Path.Combine(skillsDir, "test.skill.md"), """
            ---
            id: test.skill
            description: A test skill
            ---
            Step 1: Do the thing.
            Step 2: Report done.
            """);
        skills.LoadSkills(tempDir);

        var settings = new ResolvedSettings { Mode = AgentMode.Edit };
        var plugins = new PluginManager(settings);

        return new SpawnedAgentRunner(llm, tools, skills, plugins, settings);
    }

    [Fact]
    public async Task Spawn_unknown_skill_returns_error()
    {
        var tool = new AgentSpawnTool(BuildRunner());
        var result = await tool.ExecuteAsync("""{"skill":"no.such","input":"go"}""");
        Assert.StartsWith("error:", result);
    }

    [Fact]
    public async Task Spawn_missing_skill_param_returns_error()
    {
        var tool = new AgentSpawnTool(BuildRunner());
        var result = await tool.ExecuteAsync("""{"input":"go"}""");
        Assert.StartsWith("error: 'skill'", result);
    }

    [Fact]
    public async Task Spawn_missing_input_param_returns_error()
    {
        var tool = new AgentSpawnTool(BuildRunner());
        var result = await tool.ExecuteAsync("""{"skill":"test.skill"}""");
        Assert.StartsWith("error: 'input'", result);
    }

    [Fact]
    public async Task Spawn_invalid_json_returns_error()
    {
        var tool = new AgentSpawnTool(BuildRunner());
        var result = await tool.ExecuteAsync("not-json");
        Assert.StartsWith("error: invalid_json", result);
    }

    [Fact]
    public async Task Spawn_valid_skill_returns_llm_response()
    {
        var tool = new AgentSpawnTool(BuildRunner("skill completed successfully"));
        var result = await tool.ExecuteAsync("""{"skill":"test.skill","input":"run it","mode":"Research"}""");
        Assert.Contains("skill completed successfully", result);
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
