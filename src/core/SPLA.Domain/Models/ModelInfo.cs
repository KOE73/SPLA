using System.Collections.Generic;

namespace SPLA.Domain.Models;

/// <summary>
/// Rich metadata about a single model exposed by a provider.
/// Populated from the LM Studio native REST API (<c>/api/v1/models</c>) when available,
/// otherwise degrades to just <see cref="Id"/> from the OpenAI-compatible <c>/v1/models</c>.
/// </summary>
public class ModelInfo
{
    /// <summary>Model identifier used in chat requests (LM Studio <c>key</c>, or OpenAI <c>id</c>).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Human-friendly name, when the provider supplies one.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Model kind: "llm", "vlm", "embedding", etc. Empty when unknown.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Architecture family (e.g. "qwen35moe", "gemma4"). Empty when unknown.</summary>
    public string Arch { get; set; } = string.Empty;

    /// <summary>Quantization scheme name (e.g. "Q4_K_M"). Empty when unknown.</summary>
    public string Quantization { get; set; } = string.Empty;

    /// <summary>Bits-per-weight of the quantization, or 0 when unknown.</summary>
    public int BitsPerWeight { get; set; }

    /// <summary>Parameter count label (e.g. "40B", "35B-A3B"). Empty when unknown.</summary>
    public string ParamsString { get; set; } = string.Empty;

    /// <summary>On-disk size in bytes, or 0 when unknown.</summary>
    public long SizeBytes { get; set; }

    /// <summary>Maximum context length in tokens, or 0 when unknown.</summary>
    public int MaxContextLength { get; set; }

    /// <summary>Whether at least one instance of the model is currently loaded.</summary>
    public bool IsLoaded { get; set; }

    /// <summary>Instance id to use when unloading (from loaded_instances); falls back to <see cref="Id"/>.</summary>
    public string LoadedInstanceId { get; set; } = string.Empty;

    /// <summary>The instance id to unload — the loaded instance id when known, otherwise the model key.</summary>
    public string UnloadId => string.IsNullOrEmpty(LoadedInstanceId) ? Id : LoadedInstanceId;

    /// <summary>Whether the model can accept images.</summary>
    public bool SupportsVision { get; set; }

    /// <summary>Whether the model advertises tool/function calling.</summary>
    public bool SupportsTools { get; set; }

    /// <summary>Allowed reasoning options advertised by the model (e.g. ["off","on"] or ["low","medium","high"]). Empty when the model has no reasoning channel.</summary>
    public List<string> ReasoningOptions { get; set; } = new();

    /// <summary>The model's default reasoning option, when advertised.</summary>
    public string ReasoningDefault { get; set; } = string.Empty;

    /// <summary>Whether the model produces a separate reasoning/thinking channel.</summary>
    public bool SupportsReasoning => ReasoningOptions.Count > 0;

    /// <summary>True for on/off style reasoning (a simple toggle); false for graded effort levels.</summary>
    public bool IsReasoningOnOff =>
        ReasoningOptions.Count > 0 && ReasoningOptions.TrueForAll(o =>
            string.Equals(o, "on", System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(o, "off", System.StringComparison.OrdinalIgnoreCase));

    /// <summary>True when the rich native metadata was available; false for the bare /v1/models fallback.</summary>
    public bool HasDetails { get; set; }

    // ── Convenience flags for the settings table (icon columns) ──
    public string VisionFlag => SupportsVision ? "🖼️" : "";
    public string ToolsFlag => SupportsTools ? "🔨" : "";
    public string ReasoningFlag => SupportsReasoning ? "🧠" : "";
    public string ContextDisplay => MaxContextLength > 0 ? $"{MaxContextLength / 1024}K" : "—";
    public string ParamsDisplay => string.IsNullOrEmpty(ParamsString) ? "—" : ParamsString;
    public string QuantDisplay => string.IsNullOrEmpty(Quantization) ? "—" : Quantization;
    public string SizeDisplay => SizeBytes > 0 ? $"{SizeBytes / 1_000_000_000.0:0.0} GB" : "—";
    public string StateDisplay => IsLoaded ? "● loaded" : "○";
}
