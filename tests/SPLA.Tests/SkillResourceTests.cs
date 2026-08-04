using SPLA.Domain.Agent;
using SPLA.MCP.Core.Skills;
using SPLA.MCP.Core.Tools;
using SPLA.Tests.Fakes;

namespace SPLA.Tests;

/// <summary>
/// Attachments — the <c>references/</c> and <c>assets/</c> that ship beside a SKILL.md.
///
/// <para>Two halves are tested separately because they enforce different things. The SOURCE decides
/// what a skill's resources are and refuses anything outside its folder; the TOOL decides whether
/// this chat is entitled to ask at all. Neither is a substitute for the other: a correct source with
/// no loan check would serve any skill's references to any chat, and a correct loan check over a
/// careless source would serve <c>../../etc/passwd</c> to the one chat that is entitled.</para>
/// </summary>
public class SkillResourceTests : IDisposable
{
    private readonly string _root =
        Path.Combine(Path.GetTempPath(), "spla-skill-res-" + Guid.NewGuid().ToString("N"));

    public SkillResourceTests()
    {
        // The folder layout the real SPLA.Skills repo uses: a skill IS a folder, and everything
        // beside its SKILL.md belongs to it.
        Write("linux-host/capture/SKILL.md", "---\nid: linux-host.capture\n---\nStep 0: read the contract.");
        Write("linux-host/capture/references/package-contract.md", "THE CONTRACT");
        Write("linux-host/capture/references/ssh-tools.md", "THE SSH TOOLS");
        Write("linux-host/capture/assets/HOST.template.md", "THE TEMPLATE");
        Write("linux-host/restore/SKILL.md", "---\nid: linux-host.restore\n---\nStep 1.");
        Write("linux-host/restore/references/package-contract.md", "SOMEONE ELSE'S CONTRACT");
        Write("loose.md", "---\nid: loose\n---\nA bare file, no folder of its own.");
        Write("secret.txt", "NOT AN APPENDIX");
    }

