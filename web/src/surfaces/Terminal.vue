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
import { store } from "../state/store";

const host = ref<HTMLDivElement | null>(null);
const title = ref("SSH terminal");
const hint = ref("connecting…");
const status = ref<"connecting" | "open" | "closed">("connecting");
const statusClass = ref("connecting");

const terminalId = "term-" + Math.random().toString(36).slice(2, 10);
const requestedHost = new URLSearchParams(location.search).get("host") || undefined;

let term: Terminal | null = null;
let fit: FitAddon | null = null;
const disposers: Array<() => void> = [];

function setStatus(s: "connecting" | "open" | "closed", text: string) {
  status.value = s;
  statusClass.value = s;
  hint.value = text;
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
  fit.fit();

  // Human keystrokes → pty.
  term.onData(d => client.send("terminal.input", { terminalId, data: d }));

  // Server → terminal.
  disposers.push(client.on("terminal.data", p => { if (p.terminalId === terminalId) term!.write(p.data); }));
  disposers.push(client.on("terminal.opened", p => {
    if (p.terminalId !== terminalId) return;
    title.value = "SSH: " + p.host;
    setStatus("open", "connected — type to interact");
    term!.focus();
  }));
  disposers.push(client.on("terminal.closed", p => {
    if (p.terminalId !== terminalId) return;
    setStatus("closed", p.reason ? "closed: " + p.reason : "closed");
    term!.writeln("\r\n\x1b[90m[" + (p.reason || "closed") + "]\x1b[0m");
  }));

  const onResize = () => {
    if (!fit || !term) return;
    fit.fit();
    client.send("terminal.resize", { terminalId, cols: term.cols, rows: term.rows });
  };
  window.addEventListener("resize", onResize);
  disposers.push(() => window.removeEventListener("resize", onResize));

  // Open only once the socket is live — a solo surface mounts before/around the ws handshake, so a
  // bare send() on mount can be dropped. Fire now if already connected, else on the next connect.
  let opened = false;
  const openTerminal = () => {
    if (opened || !term) return;
    if (client.send("terminal.open", { terminalId, host: requestedHost, cols: term.cols, rows: term.rows }))
      opened = true;
  };
  if (store.connected) openTerminal();
  disposers.push(client.on("conn", p => { if (p.on) openTerminal(); }));
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
.dot { width: 8px; height: 8px; border-radius: 50%; background: #6b7280; }
.dot.connecting { background: #d29922; }
.dot.open { background: #3fb950; }
.dot.closed { background: #f85149; }
.term-host { flex: 1; min-height: 0; padding: 4px 6px; }
</style>
