namespace SPLA.Domain.Agent;

/// <summary>
/// Default in-memory implementation of <see cref="ISkillSession"/>.
/// Not thread-safe — skill activation happens on the UI/agent thread.
/// <para>Deliberately not persisted: a chat that reopens starts idle. An active skill is the state
/// of a procedure in flight, and nothing of that run survives a restart, so restoring the flag alone
/// would only reproduce the wedge (index suppressed, activation refused) with no way out.</para>
/// </summary>
public sealed class SkillSession : ISkillSession
{
    public string? ActiveSkillId { get; private set; }

    public string? ActiveBody { get; private set; }

    public string? ActiveSourceId { get; private set; }

    public string? ActiveRef { get; private set; }

    public IReadOnlyList<string> ActiveResources { get; private set; } = [];

    public event EventHandler? Changed;

    public void Activate(string skillId, string body, string? sourceId = null, string? skillRef = null,
        IReadOnlyList<string>? resources = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId);
        if (ActiveSkillId is not null)
            throw new InvalidOperationException(
                $"Skill '{ActiveSkillId}' is already active. Call Deactivate() first.");

        ActiveSkillId = skillId;
        ActiveBody = body;
        ActiveSourceId = sourceId;
        ActiveRef = skillRef;
        ActiveResources = resources ?? [];
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Deactivate()
    {
        if (ActiveSkillId is null) return;
        ActiveSkillId = null;
        ActiveBody = null;

        // The loan slip goes back with the book. Leaving it would leave skill_read_resource able to
        // serve the attachments of a skill nobody is running.
        ActiveSourceId = null;
        ActiveRef = null;
        ActiveResources = [];
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
