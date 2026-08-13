using System;
using System.IO;
using SPLA.Domain.Settings;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The project root has exactly one definition — the directory holding the manifest — and nothing in
/// the manifest can move it. These tests are the reason that claim is worth anything: before them
/// <c>WorkspacePath</c> was whatever the file said (usually <c>"."</c>) and only came out right
/// because startup happened to chdir into it.
/// </summary>
public sealed class ProjectRootTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "spla-root-" + Guid.NewGuid().ToString("N"));

    public ProjectRootTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { /* best effort */ }
    }

    private string WriteManifest(string body)
    {
        var path = Path.Combine(_dir, "test.spla");
        File.WriteAllText(path, body);
        return path;
    }

    [Fact]
    public void Root_is_the_manifest_directory_and_is_absolute()
    {
        var manifest = WriteManifest("version: 1\nname: Test\n");

        var resolved = ConfigLoader.LoadAndResolve(manifest);

        Assert.True(Path.IsPathRooted(resolved.WorkspacePath));
        Assert.Equal(
            Path.GetFullPath(_dir).TrimEnd(Path.DirectorySeparatorChar),
            resolved.WorkspacePath.TrimEnd(Path.DirectorySeparatorChar));
    }

    /// <summary>The whole point of removing the field: a manifest may not answer "where does the
    /// agent work" a second time. An old file carrying the key still loads — it is ignored, not
    /// honoured, and not an error.</summary>
    [Fact]
    public void Legacy_workspace_key_is_ignored_not_honoured()
    {
        var elsewhere = Path.Combine(_dir, "elsewhere");
        Directory.CreateDirectory(elsewhere);
        var manifest = WriteManifest("version: 1\nname: Test\nworkspace: elsewhere\n");

        var resolved = ConfigLoader.LoadAndResolve(manifest);

        Assert.Equal(
            Path.GetFullPath(_dir).TrimEnd(Path.DirectorySeparatorChar),
            resolved.WorkspacePath.TrimEnd(Path.DirectorySeparatorChar));
        Assert.True(resolved.HasProject);
    }

    /// <summary>No manifest, no project, no root. The current directory is where the process was
    /// launched — usable as a default, but callers must not treat it as a boundary, which is what
    /// <see cref="ResolvedSettings.HasProject"/> is for.</summary>
    [Fact]
    public void Without_a_manifest_there_is_no_project()
    {
        var resolved = ConfigLoader.LoadAndResolve(null);

        Assert.False(resolved.HasProject);
        Assert.Null(resolved.ProjectFilePath);
        Assert.True(Path.IsPathRooted(resolved.WorkspacePath));
    }
}
