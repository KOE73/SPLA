using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using SPLA.Library.Format;

namespace SPLA.Library.Sources;

/// <summary>
/// Skills stored as markdown in a folder — the project's <c>.spla/skills</c> and the machine's
/// <c>~/.spla/skills</c>.
///
/// <para>Two layouts, both first-class: a bare <c>name.md</c> in the root is a simple skill, and a
/// subfolder containing <c>SKILL.md</c> is a skill that carries resources alongside it (scripts,
/// references). The second form exists so a long procedure can keep its attachments together, and
/// <see cref="ListResources"/> / <see cref="ReadResource"/> are how they are handed out.</para>
///
/// <para>Change detection lives here rather than in a separate watcher class: a FileSystemWatcher is
/// an implementation detail of file-shaped sources, and a server or database source will raise
/// <see cref="Changed"/> by completely different means.</para>
/// </summary>
public sealed class DirectorySkillSource : IEditableSkillSource, IDisposable
{
    private const string FolderSkillFile = "SKILL.md";
    private const int DebounceMs = 400;

    private readonly string _root;
    private readonly ILogger? _logger;
    private readonly object _gate = new();
    private FileSystemWatcher? _watcher;
    private Timer? _debounce;
    private bool _disposed;

    public string Id { get; }
    public string Label { get; }
    public SkillTrust Trust { get; }
    public event Action? Changed;

    /// <summary>The folder this source serves. Exposed for the settings panel, which shows the user
    /// where to put a new skill file.</summary>
    public string RootPath => _root;

    public DirectorySkillSource(string id, string label, string rootPath, SkillTrust trust,
        ILogger? logger = null, bool watch = true)
    {
        Id = id;
        Label = label;
        Trust = trust;
        _root = Path.GetFullPath(rootPath);
        _logger = logger;

        if (watch) StartWatching();
    }

    /// <summary>
    /// Attaches the watcher appropriate to the current state of the root folder.
    ///
    /// <para>A missing folder is the normal case — most projects have no <c>.spla/skills</c> until
    /// the first draft is written — so it is neither an error nor created eagerly. But
    /// FileSystemWatcher cannot watch a path that does not exist, and simply giving up would mean
    /// the folder a user creates mid-session stays dark until restart. So an absent root is watched
    /// for on the nearest existing ancestor instead, and the moment it appears this swaps itself for
    /// the real content watcher.</para>
    /// </summary>
    private void StartWatching()
    {
        lock (_gate)
        {
            if (_disposed) return;

            DetachWatcher();

            if (Directory.Exists(_root))
            {
                _watcher = Attach(_root, "*.md",
                    NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    OnFileEvent);
                return;
            }

            var ancestor = NearestExistingAncestor(_root);
            if (ancestor is null)
            {
                _logger?.LogDebug(
                    "Skill folder and all its ancestors are missing — not watched. Source={SourceId} Path={Path}",
                    Id, _root);
                return;
            }

            _watcher = Attach(ancestor, "*.*", NotifyFilters.DirectoryName, OnAncestorEvent);
        }
    }

    private FileSystemWatcher? Attach(
        string path, string filter, NotifyFilters notify, FileSystemEventHandler handler)
    {
        try
        {
            var watcher = new FileSystemWatcher(path, filter)
            {
                IncludeSubdirectories = true,
                NotifyFilter = notify,
                EnableRaisingEvents = true
            };
            watcher.Changed += handler;
            watcher.Created += handler;
            watcher.Deleted += handler;
            watcher.Renamed += (s, e) => handler(s, e);
            return watcher;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            // An unwatchable folder still enumerates fine — the source degrades to no hot reload
            // rather than failing to construct.
            _logger?.LogWarning(ex, "Skill folder cannot be watched. Source={SourceId} Path={Path}", Id, path);
            return null;
        }
    }

    private static string? NearestExistingAncestor(string path)
    {
        for (var dir = Path.GetDirectoryName(path); dir != null; dir = Path.GetDirectoryName(dir))
            if (Directory.Exists(dir)) return dir;

        return null;
    }

    /// <summary>Something appeared under an ancestor while the root was missing. Only the root
    /// showing up matters; anything else is a neighbouring folder and is ignored.</summary>
    private void OnAncestorEvent(object sender, FileSystemEventArgs e)
    {
        if (!Directory.Exists(_root)) return;

        StartWatching();
        Changed?.Invoke();
    }

