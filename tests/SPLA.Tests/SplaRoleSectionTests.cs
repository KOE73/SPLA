using SPLA.Domain.Models;
using SPLA.Domain.Settings;

namespace SPLA.Tests;

/// <summary>
/// Wave 1 of <c>docs/plans/PLAN_20260902_agent_roles-and-correspondence.md</c>: a role as a type of
/// actor settings — <c>docs/adr/ADR_20260827-2_core_roles.md</c> §2.1. Each test below is named after
/// one row of that table; see the row's own comment for which claim it proves.
/// </summary>
public sealed class SplaRoleSectionTests
{
    private static string TempProjectDir() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-roles-{Guid.NewGuid():N}")).FullName;

    private static void WriteRoleFile(string projectDir, string roleName, string yaml)
    {
        var rolesDir = Directory.CreateDirectory(Path.Combine(projectDir, "roles")).FullName;
        File.WriteAllText(Path.Combine(rolesDir, roleName + ".yaml"), yaml);
    }

    // ── "agent:" is role zero — an existing manifest must not change meaning ───────────────────

    [Fact]
    public void An_existing_real_manifest_resolves_exactly_as_before_roles_existed()
    {
        // A manifest in the shape the repository's own spla.spla had before roles existed — kept as a
        // fixture rather than read from the live file, which is this workspace's working config and
        // has since declared roles of its own. Adding the field to SplaProject must be purely additive.
        var dir = TempProjectDir();
        try
        {
            var manifestPath = Path.Combine(dir, "spla.spla");
            File.WriteAllText(manifestPath, """
                version: 1
                name: SPLA
                agent:
                  mode: Agent
                  instructions:
                  - AGENTS.md
                  custom_prompt: Test project for SPLA Roslyn plugin validation.
                ui:
                  theme: cream
                """);
            var project = ConfigLoader.LoadProject(manifestPath);
            var defaults = new SplaDefaults();

            var resolved = SettingsResolver.Resolve(defaults, project);

            Assert.Null(project.Roles);
            Assert.Equal(AgentMode.Agent, resolved.Mode);
            Assert.Contains("Roslyn", resolved.CustomPrompt);
            Assert.Equal("cream", resolved.Theme);
            Assert.Null(resolved.RoleName);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    // ── "A file present in roles/ that the manifest does not name does not act" ────────────────

    [Fact]
    public void An_undeclared_role_file_on_disk_is_refused_even_though_it_loads_fine()
    {
        var dir = TempProjectDir();
        try
        {
            WriteRoleFile(dir, "reviewer", "mode: Research\n");
            var section = ConfigLoader.LoadRole(dir, "reviewer");
            Assert.NotNull(section); // loading a file is not the same as activating it

            var project = new SplaProject(); // roles: not written at all
            var baseline = SettingsResolver.Resolve(new SplaDefaults(), project);

            var ex = Assert.Throws<InvalidOperationException>(
                () => SettingsResolver.ResolveForRole(baseline, project, "reviewer", section));
            Assert.Contains("reviewer", ex.Message);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    [Fact]
    public void A_role_named_in_the_manifest_and_present_on_disk_does_act()
    {
        var dir = TempProjectDir();
        try
        {
            WriteRoleFile(dir, "reviewer", "mode: Research\n");
            var section = ConfigLoader.LoadRole(dir, "reviewer");

            var project = new SplaProject { Roles = ["reviewer"] };
            var baseline = SettingsResolver.Resolve(new SplaDefaults(), project);

            var resolved = SettingsResolver.ResolveForRole(baseline, project, "reviewer", section);

            Assert.Equal("reviewer", resolved.RoleName);
            Assert.Equal(AgentMode.Research, resolved.Mode);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    // ── "A role is not self-assigned": the only real invariant ─────────────────────────────────

    [Fact]
    public void No_role_name_is_ever_accepted_without_being_declared_first()
    {
        // The gate is the declared-names list, not possession of a loaded SplaRoleSection. An agent
        // that somehow obtained (or fabricated in memory) a perfectly well-formed role body still
        // cannot make it act — there is no code path that trusts the caller's object over the
        // manifest's own list.
        var project = new SplaProject { Roles = ["architect"] }; // "hacker" is not among these
        var baseline = SettingsResolver.Resolve(new SplaDefaults(), project);
        var fabricated = new SplaRoleSection { Mode = "Agent", Capabilities = ["core.everything"] };

        var ex = Assert.Throws<InvalidOperationException>(
            () => SettingsResolver.ResolveForRole(baseline, project, "hacker", fabricated));
        Assert.Contains("hacker", ex.Message);
        Assert.Contains("architect", ex.Message); // names what IS declared, for a human to check
    }

    [Fact]
    public void A_declared_role_whose_file_is_missing_is_refused_rather_than_silently_falling_back_to_role_zero()
    {
        var project = new SplaProject { Roles = ["ghost"] };
        var baseline = SettingsResolver.Resolve(new SplaDefaults(), project);

        Assert.Throws<InvalidOperationException>(
            () => SettingsResolver.ResolveForRole(baseline, project, "ghost", roleSection: null));
    }

    // ── "A role is NOT a subset of the project's permissions" ──────────────────────────────────

    [Fact]
    public void A_role_may_declare_a_capability_absent_from_agent_and_gets_it()
    {
        var dir = TempProjectDir();
        try
        {
            var project = new SplaProject
            {
                Roles = ["specialist"],
                Agent = new SplaAgentSection { Capabilities = ["core.read"] }
            };
            var baseline = SettingsResolver.Resolve(new SplaDefaults(), project);
            Assert.DoesNotContain("core.sql", baseline.Capabilities ?? []);

            var role = new SplaRoleSection { Capabilities = ["core.sql"] };
            var resolved = SettingsResolver.ResolveForRole(baseline, project, "specialist", role);

            // Wholesale replacement, not a union and not an intersection with agent:'s list — a role
            // reaching for something agent: never declared is exactly the point being proved here.
            Assert.Equal(["core.sql"], resolved.Capabilities);
            Assert.DoesNotContain("core.read", resolved.Capabilities!);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    [Fact]
    public void A_role_cannot_widen_the_project_root_no_matter_what_its_file_contains()
    {
        var dir = TempProjectDir();
        try
        {
            // A role file cannot declare mounts, a workspace path, or anything else that could move
            // the root — SplaRoleSection simply has no such field, so even a mischievous file (extra
            // unmatched keys) changes nothing. IgnoreUnmatchedProperties makes this the deserializer's
            // job, not a runtime check's — this test proves that pairing still holds.
            WriteRoleFile(dir, "outsider", """
                mode: Agent
                mounts:
                  - name: escape
                    path: ../../
                workspace: C:\
                """);
            var section = ConfigLoader.LoadRole(dir, "outsider");
            Assert.NotNull(section);

            var manifestPath = Path.Combine(dir, "project.spla");
            File.WriteAllText(manifestPath, "version: 1\nroles: [outsider]\n");
            var project = ConfigLoader.LoadProject(manifestPath);
            var baseline = ConfigLoader.LoadAndResolve(manifestPath);

            var resolved = SettingsResolver.ResolveForRole(baseline, project, "outsider", section);

            Assert.Equal(baseline.WorkspacePath, resolved.WorkspacePath);
            Assert.Same(baseline.Mounts, resolved.Mounts);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    // ── "Role picks a mode and narrows within it, not a second axis of permissions" ────────────

    [Fact]
    public void A_role_has_no_permissions_block_of_its_own()
    {
        // Structural, not behavioural: SplaRoleSection must not grow read/write/shell/internet fields.
        // Two overlapping permission systems is exactly what the ADR rejected.
        var properties = typeof(SplaRoleSection).GetProperties().Select(p => p.Name);
        Assert.DoesNotContain("PermRead", properties);
        Assert.DoesNotContain("Permissions", properties);
    }

    [Fact]
    public void A_role_selects_a_mode_the_same_way_agent_does_and_it_replaces_the_projects_mode()
    {
        var project = new SplaProject
        {
            Roles = ["chatty"],
            Agent = new SplaAgentSection { Mode = "Agent" }
        };
        var baseline = SettingsResolver.Resolve(new SplaDefaults(), project);
        Assert.Equal(AgentMode.Agent, baseline.Mode);

        var role = new SplaRoleSection { Mode = "Chat" };
        var resolved = SettingsResolver.ResolveForRole(baseline, project, "chatty", role);

        Assert.Equal(AgentMode.Chat, resolved.Mode);
        // The baseline itself is untouched — ResolveForRole must not mutate what it was given.
        Assert.Equal(AgentMode.Agent, baseline.Mode);
    }

    // ── model / toolsets / islands carried through, narrowing only ─────────────────────────────

    [Fact]
    public void A_roles_model_toolsets_and_islands_are_carried_onto_the_resolved_settings()
    {
        var project = new SplaProject
        {
            Roles = ["narrow"],
            ToolSets = new Dictionary<string, string> { ["ssh"] = "enabled" }
        };
        var baseline = SettingsResolver.Resolve(new SplaDefaults(), project);

        var role = new SplaRoleSection
        {
            Model = "gpt-fast",
            ToolSets = new Dictionary<string, string> { ["network"] = "disabled" },
            Islands = ["sql:prod-db"]
        };
        var resolved = SettingsResolver.ResolveForRole(baseline, project, "narrow", role);

        Assert.Equal("gpt-fast", resolved.RoleModelId);
        Assert.Equal(["sql:prod-db"], resolved.RoleIslands);
        // The role's toolsets: merges over the project's own, key by key — same rule every other
        // layer in SettingsResolver already follows.
        Assert.Equal("enabled", resolved.ToolSets["ssh"]);
        Assert.Equal("disabled", resolved.ToolSets["network"]);
        // Untouched on the baseline.
        Assert.DoesNotContain("network", baseline.ToolSets.Keys);
    }
}
