namespace SPLA.Domain.Host;

/// <summary>
/// The Phase 0 sandbox: real file system + real shell + allow-all gate. It enforces nothing, so
/// local behaviour is unchanged — its only job is to be the seam that a real
/// <c>ProcessSandbox</c> (server) or an in-memory sandbox (tests) can replace without touching any
/// tool.
/// </summary>
public sealed class PassthroughSandbox : ISandbox, IDisposable
{
    /// <summary>Process-wide fallback used when no chat scope is open (CLI, tests).</summary>
    public static readonly PassthroughSandbox Default = new();

    /// <summary>The default shell's configured silent-idle timeout, applied whenever this sandbox (or
    /// a chat's own copy of it, via <see cref="ForChat"/>) has to build its own <see cref="LocalShell"/>
    /// rather than being handed one. Null keeps <see cref="LocalShell"/>'s own 120s default. Not
    /// readonly: <see cref="SetShellSilentIdle"/> lets a live settings change take effect immediately
    /// rather than only on the next restart.</summary>
    private TimeSpan? _shellSilentIdle;

    /// <summary>
    /// Builds a sandbox, filling in the local implementations for whatever is not supplied. Note the
    /// asymmetry this creates and why <see cref="WithShell"/> exists: passing <c>null</c> here means
    /// "give me the default shell", so there is no way to say "no shell at all" through this door.
    /// </summary>
    public PassthroughSandbox(
        IWorkspace? workspace = null, IShell? shell = null, ICapabilityGate? gate = null, TimeSpan? shellSilentIdle = null)
        : this(workspace ?? new LocalWorkspace(), gate ?? AllowAllGate.Instance, shell ?? new LocalShell(shellSilentIdle))
    {
        _shellSilentIdle = shellSilentIdle;
    }

    /// <summary>The one that takes the parts as given, absent shell included. Argument order differs
    /// from the public constructor only so the two signatures do not collide.</summary>
    private PassthroughSandbox(IWorkspace workspace, ICapabilityGate gate, IShell? shell, TimeSpan? shellSilentIdle = null)
    {
        Workspace = workspace;
        Shell = shell;
        Gate = gate;
        _shellSilentIdle = shellSilentIdle;
    }

    /// <summary>The one way to build a sandbox with an explicitly absent shell — the case the public
    /// constructor cannot express, because there <c>null</c> already means "use the default".</summary>
    public static PassthroughSandbox WithShell(IWorkspace workspace, IShell? shell, ICapabilityGate? gate = null)
        => new(workspace, gate ?? AllowAllGate.Instance, shell);

    public IWorkspace Workspace { get; }
    public IShell? Shell { get; }
    public ICapabilityGate Gate { get; }

    /// <summary>
    /// The project's boundary and gate, and a shell of the chat's own.
    /// <para>
    /// Sharing the workspace is the point, not an omission: the boundary is a property of the
    /// project, and a chat that could widen it would be a chat that escapes the project. The shell is
    /// the opposite — its interactive sessions are live processes, and a process started by one chat
    /// must end when that chat does.
    /// </para>
    /// <para>A shell-less sandbox stays shell-less: execution disabled for the project is disabled
    /// for every chat in it.</para>
    /// </summary>
    public ISandbox ForChat() => new PassthroughSandbox(
        Workspace,
        Gate,
        Shell is null ? null : new LocalShell(_shellSilentIdle),
        _shellSilentIdle);

    /// <summary>Applies a changed <c>ShellTimeoutSeconds</c> setting immediately — to this sandbox's
    /// own shell if it has one, and to whatever a future <see cref="ForChat"/> builds. Existing
    /// per-chat shells already handed out keep whatever they started with, the same "next chat/turn,
    /// not retroactive" rule other settings here follow.</summary>
    public void SetShellSilentIdle(TimeSpan idle)
    {
        _shellSilentIdle = idle;
        if (Shell is LocalShell localShell) localShell.DefaultSilentIdle = idle;
    }

    /// <summary>Ends what this sandbox owns. Only the shell has anything to end — the workspace is a
    /// boundary, not a resource, and the gate is a rule.</summary>
    public void Dispose() => (Shell as IDisposable)?.Dispose();
}
