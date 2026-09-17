using SPLA.Domain.Models;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.Pipeline.Stages;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// String-typed arguments read as the schema's type. The cases come from a live Qwen 3.8 Flash run
/// through OpenRouter, where every non-string value of every call arrived quoted.
/// </summary>
public sealed class ArgumentCoercionStageTests
{
    private static readonly object Schema = new
    {
        type = "object",
        properties = new
        {
            name = new { type = "string" },
            cx = new { type = new[] { "number", "null" } },
            round = new { type = "integer" },
            delete = new { type = new[] { "boolean", "null" } },
            inside = new { type = "array", items = new { type = "integer" } },
            options = new { type = "object" },
            label = new { type = new[] { "string", "number" } },
            free = new { description = "no type declared" },
        },
    };

    private static JsonObject Coerced(string argumentsJson) =>
        JsonNode.Parse(ArgumentCoercionStage.Coerce(argumentsJson, Schema).ArgumentsJson)!.AsObject();

    [Fact]
    public void Qwen_call_as_logged_becomes_typed()
    {
        var arguments = Coerced("""{"name":"mark","cx":"290","round":"1","delete":"False","inside":"[1, 20, 43]","options":"{\"a\":1}"}""");

        Assert.Equal(JsonValueKind.Number, arguments["cx"]!.GetValueKind());
        Assert.Equal(290, arguments["cx"]!.GetValue<double>());
        Assert.Equal(1, arguments["round"]!.GetValue<long>());
        Assert.False(arguments["delete"]!.GetValue<bool>());
        Assert.Equal("[1,20,43]", arguments["inside"]!.ToJsonString());
        Assert.Equal(JsonValueKind.Object, arguments["options"]!.GetValueKind());
    }

    [Fact]
    public void Field_that_admits_a_string_is_left_alone()
    {
        var arguments = Coerced("""{"name":"5","label":"7"}""");

        Assert.Equal("5", arguments["name"]!.GetValue<string>());
        Assert.Equal("7", arguments["label"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("""{"cx":"abc"}""")]
    [InlineData("""{"round":"1.5"}""")]
    [InlineData("""{"delete":"yes"}""")]
    [InlineData("""{"inside":"{\"a\":1}"}""")]
    [InlineData("""{"cx":""}""")]
    [InlineData("""{"free":"12"}""")]
    [InlineData("""{"unknown":"12"}""")]
    [InlineData("""{"cx":290,"inside":[1,2]}""")]
    [InlineData("not json")]
    public void Nothing_unambiguous_means_nothing_changes(string argumentsJson)
    {
        var (coercedJson, coercedFields) = ArgumentCoercionStage.Coerce(argumentsJson, Schema);

        Assert.Same(argumentsJson, coercedJson);
        Assert.Empty(coercedFields);
    }

    [Fact]
    public void Schema_given_as_json_object_is_read_too()
    {
        // Foreign MCP servers hand their input schema over as a JsonObject, not an anonymous object.
        var schema = JsonNode.Parse("""{"type":"object","properties":{"limit":{"type":"integer"}}}""");

        var (coercedJson, coercedFields) = ArgumentCoercionStage.Coerce("""{"limit":"15"}""", schema);

        Assert.Equal("""{"limit":15}""", coercedJson);
        Assert.Equal(["limit"], coercedFields);
    }

    [Fact]
    public async Task Tool_receives_coerced_arguments_through_the_host()
    {
        var tool = new RecordingTool();
        var host = new McpHost(new PermissionManager());
        host.RegisterTool(tool);

        var result = await host.ExecuteToolAsync(AgentMode.Agent, tool.Name, """{"name":"mark","cx":"290"}""", CancellationToken.None);

        Assert.Equal(ToolOutcome.Ok, result.Outcome);
        Assert.Equal("""{"name":"mark","cx":290}""", tool.ReceivedArguments);
    }

    private sealed class RecordingTool : IMcpTool
    {
        public string Name => "test_record_arguments";

        public string? ReceivedArguments { get; private set; }

        public ToolDefinition GetDefinition() => new()
        {
            Function = new ToolFunctionDefinition { Name = Name, Description = "records its arguments", Parameters = Schema },
        };

        public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
        {
            ReceivedArguments = argumentsJson;
            return Task.FromResult(ToolResult.Text("ok"));
        }
    }
}
