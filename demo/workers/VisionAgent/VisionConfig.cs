using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Demo.Vision;

/// <summary>The demo's own <c>vision:</c> section of the .spla file. The standard SPLA config
/// loader ignores unknown sections, so the demo re-reads the same file for its private knobs —
/// one project file stays the single entry point, exactly like every other SPLA app.</summary>
public sealed class VisionConfig
{
    /// <summary>Video source: <c>usb:N</c> (camera index), <c>rtsp://…</c>, or a file path.</summary>
    public string Source { get; set; } = "usb:0";

    public double IntervalSeconds { get; set; } = 5;

    /// <summary>Frames wider than this are downscaled before upload (0 = keep original size).</summary>
    public int MaxWidth { get; set; } = 1024;

    public int JpegQuality { get; set; } = 80;

    /// <summary>How many previous frame turns stay in the chat context (0 = each frame standalone).</summary>
    public int KeepHistory { get; set; } = 0;

    /// <summary>Directory to save a copy of each sent frame into (empty = don't save).</summary>
    public string SaveFrames { get; set; } = "";

    /// <summary>Show two preview windows: the continuous live feed and the frame just sent
    /// to the model. Off by default — the demo stays fully headless.</summary>
    public bool ShowWindows { get; set; } = false;

    /// <summary>Stream the model's chain-of-thought to the console (dimmed, via Spectre.Console).
    /// Off by default: on a weak local model the reasoning is long and slows the visible answer.</summary>
    public bool ShowReasoning { get; set; } = false;

    public static VisionConfig Load(string splaFile)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        var doc = deserializer.Deserialize<VisionFileEnvelope>(File.ReadAllText(splaFile));
        return doc?.Vision ?? new VisionConfig();
    }

    private sealed class VisionFileEnvelope
    {
        public VisionConfig? Vision { get; set; }
    }
}
