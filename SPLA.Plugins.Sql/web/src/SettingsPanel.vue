<template>
  <div style="display:flex;flex-direction:column;gap:8px;font-size:12px">
    <div style="opacity:.7">Named database connections available to the SQL agent. Stored opaquely in the .spla project file.</div>

    <div style="display:flex;gap:16px;align-items:center">
      <label style="display:flex;gap:6px;align-items:center">
        <span style="opacity:.7">Default connection</span>
        <input v-model="defaultConnection" style="width:140px">
      </label>
      <label style="display:flex;gap:6px;align-items:center">
        <span style="opacity:.7">Default row limit</span>
        <input v-model.number="defaultLimit" type="number" min="1" style="width:90px">
      </label>
    </div>

    <button type="button" style="align-self:flex-start" @click="addConnection">+ Add Connection</button>

    <div v-if="!connections.length" style="opacity:.6;font-style:italic">No connections yet. Click "+ Add Connection".</div>

    <div v-for="(c, i) in connections" :key="c.key" style="border:1px solid var(--panel-border,#444);border-radius:4px;padding:10px;display:flex;flex-direction:column;gap:6px">
      <div style="display:flex;justify-content:space-between;align-items:center">
        <div style="display:flex;gap:10px;align-items:center">
          <span style="opacity:.7">Name</span>
          <input v-model="c.name" style="width:140px">
          <span style="opacity:.7">Provider</span>
          <select v-model="c.provider">
            <option value="mssql">mssql</option>
            <option value="postgres">postgres</option>
            <option value="sqlite">sqlite</option>
          </select>
        </div>
        <button type="button" @click="connections.splice(i, 1)">✕ Remove</button>
      </div>

      <div v-if="c.provider !== 'sqlite'" style="display:flex;gap:10px;align-items:center">
        <span style="opacity:.7;width:70px">Server</span>
        <input v-model="c.path" placeholder="sql01 or 192.168.1.10" style="width:220px">
        <span style="opacity:.7;width:70px">Database</span>
        <input v-model="c.database" style="width:160px">
      </div>
      <div v-else style="display:flex;gap:10px;align-items:center">
        <span style="opacity:.7;width:70px">File</span>
        <input v-model="c.path" placeholder="C:\data\mydb.sqlite" style="width:400px">
      </div>

      <div v-if="c.provider !== 'sqlite'" style="display:flex;gap:10px;align-items:center">
        <label v-if="c.provider === 'mssql'" style="display:flex;gap:6px;align-items:center">
          <input type="checkbox" v-model="c.trustedConnection"> Windows Auth (domain)
        </label>
        <template v-if="!c.trustedConnection || c.provider !== 'mssql'">
          <span style="opacity:.7">User</span>
          <input v-model="c.user" style="width:130px">
          <span style="opacity:.7">Password</span>
          <input v-model="c.password" type="password" placeholder="or env:MY_VAR" style="width:130px">
        </template>
      </div>

      <div style="display:flex;gap:10px;align-items:center">
        <span style="opacity:.7;width:70px">Description</span>
        <input v-model="c.description" placeholder="Shown to the AI — what this database contains" style="width:500px">
      </div>

      <div style="display:flex;gap:10px;align-items:center">
        <button type="button" :disabled="c.testing" @click="testConnection(c)">Test Connection</button>
        <span style="opacity:.7">{{ c.testStatus }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { parse, stringify } from "yaml";
import type { MountApi } from "./mount";

const props = defineProps<{ api: MountApi }>();

interface ConnRow {
  key: number;
  name: string;
  provider: string;
  path: string;      // server/host (mssql, postgres) or file (sqlite)
  database: string;
  user: string;
  password: string;
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
    password: cfg.password || "",
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
    password: c.password || undefined,
    trusted_connection: c.provider === "mssql" ? c.trustedConnection : undefined,
    description: c.description || undefined
  };
}

const blob: SqlSettingsBlob = (() => {
  try { return (parse(props.api.getYaml() || "") as SqlSettingsBlob) || {}; }
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
function toJsonCfg(c: ConnRow) {
  const cfg = rowToCfg(c);
  return {
    provider: cfg.provider,
    server: cfg.server,
    database: cfg.database,
    user: cfg.user,
    password: cfg.password,
    trustedConnection: cfg.trusted_connection,
    file: cfg.file,
    description: cfg.description
  };
}

function toYaml(): string {
  const out: SqlSettingsBlob = {
    default_connection: defaultConnection.value || undefined,
    default_limit: defaultLimit.value || 10,
    connections: Object.fromEntries(
      connections.filter(c => c.name.trim()).map(c => [c.name, rowToCfg(c)])
    )
  };
  return stringify(out);
}

defineExpose({ toYaml });
</script>
