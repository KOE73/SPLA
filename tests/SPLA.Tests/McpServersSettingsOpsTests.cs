using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using SPLA.Service;
using SPLA.Service.Contracts;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// PLAN_20260826_service_mcp-client step 6: the settings-panel wire surface for connected foreign MCP
/// servers — <see cref="SettingsOps.GetMcpServers"/>/<see cref="SettingsOps.SaveMcpServers"/> — and the
/// landmine <see cref="SettingsOps.SaveMcp"/> had before this step: it used to overwrite <c>project.Mcp</c>
/// wholesale from only the outward Enabled/Port fields, silently deleting every configured server the
/// moment someone saved the outward MCP-over-HTTP settings with both at their default.
/// </summary>
public sealed class McpServersSettingsOpsTests
{
    private static AgentRuntime BuildRuntime(string? mcpYaml = null)
    {
        var root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-mcp-servers-ops-{Guid.NewGuid():N}")).FullName;
        var manifest = Path.Combine(root, "test.spla");
        File.WriteAllText(manifest, $"""
            version: 1
            name: McpServersOpsTest
            workspace: .
            {mcpYaml ?? ""}
            """);

        return new AgentRuntime(ConfigLoader.LoadAndResolve(manifest), NullLoggerFactory.Instance);
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException("condition never became true");
            await Task.Delay(20);
        }
    }

    // ── Merge: configured entries + live McpClientManager.Servers status, by id ──────────────────

    [Fact]
    public async Task GetMcpServers_merges_a_configured_server_with_its_live_connect_status()
    {
        // A stdio command that cannot possibly exist: the background connect (kicked off by
        // AgentRuntime's constructor) fails fast and leaves the session tracked as Failed — enough to
        // prove the merge reads real status off McpClientManager.Servers, without needing a server
        // that actually answers a handshake.
        using var runtime = BuildRuntime("""
            mcp:
              servers:
                - id: ghmcp
                  transport: stdio
                  command: definitely-not-a-real-executable-xyz
            """);

        await WaitUntilAsync(() => runtime.McpClients.Servers.Any(s => s.Id == "ghmcp"));

        var payload = SettingsOps.GetMcpServers(runtime);

        var dto = Assert.Single(payload.Servers);
        Assert.Equal("ghmcp", dto.Id);
        Assert.NotNull(dto.State);
        Assert.Equal(0, dto.ToolCount);
    }

    [Fact]
    public void A_configured_server_with_no_matching_live_entry_yet_gets_a_null_state_not_an_exception()
    {
        // No mcp: section at all when the runtime starts, so the background connect (which runs once,
        // over whatever ResolvedSettings.McpServers held at startup) never touches this server — it is
        // added afterward, purely through SaveMcpServers, the same as a server appended to config after
        // a process is already running (ADR_20260826_service_mcp-client §4, open question 1).
        using var runtime = BuildRuntime();

        var saved = SettingsOps.SaveMcpServers(runtime, new McpServersPayload
        {
            Servers = { new McpServerDto { Id = "webby", Transport = "http", Url = "https://example.test/mcp" } }
        });

        var dto = Assert.Single(saved.Servers);
        Assert.Equal("webby", dto.Id);
        Assert.Null(dto.State);
        Assert.Null(dto.LastError);
        Assert.Equal(0, dto.ToolCount);

        // GetMcpServers independently agrees — not just the value SaveMcpServers happened to return.
        var fetched = Assert.Single(SettingsOps.GetMcpServers(runtime).Servers);
        Assert.Null(fetched.State);
    }

    // ── The landmine: SaveMcp must not clobber Servers, and SaveMcpServers must not clobber Enabled/Port ──

    [Fact]
    public void Saving_outward_settings_with_defaults_after_saving_servers_does_not_delete_the_servers()
    {
        using var runtime = BuildRuntime();

        SettingsOps.SaveMcpServers(runtime, new McpServersPayload
        {
            Servers = { new McpServerDto { Id = "ghmcp", Transport = "stdio", Command = "npx" } }
        });

        // Exactly the trap described in the task: outward settings saved with both fields at their
        // default (Enabled=false, Port=null) — the shape a person toggling /mcp off with no port set
        // would send.
        SettingsOps.SaveMcp(runtime, new McpSettingsPayload { Enabled = false, Port = null });

        // Survives in the live ResolvedSettings...
        var live = Assert.Single(runtime.Settings.McpServers);
        Assert.Equal("ghmcp", live.Id);

        // ...and on disk: reload the project file fresh, independent of the live object.
        var reloaded = ConfigLoader.LoadProject(runtime.Settings.ProjectFilePath!);
        Assert.NotNull(reloaded.Mcp);
        var persisted = Assert.Single(reloaded.Mcp!.Servers ?? new());
        Assert.Equal("ghmcp", persisted.Id);
    }

    [Fact]
    public void Saving_servers_after_saving_outward_settings_does_not_clear_enabled_or_port()
    {
        using var runtime = BuildRuntime();

        SettingsOps.SaveMcp(runtime, new McpSettingsPayload { Enabled = true, Port = 7777 });
        SettingsOps.SaveMcpServers(runtime, new McpServersPayload
        {
            Servers = { new McpServerDto { Id = "ghmcp", Transport = "stdio", Command = "npx" } }
        });

        Assert.True(runtime.Settings.McpEnabled);
        Assert.Equal(7777, runtime.Settings.McpPort);

        var reloaded = ConfigLoader.LoadProject(runtime.Settings.ProjectFilePath!);
        Assert.NotNull(reloaded.Mcp);
        Assert.True(reloaded.Mcp!.Enabled);
        Assert.Equal(7777, reloaded.Mcp.Port);
    }

    // ── Full round trip: both halves persist together through the actual .spla file ──────────────

    [Fact]
    public void Both_halves_of_the_mcp_section_round_trip_together_through_the_spla_file()
    {
        using var runtime = BuildRuntime();

        SettingsOps.SaveMcp(runtime, new McpSettingsPayload { Enabled = true, Port = 4242 });
        SettingsOps.SaveMcpServers(runtime, new McpServersPayload
        {
            Servers =
            {
                new McpServerDto
                {
                    Id = "ghmcp", Name = "GitHub", Transport = "stdio", Command = "npx",
                    Args = new() { "-y", "@modelcontextprotocol/server-github" },
                    Env = new() { ["GITHUB_TOKEN"] = "secret:project:github-pat" },
                    Origin = "unnamed"
                }
            }
        });

        var reloaded = ConfigLoader.LoadProject(runtime.Settings.ProjectFilePath!);

        Assert.NotNull(reloaded.Mcp);
        Assert.True(reloaded.Mcp!.Enabled);
        Assert.Equal(4242, reloaded.Mcp.Port);
        var server = Assert.Single(reloaded.Mcp.Servers ?? new());
        Assert.Equal("ghmcp", server.Id);
        Assert.Equal("npx", server.Command);
        Assert.Equal("secret:project:github-pat", server.Env?["GITHUB_TOKEN"]);

        // And the resolved settings a fresh load produces agree with what is on disk.
        var resolved = ConfigLoader.LoadAndResolve(runtime.Settings.ProjectFilePath!);
        Assert.True(resolved.McpEnabled);
        Assert.Equal(4242, resolved.McpPort);
        Assert.Equal("ghmcp", Assert.Single(resolved.McpServers).Id);
    }

    // ── An untouched project stays free of an mcp: section ──────────────────────────────────────

    [Fact]
    public void Saving_both_at_their_defaults_leaves_no_mcp_section_at_all()
    {
        using var runtime = BuildRuntime();

        SettingsOps.SaveMcp(runtime, new McpSettingsPayload { Enabled = false, Port = null });
        SettingsOps.SaveMcpServers(runtime, new McpServersPayload());

        var reloaded = ConfigLoader.LoadProject(runtime.Settings.ProjectFilePath!);
        Assert.Null(reloaded.Mcp);
    }
}
