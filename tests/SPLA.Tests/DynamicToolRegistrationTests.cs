using SPLA.Domain.Models;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.ToolSets;
using SPLA.Domain.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The mechanism half of PLAN_20260819_core_dynamic-tool-registration, built for
/// PLAN_20260826_service_mcp-client step 1: a connected MCP server registers tools after the host is
/// already up, and takes them away again when the server disconnects. These tests pin the two things
/// that make that safe — <see cref="McpHost.UnregisterTool"/> actually removing a tool, and
/// <see cref="ToolSetRegistry"/> staying internally consistent (no dangling <c>SetOfTool</c> entry)
/// across <see cref="ToolSetRegistry.AddDynamic"/>/<see cref="ToolSetRegistry.RemoveDynamic"/> — plus
/// a concurrency smoke test that would have caught the plain <c>Dictionary</c> this replaced.
/// </summary>
public sealed class DynamicToolRegistrationTests
{
    private sealed class FakeTool : IMcpTool
    {
        public string Name { get; init; } = "fake_tool";

        public ToolDefinition GetDefinition() => new()
        {
            Type = "function",
            Function = new ToolFunctionDefinition { Name = Name, Description = "fake" }
        };

        public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken ct = default) =>
            Task.FromResult(ToolResult.Text("ran"));
    }

    private static McpHost Host() => new(new PermissionManager());

    [Fact]
    public void A_registered_tool_appears_and_unregistering_it_removes_it()
    {
        var host = Host();
        host.RegisterTool(new FakeTool { Name = "ghmcp_create_issue" });

        Assert.Contains(host.GetToolDefinitions(), d => d.Function.Name == "ghmcp_create_issue");

        host.UnregisterTool("ghmcp_create_issue");

        Assert.DoesNotContain(host.GetToolDefinitions(), d => d.Function.Name == "ghmcp_create_issue");
    }

    [Fact]
    public void Unregistering_a_name_that_was_never_registered_reports_nothing_removed()
    {
        var host = Host();
        Assert.False(host.UnregisterTool("never_registered"));
    }

    /// <summary>An Origin: Mcp set with no explicit `toolsets:` entry resolves to Enabled — the
    /// descriptor existing already means the server is connected and on (ToolSetRegistry.LevelOf).</summary>
    [Fact]
    public void AddDynamic_makes_a_sets_level_and_membership_queryable()
    {
        var sets = new ToolSetRegistry(new ResolvedSettings());

        sets.AddDynamic(new ToolSetDescriptor
        {
            Id = "ghmcp",
            Origin = ToolSetOrigin.Mcp,
            OriginId = "ghmcp",
            ToolNames = ["ghmcp_create_issue"]
        });

        Assert.Equal(ToolSetLevel.Enabled, sets.LevelOf("ghmcp"));
        Assert.Equal("ghmcp", sets.SetOfTool("ghmcp_create_issue"));
    }

    [Fact]
    public void RemoveDynamic_drops_both_the_set_and_its_tool_mappings()
    {
        var sets = new ToolSetRegistry(new ResolvedSettings());
        sets.AddDynamic(new ToolSetDescriptor
        {
            Id = "ghmcp",
            Origin = ToolSetOrigin.Mcp,
            OriginId = "ghmcp",
            ToolNames = ["ghmcp_create_issue", "ghmcp_list_issues"]
        });

        var removed = sets.RemoveDynamic("ghmcp");

        Assert.True(removed);
        Assert.Null(sets.Find("ghmcp"));
        // Not a dangling id — SetOfTool must forget the tool along with the set, not just fail to
        // resolve it through Find.
        Assert.Null(sets.SetOfTool("ghmcp_create_issue"));
        Assert.Null(sets.SetOfTool("ghmcp_list_issues"));
    }

    [Fact]
    public void RemoveDynamic_reports_nothing_removed_for_an_unknown_set()
    {
        var sets = new ToolSetRegistry(new ResolvedSettings());
        Assert.False(sets.RemoveDynamic("nothing-like-this"));
    }

    /// <summary>An explicit `toolsets:` entry is a ceiling the origin cannot lift — same rule as the
    /// plugin branch, now checked for Mcp.</summary>
    [Fact]
    public void An_explicit_toolsets_entry_wins_over_the_mcp_default()
    {
        var settings = new ResolvedSettings();
        settings.ToolSets["ghmcp"] = ToolSetRegistry.Format(ToolSetLevel.Disabled);
        var sets = new ToolSetRegistry(settings);

        sets.AddDynamic(new ToolSetDescriptor
        {
            Id = "ghmcp",
            Origin = ToolSetOrigin.Mcp,
            OriginId = "ghmcp",
            ToolNames = ["ghmcp_create_issue"]
        });

        Assert.Equal(ToolSetLevel.Disabled, sets.LevelOf("ghmcp"));
    }

    /// <summary>
    /// The test that would have caught the old plain Dictionary: one task registers and unregisters
    /// tools while another repeatedly reads GetToolDefinitions(). Nothing here asserts a particular
    /// interleaving — only that concurrent read and write never throw (the failure mode a
    /// non-concurrent Dictionary produces is an InvalidOperationException from a torn enumerator, or a
    /// corrupted bucket state under concurrent writes).
    /// </summary>
    [Fact]
    public async Task Concurrent_registration_and_enumeration_does_not_throw()
    {
        var host = Host();
        const int iterations = 500;
        Exception? failure = null;

        var writer = Task.Run(() =>
        {
            try
            {
                for (var i = 0; i < iterations; i++)
                {
                    host.RegisterTool(new FakeTool { Name = "churn_tool" });
                    host.UnregisterTool("churn_tool");
                }
            }
            catch (Exception ex)
            {
                Interlocked.CompareExchange(ref failure, ex, null);
            }
        });

        var reader = Task.Run(() =>
        {
            try
            {
                for (var i = 0; i < iterations; i++)
                {
                    _ = host.GetToolDefinitions().ToList();
                }
            }
            catch (Exception ex)
            {
                Interlocked.CompareExchange(ref failure, ex, null);
            }
        });

        await Task.WhenAll(writer, reader);

        Assert.Null(failure);
    }
}
