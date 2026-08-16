using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Security;
using SPLA.MCP.BasicTools.FileSystem;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Permissions;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// A mount in the zone model, through the real pipeline. Two things are being pinned: that a mount is
/// an <b>instance</b> rather than a category, and that reading from one the operator marked
/// <c>untrusted</c> raises the chat's flag on the same wire a dirty blob does.
/// </summary>
public sealed class MountZoneTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "spla-mz-" + Guid.NewGuid().ToString("N"));
    private readonly string _aaa = Path.Combine(Path.GetTempPath(), "spla-mz-aaa-" + Guid.NewGuid().ToString("N"));
    private readonly string _bbb = Path.Combine(Path.GetTempPath(), "spla-mz-bbb-" + Guid.NewGuid().ToString("N"));

    public MountZoneTests()
    {
        Directory.CreateDirectory(_root);
        Directory.CreateDirectory(_aaa);
        Directory.CreateDirectory(_bbb);
        File.WriteAllText(Path.Combine(_root, "inside.txt"), "in the project");
        File.WriteAllText(Path.Combine(_aaa, "nginx.conf"), "worker_processes 4;");
        File.WriteAllText(Path.Combine(_bbb, "dropped.txt"), "someone else put this here");
    }

    public void Dispose()
    {
        foreach (var dir in new[] { _root, _aaa, _bbb })
            try { Directory.Delete(dir, recursive: true); } catch { /* best effort */ }
    }

    private ProjectMount Aaa => new("AAA", _aaa, MountAccess.Read, MountTrust.Trusted, "reference settings", true);
    private ProjectMount Bbb => new("BBB", _bbb, MountAccess.Read, MountTrust.Untrusted, "shared drop", true);

    private PathBoundary Boundary => new(_root, null, [Aaa, Bbb]);

    /// <summary>The wiring AgentRuntime does, reproduced: the boundary answers where a path landed,
    /// the manifest answers how far that place is believed.</summary>
    private McpHost NewHost()
    {
        var boundary = Boundary;

        Zone ZoneOfPath(string? p)
        {
            if (string.IsNullOrWhiteSpace(p)) return Zone.Unknown;
            var landing = boundary.Resolve(p);
            if (!landing.Ok) return Zone.Machine;
            return landing.Mount is { } m ? Zone.Mount(m.Name) : Zone.Project;
        }

        DataOrigin? OriginOfZone(Zone zone) => zone.Kind != "mount"
            ? null
            : boundary.Mounts.FirstOrDefault(m => m.Name.Equals(zone.Instance, StringComparison.OrdinalIgnoreCase))
                is { } mount
                    ? DataOrigin.Mount(mount.Name, mount.Trust == MountTrust.Trusted)
                    : null;

        var host = new McpHost(
            new PermissionManager(), zoneOfPath: ZoneOfPath, originOfZone: OriginOfZone);

        host.RegisterTool(new FsReadTool());
        return host;
    }

    private (IDisposable Scope, AgentSession Session) Begin()
    {
        var workspace = new LocalWorkspace(Boundary, BoundaryMode.Shadow);
        var session = new AgentSession(
            new KeyValueStore("session"), new MarkManager(), new SkillSession(),
            sandbox: new PassthroughSandbox(workspace, shell: null));
        return (AgentSessionScope.Begin(session), session);
    }

    private static string ReadArgs(string path) =>
        $$"""{"path":{{System.Text.Json.JsonSerializer.Serialize(path)}},"start_line":null,"line_count":null,"output":null,"output_name":null}""";

    // ── instance, not category ────────────────────────────────────────────────

    [Fact]
    public async Task A_read_from_a_mount_is_recorded_as_that_mount_not_as_the_project()
    {
        var host = NewHost();
        using var scope = Begin().Scope;

        await host.ExecuteToolAsync(AgentMode.Agent, "system_read_file",
            ReadArgs("mnt/AAA/nginx.conf"), CancellationToken.None);

        var traffic = Assert.Single(host.Edges.List());
        Assert.Equal(Zone.Mount("AAA"), traffic.Edge.Source);
        Assert.Equal(Zone.Context, traffic.Edge.Sink);
        Assert.NotEqual(Zone.Project, traffic.Edge.Source);
        Assert.NotEqual(Zone.Machine, traffic.Edge.Source);
    }

    /// <summary>
    /// The distinction the whole model turns on. A permission given for a folder of reference settings
    /// must not silently cover the shared drop next to it — so two mounts are two rows, exactly as
    /// <c>sql:prod</c> and <c>sql:test</c> are.
    /// </summary>
    [Fact]
    public async Task Two_mounts_are_two_zones_and_two_rows()
    {
        var host = NewHost();
        using var scope = Begin().Scope;

        await host.ExecuteToolAsync(AgentMode.Agent, "system_read_file",
            ReadArgs("mnt/AAA/nginx.conf"), CancellationToken.None);
        await host.ExecuteToolAsync(AgentMode.Agent, "system_read_file",
            ReadArgs("mnt/BBB/dropped.txt"), CancellationToken.None);

        var rows = host.Edges.List();
        Assert.Equal(2, rows.Count);
        Assert.Contains(rows, r => r.Edge.Source == Zone.Mount("AAA"));
        Assert.Contains(rows, r => r.Edge.Source == Zone.Mount("BBB"));
    }

    [Fact]
    public async Task The_project_itself_is_still_the_project()
    {
        var host = NewHost();
        using var scope = Begin().Scope;

        await host.ExecuteToolAsync(AgentMode.Agent, "system_read_file",
            ReadArgs("inside.txt"), CancellationToken.None);

        Assert.Equal(Zone.Project, Assert.Single(host.Edges.List()).Edge.Source);
    }

    // ── the doubt flag ────────────────────────────────────────────────────────

    /// <summary>Named by the operator, so it is believed — the same standing an SSH host gets. A
    /// mount that raised doubt by default would make the flag a constant within an afternoon.</summary>
    [Fact]
    public async Task Reading_a_trusted_mount_leaves_the_flag_down()
    {
        var host = NewHost();
        var (scope, session) = Begin();
        using var _ = scope;

        await host.ExecuteToolAsync(AgentMode.Agent, "system_read_file",
            ReadArgs("mnt/AAA/nginx.conf"), CancellationToken.None);

        Assert.False(session.Doubt.IsRaised);
    }

    /// <summary>The one case that differs: a folder other people put files into.</summary>
    [Fact]
    public async Task Reading_an_untrusted_mount_raises_the_flag_and_says_what_did_it()
    {
        var host = NewHost();
        var (scope, session) = Begin();
        using var _ = scope;

        await host.ExecuteToolAsync(AgentMode.Agent, "system_read_file",
            ReadArgs("mnt/BBB/dropped.txt"), CancellationToken.None);

        Assert.True(session.Doubt.IsRaised);
        var cause = Assert.Single(session.Doubt.Causes);
        Assert.Equal("mount:BBB", cause.Origin.Zone);
        Assert.Equal("system_read_file", cause.What);
    }

    /// <summary>Classifying is not refusing. A shadow that changes an outcome is not a shadow, and the
    /// flag costs a re-asked question later — never the call in front of it.</summary>
    [Fact]
    public async Task Raising_the_flag_does_not_refuse_the_call()
    {
        var host = NewHost();
        var (scope, session) = Begin();
        using var _ = scope;

        var result = await host.ExecuteToolAsync(AgentMode.Agent, "system_read_file",
            ReadArgs("mnt/BBB/dropped.txt"), CancellationToken.None);

        Assert.NotEqual(ToolOutcome.Refused, result.Outcome);
        Assert.Contains("someone else put this here", result.ToString());
        Assert.True(session.Doubt.IsRaised);
    }

    /// <summary>A host with no mounts asks nothing and gets nothing — the hook is inert rather than
    /// defaulting to suspicion.</summary>
    [Fact]
    public async Task Without_the_hook_nothing_raises_the_flag()
    {
        var boundary = Boundary;
        var host = new McpHost(new PermissionManager(), zoneOfPath: p =>
            boundary.Resolve(p ?? "").Mount is { } m ? Zone.Mount(m.Name) : Zone.Project);
        host.RegisterTool(new FsReadTool());

        var (scope, session) = Begin();
        using var _ = scope;

        await host.ExecuteToolAsync(AgentMode.Agent, "system_read_file",
            ReadArgs("mnt/BBB/dropped.txt"), CancellationToken.None);

        Assert.False(session.Doubt.IsRaised);
    }
}
