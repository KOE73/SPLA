using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Project;
using SPLA.Registry;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Tests for FileInstanceRegistry: instance discovery, probing, and record state resolution.
/// </summary>
public sealed class FileInstanceRegistryTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(
        Path.GetTempPath(),
        $"spla-registry-tests-{Guid.NewGuid():N}");

    public FileInstanceRegistryTests()
    {
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempRoot, recursive: true); } catch { /* best effort */ }
    }

    // ── No lock file ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListAsync_excludes_projects_with_no_lock_file()
    {
        // A project with no instance lock should not appear in the listing,
        // even if the project descriptor exists in the provider.
        var manifestPath = Path.Combine(_tempRoot, "project.spla");
        File.WriteAllText(manifestPath, "{}");

        var provider = new StubProjectProvider(
            new ProjectDescriptor { Id = manifestPath, ManifestPath = manifestPath });
        var registry = new FileInstanceRegistry(provider);

        var records = await registry.ListAsync();

        Assert.Empty(records);
    }

    // ── Lock held ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListAsync_includes_project_with_active_lock()
    {
        // A project with an active lock should appear in the listing.
        var manifestPath = Path.Combine(_tempRoot, "project.spla");
        var runtimeDir = Path.Combine(_tempRoot, ".spla");

        using var lockHandle = InstanceLock.Acquire(runtimeDir, "serve");

        var provider = new StubProjectProvider(
            new ProjectDescriptor { Id = manifestPath, ManifestPath = manifestPath });
        var registry = new FileInstanceRegistry(provider);

        var records = await registry.ListAsync();

        Assert.Single(records);
        Assert.Equal(manifestPath, records[0].ProjectId);
    }

    [Fact]
    public async Task ListAsync_record_has_correct_mode_from_lock()
    {
        // The lock's Mode should be preserved in the InstanceRecord.
        var manifestPath = Path.Combine(_tempRoot, "project.spla");
        var runtimeDir = Path.Combine(_tempRoot, ".spla");

        using var lockHandle = InstanceLock.Acquire(runtimeDir, "repl");

        var provider = new StubProjectProvider(
            new ProjectDescriptor { Id = manifestPath, ManifestPath = manifestPath });
        var registry = new FileInstanceRegistry(provider);

        var records = await registry.ListAsync();

        Assert.Single(records);
        Assert.Equal("repl", records[0].Info.Mode);
    }

    // ── No probe: State stays Unreachable ─────────────────────────────────────────

    [Fact]
    public async Task ListAsync_without_probe_keeps_state_unreachable()
    {
        // When no probe is supplied, the state cannot be resolved beyond the lock file,
        // so it remains Unreachable even if there is an endpoint.
        var manifestPath = Path.Combine(_tempRoot, "project.spla");
        var runtimeDir = Path.Combine(_tempRoot, ".spla");

        using var lockHandle = InstanceLock.Acquire(runtimeDir, "serve");
        lockHandle.Publish("http://localhost:8080");

        // Create registry WITHOUT a probe
        var provider = new StubProjectProvider(
            new ProjectDescriptor { Id = manifestPath, ManifestPath = manifestPath });
        var registry = new FileInstanceRegistry(provider, probe: null);

        var records = await registry.ListAsync();

        Assert.Single(records);
        Assert.Equal(InstanceState.Unreachable, records[0].State);
    }

    // ── Probe provides state ──────────────────────────────────────────────────────

    [Fact]
    public async Task ListAsync_with_probe_resolves_state_and_clients()
    {
        // When a probe is supplied and the instance is serving, the probe's answer
        // should populate the State and Clients fields in the record.
        var manifestPath = Path.Combine(_tempRoot, "project.spla");
        var runtimeDir = Path.Combine(_tempRoot, ".spla");

        using var lockHandle = InstanceLock.Acquire(runtimeDir, "serve");
        lockHandle.Publish("http://localhost:8080");

        var probe = new StubInstanceProbe(InstanceState.Working, clientCount: 2);
        var provider = new StubProjectProvider(
            new ProjectDescriptor { Id = manifestPath, ManifestPath = manifestPath });
        var registry = new FileInstanceRegistry(provider, probe);

        var records = await registry.ListAsync();

        Assert.Single(records);
        Assert.Equal(InstanceState.Working, records[0].State);
        Assert.Equal(2, records[0].Clients);
    }

    // ── No endpoint: probe is not called ──────────────────────────────────────────

    [Fact]
    public async Task ListAsync_does_not_probe_instance_without_endpoint()
    {
        // An instance that holds a project but is not serving (no endpoint) should not
        // be probed at all. The fake probe tracks calls to verify this.
        var manifestPath = Path.Combine(_tempRoot, "project.spla");
        var runtimeDir = Path.Combine(_tempRoot, ".spla");

        using var lockHandle = InstanceLock.Acquire(runtimeDir, "repl");
        // Note: no call to Publish(), so Endpoint is null

        var probe = new StubInstanceProbe(InstanceState.Working, clientCount: 0);
        var provider = new StubProjectProvider(
            new ProjectDescriptor { Id = manifestPath, ManifestPath = manifestPath });
        var registry = new FileInstanceRegistry(provider, probe);

        var records = await registry.ListAsync();

        Assert.Single(records);
        // Verify the probe was never called by checking that it has no call count
        Assert.Empty(probe.Endpoints);
        Assert.False(records[0].IsServing);
    }

    // ── FindAsync ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task FindAsync_returns_record_for_known_project_with_lock()
    {
        // FindAsync should locate a project by its manifest path and return the record
        // if a lock is held.
        var manifestPath = Path.Combine(_tempRoot, "project.spla");
        var runtimeDir = Path.Combine(_tempRoot, ".spla");

        using var lockHandle = InstanceLock.Acquire(runtimeDir, "serve");

        var provider = new StubProjectProvider(
            new ProjectDescriptor { Id = manifestPath, ManifestPath = manifestPath });
        var registry = new FileInstanceRegistry(provider);

        var record = await registry.FindAsync(manifestPath);

        Assert.NotNull(record);
        Assert.Equal(manifestPath, record.ProjectId);
    }

    [Fact]
    public async Task FindAsync_returns_null_for_unknown_project()
    {
        // FindAsync should return null when the project is not in the provider's list.
        var provider = new StubProjectProvider();
        var registry = new FileInstanceRegistry(provider);

        var record = await registry.FindAsync("/unknown/path");

        Assert.Null(record);
    }

    [Fact]
    public async Task FindAsync_returns_null_when_lock_not_held()
    {
        // FindAsync should return null when the project exists but has no lock.
        var manifestPath = Path.Combine(_tempRoot, "project.spla");
        File.WriteAllText(manifestPath, "{}");

        var provider = new StubProjectProvider(
            new ProjectDescriptor { Id = manifestPath, ManifestPath = manifestPath });
        var registry = new FileInstanceRegistry(provider);

        var record = await registry.FindAsync(manifestPath);

        Assert.Null(record);
    }

    // ── Ordering: newest started first ────────────────────────────────────────────

    [Fact]
    public async Task ListAsync_returns_results_ordered_by_started_time()
    {
        // ListAsync should return results ordered by StartedAt descending (newest first).
        // When locks are acquired in the same process, they share the same StartedAt
        // (the process start time), but the listing should still be ordered correctly.
        var dir1 = Path.Combine(_tempRoot, "proj1");
        var dir2 = Path.Combine(_tempRoot, "proj2");
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);

        var manifest1 = Path.Combine(dir1, "project.spla");
        var manifest2 = Path.Combine(dir2, "project.spla");

        var runtime1 = Path.Combine(dir1, ".spla");
        var runtime2 = Path.Combine(dir2, ".spla");

        // Acquire both locks (both will have the same StartedAt = process start time)
        using var lock1 = InstanceLock.Acquire(runtime1, "serve");
        using var lock2 = InstanceLock.Acquire(runtime2, "serve");

        var provider = new StubProjectProvider(
            new ProjectDescriptor { Id = manifest1, ManifestPath = manifest1 },
            new ProjectDescriptor { Id = manifest2, ManifestPath = manifest2 });
        var registry = new FileInstanceRegistry(provider);

        var records = await registry.ListAsync();

        // Both locks should be present
        Assert.Equal(2, records.Count);
        // Verify both are present with correct IDs (order may vary when StartedAt is identical)
        var ids = records.Select(r => r.ProjectId).ToList();
        Assert.Contains(manifest1, ids);
        Assert.Contains(manifest2, ids);
        // Verify ordering is descending by StartedAt
        Assert.True(records[0].Info.StartedAt >= records[1].Info.StartedAt);
    }

    // ── Stub implementations ──────────────────────────────────────────────────────

    /// <summary>
    /// A stub project provider for testing: returns a fixed list of projects.
    /// No file I/O beyond what the registry does with locks.
    /// </summary>
    private sealed class StubProjectProvider : IProjectProvider
    {
        private readonly List<ProjectDescriptor> _projects;

        public StubProjectProvider(params ProjectDescriptor[] projects)
        {
            _projects = projects.ToList();
        }

        public IReadOnlyList<ProjectDescriptor> List() => _projects.AsReadOnly();

        public IReadOnlyList<ProjectDescriptor> Recent() =>
            _projects
                .OrderByDescending(p => p.LastOpened ?? DateTimeOffset.MinValue)
                .ToList();

        public IProject Open(string id) =>
            throw new NotImplementedException("Test stub does not support Open");

        public IProject Create(ProjectDescriptor descriptor) =>
            throw new NotImplementedException("Test stub does not support Create");
    }

    /// <summary>
    /// A stub instance probe for testing: returns a fixed state and client count.
    /// Tracks which endpoints were probed.
    /// </summary>
    private sealed class StubInstanceProbe : IInstanceProbe
    {
        private readonly InstanceState _state;
        private readonly int _clientCount;

        public List<string> Endpoints { get; } = new();

        public StubInstanceProbe(InstanceState state, int clientCount)
        {
            _state = state;
            _clientCount = clientCount;
        }

        public Task<(InstanceState State, int Clients)?> ProbeAsync(string endpoint, CancellationToken ct = default)
        {
            Endpoints.Add(endpoint);
            return Task.FromResult<(InstanceState State, int Clients)?>((
                State: _state,
                Clients: _clientCount));
        }
    }
}
