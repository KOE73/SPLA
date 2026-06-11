# Avalonia UI Development Rules

These rules apply to:

- `SPLA.UI.Avalonia`
- `SPLA.Plugins.Host.Avalonia`
- any Avalonia-based UI plugin such as `SPLA.Plugins.*.Avalonia`

The goal is predictable structure, theme consistency, and minimal UI drift.

## 1. Default rule: create AXAML immediately

For every non-trivial Avalonia visual component, create:

- `*.axaml`
- `*.axaml.cs`

immediately.

Do not start with a large code-only `new Grid { ... }` / `new StackPanel { ... }` tree unless the UI is truly tiny and temporary.

Mandatory rule:

- if the control has more than a few elements, multiple regions, buttons, lists, cards, or embedded controls such as `NativeWebView`, it must be created in `AXAML` from the start.

Why:

- layout is easier to read;
- theme bindings are visible;
- later refactoring to styles or ViewModel is cheaper;
- plugin UI does not become a monolithic code-behind file.

## 2. Folder structure

Place Avalonia UI files in explicit UI folders.

Preferred structure:

```text
Views/
  Feature/
    FeaturePanel.axaml
    FeaturePanel.axaml.cs
Controls/
  ReusableThing.axaml
  ReusableThing.axaml.cs
Styles/
  FeatureStyles.axaml
ViewModels/
  FeatureViewModel.cs
```

Do not keep real views in the project root when they belong to a visual feature.

## 3. Split responsibilities

Use this minimum separation:

- `AXAML`: layout, bindings, visual tree, styles, resources usage.
- `axaml.cs`: control wiring, event hookup, small view-specific glue.
- `ViewModel`: state, commands, async loading, filtering, selection, derived text, tool-facing data.

Allowed in code-behind:

- `InitializeComponent()`;
- `FindControl(...)`;
- event wiring for control-specific behavior;
- view-only interop such as `NativeWebView.NavigateToString(...)`;
- temporary glue during incremental migration.

Not allowed as the final state in code-behind:

- large hand-built visual trees;
- business logic mixed with control construction;
- local theme helper classes that duplicate host resources;
- screen-sized layout assembled with imperative `new Border/new Grid/...`.

## 4. Theme and resource rules

Avalonia UI must use shared `DynamicResource` theme keys from the host.

Use:

- `AppBackgroundBrush`
- `PanelBackgroundBrush`
- `PanelBorderBrush`
- `TextForegroundBrush`
- `SubTextForegroundBrush`
- `AccentBrush`
- `InputBackgroundBrush`
- `InputBorderBrush`
- density resources such as `StandardMargin`, `StandardPadding`, `CompactMargin`, `StandardSpacing`, `HeaderFontSize`, `BaseFontSize`, `SmallFontSize`, `StandardCornerRadius`

Do not:

- hardcode colors in controls;
- build local `UiTheme` helper classes that re-resolve the same resource keys for ordinary layout;
- duplicate the host theme system inside a plugin.

If a feature needs repeated local visuals, add local styles in `Styles/*.axaml`, but still bind those styles to host resources.

## 5. Plugin UI rules

For Avalonia UI plugins:

- UI must live in the plugin project, not in `SPLA.UI.Avalonia`.
- `MainWindowViewModel` and main UI must not know concrete plugin views.
- plugin panels are opened through generic host abstractions.
- plugin-specific visuals stay inside the plugin assembly.

If the plugin has a real screen or panel, create `AXAML` immediately. Do not keep plugin UI as a long code-built tree just because the plugin is loaded dynamically.

## 6. When code-only UI is acceptable

Code-only UI is acceptable only for:

- tiny temporary diagnostic controls;
- one-off fallback placeholders;
- extremely small generated fragments;
- short-lived experiments that are explicitly marked temporary.

Even then:

- keep them small;
- move to `AXAML` once the control becomes user-facing or grows beyond a minimal placeholder.

## 7. Incremental migration rule

If an existing control is already too large in code-behind, migrate in this order:

1. move the layout into `AXAML`;
2. keep current logic in `axaml.cs`;
3. extract repeated visual parts into `Styles/` or reusable controls;
4. move state and commands into `ViewModel`.

This is the preferred low-risk migration path.

## 8. Exceptions must be explicit

If you intentionally keep Avalonia UI in code for a while, document it in the feature folder with a short note explaining:

- why `AXAML` was skipped temporarily;
- what the next extraction step is;
- what remains to be moved.

Temporary exceptions should not silently become the permanent project style.
