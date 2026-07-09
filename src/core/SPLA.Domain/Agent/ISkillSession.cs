namespace SPLA.Domain.Agent;

/// <summary>
/// Tracks the single active skill for a conversation. At most one skill is active at a time.
/// State transitions happen only through <see cref="Activate"/> and <see cref="Deactivate"/> —
/// never by writing to KV or manipulating the prompt directly.
/// <para>
/// <see cref="Changed"/> fires on every transition; the assembler and UI subscribe to rebuild
/// the prompt and toggle the "Unload skill" button.
/// </para>
/// </summary>
public interface ISkillSession
{
    /// <summary>Id of the currently active skill, or <c>null</c> when idle.</summary>
    string? ActiveSkillId { get; }

    /// <summary>Transitions Idle → Active. Throws if another skill is already active.</summary>
    void Activate(string skillId);

    /// <summary>Transitions Active → Idle. No-op if already idle.</summary>
    void Deactivate();

    /// <summary>Raised after every state transition.</summary>
    event EventHandler? Changed;
}
