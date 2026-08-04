using SPLA.Domain.Settings;
using SPLA.Library;
using SPLA.Library.Catalog;
using SPLA.Library.Format;
using SPLA.Library.Sources;

namespace SPLA.Tests;

/// <summary>Covers the providers themselves: what each one enumerates, what it refuses to read, and
/// the frontmatter contract they all share.</summary>
public class SkillSourceTests : IDisposable
{
    private readonly string _temp = Path.Combine(
        Path.GetTempPath(), "spla_skillsrc_" + Path.GetRandomFileName());

    public void Dispose()
    {
        if (Directory.Exists(_temp)) Directory.Delete(_temp, recursive: true);
    }

    private string Dir(params string[] parts)
    {
        var path = Path.Combine([_temp, .. parts]);
        Directory.CreateDirectory(path);
        return path;
    }

    private static void Write(string path, string text) => File.WriteAllText(path, text);

    // ── DirectorySkillSource ─────────────────────────────────────────────────

    [Fact]
    public void Directory_source_on_a_missing_folder_is_empty_not_an_error()
    {
        var source = new DirectorySkillSource(
            "project", "project", Path.Combine(_temp, "nope"), SkillTrust.Trusted, watch: false);

        Assert.Empty(source.Enumerate());
    }

    /// <summary>
    /// A folder that does not exist yet is the normal state of <c>.spla/skills</c> — it appears the
    /// moment the user writes a first draft. FileSystemWatcher cannot watch a missing path, so the
    /// source watches the nearest existing ancestor until the root shows up. Without this the very
    /// folder a user just created stayed dark until restart, which is the opposite of hot reload.
    /// </summary>
    [Fact]
    public void Directory_source_lights_up_when_its_folder_is_created_later()
    {
        Directory.CreateDirectory(_temp);
        var root = Path.Combine(_temp, "later", "skills");

        using var source = new DirectorySkillSource("local", "local", root, SkillTrust.Trusted);
        using var signal = new ManualResetEventSlim();
        source.Changed += () => signal.Set();

        Assert.Empty(source.Enumerate());

        Directory.CreateDirectory(root);
        Write(Path.Combine(root, "drafted.md"), "A procedure written just now.");

        Assert.True(signal.Wait(TimeSpan.FromSeconds(10)), "the source never reported the new folder");
        Assert.Contains(source.Enumerate(), e => e.Id == "drafted");
    }

    [Fact]
    public void Directory_source_reads_flat_files_and_folder_skills()
    {
        var root = Dir("skills");
        Write(Path.Combine(root, "release-notes.md"), "Just a procedure, no frontmatter.");

        var folder = Dir("skills", "host-audit");
        Write(Path.Combine(folder, "SKILL.md"), """
            ---
            id: net.host-audit
            description: audits a host
            ---
            Step 1: scan.
            """);

        var source = new DirectorySkillSource("project", "project", root, SkillTrust.Trusted, watch: false);
        var entries = source.Enumerate();

        // A bare .md with no frontmatter is a valid skill; its id falls back to the file name.
        var flat = Assert.Single(entries, e => e.Id == "release-notes");
        Assert.Equal(string.Empty, flat.Description);

        var folderSkill = Assert.Single(entries, e => e.Id == "net.host-audit");
        Assert.Equal("audits a host", folderSkill.Description);
        Assert.Equal("Step 1: scan.", source.ReadBody(folderSkill.Ref)?.Trim());
    }

    /// <summary>Subfolders without a SKILL.md are grouping, not skills, and the walk continues into
    /// them. A file that declares no id gets one from its path, so the same leaf name in two folders
    /// does not collide.</summary>
    [Fact]
    public void Directory_source_descends_through_grouping_folders_and_names_by_path()
    {
        var root = Dir("skills");
        Write(Path.Combine(Dir("skills", "network"), "dns.md"), "how to check dns");
        Write(Path.Combine(Dir("skills", "onec"), "dns.md"), "unrelated, same file name");
        Write(Path.Combine(Dir("skills", "a", "b", "c"), "deep.md"), "nested three levels");

        var ids = new DirectorySkillSource("repo", "Repository", root, SkillTrust.Trusted, watch: false)
            .Enumerate().Select(e => e.Id).ToList();

        Assert.Contains("network.dns", ids);
        Assert.Contains("onec.dns", ids);
        Assert.Contains("a.b.c.deep", ids);
    }

