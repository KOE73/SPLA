using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Formats;
using SPLA.Domain.Resources;
using SPLA.Domain.Settings;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The registry is a lookup table, so every test here is about what it answers and — the part that
/// matters more — what it says when it cannot answer. A failure that names the reachable targets is
/// how the model learns the menu without guessing twice.
/// </summary>
public sealed class FormatConverterRegistryTests
{
    private sealed class FakeConverter : IFormatConverter
    {
        public FakeConverter(string source, string target, string summary = "test converter")
        {
            SourceType = source;
            TargetType = target;
            Summary = summary;
        }

        public string SourceType { get; }
        public string TargetType { get; }
        public string Summary { get; }

        public Task<ResourceContent> ConvertAsync(
            ResourceContent source,
            IReadOnlyDictionary<string, object?>? options,
            CancellationToken ct = default)
            => Task.FromResult(new ResourceContent(source.Bytes, TargetType));
    }

    /// <summary>A different type on purpose: same pair, different implementation is the clash.</summary>
    private sealed class OtherConverter : IFormatConverter
    {
        public string SourceType => "application/pdf";
        public string TargetType => "image/png";
        public string Summary => "another one";

        public Task<ResourceContent> ConvertAsync(
            ResourceContent source,
            IReadOnlyDictionary<string, object?>? options,
            CancellationToken ct = default)
            => Task.FromResult(new ResourceContent(source.Bytes, TargetType));
    }

    private static FormatConverterRegistry NewRegistry() => FormatConverterRegistry.For(new ResolvedSettings());

    [Fact]
    public void Resolves_a_registered_pair()
    {
        var registry = NewRegistry();
        var converter = new FakeConverter("application/pdf", "image/png");
        registry.Register(converter);

        Assert.True(registry.TryResolve("application/pdf", "image/png", out var found, out var error));
        Assert.Same(converter, found);
        Assert.Null(error);
    }

    [Fact]
    public void One_registry_per_settings_instance()
    {
        var settings = new ResolvedSettings();
        Assert.Same(FormatConverterRegistry.For(settings), FormatConverterRegistry.For(settings));
    }

    [Fact]
    public void Unknown_pair_names_the_targets_that_are_reachable()
    {
        var registry = NewRegistry();
        registry.Register(new FakeConverter("application/pdf", "image/png"));
        registry.Register(new FakeConverter("application/pdf", "text/plain"));

        Assert.False(registry.TryResolve("application/pdf", "text/markdown", out _, out var error));
        Assert.Contains("image/png", error);
        Assert.Contains("text/plain", error);
    }

    [Fact]
    public void Unknown_source_says_plainly_that_nothing_is_reachable()
    {
        var registry = NewRegistry();
        registry.Register(new FakeConverter("application/pdf", "image/png"));

        Assert.False(registry.TryResolve("application/x-nothing", "image/png", out _, out var error));
        Assert.Contains("Nothing can be produced", error);
    }

    [Fact]
    public void Re_registering_the_same_type_is_benign()
    {
        var registry = NewRegistry();
        registry.Register(new FakeConverter("application/pdf", "image/png"));

        // A plugin reload, or a second runtime over one ResolvedSettings — not a wiring mistake.
        registry.Register(new FakeConverter("application/pdf", "image/png"));

        Assert.Single(registry.Cards());
    }

    [Fact]
    public void Two_different_types_on_one_pair_throw()
    {
        var registry = NewRegistry();
        registry.Register(new FakeConverter("application/pdf", "image/png"));

        var ex = Assert.Throws<InvalidOperationException>(() => registry.Register(new OtherConverter()));
        Assert.Contains(nameof(FakeConverter), ex.Message);
        Assert.Contains(nameof(OtherConverter), ex.Message);
    }

    [Fact]
    public void Mime_parameters_are_ignored_when_matching()
    {
        var registry = NewRegistry();
        registry.Register(new FakeConverter("text/plain", "text/markdown"));

        Assert.True(registry.TryResolve("text/plain; charset=utf-8", "text/markdown", out _, out _));
    }

    [Fact]
    public void A_wildcard_registration_matches_the_family()
    {
        var registry = NewRegistry();
        var wildcard = new FakeConverter("image/*", "image/png");
        registry.Register(wildcard);

        Assert.True(registry.TryResolve("image/jpeg", "image/png", out var found, out _));
        Assert.Same(wildcard, found);
    }

    [Fact]
    public void An_exact_registration_wins_over_a_wildcard_one()
    {
        var registry = NewRegistry();
        registry.Register(new FakeConverter("image/*", "image/png", "wildcard"));
        registry.Register(new FakeConverter("image/jpeg", "image/png", "exact"));

        Assert.True(registry.TryResolve("image/jpeg", "image/png", out var found, out _));
        Assert.Equal("exact", found.Summary);
    }

    [Fact]
    public void Identity_is_a_registration_not_a_special_case()
    {
        var registry = NewRegistry();
        Assert.False(registry.TryResolve("image/png", "image/png", out _, out _));

        registry.Register(new FakeConverter("image/png", "image/png", "identity"));
        Assert.True(registry.TryResolve("image/png", "image/png", out _, out _));
    }

    [Fact]
    public void Targets_for_lists_what_a_source_can_become()
    {
        var registry = NewRegistry();
        registry.Register(new FakeConverter("application/pdf", "text/plain"));
        registry.Register(new FakeConverter("application/pdf", "image/png"));

        Assert.Equal(new[] { "image/png", "text/plain" }, registry.TargetsFor("application/pdf"));
    }
}
