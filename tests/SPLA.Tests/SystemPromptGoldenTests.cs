using SPLA.Agent;
using SPLA.Agent.Composition;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Agent;
using SPLA.MCP.Core.Composition;
using SPLA.MCP.Core.Skills;
using SPLA.Tests.Fakes;
using System.Runtime.CompilerServices;

namespace SPLA.Tests;

/// <summary>
/// Byte-for-byte guard on the assembled system prompt. The prompt is a contract with the model:
/// every separator, blank line and machine header in it is load-bearing, and a refactor of how it is
/// assembled must not move a single character. Content assertions ("contains X") cannot see that —
/// this can.
///
/// <para>Golden files live beside this test. When one is missing it is written from the actual
/// output and the test fails once, on purpose: a golden must be reviewed in a diff before it counts
/// as expected. To re-approve an intentional wording change, delete the file and run twice.</para>
/// </summary>
public sealed class SystemPromptGoldenTests
{
    private static ResolvedSettings Settings(string workspace, string? customPrompt, List<string>? instructions) => new()
    {
        Mode = AgentMode.Edit,
        WorkspacePath = workspace,
        Instructions = instructions ?? [],
        CustomPrompt = customPrompt,
        Skills = new Dictionary<string, SplaSkillSection>()
    };

    /// <summary>Two features, deliberately: one with a prompt fragment and one tools-only, so the
    /// golden also pins that a null fragment contributes nothing.</summary>
    private static IReadOnlyList<IAgentFeature> Features() =>
    [
        new AgentFeature("core.skills", promptFragment: "SKILLS FEATURE FRAGMENT\nsecond line."),
        new AgentFeature("core.shell"),
        new AgentFeature("core.workspace", promptFragment: "Your working directory is {{workingDirectory}}."),
    ];

    [Fact]
    public void Idle_prompt_matches_golden()
    {
        var root = Directory.CreateTempSubdirectory("spla-prompt-golden-").FullName;
        try
        {
            File.WriteAllText(Path.Combine(root, "RULES.md"), "Rule one.\nRule two.");

            var skills = new SkillLibrary([new FakeSkillSource()
                .With("always.on", body: "Preloaded body line.", description: "always in the prompt", preloaded: true)
                .With("on.demand", body: "On-demand body.", description: "loaded when asked")]);

            var builder = new AgentContextComposer(AgentContributors.Default(
                skills, new SPLA.MCP.Core.Plugins.PluginManager(Settings(root, null, null)),
                new SkillSession(), Features()));

            var prompt = builder.Compose(Settings(root, "CUSTOM PROMPT TEXT", ["RULES.md"]), root).SystemPrompt;

            AssertGolden("system-prompt-idle.golden.txt", prompt.Replace(root, "{{WORKSPACE}}"));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Active_skill_prompt_matches_golden()
    {
        var root = Directory.CreateTempSubdirectory("spla-prompt-golden-").FullName;
        try
        {
            var session = new SkillSession();
            session.Activate("on.demand", "On-demand body.");

            var skills = new SkillLibrary([new FakeSkillSource()
                .With("always.on", body: "Preloaded body line.", description: "always in the prompt", preloaded: true)
                .With("on.demand", body: "On-demand body.", description: "loaded when asked")]);

            var builder = new AgentContextComposer(AgentContributors.Default(
                skills, new SPLA.MCP.Core.Plugins.PluginManager(Settings(root, null, null)),
                session, Features()));

            var prompt = builder.Compose(Settings(root, null, null), root).SystemPrompt;

            AssertGolden("system-prompt-active-skill.golden.txt", prompt.Replace(root, "{{WORKSPACE}}"));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    private static void AssertGolden(string fileName, string actual, [CallerFilePath] string thisFile = "")
    {
        var path = Path.Combine(Path.GetDirectoryName(thisFile)!, "Goldens", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        if (!File.Exists(path))
        {
            File.WriteAllText(path, actual);
            Assert.Fail($"Golden '{fileName}' did not exist and was written from the actual output. " +
                        "Review it in a diff, then re-run.");
        }

        Assert.Equal(File.ReadAllText(path).Replace("\r\n", "\n"), actual.Replace("\r\n", "\n"));
    }
}
