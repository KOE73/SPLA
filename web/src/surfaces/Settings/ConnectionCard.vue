<template>
  <div class="conn-card">
    <div class="conn-id-bar">{{ conn.id || "(new)" }}</div>

    <div class="conn-name-row">
      <span>Name</span>
      <input v-model="conn.name" :placeholder="conn.id">
      <span class="conn-status" :class="healthClass" :title="healthTitle"></span>
      <button class="x" title="Remove" @click="$emit('remove')">✕</button>
    </div>

    <label class="field"><span>Provider</span>
      <!-- Fixed once saved: the credential and every provider-specific field below belong to this
           provider, and there is nothing sensible to carry across. Change it by recreating. -->
      <select v-model="conn.provider" :disabled="!!conn.id" @change="onProviderChange">
        <option v-for="p in KNOWN_PROVIDERS" :key="p.value" :value="p.value">{{ p.label }}</option>
      </select>
    </label>

    <label class="field"><span>Endpoint</span>
      <input v-model="conn.endpoint">
    </label>

    <label class="field"><span>API key</span>
      <input type="password" v-model="conn.apiKey">
    </label>

    <label class="field"><span>Admin key</span>
      <input
        type="password"
        v-model="conn.adminKey"
        placeholder="optional — account balance / usage only"
      >
    </label>

    <div class="field conn-flags" v-show="(conn.provider || 'lmstudio') === 'lmstudio'">
      <span></span>
      <div class="conn-flags-wrap">
        <label class="flag-check"><input type="checkbox" v-model="conn.swapModel"> Hot-swap</label>
      </div>
    </div>

    <!-- ── Models ───────────────────────────────────────────────────────────── -->
    <div class="conn-models">
      <div class="conn-models-head">
        <span>Models</span>
        <button class="btn ghost conn-list-btn" title="Fetch model list" :disabled="fetchingModels" @click="fetchModels">
          {{ fetchingModels ? "…" : "☰" }}
        </button>
        <button class="btn ghost" title="Add an empty model row" @click="addModel()">+</button>
      </div>

      <div v-if="!conn.models.length" class="conn-models-empty">
        No models yet — pick from the provider (☰) or add one by hand.
      </div>

      <div v-for="(m, i) in conn.models" :key="m.clientId || m.id" class="conn-model-row">
        <div class="conn-model-line" @click="toggle(m)">
          <button class="conn-model-caret">{{ expanded === keyOf(m) ? "▾" : "▸" }}</button>
          <input class="conn-model-name" v-model="m.name" :placeholder="m.model || m.id" @click.stop>
          <span class="conn-model-wire">{{ m.model || "—" }}</span>
          <button class="x" title="Remove" @click.stop="conn.models.splice(i, 1)">✕</button>
        </div>

        <div v-if="expanded === keyOf(m)" class="conn-model-detail">
          <label class="field"><span>Id</span>
            <input v-model="m.id" :placeholder="suggestedId(m)">
          </label>
          <label class="field"><span>Model</span>
            <input v-model="m.model" placeholder="provider's model string">
          </label>
          <label class="field"><span>Context</span>
            <input type="number" v-model.number="m.contextLength" placeholder="auto-detect">
          </label>
          <div class="conn-actions">
            <button class="btn ghost" :disabled="testing" @click="testChat(m)">
              {{ testing ? "…" : "Test chat" }}
            </button>
          </div>
          <div v-if="reply !== null" class="conn-test-reply" :data-err="replyIsError ? '1' : undefined">{{ reply }}</div>
        </div>
      </div>
    </div>

    <ModelPickerPopup
      v-if="modelPopup"
      :models="modelPopup.models"
      :anchor="modelPopup.anchor"
      :locked="false"
      :swap="!!conn.swapModel"
      :current="''"
      @pick="onPickModel"
      @swap="onSwapModel"
      @close="modelPopup = null"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import type { ConnectionDto, ConnHealth, ModelEntryDto } from "../../protocol/types";
import ModelPickerPopup from "./ModelPickerPopup.vue";
import { uuid } from "../../util/uuid";

const KNOWN_PROVIDERS = [
  { value: "lmstudio", label: "LM Studio" },
  { value: "vllm", label: "vLLM" },
  { value: "openai", label: "OpenAI" },
  { value: "openrouter", label: "OpenRouter" },
  { value: "openai-compat", label: "OpenAI-compat" }
];
const PROVIDER_DEFAULT_EP: Record<string, string> = {
  lmstudio: "http://127.0.0.1:1234/v1",
  openai: "https://api.openai.com/v1",
  openrouter: "https://openrouter.ai/api/v1"
};

const props = defineProps<{ conn: ConnectionDto; health?: ConnHealth }>();
defineEmits<{ remove: [] }>();

const requestKey = computed(() => props.conn.id || props.conn.clientId || "");

const healthClass = computed(() => {
  const h = props.health;
  if (!h || h.ok == null) return "";
  return h.ok ? "ok" : "err";
});
const healthTitle = computed(() => {
  const h = props.health;
  if (!h || h.ok == null) return "Not checked yet";
  return h.ok ? "Reachable" : (h.error || "Unreachable");
});

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
 *  ids are global — a bare "opus" under both would be refused on save. */
function suggestedId(m: ModelEntryDto): string {
  const base = (m.name || m.model || "").toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
  const prefix = (props.conn.id || props.conn.name || "").toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
  return prefix && base ? `${prefix}-${base}` : base || prefix;
}

function addModel(model = "") {
  const m: ModelEntryDto = { id: "", clientId: uuid(), name: "", model };
  m.id = suggestedId(m);
  props.conn.models.push(m);
  expanded.value = keyOf(m);
}

// ── Catalog fetch + popup ────────────────────────────────────────────────────
const fetchingModels = ref(false);
const modelPopup = ref<{ models: string[]; anchor: HTMLElement } | null>(null);

async function fetchModels(e: MouseEvent) {
  fetchingModels.value = true;
  try {
    const result = await client.invoke<{ id: string; models?: string[]; error?: string }>(
      "connection.models",
      { id: requestKey.value, provider: props.conn.provider, endpoint: props.conn.endpoint, apiKey: props.conn.apiKey }
    );
    if (result.error || !result.models?.length) return;
    modelPopup.value = { models: result.models, anchor: (e.currentTarget as HTMLElement).parentElement as HTMLElement };
  } finally {
    fetchingModels.value = false;
  }
}

/** Picking from the catalog ADDS a row rather than overwriting one — under the tree a connection
 *  holds several models, so "pick" means "select another", not "replace the one". */
function onPickModel(model: string) {
  modelPopup.value = null;
  if (props.conn.models.some(m => m.model === model)) return;
  addModel(model);
}

async function onSwapModel(model: string) {
  modelPopup.value = null;
  try {
    const result = await client.invoke<{ id: string; model?: string; error?: string }>(
      "connection.swap_model",
      { id: requestKey.value, endpoint: props.conn.endpoint, apiKey: props.conn.apiKey, modelKey: model }
    );
    if (result.error) { reply.value = "Swap error: " + result.error; replyIsError.value = true; }
    else addModel(result.model || model);
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
