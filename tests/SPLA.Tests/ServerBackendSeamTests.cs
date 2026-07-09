using SPLA.Domain.Project;
using SPLA.Domain.Settings;

namespace SPLA.Tests;

/// <summary>
/// Server-readiness seam: the core persistence stores must work over a backend that has NO host
/// directory (<see cref="IBucket.MapToHostDirectory"/> == null) — the shape a DB / object-storage /
/// per-user cloud backend presents. <see cref="ProjectKvStore"/> now rides the bucket's key/value
/// API instead of a disk path, so it round-trips over <see cref="MemoryProjectBackend"/> with no
/// file system involved. This is the concrete proof that the storage abstraction is real and a
/// server backend can slot in behind the same interfaces.
/// </summary>
public sealed class ServerBackendSeamTests
{
    [Fact]
    public void MemoryBucket_is_a_non_disk_bucket()
    {
        var bucket = new MemoryBucket("root");
        Assert.Null(bucket.MapToHostDirectory());   // the defining trait

        Assert.Null(bucket.ReadText("k"));
        bucket.WriteText("k", "v");
        Assert.True(bucket.Exists("k"));
        Assert.Equal("v", bucket.ReadText("k"));
        Assert.Equal(["k"], bucket.ListKeys());
        bucket.Delete("k");
        Assert.False(bucket.Exists("k"));
    }

    [Fact]
    public void ProjectKv_round_trips_over_a_non_disk_backend()
    {
        var backend = new MemoryProjectBackend("proj-1");
        var root = backend.GetBucket(IProjectBackend.RootBucket);

        // First store writes through the bucket (no disk).
        new ProjectKvStore(root).Store.Set("context:goal", "server-seed");

        // A fresh store over the SAME backend loads what the first persisted — proving the KV
        // survived purely through the bucket's key/value API, not a host directory.
        var reopened = new ProjectKvStore(root);
        Assert.Equal("server-seed", reopened.Store.Get("context:goal"));

        // And it genuinely never asked for a host directory.
        Assert.Null(root.MapToHostDirectory());
    }

    [Fact]
    public void Different_projects_get_isolated_in_memory_buckets()
    {
        var a = new MemoryProjectBackend("a");
        var b = new MemoryProjectBackend("b");

        new ProjectKvStore(a.GetBucket(IProjectBackend.RootBucket)).Store.Set("k", "from-a");
        new ProjectKvStore(b.GetBucket(IProjectBackend.RootBucket)).Store.Set("k", "from-b");

        Assert.Equal("from-a", new ProjectKvStore(a.GetBucket(IProjectBackend.RootBucket)).Store.Get("k"));
        Assert.Equal("from-b", new ProjectKvStore(b.GetBucket(IProjectBackend.RootBucket)).Store.Get("k"));
    }
}
