# Known Issues

## Roslyn Plugin — In-Memory Compilation Missing BCL References

**Severity:** High  
**Status:** Confirmed (2026-08-28)  
**Affected tools:** `roslyn_project_build`, `roslyn_compile_check`, `roslyn_script_run`

### Issue

`compile_check` and `script_run` fail during compilation because the in-memory Roslyn compiler pipeline does not include metadata references to the .NET Base Class Library (BCL). Even trivial code fails:

```csharp
// compile_check: fails with CS0518 (System.Object not found)
public class A { }

// script_run: fails with NotSupportedException before first line
ctx.Log("hi");
```

### Root Cause

The in-memory compilation in these tools does not pass `MetadataReference` objects for `System.Private.CoreLib` and other essential BCL assemblies to the Roslyn compiler. The issue occurs at compile-time, before any code execution.

**Evidence:**
- `compile_check` with empty class: `CS0518: System.Object not defined or imported`
- `script_run` with single line: `NotSupportedException: Cannot create metadata reference in assembly without location`
- `project_build` (real dotnet SDK) works fine with the same code

### Workarounds

| Tool | Issue | Workaround |
|---|---|---|
| `compile_check` | No BCL in compile pipeline | Use `roslyn_project_build` instead — create temp .csproj and check diagnostics against real SDK |
| `script_run` | No BCL in compile pipeline | No direct replacement — use sequential calls to `project_run` or other tools |

### Impact

- Serial-only execution: `script_run` cannot run parallel multi-step plans
- Compile-time checks require creating temporary project files

### Related

- [[Roslyn plugin plan]](../docs/adr/roslyn-plugin-plan.md) — feature roadmap for compile-check, navigation, refactoring
