<!--
  One connection, as a collapsible ListCard — the same row shape as every other list in Settings.

  Shut, the head has to answer "which one is this and is it alive?" on its own: the health dot, the
  name, the provider, and a summary of where it points. Open, the head keeps only the dot and the
  name — provider, endpoint and everything else are fields right below, and repeating them in the
  head would just be the same text twice.

  Nothing about the item's identity sits in a strip of its own any more: the name, the id and the
  scope are ordinary labelled fields in the body, and deleting is the list's one delete affordance
  in #actions. The open/shut state belongs to the panel (it knows which connection was just added),
  so this card takes `open` and asks for the change instead of holding it.
-->
<template>
  <ListCard :open="open" @update:open="$emit('update:open', $event)"
            :summary="summary">
    <template #title>
      <!-- Live reachability, checked by the panel. It stays in the head open or shut: no field below
           carries it, and it is the one thing about a connection that changes without being edited. -->
      <span class="conn-status" :class="healthClass" :title="healthTitle"></span>
      <span class="conn-name">{{ headName }}</span>
      <!-- Two layers declaring one id is legal, and the loser still lives in its own file. Saying so
           here is what keeps the panel honest: without it the same name appears twice with no way to
           tell which one a chat actually opens on. -->
      <span v-if="conn.shadowed" class="conn-shadowed" :title="shadowedTitle">{{ t('shadowed') }}</span>
      <!-- The one thing worth seeing without opening eight model rows: not just THAT this connection
           has the default, but WHICH model on it — a connection can carry several. -->
      <span v-if="defaultModel" class="conn-default" :title="defaultModelTitle">
        ★ {{ defaultModel.name || defaultModel.model || defaultModel.id }}
      </span>
      <span v-if="!open" class="conn-provider">{{ providerLabel }}</span>
    </template>

    <template #actions>
      <DeleteButton :title="t('Delete this connection')" @click="$emit('remove')" />
    </template>

    <template #body>
      <label class="field"><span>{{ t('Name') }}</span>
        <input v-model="conn.name" :placeholder="conn.id">
      </label>

      <!-- The id is what chats and model ids are written against, so it is worth seeing; it is not
           typed here, though — a new connection gets it from the name when it is saved. -->
      <div class="field"><span>{{ t('Id') }}</span>
        <span class="conn-id-value">{{ conn.id || t('taken from the name when saved') }}</span>
      </div>

      <!-- Where this connection lives. Changing it moves the entry between files on save — it is not
           a copy, so the old file loses it. The card jumps to the matching section as you pick, which
           is the whole feedback: you can see where it will end up before you save. -->
      <label class="field"><span>{{ t('Lives in') }}</span>
        <select v-model="scope" :title="t('Which file this connection lives in')">
          <option value="shared">{{ t('shared') }}</option>
          <option value="user">{{ t('mine') }}</option>
          <option value="project">{{ t('this project') }}</option>
        </select>
      </label>

      <label class="field"><span>{{ t('Provider') }}</span>
        <!-- Fixed once saved: the credential and every provider-specific field below belong to this
             provider, and there is nothing sensible to carry across. Change it by recreating. -->
        <select v-model="conn.provider" :disabled="!!conn.id" @change="onProviderChange">
          <option v-for="p in KNOWN_PROVIDERS" :key="p.value" :value="p.value">{{ p.label }}</option>
        </select>
      </label>

      <label class="field"><span>{{ t('Endpoint') }}</span>
        <input v-model="conn.endpoint">
      </label>

      <!-- Both keys are picked from the secret store, never typed here: what this card holds is a
           reference, and the credential itself goes browser→store→server without passing through the
           connection editor at all.

           Each is its OWN entry of the plainest shape there is — one `token` field — so a bare
           reference resolves without naming a field. An api key and a management key are two
           credentials that happen to belong to one account, not one credential with two halves;
           keeping them apart is what lets every consumer read a reference the same way. -->
      <div class="field"><span>{{ t('API key') }}</span>
        <div class="cred-cell">
          <CredentialField
            :model-value="conn.apiKey || ''"
            :none-label="t('(none)')"
            create-field="token"
            @update:model-value="setCredential('apiKey', $event)"
          />
          <p v-if="conn.apiKeyIsLiteral" class="cred-literal">
            {{ t('A plaintext key is stored in {file}. Pick or create a secret above to replace it.', { file: scopeFile }) }}
          </p>
          <p v-if="strandedSecret(conn.apiKey)" class="cred-literal">
            <span v-html="t('This key points at a <b>project</b> secret, but the connection lives outside the project — it will not resolve in any other project. Move the secret to your own store.')"></span>
          </p>
        </div>
      </div>

      <div class="field"><span>{{ t('Admin key') }}</span>
        <div class="cred-cell">
          <CredentialField
            :model-value="conn.adminKey || ''"
            :none-label="t('(none) — account balance / usage only')"
            create-field="token"
            @update:model-value="setCredential('adminKey', $event)"
          />
          <p v-if="conn.adminKeyIsLiteral" class="cred-literal">
            {{ t('A plaintext key is stored in {file}. Pick or create a secret above to replace it.', { file: scopeFile }) }}
          </p>
          <p v-if="strandedSecret(conn.adminKey)" class="cred-literal">
            <span v-html="t('This key points at a <b>project</b> secret, but the connection lives outside the project — it will not resolve in any other project. Move the secret to your own store.')"></span>
          </p>
        </div>
      </div>

      <div class="field conn-flags" v-show="(conn.provider || 'lmstudio') === 'lmstudio'">
        <span></span>
        <div class="conn-flags-wrap">
          <label class="flag-check"><input type="checkbox" v-model="conn.swapModel"> {{ t('Hot-swap') }}</label>
        </div>
      </div>

      <!-- ── Rate limits ──────────────────────────────────────────────────────
           On the connection and not on a model because the provider counts against the key: two
           models under one credential share one budget. Folded away by default — the defaults are
           right until a provider says otherwise. -->
      <details class="conn-limits">
        <summary>{{ t('Rate limits') }}</summary>
        <label class="field"><span>{{ t('Min interval') }}</span>
          <input type="number" step="0.5" min="0" v-model.number="conn.minRequestInterval"
                 :placeholder="t('0 — no pacing')">
        </label>
        <p class="conn-limits-hint">
          {{ t('Seconds held between requests on this key, across every chat. A provider allowing 20 requests a minute needs 3.') }}
        </p>
        <template v-if="conn.retry">
          <label class="field"><span>{{ t('Attempts') }}</span>
            <input type="number" min="1" v-model.number="conn.retry.attempts">
          </label>
          <label class="field"><span>{{ t('First pause') }}</span>
            <input type="number" step="0.5" min="0" v-model.number="conn.retry.minDelay">
          </label>
          <label class="field"><span>{{ t('Growth') }}</span>
            <input type="number" step="0.5" min="1" v-model.number="conn.retry.step">
          </label>
          <label class="field"><span>{{ t('Longest pause') }}</span>
            <input type="number" step="1" min="0" v-model.number="conn.retry.maxDelay">
          </label>
          <label class="field"><span>{{ t('Total wait') }}</span>
            <input type="number" step="10" min="0" v-model.number="conn.retry.total">
          </label>
          <p class="conn-limits-hint">
            {{ t('Seconds. These bound our guess at when the provider will answer again — a delay it states itself is obeyed as given and ignores them.') }}
          </p>
        </template>
      </details>

      <!-- ── Models ───────────────────────────────────────────────────────────── -->
      <div class="conn-models-head">{{ t('Models') }}</div>
      <!-- The flat variant of the shared list: a connection with eight models has to read as a list,
           not as eight stacked cards, and "flat" is exactly that list drawn with separator lines.
           Each row is the same card as its parent, one scale down: shut it is the model's name and
           the wire string it resolves to; open, every field including the name. -->
      <ListPanel class="conn-models flat" :empty="!conn.models.length"
                 :empty-text="t('No models yet — add one below, then pick the model string from the provider (☰).')"
                 :add-label="t('Model')" @add="addModel()">
        <ListCard v-for="(m, i) in conn.models" :key="m.clientId || m.id"
                  :open="expanded === keyOf(m)" @update:open="toggle(m)"
                  :title="modelTitle(m)" :summary="m.model || '—'">
          <template #actions>
            <DeleteButton :title="t('Delete this model')" @click="conn.models.splice(i, 1)" />
          </template>

          <template #body>
            <label class="field"><span>{{ t('Name') }}</span>
              <input v-model="m.name" :placeholder="m.model || m.id">
            </label>
            <label class="field"><span>{{ t('Id') }}</span>
              <input v-model="m.id" :placeholder="suggestedId(m)">
            </label>
            <label class="field"><span>{{ t('Model') }}</span>
              <span class="conn-model-wrap">
                <input v-model="m.model" :placeholder="t('provider\'s model string')">
                <button
                  class="btn ghost conn-list-btn"
                  :title="t('Pick from the provider\'s catalog')"
                  :disabled="fetchingModels"
                  @click.prevent="fetchModels($event, m)"
                >{{ fetchingModels ? "…" : "☰" }}</button>
              </span>
            </label>
            <label class="field"><span>{{ t('Context') }}</span>
              <input type="number" v-model.number="m.contextLength" :placeholder="t('auto-detect')">
            </label>
            <label class="field"><span>{{ t('Temperature') }}</span>
              <input type="number" step="0.1" min="0" max="2" v-model.number="m.temperature" :placeholder="t('project default')">
            </label>
            <!-- One per scope, not one per connection: the resolver refuses a layer with two marks,
                 so clearing the others is the panel's job, not this card's — it only reports the ask. -->
            <div class="field conn-flags">
              <span></span>
              <div class="conn-flags-wrap">
                <label class="flag-check">
                  <input type="checkbox" :checked="!!m.default"
                         @change="$emit('set-default', m, ($event.target as HTMLInputElement).checked)">
                  {{ t('Default for new chats') }}
                </label>
              </div>
            </div>
            <div class="conn-actions">
              <button class="btn ghost" :disabled="testing" @click="testChat(m)">
                {{ testing ? "…" : t('Test chat') }}
              </button>
            </div>
            <div v-if="reply !== null" class="conn-test-reply" :data-err="replyIsError ? '1' : undefined">{{ reply }}</div>
          </template>
        </ListCard>
      </ListPanel>

      <!-- Inside the body on purpose: it is opened from a model row (position:fixed, so where it
           sits in the tree costs nothing), and folding the card away takes the popup with it. -->
      <ModelPickerPopup
        v-if="modelPopup"
        :models="modelPopup.models"
        :anchor="modelPopup.anchor"
        :locked="false"
        :swap="!!conn.swapModel"
        :current="modelPopup.target.model || ''"
        @pick="onPickModel"
        @swap="onSwapModel"
        @close="modelPopup = null"
      />
    </template>
  </ListCard>
