using SPLA.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SPLA.Domain.Tools;

/// <summary>
/// The forest of every live <see cref="ProgressTree"/> in one chat, not just the current turn's.
/// <para>
/// Today <see cref="ConversationOrchestrator"/> makes one <see cref="ProgressTree"/> per turn and
/// hands it out once, through <c>AgentCallbacks.OnProgressTree</c> — a subscriber that was not
/// listening at that exact moment sees nothing for the rest of the turn, and sees nothing at all for
/// whatever runs between turns. A background task (see
/// <c>docs/adr/ADR_20260824-2_core_background-tool-calls.md</c>) needs its own root that outlives the
/// turn that started it — the whole reason this exists.
/// </para>
/// <para>
/// This does not replace <see cref="ProgressTree"/> or <see cref="ProgressScope"/>; it sits above
/// them. A tree is still created and populated exactly as before — the only change is that it is
/// <see cref="Register"/>ed here instead of (or alongside) being handed out once. A subscriber that
/// attaches to the hub after several trees are already running still sees all of them: past node
/// events are gone, but every currently-open root is visible immediately.
/// </para>
/// </summary>
public sealed class ProgressHub
{
    private readonly object _gate = new();
    private readonly Dictionary<string, ProgressTree> _trees = new(StringComparer.Ordinal);
    private int _seq;

    /// <summary>A new root joined the forest — a turn started, or a background task did.</summary>
    public event Action<string, ProgressTree>? TreeAdded;

    /// <summary>Something changed on a node in any tree registered here, tree id included so a
    /// subscriber that only wants one root can filter cheaply.</summary>
    public event Action<string, ProgressNode>? NodeChanged;

    /// <summary>Every tree currently registered, keyed by the id <see cref="Register"/> returned.</summary>
    public IReadOnlyDictionary<string, ProgressTree> Trees
    {
        get { lock (_gate) return new Dictionary<string, ProgressTree>(_trees, StringComparer.Ordinal); }
    }

    /// <summary>
    /// Adds <paramref name="tree"/> as a new root and returns its id. The tree's own life is not
    /// otherwise touched — it keeps working exactly as it does today, this only makes it visible to
    /// hub subscribers as well as to whoever created it.
    /// </summary>
    public string Register(ProgressTree tree)
    {
        var id = "t" + Interlocked.Increment(ref _seq).ToString();
        lock (_gate) _trees[id] = tree;

        tree.NodeChanged += node => NodeChanged?.Invoke(id, node);
        TreeAdded?.Invoke(id, tree);
        return id;
    }

    /// <summary>Drops a finished root so <see cref="Trees"/> does not grow for the life of the chat.
    /// A tree already forgotten is a no-op — the caller does not have to track whether it ran first.</summary>
    public void Forget(string id)
    {
        lock (_gate) _trees.Remove(id);
    }
}
