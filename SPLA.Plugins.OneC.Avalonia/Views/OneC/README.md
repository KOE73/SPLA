Temporary minimal split:

- `OneCOverviewPanel.axaml` contains layout only.
- `OneCOverviewPanel.axaml.cs` keeps current logic and event wiring.

Next step, if needed:

- move state/commands into ViewModel;
- extract local OneC styles from inline button/border markup;
- keep host theme resources via `DynamicResource`.
