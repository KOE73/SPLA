<!--
  SQL plugin settings UI (built and shipped INSIDE the plugin, mounted by the host at runtime).
  Edits the plugins.sql.settings blob: named connections, each binding an address to a CREDENTIAL —
  a reference into the global secret store, never a literal password. Picking or creating that entry
  is the HOST's credential control, borrowed through CredentialSlot (values never enter this
  blob or the chat); the connection config keeps only the entry name. Mirrors the SSH plugin.
-->
<template>
  <div class="sql-set">
    <div class="muted">
      Named database connections available to the SQL agent. Passwords live in the secret store
      (Settings → Secrets); a connection only references an entry by name. Stored in the .spla project file.
    </div>

    <div class="row">
      <label><span class="muted">Default connection</span>
        <select v-model="defaultConnection">
          <option value="">(none)</option>
          <option v-for="c in connections" :key="c.key" :value="c.name">{{ c.name }}</option>
        </select>
      </label>
      <label><span class="muted">Default row limit</span>
        <input v-model.number="defaultLimit" type="number" min="1" class="w-90">
      </label>
    </div>

    <button type="button" class="self-start" @click="addConnection">+ Add Connection</button>

    <div v-if="!connections.length" class="muted empty">No connections yet. Click "+ Add Connection".</div>

    <div v-for="(c, i) in connections" :key="c.key" class="conn-card">
      <div class="row spread">
        <div class="row">
          <span class="muted">Name</span><input v-model="c.name" class="w-140" spellcheck="false">
          <span class="muted">Provider</span>
          <select v-model="c.provider">
            <option value="mssql">mssql</option>
            <option value="postgres">postgres</option>
            <option value="sqlite">sqlite</option>
          </select>
        </div>
        <button type="button" @click="connections.splice(i, 1)">✕ Remove</button>
      </div>

      <div v-if="c.provider !== 'sqlite'" class="row">
        <span class="muted w-70">Server</span>
        <input v-model="c.path" placeholder="sql01 or 192.168.1.10" class="w-220" spellcheck="false">
        <span class="muted w-70">Database</span>
        <input v-model="c.database" class="w-160" spellcheck="false">
      </div>
      <div v-else class="row">
        <span class="muted w-70">File</span>
        <input v-model="c.path" placeholder="C:\data\mydb.sqlite" class="w-400" spellcheck="false">
      </div>

      <template v-if="c.provider !== 'sqlite'">
        <div class="row">
          <label v-if="c.provider === 'mssql'" class="chk"><input type="checkbox" v-model="c.trustedConnection">
            <span>Windows Auth (domain)</span></label>
        </div>

        <template v-if="!c.trustedConnection || c.provider !== 'mssql'">
          <div class="row">
            <span class="muted w-70">Credential</span>
            <CredentialSlot :api="api" v-model="c.credential" />
          </div>

          <div class="row">
            <span class="muted w-70">User</span>
            <input v-model="c.user" :placeholder="c.credential ? '(from credential)' : 'login'" class="w-130" spellcheck="false">
          </div>
        </template>
      </template>

      <div class="row">
        <span class="muted w-70">Description</span>
        <input v-model="c.description" placeholder="Shown to the AI — what this database contains" class="grow">
      </div>

      <div class="row">
        <button type="button" :disabled="c.testing" @click="testConnection(c)">Test Connection</button>
        <span class="muted">{{ c.testStatus }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import CredentialSlot from "./CredentialSlot.vue";
import type { MountApi } from "./mount";

const props = defineProps<{ api: MountApi }>();

interface ConnRow {
  key: number;
  name: string;
  provider: string;
  path: string;      // server/host (mssql, postgres) or file (sqlite)
  database: string;
  user: string;
  credential: string;
  trustedConnection: boolean;
  description: string;
  testing: boolean;
  testStatus: string;
}

interface ConnCfg {
  provider: string;
  server?: string;
  database?: string;
  user?: string;
  credential?: string;
  password?: string;
  trusted_connection?: boolean;
  file?: string;
  description?: string;
}
interface SqlSettingsBlob {
  default_connection?: string;
  default_limit?: number;
  connections?: Record<string, ConnCfg>;
}

let nextKey = 0;
function rowFromCfg(name: string, cfg: ConnCfg): ConnRow {
  return {
    key: nextKey++,
    name,
    provider: cfg.provider || "mssql",
    path: cfg.provider === "sqlite" ? (cfg.file || "") : (cfg.server || ""),
    database: cfg.database || "",
    user: cfg.user || "",
    credential: cfg.credential || "",
    trustedConnection: cfg.trusted_connection ?? true,
    description: cfg.description || "",
    testing: false,
    testStatus: ""
  };
}
function rowToCfg(c: ConnRow): ConnCfg {
  return {
    provider: c.provider,
    server: c.provider === "sqlite" ? undefined : (c.path || undefined),
    file: c.provider === "sqlite" ? (c.path || undefined) : undefined,
    database: c.database || undefined,
    user: c.user || undefined,
    credential: c.credential || undefined,
    trusted_connection: c.provider === "mssql" ? c.trustedConnection : undefined,
    description: c.description || undefined
    // NOTE: no `password` — literals are written to the secret store via secret.set, never here.
  };
}

const blob: SqlSettingsBlob = (() => {
  try { return (JSON.parse(props.api.getJson() || "null") as SqlSettingsBlob) || {}; }
  catch { return {}; }
})();

const defaultConnection = ref(blob.default_connection || "");
const defaultLimit = ref(blob.default_limit || 10);
const connections = reactive<ConnRow[]>(
  Object.entries(blob.connections || {}).map(([name, cfg]) => rowFromCfg(name, cfg))
);

function addConnection() {
  connections.push(rowFromCfg(`db${connections.length + 1}`, { provider: "mssql" }));
}

async function testConnection(c: ConnRow) {
  c.testing = true;
  c.testStatus = "Connecting...";
  try {
    const result = await props.api.invoke<{ ok: boolean; resultJson?: string; error?: string }>("plugin.action", {
      pluginId: "sql",
      action: "testConnection",
      valueJson: JSON.stringify(toJsonCfg(c))
    });
    if (result.ok && result.resultJson) {
      const r = JSON.parse(result.resultJson) as { ok: boolean; message: string };
      c.testStatus = r.message;
    } else {
      c.testStatus = "Failed: " + (result.error || "unknown error");
    }
  } catch (e) {
    c.testStatus = "Failed: " + (e instanceof Error ? e.message : String(e));
  } finally {
    c.testing = false;
  }
}

// Server-side SqlConnectionConfig is PascalCase C# (System.Text.Json Web defaults match case-insensitively).
// The credential reference is resolved server-side by the plugin action; no literal password crosses the wire.
function toJsonCfg(c: ConnRow) {
  const cfg = rowToCfg(c);
  return {
    provider: cfg.provider,
    server: cfg.server,
    database: cfg.database,
    user: cfg.user,
    credential: cfg.credential,
    trustedConnection: cfg.trusted_connection,
    file: cfg.file,
    description: cfg.description
  };
}

function toJson(): string {
  const out: SqlSettingsBlob = {
    default_connection: defaultConnection.value || undefined,
    default_limit: defaultLimit.value || 10,
    connections: Object.fromEntries(
      connections.filter(c => c.name.trim()).map(c => [c.name.trim(), rowToCfg(c)])
    )
  };
  return JSON.stringify(out);
}

defineExpose({ toJson });
</script>

<style scoped>
/* Uses only the host's CSS variables so the panel follows theme + density. */
.sql-set { display: flex; flex-direction: column; gap: 8px; font-size: var(--fs-sm, 12px); color: var(--text, inherit); }
.muted { color: var(--muted, #888); }
.empty { font-style: italic; }
.row { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
.row.spread { justify-content: space-between; }
.self-start { align-self: flex-start; }
.grow { flex: 1; }
.w-70 { width: 70px; } .w-90 { width: 90px; } .w-120 { width: 120px; } .w-130 { width: 130px; }
.w-140 { width: 140px; } .w-160 { width: 160px; } .w-220 { width: 220px; } .w-400 { width: 400px; }
.conn-card { border: 1px solid var(--border, #444); border-radius: var(--radius, 6px); padding: 8px 10px;
  display: flex; flex-direction: column; gap: 6px; background: var(--panel, transparent); }
.chk { cursor: pointer; }
.chk input { height: auto; }
label { display: flex; gap: 6px; align-items: center; }
input, select {
  height: 24px; padding: 2px 6px; color: var(--text, inherit); background: var(--bg, transparent);
  border: 1px solid var(--border, #444); border-radius: 5px; font-family: inherit; font-size: inherit;
}
button {
  padding: 2px 10px; color: var(--text, inherit); background: var(--panel, transparent);
  border: 1px solid var(--border, #444); border-radius: 5px; cursor: pointer; font-size: inherit;
}
button:hover:not(:disabled) { border-color: var(--muted, #888); }
button:disabled { opacity: .5; cursor: default; }
</style>
