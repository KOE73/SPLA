using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Agent;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Agent;
using SPLA.MCP.Core.Plugins;
using SPLA.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPLA.Tests;

/// <summary>
/// The modular capabilities feature (docs/... ТЗ "модульные capabilities агентского ядра"): each
/// built-in core.* feature bundles tools + prompt fragment together, gated as a unit by
/// <c>agent.capabilities</c> in the project file. These tests exercise the gating end-to-end through
/// <see cref="AgentRuntime"/> (tool registration) and <see cref="SPLA.Agent.SystemPromptBuilder"/>
/// (prompt segments) — the same objects the CLI/service actually use.
/// </summary>
public sealed class AgentCapabilitiesTests
{
    /// <summary>Every tool name a fully-enabled core catalog registers (see AgentRuntime's feature
    /// catalog). Kept explicit so a change to the catalog is caught here, not just "some tools".</summary>
    private static readonly string[] AllCoreToolNames =
    {
        "agent_get_context", "agent_get_current_time",
        "agent_info",
        "system_list_files", "system_read_file", "system_search_text", "system_find_files",
        "system_create_file", "system_patch_file", "system_write_file", "system_delete_file", "image_view",
        "system_run_shell",
        "web_fetch",
        "agent_memory_set", "agent_memory_get", "agent_memory_delete", "agent_memory_list", "agent_memory_clear",
        "checkpoint_save", "context_rollback", "mark_set", "mark_rollback",
        "skill_activate", "skill_deactivate",
        "agent_spawn", "agent_spawn_batch",
        "agent_clarify",
    };

    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-capabilities-{System.Guid.NewGuid():N}")).FullName;

    private static AgentRuntime BuildRuntime(string root, string? capabilitiesYaml)
    {
        var manifest = Path.Combine(root, "test.spla");
        var agentBlock = capabilitiesYaml != null
            ? $"agent:\n  mode: Edit\n{capabilitiesYaml}"
            : "agent:\n  mode: Edit";
        File.WriteAllText(manifest, $"""
            version: 1
            name: CapabilitiesTest
            workspace: .
            {agentBlock}
            """);

        var settings = ConfigLoader.LoadAndResolve(manifest);
        return new AgentRuntime(settings, NullLoggerFactory.Instance);
    }

    private static string[] RegisteredToolNames(AgentRuntime runtime)
        => runtime.McpHost.GetToolDefinitions().Select(d => d.Function.Name).OrderBy(n => n).ToArray();

    [Fact]
    public void Capabilities_absent_enables_every_core_feature()
    {
        var root = TempRoot();
        try
        {
            using var runtime = BuildRuntime(root, capabilitiesYaml: null);

            Assert.Equal(AllCoreToolNames.OrderBy(n => n), RegisteredToolNames(runtime));

            var prompt = runtime.PromptBuilder.Build(runtime.Settings, runtime.Settings.WorkspacePath);
            Assert.Contains("Your current working directory is:", prompt);
            Assert.Contains("Tool descriptions are intentionally short.", prompt);
            Assert.Contains("Mermaid note:", prompt);
            Assert.Contains("KV memory (5 tools)", prompt);
            Assert.Contains("context management (4 tools)", prompt);
            Assert.Contains("Skill selection comes before tool planning.", prompt);
            Assert.Contains("Data channel — bulk output", prompt);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Empty_capabilities_list_disables_every_core_tool_and_core_segment_but_keeps_the_rest()
    {
        var root = TempRoot();
        try
        {
            using var runtime = BuildRuntime(root, "  capabilities: []\n");

            Assert.Empty(RegisteredToolNames(runtime));
            Assert.Empty(runtime.EnabledFeatureIds);

            var segments = runtime.PromptBuilder.BuildSegments(runtime.Settings, runtime.Settings.WorkspacePath);
            Assert.DoesNotContain(segments, s => s.Kind == SPLA.Agent.PromptSegmentKind.Core);
            Assert.Contains(segments, s => s.Kind == SPLA.Agent.PromptSegmentKind.Mode);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Checkpoints_capability_auto_enables_its_memory_dependency()
    {
        var root = TempRoot();
        try
        {
            using var runtime = BuildRuntime(root, "  capabilities: [core.checkpoints]\n");

            Assert.Contains("core.memory", runtime.EnabledFeatureIds);
            Assert.Contains("core.checkpoints", runtime.EnabledFeatureIds);

            var names = RegisteredToolNames(runtime);
            Assert.Contains("agent_memory_set", names);
            Assert.Contains("checkpoint_save", names);
            // Nothing outside the two auto-resolved features was pulled in.
            Assert.DoesNotContain("system_list_files", names);
            Assert.DoesNotContain("skill_activate", names);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Only_features_with_a_prompt_fragment_produce_a_core_segment()
    {
        // Direct construction, no AgentRuntime: the builder renders exactly what the supplied
        // IAgentFeature objects carry — a null PromptFragment yields no Core segment, text yields one.
        var settings = new ResolvedSettings
        {
            Mode = SPLA.Domain.Models.AgentMode.Edit,
            Instructions = [],
            CustomPrompt = null,
            Skills = new Dictionary<string, SplaSkillSection>()
        };
        var features = new List<IAgentFeature>
        {
            new AgentFeature("core.shell"), // tools-only: PromptFragment = null
            new AgentFeature("core.memory", promptFragment: "MEMORY FRAGMENT TEXT"),
        };
        var builder = new SystemPromptBuilder(
            new SkillManager(), new PluginManager(settings), null, features);

        var segments = builder.BuildSegments(settings, Directory.GetCurrentDirectory());

        var core = segments.Where(s => s.Kind == PromptSegmentKind.Core).ToList();
        var only = Assert.Single(core);
        Assert.Equal("Core: core.memory", only.Title);
        Assert.Equal("MEMORY FRAGMENT TEXT", only.Body);
    }

    [Fact]
    public void Unknown_capability_id_is_ignored_without_failing_to_load()
    {
        var root = TempRoot();
        try
        {
            using var runtime = BuildRuntime(root, "  capabilities: [core.memory, not.a.real.feature]\n");

            Assert.Contains("core.memory", runtime.EnabledFeatureIds);
            Assert.DoesNotContain("not.a.real.feature", runtime.EnabledFeatureIds);
            Assert.Contains("agent_memory_set", RegisteredToolNames(runtime));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
