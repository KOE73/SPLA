<template>
  <div class="file-browser">
    <!-- Header with column labels -->
    <div class="browser-header">
      <span class="col-label">{{ rootLabel }}</span>
      <span class="col-size">Size</span>
      <span class="col-date">Modified</span>
    </div>

    <div class="browser-tree">
      <div v-if="busy && rootNodes.length === 0" class="browser-loading">Loading…</div>
      <BrowserNode
        v-for="node in rootNodes"
        :key="node.ref"
        :node="node"
        :depth="0"
        :selectedRef="selectedRef"
        :getChildren="getChildren"
        :isLoaded="isLoaded"
        @select="$emit('select', $event)"
        @expand="$emit('expand', $event)"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import BrowserNode from "./BrowserNode.vue";
import type { FsNode } from "../../protocol/types";

defineProps<{
  rootLabel: string;
  rootNodes: FsNode[];
  selectedRef: string | null;
  busy: boolean;
  getChildren: (ref: string) => FsNode[];
  isLoaded: (ref: string) => boolean;
}>();

defineEmits<{
  (e: "select", node: FsNode): void;
  (e: "expand", ref: string): void;
}>();
</script>

<style scoped>
.file-browser {
  display: flex;
  flex-direction: column;
  flex: 1;           /* fills .tree-pane whose width is drag-controlled */
  min-width: 0;
  border-right: 1px solid var(--border);
  background: var(--panel);
  min-height: 0;
  overflow: hidden;
}

.browser-header {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 6px 8px;
  border-bottom: 1px solid var(--border);
  font-size: var(--fs-xs);
  font-weight: 600;
  color: var(--muted);
  text-transform: uppercase;
  letter-spacing: .05em;
  flex-shrink: 0;
}
.col-label { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.col-size  { min-width: 50px; text-align: right; }
.col-date  { min-width: 110px; text-align: right; }

.browser-tree {
  flex: 1;
  overflow: auto;
  padding: 4px 0;
}
.browser-loading {
  padding: 8px 12px;
  font-size: var(--fs-sm);
  color: var(--muted);
}
</style>
