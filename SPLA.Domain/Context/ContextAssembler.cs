using SPLA.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.Domain.Context;

/// <summary>
/// Pure rules for deciding what the model actually sees. This is the single home of context
/// assembly: which messages are sent, and how retention policies prune the history. It operates
/// only on <see cref="ChatMessage"/> — no UI view-models, no render-type checks — so the same
/// behaviour is shared by every entry point and is fully unit-testable.
/// </summary>
public static class ContextAssembler
{
    /// <summary>
    /// Returns true if <paramref name="message"/> should be sent to the model at all,
    /// independent of retention. Ephemeral notices, empty messages and orphan tool results
    /// are excluded.
    /// </summary>
    public static bool ShouldSend(ChatMessage message)
    {
        if (message.IsEphemeral) return false;

        // An assistant turn that only carries tool calls has empty content but must be sent.
        var hasToolCalls = message.ToolCalls != null && message.ToolCalls.Count > 0;
        if (string.IsNullOrWhiteSpace(message.Content) && !hasToolCalls) return false;

        // A tool result with no id cannot be correlated by the model.
        if (message.Role == ChatRole.Tool && string.IsNullOrWhiteSpace(message.ToolCallId)) return false;

        return true;
    }

    /// <summary>
    /// Builds the message list to send to the model from the full conversation, applying
    /// <see cref="ContextRetention"/>:
    /// <list type="bullet">
    /// <item><see cref="ContextRetention.Never"/> — dropped.</item>
    /// <item><see cref="ContextRetention.UntilSuperseded"/> — only the most recent message per
    /// <see cref="ChatMessage.ReplacementKey"/> survives.</item>
    /// </list>
    /// Iterates newest-first so "superseded" keeps the latest occurrence, then restores order.
    /// </summary>
    public static List<ChatMessage> Assemble(IEnumerable<ChatMessage> conversation)
    {
        var result = new List<ChatMessage>();
        var supersededKeys = new HashSet<string>();

        foreach (var m in conversation.Reverse())
        {
            if (!ShouldSend(m)) continue;
            if (m.RetentionPolicy == ContextRetention.Never) continue;

            if (m.RetentionPolicy == ContextRetention.UntilSuperseded && !string.IsNullOrEmpty(m.ReplacementKey))
            {
                if (!supersededKeys.Add(m.ReplacementKey))
                    continue;
            }

            result.Insert(0, m);
        }

        return result;
    }
}
