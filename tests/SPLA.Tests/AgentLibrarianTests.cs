using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Library;
using SPLA.Library.Librarians;
using SPLA.Library.Sources;
using SPLA.MCP.Core.Tools;
using SPLA.Tests.Fakes;

namespace SPLA.Tests;

/// <summary>
/// The librarian that reads the question.
///
/// <para>The property that carries the whole design is that its answer is never trusted as text: a
/// model returning skill ids will eventually return one that does not exist, and the library is the
/// only thing entitled to say a skill is real. Everything else here — cost, layering, the level
/// boundary — follows from that one being safe.</para>
/// </summary>
public class AgentLibrarianTests
{
    /// <summary>Answers whatever it was told to, and counts how often it was asked. The point of
    /// these tests is what surrounds the model, not the model.</summary>
    private sealed class StubGateway(string answer) : ILlmGateway
    {
        public int Calls { get; private set; }
        public string? LastSystemPrompt { get; private set; }
        public string? LastUserMessage { get; private set; }
        public Exception? Throw { get; set; }

        public Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, CancellationToken ct = default)
        {
            Calls++;
            LastSystemPrompt = ctx.Messages.FirstOrDefault(m => m.Role == ChatRole.System)?.Content;
            LastUserMessage = ctx.Messages.LastOrDefault(m => m.Role == ChatRole.User)?.Content;
            if (Throw != null) throw Throw;

            return Task.FromResult(new LlmTurnResult
            {
                Message = new ChatMessage { Role = ChatRole.Assistant, Content = answer }
            });
        }
    }

    private static SkillLibrary Fond() => new([
        new FakeSkillSource("open", level: SourceLevel.InCatalog)
            .With("linux.capture", description: "Captures a Linux host over SSH.", tags: ["ssh"])
            .With("net.dns", description: "Diagnoses DNS failures.", tags: ["dns"]),
        new FakeSkillSource("hidden", level: SourceLevel.OutOfCatalog)
            .With("secret.payroll", description: "Touches payroll.", tags: ["payroll"])
    ]);

    private static ResolvedSettings Settings(bool enabled) => new()
    {
        Mode = AgentMode.Edit,
        Instructions = [],
        CustomPrompt = null,
        Skills = new Dictionary<string, SplaSkillSection>(),
        SkillLibrarian = enabled ? new SplaLibrarianSection { Enabled = true } : null
    };

    private static AgentLibrarian Librarian(StubGateway gateway, bool enabled = true) =>
        new(Fond(), gateway, () => Settings(enabled));

    // ── The answer is a selection, not a source of truth ─────────────────────

    [Fact]
    public async Task Ids_that_do_not_exist_are_dropped()
    {
        var gateway = new StubGateway("linux.capture\nlinux.totally-invented\nnet.dns");

        var matches = await Librarian(gateway).AskAsync("back up a server");

        Assert.Equal(["linux.capture", "net.dns"], matches.Select(m => m.Card.Id));
    }

    [Fact]
    public async Task An_answer_of_pure_invention_yields_nothing()
    {
        var matches = await Librarian(new StubGateway("kubernetes.deploy\nterraform.plan")).AskAsync("q");

        Assert.Empty(matches);
    }

    /// <summary>Models decorate lists no matter what they are told, so the parser tolerates the
    /// shapes they actually reach for rather than pretending the instruction was obeyed.</summary>
    [Theory]
    [InlineData("- linux.capture\n- net.dns")]
    [InlineData("1. linux.capture\n2. net.dns")]
    [InlineData("`linux.capture`\n`net.dns`")]
    [InlineData("linux.capture — captures a host\nnet.dns: diagnoses dns")]
    [InlineData("  linux.capture  \n\n  net.dns  ")]
    public async Task Decorated_answers_still_parse(string answer)
    {
        var matches = await new AgentLibrarian(Fond(), new StubGateway(answer), () => Settings(true))
            .AskAsync("q");

        Assert.Equal(["linux.capture", "net.dns"], matches.Select(m => m.Card.Id));
    }

    [Fact]
    public async Task NONE_means_none()
    {
        Assert.Empty(await Librarian(new StubGateway("NONE")).AskAsync("q"));
    }

    [Fact]
    public async Task Duplicates_are_collapsed_and_order_is_kept()
    {
        var matches = await Librarian(new StubGateway("net.dns\nlinux.capture\nnet.dns")).AskAsync("q");

        Assert.Equal(["net.dns", "linux.capture"], matches.Select(m => m.Card.Id));
        Assert.True(matches[0].Score > matches[1].Score);
    }

    [Fact]
    public async Task The_limit_is_respected()
    {
        var matches = await Librarian(new StubGateway("linux.capture\nnet.dns")).AskAsync("q", limit: 1);

        Assert.Single(matches);
    }

    // ── The level boundary holds here too ────────────────────────────────────

    /// <summary>Out-of-catalog must not become discoverable by asking in words any more than by
    /// asking with tags — including by never appearing in the librarian's own prompt.</summary>
    [Fact]
    public async Task Out_of_catalog_is_neither_offered_to_the_librarian_nor_accepted_from_it()
    {
        var gateway = new StubGateway("secret.payroll");

        var matches = await Librarian(gateway).AskAsync("payroll");

        Assert.Empty(matches);
        Assert.DoesNotContain("secret.payroll", gateway.LastSystemPrompt);
        Assert.DoesNotContain("payroll", gateway.LastSystemPrompt);
    }

    [Fact]
    public async Task The_whole_catalog_goes_into_the_librarians_own_prompt()
    {
        var gateway = new StubGateway("NONE");

        await Librarian(gateway).AskAsync("the question");

        Assert.Contains("linux.capture", gateway.LastSystemPrompt);
        Assert.Contains("Captures a Linux host over SSH.", gateway.LastSystemPrompt);
        Assert.Equal("the question", gateway.LastUserMessage);
    }

    // ── Cost and failure ─────────────────────────────────────────────────────

    [Fact]
    public async Task An_unconfigured_librarian_never_calls_a_model()
    {
        var gateway = new StubGateway("linux.capture");
        var librarian = Librarian(gateway, enabled: false);

        Assert.False(librarian.IsAvailable);
        Assert.Empty(await librarian.AskAsync("q"));
        Assert.Equal(0, gateway.Calls);
    }

    /// <summary>A librarian who is out to lunch degrades the search, not the turn.</summary>
    [Fact]
    public async Task A_failing_model_yields_no_matches_rather_than_an_exception()
    {
        var gateway = new StubGateway("") { Throw = new HttpRequestException("connection refused") };

        Assert.Empty(await Librarian(gateway).AskAsync("q"));
    }

    [Fact]
    public async Task An_empty_question_costs_nothing()
    {
        var gateway = new StubGateway("linux.capture");

        Assert.Empty(await Librarian(gateway).AskAsync("   "));
        Assert.Equal(0, gateway.Calls);
    }

    // ── Layering inside skill_find ───────────────────────────────────────────

    /// <summary>The free pass answers first and the expensive one is never reached. Paying an LLM
    /// call to be told what a dictionary lookup already knew is latency nobody attributes correctly
    /// afterwards.</summary>
    [Fact]
    public async Task A_tag_hit_never_reaches_the_model()
    {
        var library = Fond();
        var gateway = new StubGateway("net.dns");
        var tool = new SkillFindTool(new TagLibrarian(library),
            new AgentLibrarian(library, gateway, () => Settings(true)));

        var result = (await tool.ExecuteAsync("""{"tags":["ssh"]}""")).TextContent;

        Assert.Contains("linux.capture", result);
        Assert.Equal(0, gateway.Calls);
    }

    [Fact]
    public async Task A_miss_falls_through_to_the_librarian_and_says_so()
    {
        var library = Fond();
        var gateway = new StubGateway("linux.capture");
        var tool = new SkillFindTool(new TagLibrarian(library),
            new AgentLibrarian(library, gateway, () => Settings(true)));

        var result = (await tool.ExecuteAsync("""{"text":"the box keeps dropping my connections"}""")).TextContent;

        Assert.Equal(1, gateway.Calls);
        Assert.Contains("linux.capture", result);
        Assert.Contains("matched by meaning", result);
    }

    [Fact]
    public async Task With_no_librarian_configured_a_miss_is_simply_a_miss()
    {
        var library = Fond();
        var gateway = new StubGateway("linux.capture");
        var tool = new SkillFindTool(new TagLibrarian(library),
            new AgentLibrarian(library, gateway, () => Settings(false)));

        var result = (await tool.ExecuteAsync("""{"text":"zzzzz"}""")).TextContent;

        Assert.Equal(0, gateway.Calls);
        Assert.StartsWith("no skills found", result);
    }

    /// <summary>Tags that matched nothing still carry intent, so they reach the librarian rather than
    /// being discarded on the way.</summary>
    [Fact]
    public async Task Unmatched_tags_are_passed_on_as_part_of_the_question()
    {
        var library = Fond();
        var gateway = new StubGateway("NONE");
        var tool = new SkillFindTool(new TagLibrarian(library),
            new AgentLibrarian(library, gateway, () => Settings(true)));

        await tool.ExecuteAsync("""{"tags":["kubernetes"],"text":"deploy the thing"}""");

        Assert.Contains("deploy the thing", gateway.LastUserMessage);
        Assert.Contains("kubernetes", gateway.LastUserMessage);
    }
}
