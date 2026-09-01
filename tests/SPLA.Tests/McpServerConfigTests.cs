using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SPLA.Domain.Settings;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The <c>mcp.servers:</c> declaration — parsing, layering by id and saving. Nothing here connects to
/// anything; that is later work (Plan step 2+). This is only "what did the config say".
/// </summary>
public sealed class McpServerConfigTests
{
    private static SplaMcpServerSection Server(string id, string? name = null) => new()
    {
        Id = id,
        Name = name ?? id,
        Transport = "stdio",
        Command = "npx",
        Args = new List<string> { "-y", "@modelcontextprotocol/server-github" },
        Env = new Dictionary<string, string> { ["GITHUB_TOKEN"] = "secret:project:github-pat" }
    };

    [Fact]
    public void A_server_declared_only_in_the_machine_layer_survives_into_resolved_settings()
    {
        var defaults = new SplaDefaults { Mcp = new SplaMcpSection { Servers = new() { Server("ghmcp") } } };

        var resolved = SettingsResolver.Resolve(defaults, new SplaProject());

        var server = Assert.Single(resolved.McpServers);
        Assert.Equal("ghmcp", server.Id);
        Assert.Equal("npx", server.Command);
    }

    [Fact]
    public void A_server_declared_in_both_layers_has_the_project_entry_win_whole()
    {
        var defaults = new SplaDefaults
        {
            Mcp = new SplaMcpSection
            {
                Servers = new()
                {
                    new SplaMcpServerSection
                    {
                        Id = "ghmcp", Name = "GitHub (machine)", Transport = "stdio",
                        Command = "npx", Args = new() { "-y", "@modelcontextprotocol/server-github" },
                        Env = new() { ["GITHUB_TOKEN"] = "secret:user:github-pat" }
                    }
                }
            }
        };
        var project = new SplaProject
        {
            Mcp = new SplaMcpSection
            {
                Servers = new()
                {
                    new SplaMcpServerSection
                    {
                        Id = "ghmcp", Name = "GitHub (project)", Transport = "http",
                        Url = "https://example.test/mcp"
                    }
                }
            }
        };

        var resolved = SettingsResolver.Resolve(defaults, project);

        var server = Assert.Single(resolved.McpServers);
        Assert.Equal("GitHub (project)", server.Name);
        Assert.Equal("http", server.Transport);
        Assert.Equal("https://example.test/mcp", server.Url);
        // Whole-entry replacement: the machine layer's stdio fields do not leak through.
        Assert.Null(server.Command);
        Assert.Null(server.Env);
    }

    [Fact]
    public void Servers_with_distinct_ids_from_both_layers_all_appear()
    {
        var defaults = new SplaDefaults { Mcp = new SplaMcpSection { Servers = new() { Server("ghmcp") } } };
        var project = new SplaProject { Mcp = new SplaMcpSection { Servers = new() { Server("everything") } } };

        var resolved = SettingsResolver.Resolve(defaults, project);

        Assert.Equal(new[] { "everything", "ghmcp" }, resolved.McpServers.Select(s => s.Id).OrderBy(x => x));
    }

    [Fact]
    public void An_entry_with_a_blank_or_missing_id_is_skipped_and_does_not_throw()
    {
        var project = new SplaProject
        {
            Mcp = new SplaMcpSection
            {
                Servers = new()
                {
                    new SplaMcpServerSection { Id = "", Name = "no id" },
                    new SplaMcpServerSection { Id = null, Name = "also no id" },
                    Server("ghmcp"),
                }
            }
        };

        var resolved = SettingsResolver.Resolve(null, project);

        var server = Assert.Single(resolved.McpServers);
        Assert.Equal("ghmcp", server.Id);
    }

    /// <summary>Regression guard: this section already existed for the outward half
    /// (<c>enabled</c>/<c>port</c>) before <c>servers:</c> was added, and adding the new field must
    /// not disturb it.</summary>
    [Fact]
    public void With_no_mcp_section_at_all_servers_is_empty_and_enabled_port_keep_their_defaults()
    {
        var resolved = SettingsResolver.Resolve(new SplaDefaults(), new SplaProject());

        Assert.NotNull(resolved.McpServers);
        Assert.Empty(resolved.McpServers);
        Assert.False(resolved.McpEnabled);
        Assert.Null(resolved.McpPort);
    }

    /// <summary>Round-trip through the same YAML path a real save/load uses — not a hand-rolled
    /// serializer — so a change to the underlying YamlDotNet config would be caught here too.</summary>
    [Fact]
    public void Servers_round_trip_through_yaml_with_env_args_and_headers_intact()
    {
        var path = Path.Combine(Path.GetTempPath(), $"spla-mcp-servers-{Guid.NewGuid():N}.spla");
        try
        {
            var project = new SplaProject
            {
                Name = "RoundTrip",
                Mcp = new SplaMcpSection
                {
                    Enabled = true,
                    Port = 7777,
                    Servers = new()
                    {
                        new SplaMcpServerSection
                        {
                            Id = "ghmcp",
                            Name = "GitHub",
                            Enabled = true,
                            Transport = "stdio",
                            Command = "npx",
                            Args = new() { "-y", "@modelcontextprotocol/server-github" },
                            Env = new() { ["GITHUB_TOKEN"] = "secret:project:github-pat" },
                            Description = "Issues and PRs in our repos",
                            Origin = "unnamed",
                        },
                        new SplaMcpServerSection
                        {
                            Id = "webby",
                            Name = "Web",
                            Transport = "http",
                            Url = "https://example.test/mcp",
                            Headers = new() { ["Authorization"] = "secret:user:webby#token" },
                            Origin = "named",
                        }
                    }
                }
            };

            ConfigLoader.SaveProject(project, path);
            var loaded = ConfigLoader.LoadProject(path);

            Assert.NotNull(loaded.Mcp);
            Assert.True(loaded.Mcp!.Enabled);
            Assert.Equal(7777, loaded.Mcp.Port);
            Assert.Equal(2, loaded.Mcp.Servers?.Count);

            var ghmcp = loaded.Mcp.Servers!.Single(s => s.Id == "ghmcp");
            Assert.Equal("npx", ghmcp.Command);
            Assert.Equal(new[] { "-y", "@modelcontextprotocol/server-github" }, ghmcp.Args);
            Assert.Equal("secret:project:github-pat", ghmcp.Env?["GITHUB_TOKEN"]);
            Assert.Equal("unnamed", ghmcp.Origin);

            var webby = loaded.Mcp.Servers!.Single(s => s.Id == "webby");
            Assert.Equal("https://example.test/mcp", webby.Url);
            Assert.Equal("secret:user:webby#token", webby.Headers?["Authorization"]);
            Assert.Equal("named", webby.Origin);
        }
        finally
        {
            try { File.Delete(path); } catch { /* best effort */ }
        }
    }
}
