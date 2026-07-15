<!--
  Global secret store management (the browser half of `spla secret`). The unit is an ENTRY — a named
  record of fields (user+password, a lone token, a PEM private_key…). Only keys and field NAMES ever
  come back from the server — values are write-only from here (typed into a password field, sent,
  never echoed). Two scopes: Machine (~/.spla) shared across projects, Project (<workspace>/.spla)
  travels with the project. Project edits are disabled when no project is open.
  Plugin configs consume entries via `credential: <key>` (whole record) or `secret:<key>#<field>`.
-->
<template>
  <div class="s-panel" data-tab="secrets">
    <div class="s-head">
      <b>Secrets</b>
      <button class="btn ghost" title="Refresh" @click="load">↻</button>
      <span class="hint">{{ error || "Values are write-only — never shown or sent back." }}</span>
    </div>

    <p class="expl">
      An entry is a named credential record — e.g. <code>user</code> + <code>password</code> for a host,
      a single <code>token</code> for an API, or a <code>private_key</code> for SSH. Plugin configs never
      hold values, only references: <code>credential: &lt;entry&gt;</code> (whole record) or
      <code>secret:&lt;entry&gt;#&lt;field&gt;</code> (one field).
    </p>

    <section v-for="s in scopes" :key="s.id" class="scope" :class="{ disabled: s.disabled }">
      <div class="scope-head">
        <span class="scope-name">{{ s.label }}</span>
        <span class="scope-sub">{{ s.sub }}</span>
      </div>

      <div v-if="s.disabled" class="empty">Open a project to store project-scoped secrets.</div>
      <template v-else>
        <div v-for="e in s.entries" :key="e.key" class="entry">
          <div class="e-row">
            <code class="e-key">{{ e.key }}</code>
            <span v-for="f in e.fields" :key="f" class="chip" :title="`secret:${e.key}#${f}`">
              {{ f }}
              <button class="chip-btn" :title="`Copy 'secret:${e.key}#${f}'`" @click="copy(`secret:${e.key}#${f}`)">⧉</button>
              <button class="chip-btn del" title="Delete field" @click="del(s.id, e.key, f)">×</button>
            </span>
            <span class="grow"></span>
            <button class="btn ghost tiny" :title="`Copy 'credential: ${e.key}'`" @click="copy(`credential: ${e.key}`)">⧉ ref</button>
            <button class="btn ghost tiny" :class="{ on: isOpen(s.id, e.key) }" title="Add field" @click="toggle(s.id, e.key)">＋</button>
            <button class="btn ghost del" title="Delete entry" @click="del(s.id, e.key)">×</button>
          </div>
          <form v-if="isOpen(s.id, e.key)" class="add add-field" @submit.prevent="addField(s.id, e.key)">
            <select v-model="s.fieldForm.name">
              <option v-for="n in FIELD_NAMES" :key="n" :value="n">{{ n }}</option>
              <option value="">custom…</option>
            </select>
            <input v-if="s.fieldForm.name === ''" v-model="s.fieldForm.custom" placeholder="field name" spellcheck="false" autocomplete="off" />
            <textarea v-if="isMultiline(s.fieldForm)" v-model="s.fieldForm.value" rows="3"
                      placeholder="-----BEGIN OPENSSH PRIVATE KEY----- …" spellcheck="false" autocomplete="off"></textarea>
            <input v-else v-model="s.fieldForm.value" type="password" placeholder="value" autocomplete="new-password" />
            <button class="btn" type="submit" :disabled="!fieldName(s.fieldForm) || !s.fieldForm.value">+ field</button>
          </form>
        </div>
        <div v-if="!s.entries.length" class="empty">No secrets in this scope.</div>

        <form class="add new-entry" @submit.prevent="addEntry(s.id)">
          <input v-model="s.form.key" placeholder="entry name (e.g. box, openrouter)" spellcheck="false" autocomplete="off" />
          <input v-model="s.form.user" placeholder="user (optional)" spellcheck="false" autocomplete="off" />
          <input v-model="s.form.password" type="password" placeholder="password / token" autocomplete="new-password" />
          <button class="btn" type="submit" :disabled="!s.form.key || !s.form.password">Add entry</button>
        </form>
      </template>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import { projectEnvelope } from "../../state/project";
import type { SecretEntryDto, SecretListResultPayload } from "../../protocol/types";

type ScopeId = "machine" | "project";

/** Conventional field names (mirrors SecretFields in the domain). Free-form via "custom…". */
const FIELD_NAMES = ["password", "user", "token", "private_key", "passphrase"];

const machine = ref<SecretEntryDto[]>([]);
const project = ref<SecretEntryDto[]>([]);
const projectOpen = ref(false);
const error = ref("");

interface EntryForm { key: string; user: string; password: string }
interface FieldForm { name: string; custom: string; value: string }
const forms = reactive<Record<ScopeId, EntryForm>>({
  machine: { key: "", user: "", password: "" },
  project: { key: "", user: "", password: "" },
});
const fieldForms = reactive<Record<ScopeId, FieldForm>>({
  machine: { name: "password", custom: "", value: "" },
  project: { name: "password", custom: "", value: "" },
});