</template>

<script setup lang="ts">
import { t } from "../../i18n";
import { computed, ref } from "vue";
import DeleteButton from "../../components/buttons/DeleteButton.vue";
import ListPanel from "../../components/list/ListPanel.vue";
import ListCard from "../../components/list/ListCard.vue";
import { client } from "../../protocol/SplaClient";
import type { ConnectionDto, ConnHealth, ModelEntryDto } from "../../protocol/types";
import ModelPickerPopup from "./ModelPickerPopup.vue";
import CredentialField from "../../secrets/CredentialField.vue";
import { uuid } from "../../util/uuid";

const KNOWN_PROVIDERS = [
  { value: "lmstudio", label: "LM Studio" },
  { value: "vllm", label: "vLLM" },
  { value: "openai", label: "OpenAI" },
  { value: "openrouter", label: "OpenRouter" },
  { value: "openai-compat", label: "OpenAI-compat" },
  { value: "localai", label: "LocalAI" }
];
const PROVIDER_DEFAULT_EP: Record<string, string> = {
  lmstudio: "http://127.0.0.1:1234/v1",
  openai: "https://api.openai.com/v1",
  openrouter: "https://openrouter.ai/api/v1"
};

const props = withDefaults(defineProps<{ conn: ConnectionDto; health?: ConnHealth; open?: boolean }>(),
  { open: false });
