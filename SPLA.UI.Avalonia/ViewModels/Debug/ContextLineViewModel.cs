using CommunityToolkit.Mvvm.ComponentModel;
using SPLA.Agent;
using SPLA.Domain.Models;

namespace SPLA.UI.Avalonia.ViewModels.Debug;

/// <summary>
/// One line in the "real context" debug window: where the entry came from plus a single-line
/// preview of its text. Mirrors the minimalist KV-debug row (source ≈ key, preview ≈ value).
/// </summary>
public sealed class ContextLineViewModel
{
    public ContextLineViewModel(ChatMessage msg)
    {
        Source = DescribeSource(msg);
        Full = BuildFull(msg);
        Preview = Flatten(Full);
    }

    /// <summary>Short label for the origin of this context entry (system / user / tool-result …).</summary>
    public string Source { get; }

    /// <summary>Single-line, newline-free start of the entry's text.</summary>
    public string Preview { get; }

    /// <summary>Full text (shown on hover via tooltip).</summary>
    public string Full { get; }

    private static string DescribeSource(ChatMessage m) => m.Role switch
    {
        ChatRole.System when (m.Content ?? "").StartsWith("--- Working memory", System.StringComparison.Ordinal)
            => "working-memory",
        ChatRole.System => "system",
        ChatRole.User => "user",
        ChatRole.Assistant when m.ToolCalls is { Count: > 0 } => "assistant→tool",
        ChatRole.Assistant => "assistant",
        ChatRole.Tool => "tool-result",
        _ => m.Role.ToString().ToLowerInvariant()
    };

    private static string BuildFull(ChatMessage m)
    {
        if (m.ToolCalls is { Count: > 0 })
        {
            var calls = string.Join(", ", System.Linq.Enumerable.Select(
                m.ToolCalls, t => $"{t.Function.Name}({t.Function.Arguments})"));
            var head = string.IsNullOrEmpty(m.Content) ? "" : m.Content + " ";
            return $"{head}[tools: {calls}]";
        }
        return m.Content ?? string.Empty;
    }

    private static string Flatten(string text)
    {
        if (string.IsNullOrEmpty(text)) return "(empty)";
        var oneLine = text.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Trim();
        while (oneLine.Contains("  ")) oneLine = oneLine.Replace("  ", " ");
        return oneLine;
    }
}
