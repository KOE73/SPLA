<template>
  <div class="project-item" :class="{ active }" @click="$emit('select', project.id)">
    <span class="t">{{ project.name || project.id }}</span>
  </div>
</template>

<script setup lang="ts">
import type { ProjectDescriptor } from "../protocol/types";

// Mirrors ChatListItem.vue's shape — one component per row so future per-project content
// (recent-chat count, connection health, etc.) has a single place to grow.
defineProps<{ project: ProjectDescriptor; active: boolean }>();
defineEmits<{ select: [id: string] }>();
</script>

<style scoped>
.project-item {
  padding: 6px 10px;
  border-bottom: 1px solid color-mix(in srgb, var(--border) 40%, transparent);
  cursor: pointer;
}
.project-item:hover { background: color-mix(in srgb, var(--text) 6%, transparent); }
.project-item.active { background: var(--accent-soft); color: var(--accent); }
.project-item .t { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; display: block; }
</style>
