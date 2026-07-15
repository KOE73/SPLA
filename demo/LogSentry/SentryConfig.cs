using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Demo.LogSentry;

/// <summary>The demo's own <c>sentry:</c> section of the .spla file (the standard SPLA loader
/// ignores unknown sections — one project file stays the single entry point).</summary>
public sealed class SentryConfig
{
    public string LogFile { get; set; } = "app.log";
    public List<string> Triggers { get; set; } = new();
    public double BatchSeconds { get; set; } = 10;
    public int MaxLines { get; set; } = 40;
    public int KeepHistory { get; set; } = 3;

    public static SentryConfig Load(string splaFile)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        var doc = deserializer.Deserialize<Envelope>(File.ReadAllText(splaFile));
        return doc?.Sentry ?? new SentryConfig();
    }

    private sealed class Envelope
    {
        public SentryConfig? Sentry { get; set; }
    }
}
