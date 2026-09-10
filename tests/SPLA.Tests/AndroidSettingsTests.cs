using SPLA.Plugins.Android;

namespace SPLA.Tests;

/// <summary>
/// Wave 0 of PLAN_20260910_plugins_android-device.md: the settings blob parses with defaults for a
/// missing/empty section and clamps out-of-range numbers to the bounds stated in the plan
/// (max_size 0 or 320–4096, max_fps 1–120, settle_ms 0–5000, settle_timeout_ms 0–30000).
/// </summary>
public sealed class AndroidSettingsTests
{
    [Fact]
    public void FromBlob_null_blob_returns_defaults()
    {
        var settings = AndroidSettings.FromBlob(null);
        AssertDefaults(settings);
    }

    [Fact]
    public void FromBlob_empty_blob_returns_defaults()
    {
        var settings = AndroidSettings.FromBlob(new Dictionary<string, object>());
        AssertDefaults(settings);
    }

    private static void AssertDefaults(AndroidSettings settings)
    {

        Assert.Equal("", settings.AdbPath);
        Assert.Equal("", settings.ScrcpyPath);
        Assert.Equal(0, settings.AdbServerPort);
        Assert.Equal(1280, settings.MaxSize);
        Assert.Equal(30, settings.MaxFps);
        Assert.Equal(8_000_000, settings.VideoBitRate);
        Assert.True(settings.StayAwake);
        Assert.True(settings.ScreenshotAfterAction);
        Assert.Equal(300, settings.SettleMs);
        Assert.Equal(3000, settings.SettleTimeoutMs);
        Assert.Equal(15, settings.IdleDisconnectMinutes);
    }

    [Fact]
    public void FromBlob_max_size_above_upper_bound_clamps_to_4096()
    {
        var settings = AndroidSettings.FromBlob(new Dictionary<string, object> { ["max_size"] = 99999 });

        Assert.Equal(4096, settings.MaxSize);
    }

    [Fact]
    public void FromBlob_max_size_below_lower_bound_clamps_to_320()
    {
        var settings = AndroidSettings.FromBlob(new Dictionary<string, object> { ["max_size"] = 100 });

        Assert.Equal(320, settings.MaxSize);
    }

    [Fact]
    public void FromBlob_max_size_zero_stays_zero()
    {
        // 0 means "native resolution" and is a valid value, not an out-of-range one.
        var settings = AndroidSettings.FromBlob(new Dictionary<string, object> { ["max_size"] = 0 });

        Assert.Equal(0, settings.MaxSize);
    }

    [Fact]
    public void FromBlob_fps_and_settle_bounds_clamp()
    {
        var settings = AndroidSettings.FromBlob(new Dictionary<string, object>
        {
            ["max_fps"] = 999,
            ["settle_ms"] = -5,
            ["settle_timeout_ms"] = 60000,
        });

        Assert.Equal(120, settings.MaxFps);
        Assert.Equal(0, settings.SettleMs);
        Assert.Equal(30000, settings.SettleTimeoutMs);
    }

    [Fact]
    public void FromBlob_idle_disconnect_minutes_clamps_to_1_1440()
    {
        var below = AndroidSettings.FromBlob(new Dictionary<string, object> { ["idle_disconnect_minutes"] = 0 });
        var above = AndroidSettings.FromBlob(new Dictionary<string, object> { ["idle_disconnect_minutes"] = 2880 });
        var inRange = AndroidSettings.FromBlob(new Dictionary<string, object> { ["idle_disconnect_minutes"] = 45 });

        Assert.Equal(1, below.IdleDisconnectMinutes);
        Assert.Equal(1440, above.IdleDisconnectMinutes);
        Assert.Equal(45, inRange.IdleDisconnectMinutes);
    }

    [Fact]
    public void FromBlob_video_bit_rate_clamps_to_500000_100000000()
    {
        var below = AndroidSettings.FromBlob(new Dictionary<string, object> { ["video_bit_rate"] = 100_000 });
        var above = AndroidSettings.FromBlob(new Dictionary<string, object> { ["video_bit_rate"] = 500_000_000 });
        var inRange = AndroidSettings.FromBlob(new Dictionary<string, object> { ["video_bit_rate"] = 4_000_000 });

        Assert.Equal(500_000, below.VideoBitRate);
        Assert.Equal(100_000_000, above.VideoBitRate);
        Assert.Equal(4_000_000, inRange.VideoBitRate);
    }

    [Theory]
    [InlineData(0, 0)]        // 0 = the standard adb server port 5037, a valid value.
    [InlineData(1, 1024)]     // reserved range 1..1023 snaps up to 1024.
    [InlineData(1023, 1024)]
    [InlineData(1024, 1024)]
    [InlineData(5038, 5038)]
    [InlineData(65535, 65535)]
    [InlineData(70000, 65535)]
    public void FromBlob_adb_server_port_clamps_reserved_and_out_of_range_values(int input, int expected)
    {
        var settings = AndroidSettings.FromBlob(new Dictionary<string, object> { ["adb_server_port"] = input });

        Assert.Equal(expected, settings.AdbServerPort);
    }

    [Fact]
    public void FromBlob_reads_snake_case_keys()
    {
        var settings = AndroidSettings.FromBlob(new Dictionary<string, object>
        {
            ["adb_path"] = "C:\\platform-tools",
            ["scrcpy_path"] = "C:\\scrcpy",
            ["adb_server_port"] = 5038,
            ["max_fps"] = 60,
            ["video_bit_rate"] = 4_000_000,
            ["stay_awake"] = false,
            ["screenshot_after_action"] = false,
            ["settle_ms"] = 500,
            ["idle_disconnect_minutes"] = 30,
        });

        Assert.Equal("C:\\platform-tools", settings.AdbPath);
        Assert.Equal("C:\\scrcpy", settings.ScrcpyPath);
        Assert.Equal(5038, settings.AdbServerPort);
        Assert.Equal(60, settings.MaxFps);
        Assert.Equal(4_000_000, settings.VideoBitRate);
        Assert.False(settings.StayAwake);
        Assert.False(settings.ScreenshotAfterAction);
        Assert.Equal(500, settings.SettleMs);
        Assert.Equal(30, settings.IdleDisconnectMinutes);
    }
}
