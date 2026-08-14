using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPLA.Domain.Host;

/// <summary>Why a path was refused. Callers act on these differently — a cutout is never negotiable,
/// while the root rule may still be running in shadow — so the reason has to survive as a value and
/// not only as a sentence.</summary>
public enum PathRefusal
{
    None,
    Empty,
    Unreadable,
    NetworkShare,
    OutsideRoot,
    Cutout,

    /// <summary>An address under <c>mnt/</c> naming a mount this project does not declare — including
    /// the bare prefix, which is not a folder. Nothing to fall back to: unlike the root rule there is
    /// no host path this could have meant.</summary>
    MountUnknown,

    /// <summary>Inside <c>mnt/&lt;name&gt;/</c> in name, outside the mount's target once resolved —
    /// a <c>..</c> or a link out. Never shadowed: the root rule waits because its legal exceptions
    /// were never enumerated, whereas a mount <i>is</i> that enumeration, so its edge is already
    /// decided.</summary>
    OutsideMount,

    /// <summary>The mount is declared but its target is not on this machine. A diagnosis of its own,
    /// never "file not found" — the reaction to an unplugged folder and to a missing file differ.</summary>
    MountUnavailable,

    /// <summary>A write into a mount declared <c>access: read</c>. The floor on the node, the same
    /// mechanism as <c>allow_write</c> on an SSH host.</summary>
    MountReadOnly
}

/// <summary>
/// A path the boundary refused. Distinct from an ordinary I/O failure on purpose: this is a
/// decision, not a fault, and the difference is what stops the model from "fixing" a file it was
/// simply not allowed to touch.
/// </summary>
public sealed class PathBoundaryException : Exception
{
    public PathBoundaryException(PathRefusal refusal, string message) : base(message) => Refusal = refusal;

    public PathRefusal Refusal { get; }
}

/// <summary>
/// Where a path landed. A record rather than a fifth <c>out</c> parameter: the mount is needed by
/// callers that enforce something (the floor) and by callers that print something (the canonical
/// form), and threading it through as an extra output on every overload would push the shape of one
/// caller onto all of them.
/// </summary>
/// <param name="Mount">The mount the path landed in, or null for the project root. Set whether or not
/// the mount is available — that is the executor's question, not the boundary's.</param>
public readonly record struct PathLanding(
    bool Ok,
    string FullPath,
    string? Error,
    PathRefusal Refusal,
    ProjectMount? Mount)
{
    public static PathLanding Refused(PathRefusal refusal, string error) =>
        new(false, string.Empty, error, refusal, null);

    public static PathLanding Allowed(string fullPath, ProjectMount? mount = null) =>
        new(true, fullPath, null, PathRefusal.None, mount);
}

/// <summary>
/// One answer to "is this path inside the area I am allowed to touch", and the only one. Four
/// hand-rolled versions of this check had grown up separately — SFTP transfers, skill sources, the
/// 1C indexer, the web file browser — each with its own idea of what counts as an escape, which is
/// exactly how a boundary ends up holding in three places out of four.
///
/// <para>Mechanism only, no policy: it says where a path lands, never whether the caller may go
/// there. Who is allowed what is the gate's business. <b>Mounts do not change that</b>: this class
/// reports which mount a path fell into, and <c>LocalWorkspace.Guard</c> decides whether a caller may
/// write there or whether an unplugged target is an error — because those are two different
/// questions, and telling them apart is what this class is for.</para>
///
/// <para><b>Cutouts</b> are holes inside the root — subtrees that are inside by containment but out
/// of bounds anyway, <c>.spla/</c> being the case that motivated them. A boundary without them
/// would lock the agent in together with the secrets rather than out.</para>
///
/// <para><b>Mounts</b> are the opposite shape: areas outside the root that are addressable anyway,
/// under <c>mnt/&lt;name&gt;/</c>. They are how the list of legitimate exits from the root stops
/// being un-guessable — which is the whole reason the root rule is still in shadow.</para>
/// </summary>
public sealed class PathBoundary
{
    /// <summary>No boundary at all: every path resolves to itself, nothing is refused. This is what
    /// running without a project means — the current directory is where the process was launched,
    /// which is a default, not a boundary, and pretending otherwise would bound tools by an accident
    /// of how they were started.</summary>
    public static readonly PathBoundary None = new();

    private readonly string? _root;
    private readonly string[] _cutouts;
    private readonly ProjectMount[] _mounts;

    private PathBoundary()
    {
        _root = null;
        _cutouts = [];
        _mounts = [];
    }

