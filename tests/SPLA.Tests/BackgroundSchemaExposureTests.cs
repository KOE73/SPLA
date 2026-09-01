using SPLA.Domain.Models;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Agent;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Permissions;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The <c>background</c> schema property McpHost adds for a tool that declares
/// <see cref="ToolFunctionDefinition.SupportsBackground"/> — plan step 1.3. Every assertion here
/// guards against a specific way this could go wrong for a real provider: a plain tool paying schema
/// cost for a capability it does not have (ADR §2), a strict tool shipping a schema its own provider
/// would reject (plan pitfall 13), or a foreign head being offered a capability it can never use
/// (plan pitfall 11).
/// </summary>
public class BackgroundSchemaExposureTests
{
    private sealed class FakeTool : IMcpTool
    {
        public string Name { get; init; } = "t";
        public bool SupportsBackground { get; init; }
        public bool StrictSchema { get; init; }
        public object? Parameters { get; init; } = new { type = "object", properties = new { }, required = new string[0] };

        public ToolDefinition GetDefinition() => new()
        {
            Function = new ToolFunctionDefinition
            {
                Name = Name,
                SupportsBackground = SupportsBackground,
                StrictSchema = StrictSchema,
                Parameters = Parameters
            }
        };

        public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken ct = default) =>
            Task.FromResult(ToolResult.Text("ran"));
    }

    private static McpHost HostWith(params IMcpTool[] tools)
    {
        var host = new McpHost(new PermissionManager());
        foreach (var t in tools) host.RegisterTool(t);
        return host;
    }

    private static JsonNode ParametersOf(ToolDefinition def)
        => JsonNode.Parse(JsonSerializer.Serialize(def.Function.Parameters))!;

    [Fact]
    public void A_plain_tool_gets_no_background_property_at_all()
    {
        var host = HostWith(new FakeTool { Name = "plain", SupportsBackground = false });

        var def = host.GetToolDefinitions().Single(d => d.Function.Name == "plain");

        Assert.Null(ParametersOf(def)["properties"]?["background"]);
    }

    [Fact]
    public void A_background_capable_tool_gets_a_nullable_boolean_background_property()
    {
        var host = HostWith(new FakeTool { Name = "bg", SupportsBackground = true });

        var def = host.GetToolDefinitions().Single(d => d.Function.Name == "bg");
        var prop = ParametersOf(def)["properties"]!["background"]!;

        var types = prop["type"]!.AsArray().Select(t => t!.GetValue<string>()).ToList();
        Assert.Contains("boolean", types);
        Assert.Contains("null", types);
    }

    [Fact]
    public void A_strict_schema_tool_lists_background_in_required_the_cwd_pattern()
    {
        var host = HostWith(new FakeTool { Name = "strict_bg", SupportsBackground = true, StrictSchema = true });

        var def = host.GetToolDefinitions().Single(d => d.Function.Name == "strict_bg");
        var required = ParametersOf(def)["required"]!.AsArray().Select(r => r!.GetValue<string>());

        Assert.Contains("background", required);
    }

    [Fact]
    public void A_non_strict_background_capable_tool_does_not_force_required()
    {
        var host = HostWith(new FakeTool { Name = "loose_bg", SupportsBackground = true, StrictSchema = false });

        var def = host.GetToolDefinitions().Single(d => d.Function.Name == "loose_bg");
        var required = ParametersOf(def)["required"]?.AsArray().Select(r => r!.GetValue<string>()) ?? Enumerable.Empty<string>();

        Assert.DoesNotContain("background", required);
    }

    [Fact]
    public void A_tool_with_no_parameters_at_all_still_gets_the_background_property()
    {
        var host = HostWith(new FakeTool { Name = "no_params", SupportsBackground = true, Parameters = null });

        var def = host.GetToolDefinitions().Single(d => d.Function.Name == "no_params");

        Assert.NotNull(ParametersOf(def)["properties"]?["background"]);
    }

    [Fact]
    public void Existing_properties_and_required_entries_survive_the_injection()
    {
        var host = HostWith(new FakeTool
        {
            Name = "has_stuff",
            SupportsBackground = true,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new { command = new { type = "string" } },
                required = new[] { "command" }
            }
        });

        var def = host.GetToolDefinitions().Single(d => d.Function.Name == "has_stuff");
        var node = ParametersOf(def);
        var required = node["required"]!.AsArray().Select(r => r!.GetValue<string>()).ToList();

        Assert.NotNull(node["properties"]!["command"]);
        Assert.Contains("command", required);
        Assert.Contains("background", required);
    }

    [Fact]
    public void A_foreign_head_never_sees_the_background_property_even_for_a_capable_tool()
    {
        // Plan pitfall 11: MCP naruzhu gets no flag — a tool detached from its chat has nowhere to
        // deliver a result, so advertising the capability would be advertising something that
        // degrades to a no-op the instant it is used.
        var host = HostWith(new FakeTool { Name = "bg", SupportsBackground = true });

        var def = host.GetToolDefinitionsFor(ToolExposure.Default).Single(d => d.Function.Name == "bg");

        Assert.Null(ParametersOf(def)["properties"]?["background"]);
    }
}
