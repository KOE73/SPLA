using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Models;
using SPLA.Domain.Secrets;
using SPLA.Domain.Settings;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.ToolSets;
using SPLA.Mcp.Client;
using SPLA.Runtime;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// PLAN_20260826_service_mcp-client step 5: wiring a configured server's connection into
/// <see cref="McpHost"/>/<see cref="ToolSetRegistry"/> for real, through <see cref="McpClientManager"/>.
///
/// <para>Drives the real manager end-to-end via <see cref="McpClientManager.ConnectAllAsync"/>. The
/// only concession to testability is the optional session-factory constructor parameter, used here
/// to swap each server's transport for <see cref="McpTestDuplex"/>'s in-memory line pipes — the same
/// rig steps 2/3 already use — instead of a real process or socket. Everything downstream of that
/// (naming, collision checks, registration, tool-set bookkeeping, event publication) is the manager's
/// own production code, unmodified.</para>
/// </summary>
public sealed class McpClientManagerTests
{
    /// <summary>Echoes whatever it is given back as the resolved value — the plumbing under test here
    /// is registration, not secret resolution, which is <see cref="ISecretResolver"/>'s own contract.</summary>
    private sealed class EchoSecretResolver : ISecretResolver
    {
        public ValueTask<string?> ResolveAsync(string? reference, CancellationToken ct = default) =>
            ValueTask.FromResult(reference);
        public string? Resolve(string? reference) => reference;
        public ValueTask<SecretEntry?> GetEntryAsync(string key, SecretScope scope, CancellationToken ct = default) =>
            ValueTask.FromResult<SecretEntry?>(null);
    }

