# SPLA UI Theming & Density Guidelines

See also:

- [Avalonia UI Development Rules](avalonia.md)

To ensure a consistent and compact user interface, Avalonia UI styling in SPLA relies on a strict set of global `DynamicResource` bindings. **Never use hardcoded colors or fixed margins in UI controls.**

## 1. Color Palette (Themes)

All colors are provided by the active theme dictionary (`Dark.axaml`, `Light.axaml`, `Cream.axaml`, `Emerald.axaml`). When building UI controls, always bind to the following keys using `{DynamicResource [Key]}`:

| Resource Key | Usage Rule |
|--------------|------------|
| `AppBackgroundBrush` | The deepest background layer (e.g., the main Window background). |
| `PanelBackgroundBrush` | Background for sidebars, status bars, and settings cards. |
| `PanelBorderBrush` | Borders separating panels, splitters, and card outlines. |
| `TextForegroundBrush` | Primary text (headers, chat messages, standard labels). |
| `SubTextForegroundBrush` | Secondary text (hints, dates, token counts, paths). |
| `AccentBrush` | Primary interactive elements (buttons, active tabs, important links). |
| `MessageBackgroundBrush` | Background specifically for chat message bubbles. |
| `InputBackgroundBrush` | Background for TextBoxes, ComboBoxes, and input areas. |
| `InputBorderBrush` | Borders for interactive input fields. |

**Anti-Pattern:**
```xml
<!-- WRONG: Hardcoded color breaks theme switching -->
<UserControl Background="#252526" /> 
```
**Correct Pattern:**
```xml
<UserControl Background="{DynamicResource PanelBackgroundBrush}" /> 
```

## 2. Density (Spacing & Sizing)

To support compact interfaces, SPLA will use "Density" presets: `nano`, `mini`, `norm`, `max`. 
These control margins, padding, and font sizes across the entire application.

Instead of hardcoding spacing (e.g., `Margin="10"`), UI elements must bind to `DynamicResource` spacing keys.

| Resource Key | Description | `nano` | `mini` | `norm` | `max` |
|--------------|-------------|--------|--------|--------|-------|
| `StandardMargin` | Default space around elements | `2` | `5` | `10` | `15` |
| `StandardPadding`| Default space inside cards/buttons | `4` | `8` | `15` | `20` |
| `CompactMargin`  | Space for tight lists/items | `0` | `2` | `5` | `10` |
| `HeaderFontSize` | Main category titles | `14` | `16` | `18` | `22` |
| `BaseFontSize`   | Normal text | `10` | `12` | `14` | `16` |
| `SmallFontSize`  | Hints, dates, tags | `8` | `10` | `12` | `14` |

**Implementation Rule:**
Instead of `FontSize="14" Margin="0,10"`, write:
```xml
<TextBlock FontSize="{DynamicResource BaseFontSize}" 
           Margin="{DynamicResource StandardMargin}" />
```
*(Note: For complex margins like `0,10,0,10`, Avalonia allows binding `Thickness` resources).*

## 3. Settings UI Structure

The Settings window must follow a master-detail pattern (Two-pane layout):
- **Left Pane:** Vertical navigation menu (List of categories).
- **Right Pane:** Scrollable list of "Cards". Each card represents a logical group (e.g., "Agent Behavior", "Local Permissions").
- **Cards:** Must use `PanelBackgroundBrush`, a subtle `PanelBorderBrush`, and have rounded corners (`CornerRadius="8"`). Controls within the card should align to the right, while the description stays on the left (as seen in modern IDEs).

## 4. Avalonia structure reminder

Theming rules are not a replacement for view structure rules.

Mandatory companion rules:

- build non-trivial Avalonia views in `AXAML` immediately;
- keep layout in `*.axaml`;
- keep view glue in `*.axaml.cs`;
- keep state and commands in `ViewModel` when the feature grows;
- prefer local `Styles/*.axaml` over code-only theme helper classes.

Formal structural rules are defined in [Avalonia UI Development Rules](avalonia.md).
