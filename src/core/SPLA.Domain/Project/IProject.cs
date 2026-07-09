using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Secrets;

namespace SPLA.Domain.Project;

/// <summary>
/// The project as a STORAGE BROKER, not a data container: it hands out stores and never holds
/// data itself. Consumers ask for capabilities (workspace, KV, secrets, buckets) and stay
/// ignorant of the physical layout — that lives in <see cref="IProjectBackend"/>.
/// The manifest (<c>project.spla</c>, goes to git) stays separate from runtime state
/// (<c>.spla/</c>, gitignored); on a server the manifest is a DB record and the interface
/// is unchanged.
/// </summary>
public interface IProject
{
    string Id { get; }

    string? Name { get; }

    /// <summary>Project files (source tree) — the agent-visible workspace.</summary>
    IWorkspace GetWorkspace();

    /// <summary>Project-scoped agent working memory (shared across chats, persistent).</summary>
    IKeyValueStore GetKV();

    ISecretStore GetSecrets();

    /// <summary>Opaque bucket for a subsystem/plugin ("sql", "chats", "browser-profile", ...).</summary>
    IBucket GetBucket(string name);
}
