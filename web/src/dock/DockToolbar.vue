<!--
  Thin control strip that sits ONLY above the right area (chat + tools) — see AppShell. It opens tool
  panels and controls how the tools are shown: hide/show (chat fills), fullscreen the active tool, or
  reset. The fixed left navigation column is never affected by anything here.

  The SSH button is special: terminals are not a singleton, so it opens a PICKER (configured hosts +
  live sessions from ssh.sessions.get) and every selection spawns a new terminal panel.
-->
<template>
  <div class="dock-toolbar">
    <span class="spacer"></span>

    <!-- Tool pickers — one block, pushed right toward the view controls -->
    <div class="group">
      <template v-for="t in tools" :key="t.kind">
        <span v-if="t.kind === 'ssh'" class="ssh-anchor">
          <button :title="t.title" :class="{ on: sshMenu || isOpen(t.kind) }" @click.stop="toggleSshMenu">
            <Icon :name="t.icon" />
          </button>
          <div v-if="sshMenu" class="ssh-menu" @click.stop>
            <div v-if="!ssh" class="menu-hint">loading…</div>
            <template v-else>
              <!-- Live sessions first: attach a terminal to any of them (watch the agent / reattach). -->
              <template v-if="ssh.sessions.length">
                <div class="menu-head">Live sessions — attach to watch</div>
                <div v-for="s in ssh.sessions" :key="s.id" class="menu-item session-row">
                  <button class="menu-item-main" @click="attach(s.id)">
                    <span class="dot live"></span>
                    <span class="name">{{ s.id }}</span>
                    <span class="detail">{{ s.host }}</span>
                    <span class="badge" :class="s.openedBy === 'agent' ? 'agent' : ''">{{ s.openedBy }}</span>
                    <span v-if="s.viewers" class="badge open">{{ s.viewers }}⌨</span>
                  </button>
                  <button class="kill-btn" title="End this session for everyone" @click="killSession(s.id)">✕</button>
                </div>
              </template>

              <div class="menu-head">Open a new session</div>
              <div v-if="ssh.hosts.length === 0" class="menu-hint">
                No hosts configured. Settings → Plugins → ssh.
              </div>
              <button v-for="h in ssh.hosts" :key="h.name" class="menu-item" @click="openNew(h.name)">
                <span class="dot"></span>
                <span class="name">{{ h.name }}</span>
                <span class="detail">{{ h.host }}{{ h.port && h.port !== 22 ? ":" + h.port : "" }}</span>
                <span v-if="h.isDefault" class="badge">default</span>
                <span v-if="sessionCount(h.name)" class="badge open">{{ sessionCount(h.name) }} live</span>
              </button>
            </template>
          </div>
        </span>
        <button v-else :title="t.title" :class="{ on: isOpen(t.kind) }" @click="openPanel(t.kind)">
          <Icon :name="t.icon" />
        </button>
      </template>
    </div>

    <span class="gap"></span>

    <!-- View controls — a separate block, not merged with the pickers -->
    <div class="group">
      <button :title="dockState.chatOnly ? 'Show tools' : 'Hide tools (chat fills the area)'"
              :disabled="!dockState.chatOnly && !hasTools()" @click="toggleChatOnly">
        <Icon :name="dockState.chatOnly ? 'showTools' : 'hideTools'" />
      </button>
      <button :title="dockState.maximized ? 'Restore split' : 'Fullscreen the active tool'"
              :disabled="!hasTools()" :class="{ on: dockState.maximized }" @click="toggleMaximize">
        <Icon name="fullscreen" />
      </button>
      <button title="Detach the active panel to a separate window" :disabled="activeProtected" @click="popoutActivePanel">
        <Icon name="detach" />
      </button>
      <button title="Reset layout" @click="resetDock">
        <Icon name="reset" />
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onUnmounted, ref } from "vue";
import Icon from "./Icon.vue";
import { client } from "../protocol/SplaClient";
import { projectEnvelope } from "../state/project";
import type { SshSessionsResultPayload } from "../protocol/types";
import { panelCatalog, toolKinds, type PanelKind } from "./panelCatalog";
import {
  dockState, hasTools, isProtected, openPanel, openSshTerminal,
  popoutActivePanel, resetDock, toggleChatOnly, toggleMaximize,
} from "./dockController";

const iconFor: Record<PanelKind, string> = {
  chat: "workspace", workspace: "workspace", ssh: "ssh", browserScreencast: "browser", debug: "debug", wire: "wire",
};

const tools = computed(() => toolKinds.map(kind => ({
  kind, icon: iconFor[kind], title: panelCatalog[kind].title,
})));

