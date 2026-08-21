<template>
  <!-- The project lives in ProjectBar at the foot of this column now, not in this header: it is
       connection state, and repeating it here only stole width from the chat controls. -->
  <div class="sidebar-header">
    <div class="nav-tabs">
      <button
        class="nav-tab"
        title="Chats"
        @click="openPanel('chat')"
      >💬</button>
      <button
        class="nav-tab"
        title="Project files"
        @click="openPanel('workspace')"
      >◫</button>
    </div>
    <button class="btn-new" @click="newChat">+ New</button>
  </div>

  <!-- Chat list — shown in both layouts so the user can switch chats while browsing files -->
  <div id="chats">
    <!-- Aggregate status indicator: if any chat is waiting (needs attention), show that; else if any
         is working (busy), show that. This lets the user notice activity even when the list is scrolled
         out of view. Clicking it scrolls to / selects the first chat in that state. -->
    <div v-if="aggregateState" class="status-line" :class="`aggregate-${aggregateState}`" @click="scrollToState">
      <span :title="aggregateLabel">●</span>
      <span class="label">{{ aggregateLabel }}</span>
    </div>

    <ChatListItem
      v-for="chat in store.chats"
      :key="chat.id"
      :chat="chat"
      :active="chat.id === store.currentChat"
      @select="onChatClick"
      @rename="rename"
      @delete="remove"
    />
  </div>

  <!-- Project/server status bar, pinned under the chat list. Nothing chat-scoped goes in here. -->
  <ProjectBar />

  <ProjectPicker v-if="store.projectPickerOpen" @close="store.projectPickerOpen = false" />
</template>

<script setup lang="ts">
import { computed, onUnmounted, ref } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";
import type { ChatSummary } from "../protocol/types";
import ChatListItem from "./ChatListItem.vue";
import ProjectPicker from "./ProjectPicker.vue";
import ProjectBar from "./ProjectBar.vue";
import { openPanel } from "../dock/dockController";
import { forgetSession } from "../state/chatSessions";

const offList = client.on("chat.list.result", p => { store.chats = p.chats || []; });
onUnmounted(offList);

const chatsContainerRef = ref<HTMLElement>();

function newChat() { client.send("chat.new", { title: null }); }

function onChatClick(chatId: string) {
  client.send("chat.open", { chatId });
  openPanel("chat");
}

function rename(chat: ChatSummary) {
  const nt = prompt("Rename chat", chat.title || "");
  if (nt) client.send("chat.rename", { chatId: chat.id, title: nt });
}

function remove(chatId: string) {
  if (!confirm("Delete this chat?")) return;
  client.send("chat.delete", { chatId });
  forgetSession(chatId);
  if (chatId === store.currentChat) store.currentChat = null;
}

// Compute the aggregate state for the indicator. Priority: waiting > working > (nothing).
// This tells the user at a glance if there's something demanding attention or if work is in flight.
const aggregateState = computed(() => {
  const hasWaiting = store.chats.some(c => c.state === "waiting");
  if (hasWaiting) return "waiting";
  const hasWorking = store.chats.some(c => c.state === "working");
  if (hasWorking) return "working";
  return null;
});

// User-facing label for the aggregate indicator.
const aggregateLabel = computed(() => {
  if (aggregateState.value === "waiting") return "Someone is waiting";
  if (aggregateState.value === "working") return "Work in progress";
  return "";
});

// Click handler: select the first chat in the current state and bring it into view.
function scrollToState() {
  if (!aggregateState.value) return;
  const target = store.chats.find(c => c.state === aggregateState.value);
  if (target) {
    onChatClick(target.id);
  }
}
</script>

<style scoped>
/* ── Sidebar header (div, not header — avoids #sidebar header global rule) ────── */
.sidebar-header {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px var(--pad);
  border-bottom: 1px solid var(--border);
  background: var(--panel);
  flex-shrink: 0;
}

/* ── Nav tabs (💬 / ◫) ───────────────────────────────────────────────────────── */
.nav-tabs {
  display: flex;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  overflow: hidden;
  flex-shrink: 0;
}

.nav-tab {
  background: transparent;
  border: none;
  padding: 3px 8px;
  font-size: 14px;
  line-height: 1;
  cursor: pointer;
  color: var(--muted);
  transition: background 0.1s;
}
.nav-tab:hover:not(.active) {
  background: color-mix(in srgb, var(--text) 6%, transparent);
}
.nav-tab.active {
  background: var(--accent-soft);
  color: var(--accent);
}

/* ── New chat button ─────────────────────────────────────────────────────────── */
.btn-new {
  margin-left: auto;
  background: transparent;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  color: var(--text);
  font-size: var(--fs-sm);
  padding: 3px 8px;
  cursor: pointer;
  white-space: nowrap;
  flex-shrink: 0;
}
.btn-new:hover { background: var(--accent-soft); color: var(--accent); border-color: var(--accent); }

/* ── Aggregate status indicator ────────────────────────────────────────────────── */
/* Pinned at the top of the chat list when any chat has activity. Gives users at-a-glance
   visibility of waiting (demands attention) or working (in progress) states. Clicking it
   jumps to / selects the first chat in that state. Each style has distinct visuals:
   waiting — urgent accent pulse; working — subtle quiet pulse. */
.status-line {
  padding: 6px 10px;
  border-bottom: 1px solid color-mix(in srgb, var(--border) 40%, transparent);
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: var(--fs-xs);
  cursor: pointer;
  flex-shrink: 0;
  user-select: none;
}
.status-line:hover { background: color-mix(in srgb, var(--text) 6%, transparent); }
.status-line span:first-child { font-size: 6px; }
.status-line.aggregate-waiting span:first-child {
  color: var(--danger);
  animation: status-waiting-pulse 0.8s ease-in-out infinite;
}
@keyframes status-waiting-pulse { 0%, 100% { opacity: 1; } 50% { opacity: .5; } }
.status-line.aggregate-waiting .label { color: var(--danger); font-weight: 600; }
.status-line.aggregate-working span:first-child {
  color: var(--accent);
  animation: status-working-pulse 1.6s ease-in-out infinite;
}
@keyframes status-working-pulse { 0%, 100% { opacity: .35; } 50% { opacity: 1; } }
.status-line.aggregate-working .label { color: var(--accent); }
.status-line .label { flex: 1; }
</style>