defineEmits<{
  remove: [];
  "update:open": [boolean];
  /** The model this card wants marked (or unmarked) as the layer's default. The card never writes the
   *  flag itself: uniqueness spans every connection in the scope, and only the panel sees all of them. */
  "set-default": [ModelEntryDto, boolean];
}>();

const requestKey = computed(() => props.conn.id || props.conn.clientId || "");

// ── The head ─────────────────────────────────────────────────────────────────
// A connection is named by its name, falls back to its id, and a brand-new one has neither yet.
const headName = computed(() => props.conn.name || props.conn.id || t("(new connection)"));

const shadowedTitle = computed(() =>
  t('A later layer declares "{id}" too, so chats use that one. This entry still lives in this file — edit or delete it here.',
    { id: props.conn.id }));

// At most one per connection (the panel enforces at most one per SCOPE, which spans several
// connections, so a shut connection still names one model here at most).
const defaultModel = computed(() => props.conn.models.find(m => m.default));

const defaultModelTitle = computed(() => t('Default for new chats'));

const providerLabel = computed(() => {
  const value = props.conn.provider || "lmstudio";
  return KNOWN_PROVIDERS.find(p => p.value === value)?.label || value;
});

/** Shut, the card still has to say where this connection points and how much is configured on it. */
const summary = computed(() => {
  const where = props.conn.endpoint || t("no endpoint");
  const n = props.conn.models.length;
  if (!n) return where;
  return `${where} · ${n === 1 ? t("1 model") : t("{n} models", { n })}`;
});

