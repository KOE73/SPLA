# src/plugins — plugins & skills

All `SPLA.Plugins.*` (DLL and `avalonia-ui` plugins), `SPLA.Plugins.Host.Avalonia` (UI-plugin
contracts), and `SPLA.Skills.Network` (skills = markdown data, not code). Read the root `AGENTS.md`
and `agents/plugins.md` first.

## Invariants (all plugins)

- **Tool names follow `[plugin].[domain].[action]`** (or the established `pluginid_*` snake form the
  plugin already uses). The `pluginId` prefix is how settings enable/disable and how help routing
  works.
- **Plugins are lower-trust than core.** A plugin may not grant itself a privileged capability:
  never declare `Scope=Agent` or `Scope=Skill` on a plugin tool — those are core-only. Declare
  `Scope`/`Effect`/`Risk` honestly for what the tool really does; the permission layer trusts the
  declaration, so under-declaring risk is a security bug.
- **`AssemblyLoadContext` isolation is deliberate.** Shared contract assemblies (`SPLA.Domain`,
  `SPLA.MCP.Core`, `SPLA.Observability`) load from the host, everything else from the plugin folder
  (see `PluginLoadContext`). Do not add a plugin dependency that must be unified with the host unless
  it is on that shared list.
- **Skills are data, not capabilities.** A `.md` skill describes a procedure; it does not declare
  what a tool may do. Keep skill descriptions/triggers in English (system-prompt contract layer).

## Per-plugin AGENTS.md

Add a `SPLA.Plugins.<Name>/AGENTS.md` only when that plugin has rules or an architecture that don't
generalise (e.g. the Roslyn script runner's execution model, a plugin's own indexing/storage
design). Do not add a near-empty "see parent" file — this shared file is the default.
