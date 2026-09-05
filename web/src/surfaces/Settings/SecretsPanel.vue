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
      <b>{{ t('Secrets') }}</b>
      <RefreshButton :title="t('Refresh')" @click="reload" />
      <span class="hint">{{ error || "Values are write-only — never shown or sent back." }}</span>
    </div>

    <p class="expl" v-html="t('An entry is a named credential record — e.g. <code>user</code> + <code>password</code> for a host, a single <code>token</code> for an API, or a <code>private_key</code> for SSH. Plugin configs never hold values, only references: <code>credential: secret:&amp;lt;scope&amp;gt;:&amp;lt;entry&amp;gt;</code> (whole record) or <code>secret:&amp;lt;scope&amp;gt;:&amp;lt;entry&amp;gt;#&amp;lt;field&amp;gt;</code> (one field). The scope is part of the reference — there is no search and no fallback between scopes.')"></p>

    <section v-for="s in SCOPES" :key="s.id" class="scope" :class="{ disabled: scopeDisabled(s.id) }">
      <div class="scope-head">
        <span class="scope-name">{{ t(s.label) }}</span>
        <span class="scope-sub">{{ t(s.sub) }}</span>
      </div>

      <div v-if="scopeDisabled(s.id)" class="empty">{{ t('Open a project to store project-scoped secrets.') }}</div>
      <template v-else>
        <ListPanel :empty="!entriesOf(s.id).length" :empty-text="t('No secrets in this scope.')"
                   :add-label="t('Secret')" @add="adding = s.id">
          <ListCard v-for="e in entriesOf(s.id)" :key="e.key"
                    :open="isOpen(s.id, e.key)" :no-toggle-on-click="true"
                    @update:open="toggle(s.id, e.key)">
            <template #head>
              <code class="e-key">{{ e.key }}</code>
              <span v-for="f in e.fields" :key="f" class="chip" :title="`${e.reference}#${f}`">
                {{ f }}
                <CopyButton :text="`${e.reference}#${f}`" :title="`Copy '${e.reference}#${f}'`" />
              </span>
              <span class="grow"></span>
              <CopyButton :text="`credential: ${e.reference}`" :label="t('ref')" :title="`Copy 'credential: ${e.reference}'`" />
              <ExpandButton v-if="e.canManage" :open="isOpen(s.id, e.key)" @update:open="toggle(s.id, e.key)" />
              <DeleteButton v-if="e.canManage" :title="t('Delete entry')" @click="del(s.id, e.key)" />
              <span v-else class="chip ro" :title="t('You may use this credential but not change it')">{{ t('read-only') }}</span>
            </template>
            <template v-if="isOpen(s.id, e.key)" #body>
              <SecretEntryEditor mode="edit" :scope="s.id" :entry-key="e.key" :fields="e.fields" />
            </template>
          </ListCard>
        </ListPanel>

        <SecretEntryEditor v-if="adding === s.id" class="new-editor" mode="create" :scope="s.id"
                           @created="adding = ''" @cancel="adding = ''" />
      </template>
    </section>
  </div>
</template>

<script setup lang="ts">
import { t } from "../../i18n";
import { ref } from "vue";
import SecretEntryEditor from "../../secrets/SecretEntryEditor.vue";
import ListPanel from "../../components/list/ListPanel.vue";
import ListCard from "../../components/list/ListCard.vue";
import DeleteButton from "../../components/buttons/DeleteButton.vue";
import RefreshButton from "../../components/buttons/RefreshButton.vue";
import ExpandButton from "../../components/buttons/ExpandButton.vue";
import CopyButton from "../../components/buttons/CopyButton.vue";
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

/* The entry row's head is otherwise just .list-card-head (global) — only the bits specific to a
   secret entry (mono key, the field chips) live here. */
.e-key { font-family: var(--mono); font-size: var(--fs-sm); font-weight: 600; color: var(--text); margin-right: 4px; }
.grow { flex: 1; }

.chip { display: inline-flex; align-items: center; gap: 2px; padding: 0 2px 0 6px;
  font-family: var(--mono); font-size: var(--fs-xs); color: var(--muted);
  border: 1px solid var(--border); border-radius: 999px; background: var(--bg); line-height: 16px; }
.chip .gbtn { min-width: 16px; height: 16px; padding: 0; }

.empty { color: var(--muted); font-size: var(--fs-sm); padding: 3px 0; }
.new-editor { margin-top: 6px; }
</style>
