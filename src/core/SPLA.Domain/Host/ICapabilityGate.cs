namespace SPLA.Domain.Host;

/// <summary>
/// The single point that <em>decides</em> what is allowed — policy, not mechanism. Every managed
/// tool asks the same gate; there is no tool "outside the system". The gate is built by merging
/// project + user + org policy on the "most restrictive wins" principle.
/// <para>
/// Two implementations of this one contract are planned: a <em>soft</em> gate (checks in code,
/// local trusted use) and a <em>hard</em> gate (configures an OS sandbox, untrusted server use).
/// They must return the same <em>decision</em>; only the enforcement mechanism differs.
/// </para>
/// </summary>
public interface ICapabilityGate
{
    bool CanRead(string path);
    bool CanWrite(string path);

    /// <summary>Whether shell/execute is permitted at all. <c>false</c> ⇒ shell must not start.</summary>
    bool CanExecute();

    bool CanNetwork();

    /// <summary>
    /// Whether this chat may reply into another role's chat at all. Consulted by the source side of a
    /// correspondence edge (<c>ChatRuntime.SendReply</c>, PLAN_20260902 wave 4) — a role-to-role reply
    /// is a call like any other and goes through the same gate rather than a separate permission
    /// subsystem (<c>docs/adr/ADR_20260827-2_core_roles.md</c> §2.4: "гранты те же").
    /// </summary>
    bool CanCorrespond();
}
