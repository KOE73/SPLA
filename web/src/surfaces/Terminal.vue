<!--
  Live SSH terminal (phase B of the "live console"). A real xterm bound to a server-side SSH pty:
  keystrokes go out as terminal.input, raw pty output (ANSI included) comes back as terminal.data and
  is written verbatim. The password is resolved server-side from the secret store — never sent here.

  Open standalone via /?surface=terminal (optionally &host=<name>); uses the SSH plugin's default_host
  when host is omitted.
-->
<template>
  <div class="term-surface">
    <div class="term-bar">
      <span class="dot" :class="statusClass"></span>
      <span class="term-title">{{ title }}</span>
      <span class="term-hint">{{ hint }}</span>
      <button v-if="liveSessionId" class="kill-btn" title="End this session for everyone (not just this view)" @click="killSession">
        End session
      </button>
    </div>
    <div ref="host" class="term-host"></div>
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from "vue";
import { Terminal } from "@xterm/xterm";
import { FitAddon } from "@xterm/addon-fit";
import "@xterm/xterm/css/xterm.css";
import { client } from "../protocol/SplaClient";
import { projectEnvelope } from "../state/project";
import { store } from "../state/store";

// Dock panel: dockview-vue mounts panels with ONE `params` prop; host/session ride inside
// params.params (set by openSshTerminal), and params.api lets us update the tab title/params once
// the live session id is known. Solo window (?surface=terminal) has no dock prop — fall back to URL.
const props = defineProps<{ params?: {
  params?: { host?: string; session?: string };
  api?: { setTitle(t: string): void; updateParameters(p: Record<string, unknown>): void };
} }>();

const host = ref<HTMLDivElement | null>(null);
const title = ref("SSH terminal");
const hint = ref("connecting…");
const status = ref<"connecting" | "open" | "closed">("connecting");
const statusClass = ref("connecting");
// The live session id (host#N), known only once terminal.opened resolves. Closing a tab just
// detaches this viewer and leaves the pty running (correct — other viewers/the agent may still want
// it); "End session" is the only path that actually kills it, so it only appears once we have an id.
const liveSessionId = ref<string | null>(null);

const urlParams = new URLSearchParams(location.search);
const terminalId = "term-" + Math.random().toString(36).slice(2, 10);
const requestedHost = props.params?.params?.host || urlParams.get("host") || undefined;
// Attach to an existing session (host#N) instead of opening a new one — the "watch the agent /
// reattach" path.
const requestedSession = props.params?.params?.session || urlParams.get("session") || undefined;

let term: Terminal | null = null;
let fit: FitAddon | null = null;
const disposers: Array<() => void> = [];
let opened = false;
let lastCols = 0;
let lastRows = 0;

function killSession() {
  if (liveSessionId.value) client.send("ssh.session.close", { sessionId: liveSessionId.value }, projectEnvelope());
}

function setStatus(s: "connecting" | "open" | "closed", text: string) {
  status.value = s;
  statusClass.value = s;
  hint.value = text;
}

// Fit to the host box, guarding against the zero-size layouts dockview hands us before the panel is
// measured (and while a panel is collapsed/hidden). Returns true when a real, changed size resulted.
function refit(): boolean {
  if (!fit || !term || !host.value) return false;
  if (host.value.clientWidth < 2 || host.value.clientHeight < 2) return false;
  fit.fit();
  if (term.cols === lastCols && term.rows === lastRows) return false;
  lastCols = term.cols;
  lastRows = term.rows;
  return true;
}

// Open the pty only once the socket is live AND the host has a real size — a bare send() before the
// ws handshake is dropped, and opening at a zero/default size makes the remote pty the wrong shape.
function tryOpen() {
  if (opened || !store.connected || !refit()) return;
  // Project-scoped: hosts live in the focused project's plugins.ssh.settings — without the
  // envelope the server resolves the connection's DEFAULT project and finds no hosts.
  if (client.send("terminal.open",
      { terminalId, host: requestedHost, session: requestedSession, cols: lastCols, rows: lastRows },
      projectEnvelope()))
    opened = true;
}

onMounted(() => {
  term = new Terminal({
    fontFamily: "ui-monospace, SFMono-Regular, Menlo, Consolas, monospace",
    fontSize: 13,
    cursorBlink: true,
    theme: { background: "#0b0e14", foreground: "#d6deeb" },
  });
  fit = new FitAddon();
  term.loadAddon(fit);
  term.open(host.value!);

  // Human keystrokes → pty.
  term.onData(d => client.send("terminal.input", { terminalId, data: d }));

  // Server → terminal.
  disposers.push(client.on("terminal.data", p => { if (p.terminalId === terminalId) term!.write(p.data); }));
  disposers.push(client.on("terminal.opened", p => {
    if (p.terminalId !== terminalId) return;
    title.value = "SSH: " + p.sessionId;
    setStatus("open", "connected — type to interact");
    liveSessionId.value = p.sessionId;
    // Put the live session id in the tab and panel params so the title reads host#N and a later
    // popout attaches to THIS session rather than opening a new one.
    props.params?.api?.setTitle("⌨ " + p.sessionId);
    props.params?.api?.updateParameters({ host: requestedHost, session: p.sessionId });
    term!.focus();
  }));
  disposers.push(client.on("terminal.closed", p => {
    if (p.terminalId !== terminalId) return;
    setStatus("closed", p.reason ? "closed: " + p.reason : "closed");
    term!.writeln("\r\n\x1b[90m[" + (p.reason || "closed") + "]\x1b[0m");
  }));

  // Reflow on any host-box size change — dockview resizes/moves the panel (including tear-off into a
  // separate window) without ever firing window.resize, which is why fit went stale before.
  const observer = new ResizeObserver(() => {
    if (!opened) { tryOpen(); return; }
    if (refit()) client.send("terminal.resize", { terminalId, cols: lastCols, rows: lastRows });
  });
  observer.observe(host.value!);
  disposers.push(() => observer.disconnect());

  if (store.connected) tryOpen();
  disposers.push(client.on("conn", p => { if (p.on) tryOpen(); }));
});

onBeforeUnmount(() => {
  client.send("terminal.close", { terminalId });
  disposers.forEach(d => d());
  term?.dispose();
});
</script>

<style scoped>
.term-surface { display: flex; flex-direction: column; height: 100%; background: #0b0e14; }
.term-bar {
  display: flex; align-items: center; gap: 8px;
  padding: 6px 10px; font-size: 12px; color: #9aa7bd;
  background: #11151f; border-bottom: 1px solid #1c2230;
}
.term-title { color: #d6deeb; font-weight: 600; }
.term-hint { margin-left: auto; opacity: 0.8; }
.kill-btn {
  padding: 2px 8px; border: 1px solid #3a2530; border-radius: 5px;
  color: #f85149; background: transparent; font-size: 11px;
}
.kill-btn:hover { background: color-mix(in srgb, #f85149 15%, transparent); }
.dot { width: 8px; height: 8px; border-radius: 50%; background: #6b7280; }
.dot.connecting { background: #d29922; }
.dot.open { background: #3fb950; }
.dot.closed { background: #f85149; }
.term-host { flex: 1; min-height: 0; padding: 4px 6px; }
</style>
