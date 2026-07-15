using System;

namespace SPLA.Agent.Guards;

/// <summary>
/// Detects the machine-gun tool loop — the small-local-model failure mode where identical calls
/// fly back-to-back with no thinking in between. A repeat only counts as suspicious when ALL hold:
/// same tool + same arguments, same result as last time, no accompanying assistant text/reasoning,
/// and it arrived fast. Any changed result, any commentary, or a slow round (a long-running tool
/// like ssh_session_wait, or real model thinking) resets the streak — deliberate polling is
/// legitimate and must not trip the guard.
///
/// <para>Escalation is two-stage (the orchestrator drives it): the first trip CHALLENGES the model
/// in-band ("are you stuck?") instead of killing the turn; only a second textless streak after the
/// challenge hard-stops. Replaces the old windowed HasToolCallLoop/HasErrorLoop queues, which
/// counted every identical call regardless of timing or results.</para>
/// </summary>
public sealed class ToolRepeatTracker
{
    /// <summary>Rounds slower than this (previous result → this result) are considered deliberate:
    /// covers long tool runs and long model thinking alike.</summary>
    public static readonly TimeSpan FastRound = TimeSpan.FromSeconds(10);

    private string? _lastKey;
    private int _lastResultHash;
    private DateTimeOffset _lastAt = DateTimeOffset.MinValue;

    /// <summary>Consecutive suspicious repeats (1 = first sighting, not yet a repeat).</summary>
    public int Streak { get; private set; }

    /// <summary>True after the in-band "are you stuck?" message was injected; the next full streak
    /// hard-stops. Cleared when the model changes call or says anything.</summary>
    public bool Challenged { get; set; }

    /// <summary>Records one executed tool call. <paramref name="hadText"/> — the assistant message
    /// carrying this call also contained content or reasoning (a sign of deliberate action).</summary>
    public void Observe(string name, string args, string result, bool hadText, DateTimeOffset now)
    {
        var key = name + "\n" + args;
        var hash = result.GetHashCode();
        var suspicious = key == _lastKey && hash == _lastResultHash && !hadText
                         && now - _lastAt <= FastRound;
        if (suspicious) Streak++;
        else
        {
            Streak = 1;
            if (key != _lastKey || hadText) Challenged = false;
        }
        _lastKey = key;
        _lastResultHash = hash;
        _lastAt = now;
    }

    /// <summary>Called right after injecting the challenge — the model deserves a fresh window.</summary>
    public void BeginChallenge()
    {
        Challenged = true;
        Streak = 0;
    }
}
