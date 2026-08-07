using SPLA.Domain.Models;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Permissions;

namespace SPLA.Tests;

/// <summary>
/// The outcome of a call, which is the part of <see cref="ToolResult"/> that carries a claim rather
/// than content.
/// <para>
/// The rule under test throughout: <b>the outcome describes the tool's own work, not the verdict on
/// what it examined or ran.</b> Getting it backwards costs more than tidiness — it turns the error
/// rate into a measure of the user's code and network instead of the tools, which is the same lie
/// the old string result told when it hid failures inside prose.
/// </para>
/// </summary>
public class ToolOutcomeTests
{
    // ── A tool whose behaviour the test dictates ────────────────────────────

    private sealed class StubTool : IMcpTool
    {
        public string Name { get; init; } = "stub";
        public ToolScope Scope { get; init; } = ToolScope.Project;
        public ToolEffect Effect { get; init; } = ToolEffect.Read;
        public Func<ToolResult>? Behaviour { get; init; }

        public ToolDefinition GetDefinition() => new()
        {
            Function = new ToolFunctionDefinition { Name = Name, Scope = Scope, Effect = Effect }
        };

        public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken ct = default) =>
            Task.FromResult(Behaviour?.Invoke() ?? ToolResult.Text("ok"));
    }

    private sealed class ThrowingTool : IMcpTool
    {
        public string Name => "thrower";

        public ToolDefinition GetDefinition() => new()
        {
            Function = new ToolFunctionDefinition { Name = Name, Scope = ToolScope.Project }
        };

        public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken ct = default) =>
            throw new InvalidOperationException("boom");
    }

    private static McpHost HostWith(IMcpTool tool)
    {
        var host = new McpHost(new PermissionManager());
        host.RegisterTool(tool);
        return host;
    }

    // ── The three outcomes, at the boundary that produces them ──────────────

    [Fact]
    public async Task A_tool_that_answers_is_Ok()
    {
        var host = HostWith(new StubTool { Behaviour = () => ToolResult.Text("42 rows") });

        var result = await host.ExecuteToolAsync(AgentMode.Agent, "stub", "{}");

        Assert.Equal(ToolOutcome.Ok, result.Outcome);
        Assert.False(result.IsError);
        Assert.Null(result.Reason);
    }

    /// <summary>
    /// The distinction the whole type exists for. Before the outcome was separate, this was a
    /// successful call whose text happened to begin with the word "error" — indistinguishable, to
    /// everything but a human reader, from a tool that answered.
    /// </summary>
    [Fact]
    public async Task A_tool_that_reports_failure_by_returning_is_Failed()
    {
        var host = HostWith(new StubTool { Behaviour = () => ToolResult.Fail("error: bad path", "bad path") });

        var result = await host.ExecuteToolAsync(AgentMode.Agent, "stub", "{}");

        Assert.Equal(ToolOutcome.Failed, result.Outcome);
        Assert.True(result.IsError);
        Assert.Equal("bad path", result.Reason);
    }

    /// <summary>A throw and a returned failure are the same fact told two ways, and the boundary
    /// must not tell them apart to the caller.</summary>
    [Fact]
    public async Task A_tool_that_throws_is_Failed_too()
    {
        var host = HostWith(new ThrowingTool());

        var result = await host.ExecuteToolAsync(AgentMode.Agent, "thrower", "{}");

        Assert.Equal(ToolOutcome.Failed, result.Outcome);
        Assert.Contains("boom", result.TextContent);
    }

    /// <summary>
    /// Refused, not Failed: nothing was attempted and nothing changed. Chat mode permits no tool
    /// calls at all, so the call never reaches the tool.
    /// </summary>
    [Fact]
    public async Task A_call_stopped_by_policy_is_Refused()
    {
        var reached = false;
        var host = HostWith(new StubTool
        {
            Behaviour = () => { reached = true; return ToolResult.Text("should not run"); }
        });

        var result = await host.ExecuteToolAsync(AgentMode.Chat, "stub", "{}");

        Assert.Equal(ToolOutcome.Refused, result.Outcome);
        Assert.False(reached);
        Assert.False(string.IsNullOrWhiteSpace(result.Reason));
    }

    /// <summary>
    /// A run with nobody to ask must not die on a tool that needs approval. It used to: the branch
    /// returned "no permission handler is attached", which describes the host's wiring — something a
    /// model can neither act on nor repair.
    /// <para>What it can act on is the situation, so the refusal has to carry three things: why
    /// approval was needed, that nobody is there to give it, and that repeating the call is futile.
    /// The last one matters most — without it a model retries until the loop guard stops it.</para>
    /// </summary>
    [Fact]
    public async Task A_call_needing_approval_with_nobody_to_ask_is_refused_in_terms_the_model_can_act_on()
    {
        var reached = false;
        var host = HostWith(new StubTool
        {
            // Edit mode asks before a project write, and no permission handler is attached here.
            Scope = ToolScope.Project,
            Effect = ToolEffect.Write,
            Behaviour = () => { reached = true; return ToolResult.Text("should not run"); }
        });

        var result = await host.ExecuteToolAsync(AgentMode.Edit, "stub", "{}");

        Assert.Equal(ToolOutcome.Refused, result.Outcome);
        Assert.False(reached);

        var text = result.TextContent;
        Assert.Contains("nobody to ask", text);
        Assert.Contains("will not help", text);           // retrying is futile, and says so
        Assert.Contains("requires confirmation", text);   // the verdict's reason, not the host's wiring
        Assert.DoesNotContain("permission handler", text);
    }

    [Fact]
    public async Task An_unknown_tool_is_Failed_not_a_silent_empty_answer()
    {
        var host = new McpHost(new PermissionManager());

        var result = await host.ExecuteToolAsync(AgentMode.Agent, "no_such_tool", "{}");

        Assert.Equal(ToolOutcome.Failed, result.Outcome);
        Assert.Equal("tool not found", result.Reason);
    }

    // ── The rule that is easiest to get backwards ───────────────────────────

    /// <summary>
    /// A measurement with a negative answer is a successful measurement. A ping that gets no reply,
    /// a port that is closed, a search with no matches, a build that reports compiler errors, a
    /// command exiting non-zero — the tool was asked to find out, and it found out.
    /// </summary>
    [Theory]
    [InlineData("Status: Failed (TimedOut)")]
    [InlineData("Status: Closed or Timeout")]
    [InlineData("(no rows)")]
    [InlineData("exit: 1\nbuild failed with 3 errors")]
    public async Task A_negative_measurement_is_still_Ok(string text)
    {
        var host = HostWith(new StubTool { Behaviour = () => ToolResult.Text(text) });

        var result = await host.ExecuteToolAsync(AgentMode.Agent, "stub", "{}");

        Assert.Equal(ToolOutcome.Ok, result.Outcome);
        Assert.False(result.IsError);
    }

    // ── Content is a list, not a blob ───────────────────────────────────────

    [Fact]
    public void Text_content_joins_the_text_blocks_and_ignores_the_rest()
    {
        var result = ToolResult.From(
            new ToolText("first"),
            new ToolImage("QUJD", "image/png"),
            new ToolText("second"));

        Assert.Equal("first\n\nsecond", result.TextContent);
        Assert.Single(result.Content.OfType<ToolImage>());
    }

    /// <summary>Logging or interpolating a result shows what it says, not the name of its type —
    /// scripts the model writes reach for <c>ctx.Log(result)</c> as the obvious gesture.</summary>
    [Fact]
    public void ToString_gives_the_text()
    {
        Assert.Equal("hello", ToolResult.Text("hello").ToString());
        Assert.Equal($"{ToolResult.Text("hello")}", "hello");
    }
}
