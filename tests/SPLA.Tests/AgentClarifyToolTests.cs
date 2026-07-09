using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Tools;
using System.Threading.Tasks;

namespace SPLA.Tests;

public class AgentClarifyToolTests
{
    private static AgentClarifyTool Tool() => new();

    [Fact]
    public async Task No_scope_returns_no_handler_message()
    {
        var result = await Tool().ExecuteAsync("""
            {"question":"Go?","options":[{"label":"Yes"},{"label":"No"}]}
            """);
        Assert.StartsWith("clarify: no_handler", result);
    }

    [Fact]
    public async Task Handler_receives_request_and_returns_chosen()
    {
        using var _ = ClarifyScope.Begin(req => Task.FromResult<string?>(req.Options[1].Label));

        var result = await Tool().ExecuteAsync("""
            {"question":"Pick one","options":[{"label":"A"},{"label":"B"}]}
            """);

        Assert.Equal("chosen: B", result);
    }

    [Fact]
    public async Task Missing_question_returns_error()
    {
        var result = await Tool().ExecuteAsync("""{"options":[{"label":"Yes"}]}""");
        Assert.StartsWith("error: 'question'", result);
    }

    [Fact]
    public async Task Missing_options_returns_error()
    {
        var result = await Tool().ExecuteAsync("""{"question":"Go?"}""");
        Assert.StartsWith("error: 'options'", result);
    }

    [Fact]
    public async Task Empty_options_returns_error()
    {
        var result = await Tool().ExecuteAsync("""{"question":"Go?","options":[]}""");
        Assert.StartsWith("error: at least one option", result);
    }

    [Fact]
    public async Task Invalid_json_returns_error()
    {
        var result = await Tool().ExecuteAsync("not-json");
        Assert.StartsWith("error: invalid_json", result);
    }

    [Fact]
    public async Task Handler_sees_question_and_all_options()
    {
        ClarifyRequest? captured = null;
        using var _ = ClarifyScope.Begin(req =>
        {
            captured = req;
            return Task.FromResult<string?>(req.Options[0].Label);
        });

        await Tool().ExecuteAsync("""
            {
              "question": "Which target?",
              "options": [
                {"label":"10.0.0.1","description":"Router"},
                {"label":"10.0.0.2"}
              ]
            }
            """);

        Assert.NotNull(captured);
        Assert.Equal("Which target?", captured!.Question);
        Assert.Equal(2, captured.Options.Count);
        Assert.Equal("Router", captured.Options[0].Description);
        Assert.Null(captured.Options[1].Description);
    }

    [Fact]
    public async Task Nested_scope_restores_outer_on_dispose()
    {
        var outerCalled = false;
        using var outer = ClarifyScope.Begin(_ => { outerCalled = true; return Task.FromResult<string?>("outer"); });

        using (ClarifyScope.Begin(_ => Task.FromResult<string?>("inner")))
        {
            var inner = await ClarifyScope.AskAsync(new ClarifyRequest
            {
                Question = "q",
                Options = [new ClarifyOption { Label = "x" }]
            });
            Assert.Equal("inner", inner);
            Assert.False(outerCalled);
        }

        // after inner scope disposed, outer is restored
        var afterRestore = await ClarifyScope.AskAsync(new ClarifyRequest
        {
            Question = "q",
            Options = [new ClarifyOption { Label = "x" }]
        });
        Assert.Equal("outer", afterRestore);
        Assert.True(outerCalled);
    }
}
