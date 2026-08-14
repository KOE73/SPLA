using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPLA.Domain.Host;

/// <summary>The floor on one mount: may anything change what is behind it at all. A property of the
/// node, enforced on its own — the same mechanism as <c>allow_write</c> on an SSH host, and read-only
/// by default for the same reason: the operator opens a folder up deliberately.</summary>
public enum MountAccess
{
    Read,
    Write
}

/// <summary>Whether content arriving from a mount is named content.</summary>
public enum MountTrust
{
    /// <summary>Default. The operator wrote this folder into the manifest and said what it is for —
    /// that is what naming a source means, and it is the same standing an SSH host or a database
    /// connection gets.</summary>
    Trusted,

    /// <summary>For the one case that actually differs: a folder other people put files into — a
    /// shared drop, someone else's export, a downloads directory.</summary>
    Untrusted
}

/// <summary>
/// A folder outside the project root, declared in the manifest and addressed under a reserved
/// prefix: <c>mnt/&lt;name&gt;/...</c>.
///
/// <para>The name travels in git, the target is a property of this machine. That split is the whole
/// point — a relative path in an instruction has no base to be relative to, and an absolute one is
/// wrong on the second machine and unsayable on a server.</para>
///
/// <para>Resolved at manifest load and validated there once: see <c>MountResolver</c>. By the time
/// one of these exists, <see cref="HostPath"/> is absolute and has been through
/// <see cref="PathBoundary.RealPath"/>.</para>
/// </summary>
/// <param name="Name">The address segment: <c>mnt/{Name}/...</c>.</param>
/// <param name="HostPath">Absolute target on this machine, links already followed. Set even when
/// <see cref="IsAvailable"/> is false — what was asked for is worth reporting in the refusal.</param>
/// <param name="Description">Why this folder is here. Required, and it goes into the system prompt:
/// without it the model opens the folder to find out what it is.</param>
/// <param name="IsAvailable">Whether the target exists on this machine. A missing target is a
/// diagnosis of its own, never "file not found" — the reaction to an unplugged folder and to a
/// missing file are different things.</param>
public sealed record ProjectMount(
    string Name,
    string HostPath,
    MountAccess Access,
    MountTrust Trust,
    string Description,
    bool IsAvailable)
{
    /// <summary>The one reserved name in a project root. Reserving a single segment — rather than
    /// each mount's own name — is what keeps the check stable: it is one condition for the life of
    /// the project, so adding a mount never re-opens the question of what is already in the tree.
    /// </summary>
    public const string Prefix = "mnt";

    /// <summary>The only mount type. The field exists in the schema so a second one would not be a
    /// compatibility break; it costs one word and buys the question away.</summary>
    public const string FileSystemType = "file-system";

    /// <summary>This mount's canonical address, with <paramref name="relative"/> appended when given.
    /// Forward slashes always: the canonical form is what travels into prompts, chats and stored
    /// state, and it must read the same on every host.</summary>
    public string Address(string? relative = null)
    {
        var tail = relative?.Replace('\\', '/').Trim('/') ?? string.Empty;
        return tail.Length == 0 ? $"{Prefix}/{Name}" : $"{Prefix}/{Name}/{tail}";
    }
}
