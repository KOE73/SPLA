namespace SPLA.Domain.Agent;

/// <summary>
/// Tracks the single active skill for a conversation. At most one skill is active at a time.
/// State transitions happen only through <see cref="Activate"/> and <see cref="Deactivate"/> —
/// never by writing to KV or manipulating the prompt directly.
/// <para>
/// The procedure text is captured at activation and held here for the run's duration. That is what
/// pins a running skill: the source folder is watched and reloads freely, but editing the file of a
/// skill that is mid-run cannot swap the procedure out from under the model. The new text takes
/// effect at the next activation.
/// </para>
/// <para>
/// <see cref="Changed"/> fires on every transition; the assembler and UI subscribe to rebuild
/// the prompt and toggle the "Unload skill" button.
/// </para>
/// </summary>
public interface ISkillSession
{
    /// <summary>Id of the currently active skill, or <c>null</c> when idle.</summary>
    string? ActiveSkillId { get; }

    /// <summary>The active skill's procedure text as captured at activation, or <c>null</c> when
    /// idle. Read by the prompt assembler; never re-fetched from the source while active.</summary>
    string? ActiveBody { get; }

    /// <summary>Transitions Idle → Active, pinning <paramref name="body"/> for the run.
    /// Throws if another skill is already active.</summary>
    void Activate(string skillId, string body);

    /// <summary>Transitions Active → Idle. No-op if already idle.</summary>
    void Deactivate();

    /// <summary>Raised after every state transition.</summary>
    event EventHandler? Changed;
}
