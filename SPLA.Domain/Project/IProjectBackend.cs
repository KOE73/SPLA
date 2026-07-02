namespace SPLA.Domain.Project;

/// <summary>
/// Physical storage strategy behind a project. The project brokers stores; the backend decides
/// where they live: local = <c>.spla/</c> folder, server = DB rows, memory = tests. Data
/// migration between backends is the backend's problem, never the core's.
/// </summary>
public interface IProjectBackend
{
    /// <summary>Name of the root bucket — the runtime area itself (locally: <c>.spla/</c>).</summary>
    const string RootBucket = "";

    /// <summary>Stable project identity. Locally the manifest path; on a server userId/projectName.</summary>
    string ProjectId { get; }

    /// <summary>Opens (lazily creating) the named bucket. Root bucket via <see cref="RootBucket"/>.</summary>
    IBucket GetBucket(string name);
}
