using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPLA.Domain.Host;
using SPLA.Domain.Resources;
using SPLA.Domain.Resources.Providers;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The URI parsing rules on their own, decoupled from any provider: whatever a scheme's own tests
/// exercise, every scheme has to agree on what <c>scheme://authority/path</c> means before it gets a
/// chance to disagree about what to do with it.
/// </summary>
public sealed class ResourceUriTests
{
    [Theory]
    [InlineData("file:///src/app.cs", "file", "", "src/app.cs", "file:///src/app.cs")]
    [InlineData("sftp://ioBroker/etc/nginx.conf", "sftp", "ioBroker", "etc/nginx.conf", "sftp://ioBroker/etc/nginx.conf")]
    // The opaque form parses to an empty authority like file:// does, so ToString() — which cannot
    // tell "opaque" from "empty authority" apart — renders it back out in the file-style three-slash
    // shape rather than the original one-colon shape. That is a property of the printed form, not of
    // parsing: the fields below are what a provider actually acts on, and they are exactly right.
    [InlineData("blob:handle123", "blob", "", "handle123", "blob:///handle123")]
    public void Round_trips_every_shape_the_scheme_set_actually_uses(
        string text, string scheme, string authority, string path, string printed)
    {
        Assert.True(ResourceUri.TryParse(text, out var uri, out var error));
        Assert.Null(error);
        Assert.Equal(scheme, uri.Scheme);
        Assert.Equal(authority, uri.Authority);
        Assert.Equal(path, uri.Path);
        Assert.Equal(printed, uri.ToString());
    }
}

