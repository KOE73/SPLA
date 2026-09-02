<template>
  <div
    class="chat-item" :class="{ active, archived }" :style="{ paddingLeft: 10 + depth * 14 + 'px' }"
    @click="$emit('select', chat.id)"
  >
    <!-- A spawned session sits under its parent role (ADR_20260827-2 §2.5: "список чатов становится
         деревом роль → чат") — the connector says "this is someone's child", the role badge says
         whose role it ran as. Depth alone (indent) reads as nesting but not as "why". -->
    <span v-if="depth > 0" class="tree-connector">↳</span>
    <span v-if="chat.as" class="role-badge" :title="`role: ${chat.as}`">{{ chat.as }}</span>
    <!-- Where work is happening — including a turn another window or another user started. Archived
         chats never carry a live turn (ChatRegistry.Archive closes it), so this never renders there. -->
    <span v-if="chat.turnActive" class="busy" title="A turn is running in this chat">●</span>
    <!-- State indicator: rendered for non-idle states to show operational status. The badge
         communicates: working (subtle pulse) | waiting (attention-grabbing) | stalled (muted, stuck). -->
    <span v-if="chat.state && chat.state !== 'idle'" class="state-badge" :class="`state-${chat.state}`" :title="stateLabel(chat.state)">●</span>
    <span class="t">{{ chat.title || chat.id }}</span>
    <template v-if="archived">
      <span class="x" title="Restore" @click.stop="$emit('restore', chat.id)">↺</span>
      <span class="x" title="Delete permanently" @click.stop="$emit('delete-permanently', chat.id)">✕</span>
    </template>
    <template v-else>
      <span class="x" title="Open in a separate window" @click.stop="$emit('open-window', chat)">⧉</span>
      <span class="x" title="Rename" @click.stop="$emit('rename', chat)">✎</span>
      <span class="x" title="Archive" @click.stop="$emit('archive', chat.id)">🗄</span>
      <span class="x" title="Delete" @click.stop="$emit('delete', chat.id)">✕</span>
    </template>
  </div>

  <!-- Spawned descendants, nested to whatever depth the spawn chain reached (ChatSummary.children).
       Recursive by the component's own SFC self-reference — no separate "tree node" wrapper needed.
       `active` is recomputed per row from the store rather than threaded down as a prop: a nested
       row is not "active because its parent is" — it is active only when IT is store.currentChat. -->
  <ChatListItem
    v-for="child in chat.children ?? []" :key="child.id" :chat="child" :depth="depth + 1"
    :active="child.id === store.currentChat" :archived="archived"
    @select="$emit('select', $event)" @rename="$emit('rename', $event)" @delete="$emit('delete', $event)"
    @archive="$emit('archive', $event)" @open-window="$emit('open-window', $event)"
  />
</template>

<script setup lang="ts">
import type { ChatSummary } from "../protocol/types";
import { store } from "../state/store";

// One row in the chat/project list — and, via its own children, the whole subtree spawned under it.
// Kept as its own component (not inlined in a v-for) so future per-item content — project badges,
// unread markers, live status — has one place to grow without bloating the list's own template.
withDefaults(
  defineProps<{ chat: ChatSummary; active: boolean; archived?: boolean; depth?: number }>(),
  { archived: false, depth: 0 }
);

defineEmits<{
  select: [id: string];
  rename: [chat: ChatSummary];
  delete: [id: string];
  archive: [id: string];
  restore: [id: string];
  "delete-permanently": [id: string];
  "open-window": [chat: ChatSummary];
}>();

// Convert state code to user-facing label for the title attribute.
function stateLabel(state: string): string {
  const labels: Record<string, string> = {
    working: "A turn is running and making progress",
    waiting: "Waiting for a person to respond (permission or clarification needed)",
    stalled: "A turn is registered but has not progressed recently (may be stuck)"
  };
  return labels[state] || state;
}
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

/* Tree: a spawned session's connector + the role it ran as (ADR_20260827-2 §2.5's "дерево роль → чат"). */
.chat-item .tree-connector { color: var(--muted); flex-shrink: 0; }
.chat-item .role-badge {
  flex-shrink: 0; font-size: var(--fs-xs); font-weight: 600; line-height: 1.4;
  padding: 0 5px; border-radius: var(--radius-sm);
  background: var(--accent-soft); color: var(--accent);
}

/* Archived chats — visually muted so they read as put away, not just another row in the list. */
.chat-item.archived { color: var(--muted); opacity: 0.7; font-style: italic; }
.chat-item.archived:hover { opacity: 0.9; background: color-mix(in srgb, var(--muted) 10%, transparent); }
.chat-item.archived.active { background: color-mix(in srgb, var(--muted) 18%, transparent); color: var(--text); }
.chat-item .busy { color: var(--accent); font-size: 8px; flex-shrink: 0; animation: chat-busy 1.4s ease-in-out infinite; }
@keyframes chat-busy { 0%, 100% { opacity: .25; } 50% { opacity: 1; } }

/* State badge: operational status indicator. Each state has distinct visuals:
   working — subtle pulse to show progress; waiting — attention-grabbing accent to demand notice;
   stalled — muted to indicate stuck-ness without urgency. */
.chat-item .state-badge { font-size: 6px; flex-shrink: 0; }
.chat-item .state-working { color: var(--accent); animation: state-pulse 1.6s ease-in-out infinite; }
@keyframes state-pulse { 0%, 100% { opacity: .35; } 50% { opacity: 1; } }
.chat-item .state-waiting { color: var(--danger); animation: state-waiting-pulse 0.8s ease-in-out infinite; }
@keyframes state-waiting-pulse { 0%, 100% { opacity: 1; } 50% { opacity: .5; } }
.chat-item .state-stalled { color: var(--muted); opacity: .6; }

.chat-item .x { color: var(--muted); opacity: 0; font-size: var(--fs-xs); padding: 0 2px; flex-shrink: 0; }
.chat-item:hover .x { opacity: .8; }
.chat-item .x:hover { color: var(--danger); }
</style>
