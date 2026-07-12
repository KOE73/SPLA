<template>
  <div class="dock-toolbar">
    <button title="Navigation" @click="openPanel('navigation')">☰</button>
    <button title="Chat" @click="openPanel('chat')">💬</button>
    <span class="separator"></span>
    <button title="Workspace" @click="openPanel('workspace')">◫</button>
    <button title="SSH terminal" @click="openPanel('ssh')">⌨</button>
    <button title="Experimental browser screencast" @click="openPanel('browserScreencast')">🌐</button>
    <button title="Debug" @click="openPanel('debug')">🧠</button>
    <button title="Wire" @click="openPanel('wire')">🔌</button>
    <span class="spacer"></span>
    <button :title="dockState.maximized ? 'Restore layout' : 'Maximize active panel'" @click="toggleMaximize">
      {{ dockState.maximized ? '❐' : '⛶' }}
    </button>
    <button title="Open active panel in a separate window" @click="popoutActivePanel">↗</button>
    <button title="Close active panel" :disabled="activeProtected" @click="closeActivePanel">×</button>
    <button title="Reset layout" @click="resetDock">↺</button>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { closeActivePanel, dockState, isProtected, openPanel, popoutActivePanel, resetDock, toggleMaximize } from "./dockController";

const activeProtected = computed(() => {
  dockState.activePanelId;
  return isProtected(dockState.api?.activePanel);
});
</script>

<style scoped>
.dock-toolbar { height: 28px; flex: 0 0 28px; display: flex; align-items: center; gap: 2px; padding: 2px 5px; background: var(--panel); border-bottom: 1px solid var(--border); }
button { min-width: 24px; height: 22px; padding: 0 5px; border: 1px solid transparent; border-radius: 4px; background: transparent; color: var(--muted); line-height: 1; }
button:hover:not(:disabled) { color: var(--text); background: var(--accent-soft); border-color: var(--border); }
button:disabled { opacity: .3; cursor: default; }
.separator { width: 1px; height: 16px; margin: 0 3px; background: var(--border); }
.spacer { flex: 1; }
</style>
