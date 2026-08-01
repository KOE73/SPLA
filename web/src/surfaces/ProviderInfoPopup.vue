<template>
  <!-- Teleported to body: the status bar sits inside a transformed dockview ancestor, and a
       transform makes position:fixed resolve against that ancestor instead of the viewport — the
       popup then renders far off-screen. Same reason .models-popup is appended to body. -->
  <Teleport to="body">
  <div ref="popEl" class="pi-popup" :style="style" @click.stop>
    <div v-if="loading" class="pi-empty">reading…</div>
    <div v-else-if="info?.error" class="pi-empty pi-err">{{ info.error }}</div>

    <template v-else-if="info">
      <div class="pi-head">
        <b>{{ info.connectionName }}</b>
        <span class="pi-provider">{{ info.provider }}</span>
      </div>

      <div v-for="s in info.sections" :key="s.title" class="pi-section">
        <div class="pi-section-head">
          <span>{{ s.title }}</span>
          <a v-if="s.deepLink" :href="s.deepLink" target="_blank" rel="noopener" title="Open the provider's dashboard">↗</a>
        </div>
        <div v-for="f in s.facts" :key="f.key" class="pi-fact" :data-sev="f.severity">
          <span class="pi-label">{{ f.label }}</span>
          <span class="pi-value">{{ format(f) }}</span>
          <span v-if="f.resetsAt" class="pi-reset" :title="'resets ' + new Date(f.resetsAt).toLocaleString()">
            ⟳ {{ shortTime(f.resetsAt) }}
          </span>
        </div>
        <div v-if="!s.facts.length" class="pi-empty">nothing reported</div>
      </div>
    </template>
  </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref } from "vue";
import { client } from "../protocol/SplaClient";
import { projectEnvelope } from "../state/project";
import type { ProviderFactDto, ProviderInfoResultPayload } from "../protocol/types";

const props = defineProps<{ modelId: string; anchor: HTMLElement }>();
const emit = defineEmits<{ close: [] }>();

const info = ref<ProviderInfoResultPayload | null>(null);
const loading = ref(true);
const popEl = ref<HTMLElement>();
// Reflows once real content replaces the "reading…" placeholder, so width/height used for
// clamping match what's actually on screen instead of the tiny loading state.
const size = ref(0);

const style = computed(() => {
  size.value; // register as a dependency so late measurements re-run this
  const r = props.anchor.getBoundingClientRect();
  const popW = popEl.value?.offsetWidth ?? 380;
  const popH = popEl.value?.offsetHeight ?? 200;
  // Anchored above the status bar, which sits at the bottom — opening downwards would render
  // the popup off-screen. Clamp both edges so it never spills past the viewport.
  const left = Math.min(Math.max(8, r.left), Math.max(8, window.innerWidth - popW - 8));
  const bottom = Math.min(
    Math.max(8, window.innerHeight - r.top + 6),
    window.innerHeight - popH - 8
  );
  return { left: `${left}px`, bottom: `${Math.max(8, bottom)}px` };
});

/** Formatting is driven by `kind`, which is exactly why the generic layer carries it: the UI must
 *  not need to know that "credits.balance" happens to be money. */
function format(f: ProviderFactDto): string {
  const v = f.value;
  switch (f.kind) {
    case "money": return f.unit === "USD" ? `$${v}` : `${v} ${f.unit ?? ""}`.trim();
    case "percent": return `${v}%`;
    case "duration": return `${v}s`;
    case "timestamp": return new Date(v).toLocaleString();
    default: return f.unit ? `${v} ${f.unit}` : v;
  }
}

function shortTime(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
}

async function load() {
  try {
    info.value = await client.invoke<ProviderInfoResultPayload>(
      "provider.info", { modelId: props.modelId }, projectEnvelope());
  } catch (e) {
    info.value = { modelId: props.modelId, connectionId: "", connectionName: "", sections: [],
      error: e instanceof Error ? e.message : String(e) };
  } finally {
    loading.value = false;
    nextTick(() => { size.value++; });
  }
}

function onDocClick() { emit("close"); }
function onKey(e: KeyboardEvent) { if (e.key === "Escape") emit("close"); }
function onResize() { size.value++; }

let ro: ResizeObserver | undefined;

onMounted(() => {
  load();
  document.addEventListener("click", onDocClick);
  document.addEventListener("keydown", onKey);
  window.addEventListener("resize", onResize);
  ro = new ResizeObserver(() => { size.value++; });
  if (popEl.value) ro.observe(popEl.value);
});
onUnmounted(() => {
  document.removeEventListener("click", onDocClick);
  document.removeEventListener("keydown", onKey);
  window.removeEventListener("resize", onResize);
  ro?.disconnect();
});
</script>
