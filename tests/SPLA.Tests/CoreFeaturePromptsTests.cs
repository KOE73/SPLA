using SPLA.Agent;

namespace SPLA.Tests;

/// <summary>
/// Guards the core.workspace prompt fragment against re-introducing a session-start order to hunt
/// for an AGENTS.md file. That instruction was removed (PLAN_20260911 wave 0): it duplicated
/// sequencing the global prompt already owns and conflicted with the not-yet-built agents_md mode.
/// </summary>
public sealed class CoreFeaturePromptsTests
{
    [Fact]
    public void Workspace_prompt_does_not_instruct_looking_for_agents_md()
    {
        var fragment = CoreFeaturePrompts.Load("core.workspace");

        Assert.NotNull(fragment);
        Assert.DoesNotContain("look for an AGENTS.md", fragment, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void No_core_feature_prompt_instructs_looking_for_agents_md()
    {
        foreach (var featureId in new[]
        {
            "core.workspace", "core.discipline", "core.memory", "core.checkpoints", "core.skills",
            "core.blobs", "core.toolsets", "core.shell", "core.background_tasks", "core.roles",
        })
        {
            var fragment = CoreFeaturePrompts.Load(featureId);
            if (fragment == null) continue;

            Assert.DoesNotContain("look for an AGENTS.md", fragment, StringComparison.OrdinalIgnoreCase);
        }
    }
}
