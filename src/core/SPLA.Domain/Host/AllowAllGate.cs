namespace SPLA.Domain.Host;

/// <summary>
/// Soft gate for local, trusted use: everything is permitted. Stands in until real policy merging
/// (project + user + org) and capability groups land. The future <c>HardGate</c> will implement the
/// same contract with OS-level enforcement and must return the same decisions.
/// </summary>
public sealed class AllowAllGate : ICapabilityGate
{
    public static readonly AllowAllGate Instance = new();

    public bool CanRead(string path) => true;
    public bool CanWrite(string path) => true;
    public bool CanExecute() => true;
    public bool CanNetwork() => true;
}
