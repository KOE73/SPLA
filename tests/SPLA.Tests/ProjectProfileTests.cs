using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SPLA.Domain.Project;
using SPLA.Domain.Settings;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Tests for project profile selection, creation, and manifest discovery.
/// Covers ProjectProfiles, ProjectFactory, and ConfigLoader.FindProjectFile.
/// </summary>
public sealed class ProjectProfileTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(
        Path.GetTempPath(),
        $"spla-profile-tests-{Guid.NewGuid():N}");

    public ProjectProfileTests()
    {
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempRoot, recursive: true); } catch { /* best effort */ }
    }

    // ── ProjectProfiles.TryParse ──────────────────────────────────────────────

    [Fact]
    public void TryParse_accepts_minimal_case_insensitively()
    {
        Assert.True(ProjectProfiles.TryParse("minimal", out var profile));
        Assert.Equal(ProjectProfile.Minimal, profile);

        Assert.True(ProjectProfiles.TryParse("MINIMAL", out profile));
        Assert.Equal(ProjectProfile.Minimal, profile);

        Assert.True(ProjectProfiles.TryParse("MiNiMaL", out profile));
        Assert.Equal(ProjectProfile.Minimal, profile);
    }

    [Fact]
    public void TryParse_accepts_standard_case_insensitively()
    {
        Assert.True(ProjectProfiles.TryParse("standard", out var profile));
        Assert.Equal(ProjectProfile.Standard, profile);

        Assert.True(ProjectProfiles.TryParse("STANDARD", out profile));
        Assert.Equal(ProjectProfile.Standard, profile);

        Assert.True(ProjectProfiles.TryParse("StAnDaRd", out profile));
        Assert.Equal(ProjectProfile.Standard, profile);
    }

    [Fact]
    public void TryParse_accepts_inherit_case_insensitively()
    {
        Assert.True(ProjectProfiles.TryParse("inherit", out var profile));
        Assert.Equal(ProjectProfile.Inherit, profile);

        Assert.True(ProjectProfiles.TryParse("INHERIT", out profile));
        Assert.Equal(ProjectProfile.Inherit, profile);

        Assert.True(ProjectProfiles.TryParse("InHeRiT", out profile));
        Assert.Equal(ProjectProfile.Inherit, profile);
    }

    [Fact]
    public void TryParse_rejects_unknown_name_and_returns_false()
    {
        Assert.False(ProjectProfiles.TryParse("unknown", out var profile));
        Assert.Equal(ProjectProfiles.Default, profile);

        Assert.False(ProjectProfiles.TryParse("automat", out profile));
        Assert.Equal(ProjectProfiles.Default, profile);

        Assert.False(ProjectProfiles.TryParse("", out profile));
        Assert.Equal(ProjectProfiles.Default, profile);

        Assert.False(ProjectProfiles.TryParse(null, out profile));
        Assert.Equal(ProjectProfiles.Default, profile);
    }

    // ── ProjectProfiles.Default ───────────────────────────────────────────────

    [Fact]
    public void Default_is_Minimal()
    {
        Assert.Equal(ProjectProfile.Minimal, ProjectProfiles.Default);
    }

    // ── ProjectFactory.Create ─────────────────────────────────────────────────

    [Fact]
    public void Create_in_empty_directory_writes_manifest_with_directory_name()
    {
        var dir = Path.Combine(_tempRoot, "myproject");
        Directory.CreateDirectory(dir);

        var manifestPath = ProjectFactory.Create(dir, null, ProjectProfile.Minimal);

        Assert.True(File.Exists(manifestPath));
        Assert.Equal(Path.Combine(dir, "myproject.spla"), manifestPath);

        var project = ConfigLoader.LoadProjectRaw(manifestPath);
        Assert.Equal("myproject", project.Name);
        Assert.NotNull(project.Ignore);
        Assert.NotEmpty(project.Ignore);
    }

    [Fact]
    public void Create_with_explicit_name_uses_that_name()
    {
        var dir = Path.Combine(_tempRoot, "somedir");
        Directory.CreateDirectory(dir);

        var manifestPath = ProjectFactory.Create(dir, "CustomName", ProjectProfile.Minimal);

        Assert.True(File.Exists(manifestPath));
        Assert.Equal(Path.Combine(dir, "CustomName.spla"), manifestPath);

        var project = ConfigLoader.LoadProjectRaw(manifestPath);
        Assert.Equal("CustomName", project.Name);
    }

    [Fact]
    public void Create_with_Minimal_profile_writes_empty_capabilities_list()
    {
        var dir = Path.Combine(_tempRoot, "minimal-test");
        Directory.CreateDirectory(dir);

        var manifestPath = ProjectFactory.Create(dir, "test", ProjectProfile.Minimal);
        var project = ConfigLoader.LoadProjectRaw(manifestPath);

        Assert.NotNull(project.Agent);
        Assert.NotNull(project.Agent.Capabilities);
        Assert.Empty(project.Agent.Capabilities);
    }

    [Fact]
    public void Create_with_Minimal_profile_writes_wildcard_plugin_disabled()
    {
        var dir = Path.Combine(_tempRoot, "minimal-plugin-test");
        Directory.CreateDirectory(dir);

        var manifestPath = ProjectFactory.Create(dir, "test", ProjectProfile.Minimal);
        var project = ConfigLoader.LoadProjectRaw(manifestPath);

        Assert.NotNull(project.Plugins);
        Assert.True(project.Plugins.ContainsKey(ProjectFactory.PluginWildcard));
        Assert.False(project.Plugins[ProjectFactory.PluginWildcard].Enabled);
    }

    [Fact]
    public void Create_with_Standard_profile_writes_nothing_special_for_capabilities()
    {
        var dir = Path.Combine(_tempRoot, "standard-test");
        Directory.CreateDirectory(dir);

        var manifestPath = ProjectFactory.Create(dir, "test", ProjectProfile.Standard);
        var project = ConfigLoader.LoadProjectRaw(manifestPath);

        // Standard profile should NOT set capabilities to empty list (null means everything)
        Assert.Null(project.Agent?.Capabilities);
    }

    [Fact]
    public void Create_with_Standard_profile_does_not_write_wildcard_plugin_entry()
    {
        var dir = Path.Combine(_tempRoot, "standard-plugin-test");
        Directory.CreateDirectory(dir);

        var manifestPath = ProjectFactory.Create(dir, "test", ProjectProfile.Standard);
        var project = ConfigLoader.LoadProjectRaw(manifestPath);

        // Standard profile should not have wildcard plugin entry
        if (project.Plugins != null)
        {
            Assert.False(project.Plugins.ContainsKey(ProjectFactory.PluginWildcard));
        }
    }

    [Fact]
    public void Create_with_Inherit_profile_throws_ArgumentException()
    {
        var dir = Path.Combine(_tempRoot, "inherit-test");
        Directory.CreateDirectory(dir);

        var ex = Assert.Throws<ArgumentException>(
            () => ProjectFactory.Create(dir, "test", ProjectProfile.Inherit));

        Assert.Contains("inherit", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("profile", ex.ParamName);
    }

    [Fact]
    public void Create_in_directory_with_existing_manifest_throws_InvalidOperationException()
    {
        var dir = Path.Combine(_tempRoot, "existing-test");
        Directory.CreateDirectory(dir);

        // Create first project
        ProjectFactory.Create(dir, "first", ProjectProfile.Minimal);

        // Attempt to create second project in same directory should fail
        var ex = Assert.Throws<InvalidOperationException>(
            () => ProjectFactory.Create(dir, "second", ProjectProfile.Minimal));

        Assert.Contains(dir, ex.Message);
        Assert.Contains("already holds a project", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── ProjectFactory.ManifestPathFor ────────────────────────────────────────

    [Fact]
    public void ManifestPathFor_sanitizes_illegal_filename_characters()
    {
        var dir = Path.Combine(_tempRoot, "sanitize-test");

        var path1 = ProjectFactory.ManifestPathFor(dir, "my:project");
        var fileName1 = Path.GetFileName(path1);
        Assert.Equal("my-project.spla", fileName1);

        var path2 = ProjectFactory.ManifestPathFor(dir, "my/project");
        var fileName2 = Path.GetFileName(path2);
        Assert.Equal("my-project.spla", fileName2);

        var path3 = ProjectFactory.ManifestPathFor(dir, "my<project>test");
        var fileName3 = Path.GetFileName(path3);
        Assert.Equal("my-project-test.spla", fileName3);
    }

    [Fact]
    public void ManifestPathFor_leaves_normal_names_alone()
    {
        var dir = Path.Combine(_tempRoot, "normal-test");

        var path = ProjectFactory.ManifestPathFor(dir, "myproject");
        Assert.EndsWith("myproject.spla", path);

        var path2 = ProjectFactory.ManifestPathFor(dir, "my-project-123");
        Assert.EndsWith("my-project-123.spla", path2);
    }

    // ── ConfigLoader.FindProjectFile ─────────────────────────────────────────

    [Fact]
    public void FindProjectFile_returns_null_for_empty_directory()
    {
        var dir = Path.Combine(_tempRoot, "empty-find-test");
        Directory.CreateDirectory(dir);

        var result = ConfigLoader.FindProjectFile(dir);

        Assert.Null(result);
    }

    [Fact]
    public void FindProjectFile_returns_the_manifest_when_exactly_one_exists()
    {
        var dir = Path.Combine(_tempRoot, "single-find-test");
        Directory.CreateDirectory(dir);

        var manifestPath = Path.Combine(dir, "myproject.spla");
        File.WriteAllText(manifestPath, "name: myproject");

        var result = ConfigLoader.FindProjectFile(dir);

        Assert.Equal(manifestPath, result);
    }

    [Fact]
    public void FindProjectFile_throws_InvalidOperationException_when_multiple_manifests_exist()
    {
        var dir = Path.Combine(_tempRoot, "multi-find-test");
        Directory.CreateDirectory(dir);

        var manifest1 = Path.Combine(dir, "alpha.spla");
        var manifest2 = Path.Combine(dir, "beta.spla");
        File.WriteAllText(manifest1, "name: alpha");
        File.WriteAllText(manifest2, "name: beta");

        var ex = Assert.Throws<InvalidOperationException>(
            () => ConfigLoader.FindProjectFile(dir));

        // Exception should mention both file names
        var message = ex.Message;
        Assert.Contains("alpha.spla", message);
        Assert.Contains("beta.spla", message);
        Assert.Contains("2", message); // Should mention there are 2 files
    }

    [Fact]
    public void FindProjectFile_returns_correct_file_among_mixed_extensions()
    {
        var dir = Path.Combine(_tempRoot, "mixed-ext-test");
        Directory.CreateDirectory(dir);

        // Create other files with different extensions
        File.WriteAllText(Path.Combine(dir, "config.yaml"), "");
        File.WriteAllText(Path.Combine(dir, "readme.txt"), "");

        // Create the only .spla file
        var manifestPath = Path.Combine(dir, "myproject.spla");
        File.WriteAllText(manifestPath, "name: myproject");

        var result = ConfigLoader.FindProjectFile(dir);

        Assert.Equal(manifestPath, result);
    }
}
