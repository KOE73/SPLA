# Data Ownership Rules

These rules apply to all code in this repository.

The core principle: **UI displays data. UI does not own, store, or discover data.**

Violations of this principle have caused real architectural defects in this project (skill registry living inside `SidebarPanelViewModel`, toggle flags lost on restart, CLI unaware of features that existed in UI). Do not repeat them.

## Hard rules

### 1. UI ViewModels must not be the source of truth for domain data

A ViewModel may hold display state (selected item, scroll position, expanded node).
A ViewModel must NOT hold:

- Registry of plugins, skills, tools, or any capability
- Business flags such as "is this skill enabled" or "is this tool active"
- Discovery logic (scanning directories, reading manifests, parsing files)
- Content that is later read back from the ViewModel to build a system prompt

If you catch yourself writing `foreach (var item in SomeViewModel.GetItems())` to build a system prompt — stop. The data belongs in a service or manager, not in the ViewModel.

### 2. Flags that affect system behavior must be stored in the domain layer

Toggle states (enabled, preloaded, disabled) belong on the domain object — `SkillMeta`, `PluginDescriptor`, `CapabilityItem` — not on the ViewModel node.

The ViewModel binds to the flag. It does not own it.

Why: flags that live in ViewModels cannot be persisted, cannot be read by CLI, cannot be tested without instantiating UI components.

### 3. Discovery happens in the manager, not in the UI

File system scanning, manifest parsing, and directory enumeration for plugins, skills, or any runtime capability belong in:

- `PluginManager` — for plugins and their tools
- `SkillManager` — for skills (to be created)
- A dedicated service class if neither applies

The Sidebar, the MainWindow, and any View or ViewModel receive a pre-built list. They do not scan anything.

### 4. CLI and UI must share the same capability data

If a feature (skill, plugin, tool flag) is only accessible from the UI because it was discovered inside a ViewModel, the architecture is wrong.

`SPLA.MCP.Core` is the shared layer. Anything that CLI and UI both need must live there or in `SPLA.Domain`.

### 5. Sidebar is a display component, not a registry

`SidebarPanelViewModel` may:

- Display a tree built from a list it receives
- Forward toggle actions to the domain object
- Call a service to load skill content on demand

`SidebarPanelViewModel` must NOT:

- Own `_skillNodes` as the authoritative skill list
- Scan `plugins/*/skills/` directories
- Implement `GetEnabledSkillContents()` that is called from outside to build the prompt
- Parse frontmatter

## Checklist before adding any new capability type

1. Is there a manager/service in `SPLA.MCP.Core` or `SPLA.Domain` that owns it? If not, create one first.
2. Do the flags (enabled, preloaded, etc.) live on the domain object and get persisted? If not, add them there.
3. Can the CLI access this capability without touching any UI assembly? If not, move the data.
4. Does the ViewModel only receive a list and display it? If the ViewModel is scanning or parsing — refactor.
