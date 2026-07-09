using SPLA.Domain.Agent;

namespace SPLA.Tests;

public class SkillSessionTests
{
    [Fact]
    public void Activate_sets_active_skill_id()
    {
        var session = new SkillSession();
        session.Activate("network.range-audit");
        Assert.Equal("network.range-audit", session.ActiveSkillId);
    }

    [Fact]
    public void Activate_raises_Changed()
    {
        var session = new SkillSession();
        var raised = false;
        session.Changed += (_, _) => raised = true;
        session.Activate("network.range-audit");
        Assert.True(raised);
    }

    [Fact]
    public void Deactivate_clears_active_skill_id()
    {
        var session = new SkillSession();
        session.Activate("network.range-audit");
        session.Deactivate();
        Assert.Null(session.ActiveSkillId);
    }

    [Fact]
    public void Deactivate_raises_Changed()
    {
        var session = new SkillSession();
        session.Activate("network.range-audit");
        var raised = false;
        session.Changed += (_, _) => raised = true;
        session.Deactivate();
        Assert.True(raised);
    }

    [Fact]
    public void Deactivate_when_idle_is_noop()
    {
        var session = new SkillSession();
        var raised = false;
        session.Changed += (_, _) => raised = true;
        session.Deactivate(); // no-op
        Assert.False(raised);
        Assert.Null(session.ActiveSkillId);
    }

    [Fact]
    public void Activate_while_skill_active_throws()
    {
        var session = new SkillSession();
        session.Activate("network.range-audit");
        Assert.Throws<InvalidOperationException>(() => session.Activate("host-audit"));
    }

    [Fact]
    public void Can_activate_after_deactivate()
    {
        var session = new SkillSession();
        session.Activate("network.range-audit");
        session.Deactivate();
        session.Activate("host-audit"); // must not throw
        Assert.Equal("host-audit", session.ActiveSkillId);
    }
}
