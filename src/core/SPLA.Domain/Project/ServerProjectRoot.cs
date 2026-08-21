using System.IO;
using SPLA.Domain.Identity;
using SPLA.Domain.Models;

namespace SPLA.Domain.Project;

/// <summary>
/// The server's per-user storage root. Each identity owns an area <c>{root}/users/{userKey}/</c>
/// that is created on first touch ("автоматом заводится область при первом коннекте"), and inside
/// it the user gets an ordinary <see cref="LocalProjectProvider"/> — the same file-backed provider
/// used locally, just rooted in the user's folder. That reuse is deliberate: one project code path,
/// isolation comes purely from the root. Sharing (own ∪ group-shared) layers on top of this, it does
/// not replace it.
/// </summary>
public sealed class ServerProjectRoot
{
    private readonly string _root;

    public ServerProjectRoot(string root) => _root = root;

    /// <summary>Absolute path to a user's area, created if missing. <paramref name="userKey"/> is a
    /// SID (already path-safe); still sanitized defensively so nothing exotic escapes the root.</summary>
    public string EnsureUserArea(string userKey)
    {
        var dir = Path.Combine(_root, "users", Sanitize(userKey));
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>The user's own-projects provider, rooted in their area.</summary>
    public IProjectProvider ProviderFor(IIdentity identity)
        => new LocalProjectProvider(EnsureUserArea(identity.UserKey));

    /// <summary>The user's default project, scaffolded in their area on first connect. This is what a
    /// connection lands in when it names no project — so a domain user drops into THEIR OWN empty
    /// project (workspace = their area), never the server's own project. Returns its manifest path.</summary>
    public string EnsureDefaultProject(IIdentity identity)
    {
        var area = EnsureUserArea(identity.UserKey);
        var manifest = Path.Combine(area, "default.spla");
        // Standard, not Minimal: this is the user's own area, not a folder they wandered into — the
        // caution a bare-directory project needs (see ProjectProfile.Minimal) does not apply here.
        if (!File.Exists(manifest))
            ProjectFactory.CreateAt(manifest, $"{identity.DisplayName} — default", ProjectProfile.Standard);
        return manifest;
    }

    /// <summary>
    /// The user area that contains <paramref name="path"/>, or null when it lies outside every one of
    /// them. Used to work out whose personal state a project belongs to — on a server, a workspace
    /// under <c>{root}/users/{sid}/…</c> means that person's skill branches, grants and drafts, not
    /// the service account's.
    /// </summary>
    public string? UserAreaFor(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;

        try
        {
            var usersRoot = Path.GetFullPath(Path.Combine(_root, "users"));
            var full = Path.GetFullPath(path);
            if (!full.StartsWith(usersRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return null;

            // The first segment below users/ IS the area; anything deeper is inside a project.
            var relative = full[(usersRoot.Length + 1)..];
            var cut = relative.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
            var area = cut < 0 ? relative : relative[..cut];

            return area.Length == 0 ? null : Path.Combine(usersRoot, area);
        }
        catch { return null; }
    }

    private static string Sanitize(string key)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = key.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        var name = new string(chars).Trim();
        return string.IsNullOrEmpty(name) ? "unknown" : name;
    }
}
