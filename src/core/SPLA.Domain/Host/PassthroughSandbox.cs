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

    /// <summary>
    /// Builds a sandbox, filling in the local implementations for whatever is not supplied. Note the
    /// asymmetry this creates and why <see cref="WithShell"/> exists: passing <c>null</c> here means
    /// "give me the default shell", so there is no way to say "no shell at all" through this door.
    /// </summary>
    public PassthroughSandbox(IWorkspace? workspace = null, IShell? shell = null, ICapabilityGate? gate = null)
        : this(workspace ?? new LocalWorkspace(), gate ?? AllowAllGate.Instance, shell ?? new LocalShell())
    {
    }

    /// <summary>The one that takes the parts as given, absent shell included. Argument order differs
    /// from the public constructor only so the two signatures do not collide.</summary>
    private PassthroughSandbox(IWorkspace workspace, ICapabilityGate gate, IShell? shell)
    {
        Workspace = workspace;
        Shell = shell;
        Gate = gate;
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
        Shell is null ? null : new LocalShell());

    /// <summary>Ends what this sandbox owns. Only the shell has anything to end — the workspace is a
    /// boundary, not a resource, and the gate is a rule.</summary>
    public void Dispose() => (Shell as IDisposable)?.Dispose();
}
