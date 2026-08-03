using SPLA.Agent;
using SPLA.Agent.Composition;
using SPLA.Domain.Agent;
using SPLA.MCP.Core.Composition;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Skills;
using SPLA.MCP.Core.Tools;
using SPLA.Tests.Fakes;

namespace SPLA.Tests;

/// <summary>
/// Covers the two halves of "edit a skill without restarting, but not out from under a run":
/// the body a skill starts with is pinned in the session for its whole run, and the prompt
/// assembler finds that session through the ambient scope rather than a constructor.
/// </summary>
public class SkillPinningTests
{
    private const string Original = "Step 1: the original procedure.";
    private const string Edited = "Step 1: the edited procedure.";

    private static ResolvedSettings MinimalSettings() => new()
    {
        Mode = AgentMode.Edit,
        Instructions = [],
        CustomPrompt = null,
        Skills = new Dictionary<string, SplaSkillSection>()
    };

    private static AgentContextComposer BuilderFor(SkillManager skills, ISkillSession? session = null)
        => new(AgentContributors.Default(
            skills, new SPLA.MCP.Core.Plugins.PluginManager(MinimalSettings()), session));

    private static string Build(AgentContextComposer composer)
        => composer.Compose(MinimalSettings(), Directory.GetCurrentDirectory()).SystemPrompt;

    private static IDisposable Scope(ISkillSession skills)
        => AgentSessionScope.Begin(new AgentSession(new KeyValueStore("session"), new MarkManager(), skills));

    // ── Activation captures the body ─────────────────────────────────────────

    [Fact]
    public async Task Activate_captures_the_body_into_the_session()
    {
        var source = new FakeSkillSource().With("test.skill", body: Original);
        var session = new SkillSession();

        using var _ = Scope(session);
        var result = await new SkillActivateTool(new SkillManager([source])).ExecuteAsync("""{"id":"test.skill"}""");

        Assert.StartsWith("ok:", result);
        Assert.Equal(Original, session.ActiveBody);
    }

    /// <summary>A source that enumerates a skill but cannot produce its text must fail the
    /// activation — activating into an empty ACTIVE SKILL block would leave the model believing it
    /// is following a procedure that is not there.</summary>
    [Fact]
    public async Task Activate_fails_when_the_body_cannot_be_read()
    {
        var source = new FakeSkillSource().With("test.skill", body: "");
        var session = new SkillSession();

        using var _ = Scope(session);
        var result = await new SkillActivateTool(new SkillManager([source])).ExecuteAsync("""{"id":"test.skill"}""");

        Assert.StartsWith("error:", result);
        Assert.Contains("no readable procedure", result);
        Assert.Null(session.ActiveSkillId);
    }

    // ── The pin itself ───────────────────────────────────────────────────────

    /// <summary>The requirement in one test: the file of a running skill is edited, the source
    /// reloads, and the procedure the model is following does not move.</summary>
    [Fact]
    public void Editing_a_running_skill_leaves_its_prompt_body_untouched()
    {
        var source = new FakeSkillSource().With("test.skill", body: Original);
        var skills = new SkillManager([source]);
        var session = new SkillSession();
        session.Activate("test.skill", skills.LoadBody("test.skill")!);

        source.With("test.skill", body: Edited);   // author edits the file
        source.Raise();                            // watcher fires, manager re-enumerates

        var prompt = Build(BuilderFor(skills, session));

        Assert.Contains(Original, prompt);
        Assert.DoesNotContain(Edited, prompt);
    }

    /// <summary>The other half: the pin lasts for the run, not forever. Once the skill ends, the
    /// next activation gets the edited text — which is what makes the hot reload worth having.</summary>
    [Fact]
    public void Reactivating_after_the_run_picks_up_the_edit()
    {
        var source = new FakeSkillSource().With("test.skill", body: Original);
        var skills = new SkillManager([source]);
        var session = new SkillSession();
        session.Activate("test.skill", skills.LoadBody("test.skill")!);

        source.With("test.skill", body: Edited);
        source.Raise();

        session.Deactivate();
        session.Activate("test.skill", skills.LoadBody("test.skill")!);

        var prompt = Build(BuilderFor(skills, session));

        Assert.Contains(Edited, prompt);
        Assert.DoesNotContain(Original, prompt);
    }

    /// <summary>Deleting the skill outright is the same story: a run in flight survives it, because
    /// nothing re-reads the source while the skill is active.</summary>
    [Fact]
    public void Deleting_a_running_skill_does_not_empty_the_prompt()
    {
        var source = new FakeSkillSource().With("test.skill", body: Original);
        var skills = new SkillManager([source]);
        var session = new SkillSession();
        session.Activate("test.skill", skills.LoadBody("test.skill")!);

        source.Offline = true;   // the whole provider goes away
        skills.Reload();

        Assert.Contains(Original, Build(BuilderFor(skills, session)));
    }

    // ── Ambient resolution ───────────────────────────────────────────────────

    /// <summary>The runtime's prompt builder is a singleton and the skill session belongs to a chat,
    /// so they cannot be tied by constructor. Without this the ACTIVE SKILL block never rendered
    /// outside spawned sub-agents, however correct the rest of the machinery was.</summary>
    [Fact]
    public void Builder_without_a_session_resolves_the_ambient_one()
    {
        var skills = new SkillManager([new FakeSkillSource().With("test.skill", body: Original)]);
        var session = new SkillSession();
        session.Activate("test.skill", Original);

        using var _ = Scope(session);
        var prompt = Build(BuilderFor(skills));

        Assert.Contains("=== ACTIVE SKILL: test.skill ===", prompt);
        Assert.Contains(Original, prompt);
    }

    [Fact]
    public void Builder_outside_any_scope_renders_no_active_skill()
    {
        var skills = new SkillManager([new FakeSkillSource().With("test.skill", body: Original)]);

        var prompt = Build(BuilderFor(skills));

        Assert.DoesNotContain("=== ACTIVE SKILL:", prompt);
    }

    /// <summary>An explicitly supplied session wins over the ambient one — that is what keeps a
    /// spawned sub-agent's prompt describing its own skill while it runs inside the parent's flow.</summary>
    [Fact]
    public void Explicit_session_wins_over_the_ambient_one()
    {
        var skills = new SkillManager([
            new FakeSkillSource().With("parent.skill", body: "PARENT BODY").With("child.skill", body: "CHILD BODY")
        ]);

        var parent = new SkillSession();
        parent.Activate("parent.skill", "PARENT BODY");
        var child = new SkillSession();
        child.Activate("child.skill", "CHILD BODY");

        using var _ = Scope(parent);
        var prompt = Build(BuilderFor(skills, child));

        Assert.Contains("=== ACTIVE SKILL: child.skill ===", prompt);
        Assert.DoesNotContain("parent.skill", prompt);
    }

    /// <summary>The skills index is suppressed while a skill runs so the model is not invited to
    /// switch mid-procedure. It has to hold through the ambient path too, or the suppression silently
    /// stops applying everywhere except sub-agents.</summary>
    [Fact]
    public void Ambient_active_skill_suppresses_the_skills_index()
    {
        var skills = new SkillManager([
            new FakeSkillSource().With("test.skill", body: Original).With("other.skill")
        ]);
        var session = new SkillSession();
        session.Activate("test.skill", Original);

        using var _ = Scope(session);

        Assert.DoesNotContain("Available skills:", Build(BuilderFor(skills)));
    }
}
