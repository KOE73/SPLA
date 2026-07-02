using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SPLA.Domain.Settings;

namespace SPLA.Domain.Project;

/// <summary>
/// Local provider: known projects live in <c>~/.spla/registry.json</c> (id = manifest path).
/// Seeds itself once from the legacy <c>recent_projects.txt</c> so nothing already on the recent
/// list is lost. Missing manifests are filtered on read and pruned on the next write — a deleted
/// project disappears by itself.
/// </summary>
public sealed class LocalProjectProvider : IProjectProvider
{
    private readonly string _registryPath;
    private readonly string _legacyRecentPath;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <param name="stateDir">Directory holding the registry; defaults to <c>~/.spla</c>.</param>
    public LocalProjectProvider(string? stateDir = null)
    {
        var dir = stateDir ?? ConfigLoader.GetDefaultsDir();
        _registryPath = Path.Combine(dir, "registry.json");
        _legacyRecentPath = Path.Combine(dir, "recent_projects.txt");
    }

    private sealed class RegistryEntry
    {
        public string Path { get; set; } = "";
        public DateTimeOffset? LastOpened { get; set; }
    }

    public IReadOnlyList<ProjectDescriptor> List()
        => Load().Where(e => File.Exists(e.Path)).Select(Describe).ToList();

    public IReadOnlyList<ProjectDescriptor> Recent()
        => Load().Where(e => File.Exists(e.Path))
            .OrderByDescending(e => e.LastOpened ?? DateTimeOffset.MinValue)
            .Select(Describe)
            .ToList();

    public IProject Open(string id)
    {
        var manifestPath = Path.GetFullPath(id);
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException($"Project manifest not found: {manifestPath}");

        Touch(manifestPath);
        var settings = ConfigLoader.LoadAndResolve(manifestPath);
        return LocalProject.For(settings);
    }

    public IProject Create(ProjectDescriptor descriptor)
    {
        var manifestPath = Path.GetFullPath(
            descriptor.ManifestPath
            ?? throw new ArgumentException("A local project needs a manifest path.", nameof(descriptor)));

        if (!File.Exists(manifestPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
            ConfigLoader.SaveProject(new SplaProject { Name = descriptor.Name }, manifestPath);
        }
        ConfigLoader.ScaffoldIfNew(manifestPath);
        return Open(manifestPath);
    }

    private static ProjectDescriptor Describe(RegistryEntry e) => new()
    {
        Id = e.Path,
        Name = TryReadName(e.Path),
        ManifestPath = e.Path,
        LastOpened = e.LastOpened
    };

    private static string? TryReadName(string manifestPath)
    {
        try
        {
            return ConfigLoader.LoadProjectRaw(manifestPath).Name
                ?? Path.GetFileNameWithoutExtension(manifestPath);
        }
        catch { return Path.GetFileNameWithoutExtension(manifestPath); }
    }

    private void Touch(string manifestPath)
    {
        var entries = Load();
        var entry = entries.FirstOrDefault(e =>
            string.Equals(e.Path, manifestPath, StringComparison.OrdinalIgnoreCase));
        if (entry == null)
        {
            entry = new RegistryEntry { Path = manifestPath };
            entries.Add(entry);
        }
        entry.LastOpened = DateTimeOffset.UtcNow;
        Save(entries.Where(e => File.Exists(e.Path)).ToList());
    }

    private List<RegistryEntry> Load()
    {
        try
        {
            if (File.Exists(_registryPath))
                return JsonSerializer.Deserialize<List<RegistryEntry>>(
                    File.ReadAllText(_registryPath), JsonOptions) ?? [];

            // One-time seed from the legacy recent list (most recent first → newest timestamp first).
            if (File.Exists(_legacyRecentPath))
            {
                var now = DateTimeOffset.UtcNow;
                return File.ReadAllLines(_legacyRecentPath)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0)
                    .Select((path, i) => new RegistryEntry { Path = path, LastOpened = now.AddSeconds(-i) })
                    .ToList();
            }
        }
        catch { /* malformed registry — start empty rather than block every open */ }
        return [];
    }

    private void Save(List<RegistryEntry> entries)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_registryPath)!);
            File.WriteAllText(_registryPath, JsonSerializer.Serialize(entries, JsonOptions));
        }
        catch { /* best effort — the registry is a convenience, not a source of truth */ }
    }
}
