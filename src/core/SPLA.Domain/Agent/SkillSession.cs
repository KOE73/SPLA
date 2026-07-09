namespace SPLA.Domain.Agent;

/// <summary>
/// Default in-memory implementation of <see cref="ISkillSession"/>.
/// Not thread-safe — skill activation happens on the UI/agent thread.
/// </summary>
public sealed class SkillSession : ISkillSession
{
    public string? ActiveSkillId { get; private set; }

    public event EventHandler? Changed;

    public void Activate(string skillId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(skillId);
        if (ActiveSkillId is not null)
            throw new InvalidOperationException(
                $"Skill '{ActiveSkillId}' is already active. Call Deactivate() first.");

        ActiveSkillId = skillId;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Deactivate()
    {
        if (ActiveSkillId is null) return;
        ActiveSkillId = null;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