    private sealed class StubTool(string name) : IMcpTool
    {
        public string Name { get; } = name;
        public ToolDefinition GetDefinition() => new()
        {
            Function = new ToolFunctionDefinition { Name = Name, Description = "core tool" }
        };
        public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken ct = default) =>
            Task.FromResult(ToolResult.Text("ran"));
    }

    private static SplaMcpServerSection StdioServer(string id, bool? enabled = null, string? level = null) => new()
    {
        Id = id,
        Transport = "stdio",
        // A real command is unreachable in this sandbox and unnecessary: the session factory below
        // replaces the transport with McpTestDuplex's in-memory pipes before anything is spawned, so
        // this only has to be non-empty to pass McpClientManager's "stdio needs a command" precheck.
        Command = "unused-transport-is-swapped-for-the-duplex",
        Enabled = enabled,
        Level = level
    };

    /// <summary>One server's harness: its section, the duplex pipes standing in for its transport, and
    /// a scripted server to answer the handshake and any later <c>tools/list</c> re-read.</summary>
    private sealed record ServerRig(
        SplaMcpServerSection Section,
        McpTestDuplex.LinePipe ToServer,
        McpTestDuplex.LinePipe ToClient,
        McpTestDuplex.ScriptedServer Scripted);

    private static ServerRig Rig(string id, bool? enabled = null, string? level = null)
    {
        var toServer = new McpTestDuplex.LinePipe();
        var toClient = new McpTestDuplex.LinePipe();
        return new ServerRig(StdioServer(id, enabled, level), toServer, toClient, new McpTestDuplex.ScriptedServer(toServer, toClient));
    }

    /// <summary>Wires a manager whose session factory routes each configured server id to its rig's
    /// duplex pipes — a misconfigured server that never reaches session construction (bad id, missing
    /// command, wrong transport) simply has no rig and is never looked up.</summary>
    private static (McpClientManager Manager, McpHost Host, ToolSetRegistry Sets, ResolvedSettings Settings) Build(
        params ServerRig[] rigs)
    {
        var settings = new ResolvedSettings { SecretResolver = new EchoSecretResolver() };
        settings.McpServers.AddRange(rigs.Select(r => r.Section));

        var host = new McpHost(new PermissionManager());
        var sets = new ToolSetRegistry(settings);
        host.ToolSets = sets;

        var byId = rigs.ToDictionary(r => r.Section.Id!, StringComparer.OrdinalIgnoreCase);
        McpServerSession Factory(McpServerSpec spec)
        {
            var rig = byId[spec.Id];
            return new McpServerSession(spec, s => new StreamTransport(rig.ToClient.Reader, rig.ToServer.Writer, s.Id));
        }

        var manager = new McpClientManager(
            settings, host, sets, new ServiceEvents(), NullLogger<McpClientManager>.Instance, Factory);

        return (manager, host, sets, settings);
    }

    private static JsonArray OneTool(string name, string description = "does a thing") => new(new JsonObject
    {
        ["name"] = name,
        ["description"] = description,
        ["inputSchema"] = new JsonObject { ["type"] = "object" }
    });

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException("condition never became true");
            await Task.Delay(10);
        }
    }

    [Fact]
    public async Task Connecting_a_server_registers_its_tools_under_the_prefixed_name_and_adds_a_findable_set()
    {
        var rig = Rig("gh");
        var (manager, host, sets, _) = Build(rig);

        var connect = manager.ConnectAllAsync();
        await rig.Scripted.HandshakeAsync(OneTool("create_issue"));
        await connect;

        Assert.Contains(host.GetToolDefinitions(), d => d.Function.Name == "gh_create_issue");
        Assert.Equal("gh", sets.SetOfTool("gh_create_issue"));
        Assert.NotNull(sets.Find("gh"));
    }

    [Fact]
    public async Task A_tool_name_colliding_with_an_already_registered_tool_is_refused_while_the_rest_of_the_server_still_registers()
    {
        var rig = Rig("gh");
        var (manager, host, sets, _) = Build(rig);
        // A core tool already holds the name a foreign tool would otherwise get prefixed to.
        host.RegisterTool(new StubTool("gh_create_issue"));

        var connect = manager.ConnectAllAsync();
        await rig.Scripted.HandshakeAsync(new JsonArray(
            new JsonObject { ["name"] = "create_issue", ["description"] = "foreign", ["inputSchema"] = new JsonObject { ["type"] = "object" } },
            new JsonObject { ["name"] = "list_issues", ["description"] = "foreign", ["inputSchema"] = new JsonObject { ["type"] = "object" } }));
        await connect;

        // The pre-existing owner of the name is untouched (still exactly one holder of it, and it is
        // the core tool, not the foreign one silently overwriting it) — and the sibling tool that did
        // not collide still made it in.
        var holders = host.GetToolDefinitions().Where(d => d.Function.Name == "gh_create_issue").ToList();
        Assert.Single(holders);
        Assert.Equal("core tool", holders[0].Function.Description);
        Assert.Contains(host.GetToolDefinitions(), d => d.Function.Name == "gh_list_issues");
    }

    [Fact]
    public async Task A_tool_dropped_from_a_later_tools_list_is_unregistered()
    {
        var rig = Rig("gh");
        var (manager, host, sets, _) = Build(rig);

        var connect = manager.ConnectAllAsync();
        await rig.Scripted.HandshakeAsync(new JsonArray(
            new JsonObject { ["name"] = "create_issue", ["inputSchema"] = new JsonObject { ["type"] = "object" } },
            new JsonObject { ["name"] = "list_issues", ["inputSchema"] = new JsonObject { ["type"] = "object" } }));
        await connect;

        Assert.Contains(host.GetToolDefinitions(), d => d.Function.Name == "gh_list_issues");

        // Server says its list changed (would normally be a notification; driven directly here by
        // asking the session to re-read, since McpTestDuplex's ScriptedServer answers requests, not
        // notifications). Reply with one tool short.
        var refresh = manager.Servers.Single(s => s.Id == "gh"); // sanity: server is tracked
        Assert.Equal(2, refresh.ToolCount);

        var session = GetSessionField(manager, "gh");
        var reread = session.RefreshToolsAsync();
        var request = await rig.Scripted.ReadAsync();
        rig.Scripted.Reply(request, new JsonObject
        {
            ["tools"] = new JsonArray(
                new JsonObject { ["name"] = "create_issue", ["inputSchema"] = new JsonObject { ["type"] = "object" } })
        });
        await reread;

        Assert.Contains(host.GetToolDefinitions(), d => d.Function.Name == "gh_create_issue");
        Assert.DoesNotContain(host.GetToolDefinitions(), d => d.Function.Name == "gh_list_issues");
    }

    [Fact]
    public async Task Leaving_Ready_removes_the_servers_tools_from_the_host_and_its_set_from_the_registry()
    {
        var rig = Rig("gh");
        var (manager, host, sets, _) = Build(rig);

        var connect = manager.ConnectAllAsync();
        await rig.Scripted.HandshakeAsync(OneTool("create_issue"));
        await connect;

        Assert.Contains(host.GetToolDefinitions(), d => d.Function.Name == "gh_create_issue");
        Assert.NotNull(sets.Find("gh"));

        // Hang up the server side — the client's transport observes the pipe close and the session
        // moves out of Ready (Disconnected, then it starts trying to reconnect on its own, which is
        // fine: the assertions below only care about the moment it left Ready).
        rig.ToClient.Close();
        var session = GetSessionField(manager, "gh");
        await WaitUntilAsync(() => session.State != McpSessionState.Ready);

        Assert.DoesNotContain(host.GetToolDefinitions(), d => d.Function.Name == "gh_create_issue");
        Assert.Null(sets.Find("gh"));
    }

    [Fact]
    public async Task A_disabled_server_is_never_connected_and_nothing_shows_up()
    {
        var disabledRig = Rig("gh", enabled: false);
        var (manager, host, sets, _) = Build(disabledRig);

        await manager.ConnectAllAsync();

        Assert.Empty(manager.Servers);
        Assert.Empty(host.GetToolDefinitions());
        Assert.Null(sets.Find("gh"));
    }

    [Fact]
    public async Task An_explicit_toolsets_entry_set_before_connecting_wins_over_a_conflicting_level_on_the_section()
    {
        var rig = Rig("gh", level: "enabled");
        var (manager, host, sets, settings) = Build(rig);
        // Set BEFORE ConnectAllAsync ever runs — this is the precedence under test: an explicit entry
        // is a ceiling the `level:` convenience field must not lift, even though the section says the
        // opposite.
        settings.ToolSets["gh"] = ToolSetRegistry.Format(ToolSetLevel.Disabled);

        var connect = manager.ConnectAllAsync();
        await rig.Scripted.HandshakeAsync(OneTool("create_issue"));
        await connect;

        Assert.Equal(ToolSetLevel.Disabled, sets.LevelOf("gh"));
    }

    [Fact]
    public async Task The_level_convenience_field_seeds_ToolSets_when_nothing_was_configured()
    {
        var rig = Rig("gh", level: "disabled");
        var (manager, host, sets, settings) = Build(rig);
        Assert.False(settings.ToolSets.ContainsKey("gh"));

        // The seeding happens before the connect attempt, regardless of whether the handshake ever
        // completes — answer it anyway so the call returns promptly.
        var connect = manager.ConnectAllAsync();
        await rig.Scripted.HandshakeAsync(OneTool("create_issue"));
        await connect;

        Assert.Equal(ToolSetLevel.Disabled, sets.LevelOf("gh"));
    }

    [Fact]
    public async Task A_misconfigured_stdio_server_without_a_command_does_not_stop_a_well_configured_second_server()
    {
        var bad = new SplaMcpServerSection { Id = "broken", Transport = "stdio" }; // no Command
        var good = Rig("gh");

        var settings = new ResolvedSettings { SecretResolver = new EchoSecretResolver() };
        settings.McpServers.Add(bad);
        settings.McpServers.Add(good.Section);

        var host = new McpHost(new PermissionManager());
        var sets = new ToolSetRegistry(settings);
        host.ToolSets = sets;

        McpServerSession Factory(McpServerSpec spec) =>
            new(spec, s => new StreamTransport(good.ToClient.Reader, good.ToServer.Writer, s.Id));

        var manager = new McpClientManager(
            settings, host, sets, new ServiceEvents(), NullLogger<McpClientManager>.Instance, Factory);

        var connect = manager.ConnectAllAsync();
        await good.Scripted.HandshakeAsync(OneTool("create_issue"));
        var exception = await Record.ExceptionAsync(() => connect);

        Assert.Null(exception);
        // "broken" never became a tracked server (rejected before a session was ever built); "gh"
        // connected fine, proving the bad entry did not take the whole batch down with it.
        Assert.DoesNotContain(manager.Servers, s => s.Id == "broken");
        Assert.Contains(host.GetToolDefinitions(), d => d.Function.Name == "gh_create_issue");
    }

    /// <summary>Reaches the private per-server <c>McpServerSession</c> the manager built, for the two
    /// tests that need to drive a second round trip (list-changed) or force a disconnect on it
    /// directly. <see cref="McpClientManager"/> deliberately does not expose sessions publicly — status
    /// is the only public surface (per the task's design) — so this is reflection over the private
    /// <c>_servers</c> dictionary rather than a widened production API added just for a test.</summary>
    private static McpServerSession GetSessionField(McpClientManager manager, string serverId)
    {
        var serversField = typeof(McpClientManager).GetField("_servers",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var servers = (System.Collections.IDictionary)serversField.GetValue(manager)!;
        var entry = servers[serverId]!;
        var sessionProperty = entry.GetType().GetProperty("Session")!;
        return (McpServerSession)sessionProperty.GetValue(entry)!;
    }
}
