using SPLA.Domain.Agent;
using SPLA.Domain.Resources;
using SPLA.Domain.Settings;
using SPLA.Plugins.Geometry.Image;

namespace SPLA.Tests.Geometry;

/// <summary>
/// An image address arrives in one of three forms and the order they are tried in is part of the
/// contract: a handle first, then a scheme, then a plain path. A wrong address must come back as a
/// sentence the model can act on, never as an exception.
/// </summary>
public sealed class ImageSourceTests
{
    private static readonly byte[] Pixels = [1, 2, 3, 4, 5];

    private static IDisposable Scope() =>
        AgentSessionScope.Begin(new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession()));

    [Fact]
    public async Task A_blob_handle_is_read_from_the_chat_store()
    {
        using var _ = Scope();
        var handle = AgentSessionScope.Current!.Blobs.Put(
            BlobPayload.OfBytes(Pixels, "image/png"), "frame", SPLA.Domain.Security.DataOrigin.Internet);

        var loaded = await ImageSource.LoadAsync(handle, new ResolvedSettings());

        Assert.True(loaded.Ok);
        Assert.Equal(Pixels, loaded.Bytes);
        Assert.Equal(SPLA.Domain.Security.DataOrigin.Internet, loaded.Origin);
    }

    [Fact]
    public async Task An_unknown_blob_handle_answers_with_text()
    {
        using var _ = Scope();

        var loaded = await ImageSource.LoadAsync("blob:nothing-here", new ResolvedSettings());

        Assert.False(loaded.Ok);
        Assert.Contains("nothing-here", loaded.Error);
    }

    [Fact]
    public async Task An_address_with_a_scheme_goes_to_its_resource_provider()
    {
        var settings = new ResolvedSettings();
        ResourceRegistry.For(settings).Register(new FakeProvider(Pixels));

        var loaded = await ImageSource.LoadAsync("fake://frames/one.png", settings);

        Assert.True(loaded.Ok);
        Assert.Equal(Pixels, loaded.Bytes);
    }

    [Fact]
    public async Task An_unserved_scheme_answers_with_text_naming_what_is_served()
    {
        var settings = new ResolvedSettings();
        ResourceRegistry.For(settings).Register(new FakeProvider(Pixels));

        var loaded = await ImageSource.LoadAsync("nosuch://frames/one.png", settings);

        Assert.False(loaded.Ok);
        Assert.Contains("nosuch", loaded.Error);
    }

    [Fact]
    public async Task A_provider_that_throws_answers_with_text()
    {
        var settings = new ResolvedSettings();
        ResourceRegistry.For(settings).Register(new FakeProvider(Pixels, throws: true));

        var loaded = await ImageSource.LoadAsync("fake://frames/one.png", settings);

        Assert.False(loaded.Ok);
        Assert.Contains("no frame there", loaded.Error);
    }

    [Fact]
    public async Task A_plain_path_is_read_through_the_workspace()
    {
        var path = Path.Combine(Path.GetTempPath(), $"spla-geometry-{Guid.NewGuid():N}.png");
        await File.WriteAllBytesAsync(path, Pixels);
        try
        {
            var loaded = await ImageSource.LoadAsync(path, new ResolvedSettings());

            Assert.True(loaded.Ok);
            Assert.Equal(Pixels, loaded.Bytes);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task A_missing_path_answers_with_text()
    {
        var path = Path.Combine(Path.GetTempPath(), $"spla-geometry-missing-{Guid.NewGuid():N}.png");

        var loaded = await ImageSource.LoadAsync(path, new ResolvedSettings());

        Assert.False(loaded.Ok);
        Assert.Contains("Could not read", loaded.Error);
    }

    [Fact]
    public async Task An_empty_address_answers_with_text()
    {
        var loaded = await ImageSource.LoadAsync("   ", new ResolvedSettings());

        Assert.False(loaded.Ok);
        Assert.False(string.IsNullOrWhiteSpace(loaded.Error));
    }

    /// <summary>A read-only provider standing in for whatever scheme the project has registered.</summary>
    private sealed class FakeProvider(byte[] bytes, bool throws = false) : IResourceProvider
    {
        public string Scheme => "fake";
        public string Summary => "test frames — fake://frames/one.png";

        public Task<ResourceContent> ReadAsync(ResourceUri uri, CancellationToken ct = default)
            => throws
                ? throw new InvalidOperationException("no frame there")
                : Task.FromResult(new ResourceContent(bytes, "image/png"));

        public Task<bool> ExistsAsync(ResourceUri uri, CancellationToken ct = default) => Task.FromResult(!throws);

        public Task<IReadOnlyList<ResourceEntry>> ListAsync(ResourceUri uri, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<ResourceEntry>>([]);
    }
}
