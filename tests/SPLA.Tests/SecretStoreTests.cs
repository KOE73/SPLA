using SPLA.Domain.Secrets;
using SPLA.Domain.Settings;
using SPLA.Secrets.Dpapi;

namespace SPLA.Tests;

/// <summary>
/// DPAPI secret store + backend selection. DPAPI is Windows-only, so those cases no-op elsewhere.
/// All file I/O is confined to a throwaway temp directory — never the real ~/.spla.
/// </summary>
public class SecretStoreTests : IDisposable
{
    private readonly string _dir;

    public SecretStoreTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "spla-secrets-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { /* best effort */ }
    }

    [Fact]
    public async Task Dpapi_RoundTrips_And_EncryptsAtRest()
    {
        if (!OperatingSystem.IsWindows()) return;

        ISecretStore store = new DpapiFileSecretStore(workspacePath: null, machineDir: _dir);
        await store.SetAsync("ssh/box/koe", "test-pw-value", SecretScope.Machine);

        Assert.Equal("test-pw-value", await store.GetAsync("ssh/box/koe"));

        // On disk it must NOT be plaintext, and it must carry the dpapi: marker.
        var raw = await File.ReadAllTextAsync(Path.Combine(_dir, "secrets.dpapi.yaml"));
        Assert.Contains("dpapi:", raw);
        Assert.DoesNotContain("test-pw-value", raw);
    }

    [Fact]
    public async Task Dpapi_ListKeys_DoesNotRequireDecryption()
    {
        if (!OperatingSystem.IsWindows()) return;

        ISecretStore store = new DpapiFileSecretStore(null, _dir);
        await store.SetAsync("a/one", "v1", SecretScope.Machine);
        await store.SetAsync("b/two", "v2", SecretScope.Machine);

        // Corrupt one value — listing must still return both keys.
        var file = Path.Combine(_dir, "secrets.dpapi.yaml");
        await File.WriteAllTextAsync(file, "a/one: dpapi:not-valid-base64!!!\nb/two: dpapi:alsobad\n");

        var keys = await store.ListKeysAsync(SecretScope.Machine);
        Assert.Equal(new[] { "a/one", "b/two" }, keys);

        // And the corrupt value is treated as absent, not thrown.
        Assert.Null(await store.GetAsync("a/one"));
    }

    [Fact]
    public async Task Dpapi_ProjectScope_OverridesMachine()
    {
        if (!OperatingSystem.IsWindows()) return;

        var ws = Path.Combine(_dir, "workspace");
        Directory.CreateDirectory(ws);
        ISecretStore store = new DpapiFileSecretStore(ws, _dir);

        await store.SetAsync("k", "machine-val", SecretScope.Machine);
        await store.SetAsync("k", "project-val", SecretScope.Project);

        Assert.Equal("project-val", await store.GetAsync("k"));
        Assert.Equal("machine-val", await store.GetAsync("k", SecretScope.Machine));
    }

    [Fact]
    public async Task Dpapi_Delete_ReportsExistence()
    {
        if (!OperatingSystem.IsWindows()) return;

        ISecretStore store = new DpapiFileSecretStore(null, _dir);
        await store.SetAsync("k", "v", SecretScope.Machine);

        Assert.True(await store.DeleteAsync("k", SecretScope.Machine));
        Assert.False(await store.DeleteAsync("k", SecretScope.Machine));
        Assert.Null(await store.GetAsync("k"));
    }

    [Fact]
    public async Task Entry_RoundTrips_MultipleFields_And_ResolvesByField()
    {
        ISecretStore store = new FileSecretStore(workspacePath: null, machineDir: _dir);
        await store.SetEntryAsync("box", new Dictionary<string, string>
        {
            ["user"] = "koe",
            ["password"] = "pw-value"
        }, SecretScope.Machine);

        var entry = await store.GetEntryAsync("box");
        Assert.NotNull(entry);
        Assert.Equal("koe", entry!["user"]);
        Assert.Equal(new[] { "password", "user" }, entry.FieldNames);
        // Bare secret:KEY prefers the password field; #field picks explicitly.
        Assert.Equal("pw-value", entry.DefaultValue);

        var resolver = new SecretResolver(store);
        Assert.Equal("koe", await resolver.ResolveAsync("secret:box#user"));
        Assert.Equal("pw-value", await resolver.ResolveAsync("secret:box"));
        Assert.Null(await resolver.ResolveAsync("secret:box#missing"));

        // Listing exposes field names, never values.
        var listed = Assert.Single(await store.ListEntriesAsync(SecretScope.Machine));
        Assert.Equal("box", listed.Key);
        Assert.Equal(new[] { "password", "user" }, listed.Fields);
    }

    [Fact]
    public async Task Entry_LegacyScalarFile_ReadsAsPasswordField()
    {
        // Pre-entry flat "key: value" shape degrades to a single password field instead of vanishing.
        await File.WriteAllTextAsync(Path.Combine(_dir, "secrets.yaml"), "old/key: old-value\n");
        ISecretStore store = new FileSecretStore(null, _dir);

        Assert.Equal("old-value", await store.GetAsync("old/key"));
        var entry = await store.GetEntryAsync("old/key");
        Assert.Equal(new[] { "password" }, entry!.FieldNames);
    }

    [Fact]
    public void ConfigLoader_UsesFactory_ForDpapi_AndFallsBackToFile()
    {
        var prevHome = Environment.GetEnvironmentVariable("SPLA_HOME");
        var prevFactory = ConfigLoader.SecretStoreFactory;
        try
        {
            // SPLA_HOME redirects the whole machine layer into our temp dir — proves the override AND
            // keeps the test off the real ~/.spla.
            Environment.SetEnvironmentVariable("SPLA_HOME", _dir);
            File.WriteAllText(Path.Combine(_dir, "defaults.yaml"), "version: 1\nsecrets:\n  backend: dpapi\n");

            // With a factory that supplies dpapi, the resolved store is the DPAPI one.
            ConfigLoader.SecretStoreFactory = (backend, ws, machineDir) =>
                backend == "dpapi" ? new DpapiFileSecretStore(ws, machineDir) : null;
            var resolved = ConfigLoader.LoadAndResolve();
            Assert.IsType<DpapiFileSecretStore>(resolved.Secrets);

            // With no factory, the same config must fall back to the plaintext file store, not crash.
            ConfigLoader.SecretStoreFactory = null;
            var fallback = ConfigLoader.LoadAndResolve();
            Assert.IsType<FileSecretStore>(fallback.Secrets);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SPLA_HOME", prevHome);
            ConfigLoader.SecretStoreFactory = prevFactory;
        }
    }
}
