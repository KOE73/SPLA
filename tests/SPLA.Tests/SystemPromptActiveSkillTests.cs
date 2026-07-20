using SPLA.Agent;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Plugins;
using System.IO;

namespace SPLA.Tests;

public class SystemPromptActiveSkillTests
{
    private static ResolvedSettings MinimalSettings() => new()
    {
        Mode = AgentMode.Edit,
        Instructions = [],
        CustomPrompt = null,
        Skills = new Dictionary<string, SPLA.Domain.Settings.SplaSkillSection>()
    };

    private static SPLA.MCP.Core.Plugins.PluginManager EmptyPluginManager()
        => new(MinimalSettings());

    [Fact]
    public void Prompt_contains_no_active_skill_block_when_idle()
    {
        var session = new SkillSession(); // idle
        var builder = new SystemPromptBuilder(new SkillManager(), EmptyPluginManager(), session);

        var prompt = builder.Build(MinimalSettings(), Directory.GetCurrentDirectory());

        Assert.DoesNotContain("=== ACTIVE SKILL:", prompt);
    }

    [Fact]
    public void Prompt_contains_active_skill_block_when_skill_activated()
    {
        var session = new SkillSession();
        var skills = new SkillManager();
        // Write a temporary skill file so SkillManager can load it
        var tempDir = Path.Combine(Path.GetTempPath(), "spla_test_" + Path.GetRandomFileName());
        var skillsDir = Path.Combine(tempDir, "test-plugin", "skills");
        Directory.CreateDirectory(skillsDir);
        var skillFile = Path.Combine(skillsDir, "test.skill.md");
        File.WriteAllText(skillFile, """
            ---
            id: test.skill
            description: A test skill
            ---
            Step 1: Do the thing.
            Step 2: Call skill_deactivate.
            """);

        try
        {
            skills.LoadSkills(tempDir);
            session.Activate("test.skill");
            var builder = new SystemPromptBuilder(skills, EmptyPluginManager(), session);

            var prompt = builder.Build(MinimalSettings(), Directory.GetCurrentDirectory());

            Assert.Contains("=== ACTIVE SKILL: test.skill ===", prompt);
            Assert.Contains("Step 1: Do the thing.", prompt);
            Assert.Contains("=== END ACTIVE SKILL: test.skill ===", prompt);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Prompt_hides_ondemand_skill_list_when_skill_active()
    {
        var session = new SkillSession();
        var skills = new SkillManager();

        var tempDir = Path.Combine(Path.GetTempPath(), "spla_test_" + Path.GetRandomFileName());
        var skillsDir = Path.Combine(tempDir, "test-plugin", "skills");
        Directory.CreateDirectory(skillsDir);
        File.WriteAllText(Path.Combine(skillsDir, "test.skill.md"), """
            ---
            id: test.skill
            description: A test skill
            ---
            Step 1: Do the thing.
            """);

        try
        {
            skills.LoadSkills(tempDir);
            session.Activate("test.skill");
            var builder = new SystemPromptBuilder(skills, EmptyPluginManager(), session);

            var prompt = builder.Build(MinimalSettings(), Directory.GetCurrentDirectory());

            // On-demand skill list header should not appear when a skill is active
            Assert.DoesNotContain("Available skills:", prompt);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Prompt_shows_ondemand_skill_list_when_idle()
    {
        var session = new SkillSession(); // idle
        var skills = new SkillManager();

        var tempDir = Path.Combine(Path.GetTempPath(), "spla_test_" + Path.GetRandomFileName());
        var skillsDir = Path.Combine(tempDir, "test-plugin", "skills");
        Directory.CreateDirectory(skillsDir);
        File.WriteAllText(Path.Combine(skillsDir, "test.skill.md"), """
            ---
            id: test.skill
            description: A test skill
            ---
            Step 1: Do the thing.
            """);

        try
        {
            skills.LoadSkills(tempDir);
            var builder = new SystemPromptBuilder(skills, EmptyPluginManager(), session);

            var prompt = builder.Build(MinimalSettings(), Directory.GetCurrentDirectory());

            Assert.Contains("Available skills:", prompt);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Prompt_instructs_skill_matching_before_tool_planning()
    {
        var session = new SkillSession();
        var skills = new SkillManager();

        var tempDir = Path.Combine(Path.GetTempPath(), "spla_test_" + Path.GetRandomFileName());
        var skillsDir = Path.Combine(tempDir, "test-plugin", "skills");
        Directory.CreateDirectory(skillsDir);
        File.WriteAllText(Path.Combine(skillsDir, "test.skill.md"), """
            ---
            id: test.skill
            description: A test skill
            ---
            Step 1: Do the thing.
            """);

        try
        {
            skills.LoadSkills(tempDir);
            var builder = new SystemPromptBuilder(skills, EmptyPluginManager(), session);

            var prompt = builder.Build(MinimalSettings(), Directory.GetCurrentDirectory());

            Assert.Contains("Skill selection comes before tool planning.", prompt);
            Assert.Contains("Before explaining which tools you will use", prompt);
            Assert.Contains("compare the user's request with the available skills", prompt);
            Assert.Contains("call skill_activate with the skill id after agent_info", prompt);
            Assert.Contains("agent_info alone only previews/loads instructions and does not make the skill active", prompt);
            Assert.Contains("do not end a turn with only reasoning about the next step", prompt);
            Assert.Contains("use project scope only when the user explicitly asks", prompt);
            Assert.Contains("scope: session = this chat (default)", prompt);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
