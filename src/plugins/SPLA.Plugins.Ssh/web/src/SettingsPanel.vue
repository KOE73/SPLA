<!--
  SSH plugin settings UI (built and shipped INSIDE the plugin, mounted by the host at runtime).
  Edits the plugins.ssh.settings blob: named hosts, each binding an address to a CREDENTIAL — a
  reference into the global secret store, never a literal password. Picking or creating that entry is
  the HOST's credential control, borrowed through CredentialSlot — this plugin never speaks the
  secret protocol and never handles a value; the host config keeps only the reference.

  Shape: the list of cards, then "＋ Host" — the same one every Settings list in the host wears.
  That shape is not hand-drawn here; it comes from ./kit (ListPanel/ListCard/glyph buttons), this
  bundle's copy of the host's shared list kit. Nothing below carries a border, a background or a
  radius of its own.
-->
<template>
  <div class="ssh-set">
    <div class="muted">
      {{ t('Named SSH hosts available to the agent and the terminal. Passwords/keys live in the secret store (Settings → Secrets); a host only references an entry by name.') }}
    </div>

    <div class="row">
      <label><span class="muted">{{ t('Default host') }}</span>
        <select v-model="defaultHost">
          <option value="">{{ t('(none)') }}</option>
          <option v-for="h in hosts" :key="h.key" :value="h.name">{{ h.name }}</option>
        </select>
      </label>
      <label><span class="muted">{{ t('Timeout, s') }}</span>
        <input v-model.number="timeoutSeconds" type="number" min="5" max="120" class="w-70">
      </label>
    </div>

    <ListPanel :empty="!hosts.length" :empty-text="t('No hosts yet.')"
               :add-label="t('Host')" @add="addHost">
      <!-- Head: the card draws the caret, the name and the collapsed "where it points" line itself —
           this panel only says what the words are and hangs the trash on the right. -->
      <ListCard v-for="(h, i) in hosts" :key="h.key"
                :open="isOpen(h.key)" :title="h.name || t('(new host)')" :summary="summary(h)"
                @update:open="toggle(h.key)">
        <template #actions>
          <DeleteButton @click="removeHost(i)" />
        </template>

        <template #body>
          <div class="row">
            <span class="muted w-label">{{ t('Name') }}</span><input v-model="h.name" class="w-120" spellcheck="false">
            <span class="muted">{{ t('Host') }}</span><input v-model="h.host" :placeholder="t('10.0.0.5 or box.local')" class="w-180" spellcheck="false">
            <span class="muted">{{ t('Port') }}</span><input v-model.number="h.port" type="number" min="1" max="65535" class="w-70">
          </div>

          <div class="row">
            <span class="muted w-label">{{ t('Credential') }}</span>
            <CredentialSlot :api="api" v-model="h.credential" />
          </div>

          <div class="row">
            <span class="muted w-label">{{ t('User') }}</span>
            <input v-model="h.user" :placeholder="h.credential ? '(from credential)' : 'login'" class="w-120" spellcheck="false">
            <span class="muted">{{ t('Key file') }}</span>
            <input v-model="h.keyFile" :placeholder="t('optional: C:\\Users\\me\\.ssh\\id_ed25519')" class="w-260" spellcheck="false">
          </div>

          <div class="row">
            <span class="muted w-label">{{ t('Description') }}</span>
            <input v-model="h.description" :placeholder="t('Shown to the AI — what this host is')" class="grow">
          </div>

          <div class="row">
            <label class="chk"><input v-model="h.allowWrite" type="checkbox">
              <span>{{ t('Allow the agent to write (apt, systemctl, edit files…)') }}</span></label>
            <span class="muted">{{ t('off = read-only guard blocks mutating commands; human terminal is never guarded') }}</span>
          </div>

          <div class="row">
            <button type="button" :disabled="h.testing" @click="testHost(h)">{{ t('Test connection') }}</button>
            <span class="muted">{{ h.testStatus }}</span>
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
import DeleteButton from "./kit/DeleteButton.vue";
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
  try { return (JSON.parse(props.api.getJson() || "null") as SshSettingsBlob) || {}; }
  catch { return {}; }
})();

const defaultHost = ref(blob.default_host || "");
const timeoutSeconds = ref(blob.timeout_seconds || 20);
const hosts = reactive<HostRow[]>(
  Object.entries(blob.hosts || {}).map(([name, cfg]) => rowFromCfg(name, cfg))
);

// Which cards are expanded. Saved hosts open collapsed — the head says which one this is;
// a host you just added opens, because its fields are the reason you clicked.
const openKeys = reactive(new Set<number>());
const isOpen = (key: number) => openKeys.has(key);
function toggle(key: number) {
  if (!openKeys.delete(key)) openKeys.add(key);
}

/** What a collapsed card shows instead of its fields: where this host points. */
function summary(h: HostRow): string {
  const addr = h.host || "(no address)";
  const withPort = h.port && h.port !== 22 ? `${addr}:${h.port}` : addr;
  return h.user ? `${h.user}@${withPort}` : withPort;
}

function addHost() {
  const row = rowFromCfg(`host${hosts.length + 1}`, {});
  hosts.push(row);
  openKeys.add(row.key);
}

function removeHost(i: number) {
  const [row] = hosts.splice(i, 1);
  if (row) openKeys.delete(row.key);
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

function toJson(): string {
  const out: SshSettingsBlob = {
    default_host: defaultHost.value || undefined,
    timeout_seconds: timeoutSeconds.value || 20,
    hosts: Object.fromEntries(
      hosts.filter(h => h.name.trim()).map(h => [h.name.trim(), rowToCfg(h)])
    )
  };
  return JSON.stringify(out);
}

defineExpose({ toJson });
</script>

<style scoped>
/* Only what is specific to an SSH host lives here. The card (border, background, radius), the
   list spacing, the add button and the glyph buttons are the kit's — see ./kit/kit.css.
   Uses only the host's CSS variables so the panel follows theme + density. */
.ssh-set { display: flex; flex-direction: column; gap: var(--gap, 10px); font-size: var(--fs-sm, 12px); color: var(--text, inherit); }
.muted { color: var(--muted, #888); }
.row { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
.grow { flex: 1; }
.w-label { width: 80px; }
.w-70 { width: 70px; } .w-120 { width: 120px; } .w-180 { width: 180px; } .w-260 { width: 260px; }
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
