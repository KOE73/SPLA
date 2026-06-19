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
    /// <summary>Sequential position in the current context list (1-based, for display only).</summary>
    public int Index { get; init; }

    /// <summary>Stable message identity, e.g. U-3, A-4. May include mark suffix like [loop-start].</summary>
    public string MsgIdLabel { get; }

    /// <summary>True when this message is actually sent to the LLM in the last request.</summary>
    public bool InContext { get; init; } = true;

    /// <summary>True in "All" mode for messages that exist in history but were NOT sent to the LLM.</summary>
    public bool IsDimmed => !InContext;

    public ContextLineViewModel(ChatMessage msg)
    {
        MsgIdLabel = string.IsNullOrEmpty(msg.Mark) ? msg.MsgId : $"{msg.MsgId} [{msg.Mark}]";
        Source = DescribeSource(msg);
        Full = BuildFull(msg);
        Preview = Flatten(Full);
        WordCount = string.IsNullOrWhiteSpace(Full) ? 0
            : Full.Split((char[]?)null, System.StringSplitOptions.RemoveEmptyEntries).Length;
        ApproxTokens = (Full.Length + 3) / 4;
    }

    /// <summary>Short label for the origin of this context entry (system / user / tool-result …).</summary>
    public string Source { get; }

    /// <summary>Single-line, newline-free start of the entry's text.</summary>
    public string Preview { get; }

    /// <summary>Full text (shown on hover via tooltip).</summary>
    public string Full { get; }

    /// <summary>Approximate word count of the full entry.</summary>
    public int WordCount { get; }

    /// <summary>Approximate token count (~chars/4).</summary>
    public int ApproxTokens { get; }

    /// <summary>Short display string for the token column.</summary>
    public string TokenLabel => $"~{ApproxTokens} tok";

    private static string DescribeSource(ChatMessage m)
    {
        if (m.IsLabel) return string.IsNullOrEmpty(m.Mark) ? "label" : $"label [{m.Mark}]";
        return m.Role switch
        {
            ChatRole.System when (m.Content ?? "").StartsWith("--- Working memory", System.StringComparison.Ordinal)
                => "working-memory",
            ChatRole.System   => "system",
            ChatRole.User     => "user",
            ChatRole.Assistant when m.ToolCalls is { Count: > 0 } => "assistant→tool",
            ChatRole.Assistant => "assistant",
            ChatRole.Tool      => "tool-result",
            _ => m.Role.ToString().ToLowerInvariant()
        };
    }

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