function isOpen(kind: PanelKind) {
  dockState.activePanelId; // re-evaluate as the layout changes
  if (kind === "ssh") return !!dockState.api?.panels.some(p => p.id === "ssh" || p.id.startsWith("ssh:"));
  return !!dockState.api?.getPanel(kind);
}

const activeProtected = computed(() => {
  dockState.activePanelId;
  return isProtected(dockState.api?.activePanel);
});

// ── SSH host picker ────────────────────────────────────────────────────────
const sshMenu = ref(false);
const ssh = ref<SshSessionsResultPayload | null>(null);

function fetchSessions() { client.send("ssh.sessions.get", undefined, projectEnvelope()); }

const offSessions = client.on("ssh.sessions.result", p => { ssh.value = p; });
// The server pushes ssh.sessions.changed when the agent opens/closes a session — refresh the open
// picker live so a session the agent just opened appears without reopening the menu.
const offChanged = client.on("ssh.sessions.changed", () => { if (sshMenu.value) fetchSessions(); });
onUnmounted(() => { offSessions(); offChanged(); });

function toggleSshMenu() {
  sshMenu.value = !sshMenu.value;
  if (sshMenu.value) { ssh.value = null; fetchSessions(); }
}

function attach(session: string) {
  sshMenu.value = false;
  openSshTerminal({ session });
}
function openNew(host: string) {
  sshMenu.value = false;
  openSshTerminal({ host });
}

// Ends the session for everyone (agent + any attached terminals) — distinct from closing a tab,
// which only detaches this viewer and leaves the pty running (that part is correct: other viewers
// or the agent may still want it). This is the only way to actually kill a session from the picker.
function killSession(sessionId: string) {
  client.send("ssh.session.close", { sessionId }, projectEnvelope());
  fetchSessions();
}

function sessionCount(host: string) {
  return ssh.value?.sessions.filter(s => s.host.toLowerCase() === host.toLowerCase()).length || 0;
}

// Any click outside closes the picker.
function onDocClick() { sshMenu.value = false; }
document.addEventListener("click", onDocClick);
onUnmounted(() => document.removeEventListener("click", onDocClick));
</script>

<style scoped>
.dock-toolbar { height: 30px; flex: 0 0 30px; display: flex; align-items: center; padding: 2px 6px; background: var(--panel); border-bottom: 1px solid var(--border); }
.group { display: flex; align-items: center; gap: 2px; }
button { display: inline-flex; align-items: center; justify-content: center; width: 26px; height: 24px; padding: 0; border: 1px solid transparent; border-radius: 5px; background: transparent; color: var(--muted); }
button:hover:not(:disabled) { color: var(--text); background: var(--accent-soft); border-color: var(--border); }
button.on { color: var(--accent); background: var(--accent-soft); }
button:disabled { opacity: .3; cursor: default; }
.spacer { flex: 1; }
.gap { width: 14px; }

.ssh-anchor { position: relative; display: inline-flex; }
.ssh-menu {
  position: absolute; top: 28px; right: 0; z-index: 60; min-width: 260px;
  padding: 4px; border: 1px solid var(--border); border-radius: 8px;
  background: var(--panel); box-shadow: 0 6px 22px color-mix(in srgb, #000 30%, transparent);
}
.menu-head { padding: 4px 8px 6px; font-size: var(--fs-xs); color: var(--muted); }
.menu-hint { padding: 4px 8px 8px; font-size: var(--fs-sm); color: var(--muted); }
.menu-item {
  display: flex; align-items: center; gap: 7px; width: 100%; height: auto;
  padding: 5px 8px; border-radius: 5px; text-align: left; font-size: var(--fs-sm); color: var(--text);
}
.menu-item .name { font-weight: 600; }
.menu-item .detail { color: var(--muted); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; flex: 1; }
.menu-item .dot { width: 7px; height: 7px; border-radius: 50%; background: var(--border); flex: 0 0 7px; }
.menu-item .dot.live { background: #3fb950; }
.session-row { display: flex; align-items: stretch; gap: 2px; padding: 0; }
.menu-item-main {
  display: flex; align-items: center; gap: 7px; flex: 1; min-width: 0; height: auto;
  padding: 5px 8px; border-radius: 5px; text-align: left; font-size: var(--fs-sm); color: var(--text);
}
.kill-btn {
  flex: 0 0 auto; width: 22px; border-radius: 5px; color: var(--muted); background: transparent;
}
.kill-btn:hover { color: #f85149; background: color-mix(in srgb, #f85149 15%, transparent); }
.badge {
  padding: 1px 5px; border-radius: 8px; font-size: 10px;
  color: var(--muted); border: 1px solid var(--border);
}
.badge.agent { color: #3fb950; border-color: color-mix(in srgb, #3fb950 45%, transparent); }
.badge.open { color: var(--accent); border-color: color-mix(in srgb, var(--accent) 45%, transparent); }
</style>
