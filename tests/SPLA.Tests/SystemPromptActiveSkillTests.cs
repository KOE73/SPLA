using SPLA.Agent;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Skills;
using SPLA.Tests.Fakes;

namespace SPLA.Tests;

public class SystemPromptActiveSkillTests
{
    private static ResolvedSettings MinimalSettings() => new()
    {
        Mode = AgentMode.Edit,
        Instructions = [],
        CustomPrompt = null,
        Skills = new Dictionary<string, SplaSkillSection>()
    };

    private static SPLA.MCP.Core.Plugins.PluginManager EmptyPluginManager()
        => new(MinimalSettings());

    private static SkillManager ManagerWith(params ISkillSource[] sources) => new(sources);

    private static SystemPromptBuilder BuilderFor(SkillManager skills, ISkillSession session)
        => new(skills, EmptyPluginManager(), session);

    [Fact]
    public void Prompt_contains_no_active_skill_block_when_idle()
    {
        var builder = BuilderFor(ManagerWith(), new SkillSession());

        var prompt = builder.Build(MinimalSettings(), Directory.GetCurrentDirectory());

        Assert.DoesNotContain("=== ACTIVE SKILL:", prompt);
    }

    [Fact]
    public void Prompt_contains_active_skill_block_when_skill_activated()
    {
        var session = new SkillSession();
        var skills = ManagerWith(new FakeSkillSource().With("test.skill", body: "Step 1: Do the thing."));
        session.Activate("test.skill");

        var prompt = BuilderFor(skills, session).Build(MinimalSettings(), Directory.GetCurrentDirectory());

        Assert.Contains("=== ACTIVE SKILL: test.skill ===", prompt);
        Assert.Contains("Step 1: Do the thing.", prompt);
        Assert.Contains("=== END ACTIVE SKILL: test.skill ===", prompt);
    }

    [Fact]
    public void Prompt_hides_ondemand_skill_list_when_skill_active()
    {
        var session = new SkillSession();
        var skills = ManagerWith(new FakeSkillSource().With("test.skill"));
        session.Activate("test.skill");

        var prompt = BuilderFor(skills, session).Build(MinimalSettings(), Directory.GetCurrentDirectory());

        Assert.DoesNotContain("Available skills:", prompt);
    }

    [Fact]
    public void Prompt_shows_ondemand_skill_list_when_idle()
    {
        var skills = ManagerWith(new FakeSkillSource().With("test.skill"));

        var prompt = BuilderFor(skills, new SkillSession()).Build(MinimalSettings(), Directory.GetCurrentDirectory());

        Assert.Contains("Available skills:", prompt);
        Assert.Contains("test.skill", prompt);
    }

    /// <summary>The original defect, at the level where it was visible: a skill whose requirements
    /// are unmet must not be advertised to the model, however it got into the registry.</summary>
    [Fact]
    public void Prompt_omits_skill_whose_required_tool_is_missing()
    {
        var skills = ManagerWith(new FakeSkillSource()
            .With("network.host-audit", requiresTools: ["port_scan"])
            .With("plain.skill"));
        skills.SetProbe(new SkillCapabilityProbe(tools: ["something_else"], features: null));

        var prompt = BuilderFor(skills, new SkillSession()).Build(MinimalSettings(), Directory.GetCurrentDirectory());

        Assert.DoesNotContain("network.host-audit", prompt);
        Assert.Contains("plain.skill", prompt);
    }

    [Fact]
    public void Prompt_instructs_skill_matching_before_tool_planning()
    {
        var skills = ManagerWith(new FakeSkillSource().With("test.skill"));

        var prompt = BuilderFor(skills, new SkillSession()).Build(MinimalSettings(), Directory.GetCurrentDirectory());

        Assert.Contains("Skill selection comes before tool planning.", prompt);
        Assert.Contains("Before explaining which tools you will use", prompt);
        Assert.Contains("compare the user's request with the available skills", prompt);
        Assert.Contains("call skill_activate with the skill id after agent_info", prompt);
        Assert.Contains("agent_info alone only previews/loads instructions and does not make the skill active", prompt);
        Assert.Contains("do not end a turn with only reasoning about the next step", prompt);
        Assert.Contains("use project scope only when the user explicitly asks", prompt);
        Assert.Contains("scope: session = this chat (default)", prompt);
    }
}