/// <summary>
/// <see cref="FileResourceProvider"/> against a real temp workspace — the same
/// <see cref="LocalWorkspace"/>/<see cref="PathBoundary"/> pairing <c>WorkspaceBoundaryTests</c> and
/// <c>MountZoneTests</c> already use, because this provider is deliberately a second doorway onto
/// that same seam and not a store of its own.
/// </summary>
public sealed class FileResourceProviderTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "spla-res-" + Guid.NewGuid().ToString("N"));
    private readonly string _aaa = Path.Combine(Path.GetTempPath(), "spla-res-aaa-" + Guid.NewGuid().ToString("N"));

    public FileResourceProviderTests()
    {
        Directory.CreateDirectory(_root);
        Directory.CreateDirectory(_aaa);
        File.WriteAllText(Path.Combine(_root, "app.cs"), "// project file");
        Directory.CreateDirectory(Path.Combine(_root, "src"));
        File.WriteAllText(Path.Combine(_root, "src", "inner.cs"), "// nested");
        File.WriteAllText(Path.Combine(_aaa, "nginx.conf"), "worker_processes 4;");
    }

    public void Dispose()
    {
        foreach (var dir in new[] { _root, _aaa })
            try { Directory.Delete(dir, recursive: true); } catch { /* best effort */ }
    }

    private ProjectMount Aaa => new("AAA", _aaa, MountAccess.Write, MountTrust.Trusted, "reference settings", true);

    private LocalWorkspace NewWorkspace()
        => new(new PathBoundary(_root, null, [Aaa]));

    private FileResourceProvider NewProvider(LocalWorkspace ws)
        => new(() => ws);

    private static ResourceUri Parse(string text)
    {
        Assert.True(ResourceUri.TryParse(text, out var uri, out _));
        return uri;
    }

    [Fact]
    public async Task Reads_a_project_file_by_its_logical_path()
    {
        var provider = NewProvider(NewWorkspace());

        var content = await provider.ReadAsync(Parse("file:///app.cs"));

        Assert.Equal("// project file", Encoding.UTF8.GetString(content.Bytes));
        Assert.StartsWith("text/plain", content.ContentType);
    }

    [Fact]
    public async Task Exists_answers_for_both_files_and_directories()
    {
        var provider = NewProvider(NewWorkspace());

        Assert.True(await provider.ExistsAsync(Parse("file:///app.cs")));
        Assert.True(await provider.ExistsAsync(Parse("file:///src")));
        Assert.False(await provider.ExistsAsync(Parse("file:///nope.txt")));
    }

    [Fact]
    public async Task Writes_a_new_file_creating_parent_directories_first()
    {
        var provider = NewProvider(NewWorkspace());

        await provider.WriteAsync(Parse("file:///new/nested/note.txt"), Encoding.UTF8.GetBytes("hi"), overwrite: false);

        Assert.Equal("hi", File.ReadAllText(Path.Combine(_root, "new", "nested", "note.txt")));
    }

    /// <summary>The create-only half of <see cref="IResourceWriter.WriteAsync"/>: refusing an
    /// existing address is the whole reason the flag exists, and a provider that silently replaced
    /// instead would make the flag decorative.</summary>
    [Fact]
    public async Task Overwrite_false_refuses_an_existing_file_and_leaves_it_untouched()
    {
        var provider = NewProvider(NewWorkspace());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.WriteAsync(Parse("file:///app.cs"), Encoding.UTF8.GetBytes("replaced"), overwrite: false));

        Assert.Equal("// project file", File.ReadAllText(Path.Combine(_root, "app.cs")));
    }

    [Fact]
    public async Task Overwrite_true_replaces_an_existing_file()
    {
        var provider = NewProvider(NewWorkspace());

        await provider.WriteAsync(Parse("file:///app.cs"), Encoding.UTF8.GetBytes("replaced"), overwrite: true);

        Assert.Equal("replaced", File.ReadAllText(Path.Combine(_root, "app.cs")));
    }

    [Fact]
    public async Task Deletes_a_file()
    {
        var provider = NewProvider(NewWorkspace());
        await provider.DeleteAsync(Parse("file:///app.cs"));

        Assert.False(File.Exists(Path.Combine(_root, "app.cs")));
    }

    [Fact]
    public async Task Makes_a_directory()
    {
        var provider = NewProvider(NewWorkspace());
        await provider.MakeDirAsync(Parse("file:///made"));

        Assert.True(Directory.Exists(Path.Combine(_root, "made")));
    }

    /// <summary>A mount is reached through the same logical path space as everything else, with no
    /// separate scheme or mapping — <c>mnt/&lt;name&gt;/...</c> is already inside what the workspace
    /// accepts, and <see cref="ResourceUri.Path"/> having stripped the leading slash is the only
    /// translation the provider needs to do.</summary>
    [Fact]
    public async Task A_mount_style_path_resolves_through_the_same_workspace()
    {
        var provider = NewProvider(NewWorkspace());

        var content = await provider.ReadAsync(Parse("file:///mnt/AAA/nginx.conf"));

        Assert.Equal("worker_processes 4;", Encoding.UTF8.GetString(content.Bytes));
    }

    [Fact]
    public async Task Lists_direct_children_as_containers_and_files()
    {
        var provider = NewProvider(NewWorkspace());

        var entries = await provider.ListAsync(Parse("file:///."));

        var src = Assert.Single(entries, e => e.Name == "src");
        Assert.True(src.IsContainer);

        var app = Assert.Single(entries, e => e.Name == "app.cs");
        Assert.False(app.IsContainer);

        // Non-recursive: the nested file must not surface at the root listing.
        Assert.DoesNotContain(entries, e => e.Name == "inner.cs");
    }

    /// <summary>
    /// <c>file:///</c> is the obvious spelling of the root, and PathBoundary refuses an empty path
    /// outright — so without the provider spelling the root once, the most natural address anyone
    /// could write would be the single address that cannot work, discovered only by being refused.
    /// Both spellings must therefore land on the same place.
    /// </summary>
    [Fact]
    public async Task The_bare_root_address_means_the_project_root()
    {
        var provider = NewProvider(NewWorkspace());

        var bare = await provider.ListAsync(Parse("file:///"));
        var dotted = await provider.ListAsync(Parse("file:///."));

        Assert.Equal(
            dotted.Select(e => e.Name).OrderBy(n => n),
            bare.Select(e => e.Name).OrderBy(n => n));
    }

    /// <summary>
    /// The workspace can only be written as text today, so bytes that are not valid UTF-8 cannot
    /// survive the trip. Refusing is the point: writing them anyway would corrupt the file silently
    /// and the only symptom would appear much later, somewhere else.
    /// </summary>
    [Fact]
    public async Task Refuses_content_that_utf8_cannot_carry_rather_than_corrupting_it()
    {
        var provider = NewProvider(NewWorkspace());

        // A lone continuation byte — not reachable by encoding any string, so the round-trip fails.
        byte[] notUtf8 = [0xC3, 0x28, 0xA9];

        await Assert.ThrowsAsync<NotSupportedException>(
            () => provider.WriteAsync(Parse("file:///binary.bin"), notUtf8, overwrite: true));

        Assert.False(File.Exists(Path.Combine(_root, "binary.bin")));
    }

    /// <summary>
    /// The bytes outrank the name. A PNG that someone saved as <c>.txt</c> is still a PNG, and a
    /// reader that believed the extension would hand a vision backend something it cannot decode —
    /// which is the whole reason the read carries a type at all rather than leaving each caller to
    /// guess from the address.
    /// </summary>
    [Fact]
    public async Task A_signature_outranks_a_lying_extension()
    {
        byte[] png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x01, 0x02, 0x03];
        File.WriteAllBytes(Path.Combine(_root, "shot.png"), png);
        File.WriteAllBytes(Path.Combine(_root, "not-really.txt"), png);
        var provider = NewProvider(NewWorkspace());

        Assert.Equal("image/png", (await provider.ReadAsync(Parse("file:///shot.png"))).ContentType);
        Assert.Equal("image/png", (await provider.ReadAsync(Parse("file:///not-really.txt"))).ContentType);
    }

    /// <summary>
    /// The case the old byte[]-returning read could not express at all: no extension, no signature,
    /// and yet plainly text. Answering "octet-stream" there would push the caller back into the
    /// guessing the type was introduced to end.
    /// </summary>
    [Fact]
    public async Task A_file_with_no_extension_is_recognised_as_text_from_its_bytes()
    {
        File.WriteAllText(Path.Combine(_root, "LICENSE"), "Permission is hereby granted.");
        var provider = NewProvider(NewWorkspace());

        var content = await provider.ReadAsync(Parse("file:///LICENSE"));

        Assert.StartsWith("text/plain", content.ContentType);
    }

    /// <summary>And the other half of the same rule: unrecognised bytes with nothing to go on say so
    /// out loud rather than being optimistically called text.</summary>
    [Fact]
    public async Task Unrecognised_binary_with_no_extension_is_octet_stream()
    {
        File.WriteAllBytes(Path.Combine(_root, "blob"), [0x00, 0x01, 0x02, 0xFF, 0xFE, 0x7F, 0x03]);
        var provider = NewProvider(NewWorkspace());

        var content = await provider.ReadAsync(Parse("file:///blob"));

        Assert.Equal("application/octet-stream", content.ContentType);
    }

    [Fact]
    public async Task Listing_a_file_instead_of_a_directory_is_refused_clearly()
    {
        var provider = NewProvider(NewWorkspace());

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.ListAsync(Parse("file:///app.cs")));
    }

    [Fact]
    public async Task An_authority_is_refused_because_file_has_none()
    {
        var provider = NewProvider(NewWorkspace());
        var uri = Parse("file://somehost/app.cs");

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => provider.ExistsAsync(uri));
        Assert.Contains("file:///", ex.Message);
    }

    /// <summary>A boundary refusal (the cutout, a mount rule) is a decision made below this
    /// provider, on the same seam every filesystem tool already goes through — this provider must
    /// not catch and re-wrap it, or a model told "the file could not be read" would go looking for a
    /// fault instead of respecting a boundary it was never allowed past.</summary>
    [Fact]
    public async Task A_workspace_boundary_refusal_propagates_unmodified()
    {
        Directory.CreateDirectory(Path.Combine(_root, ".spla"));
        File.WriteAllText(Path.Combine(_root, ".spla", "secrets.yaml"), "password: hunter2");
        var provider = NewProvider(new LocalWorkspace(new PathBoundary(_root, [".spla"], [Aaa])));

        var refusal = await Assert.ThrowsAsync<PathBoundaryException>(
            () => provider.ReadAsync(Parse("file:///.spla/secrets.yaml")));

        Assert.Equal(PathRefusal.Cutout, refusal.Refusal);
    }
}

