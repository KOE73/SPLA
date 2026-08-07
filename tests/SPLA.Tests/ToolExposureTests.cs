using SPLA.Domain.Models;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Agent;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.ToolSets;
using SPLA.Domain.Settings;

namespace SPLA.Tests;

/// <summary>
/// What a caller other than this chat's own head is offered.
/// <para>
/// The rule under test: a conversation-bound tool is never served outward, because outside the
/// conversation it has no referent — a mark set on somebody else's history, or a rollback of it, is
/// not a permission question but a meaningless one. Everything else follows what the project permits.
/// </para>
/// </summary>
public class ToolExposureTests
{
    private sealed class FakeTool : IMcpTool
    {
        public string Name { get; init; } = "t";
        public bool Bound { get; init; }

        public ToolDefinition GetDefinition() => new()
        {
            Function = new ToolFunctionDefinition { Name = Name, ConversationBound = Bound }
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

    private static string[] Served(McpHost host, ToolExposure? exposure = null) =>
        host.GetToolDefinitionsFor(exposure ?? ToolExposure.Default)
            .Select(d => d.Function.Name)
            .OrderBy(n => n)
            .ToArray();

    [Fact]
    public void A_conversation_bound_tool_is_never_served_outward()
    {
        var host = HostWith(
            new FakeTool { Name = "sql_query" },
            new FakeTool { Name = "mark_set", Bound = true });

        Assert.Equal(["sql_query"], Served(host));
    }

    /// <summary>
    /// Not a policy that a narrower exposure could relax: naming the tool explicitly still does not
    /// produce it. That is the difference between "not offered to you" and "meaningless to you".
    /// </summary>
    [Fact]
    public void Naming_a_conversation_bound_tool_explicitly_does_not_produce_it()
    {
        var host = HostWith(new FakeTool { Name = "mark_set", Bound = true });

        var exposure = new ToolExposure { OnlyTools = new HashSet<string> { "mark_set" } };

        Assert.Empty(Served(host, exposure));
    }

    [Fact]
    public void Narrowing_hands_over_the_smaller_table_a_skill_asked_for()
    {
        var host = HostWith(
            new FakeTool { Name = "sql_query" },
            new FakeTool { Name = "ssh_run" },
            new FakeTool { Name = "network_ping_host" });

        var exposure = new ToolExposure { OnlyTools = new HashSet<string> { "sql_query", "ssh_run" } };

        Assert.Equal(["sql_query", "ssh_run"], Served(host, exposure));
    }

    /// <summary>
    /// A set the owner switched off does not exist — for anyone. Exposure narrows what the project
    /// permits; it can never widen it.
    /// </summary>
    [Fact]
    public void A_tool_in_a_disabled_set_is_not_served_outward_either()
    {
        var settings = new ResolvedSettings();
        settings.ToolSets["ssh"] = ToolSetRegistry.Format(ToolSetLevel.Disabled);

        var tool = new FakeTool { Name = "ssh_run" };
        var sets = new ToolSetRegistry(settings, features: [new AgentFeature("ssh", tools: [tool])]);

        var host = new McpHost(new PermissionManager(settings: settings)) { ToolSets = sets };
        host.RegisterTool(tool);

        Assert.Empty(Served(host));
        // And naming it does not bring it back: the narrowing is applied on top, never instead.
        Assert.Empty(Served(host, new ToolExposure { OnlyTools = new HashSet<string> { "ssh_run" } }));
    }

    /// <summary>
    /// Exposure asks "whose call is this", disclosure asks "how much context does this set cost in a
    /// chat". A set that is merely not raised right now is still perfectly real to a foreign caller,
    /// which has no chat to raise it in.
    /// </summary>
    [Fact]
    public void A_set_that_is_simply_not_raised_is_still_served_outward()
    {
        var settings = new ResolvedSettings();
        settings.ToolSets["ssh"] = ToolSetRegistry.Format(ToolSetLevel.AgentDemand);

        var tool = new FakeTool { Name = "ssh_run" };
        var sets = new ToolSetRegistry(settings, features: [new AgentFeature("ssh", tools: [tool])]);

        var host = new McpHost(new PermissionManager(settings: settings)) { ToolSets = sets };
        host.RegisterTool(tool);

        // Not disclosed to the model in a chat until raised…
        Assert.DoesNotContain("ssh_run", host.GetToolDefinitions().Select(d => d.Function.Name));
        // …but permitted by the project, so a foreign caller is offered it.
        Assert.Equal(["ssh_run"], Served(host));
    }
}
