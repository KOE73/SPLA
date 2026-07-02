<template>
  <div class="picker-overlay" @click.self="close">
    <div class="picker-panel">
      <div class="picker-header">
        <b>Projects</b>
        <button class="x" title="Close" @click="close">✕</button>
      </div>

      <div class="picker-list">
        <div v-if="loading" class="picker-empty">Loading…</div>
        <div v-else-if="store.projects.length === 0" class="picker-empty">No known projects yet.</div>
        <ProjectListItem
          v-for="p in store.projects"
          :key="p.id"
          :project="p"
          :active="p.id === store.currentProjectId"
          @select="select"
        />
      </div>

      <div v-if="error" class="picker-error">{{ error }}</div>

      <div class="picker-footer">
        <button class="btn-new-project" @click="createProject">+ New Project…</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";
import { setCurrentProject } from "../state/project";
import type { ProjectContextPayload, ProjectListResultPayload } from "../protocol/types";
import ProjectListItem from "./ProjectListItem.vue";

const emit = defineEmits<{ close: [] }>();
const loading = ref(true);
const error = ref<string | null>(null);

onMounted(async () => {
  try {
    const result = await client.invoke<ProjectListResultPayload>("project.recent");
    store.projects = result.projects || [];
  } finally {
    loading.value = false;
  }
});

function close() { emit("close"); }

/** Opens the project (building its runtime on first touch — cheap to repeat) and re-focuses the
 * whole sidebar/chat area on it. No new connection needed — same socket, just a different
 * ProjectId on every subsequent envelope (see AgentRuntimeRegistry/ClientConnection.Resolve). */
async function select(projectId: string) {
  try {
    const ctx = await client.invoke<ProjectContextPayload>("project.open", { projectId });
    applyContext(ctx);
  } catch (e) {
    error.value = "Failed to open project: " + e;
    console.error(error.value);
  }
}

async function createProject() {
  const manifestPath = prompt(
    "Path for the new project's .spla manifest (e.g. C:\\Projects\\Demo\\Demo.spla):"
  );
  if (!manifestPath) return;
  const name = prompt("Project name:", manifestPath.split(/[\\/]/).pop()?.replace(/\.spla$/i, "") || "");
  try {
    const ctx = await client.invoke<ProjectContextPayload>("project.create", { manifestPath, name });
    applyContext(ctx);
  } catch (e) {
    error.value = "Failed to create project: " + e;
    console.error(error.value);
  }
}

function applyContext(ctx: ProjectContextPayload) {
  setCurrentProject(ctx.projectId, ctx.projectName);
  store.workspacePath = ctx.workspacePath ?? null;
  if (ctx.theme) store.theme = ctx.theme;
  store.currentChat = null;
  store.chats = [];
  client.send("chat.list", null, { projectId: ctx.projectId });
  close();
}
</script>

<style scoped>
.picker-overlay {
  position: fixed;
  inset: 0;
  background: color-mix(in srgb, black 35%, transparent);
  display: flex;
  align-items: flex-start;
  justify-content: flex-start;
  z-index: 50;
}
.picker-panel {
  margin: 44px 0 0 8px;
  width: 300px;
  max-height: min(70vh, 480px);
  display: flex;
  flex-direction: column;
  background: var(--panel);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  box-shadow: 0 8px 24px color-mix(in srgb, black 25%, transparent);
  overflow: hidden;
}
.picker-header {
  display: flex;
  align-items: center;
  padding: 8px 10px;
  border-bottom: 1px solid var(--border);
  font-size: var(--fs-sm);
}
.picker-header b { flex: 1; }
.picker-header .x {
  background: transparent;
  border: none;
  color: var(--muted);
  cursor: pointer;
  font-size: var(--fs-sm);
}
.picker-header .x:hover { color: var(--danger); }

.picker-list { overflow: auto; flex: 1; }
.picker-empty { padding: 12px 10px; color: var(--muted); font-size: var(--fs-sm); }
.picker-error { padding: 8px 10px; color: var(--danger); font-size: var(--fs-sm); border-top: 1px solid var(--border); }

.picker-footer { border-top: 1px solid var(--border); padding: 6px; }
.btn-new-project {
  width: 100%;
  background: transparent;
  border: 1px dashed var(--border);
  border-radius: var(--radius-sm);
  color: var(--text);
  font-size: var(--fs-sm);
  padding: 6px 8px;
  cursor: pointer;
}
.btn-new-project:hover { background: var(--accent-soft); color: var(--accent); border-color: var(--accent); }
</style>
