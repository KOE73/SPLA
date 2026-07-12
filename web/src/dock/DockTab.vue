<template>
  <div class="spla-dock-tab">
    <span class="title">{{ title }}</span>
    <button v-if="!protectedPanel" title="Close panel" @pointerdown.stop @click.stop="api.close()">×</button>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { DockviewPanelApi } from "dockview-vue";
import { panelCatalog, type PanelKind } from "./panelCatalog";

const props = defineProps<{ title: string; api: DockviewPanelApi }>();
const protectedPanel = computed(() => !!panelCatalog[props.api.id as PanelKind]?.protected);
</script>

<style scoped>
.spla-dock-tab { height: 100%; min-width: 0; display: flex; align-items: center; gap: 5px; padding: 0 5px 0 8px; color: inherit; }
.title { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
button { width: 18px; height: 18px; padding: 0; border: 0; border-radius: 3px; color: var(--muted); background: transparent; line-height: 1; }
button:hover { color: var(--text); background: var(--accent-soft); }
</style>
