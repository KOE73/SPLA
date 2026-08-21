using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPLA.Agent.Composition;
using SPLA.Domain.Agent;
using SPLA.Domain.Formats;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Resources;
using SPLA.Domain.Resources.Providers;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Agent;
using SPLA.MCP.Core.Composition;
using SPLA.MCP.Core.Formats;
using SPLA.MCP.Core.Tools.Resources;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The verbs as the model actually meets them. Two claims are load-bearing here and neither is
/// visible from the provider tests: that <c>as</c> routes through the conversion registry and the
/// REQUESTED type — not the detected one — decides where the result lands, and that with the master
/// switch off none of this reaches the agent at all.
/// </summary>
public sealed class ResourceToolsTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "spla-restools-" + Guid.NewGuid().ToString("N"));

    public ResourceToolsTests()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "notes.txt"), "plain words");
        File.WriteAllText(Path.Combine(_root, "config.json"), """{"name":"spla","port":8080}""");
        // A PNG signature is enough: nothing decodes it, and the whole point is that a byte run
        // nobody can read as text must not be inlined.
        File.WriteAllBytes(Path.Combine(_root, "shot.png"),
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, .. Enumerable.Repeat((byte)0xC0, 64)]);
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { /* best effort */ }
    }

    private ResourceRegistry Files()
    {
        var registry = new ResourceRegistry();
        var workspace = new LocalWorkspace(new PathBoundary(_root, null, []));
        registry.Register(new FileResourceProvider(() => workspace));
        return registry;
    }

    private static FormatConverterRegistry Converters()
    {
        var registry = new FormatConverterRegistry();
        BuiltInConverters.RegisterInto(registry);
        return registry;
    }

    private ResourceReadTool Read() => new(Files(), Converters());

    private static IDisposable Session(BlobStore blobs)
        => AgentSessionScope.Begin(new AgentSession(new KeyValueStore("session"), new MarkManager(), new SkillSession(), blobs));

    // ── read: the default basket ─────────────────────────────────────────────

    [Fact]
    public async Task Text_comes_back_as_text_with_no_target_asked_for()
    {
        var result = await Read().ExecuteAsync("""{"uri":"file:///notes.txt"}""");

        Assert.Equal(ToolOutcome.Ok, result.Outcome);
        Assert.Equal("plain words", result.TextContent);
    }

    /// <summary>
    /// The safe default: an unlabelled binary read with no target must not be able to fill the
    /// context window, so it goes to the blob store and the model gets a handle instead of bytes.
    /// </summary>
    [Fact]
    public async Task Binary_without_a_target_is_stored_and_answered_with_a_handle()
    {
        var blobs = new BlobStore();
        using var _ = Session(blobs);

        var result = await Read().ExecuteAsync("""{"uri":"file:///shot.png"}""");

        Assert.Equal(ToolOutcome.Ok, result.Outcome);
        Assert.Contains(BlobStore.HandlePrefix, result.TextContent);
        Assert.Contains(result.Content, c => c is ToolResource);

        // Nothing bulky travelled: the text is a note, not a payload.
        Assert.True(result.TextContent.Length < 400, result.TextContent);
        Assert.DoesNotContain("ÀÀÀ", result.TextContent);
    }

    // ── read: as ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Asking_for_text_from_json_decodes_it_through_the_registry()
    {
        var result = await Read().ExecuteAsync("""{"uri":"file:///config.json","as":"text/plain"}""");

        Assert.Equal(ToolOutcome.Ok, result.Outcome);
        Assert.Contains("\"port\":8080", result.TextContent);
    }

    /// <summary>The day-one pair finally carrying real traffic: a genuine reshape, reached from the
    /// tool the model calls rather than from a unit test of the converter.</summary>
    [Fact]
    public async Task Asking_for_yaml_from_json_produces_yaml()
    {
        var result = await Read().ExecuteAsync("""{"uri":"file:///config.json","as":"application/yaml"}""");

        Assert.Equal(ToolOutcome.Ok, result.Outcome);
        Assert.Contains("name: spla", result.TextContent);
        Assert.Contains("port: 8080", result.TextContent);
        Assert.DoesNotContain("{", result.TextContent);
    }

    /// <summary>An unreachable target is where the model learns the menu — the refusal has to list
    /// what this source CAN become, or the next call is another guess.</summary>
    [Fact]
    public async Task An_unreachable_target_is_refused_with_the_reachable_ones_named()
    {
        var result = await Read().ExecuteAsync("""{"uri":"file:///config.json","as":"audio/mpeg"}""");

        Assert.Equal(ToolOutcome.Failed, result.Outcome);
        Assert.Contains("audio/mpeg", result.TextContent);
        Assert.Contains("application/yaml", result.TextContent);
    }

    // ── the verb matrix ──────────────────────────────────────────────────────

    /// <summary>A scheme that cannot write says which verbs it does serve. Discovering the matrix by
    /// triggering a stack trace is the affordance trap the address space exists to close.</summary>
    [Fact]
    public async Task A_scheme_that_cannot_write_refuses_naming_the_verbs_it_does_support()
    {
        var registry = new ResourceRegistry();
        registry.Register(new ReadOnlyScheme());

        var result = await new ResourceWriteTool(registry)
            .ExecuteAsync("""{"uri":"ro:///whatever","content":"x"}""");

        Assert.Equal(ToolOutcome.Refused, result.Outcome);
        Assert.Contains("does not support 'write'", result.TextContent);
        Assert.Contains("read, exists, list", result.TextContent);
    }

    private sealed class ReadOnlyScheme : IResourceProvider
    {
        public string Scheme => "ro";
        public string Summary => "a read-only store";
        public Task<ResourceContent> ReadAsync(ResourceUri uri, System.Threading.CancellationToken ct = default)
            => Task.FromResult(new ResourceContent([], "text/plain"));
        public Task<bool> ExistsAsync(ResourceUri uri, System.Threading.CancellationToken ct = default)
            => Task.FromResult(true);
        public Task<IReadOnlyList<ResourceEntry>> ListAsync(ResourceUri uri, System.Threading.CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<ResourceEntry>>([]);
    }

    // ── the switch ───────────────────────────────────────────────────────────

    private static string TempManifestRoot() =>
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "spla-restools-rt-" + Guid.NewGuid().ToString("N"))).FullName;

    private static SPLA.Runtime.AgentRuntime BuildRuntime(string root, bool unifiedResources)
    {
        var manifest = Path.Combine(root, "test.spla");
        File.WriteAllText(manifest, $"""
            version: 1
            name: ResourceToolsTest
            workspace: .
            agent:
              mode: Edit
              unified_resources: {(unifiedResources ? "true" : "false")}
            """);

        return new SPLA.Runtime.AgentRuntime(
            SPLA.Domain.Settings.ConfigLoader.LoadAndResolve(manifest),
            Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
    }

    private static string[] ResourceToolNames(SPLA.Runtime.AgentRuntime runtime)
        => runtime.McpHost.GetToolDefinitions()
            .Select(d => d.Function.Name)
            .Where(n => n.StartsWith("resource_", StringComparison.Ordinal))
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

    /// <summary>Off means NOT REGISTERED — not registered-and-refusing. Anything else would leave the
    /// switched-off arm of the experiment differing from the old agent by a tool list.</summary>
    [Fact]
    public void The_verbs_are_absent_from_the_tool_list_while_the_switch_is_off()
    {
        var root = TempManifestRoot();
        try
        {
            using var runtime = BuildRuntime(root, unifiedResources: false);
            Assert.Empty(ResourceToolNames(runtime));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void The_verbs_appear_once_the_switch_is_on()
    {
        var root = TempManifestRoot();
        try
        {
            using var runtime = BuildRuntime(root, unifiedResources: true);

            Assert.Equal(
                new[] { "resource_delete", "resource_exists", "resource_list", "resource_mkdir", "resource_read", "resource_write" },
                ResourceToolNames(runtime));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}

/// <summary>
/// The inertness guarantee, as a measurement rather than a claim.
///
/// <para>The whole basis for shipping the address space switched off is that the two arms of the
/// experiment differ only by the thing being measured. A prompt that leaked so much as a newline
/// while the switch was off would make the comparison worthless — so the assertion is equality of the
/// composed prompt, byte for byte, between a project that registered every scheme and every
/// conversion and one that registered nothing at all.</para>
/// </summary>
public sealed class ResourcePromptInertnessTests
{
    [Fact]
    public void With_the_switch_off_the_prompt_is_byte_for_byte_the_prompt_of_a_project_that_has_no_resources()
    {
        var root = Directory.CreateTempSubdirectory("spla-res-inert-").FullName;
        try
        {
            var bare = Compose(root, Populated(root, unifiedResources: false, populate: false));
            var loaded = Compose(root, Populated(root, unifiedResources: false, populate: true));

            Assert.Equal(bare, loaded);

            // And the switch is what makes the difference — otherwise the equality above would be
            // proving that the contributor is dead rather than that it is quiet.
            var on = Compose(root, Populated(root, unifiedResources: true, populate: true));
            Assert.NotEqual(bare, on);
            Assert.Contains("application/json -> application/yaml", on);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    private static ResolvedSettings Populated(string root, bool unifiedResources, bool populate)
    {
        var settings = new ResolvedSettings
        {
            Mode = AgentMode.Edit,
            WorkspacePath = root,
            UnifiedResources = unifiedResources,
            Instructions = [],
            Skills = new Dictionary<string, SplaSkillSection>()
        };

        if (populate)
        {
            ResourceRegistry.For(settings).Register(new FileResourceProvider(() => new LocalWorkspace()));
            BuiltInConverters.RegisterInto(FormatConverterRegistry.For(settings));
        }

        return settings;
    }

    private static string Compose(string root, ResolvedSettings settings)
        => new AgentContextComposer(AgentContributors.Default(
                new SPLA.Library.SkillLibrary([]),
                new SPLA.MCP.Core.Plugins.PluginManager(settings),
                new SkillSession(),
                [new AgentFeature("core.files")]))
            .Compose(settings, root).SystemPrompt;
}