const scopes = computed(() => [
  { id: "machine" as ScopeId, label: "Machine", sub: "~/.spla — shared across all projects", entries: machine.value, disabled: false, form: forms.machine, fieldForm: fieldForms.machine },
  { id: "project" as ScopeId, label: "Project", sub: "<workspace>/.spla — travels with the project", entries: project.value, disabled: !projectOpen.value, form: forms.project, fieldForm: fieldForms.project },
]);

/** Which entry has its add-field form open ("scope:key"), one at a time — rows stay one-line otherwise. */
const openEntry = ref("");
function isOpen(scope: ScopeId, key: string) { return openEntry.value === `${scope}:${key}`; }
function toggle(scope: ScopeId, key: string) {
  openEntry.value = isOpen(scope, key) ? "" : `${scope}:${key}`;
}

function fieldName(f: FieldForm) { return (f.name || f.custom).trim(); }
function isMultiline(f: FieldForm) { return fieldName(f) === "private_key"; }

function apply(p: SecretListResultPayload) {
  machine.value = p.machine || [];
  project.value = p.project || [];
  projectOpen.value = !!p.projectOpen;
  error.value = p.error || "";
}

async function load() {
  try { apply(await client.invoke<SecretListResultPayload>("secret.list", undefined, projectEnvelope())); }
  catch (e) { error.value = String(e); }
}

async function set(scope: ScopeId, key: string, fields: Record<string, string>) {
  try { apply(await client.invoke<SecretListResultPayload>("secret.set", { key, fields, scope }, projectEnvelope())); return true; }
  catch (e) { error.value = String(e); return false; }
}

async function addEntry(scope: ScopeId) {
  const f = forms[scope];
  if (!f.key || !f.password) return;
  const fields: Record<string, string> = { password: f.password };
  if (f.user) fields.user = f.user;
  if (await set(scope, f.key.trim(), fields)) { f.key = ""; f.user = ""; f.password = ""; }
}

async function addField(scope: ScopeId, key: string) {
  const f = fieldForms[scope];
  const name = fieldName(f);
  if (!name || !f.value) return;
  if (await set(scope, key, { [name]: f.value })) { f.custom = ""; f.value = ""; }
}

/** Whole entry when field is omitted; one field otherwise (server drops the entry when empty). */
async function del(scope: ScopeId, key: string, field?: string) {
  try { apply(await client.invoke<SecretListResultPayload>("secret.delete", { key, field, scope }, projectEnvelope())); }
  catch (e) { error.value = String(e); }
}

function copy(text: string) { navigator.clipboard?.writeText(text).catch(() => {}); }

client.on("welcome", load);
load();
</script>

<style scoped>
.expl { margin: 4px 0 12px; color: var(--muted); font-size: var(--fs-sm); line-height: 1.5; max-width: 640px; }
.expl code { font-family: var(--mono); color: var(--text); }
.scope { margin: 10px 0 18px; }
.scope-head { display: flex; align-items: baseline; gap: 8px; margin-bottom: 6px; }
.scope-name { font-weight: 600; color: var(--text); }
.scope-sub { font-size: var(--fs-xs); color: var(--muted); font-family: var(--mono); }

/* One-line entry rows; density vars size the gaps/padding so nano..max scale. */
.entry { margin: 0 0 var(--gap, 8px); padding: 2px 6px; border: 1px solid var(--border);
  border-radius: var(--radius, 7px); background: var(--panel); }
.e-row { display: flex; align-items: center; gap: calc(var(--gap, 8px) * 0.75); min-height: 24px; }
.e-key { font-family: var(--mono); font-size: var(--fs-sm); font-weight: 600; color: var(--text); margin-right: 4px; }
.grow { flex: 1; }

.chip { display: inline-flex; align-items: center; gap: 2px; padding: 0 3px 0 6px;
  font-family: var(--mono); font-size: var(--fs-xs); color: var(--muted);
  border: 1px solid var(--border); border-radius: 999px; background: var(--bg); line-height: 16px; }
.chip-btn { background: none; border: none; padding: 0 2px; cursor: pointer;
  color: transparent; font-size: var(--fs-xs); line-height: 1; }
.chip:hover .chip-btn { color: var(--muted); }
.chip .chip-btn:hover { color: var(--text); }
.chip .chip-btn.del:hover { color: var(--danger, #f85149); }

.tiny { font-size: var(--fs-xs); padding: 0 5px; color: var(--muted); }
.tiny:hover, .tiny.on { color: var(--text); }
.del { color: var(--muted); }
.del:hover { color: var(--danger, #f85149); }
.empty { color: var(--muted); font-size: var(--fs-sm); padding: 3px 0; }

.add { display: flex; gap: 6px; align-items: flex-start; }
.add input, .add select, .add textarea {
  height: 26px; padding: 2px 7px; color: var(--text); background: var(--bg);
  border: 1px solid var(--border); border-radius: 5px; font-family: inherit;
}
.add textarea { height: auto; flex: 1; font-family: var(--mono); font-size: var(--fs-xs); resize: vertical; }
.add-field { margin: 4px 0 4px 14px; }
.add-field select { width: 110px; font-family: var(--mono); }
.add-field input[type=password] { width: 180px; }
.new-entry { margin-top: 2px; }
.new-entry input:first-child { flex: 1; font-family: var(--mono); }
.new-entry input { width: 150px; }
</style>