/// <summary>
/// <see cref="ResourceRegistry"/>'s own invariant — one provider per scheme — and the line it draws
/// between a genuine clash and mere re-entry. Pinned against a real provider rather than a stub,
/// since <see cref="FileResourceProvider"/> is the first scheme wired through it.
/// </summary>
public sealed class ResourceRegistryDuplicateSchemeTests
{
    /// <summary>A second, deliberately different type claiming a taken scheme.</summary>
    private sealed class RivalFileProvider : IResourceProvider
    {
        public string Scheme => "file";
        public string Summary => "a rival claim on the same scheme";

        public Task<ResourceContent> ReadAsync(ResourceUri uri, System.Threading.CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<bool> ExistsAsync(ResourceUri uri, System.Threading.CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<System.Collections.Generic.IReadOnlyList<ResourceEntry>> ListAsync(
            ResourceUri uri, System.Threading.CancellationToken ct = default)
            => throw new NotSupportedException();
    }

    [Fact]
    public void A_different_provider_claiming_a_taken_scheme_throws()
    {
        var resources = ResourceRegistry.For(new SPLA.Domain.Settings.ResolvedSettings());
        resources.Register(new FileResourceProvider(() => new LocalWorkspace()));

        var ex = Assert.Throws<InvalidOperationException>(
            () => resources.Register(new RivalFileProvider()));

        Assert.Contains("file", ex.Message);
    }

    /// <summary>
    /// Re-entry is not a clash. A second <c>AgentRuntime</c> over one <c>ResolvedSettings</c> is an
    /// ordinary thing for tests and reloads to do, and turning it into a startup crash would punish
    /// a benign sequence — so the same provider type arriving twice is a no-op, and the scheme keeps
    /// working afterwards rather than merely failing quietly.
    /// </summary>
    [Fact]
    public void The_same_provider_type_registering_twice_is_re_entry_not_a_clash()
    {
        var resources = ResourceRegistry.For(new SPLA.Domain.Settings.ResolvedSettings());

        resources.Register(new FileResourceProvider(() => new LocalWorkspace()));
        resources.Register(new FileResourceProvider(() => new LocalWorkspace()));

        Assert.True(resources.TryResolve("file:///src/app.cs", out _, out _, out _));
        Assert.Single(resources.Cards());
    }
}
