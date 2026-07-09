using SPLA.Domain.Models;
using System.Collections.Generic;

namespace SPLA.Domain.Agent;

/// <summary>
/// Shared state between <see cref="ConversationOrchestrator"/> and the context management tools.
///
/// Both mechanisms work by inserting an invisible Label message (IsLabel=true, L-* MsgId)
/// immediately BEFORE the assistant message that called the tool. The label is the rollback
/// anchor — TruncateTo(label) removes the tool call and everything after, leaving a clean
/// boundary. The LLM never sees labels (filtered by ContextAssembler).
///
/// 1. Unnamed checkpoint stack — checkpoint_save inserts a label and pushes its MsgId;
///    context_rollback pops the top MsgId and requests restore.
/// 2. Named marks — mark_set inserts a label with a name; mark_rollback finds the label
///    by name and requests restore. The same label is reused across iterations (no accumulation).
/// </summary>
public class MarkManager
{
    private readonly Stack<string> _checkpointStack = new();

    public Conversation?  Target        { get; set; }

    /// <summary>
    /// Set by the orchestrator to the assistant message currently being processed
    /// (before its tool calls are executed). Used by CheckpointSave / MarkSet to know
    /// where to insert the label.
    /// </summary>
    public ChatMessage? CurrentAssistantMsg { get; set; }

    // ── Rollback signal (read by orchestrator) ─────────────────────────────────

    public bool   RestoreRequested { get; private set; }
    public string? RestoreAnchorId  { get; private set; }
    public string? RestoreLabel     { get; private set; }  // mark name or null for unnamed
    public string? RestoreResume    { get; private set; }  // from label.Content

    // ── Unnamed checkpoint stack ───────────────────────────────────────────────

    /// <summary>
    /// Inserts a label before the current assistant message (from CurrentAssistantMsg) and pushes its MsgId.
    /// </summary>
    public string CheckpointSave(string? resume = null)
    {
        if (Target == null || CurrentAssistantMsg == null)
            return "error: no conversation attached";

        var label = Target.InsertLabelBefore(CurrentAssistantMsg, markName: null, resume: resume);
        _checkpointStack.Push(label.MsgId);
        return $"ok: checkpoint pushed at {label.MsgId} (stack depth: {_checkpointStack.Count})";
    }

    /// <summary>Pops the top checkpoint and signals the orchestrator to restore.</summary>
    public string ContextRollback()
    {
        if (_checkpointStack.Count == 0)
            return "error: no checkpoint saved — call checkpoint_save first";

        var labelId = _checkpointStack.Pop();
        SetRestore(labelId);
        return $"ok: rollback to {labelId} scheduled";
    }

    // ── Named marks ───────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts a label with <paramref name="name"/> before the current assistant message.
    /// If a label with this name already exists it is cleared first (named marks don't accumulate).
    /// </summary>
    public string MarkSet(string name, string? resume = null)
    {
        if (Target == null || CurrentAssistantMsg == null)
            return "error: no conversation attached";

        // Remove previous label with the same name to avoid accumulation.
        var old = Target.FindLabel(name);
        if (old != null)
        {
            old.Mark = null;   // detach name; the label itself is harmless (tiny, not sent to LLM)
        }

        var label = Target.InsertLabelBefore(CurrentAssistantMsg, markName: name, resume: resume);
        return $"ok: mark '{name}' set at {label.MsgId}";
    }

    /// <summary>Finds the label carrying <paramref name="name"/> and requests restore.</summary>
    public string MarkRollback(string name)
    {
        if (Target == null)
            return "error: no conversation attached";

        var label = Target.FindLabel(name);
        if (label == null)
            return $"error: mark '{name}' not found — it may have been deleted";

        SetRestore(label.MsgId);
        return $"ok: rollback to mark '{name}' ({label.MsgId}) scheduled";
    }

    // ── Orchestrator handshake ─────────────────────────────────────────────────

    /// <summary>Called by the orchestrator after applying the restore.</summary>
    public void Confirm()
    {
        RestoreRequested = false;
        RestoreAnchorId  = null;
        RestoreLabel     = null;
        RestoreResume    = null;
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void SetRestore(string labelId)
    {
        var label = Target?.Messages.LastOrDefault(m => m.MsgId == labelId);
        RestoreAnchorId  = labelId;
        RestoreLabel     = label?.Mark;
        RestoreResume    = string.IsNullOrWhiteSpace(label?.Content) ? null : label!.Content;
        RestoreRequested = true;
    }
}
