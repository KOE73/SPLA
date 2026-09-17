using System.Text.Json;
using SPLA.Plugins.Android.Runtime;

namespace SPLA.Tests;

/// <summary>
/// Wave 1 (PLAN_20260910_plugins_android-device.md §1.2): the spla-runtime.json install receipt —
/// Write → TryRead round-trip, absent file → no receipt, and the manual-install fallback (ADR
/// §3.6: a folder without a receipt but with its marker file is "installed manually", version
/// 'unknown', channel 'manual').
/// </summary>
public sealed class RuntimeReceiptTests
{
    [Fact]
    public void Write_then_TryRead_round_trips_every_field()
    {
        var folder = CreateTempFolder();
        try
        {
            var receipt = new RuntimeReceipt(
                "scrcpy",
                "4.1",
                "pinned",
                "https://github.com/Genymobile/scrcpy/releases/download/v4.1/scrcpy-win64-v4.1.zip",
                "a90f7c3" + new string('0', 57),
                true,
                "2026-09-10T10:00:00+03:00");

            RuntimeReceipt.Write(folder, receipt);

            var read = RuntimeReceipt.TryRead(folder);

            Assert.Equal(receipt, read);
        }
        finally
        {
            DeleteTempFolder(folder);
        }
    }

    [Fact]
    public void TryRead_on_a_folder_without_a_receipt_returns_null()
    {
        var folder = CreateTempFolder();
        try
        {
            Assert.Null(RuntimeReceipt.TryRead(folder));
        }
        finally
        {
            DeleteTempFolder(folder);
        }
    }

    [Fact]
    public void TryRead_on_a_folder_that_does_not_exist_returns_null()
    {
        var folder = Path.Combine(Path.GetTempPath(), "spla-tests", Guid.NewGuid().ToString("N"));

        Assert.Null(RuntimeReceipt.TryRead(folder));
    }

    [Fact]
    public void Write_serializes_the_plan_shape_with_snake_case_fields_and_an_offset_timestamp()
    {
        var folder = CreateTempFolder();
        try
        {
            RuntimeReceipt.Write(folder, new RuntimeReceipt(
                "scrcpy",
                "4.1",
                "pinned",
                "https://github.com/Genymobile/scrcpy/releases/download/v4.1/scrcpy-win64-v4.1.zip",
                "f".PadRight(64, '0'),
                true,
                "2026-09-10T10:00:00+03:00"));

            using var document = JsonDocument.Parse(
                File.ReadAllText(Path.Combine(folder, RuntimeReceipt.FileName)));
            var root = document.RootElement;

            Assert.Equal("scrcpy", root.GetProperty("component").GetString());
            Assert.Equal("4.1", root.GetProperty("version").GetString());
            Assert.Equal("pinned", root.GetProperty("channel").GetString());
            Assert.Equal("https://github.com/Genymobile/scrcpy/releases/download/v4.1/scrcpy-win64-v4.1.zip",
                root.GetProperty("url").GetString());
            Assert.Equal("f".PadRight(64, '0'), root.GetProperty("sha256").GetString());
            Assert.True(root.GetProperty("verified").GetBoolean());
            Assert.Equal("2026-09-10T10:00:00+03:00", root.GetProperty("installed_at").GetString());
        }
        finally
        {
            DeleteTempFolder(folder);
        }
    }

    [Fact]
    public void Manual_install_adb_marker_without_a_receipt_is_unknown_version_on_the_manual_channel()
    {
        var folder = CreateTempFolder();
        try
        {
            File.WriteAllText(Path.Combine(folder, "adb.exe"), "dummy adb.exe");

            Assert.Null(RuntimeReceipt.TryRead(folder));

            var manual = RuntimeReceipt.TryManualInstall("adb", folder);

            Assert.NotNull(manual);
            Assert.Equal("adb", manual!.Component);
            Assert.Equal("unknown", manual.Version);
            Assert.Equal("manual", manual.Channel);
        }
        finally
        {
            DeleteTempFolder(folder);
        }
    }

    [Fact]
    public void Manual_install_scrcpy_marker_without_a_receipt_is_unknown_version_on_the_manual_channel()
    {
        var folder = CreateTempFolder();
        try
        {
            File.WriteAllText(Path.Combine(folder, "scrcpy-server"), "dummy scrcpy-server");

            var manual = RuntimeReceipt.TryManualInstall("scrcpy", folder);

            Assert.NotNull(manual);
            Assert.Equal("scrcpy", manual!.Component);
            Assert.Equal("unknown", manual.Version);
            Assert.Equal("manual", manual.Channel);
        }
        finally
        {
            DeleteTempFolder(folder);
        }
    }

    [Fact]
    public void Marker_file_without_a_receipt_is_not_reported_as_installed()
    {
        var folder = CreateTempFolder();
        try
        {
            Assert.Null(RuntimeReceipt.TryManualInstall("adb", folder));
            Assert.Null(RuntimeReceipt.TryManualInstall("scrcpy", folder));
            Assert.Null(RuntimeReceipt.TryManualInstall("unknown-component", folder));
        }
        finally
        {
            DeleteTempFolder(folder);
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
