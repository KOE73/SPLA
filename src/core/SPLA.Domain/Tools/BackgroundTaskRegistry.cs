using SPLA.Domain.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SPLA.Domain.Tools;

/// <summary>How a background task stands. Mirrors <see cref="ToolOutcome"/> for the finished states
/// (a task that failed is a task whose tool returned <see cref="ToolOutcome.Failed"/> or threw) plus
/// <see cref="Cancelled"/>, which has no equivalent there — nothing a synchronous call can do to
/// itself corresponds to being killed from outside mid-run.</summary>
public enum BackgroundTaskState
{
    Running,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// One detached call: what it is, whether it is still going, and what it produced once it is not.
/// <para>
/// <see cref="ArgumentsSummary"/>, not the raw JSON: this record is what a delivered-result message
/// and a <c>task_list</c> row both quote back at the model, and a truncated preview is what
/// <c>docs/adr/ADR_20260824-2_core_background-tool-calls.md</c> §4.5 asks for — enough to remind the
/// model what it asked for, never the whole thing again.
/// </para>
/// </summary>
public sealed class BackgroundTaskRecord
{
    private const int SummaryMaxChars = 200;

    public BackgroundTaskRecord(string id, string toolName, string argumentsJson, CancellationTokenSource cts)
    {
        Id = id;
        ToolName = toolName;
        ArgumentsSummary = Truncate(argumentsJson, SummaryMaxChars);
        StartedAt = DateTimeOffset.UtcNow;
        Cts = cts;
    }

    public string Id { get; }
    public string ToolName { get; }
    public string ArgumentsSummary { get; }
    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset? FinishedAt { get; internal set; }
    public BackgroundTaskState State { get; internal set; } = BackgroundTaskState.Running;
    public ToolResult? Result { get; internal set; }

    /// <summary>The task's own cancellation source, linked to the chat's lifetime — see
    /// <see cref="BackgroundTaskRegistry"/>. Cancelling this and nothing else is what a <c>task_cancel</c>
    /// call, or the chat closing, does; it is never the turn's own token, which would end when the
    /// turn that launched the task does and defeat the entire point of backgrounding it.</summary>
    public CancellationTokenSource Cts { get; }

    /// <summary>The progress tree id this task registered in the chat's <see cref="ProgressHub"/>, once
    /// known. Set by the caller that opened the tree — the record does not create one itself, since
    /// a tree may need to exist before the record does (to attach the running call's own root node).</summary>
    public string? ProgressTreeId { get; set; }

    private static string Truncate(string text, int max)
        => text.Length <= max ? text : text[..max] + "…";
}

/// <summary>
/// A chat's live and recently-finished detached calls. One per chat — see
/// <c>docs/adr/ADR_20260824-2_core_background-tool-calls.md</c>: "реестр задач — на чате, не на
/// процессе".
/// <para>
/// <b>Capped, not queued.</b> A cap that quietly queued excess launches would let a model that fires
/// off ten scans believe all ten are running; refusing the eleventh with a plain reason is the
/// honest answer, and it is a reason the model can act on ("wait for one to finish, or don't
/// background this one").
/// </para>
/// </summary>
public sealed class BackgroundTaskRegistry
{
    /// <summary>Live task cap per chat — ADR §2, "лимит живых задач на чат (предлагается 8)".</summary>
    public const int MaxLiveTasks = 8;

    private readonly ConcurrentDictionary<string, BackgroundTaskRecord> _tasks =
        new(StringComparer.Ordinal);
    private readonly CancellationToken _chatLifetime;
    private int _seq;

    /// <param name="chatLifetime">Cancelled when the owning chat is disposed. Every task's own token
    /// is linked to this one, which is what lets <c>ChatRuntime.Dispose</c> end every live task with
    /// a single cancel instead of having to know their ids.</param>
    public BackgroundTaskRegistry(CancellationToken chatLifetime = default) => _chatLifetime = chatLifetime;

    /// <summary>
    /// Reserves a slot and returns a fresh record, or a refusal reason when the chat already has
    /// <see cref="MaxLiveTasks"/> running. The returned record's <see cref="BackgroundTaskRecord.Cts"/>
    /// is linked to the chat's lifetime; the caller starts it and must eventually call
    /// <see cref="Finish"/>.
    /// </summary>
    public (BackgroundTaskRecord? record, string? refusal) TryStart(string toolName, string argumentsJson)
    {
        if (LiveCount >= MaxLiveTasks)
            return (null, $"This chat already has {MaxLiveTasks} background tasks running — " +
                          "wait for one to finish, or run this one in the foreground instead.");

        var id = "bg_" + Interlocked.Increment(ref _seq);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(_chatLifetime);
        var record = new BackgroundTaskRecord(id, toolName, argumentsJson, cts);
        _tasks[id] = record;
        return (record, null);
    }

    /// <summary>Marks a task done — successfully, with a failure, or cancelled — and records what it
    /// produced. Idempotent only in the sense that a second call overwrites the first; callers are
    /// expected to call this exactly once, from the one place a task's own run ends.</summary>
    public void Finish(string id, BackgroundTaskState state, ToolResult result)
    {
        if (!_tasks.TryGetValue(id, out var record)) return;
        record.State = state;
        record.Result = result;
        record.FinishedAt = DateTimeOffset.UtcNow;
    }

    public bool TryGet(string id, out BackgroundTaskRecord record) => _tasks.TryGetValue(id, out record!);

    /// <summary>Every task this chat knows about, running or finished, oldest first. Finished tasks
    /// are not pruned here — see <see cref="Forget"/> — so a result delivered once through the inbox
    /// can still be asked for again via <c>task_output</c>.</summary>
    public IReadOnlyList<BackgroundTaskRecord> All =>
        _tasks.Values.OrderBy(t => t.StartedAt).ToList();

    public int LiveCount => _tasks.Values.Count(t => t.State == BackgroundTaskState.Running);

    /// <summary>Requests cancellation of a live task. Returns false for an unknown id or one already
    /// finished — cancelling a task that is not running is not an error, just a no-op the caller
    /// should not mistake for success.</summary>
    public bool Cancel(string id)
    {
        if (!_tasks.TryGetValue(id, out var record)) return false;
        if (record.State != BackgroundTaskState.Running) return false;
        record.Cts.Cancel();
        return true;
    }

    /// <summary>Drops a finished task's record so <see cref="All"/> does not grow for the life of the
    /// chat. Never called on a running task — cancel it first.</summary>
    public void Forget(string id) => _tasks.TryRemove(id, out _);

    /// <summary>
    /// Cancels every currently-running task and returns what it actually cancelled — the caller (Stop,
    /// PLAN_20260825 wave C) has to be able to say what it stopped, not just that it stopped something.
    /// <para>
    /// Unlike <see cref="Cancel"/>'s single-id boolean, this snapshots <see cref="BackgroundTaskState.Running"/>
    /// tasks before signalling any of them: a task's own completion can race this call and flip its
    /// state between the check and the signal, and the returned list must reflect what was true at the
    /// moment cancellation was requested, not a half-updated view a concurrent Finish() left behind.
    /// </para>
    /// </summary>
    public IReadOnlyList<BackgroundTaskRecord> CancelAll()
    {
        // Oldest first — same ordering as All, so a report built from this list ("stopped bg_1, bg_2")
        // reads in the order the tasks were started rather than whatever the concurrent dictionary's
        // internal bucket order happens to be.
        var running = _tasks.Values.Where(t => t.State == BackgroundTaskState.Running)
            .OrderBy(t => t.StartedAt).ToList();
        foreach (var record in running) record.Cts.Cancel();
        return running;
    }
}
