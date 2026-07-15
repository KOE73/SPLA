<!--
  SSH plugin settings UI (built and shipped INSIDE the plugin, mounted by the host at runtime).
  Edits the plugins.ssh.settings blob: named hosts, each binding an address to a CREDENTIAL — a
  reference into the global secret store, never a literal password. The "new credential" inline form
  writes user/password straight to the secret store via secret.set (values never enter this blob or
  the chat); the host config keeps only the entry name.
-->
<template>
  <div class="ssh-set">
    <div class="muted">
      Named SSH hosts available to the agent and the terminal. Passwords/keys live in the secret
      store (Settings → Secrets); a host only references an entry by name.
    </div>

    <div class="row">
      <label><span class="muted">Default host</span>
        <select v-model="defaultHost">
          <option value="">(none)</option>
          <option v-for="h in hosts" :key="h.key" :value="h.name">{{ h.name }}</option>
        </select>
      </label>
      <label><span class="muted">Timeout, s</span>
        <input v-model.number="timeoutSeconds" type="number" min="5" max="120" class="w-70">
      </label>
    </div>

    <button type="button" class="self-start" @click="addHost">+ Add host</button>
    <div v-if="!hosts.length" class="muted empty">No hosts yet. Click "+ Add host".</div>

    <div v-for="(h, i) in hosts" :key="h.key" class="host-card">
      <div class="row spread">
        <div class="row">
          <span class="muted">Name</span><input v-model="h.name" class="w-120" spellcheck="false">
          <span class="muted">Host</span><input v-model="h.host" placeholder="10.0.0.5 or box.local" class="w-180" spellcheck="false">
          <span class="muted">Port</span><input v-model.number="h.port" type="number" min="1" max="65535" class="w-70">
        </div>
        <button type="button" @click="hosts.splice(i, 1)">✕ Remove</button>
      </div>

      <div class="row">
        <span class="muted w-label">Credential</span>
        <select v-model="h.credential">
          <option value="">(none — use fields below)</option>
          <option v-for="c in credentials" :key="c" :value="c">{{ c }}</option>
        </select>
        <button type="button" @click="h.newCred = !h.newCred">{{ h.newCred ? "cancel" : "new…" }}</button>
        <span class="muted">entry in the secret store: user + password or private_key</span>
      </div>

      <div v-if="h.newCred" class="row new-cred">
        <input v-model="h.credName" placeholder="entry name" class="w-120" spellcheck="false">
        <input v-model="h.credUser" placeholder="user" class="w-120" spellcheck="false">
        <input v-model="h.credPassword" type="password" placeholder="password" class="w-140" autocomplete="new-password">
        <button type="button" :disabled="!h.credName || !h.credPassword" @click="createCredential(h)">Save to store</button>
        <span class="muted">{{ h.credStatus }}</span>
      </div>

      <div class="row">
        <span class="muted w-label">User</span>
        <input v-model="h.user" :placeholder="h.credential ? '(from credential)' : 'login'" class="w-120" spellcheck="false">
        <span class="muted">Key file</span>
        <input v-model="h.keyFile" placeholder="optional: C:\Users\me\.ssh\id_ed25519" class="w-260" spellcheck="false">
      </div>

      <div class="row">
        <span class="muted w-label">Description</span>
        <input v-model="h.description" placeholder="Shown to the AI — what this host is" class="grow">
      </div>

      <div class="row">
        <label class="chk"><input v-model="h.allowWrite" type="checkbox">
          <span>Allow the agent to write (apt, systemctl, edit files…)</span></label>
        <span class="muted">off = read-only guard blocks mutating commands; human terminal is never guarded</span>
      </div>

      <div class="row">
        <button type="button" :disabled="h.testing" @click="testHost(h)">Test connection</button>
        <span class="muted">{{ h.testStatus }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from "vue";
import { parse, stringify } from "yaml";
import type { MountApi } from "./mount";

const props = defineProps<{ api: MountApi }>();

interface HostCfg {
  host?: string; port?: number; user?: string; credential?: string;
  password?: string; key_file?: string; key_passphrase?: string; description?: string;
  allow_write?: boolean;
}
interface SshSettingsBlob {
  default_host?: string;
  timeout_seconds?: number;
  hosts?: Record<string, HostCfg>;
}
interface HostRow {
  key: number; name: string; host: string; port: number; user: string; credential: string;
  password: string; keyFile: string; keyPassphrase: string; description: string; allowWrite: boolean;
  newCred: boolean; credName: string; credUser: string; credPassword: string; credStatus: string;
  testing: boolean; testStatus: string;
}

let nextKey = 0;
function rowFromCfg(name: string, cfg: HostCfg): HostRow {
  return {
    key: nextKey++, name,
    host: cfg.host || "", port: cfg.port || 22, user: cfg.user || "",
    credential: cfg.credential || "",
    password: cfg.password || "", keyFile: cfg.key_file || "", keyPassphrase: cfg.key_passphrase || "",
    description: cfg.description || "", allowWrite: !!cfg.allow_write,
    newCred: false, credName: "", credUser: "", credPassword: "", credStatus: "",
    testing: false, testStatus: ""
  };
}
function rowToCfg(h: HostRow): HostCfg {
  return {
    host: h.host || undefined,
    port: h.port === 22 ? undefined : h.port,
    user: h.user || undefined,
    credential: h.credential || undefined,
    // Legacy single-value pointers stay untouched if they were in the blob and no credential is set.
    password: h.credential ? undefined : (h.password || undefined),
    key_file: h.keyFile || undefined,
    key_passphrase: h.credential ? undefined : (h.keyPassphrase || undefined),
    description: h.description || undefined,
    allow_write: h.allowWrite || undefined
  };
}

const blob: SshSettingsBlob = (() => {
  try { return (parse(props.api.getYaml() || "") as SshSettingsBlob) || {}; }
  catch { return {}; }
})();

const defaultHost = ref(blob.default_host || "");
const timeoutSeconds = ref(blob.timeout_seconds || 20);
const hosts = reactive<HostRow[]>(
  Object.entries(blob.hosts || {}).map(([name, cfg]) => rowFromCfg(name, cfg))
);

/** Secret-store entry names (machine + project) for the credential dropdown. Keys only — the
 * secret.list protocol never returns values. */
const credentials = ref<string[]>([]);
interface SecretEntryDto { key: string; fields: string[] }
interface SecretListResult { machine: SecretEntryDto[]; project: SecretEntryDto[] }
async function loadCredentials() {
  try {
    const r = await props.api.invoke<SecretListResult>("secret.list");
    credentials.value = [...new Set([...(r.machine || []), ...(r.project || [])].map(e => e.key))].sort();
  } catch { /* store unreachable — dropdown stays empty, manual entry still works */ }
}
onMounted(loadCredentials);

function addHost() {
  hosts.push(rowFromCfg(`host${hosts.length + 1}`, {}));
}

/** Writes user+password as a machine-scope secret entry and binds the host to it. */
async function createCredential(h: HostRow) {
  h.credStatus = "Saving…";
  try {
    const fields: Record<string, string> = { password: h.credPassword };
    if (h.credUser) fields.user = h.credUser;
    await props.api.invoke("secret.set", { key: h.credName.trim(), fields, scope: "machine" });
    h.credential = h.credName.trim();
    h.newCred = false; h.credName = ""; h.credUser = ""; h.credPassword = ""; h.credStatus = "";
    await loadCredentials();
  } catch (e) {
    h.credStatus = "Failed: " + (e instanceof Error ? e.message : String(e));
  }
}

async function testHost(h: HostRow) {
  h.testing = true;
  h.testStatus = "Connecting…";
  try {
    const result = await props.api.invoke<{ ok: boolean; resultJson?: string; error?: string }>("plugin.action", {
      pluginId: "ssh",
      action: "testHost",
      valueJson: JSON.stringify({
        host: h.host, port: h.port, user: h.user || undefined,
        credential: h.credential || undefined,
        password: h.credential ? undefined : (h.password || undefined),
        keyFile: h.keyFile || undefined
      })
    });
    if (result.ok && result.resultJson) {
      const r = JSON.parse(result.resultJson) as { ok: boolean; message: string };
      h.testStatus = r.message;
    } else {
      h.testStatus = "Failed: " + (result.error || "unknown error");
    }
  } catch (e) {
    h.testStatus = "Failed: " + (e instanceof Error ? e.message : String(e));
  } finally {
    h.testing = false;
  }
}

function toYaml(): string {
  const out: SshSettingsBlob = {
    default_host: defaultHost.value || undefined,
    timeout_seconds: timeoutSeconds.value || 20,
    hosts: Object.fromEntries(
      hosts.filter(h => h.name.trim()).map(h => [h.name.trim(), rowToCfg(h)])
    )
  };
  return stringify(out);
}

defineExpose({ toYaml });
</script>

<style scoped>
/* Uses only the host's CSS variables so the panel follows theme + density. */
.ssh-set { display: flex; flex-direction: column; gap: 8px; font-size: var(--fs-sm, 12px); color: var(--text, inherit); }
.muted { color: var(--muted, #888); }
.empty { font-style: italic; }
.row { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
.row.spread { justify-content: space-between; }
.self-start { align-self: flex-start; }
.grow { flex: 1; }
.w-label { width: 80px; }
.w-70 { width: 70px; } .w-120 { width: 120px; } .w-140 { width: 140px; } .w-180 { width: 180px; } .w-260 { width: 260px; }
.host-card { border: 1px solid var(--border, #444); border-radius: var(--radius, 6px); padding: 8px 10px;
  display: flex; flex-direction: column; gap: 6px; background: var(--panel, transparent); }
.new-cred { padding: 4px 6px; border: 1px dashed var(--border, #444); border-radius: 5px; }
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
