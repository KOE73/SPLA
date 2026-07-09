using System.Collections.Generic;

namespace SPLA.Domain.Project;

/// <summary>A known project as the provider lists it — enough for a picker/tree, no stores opened.</summary>
public sealed record ProjectDescriptor
{
    /// <summary>Provider-scoped identity: locally the manifest path, on a server userId/projectName.</summary>
    public required string Id { get; init; }

    public string? Name { get; init; }

    /// <summary>Manifest location for local projects; null where there is no manifest file (server).</summary>
    public string? ManifestPath { get; init; }

    public DateTimeOffset? LastOpened { get; init; }
}

/// <summary>
/// The level above a single project: enumerates, opens and creates projects. The projects→chats
/// tree sits on this (List → per-project chat history), independent of whether a window equals a
/// project. Local = registry file + manifests on disk; server = DB, same interface.
/// </summary>
public interface IProjectProvider
{
    /// <summary>All known projects, stale entries (missing manifests) filtered out.</summary>
    IReadOnlyList<ProjectDescriptor> List();

    /// <summary>Known projects ordered by last-opened, most recent first.</summary>
    IReadOnlyList<ProjectDescriptor> Recent();

    /// <summary>Opens the project by id, registering it (and its recency) as a side effect.</summary>
    IProject Open(string id);

    /// <summary>Creates a new project from the descriptor and opens it.</summary>
    IProject Create(ProjectDescriptor descriptor);
}
