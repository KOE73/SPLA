using System.IO;
using SPLA.Domain.Project;

namespace SPLA.Tests;

/// <summary>
/// Phase 2 (docs/host-abstraction-plan.md): the provider level above a single project —
/// create/open/list/recent against an isolated state dir, never touching the real ~/.spla.
/// </summary>
public sealed class ProjectProviderTests
{
    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-provider-{Guid.NewGuid():N}")).FullName;

    [Fact]
    public void Create_open_list_recent_round_trip()
    {
        var root = TempRoot();
        try
        {
            var provider = new LocalProjectProvider(stateDir: Path.Combine(root, "state"));

            var manifestA = Path.Combine(root, "a", "alpha.spla");
            var manifestB = Path.Combine(root, "b", "beta.spla");

            var alpha = provider.Create(new ProjectDescriptor { Id = manifestA, ManifestPath = manifestA, Name = "Alpha" });
            Assert.Equal("Alpha", alpha.Name);
            Assert.True(File.Exists(manifestA));

            provider.Create(new ProjectDescriptor { Id = manifestB, ManifestPath = manifestB, Name = "Beta" });

            Assert.Equal(2, provider.List().Count);

            // Re-opening Alpha makes it the most recent.
            provider.Open(manifestA);
            Assert.Equal("Alpha", provider.Recent()[0].Name);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Deleted_manifest_disappears_from_listings()
    {
        var root = TempRoot();
        try
        {
            var provider = new LocalProjectProvider(stateDir: Path.Combine(root, "state"));
            var manifest = Path.Combine(root, "gone.spla");
            provider.Create(new ProjectDescriptor { Id = manifest, ManifestPath = manifest, Name = "Gone" });
            Assert.Single(provider.List());

            File.Delete(manifest);
            Assert.Empty(provider.List());
            Assert.Empty(provider.Recent());
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Legacy_recent_projects_txt_seeds_the_registry_in_order()
    {
        var root = TempRoot();
        try
        {
            var stateDir = Directory.CreateDirectory(Path.Combine(root, "state")).FullName;
            var m1 = Path.Combine(root, "one.spla");
            var m2 = Path.Combine(root, "two.spla");
            File.WriteAllText(m1, "name: one");
            File.WriteAllText(m2, "name: two");
            // Legacy format: most recent first.
            File.WriteAllLines(Path.Combine(stateDir, "recent_projects.txt"), [m1, m2]);

            var recent = new LocalProjectProvider(stateDir).Recent();
            Assert.Equal(2, recent.Count);
            Assert.Equal("one", recent[0].Name);
            Assert.Equal("two", recent[1].Name);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Opened_project_brokers_stores_under_its_own_runtime_area()
    {
        var root = TempRoot();
        try
        {
            var provider = new LocalProjectProvider(stateDir: Path.Combine(root, "state"));
            var manifest = Path.Combine(root, "proj", "demo.spla");
            var project = provider.Create(new ProjectDescriptor { Id = manifest, ManifestPath = manifest, Name = "Demo" });

            project.GetBucket("chats").WriteText("probe.yaml", "id: x");
            Assert.True(File.Exists(Path.Combine(root, "proj", ".spla", "chats", "probe.yaml")));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
