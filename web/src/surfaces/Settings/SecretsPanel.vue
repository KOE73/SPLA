<!--
  Global secret store management (the browser half of `spla secret`). The unit is an ENTRY — a named
  record of fields (user+password, a lone token, a PEM private_key…). Only keys and field NAMES ever
  come back from the server — values are write-only from here (typed into a password field, sent,
  never echoed). Three scopes, always stated explicitly — User (mine), Project (travels with the
  project), Shared (administered, ACL-gated). Nothing defaults: where a credential lives is the
  user's decision, and the scope is shown on every row so two same-named entries are never confused.
  Project edits are disabled when no project is open. Plugin configs consume entries by full
  reference: `credential: secret:<scope>:<key>` or `secret:<scope>:<key>#<field>`.

  A row stays one line — key plus the field names it holds — until it is expanded into the shared
  SecretEntryEditor, the same component the credential picker uses to create entries elsewhere.
-->
<template>
  <div class="s-panel" data-tab="secrets">
    <div class="s-head">
      <b>Secrets</b>
      <button class="btn ghost" title="Refresh" @click="reload">↻</button>
      <span class="hint">{{ error || "Values are write-only — never shown or sent back." }}</span>
    </div>

    <p class="expl">
      An entry is a named credential record — e.g. <code>user</code> + <code>password</code> for a host,
      a single <code>token</code> for an API, or a <code>private_key</code> for SSH. Plugin configs never
      hold values, only references: <code>credential: secret:&lt;scope&gt;:&lt;entry&gt;</code> (whole record)
      or <code>secret:&lt;scope&gt;:&lt;entry&gt;#&lt;field&gt;</code> (one field). The scope is part of the
      reference — there is no search and no fallback between scopes.
    </p>

    <section v-for="s in SCOPES" :key="s.id" class="scope" :class="{ disabled: scopeDisabled(s.id) }">
      <div class="scope-head">
        <span class="scope-name">{{ s.label }}</span>
        <span class="scope-sub">{{ s.sub }}</span>
      </div>

      <div v-if="scopeDisabled(s.id)" class="empty">Open a project to store project-scoped secrets.</div>
      <template v-else>
        <div v-for="e in entriesOf(s.id)" :key="e.key" class="entry">
          <div class="e-row">
            <code class="e-key">{{ e.key }}</code>
            <span v-for="f in e.fields" :key="f" class="chip" :title="`${e.reference}#${f}`">
              {{ f }}
              <button class="chip-btn" :title="`Copy '${e.reference}#${f}'`" @click="copy(`${e.reference}#${f}`)">⧉</button>
            </span>
            <span class="grow"></span>
            <button class="btn ghost tiny" :title="`Copy 'credential: ${e.reference}'`" @click="copy(`credential: ${e.reference}`)">⧉ ref</button>
            <button class="btn ghost tiny caret" v-if="e.canManage" :class="{ on: isOpen(s.id, e.key) }"
                    title="Edit fields" @click="toggle(s.id, e.key)">{{ isOpen(s.id, e.key) ? "▾" : "▸" }}</button>
            <button class="btn ghost del" v-if="e.canManage" title="Delete entry" @click="del(s.id, e.key)">🗑</button>
            <span v-else class="chip ro" title="You may use this credential but not change it">read-only</span>
          </div>
          <SecretEntryEditor v-if="isOpen(s.id, e.key)" mode="edit" :scope="s.id"
                             :entry-key="e.key" :fields="e.fields" />
        </div>
        <div v-if="!entriesOf(s.id).length" class="empty">No secrets in this scope.</div>

        <div class="new">
          <button v-if="adding !== s.id" class="btn ghost" @click="adding = s.id">＋ New entry</button>
          <SecretEntryEditor v-else mode="create" :scope="s.id"
                             @created="adding = ''" @cancel="adding = ''" />
        </div>
      </template>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import SecretEntryEditor from "../../secrets/SecretEntryEditor.vue";
import { SCOPES, deleteSecret, entriesOf, loadSecrets, scopeDisabled } from "../../secrets/store";
import { client } from "../../protocol/SplaClient";
import type { SecretScopeId } from "../../protocol/types";

const error = ref("");

/** Which entry is expanded ("scope:key"), one at a time — rows stay one-line otherwise. */
const openEntry = ref("");
/** Which scope has its "new entry" editor open. */
const adding = ref<SecretScopeId | "">("");

function isOpen(scope: SecretScopeId, key: string) { return openEntry.value === `${scope}:${key}`; }
function toggle(scope: SecretScopeId, key: string) {
  openEntry.value = isOpen(scope, key) ? "" : `${scope}:${key}`;
}

async function run(op: Promise<void>) {
  error.value = "";
  try { await op; } catch (e) { error.value = e instanceof Error ? e.message : String(e); }
}

const reload = () => run(loadSecrets());
const del = (scope: SecretScopeId, key: string) => run(deleteSecret(scope, key));

function copy(text: string) { navigator.clipboard?.writeText(text).catch(() => {}); }

client.on("welcome", reload);
reload();
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

.tiny { font-size: var(--fs-xs); padding: 0 5px; color: var(--muted); }
.tiny:hover, .tiny.on { color: var(--text); }
.del { color: var(--muted); }
.del:hover { color: var(--danger, #f85149); }
.empty { color: var(--muted); font-size: var(--fs-sm); padding: 3px 0; }
.new { margin-top: 2px; }
</style>
