using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPLA.Domain.Project;

/// <summary>
/// Local bucket: one directory, keys are file names. The directory is created lazily on first
/// write (or when a disk-bound consumer maps it), so opening buckets never litters the disk.
/// </summary>
public sealed class FileBucket : IBucket
{
    private readonly string _dir;

    public FileBucket(string name, string directory)
    {
        Name = name;
        _dir = directory;
    }

    public string Name { get; }

    private string PathFor(string key) => Path.Combine(_dir, key);

    public bool Exists(string key) => File.Exists(PathFor(key));

    public string? ReadText(string key)
        => File.Exists(PathFor(key)) ? File.ReadAllText(PathFor(key)) : null;

    public void WriteText(string key, string content)
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(PathFor(key), content);
    }

    public void Delete(string key)
    {
        if (File.Exists(PathFor(key))) File.Delete(PathFor(key));
    }

    public IReadOnlyList<string> ListKeys()
        => Directory.Exists(_dir)
            ? Directory.GetFiles(_dir).Select(f => Path.GetFileName(f)!).ToList()
            : [];

    public string? MapToHostDirectory()
    {
        Directory.CreateDirectory(_dir);
        return _dir;
    }
}
