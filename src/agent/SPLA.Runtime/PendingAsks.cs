using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;

namespace SPLA.Runtime;

/// <summary>What the agent is waiting for an answer to.</summary>
public enum PendingAskKind
{
    /// <summary>A tool call needs a permission decision.</summary>
    Permission,

    /// <summary>The agent asked the person a question and cannot proceed without a choice.</summary>
    Clarify
}

/// <summary>
/// One outstanding question from a running turn, in the form a client needs to render it.
/// </summary>
/// <param name="RequestId">Correlation id; the answer comes back carrying it.</param>
/// <param name="ChatId">The chat whose turn is blocked. A dialog must never appear over another chat.</param>
public sealed record PendingAsk(
    string RequestId,
    string ChatId,
    PendingAskKind Kind,
    string? ToolName,
    string? Arguments,
    string? Question,
    IReadOnlyList<ClarifyOption>? Options,
    DateTimeOffset CreatedAt);

/// <summary>Why an outstanding question stopped being outstanding — carried to every client so a
/// dialog that someone else already answered disappears instead of lingering.</summary>
public enum AskResolution
{
    /// <summary>A person answered it.</summary>
    Answered,

    /// <summary>The turn was cancelled or the runtime is going away.</summary>
    Cancelled,

    /// <summary>Nobody answered within the configured window.</summary>
    TimedOut
}

/// <summary>
/// The questions a running turn is waiting on, held by the PROJECT's runtime rather than by the
/// connection that started the turn.
///
/// <para>This is the difference between "the window that asked went away" and "there is nobody to
/// ask": previously both pending queues lived on <c>ClientConnection</c>, so closing a window
/// answered every outstanding question with <see cref="PermissionDecision.Deny"/> and the turn
/// finished by refusing everything it had been about to do. A question belongs to the chat; any
/// client watching that chat may answer it, and a client that arrives later must still see it.</para>
///
/// <para>The wait is bounded but generously: a person who walked away should be able to come back
/// and answer, so the timeout is measured in tens of minutes and exists only so an unattended
/// instance cannot block forever. Timing out denies — the one safe answer when nobody is there —
/// and says so, rather than pretending a person decided.</para>
/// </summary>
public sealed class PendingAskStore
{
    private sealed record Entry(PendingAsk Ask, Action<AskResolution> Abandon)
    {
        public TaskCompletionSource<PermissionDecision>? Permission { get; init; }
        public TaskCompletionSource<string?>? Clarify { get; init; }
    }

    private readonly ConcurrentDictionary<string, Entry> _entries = new();

    /// <summary>How long an unanswered question is kept before it is denied. <see cref="TimeSpan.Zero"/>
    /// (or negative) means no limit — the turn waits for a person indefinitely.</summary>
    public TimeSpan Timeout { get; }

    public PendingAskStore(TimeSpan timeout) => Timeout = timeout;

    /// <summary>Raised when a new question appears. The host fans it out to every client watching the
    /// chat — the store itself knows nothing about sockets.</summary>
    public event Action<PendingAsk>? Asked;

    /// <summary>Raised when a question is no longer outstanding, whoever closed it.</summary>
    public event Action<PendingAsk, AskResolution>? Resolved;

    /// <summary>True while any chat of this project is blocked on a person — the <c>waiting</c> state.</summary>
    public bool Any => !_entries.IsEmpty;

    /// <summary>Outstanding questions, optionally narrowed to one chat. Used to replay them to a
    /// client that connected (or opened the chat) after the question was asked.</summary>
    public IReadOnlyList<PendingAsk> List(string? chatId = null)
        => _entries.Values
            .Select(e => e.Ask)
            .Where(a => chatId == null || a.ChatId == chatId)
            .OrderBy(a => a.CreatedAt)
            .ToList();

    public async Task<PermissionDecision> AskPermissionAsync(
        string chatId, ToolFunctionDefinition def, string arguments, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<PermissionDecision>(TaskCreationOptions.RunContinuationsAsynchronously);
        var ask = NewAsk(chatId, PendingAskKind.Permission, toolName: def.Name, arguments: arguments,
            question: null, options: null);

        var entry = new Entry(ask, _ => tcs.TrySetResult(PermissionDecision.Deny)) { Permission = tcs };
        return await AwaitAsync(entry, tcs.Task, ct);
    }

    public async Task<string?> AskClarifyAsync(string chatId, ClarifyRequest request, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var ask = NewAsk(chatId, PendingAskKind.Clarify, toolName: null, arguments: null,
            question: request.Question, options: request.Options);

        var entry = new Entry(ask, _ => tcs.TrySetResult(null)) { Clarify = tcs };
        return await AwaitAsync(entry, tcs.Task, ct);
    }

    /// <summary>Answers a permission question. Returns false when the id is unknown — which is the
    /// normal outcome for the second client to click, not an error.</summary>
    public bool CompletePermission(string requestId, PermissionDecision decision)
    {
        if (!_entries.TryRemove(requestId, out var entry) || entry.Permission is null) return false;
        entry.Permission.TrySetResult(decision);
        Resolved?.Invoke(entry.Ask, AskResolution.Answered);
        return true;
    }

