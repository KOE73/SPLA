<template>
  <div class="sidebar-header">
    <b class="app-title">SPLA</b>
    <div class="nav-tabs">
      <button
        class="nav-tab"
        :class="{ active: store.layout === 'default' }"
        title="Chats"
        @click="setLayout('default')"
      >💬</button>
      <button
        class="nav-tab"
        :class="{ active: store.layout === 'workspace' }"
        title="Project files"
        @click="setLayout('workspace')"
      >◫</button>
    </div>
    <button v-if="store.layout === 'default'" class="btn-new" @click="newChat">+ New</button>
  </div>

  <!-- Chat list — shown in both layouts so the user can switch chats while browsing files -->
  <div id="chats">
    <div
      v-for="chat in store.chats"
      :key="chat.id"
      class="chat-item"
      :class="{ active: chat.id === store.currentChat }"
      @click="onChatClick(chat.id)"
    >
      <span class="t">{{ chat.title || chat.id }}</span>
      <span class="x" title="Rename" @click.stop="rename(chat)">✎</span>
      <span class="x" title="Delete" @click.stop="remove(chat.id)">✕</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onUnmounted } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";
import type { ChatSummary } from "../protocol/types";

const offList = client.on("chat.list.result", p => { store.chats = p.chats || []; });
onUnmounted(offList);

function setLayout(name: string) {
  store.layout = name;
  localStorage.setItem("spla.layout", name);
}

function newChat() { client.send("chat.new", { title: null }); }

function onChatClick(chatId: string) {
  client.send("chat.open", { chatId });
  // Switch to chat layout when opening a chat from file-browser mode
  if (store.layout !== "default") setLayout("default");
}

function rename(chat: ChatSummary) {
  const nt = prompt("Rename chat", chat.title || "");
  if (nt) client.send("chat.rename", { chatId: chat.id, title: nt });
}

function remove(chatId: string) {
  if (!confirm("Delete this chat?")) return;
  client.send("chat.delete", { chatId });
  if (chatId === store.currentChat) store.currentChat = null;
}
</script>

<style scoped>
/* ── Sidebar header (div, not header — avoids #sidebar header global rule) ────── */
.sidebar-header {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: var(--pad);
  border-bottom: 1px solid var(--border);
  background: var(--panel);
  flex-shrink: 0;
}

.app-title {
  font-size: var(--fs-sm);
  color: var(--text);
  white-space: nowrap;
  margin-right: 2px;
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
