<template>
  <div>
    <!-- Row ---------------------------------------------------------------- -->
    <div
      class="node-row"
      :class="{ leaf: node.kind === 'leaf', selected: isSelected }"
      :style="{ paddingLeft: depth * 16 + 8 + 'px' }"
      @click="onClick"
    >
      <!-- folder arrow or file type badge -->
      <span v-if="node.kind === 'folder'" class="node-ic folder-ic">
        {{ expanded ? "▾" : "▸" }}
      </span>
      <span v-else class="node-ic file-badge" :data-ext="fileExt">{{ fileIcon }}</span>

      <span class="node-label">{{ node.label }}</span>
      <span class="node-meta">{{ sizeText }}</span>
      <span class="node-meta date">{{ dateText }}</span>
    </div>

    <!-- Children (lazy-loaded, shown when expanded) ------------------------ -->
    <template v-if="expanded && node.kind === 'folder'">
      <BrowserNode
        v-for="child in children"
        :key="child.ref"
        :node="child"
        :depth="depth + 1"
        :selectedRef="selectedRef"
        :getChildren="getChildren"
        :isLoaded="isLoaded"
        @select="$emit('select', $event)"
        @expand="$emit('expand', $event)"
      />
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import type { FsNode } from "../../protocol/types";

const props = defineProps<{
  node: FsNode;
  depth: number;
  selectedRef: string | null;
  getChildren: (ref: string) => FsNode[];
  isLoaded: (ref: string) => boolean;
}>();

const emit = defineEmits<{
  (e: "select", node: FsNode): void;
  (e: "expand", ref: string): void;
}>();

const expanded = ref(false);

const isSelected = computed(() => props.selectedRef === props.node.ref);
const children   = computed(() => props.getChildren(props.node.ref));

const fileExt = computed(() => {
  const parts = props.node.label.split(".");
  return parts.length > 1 ? parts.pop()!.toLowerCase() : "";
});

const FILE_ICONS: Record<string, string> = {
  md:    "MD",
  jsonl: "JL",
  json:  "JS",
  yaml:  "YL",
  yml:   "YL",
  sql:   "SQ",
  ts:    "TS",
  cs:    "CS",
  txt:   "TX",
  cmd:   "CM",
  sh:    "SH",
};

const fileIcon = computed(() => FILE_ICONS[fileExt.value] ?? "··");

function onClick() {
  if (props.node.kind === "folder") {
    expanded.value = !expanded.value;
    if (expanded.value && !props.isLoaded(props.node.ref)) {
      emit("expand", props.node.ref);
    }
  } else {
    emit("select", props.node);
  }
}

const sizeText = computed(() => {
  const b = props.node.sizeBytes;
  if (b == null) return "";
  if (b < 1024) return `${b} B`;
  if (b < 1024 * 1024) return `${(b / 1024).toFixed(1)} KB`;
  return `${(b / (1024 * 1024)).toFixed(1)} MB`;
});

const dateText = computed(() => {
  if (!props.node.modified) return "";
  const d = new Date(props.node.modified);
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")} ${String(d.getHours()).padStart(2, "0")}:${String(d.getMinutes()).padStart(2, "0")}`;
});
</script>

<style scoped>
.node-row {
  display: flex;
  align-items: center;
  gap: 4px;
  padding-top: 3px;
  padding-bottom: 3px;
  padding-right: 8px;
  cursor: pointer;
  font-size: var(--fs-sm);
  user-select: none;
  border-radius: 3px;
  color: var(--muted);
}
.node-row:hover { background: color-mix(in srgb, var(--text) 7%, transparent); }
.node-row.selected { background: var(--accent-soft); color: var(--accent); }
.node-row.leaf { color: var(--text); }

/* folder arrow */
.folder-ic {
  width: 14px;
  flex-shrink: 0;
  text-align: center;
  font-size: var(--fs-xs);
  color: var(--muted);
}

/* file type badge */
.file-badge {
  flex-shrink: 0;
  width: 22px;
  font-size: 9px;
  font-weight: 700;
  letter-spacing: 0;
  text-align: center;
  border-radius: 3px;
  padding: 1px 2px;
  line-height: 1.3;
  color: #fff;
  background: var(--muted);
}

/* per-type colors */
.file-badge[data-ext="md"]   { background: #5b7fa3; }
.file-badge[data-ext="jsonl"]{ background: var(--accent); }
.file-badge[data-ext="json"] { background: #7a6fba; }
.file-badge[data-ext="yaml"],
.file-badge[data-ext="yml"]  { background: #5fa06a; }
.file-badge[data-ext="sql"]  { background: #b06040; }
.file-badge[data-ext="ts"]   { background: #3178c6; }
.file-badge[data-ext="cs"]   { background: #7b5fa0; }

.node-row.selected .file-badge { opacity: 0.85; }

.node-label {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.node-meta {
  font-size: var(--fs-xs);
  color: var(--muted);
  white-space: nowrap;
  min-width: 50px;
  text-align: right;
}
.node-meta.date { min-width: 110px; }
</style>
