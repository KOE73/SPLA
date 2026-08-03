using System.Collections.Generic;
using System.Linq;

namespace SPLA.Domain.Agent;

/// <summary>How a tool set came to be raised in this chat. Recorded because "where did twelve new
/// tools come from in the middle of a chat" has to have an answer — it is written into the
/// composition manifest.</summary>
public enum ToolSetActivationBy
{
    /// <summary>A skill that declares the set in its requirements was activated.</summary>
    Skill,

    /// <summary>The model called <c>toolset_activate</c>.</summary>
    Agent,

    /// <summary>The person raised it from the UI.</summary>
    User
}

public readonly record struct ToolSetActivation(string SetId, ToolSetActivationBy By, string? Reason);

/// <summary>
/// Which tool sets are raised in one chat. The level (what the user allows at all) is a standing
/// setting and lives in the project; this is the other half — what is armed right now, here.
///
/// <para>Chat-scoped on purpose: parallel chats are a supported case with their own modes and
/// permission scopes, so "the agent raised ssh" must not leak into the chat next door. Same shape
/// and same lifetime as <see cref="ISkillSession"/>, and equally not persisted — a reopened chat
/// starts with nothing raised.</para>
/// </summary>
public interface IToolSetSession
{
    /// <summary>Sets raised in this chat, in the order they were raised.</summary>
    IReadOnlyList<ToolSetActivation> Active { get; }

    bool IsActive(string setId);

    /// <summary>Raises a set. Raising an already-raised set is a no-op and keeps the original
    /// reason — the first activation is the one that explains it.</summary>
    void Activate(string setId, ToolSetActivationBy by, string? reason = null);

    /// <summary>Lowers a set. No-op when it was not raised.</summary>
    /// <returns>True when something was actually lowered.</returns>
    bool Deactivate(string setId);

    /// <summary>Lowers every set raised by <paramref name="by"/> — how a finished skill takes its own
    /// sets back down without touching what the agent or the user raised.</summary>
    void DeactivateAllBy(ToolSetActivationBy by);

    event System.EventHandler? Changed;
}

/// <summary>Default in-memory implementation. Not thread-safe: a chat runs one turn at a time.</summary>
public sealed class ToolSetSession : IToolSetSession
{
    private readonly List<ToolSetActivation> _active = [];

    public IReadOnlyList<ToolSetActivation> Active => _active;

    public event System.EventHandler? Changed;

    public bool IsActive(string setId) =>
        _active.Any(a => string.Equals(a.SetId, setId, System.StringComparison.OrdinalIgnoreCase));

    public void Activate(string setId, ToolSetActivationBy by, string? reason = null)
    {
        System.ArgumentException.ThrowIfNullOrWhiteSpace(setId);
        if (IsActive(setId)) return;

        _active.Add(new ToolSetActivation(setId, by, reason));
        Changed?.Invoke(this, System.EventArgs.Empty);
    }

    public bool Deactivate(string setId)
    {
        var removed = _active.RemoveAll(a =>
            string.Equals(a.SetId, setId, System.StringComparison.OrdinalIgnoreCase));
        if (removed == 0) return false;

        Changed?.Invoke(this, System.EventArgs.Empty);
        return true;
    }

    public void DeactivateAllBy(ToolSetActivationBy by)
    {
        if (_active.RemoveAll(a => a.By == by) == 0) return;
        Changed?.Invoke(this, System.EventArgs.Empty);
    }
}
