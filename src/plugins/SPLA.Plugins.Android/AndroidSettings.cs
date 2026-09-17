using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Plugins.Android;

/// <summary>
/// Plugin-owned settings (android section in .spla file under plugins.android.settings).
/// Values are clamped to sane ranges: an out-of-range number in the blob is rounded to the
/// nearest bound instead of rejected, so a hand-edited file degrades instead of breaking.
/// </summary>
public sealed class AndroidSettings
{
    private static readonly ISerializer Ser = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
    private static readonly IDeserializer De = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties().Build();

    /// <summary>Folder of the adb component (adb.exe + AdbWin*.dll). Empty = %LOCALAPPDATA%\SPLA\runtime\android\adb.</summary>
    [YamlMember(Alias = "adb_path")]
    public string AdbPath { get; set; } = "";

    /// <summary>Folder of the scrcpy component (scrcpy-server + FFmpeg dlls). Empty = %LOCALAPPDATA%\SPLA\runtime\android\scrcpy.</summary>
    [YamlMember(Alias = "scrcpy_path")]
    public string ScrcpyPath { get; set; } = "";

    /// <summary>adb server port. 0 = the standard 5037; otherwise passed to adb.exe as ANDROID_ADB_SERVER_PORT.</summary>
    [YamlMember(Alias = "adb_server_port")]
    public int AdbServerPort { get; set; } = 0;

    /// <summary>Longest side of the streamed frame in pixels. 0 = the device's native resolution.</summary>
    [YamlMember(Alias = "max_size")]
    public int MaxSize { get; set; } = 1280;

    /// <summary>Maximum frame rate of the scrcpy stream.</summary>
    [YamlMember(Alias = "max_fps")]
    public int MaxFps { get; set; } = 30;

    /// <summary>Video bit rate of the scrcpy stream in bits per second.</summary>
    [YamlMember(Alias = "video_bit_rate")]
    public int VideoBitRate { get; set; } = 8_000_000;

    /// <summary>Keep the screen on while a session is connected.</summary>
    [YamlMember(Alias = "stay_awake")]
    public bool StayAwake { get; set; } = true;

    /// <summary>Action tools (tap, swipe, …) return a fresh screenshot after the screen settles.</summary>
    [YamlMember(Alias = "screenshot_after_action")]
    public bool ScreenshotAfterAction { get; set; } = true;

    /// <summary>How long the screen must be unchanged before it counts as settled.</summary>
    [YamlMember(Alias = "settle_ms")]
    public int SettleMs { get; set; } = 300;

    /// <summary>How long to wait for the screen to settle at most.</summary>
    [YamlMember(Alias = "settle_timeout_ms")]
    public int SettleTimeoutMs { get; set; } = 3000;

    /// <summary>Drop a chat's device lease after this many minutes without a tool call.</summary>
    [YamlMember(Alias = "idle_disconnect_minutes")]
    public int IdleDisconnectMinutes { get; set; } = 15;

    public static AndroidSettings FromBlob(Dictionary<string, object>? blob)
    {
        if (blob is null || blob.Count == 0) return new();
        var settings = De.Deserialize<AndroidSettings>(Ser.Serialize(blob)) ?? new();
        settings.Clamp();
        return settings;
    }

    private void Clamp()
    {
        // 0 is a valid max_size ("native resolution"); every other value must be within 320–4096.
        MaxSize = MaxSize == 0 ? 0 : Clamp(MaxSize, 320, 4096);
        MaxFps = Clamp(MaxFps, 1, 120);
        SettleMs = Clamp(SettleMs, 0, 5000);
        SettleTimeoutMs = Clamp(SettleTimeoutMs, 0, 30000);
        IdleDisconnectMinutes = Clamp(IdleDisconnectMinutes, 1, 1440);
        VideoBitRate = Clamp(VideoBitRate, 500_000, 100_000_000);
        // 0 is the standard adb server port 5037; 1..1023 are reserved, so anything below 1024
        // that is not 0 snaps up to 1024.
        AdbServerPort = AdbServerPort switch
        {
            0 => 0,
            < 1024 => 1024,
            > 65535 => 65535,
            _ => AdbServerPort,
        };
    }

    private static int Clamp(int value, int min, int max) =>
        value < min ? min : value > max ? max : value;
}
