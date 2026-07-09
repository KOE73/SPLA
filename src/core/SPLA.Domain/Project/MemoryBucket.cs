using System.Collections.Generic;
using System.Linq;

namespace SPLA.Domain.Project;

/// <summary>
/// A non-disk bucket: keys and text values live in a dictionary, nothing touches the file system.
/// The reference "there is no host path" backend — <see cref="MapToHostDirectory"/> returns null,
/// which is exactly the shape a server backend (DB rows, object storage, a per-user cloud folder)
/// presents. Any store that works over this works over those; the disk-bound path
/// (<c>MapToHostDirectory</c> != null) is the special case, not the rule.
/// </summary>
public sealed class MemoryBucket : IBucket
{
    private readonly Dictionary<string, string> _entries = new();

    public MemoryBucket(string name) => Name = name;

    public string Name { get; }

    public bool Exists(string key) => _entries.ContainsKey(key);

    public string? ReadText(string key) => _entries.TryGetValue(key, out var v) ? v : null;

    public void WriteText(string key, string content) => _entries[key] = content;

    public void Delete(string key) => _entries.Remove(key);

    public IReadOnlyList<string> ListKeys() => _entries.Keys.ToList();

    /// <summary>Always null — this bucket has no host directory. Disk-bound consumers must degrade;
    /// key/value consumers work unchanged.</summary>
    public string? MapToHostDirectory() => null;
}
