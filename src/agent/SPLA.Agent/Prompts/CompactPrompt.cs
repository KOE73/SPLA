using System.IO;
using System.Text;

namespace SPLA.Agent;

/// <summary>
/// The single English prompt sent to the model for <c>/compact</c> — see
/// <c>docs/adr/ADR_20260911-3_agent_compaction.md</c> §2.4. Embedded the same way
/// <see cref="CoreFeaturePrompts"/> embeds a feature's <c>prompt.md</c>: read once, cached, never a
/// runtime file dependency.
/// </summary>
public static class CompactPrompt
{
    private const string ResourceName = "SPLA.Agent.Prompts.compact.md";

    private static string? _cached;

    /// <summary>The compaction prompt text, trimmed. Loaded once per process.</summary>
    public static string Text
    {
        get
        {
            if (_cached != null) return _cached;
            var assembly = typeof(CompactPrompt).Assembly;
            using var stream = assembly.GetManifestResourceStream(ResourceName);
            if (stream == null) return _cached = string.Empty;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return _cached = reader.ReadToEnd().Trim();
        }
    }
}