    private void DetachWatcher()
    {
        if (_watcher is null) return;

        // Disposing is enough to silence it — the handlers are reachable only from the watcher and
        // go with it. Unsubscribing each one first would buy nothing and has to be kept in step with
        // Attach, which is exactly the kind of drift worth not signing up for.
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _watcher = null;
    }

    public bool CanWrite => true;

    public IReadOnlyList<SkillEntry> Enumerate()
    {
        if (!Directory.Exists(_root)) return [];

        var entries = new List<SkillEntry>();
        try
        {
            Scan(_root, entries);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger?.LogWarning(ex, "Skill source unreadable. Source={SourceId} Path={Path}", Id, _root);
        }

        return entries;
    }

    /// <summary>
    /// Walks a folder, then its subfolders. Two things a subfolder can be, distinguished by whether
    /// it holds a SKILL.md:
    /// <list type="bullet">
    /// <item>a SKILL.md present → the folder IS one skill, and everything beside it is that skill's
    /// resources (scripts, references). Do not descend further.</item>
    /// <item>no SKILL.md → the folder is just organisation ("skills/network/", "skills/1c/") and the
    /// walk continues into it.</item>
    /// </list>
    /// Nesting is therefore free for grouping, and never ambiguous: a folder either declares itself a
    /// skill or it does not.
    /// </summary>
    private void Scan(string directory, List<SkillEntry> entries, bool isRoot = true)
    {
        var folderSkill = Path.Combine(directory, FolderSkillFile);
        if (!isRoot && File.Exists(folderSkill))
        {
            Add(entries, folderSkill, FallbackId(directory));
            return;
        }

        foreach (var file in Directory.EnumerateFiles(directory, "*.md", SearchOption.TopDirectoryOnly)
                                      .OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            // README.md is documentation ABOUT the folder everywhere else in the world; treating it
            // as a skill would put a description of the layout into the agent's skill index.
            if (Path.GetFileName(file).Equals("README.md", StringComparison.OrdinalIgnoreCase)) continue;

            Add(entries, file, FallbackId(Path.ChangeExtension(file, null)!));
        }

        foreach (var sub in Directory.EnumerateDirectories(directory)
                                     .OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
        {
            // Skip the noise that accumulates next to authored files.
            var name = Path.GetFileName(sub);
            if (name.StartsWith('.') || name is "bin" or "obj" or "node_modules") continue;

            Scan(sub, entries, isRoot: false);
        }
    }

    /// <summary>Id for a file that declares none: the path relative to the root, dotted. So
    /// <c>skills/network/dns.md</c> becomes <c>network.dns</c> — folder structure turns into an id
    /// namespace, matching how skills are already named, and two files with the same leaf name in
    /// different folders do not collide.</summary>
    private string FallbackId(string pathWithoutExtension) =>
        Path.GetRelativePath(_root, pathWithoutExtension)
            .Replace(Path.DirectorySeparatorChar, '.')
            .Replace(Path.AltDirectorySeparatorChar, '.');

    private void Add(List<SkillEntry> entries, string file, string fallbackId)
    {
        try
        {
            var skillRef = Path.GetRelativePath(_root, file).Replace('\\', '/');
            entries.Add(SkillFrontmatter.Parse(File.ReadAllText(file), fallbackId, skillRef));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger?.LogWarning(ex, "Skill file unreadable. Source={SourceId} File={File}", Id, file);
        }
    }

    public string? ReadBody(string skillRef)
    {
        var path = Resolve(skillRef);
        if (path is null || !File.Exists(path)) return null;

        try
        {
            return SkillFrontmatter.StripHeader(File.ReadAllText(path));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger?.LogWarning(ex, "Skill body unreadable. Source={SourceId} Ref={Ref}", Id, skillRef);
            return null;
        }
    }

    /// <summary>
    /// Everything beside a folder skill's <c>SKILL.md</c>, walked recursively — that is exactly the
    /// definition <see cref="Scan"/> already uses to stop descending, read from the other side.
    ///
    /// <para>A bare <c>name.md</c> in a grouping folder has no attachments at all, and deliberately so:
    /// its neighbours are other people's skills, not its vkladyshi. Carrying attachments is what the
    /// folder layout is FOR.</para>
    /// </summary>
    public IReadOnlyList<string> ListResources(string skillRef)
    {
        var root = ResourceRoot(skillRef);
        if (root is null) return [];

        var found = new List<string>();
        try
        {
            Collect(root, root, found);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger?.LogWarning(ex, "Skill resources unreadable. Source={SourceId} Ref={Ref}", Id, skillRef);
        }

        found.Sort(StringComparer.OrdinalIgnoreCase);
        return found;
    }

    public string? ReadResource(string skillRef, string resourcePath)
    {
        var root = ResourceRoot(skillRef);
        if (root is null) return null;

        var path = ResolveUnder(root, resourcePath);
        if (path is null || !File.Exists(path)) return null;

        // SKILL.md is the book, not an appendix. It is already pinned in the session, and letting it
        // back in through the resource door would just be a second way to read the same text.
        if (Path.GetFileName(path).Equals(FolderSkillFile, StringComparison.OrdinalIgnoreCase)) return null;

        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger?.LogWarning(ex, "Skill resource unreadable. Source={SourceId} Ref={Ref} Resource={Resource}",
                Id, skillRef, resourcePath);
            return null;
        }
    }

