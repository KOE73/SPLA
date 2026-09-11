using SPLA.Plugins.Android.Runtime;

namespace SPLA.Tests;

/// <summary>
/// Wave 1 (PLAN_20260910_plugins_android-device.md §1.1): component folder resolution per ADR
/// §3.6 — empty → the shared user default, rooted → as-is, relative → against the project file's
/// directory, relative without a project → a configuration error visible in the status.
/// </summary>
public sealed class RuntimePathsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_empty_configured_falls_back_to_the_default_folder(string? configured)
    {
        var (folder, error) = RuntimePaths.Resolve("adb", configured, null);

        Assert.Equal(RuntimePaths.DefaultFolder("adb"), folder);
        Assert.Null(error);
    }

    [Fact]
    public void Resolve_rooted_path_is_returned_as_is_even_with_a_project()
    {
        const string configured = @"C:\tools\platform-tools";

        var (folder, error) = RuntimePaths.Resolve("adb", configured, @"C:\proj\work.spla");

        Assert.Equal(configured, folder);
        Assert.Null(error);
    }

    [Fact]
    public void Resolve_relative_path_is_combined_with_the_project_folder()
    {
        var (folder, error) = RuntimePaths.Resolve("adb", "tools/adb", @"C:\proj\work.spla");

        Assert.Equal(@"C:\proj\tools\adb", folder);
        Assert.Null(error);
    }

    [Fact]
    public void Resolve_relative_path_without_a_project_reports_the_exact_configuration_error()
    {
        var (folder, error) = RuntimePaths.Resolve("scrcpy", "scrcpy", null);

        Assert.Null(folder);
        Assert.Equal("relative scrcpy_path needs an open project", error);
    }

    [Fact]
    public void DefaultFolder_lives_under_localappdata_spla_runtime_android()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        Assert.Equal(
            Path.Combine(localAppData, "SPLA", "runtime", "android", "scrcpy"),
            RuntimePaths.DefaultFolder("scrcpy"));
    }

    [Fact]
    public void AdbExe_joins_adb_exe_to_the_folder()
    {
        Assert.Equal(@"C:\rt\adb\adb.exe", RuntimePaths.AdbExe(@"C:\rt\adb"));
    }

    [Fact]
    public void ScrcpyServer_returns_null_when_absent_and_the_path_when_present()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            Assert.Null(RuntimePaths.ScrcpyServer(tempFolder));

            File.WriteAllText(Path.Combine(tempFolder, "scrcpy-server"), "dummy");

            Assert.Equal(Path.Combine(tempFolder, "scrcpy-server"), RuntimePaths.ScrcpyServer(tempFolder));
        }
        finally
        {
            DeleteTempFolder(tempFolder);
        }
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "spla-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static void DeleteTempFolder(string folder)
    {
        if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
    }
}
