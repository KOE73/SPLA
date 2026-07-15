<template>
  <div class="spla-dock-tab">
    <span class="title">{{ title }}</span>
    <button v-if="!protectedPanel" title="Close panel" @pointerdown.stop @click.stop="api.close()">×</button>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from "vue";
import type { DockviewPanelApi } from "dockview-vue";
import { panelCatalog, type PanelKind } from "./panelCatalog";

// dockview-vue mounts every component (content AND tab renderers) with a single `params` prop whose
// value carries the real handles: { params, api, containerApi, tabLocation }. Reading `api`/`title`
// as top-level props gets undefined — the old shape threw `undefined.id` on every tab re-render.
const props = defineProps<{ params: { api: DockviewPanelApi } }>();
const api = computed(() => props.params.api);
// Label from our own catalog by id — fixed-kind panels (chat, workspace, ...) always use this.
const def = computed(() => panelCatalog[api.value.id as PanelKind]);

// Dynamic panels (ssh:*) aren't in the catalog, so they fall back to dockview's own title — but
// `api.title` is a plain getter, not a Vue ref, so a computed reading it once never notices later
// setTitle() calls (Terminal.vue renames the tab from the host to "SSH: <sessionId>" once the pty
// connects). Track it explicitly via onDidTitleChange or the tab goes blank/stale forever.
const liveTitle = ref(api.value.title ?? "");
const titleSub = api.value.onDidTitleChange(e => { liveTitle.value = e.title ?? ""; });
onBeforeUnmount(() => titleSub.dispose());

const title = computed(() => def.value ? `${def.value.icon} ${def.value.title}` : liveTitle.value);
const protectedPanel = computed(() => !!def.value?.protected);
</script>

<style scoped>
.spla-dock-tab { height: 100%; min-width: 0; display: flex; align-items: center; gap: 5px; padding: 0 5px 0 8px; color: inherit; }
.title { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
button { width: 18px; height: 18px; padding: 0; border: 0; border-radius: 3px; color: var(--muted); background: transparent; line-height: 1; }
button:hover { color: var(--text); background: var(--accent-soft); }
</style>
