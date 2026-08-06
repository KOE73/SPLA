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
    public ToolResult CheckpointSave(string? resume = null)
    {
        if (Target == null || CurrentAssistantMsg == null)
            return ToolResult.Refuse("error: no conversation attached", "no conversation");

        var label = Target.InsertLabelBefore(CurrentAssistantMsg, markName: null, resume: resume);
        _checkpointStack.Push(label.MsgId);
        return ToolResult.Text($"ok: checkpoint pushed at {label.MsgId} (stack depth: {_checkpointStack.Count})");
    }

    /// <summary>Pops the top checkpoint and signals the orchestrator to restore.</summary>
    public ToolResult ContextRollback()
    {
        if (_checkpointStack.Count == 0)
            return ToolResult.Refuse("error: no checkpoint saved — call checkpoint_save first", "no checkpoint");

        var labelId = _checkpointStack.Pop();
        SetRestore(labelId);
        return ToolResult.Text($"ok: rollback to {labelId} scheduled");
    }

    // ── Named marks ───────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts a label with <paramref name="name"/> before the current assistant message.
    /// If a label with this name already exists it is cleared first (named marks don't accumulate).
    /// </summary>
    public ToolResult MarkSet(string name, string? resume = null)
    {
        if (Target == null || CurrentAssistantMsg == null)
            return ToolResult.Refuse("error: no conversation attached", "no conversation");

        // Remove previous label with the same name to avoid accumulation.
        var old = Target.FindLabel(name);
        if (old != null)
        {
            old.Mark = null;   // detach name; the label itself is harmless (tiny, not sent to LLM)
        }

        var label = Target.InsertLabelBefore(CurrentAssistantMsg, markName: name, resume: resume);
        return ToolResult.Text($"ok: mark '{name}' set at {label.MsgId}");
    }

    /// <summary>Finds the label carrying <paramref name="name"/> and requests restore.</summary>
    public ToolResult MarkRollback(string name)
    {
        if (Target == null)
            return ToolResult.Refuse("error: no conversation attached", "no conversation");

        var label = Target.FindLabel(name);
        if (label == null)
            return ToolResult.Fail($"error: mark '{name}' not found — it may have been deleted", "mark not found");

        SetRestore(label.MsgId);
        return ToolResult.Text($"ok: rollback to mark '{name}' ({label.MsgId}) scheduled");
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
