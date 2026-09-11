using Microsoft.Extensions.Logging;
using SPLA.Agent.Composition;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Composition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Wave 2 of <c>ADR_20260911-2_agent_agents-md-scopes.md</c>: the <c>agents_md</c> setting and the
/// <see cref="ProjectAgentsContributor"/> that injects the root <c>AGENTS.md</c> under it.
/// </summary>
public sealed class ProjectAgentsContributorTests : IDisposable
{
    private readonly string _root;

    public ProjectAgentsContributorTests()
    {
        _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-agentsmd-{Guid.NewGuid():N}")).FullName;
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { /* best effort */ }
    }

    private ResolvedSettings Settings(AgentsMdMode mode = AgentsMdMode.Inject) => new()
    {
        WorkspacePath = _root,
        AgentsMd = mode,
        Instructions = [],
        Skills = new Dictionary<string, SplaSkillSection>()
    };

    private static string Compose(ResolvedSettings settings)
    {
        var composer = new AgentContextComposer([new ProjectAgentsContributor()]);
        return composer.Compose(settings, settings.WorkspacePath).SystemPrompt;
    }

    [Fact]
    public void Inject_with_no_root_file_emits_only_the_declaration()
    {
        var prompt = Compose(Settings());

        Assert.Contains("<agents> blocks", prompt);
        Assert.Contains("A narrower scope overrides a broader one on conflict.", prompt);
        Assert.DoesNotContain("<agents ", prompt);
    }

    [Fact]
    public void Inject_with_root_file_emits_declaration_and_the_file_verbatim()
    {
        File.WriteAllText(Path.Combine(_root, "AGENTS.md"), "# Root rules\n\nDo the thing carefully.");

        var prompt = Compose(Settings());

        Assert.Contains("<agents> blocks", prompt);
        Assert.Contains("<agents scope=\"\" source=\"AGENTS.md\">", prompt);
        Assert.Contains("# Root rules\n\nDo the thing carefully.", prompt);
        Assert.Contains("</agents>", prompt);
    }

    [Fact]
    public void Ignore_emits_nothing_even_with_a_root_file_present()
    {
        File.WriteAllText(Path.Combine(_root, "AGENTS.md"), "# Root rules");

        var prompt = Compose(Settings(AgentsMdMode.Ignore));

        Assert.Equal(string.Empty, prompt);
    }

    [Fact]
    public void A_role_set_to_ignore_inside_a_project_set_to_inject_gets_nothing()
    {
        File.WriteAllText(Path.Combine(_root, "AGENTS.md"), "# Root rules");

        var project = new SplaProject { Agent = new SplaAgentSection { AgentsMd = "inject" } };
        var baseline = SettingsResolver.Resolve(new SplaDefaults(), project);
        baseline.WorkspacePath = _root;

        var roleSettings = SettingsResolver.ResolveForRole(
            baseline, new SplaProject { Roles = ["narrow"], Agent = project.Agent },
            "narrow", new SplaRoleSection { AgentsMd = "ignore" });

        Assert.Equal(AgentsMdMode.Ignore, roleSettings.AgentsMd);
        Assert.Equal(string.Empty, Compose(roleSettings));
    }

    [Fact]
    public void Unknown_agents_md_value_in_the_project_manifest_fails_to_load_clearly()
    {
        var project = new SplaProject { Agent = new SplaAgentSection { AgentsMd = "sometimes" } };

        var ex = Assert.Throws<InvalidOperationException>(
            () => SettingsResolver.Resolve(new SplaDefaults(), project));

        Assert.Contains("sometimes", ex.Message);
        Assert.Contains("agents_md", ex.Message);
    }

    [Fact]
    public void Default_mode_is_inject_when_the_key_is_absent_everywhere()
    {
        var resolved = SettingsResolver.Resolve(new SplaDefaults(), new SplaProject());

        Assert.Equal(AgentsMdMode.Inject, resolved.AgentsMd);
    }

    private sealed class CapturingLogger : ILogger<SPLA.Agent.Composition.InstructionsContributor>
    {
        public readonly List<string> Warnings = [];
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel >= LogLevel.Warning) Warnings.Add(formatter(state, exception));
        }
        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    [Fact]
    public void AGENTS_md_listed_under_instructions_is_skipped_with_a_warning_not_read_twice()
    {
        File.WriteAllText(Path.Combine(_root, "AGENTS.md"), "# Root rules");
        var logger = new CapturingLogger();
        var contributor = new SPLA.Agent.Composition.InstructionsContributor(logger);

        var settings = Settings();
        settings.Instructions = ["AGENTS.md"];
        var context = new AgentContributionContext(settings, _root);

        var contribution = contributor.Contribute(context);

        Assert.Empty(contribution.Context);
        Assert.Single(logger.Warnings);
        Assert.Contains("AGENTS.md", logger.Warnings[0]);
    }
}
