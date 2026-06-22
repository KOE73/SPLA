using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.Plugins.Roslyn;

namespace SPLA.Tests;

public class ScriptRunToolTests
{
    private sealed class FakeToolHost : IToolHost
    {
        public int Calls;
        private readonly Func<string, string, string> _handler;

        public FakeToolHost(Func<string, string, string> handler) => _handler = handler;

        public IEnumerable<ToolDefinition> GetToolDefinitions() => Array.Empty<ToolDefinition>();

        public Task<string> ExecuteToolAsync(AgentMode mode, string name, string argumentsJson, CancellationToken ct = default)
        {
            Interlocked.Increment(ref Calls);
            return Task.FromResult(_handler(name, argumentsJson));
        }
    }

    private static Task<string> Run(IToolHost host, string code, int? timeout = null)
    {
        var tool = new ScriptRunTool();
        var args = timeout is null
            ? JsonSerializer.Serialize(new { code })
            : JsonSerializer.Serialize(new { code, timeout_seconds = timeout.Value });
        using (ToolHostScope.Begin(host, AgentMode.Agent))
            return tool.ExecuteAsync(args);
    }

    [Fact]
    public async Task Script_invokes_tool_via_ctx_run_and_captures_output()
    {
        var host = new FakeToolHost((name, json) => $"called {name} with {json}");
        var result = await Run(host, """
            var r = await ctx.Run("network_ping_host", new { host = "8.8.8.8" });
            ctx.Log(r);
            """);

        Assert.StartsWith("ok: true", result);
        Assert.Equal(1, host.Calls);
        Assert.Contains("called network_ping_host", result);
        Assert.Contains("\"host\":\"8.8.8.8\"", result); // args serialized verbatim
    }

    [Fact]
    public async Task Parallel_runs_invoke_the_tool_for_each_item()
    {
        var host = new FakeToolHost((name, json) => "ok");
        var result = await Run(host, """
            var items = Enumerable.Range(0, 25).ToArray();
            var tasks = items.Select(i => ctx.Run("work", new { i }));
            var results = await Task.WhenAll(tasks);
            ctx.Log($"done {results.Length}");
            """);

        Assert.StartsWith("ok: true", result);
        Assert.Equal(25, host.Calls);
        Assert.Contains("done 25", result);
    }

    [Fact]
    public async Task Return_value_is_surfaced()
    {
        var host = new FakeToolHost((n, j) => "x");
        var result = await Run(host, "21 * 2");
        Assert.StartsWith("ok: true", result);
        Assert.Contains("return: 42", result);
    }

    [Fact]
    public async Task Console_writeline_is_captured()
    {
        var host = new FakeToolHost((n, j) => "x");
        var result = await Run(host, "Console.WriteLine(\"hello from script\");");
        Assert.Contains("hello from script", result);
    }

    [Fact]
    public async Task Compile_error_reports_not_ok_with_diagnostics()
    {
        var host = new FakeToolHost((n, j) => "x");
        var result = await Run(host, "this is not valid c#;");
        Assert.StartsWith("ok: false", result);
        Assert.Contains("CS", result);
    }

    [Fact]
    public async Task Runtime_exception_is_caught_not_thrown()
    {
        var host = new FakeToolHost((n, j) => "x");
        var result = await Run(host, "throw new System.InvalidOperationException(\"boom\");");
        Assert.StartsWith("ok: false", result);
        Assert.Contains("boom", result);
        Assert.Contains("InvalidOperationException", result);
    }

    [Fact]
    public async Task Timeout_cancels_awaits_that_honor_the_token()
    {
        var host = new FakeToolHost((n, j) => "x");
        var result = await Run(host, "await Task.Delay(60000, ctx.Cancellation);", timeout: 1);
        Assert.StartsWith("ok: false", result);
        Assert.Contains("timed out", result);
    }

    [Fact]
    public async Task Without_host_scope_returns_error()
    {
        var tool = new ScriptRunTool();
        var result = await tool.ExecuteAsync(JsonSerializer.Serialize(new { code = "1+1" }));
        Assert.StartsWith("Error:", result);
        Assert.Contains("No tool host", result);
    }
}
