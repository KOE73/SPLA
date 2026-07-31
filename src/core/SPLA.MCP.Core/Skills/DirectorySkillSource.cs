using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SPLA.MCP.Core.Skills;

/// <summary>
/// Skills stored as markdown in a folder — the project's <c>.spla/skills</c> and the machine's
/// <c>~/.spla/skills</c>.
///
/// <para>Two layouts, both first-class: a bare <c>name.md</c> in the root is a simple skill, and a
/// subfolder containing <c>SKILL.md</c> is a skill that carries resources alongside it (scripts,
/// references). The second form exists so a long procedure can keep its attachments together; the
/// attachments themselves are not read yet.</para>
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
    private readonly FileSystemWatcher? _watcher;
    private Timer? _debounce;

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

        // A missing folder is the normal case (most projects have no .spla/skills), so it is not an
        // error and not created eagerly — but it must still be watched for, since creating it later
        // should light the source up without a restart. FileSystemWatcher cannot watch a path that
        // does not exist, so watching starts only once the folder is there.
        if (watch && Directory.Exists(_root))
        {
            _watcher = new FileSystemWatcher(_root, "*.md")
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                EnableRaisingEvents = true
            };
            _watcher.Changed += OnFileEvent;
            _watcher.Created += OnFileEvent;
            _watcher.Deleted += OnFileEvent;
            _watcher.Renamed += OnFileEvent;
        }
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

    public void WriteBody(string skillRef, string text)
    {
        var path = Resolve(skillRef)
            ?? throw new ArgumentException($"ref '{skillRef}' escapes source '{Id}'", nameof(skillRef));

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    /// <summary>Maps a ref back to a full path, refusing anything that would escape the root — refs
    /// can reach this source from a client, so "../../etc" must not resolve.</summary>
    private string? Resolve(string skillRef)
    {
        if (string.IsNullOrWhiteSpace(skillRef)) return null;

        var full = Path.GetFullPath(Path.Combine(_root, skillRef));
        var rootPrefix = _root.EndsWith(Path.DirectorySeparatorChar) ? _root : _root + Path.DirectorySeparatorChar;
        return full.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase) ? full : null;
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        // Editors write a file in several bursts; collapse them into one reload.
        _debounce?.Dispose();
        _debounce = new Timer(_ => Changed?.Invoke(), null, DebounceMs, Timeout.Infinite);
    }

    public void Dispose()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }
        _debounce?.Dispose();
    }
}