    /// <summary>A folder holding a SKILL.md IS the skill; its siblings are that skill's resources and
    /// must not be enumerated as separate skills.</summary>
    [Fact]
    public void A_folder_with_SKILL_md_is_one_skill_and_its_resources_are_not_scanned()
    {
        var root = Dir("skills");
        var skillDir = Dir("skills", "host-audit");
        Write(Path.Combine(skillDir, "SKILL.md"), "---\nid: net.host-audit\n---\nStep 1.");
        Write(Path.Combine(Dir("skills", "host-audit", "references"), "ports.md"), "a resource, not a skill");

        var ids = new DirectorySkillSource("repo", "Repository", root, SkillTrust.Trusted, watch: false)
            .Enumerate().Select(e => e.Id).ToList();

        Assert.Equal(["net.host-audit"], ids);
    }

    [Fact]
    public void Readme_and_tooling_folders_are_not_skills()
    {
        var root = Dir("skills");
        Write(Path.Combine(root, "README.md"), "# skills\n\nHow this folder works.");
        Write(Path.Combine(Dir("skills", "node_modules", "pkg"), "doc.md"), "vendored noise");
        Write(Path.Combine(Dir("skills", ".git"), "hook.md"), "vcs noise");
        Write(Path.Combine(root, "real.md"), "an actual skill");

        var ids = new DirectorySkillSource("repo", "Repository", root, SkillTrust.Trusted, watch: false)
            .Enumerate().Select(e => e.Id).ToList();

        Assert.Equal(["real"], ids);
    }

    [Fact]
    public void Directory_source_refuses_a_ref_that_escapes_its_root()
    {
        var root = Dir("skills");
        Write(Path.Combine(_temp, "outside.md"), "secret");

        var source = new DirectorySkillSource("project", "project", root, SkillTrust.Trusted, watch: false);

        Assert.Null(source.ReadBody("../outside.md"));
    }

    // ── PluginSkillSource ────────────────────────────────────────────────────

    [Fact]
    public void Plugin_source_reads_both_package_layouts()
    {
        var pluginDir = Dir("plugins", "network");
        Write(Path.Combine(pluginDir, "at-root.md"), "---\nid: net.a\n---\nbody a");
        Write(Path.Combine(Dir("plugins", "network", "skills"), "in-subfolder.md"), "---\nid: net.b\n---\nbody b");

        var source = new PluginSkillSource("network", "Network", pluginDir, () => true);
        var ids = source.Enumerate().Select(e => e.Id).ToList();

        Assert.Contains("net.a", ids);
        Assert.Contains("net.b", ids);
    }

    /// <summary>The original defect. A disabled plugin's skills must not exist as far as the rest of
    /// the system is concerned — not "exist but get filtered later", which is what let them through.</summary>
    [Fact]
    public void Plugin_source_yields_nothing_while_its_plugin_is_disabled()
    {
        var pluginDir = Dir("plugins", "network");
        Write(Path.Combine(pluginDir, "host-audit.md"), "---\nid: network.host-audit\n---\nscan it");

        var enabled = false;
        var source = new PluginSkillSource("network", "Network", pluginDir, () => enabled);

        Assert.Empty(source.Enumerate());

        enabled = true;
        Assert.Single(source.Enumerate());
    }

    [Fact]
    public void Disabled_plugin_skills_never_reach_the_manager()
    {
        var pluginDir = Dir("plugins", "network");
        Write(Path.Combine(pluginDir, "host-audit.md"), "---\nid: network.host-audit\n---\nscan it");

        var manager = new SkillLibrary([new PluginSkillSource("network", "Network", pluginDir, () => false)]);

        Assert.Empty(manager.Holdings());
        Assert.Empty(manager.Catalog());
        Assert.Null(manager.Find("network.host-audit"));
    }

    // ── Frontmatter ──────────────────────────────────────────────────────────

