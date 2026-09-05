<!--
  SQL plugin settings UI (built and shipped INSIDE the plugin, mounted by the host at runtime).
  Edits the plugins.sql.settings blob: named connections, each binding an address to a CREDENTIAL —
  a reference into the global secret store, never a literal password. Picking or creating that entry
  is the HOST's credential control, borrowed through CredentialSlot (values never enter this
  blob or the chat); the connection config keeps only the entry name. Mirrors the SSH plugin.

  Shape: the list of cards, then "＋ Connection" — the same one every Settings list in the host
  wears. That shape is not hand-drawn here; it comes from ./kit (ListPanel/ListCard/glyph buttons),
  this bundle's copy of the host's shared list kit. Nothing below carries a border, a background or
  a radius of its own.
-->
<template>
  <div class="sql-set">
    <div class="muted">
      {{ t('Named database connections available to the SQL agent. Passwords live in the secret store (Settings → Secrets); a connection only references an entry by name. Stored in the .spla project file.') }}
    </div>

    <div class="row">
      <label><span class="muted">{{ t('Default connection') }}</span>
        <select v-model="defaultConnection">
          <option value="">{{ t('(none)') }}</option>
          <option v-for="c in connections" :key="c.key" :value="c.name">{{ c.name }}</option>
        </select>
      </label>
      <label><span class="muted">{{ t('Default row limit') }}</span>
        <input v-model.number="defaultLimit" type="number" min="1" class="w-90">
      </label>
    </div>

    <ListPanel :empty="!connections.length" :empty-text="t('No connections yet.')"
               :add-label="t('Connection')" @add="addConnection">
      <ListCard v-for="(c, i) in connections" :key="c.key"
                :open="isOpen(c.key)" @update:open="toggle(c.key)">
        <template #head>
          <b class="name">{{ c.name || "(new connection)" }}</b>
          <span class="muted">{{ c.provider }}</span>
          <span v-if="!isOpen(c.key)" class="muted sum">{{ summary(c) }}</span>
          <span class="grow"></span>
          <RemoveButton :label="t('Remove')" @click="removeConnection(i)" />
          <ExpandButton :open="isOpen(c.key)" @update:open="toggle(c.key)" />
        </template>

        <template #body>
          <div class="row">
            <span class="muted w-70">{{ t('Name') }}</span><input v-model="c.name" class="w-140" spellcheck="false">
            <span class="muted">{{ t('Provider') }}</span>
            <select v-model="c.provider">
              <option value="mssql">{{ t('mssql') }}</option>
              <option value="postgres">{{ t('postgres') }}</option>
              <option value="sqlite">{{ t('sqlite') }}</option>
            </select>
          </div>

          <div v-if="c.provider !== 'sqlite'" class="row">
            <span class="muted w-70">{{ t('Server') }}</span>
            <input v-model="c.path" :placeholder="t('sql01 or 192.168.1.10')" class="w-220" spellcheck="false">
            <span class="muted w-70">{{ t('Database') }}</span>
            <input v-model="c.database" class="w-160" spellcheck="false">
          </div>
          <div v-else class="row">
            <span class="muted w-70">{{ t('File') }}</span>
            <input v-model="c.path" :placeholder="t('C:\data\mydb.sqlite')" class="w-400" spellcheck="false">
          </div>

          <template v-if="c.provider !== 'sqlite'">
            <div class="row">
              <label v-if="c.provider === 'mssql'" class="chk"><input type="checkbox" v-model="c.trustedConnection">
                <span>{{ t('Windows Auth (domain)') }}</span></label>
            </div>

            <template v-if="!c.trustedConnection || c.provider !== 'mssql'">
              <div class="row">
                <span class="muted w-70">{{ t('Credential') }}</span>
                <CredentialSlot :api="api" v-model="c.credential" />
              </div>

              <div class="row">
                <span class="muted w-70">{{ t('User') }}</span>
                <input v-model="c.user" :placeholder="c.credential ? '(from credential)' : 'login'" class="w-130" spellcheck="false">
              </div>
            </template>
          </template>

          <div class="row">
            <span class="muted w-70">{{ t('Description') }}</span>
            <input v-model="c.description" :placeholder="t('Shown to the AI — what this database contains')" class="grow">
          </div>

          <div class="row">
            <button type="button" :disabled="c.testing" @click="testConnection(c)">{{ t('Test Connection') }}</button>
            <span class="muted">{{ c.testStatus }}</span>
          </div>
        </template>
      </ListCard>
    </ListPanel>
  </div>
</template>

<script setup lang="ts">
import { t } from "./i18n";
import { reactive, ref } from "vue";
import CredentialSlot from "./CredentialSlot.vue";
import ListPanel from "./kit/ListPanel.vue";
import ListCard from "./kit/ListCard.vue";
import RemoveButton from "./kit/RemoveButton.vue";
import ExpandButton from "./kit/ExpandButton.vue";
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

// Which cards are expanded. Saved connections open collapsed — the head says which one this is;
// a connection you just added opens, because its fields are the reason you clicked.
const openKeys = reactive(new Set<number>());
const isOpen = (key: number) => openKeys.has(key);
function toggle(key: number) {
  if (!openKeys.delete(key)) openKeys.add(key);
}

/** What a collapsed card shows instead of its fields: where this connection points. */
function summary(c: ConnRow): string {
  if (c.provider === "sqlite") return c.path || "(no file)";
  const server = c.path || "(no server)";
  return c.database ? `${server} / ${c.database}` : server;
}

function addConnection() {
  const row = rowFromCfg(`db${connections.length + 1}`, { provider: "mssql" });
  connections.push(row);
  openKeys.add(row.key);
}

function removeConnection(i: number) {
  const [row] = connections.splice(i, 1);
  if (row) openKeys.delete(row.key);
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
/* Only what is specific to a SQL connection lives here. The card (border, background, radius), the
   list spacing, the add button and the glyph buttons are the kit's — see ./kit/kit.css. */
.sql-set { display: flex; flex-direction: column; gap: var(--gap, 10px); font-size: var(--fs-sm, 12px); color: var(--text, inherit); }
.muted { color: var(--muted, #888); }
.row { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
.grow { flex: 1; }
.name { font-size: var(--fs-sm, 12px); }
.sum { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.w-70 { width: 70px; } .w-90 { width: 90px; } .w-130 { width: 130px; }
.w-140 { width: 140px; } .w-160 { width: 160px; } .w-220 { width: 220px; } .w-400 { width: 400px; }
.chk { cursor: pointer; }
.chk input { height: auto; }
label { display: flex; gap: 6px; align-items: center; }
input, select {
  height: 24px; padding: 2px 6px; color: var(--text, inherit); background: var(--bg, transparent);
  border: 1px solid var(--border, #444); border-radius: var(--radius-sm, 5px); font-family: inherit; font-size: inherit;
}
/* :not(.gbtn) — the kit's glyph buttons land in this panel's scope through the card slots, and they
   bring their own look; this rule is for the panel's own plain buttons only. */
button:not(.gbtn) {
  padding: 2px 10px; color: var(--text, inherit); background: var(--panel, transparent);
  border: 1px solid var(--border, #444); border-radius: var(--radius-sm, 5px); cursor: pointer; font-size: inherit;
}
button:not(.gbtn):hover:not(:disabled) { border-color: var(--muted, #888); }
button:not(.gbtn):disabled { opacity: .5; cursor: default; }
</style>