    /// <param name="root">Absolute path to the area. Relative input is made absolute here.</param>
    /// <param name="cutouts">Paths carved out of <paramref name="root"/>, relative to it or absolute.</param>
    /// <param name="mounts">Declared folders outside the root, already validated and resolved by
    /// <c>MountResolver</c> — this class never reads a manifest and never decides what is legal to
    /// declare.</param>
    public PathBoundary(string root, IEnumerable<string>? cutouts = null, IEnumerable<ProjectMount>? mounts = null)
    {
        if (string.IsNullOrWhiteSpace(root))
            throw new ArgumentException("A boundary needs a root; use PathBoundary.None for no boundary.", nameof(root));

        _root = Normalize(Path.GetFullPath(root));
        _cutouts = cutouts?
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => Normalize(Path.GetFullPath(Path.IsPathRooted(c) ? c : Path.Combine(_root, c))))
            .ToArray() ?? [];
        _mounts = mounts?.ToArray() ?? [];
    }

    /// <summary>The area, or null when unbounded.</summary>
    public string? Root => _root;

    public bool IsBounded => _root is not null;

    /// <summary>The declared mounts, in the order the manifest listed them.</summary>
    public IReadOnlyList<ProjectMount> Mounts => _mounts;

    /// <summary>
    /// Resolves <paramref name="logicalPath"/> and reports where it landed.
    /// <see cref="PathLanding.Error"/> is written for a human and for the model: it says what was
    /// refused and why, never how the host is laid out.
    /// </summary>
    public PathLanding Resolve(string logicalPath)
    {
        if (string.IsNullOrWhiteSpace(logicalPath))
            return PathLanding.Refused(PathRefusal.Empty, "the path is empty");

        var candidate = logicalPath.Trim();

        if (_root is null)
        {
            // Unbounded: normalise and hand it back. GetFullPath still throws on genuinely malformed
            // input, and that is a fault, not a refusal.
            try { return PathLanding.Allowed(Path.GetFullPath(candidate)); }
            catch (Exception ex)
            {
                return PathLanding.Refused(PathRefusal.Unreadable, $"the path cannot be read: {ex.Message}");
            }
        }

        // A network share is nobody's project. Caught before combining, because "\\host\share"
        // combined with a root silently becomes the share itself.
        if (candidate.StartsWith(@"\\", StringComparison.Ordinal) ||
            candidate.StartsWith("//", StringComparison.Ordinal))
            return PathLanding.Refused(PathRefusal.NetworkShare, "network shares are outside the project");

        // The reserved prefix is read before anything else: `mnt/AAA/x` is an address, not a folder
        // under the root, and combining it with the root first would quietly turn it into one.
        if (!Path.IsPathRooted(candidate) && LooksLikeMountAddress(candidate, out var name, out var rest))
            return ResolveInMount(name, rest);

        string resolved;
        try
        {
            resolved = Path.GetFullPath(Path.IsPathRooted(candidate) ? candidate : Path.Combine(_root, candidate));
        }
        catch (Exception ex)
        {
            return PathLanding.Refused(PathRefusal.Unreadable, $"the path cannot be read: {ex.Message}");
        }

        // Follow links BEFORE deciding: a junction inside the project pointing at C:\ is a legitimate
        // path by every string test there is. The check has to be on where it actually lands.
        resolved = Normalize(ResolveLinks(resolved));

        if (!IsUnder(resolved, _root))
        {
            // A host path may still land in a declared mount — that is a legal exit, named in the
            // manifest, and calling it "outside the project" would report the one case the manifest
            // exists to describe as the failure it was meant to remove.
            var landed = _mounts.FirstOrDefault(m => IsUnder(resolved, m.HostPath));
            if (landed is not null) return PathLanding.Allowed(resolved, landed);

            return PathLanding.Refused(PathRefusal.OutsideRoot, "the path is outside the project");
        }

        foreach (var cutout in _cutouts)
        {
            if (IsUnder(resolved, cutout))
                return PathLanding.Refused(
                    PathRefusal.Cutout,
                    $"'{Path.GetFileName(cutout)}' is the application's own folder and is not open to tools");
        }

        return PathLanding.Allowed(resolved);
    }

    /// <inheritdoc cref="Resolve"/>
    public bool TryResolve(string logicalPath, out string fullPath, out string? error)
        => TryResolve(logicalPath, out fullPath, out error, out _);

    /// <inheritdoc cref="Resolve"/>
    /// <param name="refusal">Which rule refused, for callers that treat them differently.</param>
    public bool TryResolve(string logicalPath, out string fullPath, out string? error, out PathRefusal refusal)
    {
        var landing = Resolve(logicalPath);
        fullPath = landing.FullPath;
        error = landing.Error;
        refusal = landing.Refusal;
        return landing.Ok;
    }

    /// <summary>Whether an already-absolute path lands in bounds. Same rules as
    /// <see cref="Resolve"/>, for callers that only need the verdict.</summary>
    public bool Contains(string fullPath) => Resolve(fullPath).Ok;

    /// <summary>
    /// The address to show for <paramref name="path"/>: <c>mnt/&lt;name&gt;/...</c> for anything in a
    /// mount, otherwise a path relative to the root. Null when it lands nowhere this boundary knows.
    ///
    /// <para>Required wherever a path is printed, returned to the model, logged, or stored. A host
    /// path handed back to the model comes back as an argument and resolves under different rules on
    /// the next machine — and a host path written into saved state is what makes a project stop being
    /// portable. Forward slashes, always: the canonical form has to read the same everywhere.</para>
    /// </summary>
    public string? ToCanonical(string path)
    {
        var landing = Resolve(path);
        if (!landing.Ok) return null;
        if (_root is null) return landing.FullPath;

        if (landing.Mount is { } mount)
            return mount.Address(Relative(landing.FullPath, mount.HostPath));

        var relative = Relative(landing.FullPath, _root);
        return relative.Length == 0 ? "." : relative;
    }

    /// <summary>
    /// The real location behind any links on the way to <paramref name="path"/>. Public because a
    /// mount target has to be put through exactly the same treatment as the root before anything is
    /// judged against it — a target that is itself a link would otherwise contain none of its own
    /// files once they resolve.
    ///
    /// <para>The target may not exist yet — writing a new file is the normal case — so the walk goes
    /// up to the nearest ancestor that does exist, resolves that, and puts the remainder back. A
    /// link anywhere along the existing part is therefore caught, which is the point: the escape is
    /// usually a junction on a parent folder, not on the leaf.</para>
    /// </summary>
    public static string RealPath(string path) => ResolveLinks(path);

    /// <summary>Whether <paramref name="path"/> is written as an address under the reserved prefix.
    /// Answered on the text, before any combining — that is the point of reserving a name.</summary>
    private static bool LooksLikeMountAddress(string path, out string name, out string rest)
    {
        name = string.Empty;
        rest = string.Empty;

        var segments = path
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(s => s != ".")
            .ToArray();

        if (segments.Length == 0 || !segments[0].Equals(ProjectMount.Prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        if (segments.Length >= 2) name = segments[1];
        rest = string.Join('/', segments.Skip(2));
        return true;
    }

    private PathLanding ResolveInMount(string name, string rest)
    {
        if (name.Length == 0)
            return PathLanding.Refused(
                PathRefusal.MountUnknown,
                $"'{ProjectMount.Prefix}' is not a folder — it is the prefix declared mounts are addressed " +
                $"under, as {ProjectMount.Prefix}/<name>/.... {KnownMounts()}");

        var mount = _mounts.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (mount is null)
            return PathLanding.Refused(
                PathRefusal.MountUnknown,
                $"this project declares no mount named '{name}'. {KnownMounts()}");

        string resolved;
        try
        {
            resolved = rest.Length == 0
                ? mount.HostPath
                : Path.GetFullPath(Path.Combine(mount.HostPath, rest.Replace('/', Path.DirectorySeparatorChar)));
        }
        catch (Exception ex)
        {
            return PathLanding.Refused(PathRefusal.Unreadable, $"the path cannot be read: {ex.Message}");
        }

        resolved = Normalize(ResolveLinks(resolved));

        if (!IsUnder(resolved, mount.HostPath))
            return PathLanding.Refused(
                PathRefusal.OutsideMount,
                $"the path leaves mount '{mount.Name}'");

        return PathLanding.Allowed(resolved, mount);
    }

    private string KnownMounts() => _mounts.Length == 0
        ? "This project declares no mounts."
        : "Declared: " + string.Join(", ", _mounts.Select(m => m.Address())) + ".";

    private static string Relative(string path, string area) =>
        path.Length <= area.Length
            ? string.Empty
            : path[(area.Length + 1)..].Replace('\\', '/');

    private static string ResolveLinks(string path)
    {
        try
        {
            var remainder = new Stack<string>();
            var current = path;

            while (true)
            {
                if (File.Exists(current) || Directory.Exists(current))
                {
                    var target = File.Exists(current)
                        ? File.ResolveLinkTarget(current, returnFinalTarget: true)?.FullName
                        : new DirectoryInfo(current).ResolveLinkTarget(returnFinalTarget: true)?.FullName;

                    current = target ?? current;
                    break;
                }

                var parent = Path.GetDirectoryName(current);
                if (string.IsNullOrEmpty(parent) || parent == current) return path;

                remainder.Push(Path.GetFileName(current));
                current = parent;
            }

            while (remainder.Count > 0) current = Path.Combine(current, remainder.Pop());
            return Path.GetFullPath(current);
        }
        catch (IOException)
        {
            // A path we cannot inspect is not a path we can vouch for; fall back to the literal one
            // and let the containment check decide on that.
            return path;
        }
        catch (UnauthorizedAccessException)
        {
            return path;
        }
    }

    private static bool IsUnder(string path, string root) =>
        path.Equals(root, StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string path) =>
        path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) is { Length: > 0 } trimmed
            ? trimmed
            : path;
}