// The mark rides in the shut row's title: which model a new chat opens on is the one thing here worth
// seeing without opening eight rows to find it.
const modelTitle = (m: ModelEntryDto) =>
  (m.default ? "★ " : "") + (m.name || m.model || m.id || t("(new model)"));

// ── Scope: which file this connection lives in ───────────────────────────────
// An entry that never said counts as project — the layer everything was in before scopes existed,
// and the same fallback the server applies when a client says nothing.
const scope = computed({
  get: () => props.conn.scope || "project",
  set: (v: string) => { props.conn.scope = v; }
});

const scopeFile = computed(() => scope.value === "project"
  ? t("this project's .spla")
  : scope.value === "user" ? t("your own connections.yaml") : t("the shared connections file"));

/** A connection outside the project pointing at a project secret resolves in exactly one project —
 *  which defeats the reason it was put in a shared layer. Worth saying at the moment the scope is
 *  visible; it is a warning, not a refusal (the reference may well be deliberate for now). */
function strandedSecret(reference?: string): boolean {
  return scope.value !== "project" && (reference || "").startsWith("secret:project:");
}

const healthClass = computed(() => {
  const h = props.health;
  if (!h || h.ok == null) return "";
  return h.ok ? "ok" : "err";
});
const healthTitle = computed(() => {
  const h = props.health;
  if (!h || h.ok == null) return t("Not checked yet");
  return h.ok ? t("Reachable") : (h.error || t("Unreachable"));
});

/** Sets one credential reference and drops the "untouched literal" marker with it: the server reads
 *  that marker as "the editor never saw this key, keep it", so leaving it set here would silently
 *  discard the reference the user just chose — including the choice to have none. */
function setCredential(which: "apiKey" | "adminKey", reference: string) {
  props.conn[which] = reference;
  props.conn[which === "apiKey" ? "apiKeyIsLiteral" : "adminKeyIsLiteral"] = false;
}

function onProviderChange() {
  const def = PROVIDER_DEFAULT_EP[props.conn.provider || ""];
  const currentIsDefault = Object.values(PROVIDER_DEFAULT_EP).some(d => d === (props.conn.endpoint || "").replace(/\/$/, ""));
  if (def && (!props.conn.endpoint || currentIsDefault)) props.conn.endpoint = def;
}

// ── Model rows ───────────────────────────────────────────────────────────────
// Collapsed by default and one open at a time: a connection with eight models must stay a list, not
// eight stacked cards.
const expanded = ref<string | null>(null);
const keyOf = (m: ModelEntryDto) => m.clientId || m.id;
function toggle(m: ModelEntryDto) {
  expanded.value = expanded.value === keyOf(m) ? null : keyOf(m);
  reply.value = null;
}

/** Readable default id, prefixed by the connection: two connections often carry the same model, and
 *  ids are global — a bare "opus" under both would be refused on save. Based on the model string
 *  first — that is what actually distinguishes one row from another — falling back to the name only
 *  when no model has been picked yet. */
function suggestedId(m: ModelEntryDto): string {
  const base = (m.model || m.name || "").toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
  const prefix = (props.conn.id || props.conn.name || "").toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
  return prefix && base ? `${prefix}-${base}` : base || prefix;
}

function addModel() {
  // Left blank on purpose: with nothing picked yet the only suggestion is the bare connection id,
  // which names nothing about the model. The placeholder shows it; the real id fills in once a model
  // is chosen (onPickModel/onSwapModel) or, failing that, the server derives one from it on save.
  const m: ModelEntryDto = { id: "", clientId: uuid(), name: "", model: "" };
  props.conn.models.push(m);
  expanded.value = keyOf(m);
}

