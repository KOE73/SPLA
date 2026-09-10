using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// A role gets exactly what it declares — more than <c>agent:</c> where it says so, less where it
/// narrows — for listing, for execution and for the prompt alike. Written after a coordinator with no
/// file tools left every worker it spawned without them too: the built-in tools were registered once
/// per process from <c>agent:</c>'s capabilities, and a role could only filter what was there.
/// </summary>
public sealed class RoleCapabilityTests
{
    private static (AgentRuntime Runtime, ChatRegistry Chats, string Root) BuildProject(
        string agentBlock, params (string Name, string Body)[] roles)
    {
        var root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-rolecaps-{Guid.NewGuid():N}")).FullName;
        var rolesDir = Directory.CreateDirectory(Path.Combine(root, "roles")).FullName;
        foreach (var (name, body) in roles)
            File.WriteAllText(Path.Combine(rolesDir, name + ".yaml"), body);

        var manifest = Path.Combine(root, "test.spla");
        File.WriteAllText(manifest,
            "version: 1\nname: RoleCapabilityTest\nworkspace: .\n" + agentBlock +
            $"roles: [{string.Join(", ", roles.Select(r => r.Name))}]\n");

        var runtime = new AgentRuntime(ConfigLoader.LoadAndResolve(manifest), NullLoggerFactory.Instance);
        return (runtime, new ChatRegistry(runtime), root);
    }

    private const string CoordinatorAgent =
        "agent:\n  mode: Agent\n  capabilities: [core.memory]\n  custom_prompt: COORDINATOR PROMPT\n";

    private const string WorkerRole =
        "mode: Agent\ncapabilities: [core.files, core.memory]\ncustom_prompt: WORKER PROMPT\n";

    private static IDisposable SessionUnder(ResolvedSettings? settings) =>
        AgentSessionScope.Begin(new AgentSession(
            new KeyValueStore("session"), new MarkManager(), new SkillSession(), settings: settings));

    [Fact]
    public void A_role_sees_a_capability_the_default_agent_does_not_have()
    {
        var (runtime, chats, root) = BuildProject(CoordinatorAgent, ("worker", WorkerRole));
        try
        {
            var worker = chats.CreateNew("worker", role: "worker");
            var coordinator = chats.CreateNew("coordinator");

            Assert.Contains("system_read_file", worker.AvailableToolNames());
            Assert.DoesNotContain("system_read_file", coordinator.AvailableToolNames());
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void A_roles_prompt_is_built_from_the_roles_own_settings()
    {
        var (runtime, chats, root) = BuildProject(CoordinatorAgent, ("worker", WorkerRole));
        try
        {
            var workerPrompt = chats.CreateNew("worker", role: "worker").ComposeContext().SystemPrompt;
            var coordinatorPrompt = chats.CreateNew("coordinator").ComposeContext().SystemPrompt;

            Assert.Contains("WORKER PROMPT", workerPrompt);
            Assert.DoesNotContain("COORDINATOR PROMPT", workerPrompt);
            Assert.Contains("COORDINATOR PROMPT", coordinatorPrompt);
            Assert.DoesNotContain("WORKER PROMPT", coordinatorPrompt);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    /// <summary>A capability off for the session is refused when called, not only hidden from the
    /// list — a model calls what it remembers from history or guesses, not only what it was shown.</summary>
    [Fact]
    public async Task A_capability_off_for_the_session_is_refused_at_execution()
    {
        var (runtime, _, root) = BuildProject(
            "agent:\n  mode: Agent\n", ("narrow", "capabilities: [core.files]\n"));
        try
        {
            var manifestDir = Path.GetDirectoryName(runtime.Settings.ProjectFilePath)!;
            var narrow = SettingsResolver.ResolveForRole(
                runtime.Settings, runtime.Settings.Manifest!, "narrow", ConfigLoader.LoadRole(manifestDir, "narrow"));

            using (SessionUnder(narrow))
            {
                var refused = await runtime.McpHost.ExecuteToolAsync(AgentMode.Agent, "agent_memory_list", "{}");
                Assert.Contains("not found", refused.TextContent);
            }

            using (SessionUnder(null))
            {
                var allowed = await runtime.McpHost.ExecuteToolAsync(AgentMode.Agent, "agent_memory_list", "{}");
                Assert.DoesNotContain("not found", allowed.TextContent);
            }
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    /// <summary>Outside any session — a settings panel, a foreign MCP head — the project's own list is
    /// what counts, so registering every built-in tool does not widen what the project exposes.</summary>
    [Fact]
    public void Outside_a_session_the_projects_capabilities_decide()
    {
        var (runtime, _, root) = BuildProject(CoordinatorAgent, ("worker", WorkerRole));
        try
        {
            var names = runtime.McpHost.GetToolDefinitions().Select(d => d.Function.Name).ToList();

            Assert.Contains("agent_memory_set", names);
            Assert.DoesNotContain("system_read_file", names);
            Assert.DoesNotContain("system_read_file", runtime.McpHost.GetPermittedToolNames());
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }
}
