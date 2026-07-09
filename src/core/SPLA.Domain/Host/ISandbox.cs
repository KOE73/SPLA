namespace SPLA.Domain.Host;

/// <summary>
/// The host boundary: everything an agent can use to touch the real system, gathered in one place.
/// Not the architectural top (that is the agent context) but the <em>risk centre</em> — memory,
/// secrets and the LLM are safe by nature and live outside it.
/// <para>
/// The enforcement mechanism differs per member: <see cref="Workspace"/> is bounded in code via
/// <see cref="Gate"/>, while <see cref="Shell"/> (arbitrary code) needs OS-level isolation in
/// untrusted scenarios — or is simply absent (<c>null</c>).
/// </para>
/// </summary>
public interface ISandbox
{
    IWorkspace Workspace { get; }

    /// <summary><c>null</c> when execution is disabled in this scenario.</summary>
    IShell? Shell { get; }

    ICapabilityGate Gate { get; }
}