// ── Catalog fetch + popup ────────────────────────────────────────────────────
const fetchingModels = ref(false);
const modelPopup = ref<{ models: string[]; anchor: HTMLElement; target: ModelEntryDto } | null>(null);

/** The catalog belongs to one model row: it fills that row's `model` field. */
async function fetchModels(e: MouseEvent, target: ModelEntryDto) {
  // Grab the element now: the browser nulls `currentTarget` once the handler returns, so reading it
  // after the await threw and the popup never opened.
  const anchor = e.currentTarget as HTMLElement;
  fetchingModels.value = true;
  reply.value = null;
  try {
    const result = await client.invoke<{ id: string; models?: string[]; error?: string }>(
      "connection.models",
      { id: requestKey.value, provider: props.conn.provider, endpoint: props.conn.endpoint, apiKey: props.conn.apiKey }
    );
    if (result.error || !result.models?.length) {
      replyIsError.value = true;
      reply.value = result.error || "provider returned no models";
      return;
    }
    modelPopup.value = { models: result.models, anchor, target };
  } catch (err) {
    replyIsError.value = true;
    reply.value = err instanceof Error ? err.message : String(err);
  } finally {
    fetchingModels.value = false;
  }
}

function onPickModel(model: string) {
  const target = modelPopup.value?.target;
  modelPopup.value = null;
  if (!target) return;
  target.model = model;
  if (!target.id) target.id = suggestedId(target);
}

async function onSwapModel(model: string) {
  const target = modelPopup.value?.target;
  modelPopup.value = null;
  try {
    const result = await client.invoke<{ id: string; model?: string; error?: string }>(
      "connection.swap_model",
      { id: requestKey.value, endpoint: props.conn.endpoint, apiKey: props.conn.apiKey, modelKey: model }
    );
    if (result.error) { reply.value = "Swap error: " + result.error; replyIsError.value = true; }
    else if (target) {
      target.model = result.model || model;
      if (!target.id) target.id = suggestedId(target);
    }
  } catch (e) {
    reply.value = "Swap error: " + (e instanceof Error ? e.message : String(e));
    replyIsError.value = true;
  }
}

// ── Test chat (per model — reachability of the connection is the health dot) ──
const testing = ref(false);
const reply = ref<string | null>(null);
const replyIsError = ref(false);

async function testChat(m: ModelEntryDto) {
  testing.value = true;
  reply.value = null;
  try {
    const result = await client.invoke<{ id: string; reply?: string; error?: string }>(
      "connection.test",
      {
        id: requestKey.value,
        provider: props.conn.provider,
        endpoint: props.conn.endpoint,
        apiKey: props.conn.apiKey,
        model: m.model
      }
    );
    replyIsError.value = !!result.error;
    reply.value = result.error || result.reply || "(empty response)";
  } catch (e) {
    replyIsError.value = true;
    reply.value = e instanceof Error ? e.message : String(e);
  } finally {
    testing.value = false;
  }
}
</script>

<style scoped>
/* Only what is specific to a connection's head lives here — the caret, the title row, the actions
   and the body are .list-card* in app.css, and the card's chrome is the list's three variables. */
.conn-name { min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
/* The provider rides along with the name while shut, so it is a qualifier, not a second title. */
.conn-provider { font-weight: 400; color: var(--muted); font-size: var(--fs-sm); white-space: nowrap; }
/* A note, not a warning: the entry is fine, it just is not the one in force. */
.conn-shadowed { font-weight: 400; color: var(--muted); font-size: var(--fs-xs); white-space: nowrap;
  border: 1px solid var(--line); border-radius: 3px; padding: 0 4px; opacity: .85; }
/* Positive, not a warning — same star as the model row and the chat picker, so the mark reads as one
   thing wherever it turns up. */
.conn-default { font-weight: 400; color: var(--accent); font-size: var(--fs-sm); white-space: nowrap;
  overflow: hidden; text-overflow: ellipsis; }
/* The id is shown, not edited: it reads as the key it is, in the same mono the old id bar used. */
.conn-id-value { font-family: var(--mono); font-size: var(--fs-xs); color: var(--accent);
  min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
</style>
