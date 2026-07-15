using SPLA.Runtime;
using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Project;
using SPLA.Service;

namespace SPLA.Tests;

/// <summary>
/// Phase 2.1 (docs/host-abstraction-plan.md): the multi-project seam. A single service process
/// can hold more than one <see cref="AgentRuntime"/> side by side, each keyed by project id and
/// built lazily; reopening the same project returns the SAME runtime so its state (chats, KV,
/// connection-health cache) stays one consistent instance regardless of caller.
/// </summary>
public sealed class AgentRuntimeRegistryTests
{
    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-runtime-registry-{Guid.NewGuid():N}")).FullName;

    [Fact]
    public void Reopening_the_same_project_returns_the_same_runtime_instance()
    {
        var root = TempRoot();
        try
        {
            using var registry = new AgentRuntimeRegistry(
                NullLoggerFactory.Instance, new LocalProjectProvider(Path.Combine(root, "state")));

            var manifest = Path.Combine(root, "proj", "demo.spla");
            var created = registry.Create(new ProjectDescriptor { Id = manifest, ManifestPath = manifest, Name = "Demo" });
            var reopened = registry.Open(manifest);

            Assert.Same(created.Runtime, reopened.Runtime);
            Assert.Same(created.Chats, reopened.Chats);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void Different_projects_get_isolated_runtimes()
    {
        var root = TempRoot();
        try
        {
            using var registry = new AgentRuntimeRegistry(
                NullLoggerFactory.Instance, new LocalProjectProvider(Path.Combine(root, "state")));

            var manifestA = Path.Combine(root, "a", "alpha.spla");
            var manifestB = Path.Combine(root, "b", "beta.spla");
            var a = registry.Create(new ProjectDescriptor { Id = manifestA, ManifestPath = manifestA, Name = "Alpha" });
            var b = registry.Create(new ProjectDescriptor { Id = manifestB, ManifestPath = manifestB, Name = "Beta" });

            Assert.NotSame(a.Runtime, b.Runtime);
            Assert.Equal(root + "\\a", Path.GetDirectoryName(a.Runtime.Settings.ProjectFilePath));
            Assert.Equal("Alpha", a.Runtime.Settings.ProjectName);
            Assert.Equal("Beta", b.Runtime.Settings.ProjectName);

            // Registry listing reflects both, independent of open order.
            Assert.Equal(2, registry.List().Count);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void No_project_sentinel_is_stable_across_null_and_empty_and_explicit_key()
    {
        using var registry = new AgentRuntimeRegistry(NullLoggerFactory.Instance);

        var viaNull = registry.Open(null);
        var viaEmpty = registry.Open("");
        var viaSentinel = registry.Open(AgentRuntimeRegistry.NoProjectId);

        Assert.Same(viaNull.Runtime, viaEmpty.Runtime);
        Assert.Same(viaNull.Runtime, viaSentinel.Runtime);
    }
}
