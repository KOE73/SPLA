namespace SPLA.Domain.Interfaces;

/// <summary>
/// Optional capability for an <see cref="ILLMService"/> provider that can report real token usage
/// (as opposed to a client-side estimate). It is intentionally <b>separate</b> from the core service
/// contract: a provider that has no usage information simply does not implement this interface, and
/// consumers detect support with a type check —
/// <c>if (llm is ITokenUsageReporter { SupportsUsage: true } r) …</c>.
/// <para>
/// The per-message numbers themselves travel on <see cref="Models.ChatMessage.PromptTokens"/> /
/// <see cref="Models.ChatMessage.CompletionTokens"/> (null when not reported); this interface only
/// advertises whether a provider populates them, so UIs can choose between "real" and "estimated"
/// presentation without referencing any concrete provider type.
/// </para>
/// </summary>
public interface ITokenUsageReporter
{
    /// <summary>True when this provider populates the token counts on returned messages.</summary>
    bool SupportsUsage { get; }
}
