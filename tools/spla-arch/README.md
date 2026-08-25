# SPLA Architecture Tool (`spla-arch`)

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
