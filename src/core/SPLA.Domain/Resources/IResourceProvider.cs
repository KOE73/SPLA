using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Domain.Resources;

/// <summary>
/// What can be done to a resource. The list is deliberately short and deliberately file-shaped:
/// these are the operations a model already understands without being taught, because it has seen
/// them a million times over ordinary filesystems.
///
/// <para><b>The admission test for a new member</b> (the rule this set is maintained under): if
/// using the verb needs more than the prompt of its own schema — if it needs caveats, per-scheme
/// exceptions, or a bag of options to become usable — then it is either a bad verb or a bad schema,
/// and it does not go in. That is why there is no <c>Query</c> and no <c>Search</c> here: "name what
/// lies at this address" is a verb, "find whatever matches this condition" is a query language, and
/// pretending the second is the first is how an abstraction acquires an <c>ioctl</c>.</para>
/// </summary>
public enum ResourceVerb
{
    /// <summary>Base. Content out of the resource.</summary>
    Read,

    /// <summary>Base. Whether anything is at this address.</summary>
    Exists,

    /// <summary>Base. Plain <c>ls</c> — what lies directly under this address. Not a search, not a
    /// filter, not recursive.</summary>
    List,

    /// <summary>Extended. Content in, creating or overwriting.</summary>
    Write,

    /// <summary>Extended. Remove what is at this address.</summary>
    Delete,

    /// <summary>Extended. Make a container. Meaningless for stores whose containers are implied by
    /// their keys (S3 prefixes, KV namespaces), which is precisely why it is not in the base set.</summary>
    MakeDir
}

/// <summary>One entry in a listing.</summary>
/// <param name="Name">The last segment only — the caller already knows the address it asked about.</param>
/// <param name="IsContainer">A directory, prefix or namespace rather than content.</param>
/// <param name="Size">Bytes, when the store knows cheaply. Null rather than 0 when it does not:
/// a guessed size is worse than an absent one.</param>
public readonly record struct ResourceEntry(string Name, bool IsContainer, long? Size = null);

/// <summary>
/// Content plus the one thing raw bytes never carry: what they are.
///
/// <para>Read as a Content-Type, not as a scheme. The address says WHERE something lives; this says
/// WHAT came back — the same split the web settled on thirty years ago, and the reason there is no
/// <c>image://</c> scheme here. One address can legitimately answer with a picture today and text
/// tomorrow (a browser tab), which an address-encoded type could not express without splitting the
/// address in two.</para>
///
/// <para><b>The type decides routing, never decoding.</b> Nothing in this assembly opens the bytes:
/// a PNG is decoded to pixels by the vision backend on the far side, a docx by a converter living in
/// a plugin. Core reads the label; it does not read the contents. That line is what keeps FFmpeg and
/// half a zip inflater out of the foundation eleven other projects depend on.</para>
/// </summary>
/// <param name="ContentType">A MIME type. Never null and never empty — a provider that does not know
/// says <c>application/octet-stream</c> out loud, because an absent type puts the caller back to
/// guessing, and guessing is what naming things was supposed to end.</param>
public readonly record struct ResourceContent(byte[] Bytes, string ContentType);

/// <summary>
/// A storage behind one URI scheme.
///
/// <para><b>The base set is mandatory</b> — a provider that cannot read, test existence and list is
/// not registered at all. This is not strictness for its own sake: the alternative is a scheme whose
/// verbs fail at call time, which puts the model back to discovering affordances by triggering
/// errors — the exact trap the whole URI idea exists to escape.</para>
///
/// <para><b>Everything beyond the base set is a separate interface</b>, and support is read off the
/// type (<c>provider is IResourceWriter</c>) rather than off a flag the provider sets. A declared
/// capability can drift from the truth; an implemented interface cannot. Adding a verb therefore
/// costs one interface plus one line in <see cref="ResourceRegistry"/>'s matrix — which is the whole
/// design requirement for the verb set: cheap to extend, impossible to lie about.</para>
/// </summary>
public interface IResourceProvider
{
    /// <summary>The scheme this serves, lowercased and without the colon: <c>file</c>, <c>sftp</c>.</summary>
    string Scheme { get; }

    /// <summary>One line for the system prompt: what lives under this scheme and how it is addressed.
    /// Shown to the model alongside the verb matrix, so it reads as an example, not as prose —
    /// e.g. "project files and declared mounts — file:///src/app.cs, file:///mnt/AAA/nginx.conf".</summary>
    string Summary { get; }

    /// <summary>Content out of the resource, labelled with what it is — see
    /// <see cref="ResourceContent"/> for why the label travels with the bytes rather than being
    /// re-derived by every caller.</summary>
    Task<ResourceContent> ReadAsync(ResourceUri uri, CancellationToken ct = default);

    Task<bool> ExistsAsync(ResourceUri uri, CancellationToken ct = default);

    /// <summary>Direct children only. Never recursive — see the note on <see cref="ResourceVerb.List"/>.</summary>
    Task<IReadOnlyList<ResourceEntry>> ListAsync(ResourceUri uri, CancellationToken ct = default);
}

/// <summary>Extended verb: <see cref="ResourceVerb.Write"/>.</summary>
public interface IResourceWriter
{
    /// <param name="overwrite">False means "create only" — the provider refuses an existing address
    /// rather than replacing it. Kept explicit because the two are different intentions and the
    /// filesystem tools already distinguish them (<c>system_create_file</c> vs <c>system_write_file</c>).</param>
    Task WriteAsync(ResourceUri uri, byte[] content, bool overwrite, CancellationToken ct = default);
}

/// <summary>Extended verb: <see cref="ResourceVerb.Delete"/>.</summary>
public interface IResourceRemover
{
    Task DeleteAsync(ResourceUri uri, CancellationToken ct = default);
}

/// <summary>Extended verb: <see cref="ResourceVerb.MakeDir"/>.</summary>
public interface IResourceContainerMaker
{
    Task MakeDirAsync(ResourceUri uri, CancellationToken ct = default);
}