    private void Write(string relative, string text)
    {
        var path = Path.Combine(_root, relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    private DirectorySkillSource Source() =>
        new("repo", "Repo", _root, SkillTrust.Trusted, watch: false);

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch (IOException) { }
    }

    // ── The source: what belongs to a skill ──────────────────────────────────

    [Fact]
    public void Folder_skill_lists_everything_beside_its_SKILL_md()
    {
        var resources = Source().ListResources("linux-host/capture/SKILL.md");

        Assert.Equal(
            ["assets/HOST.template.md", "references/package-contract.md", "references/ssh-tools.md"],
            resources);
    }

    [Fact]
    public void Resource_text_is_read_from_the_source()
    {
        Assert.Equal("THE CONTRACT",
            Source().ReadResource("linux-host/capture/SKILL.md", "references/package-contract.md"));
    }

    /// <summary>A bare <c>name.md</c> has no attachments: its neighbours are other people's skills,
    /// not its own vkladyshi. Carrying attachments is exactly what the folder layout is for.</summary>
    [Fact]
    public void Bare_file_skill_has_no_resources()
    {
        var source = Source();

        Assert.Empty(source.ListResources("loose.md"));
        Assert.Null(source.ReadResource("loose.md", "secret.txt"));
    }

    /// <summary>These strings arrive from a model, so the escape check is the source's job and runs
    /// before anything touches the disk.</summary>
    [Theory]
    [InlineData("../restore/references/package-contract.md")]
    [InlineData("../../secret.txt")]
    [InlineData("references/../../../secret.txt")]
    public void Resource_path_cannot_escape_the_skill_folder(string path)
    {
        Assert.Null(Source().ReadResource("linux-host/capture/SKILL.md", path));
    }

    [Fact]
    public void Absolute_resource_path_is_refused()
    {
        var outside = Path.Combine(_root, "secret.txt");

        Assert.Null(Source().ReadResource("linux-host/capture/SKILL.md", outside));
    }

    /// <summary>The procedure is already pinned in the session; letting it back in through the
    /// resource door would just be a second way to read the same text.</summary>
    [Fact]
    public void SKILL_md_is_not_one_of_its_own_resources()
    {
        var source = Source();

        Assert.DoesNotContain("SKILL.md", source.ListResources("linux-host/capture/SKILL.md"));
        Assert.Null(source.ReadResource("linux-host/capture/SKILL.md", "SKILL.md"));
    }

    [Fact]
    public void Unknown_skill_ref_has_no_resources()
    {
        var source = Source();

        Assert.Empty(source.ListResources("nope/SKILL.md"));
        Assert.Null(source.ReadResource("nope/SKILL.md", "references/package-contract.md"));
    }

    // ── The tool: who is entitled to ask ─────────────────────────────────────

    private static IDisposable Scope(ISkillSession skills)
        => AgentSessionScope.Begin(new AgentSession(new KeyValueStore("session"), new MarkManager(), skills));

    private static FakeSkillSource TwoSkillsWithResources() =>
        new FakeSkillSource()
            .With("mine").WithResource("mine", "references/contract.md", "MY CONTRACT")
            .With("theirs").WithResource("theirs", "references/secret.md", "THEIR SECRET");

    [Fact]
    public async Task Active_skill_can_read_its_own_resource()
    {
        var skills = new SkillManager([TwoSkillsWithResources()]);
        var session = new SkillSession();
        using var _ = Scope(session);

        await new SkillActivateTool(skills).ExecuteAsync("""{"id":"mine"}""");
        var result = await new SkillReadResourceTool(skills)
            .ExecuteAsync("""{"path":"references/contract.md"}""");

        Assert.Equal("MY CONTRACT", result);
    }

    /// <summary>The list of what came with the book is pinned at activation and printed in the ACTIVE
    /// SKILL block — the model cannot ask for what it does not know exists.</summary>
    [Fact]
    public async Task Activation_pins_the_loan_slip()
    {
        var skills = new SkillManager([TwoSkillsWithResources()]);
        var session = new SkillSession();
        using var _ = Scope(session);

        await new SkillActivateTool(skills).ExecuteAsync("""{"id":"mine"}""");

        Assert.Equal("test", session.ActiveSourceId);
        Assert.Equal("mine.md", session.ActiveRef);
        Assert.Equal(["references/contract.md"], session.ActiveResources);
    }

    /// <summary>The tool has no argument for naming a skill, so the only way to reach another one is
    /// to guess its paths. The loan slip is what refuses that.</summary>
    [Fact]
    public async Task Another_skills_resource_is_refused()
    {
        var skills = new SkillManager([TwoSkillsWithResources()]);
        var session = new SkillSession();
        using var _ = Scope(session);

        await new SkillActivateTool(skills).ExecuteAsync("""{"id":"mine"}""");
        var result = await new SkillReadResourceTool(skills)
            .ExecuteAsync("""{"path":"references/secret.md"}""");

        Assert.StartsWith("error:", result);
        Assert.DoesNotContain("THEIR SECRET", result);
    }

    [Fact]
    public async Task Reading_without_an_active_skill_is_refused()
    {
        var skills = new SkillManager([TwoSkillsWithResources()]);
        using var _ = Scope(new SkillSession());

        var result = await new SkillReadResourceTool(skills)
            .ExecuteAsync("""{"path":"references/contract.md"}""");

        Assert.StartsWith("error:", result);
        Assert.Contains("no skill is active", result);
    }

    /// <summary>Deactivation returns the loan slip with the book. Otherwise the tool would keep
    /// serving the attachments of a skill nobody is running.</summary>
    [Fact]
    public async Task Resources_stop_being_readable_after_deactivation()
    {
        var skills = new SkillManager([TwoSkillsWithResources()]);
        var session = new SkillSession();
        using var _ = Scope(session);

        await new SkillActivateTool(skills).ExecuteAsync("""{"id":"mine"}""");
        await new SkillDeactivateTool().ExecuteAsync("{}");
        var result = await new SkillReadResourceTool(skills)
            .ExecuteAsync("""{"path":"references/contract.md"}""");

        Assert.StartsWith("error:", result);
        Assert.Empty(session.ActiveResources);
    }

    /// <summary>"The source vanished while the book is out": the slip stays valid, the read fails
    /// honestly, and the pinned procedure keeps running rather than the whole chat falling over.
    /// This is the case the live-reference decision was made on.</summary>
    [Fact]
    public async Task Vanished_source_fails_the_read_but_not_the_run()
    {
        var source = TwoSkillsWithResources();
        var skills = new SkillManager([source]);
        var session = new SkillSession();
        using var _ = Scope(session);

        await new SkillActivateTool(skills).ExecuteAsync("""{"id":"mine"}""");
        source.Offline = true;

        var result = await new SkillReadResourceTool(skills)
            .ExecuteAsync("""{"path":"references/contract.md"}""");

        Assert.StartsWith("error:", result);
        Assert.Equal("mine", session.ActiveSkillId);
        Assert.False(string.IsNullOrEmpty(session.ActiveBody));
    }

    // ── The two halves together, on the real folder layout ───────────────────

    /// <summary>Step 0 of <c>linux-host.capture</c> end to end: activate the folder skill, then read
    /// the two references it names — through the tool, not through the file system.</summary>
    [Fact]
    public async Task Folder_skill_step_zero_reads_its_references_through_the_tool()
    {
        var skills = new SkillManager([Source()]);
        var session = new SkillSession();
        using var _ = Scope(session);

        Assert.StartsWith("ok:",
            await new SkillActivateTool(skills).ExecuteAsync("""{"id":"linux-host.capture"}"""));

        var tool = new SkillReadResourceTool(skills);
        Assert.Equal("THE CONTRACT",
            await tool.ExecuteAsync("""{"path":"references/package-contract.md"}"""));
        Assert.Equal("THE SSH TOOLS",
            await tool.ExecuteAsync("""{"path":"references/ssh-tools.md"}"""));

        // The neighbouring skill's identically named reference stays its own.
        Assert.DoesNotContain("SOMEONE ELSE'S",
            await tool.ExecuteAsync("""{"path":"../restore/references/package-contract.md"}"""));
    }
}
