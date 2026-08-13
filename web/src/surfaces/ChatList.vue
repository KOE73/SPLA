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
import { onUnmounted } from "vue";
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

/** The project this connection is currently focused on — undefined (omitted) means "the
 * connection's default project", exactly what a single-project client already wants. */
function projectExtra() {
  return store.currentProjectId ? { projectId: store.currentProjectId } : undefined;
}

function newChat() { client.send("chat.new", { title: null }, projectExtra()); }

function onChatClick(chatId: string) {
  client.send("chat.open", { chatId }, projectExtra());
  openPanel("chat");
}

function rename(chat: ChatSummary) {
  const nt = prompt("Rename chat", chat.title || "");
  if (nt) client.send("chat.rename", { chatId: chat.id, title: nt }, projectExtra());
}

function remove(chatId: string) {
  if (!confirm("Delete this chat?")) return;
  client.send("chat.delete", { chatId }, projectExtra());
  forgetSession(chatId);
  if (chatId === store.currentChat) store.currentChat = null;
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
</style>
