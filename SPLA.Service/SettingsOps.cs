using SPLA.Domain.Settings;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// Read/write operations for project settings, exposed to clients over the protocol. Kept separate
/// from <see cref="ClientConnection"/> so each settings area (connections today; modes/permissions/
/// plugins later) is a small, self-contained unit rather than swelling the connection dispatch.
/// <para>
/// Edits mutate the live <see cref="ResolvedSettings"/> in place (so running chats pick them up) and,
/// when the service is on a real <c>.spla</c> project, persist into that file's <c>connections:</c>
/// section. With no project file, edits live only for the session.
/// </para>
/// </summary>
public static class SettingsOps
{
    public static ConnectionsPayload GetConnections(AgentRuntime runtime) => new()
    {
        CanPersist = runtime.Settings.ProjectFilePath != null,
        Connections = runtime.Settings.Connections.Select(c => new ConnectionEditDto
        {
            Id = c.Id,
            Name = c.Name,
            Provider = c.Provider,
            Endpoint = c.Endpoint,
            ApiKey = c.ApiKey,
            Model = c.Model
        }).ToList()
    };

    /// <summary>Replaces the connection list: persists to the .spla project (when present) and mutates
    /// the live settings so chats see the new set immediately. Returns the canonical list to broadcast.</summary>
    public static ConnectionsPayload SaveConnections(AgentRuntime runtime, IEnumerable<ConnectionEditDto> incoming)
    {
        var sections = incoming
            .Select(ToSection)
            .Where(c => !string.IsNullOrWhiteSpace(c.Id))
            .GroupBy(c => c.Id, StringComparer.OrdinalIgnoreCase)   // last write wins per id
            .Select(g => g.Last())
            .ToList();

        // Persist into the project file's connections: section, leaving everything else untouched.
        var path = runtime.Settings.ProjectFilePath;
        if (path != null)
        {
            var project = ConfigLoader.LoadProjectRaw(path);
            project.Connections = sections.Count > 0 ? sections : null;
            ConfigLoader.SaveProject(project, path);
        }

        // Mutate the live settings in place so running chats resolve against the new list.
        runtime.Settings.Connections.Clear();
        runtime.Settings.Connections.AddRange(sections);

        return GetConnections(runtime);
    }

    private static SplaConnectionSection ToSection(ConnectionEditDto d)
    {
        var id = string.IsNullOrWhiteSpace(d.Id) ? Slug(d.Name ?? d.Model ?? "") : d.Id.Trim();
        return new SplaConnectionSection
        {
            Id = id,
            Name = string.IsNullOrWhiteSpace(d.Name) ? null : d.Name.Trim(),
            Provider = Blank(d.Provider),
            Endpoint = Blank(d.Endpoint),
            ApiKey = Blank(d.ApiKey),
            Model = Blank(d.Model)
        };
    }

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string Slug(string s)
    {
        var chars = s.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        var slug = new string(chars).Trim('-');
        return string.IsNullOrEmpty(slug) ? "conn-" + Guid.NewGuid().ToString("N")[..6] : slug;
    }
}
