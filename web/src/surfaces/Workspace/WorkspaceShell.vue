<template>
  <div class="ws-root">
  <!-- No workspace root ---------------------------------------------------- -->
  <div v-if="!workspacePath" class="ws-empty">
    <div class="ws-empty-icon">◫</div>
    <div>No project open</div>
    <div class="ws-empty-hint">Open a <code>.spla</code> project to browse its files.</div>
  </div>

  <!-- Workspace shell ------------------------------------------------------- -->
  <div v-else class="ws-shell">
    <!-- Left pane: file tree (width controlled by drag) -->
    <div class="tree-pane" :style="{ width: treeWidth + 'px' }">
      <FileBrowser
        :rootLabel="rootLabel"
        :rootNodes="rootNodes"
        :selectedRef="selectedRef"
        :busy="busy"
        :getChildren="getChildren"
        :isLoaded="isLoaded"
        @select="onNodeSelect"
        @expand="onNodeExpand"
      />
    </div>

    <!-- Drag splitter -->
    <div class="ws-splitter" @mousedown.prevent="startResize" title="Drag to resize" />

    <!-- Right pane -->
    <div v-if="!selectedNode" class="ws-placeholder">
      <span>Select a file to open it.</span>
    </div>
    <EditorHost
      v-else
      :text="fileText"
      :contentType="selectedNode.contentType ?? 'txt'"
      :docId="selectedNode.ref"
      :dark="isDark"
      @save="onSave"
    />
  </div>
  </div><!-- /ws-root -->
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from "vue";
import { store } from "../../state/store";
import { client } from "../../protocol/SplaClient";
import { useFsBrowser } from "../../composables/useFsBrowser";
import type { FsNode, FsReadResultPayload, FsWriteResultPayload } from "../../protocol/types";
import FileBrowser from "./FileBrowser.vue";
import EditorHost from "./EditorHost.vue";

const TREE_WIDTH_KEY = "spla.treeWidth";
const TREE_MIN = 140;
const TREE_MAX = 600;

const workspacePath = computed(() => store.workspacePath);
const isDark = computed(() => ["dark", "emerald"].includes(store.theme));

const { rootNodes, busy, browse, getChildren, isLoaded } = useFsBrowser();

const rootLabel = computed(() => {
  if (!workspacePath.value) return "";
  const parts = workspacePath.value.replace(/\\/g, "/").split("/").filter(Boolean);
  return parts[parts.length - 1] || workspacePath.value;
});

const selectedNode = ref<FsNode | null>(null);
const selectedRef  = computed(() => selectedNode.value?.ref ?? null);
const fileText     = ref("");

// ── Tree width (drag-resizable, persisted) ────────────────────────────────────
const treeWidth = ref(parseInt(localStorage.getItem(TREE_WIDTH_KEY) || "280", 10));

let resizing = false;
let resizeStartX = 0;
let resizeStartW = 0;

function startResize(e: MouseEvent) {
  resizing = true;
  resizeStartX = e.clientX;
  resizeStartW = treeWidth.value;
  window.addEventListener("mousemove", onResizeMove);
  window.addEventListener("mouseup", onResizeUp);
  document.body.style.cursor = "col-resize";
  document.body.style.userSelect = "none";
}

function onResizeMove(e: MouseEvent) {
  if (!resizing) return;
  const delta = e.clientX - resizeStartX;
  treeWidth.value = Math.min(TREE_MAX, Math.max(TREE_MIN, resizeStartW + delta));
}

function onResizeUp() {
  resizing = false;
  window.removeEventListener("mousemove", onResizeMove);
  window.removeEventListener("mouseup", onResizeUp);
  document.body.style.cursor = "";
  document.body.style.userSelect = "";
  localStorage.setItem(TREE_WIDTH_KEY, String(treeWidth.value));
}

onUnmounted(() => {
  window.removeEventListener("mousemove", onResizeMove);
  window.removeEventListener("mouseup", onResizeUp);
});

// ── File loading ──────────────────────────────────────────────────────────────
onMounted(() => { if (workspacePath.value) browse(); });
watch(workspacePath, v => {
  if (v) { selectedNode.value = null; fileText.value = ""; browse(); }
});

async function onNodeSelect(node: FsNode) {
  selectedNode.value = node;
  fileText.value = "";
  try {
    const result = await client.invoke<FsReadResultPayload>("fs.read", { ref: node.ref });
    fileText.value = result?.text ?? "";
  } catch (e) {
    fileText.value = `// Error loading file: ${e}`;
  }
}

async function onNodeExpand(ref: string) { await browse(ref); }

async function onSave(payload: { docId: string; text: string }) {
  try {
    await client.invoke<FsWriteResultPayload>("fs.write", { ref: payload.docId, text: payload.text });
  } catch (e) {
    console.error("workspace autosave failed:", e);
  }
}
</script>

<style scoped>
/* ── Root wrapper ────────────────────────────────────────────────────────────── */
.ws-root {
  display: flex;
  /* Fill the dock panel. dockview's content container isn't a flex column, so `flex:1` alone
     collapses to content height — pin height/width like the other dock surfaces do. */
  width: 100%;
  height: 100%;
  min-height: 0;
  overflow: hidden;
}

/* ── Empty state ─────────────────────────────────────────────────────────────── */
.ws-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  flex: 1;
  min-height: 0;
  color: var(--muted);
  font-size: var(--fs-sm);
}
.ws-empty-icon { font-size: 40px; opacity: .3; }
.ws-empty-hint { font-size: var(--fs-xs); }
.ws-empty code {
  background: var(--panel); border: 1px solid var(--border);
  border-radius: 3px; padding: 1px 4px; font-family: var(--mono);
}

/* ── Shell layout ────────────────────────────────────────────────────────────── */
.ws-shell { display: flex; flex: 1; min-height: 0; overflow: hidden; }

/* ── Tree pane (width set inline via :style) ─────────────────────────────────── */
.tree-pane {
  display: flex;
  flex-direction: column;
  min-width: 0;
  min-height: 0;
  flex-shrink: 0;
  overflow: hidden;
}

/* ── Drag splitter ───────────────────────────────────────────────────────────── */
.ws-splitter {
  width: 5px;
  flex-shrink: 0;
  background: var(--border);
  cursor: col-resize;
  transition: background 0.15s;
  position: relative;
}
.ws-splitter:hover,
.ws-splitter:active {
  background: var(--accent);
}

/* ── Right-pane placeholder ──────────────────────────────────────────────────── */
.ws-placeholder {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--muted);
  font-size: var(--fs-sm);
}
</style>
