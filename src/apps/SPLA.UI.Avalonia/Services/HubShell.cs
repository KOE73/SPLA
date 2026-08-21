using System;
using System.Threading;

namespace SPLA.UI.Avalonia.Services;

/// <summary>
/// The machine's tray shell: one process per Windows session that owns the tray icon and nothing else.
///
/// <para><b>Why one process rather than one per window.</b> The tray was always a machine-wide view —
/// it draws every instance, not the one behind any particular window — but its *existence* was
/// per-process, so three open projects meant three identical icons. Content and lifetime disagreed.
/// Giving it its own process settles that: windows show projects, this shows the machine.</para>
///
/// <para><b>Why a session-scoped mutex, not a global one.</b> An unprefixed mutex name lives in the
/// terminal-server session namespace, which is the right scope precisely because a tray is per
/// session: two people signed in to the same box over RDP each have their own notification area, and
/// a machine-global lock would give the second one no tray at all.</para>
///
/// <para><b>Why it is safe for anyone to try.</b> Every window attempts to start this shell, and all
/// but the first lose the mutex and exit immediately. That is cheaper than electing a leader and has
/// no state to get wrong — the OS is the arbiter.</para>
/// </summary>
public sealed class HubShell : IDisposable
{
    private const string MutexName = "SPLA.HubShell";

    private readonly Mutex _mutex;

    private HubShell(Mutex mutex) => _mutex = mutex;

    /// <summary>Claims the role of this session's tray shell, or returns null when somebody already
    /// holds it. Null means "quit quietly" — it is the expected outcome for every launch after the
    /// first, not an error to report.</summary>
    public static HubShell? Claim()
    {
        var mutex = new Mutex(initiallyOwned: true, MutexName, out var isFirst);
        if (isFirst) return new HubShell(mutex);

        mutex.Dispose();
        return null;
    }

    public void Dispose()
    {
        try { _mutex.ReleaseMutex(); }
        catch (ApplicationException) { /* never acquired, or already gone with the process */ }
        _mutex.Dispose();
    }
}
