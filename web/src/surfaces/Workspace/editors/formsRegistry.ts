/**
 * Client-side forms registry: maps JSONL content to a server-side schema name.
 *
 * Resolution order for a given JSONL text:
 *   1. Explicit  — $schema field in the meta line wins immediately.
 *   2. Registry  — entries are checked in registration order; first match wins.
 *   3. null      — no form available; FormsEditor shows "switch to Text".
 *
 * New entries can be registered from anywhere (plugins, future surfaces) by
 * calling registerForm(). Built-in registrations live at the bottom of this file.
 */

export interface FormsRegistryEntry {
  /** Schema name sent to the server via schema.get (e.g. "sql-table@1"). */
  schemaName: string;
  /**
   * Auto-detection predicate. Called when no explicit $schema is found in the file.
   * Return true to claim this entry handles the given JSONL text.
   */
  detect?: (jsonl: string) => boolean;
}

const _registry: FormsRegistryEntry[] = [];

export function registerForm(entry: FormsRegistryEntry): void {
  _registry.push(entry);
}

/** Read-only snapshot of current registrations (for debugging / settings UI). */
export function listForms(): readonly FormsRegistryEntry[] {
  return _registry;
}

/**
 * Resolve the schema name for `jsonl`:
 *  - Returns the explicit `$schema` value if present in the file.
 *  - Otherwise walks the registry and returns the first matching entry's schemaName.
 *  - Returns null if nothing matches.
 */
export function resolveSchemaName(jsonl: string): string | null {
  // 1. Explicit $schema in any line (typically the meta line)
  for (const raw of jsonl.split("\n")) {
    const t = raw.trim();
    if (!t) continue;
    try {
      const row = JSON.parse(t) as Record<string, unknown>;
      if (typeof row.$schema === "string") return row.$schema;
    } catch { /* not valid JSON — skip */ }
  }

  // 2. Registry auto-detection
  for (const entry of _registry) {
    if (entry.detect?.(jsonl)) return entry.schemaName;
  }

  return null;
}

// ── Built-in registrations ────────────────────────────────────────────────────

/**
 * SQL table description (sql-table@1):
 * Flat JSONL where each line is {field, type, desc?, agg?, note?}.
 * Detects by presence of "field" + "type" string keys in any line.
 */
registerForm({
  schemaName: "sql-table@1",
  detect: (jsonl) =>
    jsonl.split("\n").some((raw) => {
      const t = raw.trim();
      if (!t) return false;
      try {
        const row = JSON.parse(t) as Record<string, unknown>;
        return typeof row["field"] === "string" && typeof row["type"] === "string";
      } catch {
        return false;
      }
    }),
});
