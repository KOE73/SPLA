# SPLA Architecture Tool (`spla-arch`)

> **Retired (2026-08-31). Do not run.** This tool wrote `docs/diagrams/model-*.json`;
> those files are gone and the model now lives in `docs/diagrams/projects/` under
> contract **v3** ([CONTRACT.md](../spla-diagram/docs/CONTRACT.md)). Its successor
> [`tools/spla-atlas`](../spla-atlas/) (Roslyn) is **not written yet**, so no
> code-to-diagram sync exists at the moment. Kept as a reference for rule
> resolution and finding wording — see [AGENTS.md](AGENTS.md).

Lightweight Go CLI tool for deterministic architecture extraction, C# source parsing, and diagram JSON graph management.

## Capabilities

1. **`parse`**: Recursively scans C# projects, extracts classes, interfaces, records, structs, and generates inheritance/implementation relationships (`implements`, `extends`) automatically into a clean, typed JSON model.
2. **`layout`**: Deterministic grid layout engine for aligning nodes inside their container zones.

## Usage

```bash
# Scan a C# subsystem and generate a diagram model
go run main.go parse --src ../../src/core --out ../../docs/diagram-mvp/model-core-auto.json

# Align nodes in a diagram
go run main.go layout --file ../../docs/diagram-mvp/model.json
```
