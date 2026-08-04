using System.Collections.Generic;

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

    /// <summary>Address of the active skill's source, or <c>null</c> when idle or when the caller did
    /// not supply one. Half of the loan slip: it says which shelf the attachments come off.</summary>
    string? ActiveSourceId { get; }

    /// <summary>The active skill's ref within its source, opaque here as everywhere outside that
    /// source. The other half of the loan slip.</summary>
    string? ActiveRef { get; }

    /// <summary>The attachments this skill came with, as listed at activation. Empty when idle or when
    /// the skill carries none.
    /// <para>Only the LIST is pinned, not the contents: a procedure reads two of its references and
    /// ignores a dozen templates, so snapshotting them all at activation would charge for what nobody
    /// opens. The text is fetched from the source at the moment it is asked for — and if the source is
    /// gone by then, the read fails honestly while the pinned procedure keeps running.</para></summary>
    IReadOnlyList<string> ActiveResources { get; }

    /// <summary>Transitions Idle → Active, pinning <paramref name="body"/> for the run.
    /// The trailing arguments are the loan slip — where this skill came from and what came with it —
    /// and are optional so a caller with nothing to attach stays a one-liner.
    /// Throws if another skill is already active.</summary>
    void Activate(string skillId, string body, string? sourceId = null, string? skillRef = null,
        IReadOnlyList<string>? resources = null);

    /// <summary>Transitions Active → Idle. No-op if already idle.</summary>
    void Deactivate();

    /// <summary>Raised after every state transition.</summary>
    event EventHandler? Changed;
}
