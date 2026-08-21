namespace SPLA.Domain.Project;

/// <summary>
/// What an instance is doing, in the one vocabulary that serves both the badge a person sees and the
/// rule that decides whether the instance may be evicted. Two lists would drift, and the drift would
/// show up as an instance quietly killed while somebody was walking back to their desk.
/// </summary>
public enum InstanceState
{
    /// <summary>No unfinished turn. The only state an instance may be evicted from — nothing here is
    /// unique to the process: chats, KV and the usage tally are on disk, so a restart costs warm-up,
    /// not work.</summary>
    Idle,

    /// <summary>A turn is in flight and moving.</summary>
    Working,

    /// <summary>Blocked on a person: a permission or a clarification. Never evicted — the whole point
    /// of the question outliving its window is that somebody can come back and answer it.</summary>
    Waiting,

    /// <summary>A turn is registered but nothing has happened in it for a long time — a small model
    /// that stopped halfway. Never evicted either, and for a sharper reason: what it is holding is
    /// in memory, and the log does not always have all of it.</summary>
    Stalled,

    /// <summary>Said of an instance by an observer that has lost contact with it. Never a state an
    /// instance reports about itself.</summary>
    Unreachable
}

/// <summary>Naming for <see cref="InstanceState"/> on the wire and in a lock file.</summary>
public static class InstanceStates
{
    public static string Name(InstanceState state) => state switch
    {
        InstanceState.Idle => "idle",
        InstanceState.Working => "working",
        InstanceState.Waiting => "waiting",
        InstanceState.Stalled => "stalled",
        _ => "unreachable"
    };

    public static bool TryParse(string? name, out InstanceState state)
    {
        switch (name?.Trim().ToLowerInvariant())
        {
            case "idle": state = InstanceState.Idle; return true;
            case "working": state = InstanceState.Working; return true;
            case "waiting": state = InstanceState.Waiting; return true;
            case "stalled": state = InstanceState.Stalled; return true;
            case "unreachable": state = InstanceState.Unreachable; return true;
            default: state = InstanceState.Unreachable; return false;
        }
    }

    /// <summary>Whether an instance in this state may be shut down for being unused.</summary>
    public static bool MayEvict(InstanceState state) => state == InstanceState.Idle;
}
