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

        ISecretStore store = new DpapiFileSecretStore(workspacePath: null, userDir: _dir);
        await store.SetAsync("ssh/box/koe", "test-pw-value", SecretScope.User);

        Assert.Equal("test-pw-value", await store.GetAsync("ssh/box/koe", SecretScope.User));

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
        await store.SetAsync("a/one", "v1", SecretScope.User);
        await store.SetAsync("b/two", "v2", SecretScope.User);

        // Corrupt one value — listing must still return both keys.
        var file = Path.Combine(_dir, "secrets.dpapi.yaml");
        await File.WriteAllTextAsync(file, "a/one: dpapi:not-valid-base64!!!\nb/two: dpapi:alsobad\n");

        var keys = await store.ListKeysAsync(SecretScope.User);
        Assert.Equal(new[] { "a/one", "b/two" }, keys);

        // And the corrupt value is treated as absent, not thrown.
        Assert.Null(await store.GetAsync("a/one", SecretScope.User));
    }

    [Fact]
    public async Task Scopes_DoNotShadowEachOther()
    {
        if (!OperatingSystem.IsWindows()) return;

        var ws = Path.Combine(_dir, "workspace");
        Directory.CreateDirectory(ws);
        ISecretStore store = new DpapiFileSecretStore(ws, _dir);

        await store.SetAsync("k", "user-val", SecretScope.User);
        await store.SetAsync("k", "project-val", SecretScope.Project);

        // The same key in two scopes is legal and unambiguous — each is only ever reachable by
        // naming its scope. This is the whole point of dropping the search: nothing "wins".
        Assert.Equal("user-val", await store.GetAsync("k", SecretScope.User));
        Assert.Equal("project-val", await store.GetAsync("k", SecretScope.Project));
        Assert.Null(await store.GetAsync("k", SecretScope.Shared));
    }

    [Fact]
    public async Task Dpapi_Delete_ReportsExistence()
    {
        if (!OperatingSystem.IsWindows()) return;

        ISecretStore store = new DpapiFileSecretStore(null, _dir);
        await store.SetAsync("k", "v", SecretScope.User);

        Assert.True(await store.DeleteAsync("k", SecretScope.User));
        Assert.False(await store.DeleteAsync("k", SecretScope.User));
        Assert.Null(await store.GetAsync("k", SecretScope.User));
    }

    [Fact]
    public async Task Entry_RoundTrips_MultipleFields_And_ResolvesByField()
    {
        ISecretStore store = new FileSecretStore(workspacePath: null, userDir: _dir);
        await store.SetEntryAsync("box", new Dictionary<string, string>
        {
            ["user"] = "koe",
            ["password"] = "pw-value"
        }, SecretScope.User);

        var entry = await store.GetEntryAsync("box", SecretScope.User);
        Assert.NotNull(entry);
        Assert.Equal("koe", entry!["user"]);
        Assert.Equal(new[] { "password", "user" }, entry.FieldNames);
        // Bare secret:KEY prefers the password field; #field picks explicitly.
        Assert.Equal("pw-value", entry.DefaultValue);

        var resolver = new SecretResolver(store);
        Assert.Equal("koe", await resolver.ResolveAsync("secret:user:box#user"));
        Assert.Equal("pw-value", await resolver.ResolveAsync("secret:user:box"));
        Assert.Null(await resolver.ResolveAsync("secret:user:box#missing"));

        // A scope-less reference is an error, never a search.
        await Assert.ThrowsAsync<FormatException>(
            async () => await resolver.ResolveAsync("secret:box"));
        await Assert.ThrowsAsync<FormatException>(
            async () => await resolver.ResolveAsync("secret:nosuchscope:box"));

        // Listing exposes field names, never values.
        var listed = Assert.Single(await store.ListEntriesAsync(SecretScope.User));
        Assert.Equal("box", listed.Key);
        Assert.Equal(new[] { "password", "user" }, listed.Fields);
    }

    [Fact]
    public async Task Entry_LegacyScalarFile_ReadsAsPasswordField()
    {
        // Pre-entry flat "key: value" shape degrades to a single password field instead of vanishing.
        await File.WriteAllTextAsync(Path.Combine(_dir, "secrets.yaml"), "old/key: old-value\n");
        ISecretStore store = new FileSecretStore(null, _dir);

        Assert.Equal("old-value", await store.GetAsync("old/key", SecretScope.User));
        var entry = await store.GetEntryAsync("old/key", SecretScope.User);
        Assert.Equal(new[] { "password" }, entry!.FieldNames);
    }

    /// <summary>
    /// Redirected through <see cref="MachineLayerScope"/> rather than by setting <c>SPLA_HOME</c> and
    /// the static factory. Both of those are process-wide, and this test used to hold them for the
    /// length of its body — during which every other class running in parallel resolved ITS machine
    /// layer to this test's temp directory, one that is deleted on the way out. That was a real
    /// intermittent failure, not a theoretical one.
    /// </summary>
    [Fact]
    public void ConfigLoader_UsesFactory_ForDpapi_AndFallsBackToFile()
    {
        File.WriteAllText(Path.Combine(_dir, "defaults.yaml"), "version: 1\nsecrets:\n  backend: dpapi\n");

        // With a factory that supplies dpapi, the resolved store is the DPAPI one.
        using (MachineLayerScope.Begin(_dir, (backend, ws, machineDir) =>
                   backend == "dpapi" ? new DpapiFileSecretStore(ws, machineDir) : null))
        {
            Assert.IsType<DpapiFileSecretStore>(ConfigLoader.LoadAndResolve().Secrets);
        }

        // With no factory, the same config must fall back to the plaintext file store, not crash.
        using (MachineLayerScope.Begin(_dir, suppressSecretStoreFactory: true))
        {
            Assert.IsType<FileSecretStore>(ConfigLoader.LoadAndResolve().Secrets);
        }
    }

    /// <summary>
    /// The override rule itself, proved without moving a process-wide setting to prove it — which is
    /// the whole point: a test that sets <c>SPLA_HOME</c> is a test every parallel test can see.
    /// </summary>
    [Fact]
    public void An_override_home_replaces_the_machine_layer_and_a_blank_one_does_not()
    {
        Assert.Equal(Path.GetFullPath(_dir), ConfigLoader.ResolveDefaultsDir(_dir));
        Assert.Equal(Path.GetFullPath(_dir), ConfigLoader.ResolveDefaultsDir("  " + _dir + "  "));

        var machineOwn = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".spla");

        Assert.Equal(machineOwn, ConfigLoader.ResolveDefaultsDir(null));
        Assert.Equal(machineOwn, ConfigLoader.ResolveDefaultsDir("   "));
    }

    /// <summary>
    /// A scope answers for its own flow only. Without this the redirection above would be the same
    /// process-wide leak wearing a different name.
    ///
    /// <para>The neighbour is started with the execution context suppressed, because that is what a
    /// concurrently running test actually is: a flow this one did not start. An ordinary
    /// <c>Task.Run</c> would inherit the scope and prove nothing — ambient state flows DOWN into work
    /// this flow begins, which is the behaviour we want, and never sideways, which is the behaviour
    /// the bug needed.</para>
    /// </summary>
    [Fact]
    public void A_scope_is_invisible_to_work_running_beside_it()
    {
        var outside = ConfigLoader.GetDefaultsDir();

        using (MachineLayerScope.Begin(_dir))
        {
            Assert.Equal(Path.GetFullPath(_dir), ConfigLoader.GetDefaultsDir());

            var seenByNeighbour = "";
            using (ExecutionContext.SuppressFlow())
            {
                var neighbour = new Thread(() => seenByNeighbour = ConfigLoader.GetDefaultsDir());
                neighbour.Start();
                neighbour.Join();
            }

            Assert.Equal(outside, seenByNeighbour);
        }

        Assert.Equal(outside, ConfigLoader.GetDefaultsDir());
    }
}
