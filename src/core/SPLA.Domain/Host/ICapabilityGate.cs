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
}
