<template>
  <div class="chat-item" :class="{ active }" @click="$emit('select', chat.id)">
    <span class="t">{{ chat.title || chat.id }}</span>
    <span class="x" title="Rename" @click.stop="$emit('rename', chat)">✎</span>
    <span class="x" title="Delete" @click.stop="$emit('delete', chat.id)">✕</span>
  </div>
</template>

<script setup lang="ts">
import type { ChatSummary } from "../protocol/types";

// One row in the chat/project list. Kept as its own component (not inlined in a v-for) so future
// per-item content — project badges, unread markers, live status — has one place to grow without
// bloating the list's own template.
defineProps<{ chat: ChatSummary; active: boolean }>();
defineEmits<{ select: [id: string]; rename: [chat: ChatSummary]; delete: [id: string] }>();
</script>

<style scoped>
.chat-item {
  padding: 6px 10px;
  border-bottom: 1px solid color-mix(in srgb, var(--border) 40%, transparent);
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
}
.chat-item:hover { background: color-mix(in srgb, var(--text) 6%, transparent); }
.chat-item.active { background: var(--accent-soft); color: var(--accent); }
.chat-item .t { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.chat-item .x { color: var(--muted); opacity: 0; font-size: var(--fs-xs); padding: 0 2px; flex-shrink: 0; }
.chat-item:hover .x { opacity: .8; }
.chat-item .x:hover { color: var(--danger); }
</style>
