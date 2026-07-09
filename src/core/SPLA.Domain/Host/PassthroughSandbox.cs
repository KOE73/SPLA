namespace SPLA.Domain.Host;

/// <summary>
/// The Phase 0 sandbox: real file system + real shell + allow-all gate. It enforces nothing, so
/// local behaviour is unchanged — its only job is to be the seam that a real
/// <c>ProcessSandbox</c> (server) or an in-memory sandbox (tests) can replace without touching any
/// tool.
/// </summary>
public sealed class PassthroughSandbox : ISandbox
{
    /// <summary>Process-wide fallback used when no chat scope is open (CLI, tests).</summary>
    public static readonly PassthroughSandbox Default = new();

    public PassthroughSandbox(IWorkspace? workspace = null, IShell? shell = null, ICapabilityGate? gate = null)
    {
        Workspace = workspace ?? new LocalWorkspace();
        Shell = shell ?? new LocalShell();
        Gate = gate ?? AllowAllGate.Instance;
    }

    public IWorkspace Workspace { get; }
    public IShell? Shell { get; }
    public ICapabilityGate Gate { get; }
}
