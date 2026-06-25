namespace SPLA.Domain.Models;

/// <summary>
/// A running tally of real (provider-reported) token usage. Used for both the live per-chat counter
/// and the persistent project-lifetime accumulator. Every value is a sum across turns — never an
/// estimate — and <see cref="Turns"/> counts how many LLM responses contributed.
/// </summary>
public sealed class TokenUsageTotals
{
    /// <summary>Total input/prompt tokens sent to the model across all counted turns.</summary>
    public long PromptTokens { get; set; }

    /// <summary>Total output/completion tokens produced by the model across all counted turns.</summary>
    public long CompletionTokens { get; set; }

    /// <summary>Number of LLM turns that contributed at least one real token figure.</summary>
    public long Turns { get; set; }

    /// <summary>Prompt + completion. Convenience for display.</summary>
    public long TotalTokens => PromptTokens + CompletionTokens;

    /// <summary>Folds one turn's reported usage in. Null figures are ignored (provider did not report).</summary>
    public void Add(int? prompt, int? completion)
    {
        var counted = false;
        if (prompt is int p) { PromptTokens += p; counted = true; }
        if (completion is int c) { CompletionTokens += c; counted = true; }
        if (counted) Turns++;
    }

    public TokenUsageTotals Clone() => new()
    {
        PromptTokens = PromptTokens,
        CompletionTokens = CompletionTokens,
        Turns = Turns
    };
}