    /// <summary>The folder whose contents are this skill's attachments, or null when the skill has
    /// none — either the ref is unknown, or it names a bare file rather than a skill folder.</summary>
    private string? ResourceRoot(string skillRef)
    {
        var path = Resolve(skillRef);
        if (path is null || !File.Exists(path)) return null;
        if (!Path.GetFileName(path).Equals(FolderSkillFile, StringComparison.OrdinalIgnoreCase)) return null;

        return Path.GetDirectoryName(path);
    }

    private static void Collect(string root, string directory, List<string> found)
    {
        foreach (var file in Directory.EnumerateFiles(directory))
        {
            if (Path.GetFileName(file).Equals(FolderSkillFile, StringComparison.OrdinalIgnoreCase)) continue;
            found.Add(Path.GetRelativePath(root, file).Replace('\\', '/'));
        }

        foreach (var sub in Directory.EnumerateDirectories(directory))
        {
            // The same exclusions the scanner uses — noise beside authored files is not an appendix.
            var name = Path.GetFileName(sub);
            if (name.StartsWith('.') || name is "bin" or "obj" or "node_modules") continue;

            Collect(root, sub, found);
        }
    }

    public void WriteBody(string skillRef, string text)
    {
        var path = Resolve(skillRef)
            ?? throw new ArgumentException($"ref '{skillRef}' escapes source '{Id}'", nameof(skillRef));

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    /// <summary>Maps a ref back to a full path, refusing anything that would escape the root — refs
    /// can reach this source from a client, so "../../etc" must not resolve.</summary>
    private string? Resolve(string skillRef) => ResolveUnder(_root, skillRef);

    /// <summary>The containment check both refs and resource paths go through. Kept in one place so
    /// the attachment door cannot drift away from the door the skill ref already uses: an absolute
    /// path, a "..", or a symlink-free sideways hop all end up outside <paramref name="baseDir"/> and
    /// all resolve to null.</summary>
    private static string? ResolveUnder(string baseDir, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative)) return null;

        var full = Path.GetFullPath(Path.Combine(baseDir, relative));
        var prefix = baseDir.EndsWith(Path.DirectorySeparatorChar)
            ? baseDir
            : baseDir + Path.DirectorySeparatorChar;
        return full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? full : null;
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        lock (_gate)
        {
            if (_disposed) return;

            // Editors write a file in several bursts; collapse them into one reload.
            _debounce?.Dispose();
            _debounce = new Timer(_ => Fire(), null, DebounceMs, Timeout.Infinite);
        }
    }

    private void Fire()
    {
        // The root can be the thing that was removed. Re-arming here drops back to watching the
        // ancestor, so deleting and recreating the folder is survivable rather than one-way.
        if (!Directory.Exists(_root)) StartWatching();

        Changed?.Invoke();
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _disposed = true;
            DetachWatcher();
            _debounce?.Dispose();
            _debounce = null;
        }
    }
}