    /// <summary>Answers a clarify question. <paramref name="choice"/> null = "you decide".</summary>
    public bool CompleteClarify(string requestId, string? choice)
    {
        if (!_entries.TryRemove(requestId, out var entry) || entry.Clarify is null) return false;
        entry.Clarify.TrySetResult(choice);
        Resolved?.Invoke(entry.Ask, AskResolution.Answered);
        return true;
    }

    private PendingAsk NewAsk(
        string chatId, PendingAskKind kind, string? toolName, string? arguments,
        string? question, IReadOnlyList<ClarifyOption>? options)
        => new(Guid.NewGuid().ToString("N"), chatId, kind, toolName, arguments, question, options,
            DateTimeOffset.UtcNow);

    /// <summary>Publishes the question, waits for whichever comes first — an answer, cancellation of
    /// the turn, or the timeout — and guarantees the entry is gone afterwards.</summary>
    private async Task<T> AwaitAsync<T>(Entry entry, Task<T> answer, CancellationToken ct)
    {
        _entries[entry.Ask.RequestId] = entry;
        Asked?.Invoke(entry.Ask);

        using var registration = ct.Register(() => Abandon(entry.Ask.RequestId, AskResolution.Cancelled));

        if (Timeout > TimeSpan.Zero)
        {
            using var timer = new CancellationTokenSource(Timeout);
            using var onTimeout = timer.Token.Register(() => Abandon(entry.Ask.RequestId, AskResolution.TimedOut));
            return await answer;
        }

        return await answer;
    }

    /// <summary>Closes a question nobody answered. Idempotent: whoever gets there first wins, and the
    /// losers see an unknown id.</summary>
    private void Abandon(string requestId, AskResolution reason)
    {
        if (!_entries.TryRemove(requestId, out var entry)) return;
        entry.Abandon(reason);
        Resolved?.Invoke(entry.Ask, reason);
    }

    /// <summary>Closes everything outstanding — used when the runtime itself is going away.</summary>
    public void AbandonAll(AskResolution reason)
    {
        foreach (var id in _entries.Keys.ToList()) Abandon(id, reason);
    }
}

/// <summary>
/// The turns currently running in this project, keyed by chat. Lives on the runtime for the same
/// reason <see cref="PendingAskStore"/> does: a turn is the chat's, not the connection's. Closing
/// the window that started a turn used to cancel it, which is exactly the behaviour the instance
/// lease model forbids — the work outlives the client, and any client may cancel it.
/// </summary>
public sealed class TurnRegistry
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _turns = new();
    private readonly ConcurrentDictionary<string, long> _activity = new();
    private long _lastActivityTicks = DateTime.UtcNow.Ticks;

    /// <summary>When something last actually happened in a turn — a token, a tool call, a result.
    /// A turn that is registered but silent for a long time is a model that stopped halfway, which
    /// is a different thing from a turn doing work, and the difference decides whether the instance
    /// may be evicted.</summary>
    public DateTime LastActivityUtc => new(Volatile.Read(ref _lastActivityTicks), DateTimeKind.Utc);

    /// <summary>Records that a turn is still moving. Kept per chat as well as for the project,
    /// because "stopped halfway" is a thing a person sees on one chat in a sidebar, not only a
    /// property of the whole instance.</summary>
    public void Touch(string? chatId = null)
    {
        var now = DateTime.UtcNow.Ticks;
        Volatile.Write(ref _lastActivityTicks, now);
        if (chatId is not null) _activity[chatId] = now;
    }

    /// <summary>When this chat's turn last did anything. Null when no turn is registered for it.</summary>
    public DateTime? LastActivityFor(string chatId)
        => _turns.ContainsKey(chatId) && _activity.TryGetValue(chatId, out var ticks)
            ? new DateTime(ticks, DateTimeKind.Utc)
            : _turns.ContainsKey(chatId) ? DateTime.UtcNow : null;

    /// <summary>Registers a turn's cancellation source. Replaces any stale entry for the chat — the
    /// chat's own gate already serialises turns, so a second entry means the first one is finished.</summary>
    public void Register(string chatId, CancellationTokenSource cts)
    {
        _turns[chatId] = cts;
        Touch(chatId);
    }

    public void Remove(string chatId, CancellationTokenSource cts)
    {
        if (_turns.TryGetValue(chatId, out var current) && ReferenceEquals(current, cts))
        {
            _turns.TryRemove(chatId, out _);
            _activity.TryRemove(chatId, out _);
        }
    }

    /// <summary>Cancels the chat's running turn, whoever asked. False = nothing was running.</summary>
    public bool TryCancel(string chatId)
    {
        if (!_turns.TryGetValue(chatId, out var cts)) return false;
        cts.Cancel();
        return true;
    }

    /// <summary>Cancels every running turn in this project. Only for a forced instance stop: the normal
    /// refusal path (<see cref="Any"/> plus <c>SPLA.Domain.Project.InstanceStates.MayEvict</c>) exists
    /// precisely so this is never reached by accident.</summary>
    public void CancelAll()
    {
        foreach (var cts in _turns.Values) cts.Cancel();
    }

    /// <summary>True while any turn of this project is in flight — the <c>working</c> state.</summary>
    public bool Any => !_turns.IsEmpty;

    public IReadOnlyList<string> BusyChatIds => _turns.Keys.ToList();
}