    [Fact]
    public void Frontmatter_parses_requirements_and_flags()
    {
        var entry = SkillFrontmatter.Parse("""
            ---
            id: net.audit
            description: does things
            requires:
              tools: [dns_lookup, port_scan]
              features: [core.memory]
            uses:
              tools: [tls_probe]
            ---
            the body
            """, "fallback", "net.md");

        Assert.Equal("net.audit", entry.Id);
        Assert.Equal(["dns_lookup", "port_scan"], entry.Requires.Tools);
        Assert.Equal(["core.memory"], entry.Requires.Features);
        Assert.Equal(["tls_probe"], entry.Uses.Tools);
        Assert.True(entry.DefaultEnabled);
    }

    [Fact]
    public void Frontmatter_without_requirements_is_empty_not_null()
    {
        var entry = SkillFrontmatter.Parse("---\nid: plain\n---\nbody", "fallback", "plain.md");

        Assert.True(entry.Requires.IsEmpty);
        Assert.True(entry.Uses.IsEmpty);
    }

    /// <summary>The old IndexOf("---") parser truncated the header at the first dash run it found
    /// anywhere, including inside a value.</summary>
    [Fact]
    public void Frontmatter_delimiter_inside_a_value_does_not_end_the_header()
    {
        var entry = SkillFrontmatter.Parse("""
            ---
            id: net.audit
            description: "before --- after"
            ---
            real body
            """, "fallback", "x.md");

        Assert.Equal("net.audit", entry.Id);
        Assert.Equal("before --- after", entry.Description);
        Assert.Equal("real body", SkillFrontmatter.StripHeader("""
            ---
            id: net.audit
            description: "before --- after"
            ---
            real body
            """).Trim());
    }

    [Fact]
    public void Frontmatter_that_is_malformed_degrades_instead_of_disappearing()
    {
        var entry = SkillFrontmatter.Parse("---\n\tid: [unclosed\n---\nbody", "on-disk-name", "x.md");

        Assert.Equal("on-disk-name", entry.Id);
        Assert.True(entry.DefaultEnabled);
    }

    /// <summary>How real skills are written: an unquoted description containing ": ". That is not
    /// valid YAML, and a strict parser drops the whole header — losing the id too, so the skill
    /// silently vanishes. The salvage pass recovers the scalar fields.</summary>
    [Fact]
    public void Frontmatter_recovers_scalars_from_an_unquoted_colon_in_the_description()
    {
        var entry = SkillFrontmatter.Parse("""
            ---
            id: network.dns-diagnostics
            description: DNS troubleshooting. Trigger on: DNS not resolving, check DNS records.
            enabled: false
            ---
            Step 1.
            """, "dns-diagnostics", "dns.md");

        Assert.Equal("network.dns-diagnostics", entry.Id);
        Assert.StartsWith("DNS troubleshooting.", entry.Description);
        Assert.Contains("Trigger on: DNS not resolving", entry.Description);
        Assert.False(entry.DefaultEnabled);   // a scalar after the offending line is recovered too
    }

    // ── Registry ─────────────────────────────────────────────────────────────

    [Fact]
    public void Registry_builds_the_default_set_when_nothing_is_configured()
    {
        var context = new SkillSourceContext(_temp, Path.Combine(_temp, "home"), null);

        var sources = SkillSourceRegistry.Build(null, context);

        Assert.Equal(["repo", "local", "machine"], sources.Select(s => s.Id));
    }

    [Fact]
    public void Registry_skips_unusable_entries_and_keeps_the_rest()
    {
        var context = new SkillSourceContext(_temp, Path.Combine(_temp, "home"), null);

        var sources = SkillSourceRegistry.Build(
        [
            new SplaSkillSourceSection { Type = "directory" },                       // no path
            new SplaSkillSourceSection { Type = "carrier-pigeon", Path = "x" },      // no factory
            new SplaSkillSourceSection { Type = "directory", Path = ".spla/skills" }
        ], context);

        Assert.Equal(["local"], sources.Select(s => s.Id));
    }

    [Fact]
    public void Registry_appends_plugin_sources_after_configured_ones()
    {
        var context = new SkillSourceContext(_temp, Path.Combine(_temp, "home"), null);
        var pluginSource = new PluginSkillSource("network", "Network", Dir("plugins", "network"), () => true);

        var sources = SkillSourceRegistry.Build(
            [new SplaSkillSourceSection { Type = "directory", Path = ".spla/skills" }],
            context, [pluginSource]);

        Assert.Equal(["local", "plugin:network"], sources.Select(s => s.Id));
    }
}
