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
      <select v-model="conn.provider" @change="onProviderChange">
        <option v-for="p in KNOWN_PROVIDERS" :key="p.value" :value="p.value">{{ p.label }}</option>
      </select>
    </label>

    <label class="field"><span>Endpoint</span>
      <input v-model="conn.endpoint">
    </label>

    <label class="field"><span>API key</span>
      <input type="password" v-model="conn.apiKey">
    </label>

    <div class="field">
      <span>Model</span>
      <div class="conn-model-wrap">
        <input
          ref="modelInputEl"
          v-model="conn.model"
          :readonly="conn.lockModel"
          :class="{ locked: conn.lockModel }"
        >
        <button class="btn ghost conn-list-btn" title="Fetch model list" :disabled="fetchingModels" @click="fetchModels">
          {{ fetchingModels ? "…" : "☰" }}
        </button>
      </div>
    </div>

    <div class="field conn-flags">
      <span></span>
      <div class="conn-flags-wrap">
        <label class="flag-check"><input type="checkbox" v-model="conn.lockModel"> Lock model</label>
        <label class="flag-check" v-show="(conn.provider || 'lmstudio') === 'lmstudio'">
          <input type="checkbox" v-model="conn.swapModel"> Hot-swap
        </label>
      </div>
    </div>

    <div class="conn-actions">
      <button class="btn ghost" :disabled="testing" @click="testChat">{{ testing ? "…" : "Test chat" }}</button>
    </div>

    <div v-if="reply !== null" class="conn-test-reply" :data-err="replyIsError ? '1' : undefined">{{ reply }}</div>

    <ModelPickerPopup
      v-if="modelPopup"
      :models="modelPopup.models"
      :anchor="modelPopup.anchor"
      :locked="!!conn.lockModel"
      :swap="!!conn.swapModel && !conn.lockModel"
      :current="conn.model"
      @pick="onPickModel"
      @swap="onSwapModel"
      @close="modelPopup = null"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import type { ConnectionDto, ConnHealth } from "../../protocol/types";
import ModelPickerPopup from "./ModelPickerPopup.vue";

const KNOWN_PROVIDERS = [
  { value: "lmstudio", label: "LM Studio" },
  { value: "vllm", label: "vLLM" },
  { value: "openai", label: "OpenAI" },
  { value: "openai-compat", label: "OpenAI-compat" }
];
const PROVIDER_DEFAULT_EP: Record<string, string> = {
  lmstudio: "http://127.0.0.1:1234/v1",
  openai: "https://api.openai.com/v1"
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

// ── Model fetch + popup ──────────────────────────────────────────────────────
const modelInputEl = ref<HTMLInputElement>();
const fetchingModels = ref(false);
const modelPopup = ref<{ models: string[]; anchor: HTMLElement } | null>(null);

async function fetchModels() {
  fetchingModels.value = true;
  try {
    const result = await client.invoke<{ id: string; models?: string[]; error?: string }>(
      "connection.models",
      { id: requestKey.value, provider: props.conn.provider, endpoint: props.conn.endpoint, apiKey: props.conn.apiKey }
    );
    if (result.error || !result.models?.length) return;
    // anchor to the model input's wrap (the button sits inside it) so the popup can right-align to it
    const anchor = modelInputEl.value?.parentElement as HTMLElement;
    modelPopup.value = { models: result.models, anchor };
  } finally {
    fetchingModels.value = false;
  }
}
function onPickModel(model: string) {
  props.conn.model = model;
  modelPopup.value = null;
}
async function onSwapModel(model: string) {
  modelPopup.value = null;
  const prevModel = props.conn.model;
  props.conn.model = "⏳ swapping…";
  try {
    const result = await client.invoke<{ id: string; model?: string; error?: string }>(
      "connection.swap_model",
      { id: requestKey.value, endpoint: props.conn.endpoint, apiKey: props.conn.apiKey, modelKey: model }
    );
    if (result.error) {
      props.conn.model = prevModel;
      reply.value = "Swap error: " + result.error;
      replyIsError.value = true;
    } else {
      props.conn.model = result.model || "";
    }
  } catch (e) {
    props.conn.model = prevModel;
    reply.value = "Swap error: " + (e instanceof Error ? e.message : String(e));
    replyIsError.value = true;
  }
}

// ── Test chat ────────────────────────────────────────────────────────────────
const testing = ref(false);
const reply = ref<string | null>(null);
const replyIsError = ref(false);

async function testChat() {
  testing.value = true;
  reply.value = null;
  try {
    const result = await client.invoke<{ id: string; reply?: string; error?: string }>(
      "connection.test",
      { id: requestKey.value, provider: props.conn.provider, endpoint: props.conn.endpoint, apiKey: props.conn.apiKey, model: props.conn.model }
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
