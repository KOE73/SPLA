using System.IO;
using SPLA.Domain.Project;
using SPLA.Domain.Settings;

namespace SPLA.Tests;

/// <summary>
/// Phase 1 (docs/host-abstraction-plan.md): the project is a storage BROKER. Consumers get
/// buckets/KV from <see cref="IProject"/> and never hand-roll ".spla" paths; the physical layout
/// decision lives in exactly one place — <see cref="LocalProjectBackend.For"/>.
/// </summary>
public sealed class ProjectBrokerTests
{
    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-broker-{Guid.NewGuid():N}")).FullName;

    [Fact]
    public void FileBucket_round_trips_and_creates_directory_lazily()
    {
        var root = TempRoot();
        try
        {
            var bucket = new FileBucket("sql", Path.Combine(root, "sql"));

            // Opening a bucket must not litter the disk.
            Assert.False(Directory.Exists(Path.Combine(root, "sql")));
            Assert.Null(bucket.ReadText("connections.yaml"));
            Assert.Empty(bucket.ListKeys());

            bucket.WriteText("connections.yaml", "a: 1");
            Assert.True(bucket.Exists("connections.yaml"));
            Assert.Equal("a: 1", bucket.ReadText("connections.yaml"));
            Assert.Equal(["connections.yaml"], bucket.ListKeys());

            bucket.Delete("connections.yaml");
            Assert.False(bucket.Exists("connections.yaml"));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Backend_decides_runtime_layout_project_vs_global()
    {
        var root = TempRoot();
        try
        {
            var withProject = LocalProjectBackend.For(new ResolvedSettings
            {
                ProjectFilePath = Path.Combine(root, "demo.spla"),
                WorkspacePath = root
            });
            Assert.Equal(Path.Combine(root, ".spla"),
                withProject.GetBucket(IProjectBackend.RootBucket).MapToHostDirectory());
            Assert.Equal(Path.Combine(root, ".spla", "chats"),
                withProject.GetBucket("chats").MapToHostDirectory());

            // No project → the global ~/.spla plays the runtime area (historical fallback).
            var withoutProject = LocalProjectBackend.For(new ResolvedSettings());
            Assert.Equal(ConfigLoader.GetDefaultsDir(),
                withoutProject.GetBucket(IProjectBackend.RootBucket).MapToHostDirectory());
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Project_brokers_kv_persisted_in_runtime_area()
    {
        var root = TempRoot();
        try
        {
            var settings = new ResolvedSettings
            {
                ProjectFilePath = Path.Combine(root, "demo.spla"),
                WorkspacePath = root
            };
            var project = LocalProject.For(settings);

            project.GetKV().Set("context:goal", "phase1");

            // The KV landed in the project's runtime area, single-sourced by the backend.
            var kvFile = Path.Combine(root, ".spla", "project-kv.yaml");
            Assert.True(File.Exists(kvFile));
            Assert.Contains("phase1", File.ReadAllText(kvFile));

            Assert.Equal("phase1", project.GetKV().Get("context:goal"));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void ChatManager_stores_history_in_broker_buckets()
    {
        var root = TempRoot();
        try
        {
            var settings = new ResolvedSettings
            {
                ProjectFilePath = Path.Combine(root, "demo.spla"),
                WorkspacePath = root
            };

            var manager = new ChatManager(settings);
            var chat = manager.CreateNewChat("broker test");

            Assert.True(File.Exists(Path.Combine(root, ".spla", "chats", $"{chat.Id}.yaml")));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
