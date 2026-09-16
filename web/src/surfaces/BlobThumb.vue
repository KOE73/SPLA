<!--
  A thumbnail for one image blob in the debug panel. The snapshot lists blobs without their bytes, so
  each row asks for its own picture (`debug.blob.get`) — and only once it scrolls into view: a chat
  with fifty screenshots must not pull fifty pictures to draw a list.
-->
<template>
  <span ref="el" class="blob-thumb" :class="{ failed: !!error }" :title="error || t('Open image')">
    <img v-if="url" :src="url" :alt="handle" @click="$emit('open', handle)">
    <span v-else-if="error" class="bt-note">{{ t('no preview') }}</span>
    <span v-else class="bt-note">…</span>
  </span>
</template>

<script lang="ts">
import { reactive } from "vue";

/** Fetched pictures by "chatId|handle|version". The debug snapshot is re-requested after every tool
 *  result; without this each refresh would fetch every visible picture again. `version` is the row's
 *  own size/kind text, so a named blob overwritten with new content is fetched anew. */
export const blobThumbCache = reactive(new Map<string, { url?: string; error?: string }>());
export const blobCacheKey = (chatId: string, handle: string, version: string) => `${chatId}|${handle}|${version}`;
</script>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from "vue";
import { t } from "../i18n";
import { client } from "../protocol/SplaClient";
import type { DebugBlobResultPayload } from "../protocol/types";

const props = defineProps<{ chatId: string; handle: string; version: string }>();
defineEmits<{ (e: "open", handle: string): void }>();

const el = ref<HTMLElement | null>(null);
const key = computed(() => blobCacheKey(props.chatId, props.handle, props.version));
const url = computed(() => blobThumbCache.get(key.value)?.url);
const error = computed(() => blobThumbCache.get(key.value)?.error);

async function load() {
  const k = key.value;
  if (blobThumbCache.has(k)) return;
  blobThumbCache.set(k, {});
  try {
    const r = await client.invoke<DebugBlobResultPayload>("debug.blob.get", { handle: props.handle },
      { chatId: props.chatId }, 30000);
    blobThumbCache.set(k, r.url ? { url: r.url } : { error: r.error || "no preview" });
  } catch (e) {
    // Not cached as a failure: a dropped socket is not an answer about this blob.
    blobThumbCache.delete(k);
    console.warn("debug.blob.get failed", e);
  }
}

let io: IntersectionObserver | null = null;
onMounted(() => {
  if (typeof IntersectionObserver === "undefined" || !el.value) { load(); return; }
  io = new IntersectionObserver(entries => {
    if (entries.some(e => e.isIntersecting)) { io?.disconnect(); io = null; load(); }
  });
  io.observe(el.value);
});
onUnmounted(() => io?.disconnect());
</script>

<style scoped>
.blob-thumb { flex: 0 0 auto; display: inline-flex; align-items: center; justify-content: center;
  width: 56px; height: 40px; }
.blob-thumb img { max-width: 56px; max-height: 40px; border: 1px solid var(--border);
  border-radius: var(--radius-sm); cursor: zoom-in; display: block; }
.blob-thumb img:hover { border-color: var(--accent); }
.bt-note { color: var(--muted); font-size: var(--fs-xs); }
</style>
