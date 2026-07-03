<template>
  <!-- Sidebar footer slot: server-side context (who is signed in, and room for more — connection /
       server info) pinned under the chat list. Collapses to nothing when there is no authenticated
       user (local/embedded), so single-user mode looks unchanged. -->
  <div v-if="store.userName" class="sidebar-foot">
    <div class="foot-row" :title="store.userName">
      <span class="foot-icon">👤</span>
      <span class="foot-name">{{ store.userName }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { store } from "../state/store";
</script>

<style scoped>
.sidebar-foot {
  flex-shrink: 0;
  border-top: 1px solid var(--border);
  padding: 6px var(--pad);
  background: var(--panel);
  font-size: var(--fs-sm);
  color: var(--muted);
}
.foot-row {
  display: flex;
  align-items: center;
  gap: 6px;
}
.foot-name {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
