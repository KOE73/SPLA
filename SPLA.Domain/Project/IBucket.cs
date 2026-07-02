using System.Collections.Generic;

namespace SPLA.Domain.Project;

/// <summary>
/// Opaque named storage a project hands to a subsystem or plugin. The consumer sees string keys
/// and text values; where the data physically lives is the backend's business (local disk, DB
/// rows, memory). The project itself never knows what is inside a bucket — "sql" belongs to the
/// SQL plugin, "chats" to the chat manager.
/// </summary>
public interface IBucket
{
    string Name { get; }

    bool Exists(string key);

    /// <summary>Returns the stored text, or null when the key does not exist.</summary>
    string? ReadText(string key);

    void WriteText(string key, string content);

    void Delete(string key);

    IReadOnlyList<string> ListKeys();

    /// <summary>
    /// Real directory for disk-bound consumers that cannot speak key/value (sqlite files, browser
    /// profiles, log writers). Virtual/server backends return null and such consumers degrade —
    /// same staging as <see cref="Host.IWorkspace.MapPathToHost"/>.
    /// </summary>
    string? MapToHostDirectory();
}
