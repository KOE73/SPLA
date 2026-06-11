# SPLA Color Resources (Themes & Color Keys)

This file describes all the color brush (`SolidColorBrush`) resource keys used in SPLA.UI.Avalonia themes. All themes (`Cream.axaml`, `Dark.axaml`, `Emerald.axaml`, `Light.axaml`) must define this set of resources.

---

## 1. Core Application Colors

| Resource Key | Description |
| :--- | :--- |
| `AppBackgroundBrush` | The main background color for the application windows (main window, settings window). |
| `PanelBackgroundBrush` | The background color for side panels, settings blocks, and lists. |
| `PanelBorderBrush` | The color for borders, dividers, and frames between panels. |
| `TextForegroundBrush` | The primary text and header color. |
| `SubTextForegroundBrush` | The secondary (muted) text color used for hints, dates, and metadata. |
| `AccentBrush` | The main accent color (used for primary action buttons, focus, and highlighting active states). |

---

## 2. Chat & Message Bubble Colors

| Resource Key | Description |
| :--- | :--- |
| `MessageBackgroundBrush` | The base message background color in classic/document chat mode. |
| `UserBubbleBackgroundBrush` | The bubble background color for user messages (`User`) in both Bubble and Diagnostic chat views. |
| `AiBubbleBackgroundBrush` | The bubble background color for assistant/AI messages (`Assistant`) in both Bubble and Diagnostic chat views. |
| `SystemBubbleBackgroundBrush` | Dedicated background color for system messages (`System`) in the Diagnostic view. |
| `ToolBubbleBackgroundBrush` | Dedicated background color for tool execution outputs and call details (`Tool`) in the Diagnostic view. |
| `ErrorBubbleBackgroundBrush` | Dedicated background color for error messages (`Error`) in the Diagnostic view. |

---

## 3. Control & Component Colors

| Resource Key | Description |
| :--- | :--- |
| `InputBackgroundBrush` | The background color for text entry fields (`TextBox`). |
| `InputBorderBrush` | The border color for text entry fields (`TextBox`) in their default state. |
| `ComboBoxBackground` | The background color for dropdown fields (`ComboBox`). |
| `ComboBoxBorderBrush` | The border color for dropdown fields. |
| `ComboBoxForeground` | The text color inside dropdown fields. |
| `ComboBoxDropDownBackground` | The background color for the opened popup list of a ComboBox. |
| `ComboBoxItemBackgroundSelected` | The background color of the selected item within the dropdown list. |
