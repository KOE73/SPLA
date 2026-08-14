using System;
using System.IO;
using System.Linq;
using SPLA.Domain.Host;
using SPLA.Service;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The human surface behind the same boundary as the agent's. It used to build its own, cutout-free,
/// which put the restriction on the safest surface and left it off the most dangerous one — the
/// defect ADR_20260811 records. Consolidating means one answer, and these pin what that answer is.
/// </summary>
public sealed class WorkspaceOpsBoundaryTests : IDisposable
{
    private readonly string _stage = Path.Combine(Path.GetTempPath(), "spla-wsops-" + Guid.NewGuid().ToString("N"));
    private readonly string _root;
    private readonly string _mountTarget;

    public WorkspaceOpsBoundaryTests()
    {
        _root = Path.Combine(_stage, "project");
        _mountTarget = Path.Combine(_stage, "reference");
        Directory.CreateDirectory(Path.Combine(_root, "src"));
        Directory.CreateDirectory(Path.Combine(_root, ".spla"));
        Directory.CreateDirectory(_mountTarget);

        File.WriteAllText(Path.Combine(_root, "src", "app.cs"), "// code");
        File.WriteAllText(Path.Combine(_root, ".spla", "secrets.yaml"), "key: hunter2");
        File.WriteAllText(Path.Combine(_mountTarget, "nginx.conf"), "worker_processes 4;");
    }

    public void Dispose()
    {
        try { Directory.Delete(_stage, recursive: true); } catch { /* best effort */ }
    }

    private PathBoundary Boundary(MountAccess access = MountAccess.Read) =>
        new(_root, [".spla"],
            [new ProjectMount("AAA", _mountTarget, access, MountTrust.Trusted, "reference settings", true)]);

    /// <summary>The change this consolidation makes to the web client, stated as a test rather than
    /// left to be noticed: <c>.spla/</c> holds the chats, the secrets and the accounting, and it is
    /// closed to this surface too now.</summary>
    [Fact]
    public void The_applications_own_folder_is_not_readable_through_the_editor()
    {
        var result = WorkspaceOps.Read(Boundary(), Path.Combine(_root, ".spla", "secrets.yaml"));

        Assert.Equal("Access denied: path is outside workspace.", result.Error);
        Assert.DoesNotContain("hunter2", result.Text ?? "");
    }

    /// <summary>A folder listed but dead on click is worse than one that is not listed.</summary>
    [Fact]
    public void The_applications_own_folder_is_not_listed_either()
    {
        var nodes = WorkspaceOps.Browse(Boundary(), null).Nodes;

        Assert.DoesNotContain(nodes, n => n.Label == ".spla");
        Assert.Contains(nodes, n => n.Label == "src");
    }

    [Fact]
    public void Ordinary_project_files_are_unaffected()
    {
        var result = WorkspaceOps.Read(Boundary(), Path.Combine(_root, "src", "app.cs"));

        Assert.Null(result.Error);
        Assert.Equal("// code", result.Text);
    }

    /// <summary>
    /// A mount is reachable from here because it is reachable from the boundary — but the floor is a
    /// property of the node, and this surface writes through FileContentSource without passing
    /// LocalWorkspace.Guard. Without this check the editor would be the one way around a mount the
    /// operator called canonical.
    /// </summary>
    [Fact]
    public void A_read_only_mount_cannot_be_written_through_the_editor()
    {
        var target = Path.Combine(_mountTarget, "nginx.conf");

        var result = WorkspaceOps.Write(Boundary(), target, "worker_processes 8;");

        Assert.False(result.Ok);
        Assert.Contains("mount 'AAA' is declared read-only", result.Error);
        Assert.Equal("worker_processes 4;", File.ReadAllText(target));
    }

    [Fact]
    public void A_writable_mount_can_be_written_through_the_editor()
    {
        var target = Path.Combine(_mountTarget, "nginx.conf");

        var result = WorkspaceOps.Write(Boundary(MountAccess.Write), target, "worker_processes 8;");

        Assert.True(result.Ok);
        Assert.Equal("worker_processes 8;", File.ReadAllText(target));
    }

    [Fact]
    public void Anything_outside_the_root_and_outside_every_mount_is_still_refused()
    {
        var result = WorkspaceOps.Read(Boundary(), Path.Combine(_stage, "elsewhere.txt"));

        Assert.Equal("Access denied: path is outside workspace.", result.Error);
    }

    /// <summary>Callers hand in a bounded boundary; a project-less one must not take the surface down
    /// with it.</summary>
    [Fact]
    public void An_unbounded_boundary_browses_to_nothing_rather_than_throwing()
        => Assert.Empty(WorkspaceOps.Browse(PathBoundary.None, null).Nodes);
}
