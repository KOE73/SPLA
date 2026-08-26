<template>
  <div class="s-panel" data-tab="mcp">
    <div class="s-head"><b>MCP</b><span class="hint">{{ hint }}</span></div>
    <div class="conn-card">
      <div class="conn-head"><span class="id">HTTP endpoint</span></div>
      <p class="hint">
        When this project is served (<code>spla serve</code>), <code>POST /mcp</code> lets any number
        of MCP clients share the one running instance instead of each taking its own writer lease —
        see MCP_USAGE.md. Off by default is the strict case: no second head over HTTP, only the
        writer that opened the project.
      </p>
      <label class="field"><span>Offer /mcp</span>
        <span style="display: flex; align-items: center; gap: 8px">
          <input type="checkbox" v-model="enabled" />
          <span class="hint">maps POST /mcp on the next `spla serve` start</span>
        </span>
      </label>
      <label class="field"><span>Fixed port</span>
        <input type="number" v-model.number="port" min="1" max="65535" placeholder="ephemeral" style="width: 8em" />
      </label>
      <p class="hint">
        A fixed port means a client can hardcode <code>http://127.0.0.1:&lt;port&gt;/mcp</code>
        instead of reading the ephemeral one out of the instance lock file each time. Leave blank for
        the usual OS-assigned port. An explicit <code>--port</code> on the <code>spla serve</code>
        command line still wins over this.
      </p>
      <p class="hint" style="color: var(--accent)">
        Takes effect on the next <code>spla serve</code> start — a running instance already has its
        listener bound and does not pick this up live.
      </p>
    </div>

    <div class="conn-card">
      <div class="conn-head"><span class="id">Connected servers</span><span class="hint">{{ serversHint }}</span></div>
      <p class="hint">
        A grant is taken on the whole server, not on one tool inside it — a foreign tool declares
        none of our own Scope/Effect/Risk axes, so there is nothing narrower to grant against. This is
        a deliberately naive first wave, not a security model (see ADR_20260826_service_mcp-client).
      </p>
      <p class="hint" style="color: var(--accent)">
        Adding, removing or editing a server here takes effect on the next <code>spla serve</code>/process
        start — servers connect once, at startup. The "Reconnect" button below is the one thing that is
        live: it retries a server this process already attempted.
      </p>

      <div class="pl-list">
        <div v-if="!servers.length" class="notice">no servers configured</div>
        <div v-for="s in servers" :key="s._key" class="pl-card" :class="{ open: isOpen(s._key) }">
          <div class="pl-row" @click="toggle(s._key)">
            <input type="checkbox" v-model="s.enabled" @click.stop>
            <b class="pl-name">{{ s.name || s.id || "(new server)" }}</b>
            <span class="ver">{{ s.transport }}</span>
            <span class="dot" :class="stateClass(s.state)"></span>
            <span class="state-word" :class="stateClass(s.state)">{{ s.state || "never connected" }}</span>
            <span v-if="!isOpen(s._key)" class="pl-sum">{{ summary(s) }}</span>
            <span class="grow"></span>
            <button v-if="s.state" class="btn ghost" type="button" :disabled="reconnecting === s.id"
                    @click.stop="reconnect(s)">
              {{ reconnecting === s.id ? "reconnecting…" : "Reconnect" }}
            </button>
            <span class="chev">{{ isOpen(s._key) ? "▾" : "▸" }}</span>
          </div>

          <div v-if="isOpen(s._key)" class="pl-body">
            <label class="field col"><span>Id</span>
              <input v-model="s.id" placeholder="ghmcp" class="mono">
              <span class="hint">Prefixes every tool this server offers (<code>{{ s.id || "id" }}_tool_name</code>).
                Renaming an already-connected server breaks its stored grants and history — treat it as
                load-bearing, not cosmetic.</span>
            </label>
            <label class="field col"><span>Name</span><input v-model="s.name" placeholder="falls back to id"></label>
            <label class="field col"><span>Transport</span>
              <select v-model="s.transport">
                <option value="stdio">stdio</option>
                <option value="http">http</option>
              </select>
            </label>

            <template v-if="s.transport === 'stdio'">
              <label class="field col"><span>Command</span><input v-model="s.command" placeholder="npx" class="mono"></label>
              <label class="field col"><span>Args (one per line)</span>
                <textarea :value="(s.args || []).join('\n')" rows="2" class="mono"
                          @change="setArgs(s, ($event.target as HTMLTextAreaElement).value)"></textarea>
              </label>
              <label class="field col"><span>Working directory</span><input v-model="s.cwd" placeholder="inherit"></label>
              <div class="field col">
                <span>Environment</span>
                <KvRows :rows="s.env" scope="project" @update:rows="s.env = $event" />
              </div>
            </template>

            <template v-else>
              <label class="field col"><span>URL</span><input v-model="s.url" placeholder="https://example.test/mcp"></label>
              <div class="field col">
                <span>Headers</span>
                <KvRows :rows="s.headers" scope="project" @update:rows="s.headers = $event" />
              </div>
            </template>

            <label class="field col"><span>Description</span><textarea v-model="s.description" rows="2"></textarea></label>

            <label class="field col">
              <span>Origin</span>
              <select v-model="s.origin">
                <option value="unnamed">unnamed (default) — results raise the chat's doubt flag</option>
                <option value="named">named — vouch for what this server returns</option>
              </select>
              <span class="hint">The operator named the pipe, not what flows through it — mark named
                only when you vouch for what this server's tools actually return, the same act as
                adding a host to trusted_domains.</span>
            </label>

            <label class="field col">
              <span>Tools in context</span>
              <select v-model="s.level">
                <option value="">follow the enable flag</option>
                <option value="enabled">always — full definitions in every request</option>
                <option value="agent_demand">announced — one line; the agent loads it when needed</option>
                <option value="skill_demand">on skill demand — nothing until a skill requires it</option>
                <option value="disabled">never — the set does not exist for the model</option>
              </select>
            </label>

            <p v-if="s.lastError" class="hint" style="color: var(--danger, #f85149)">Last error: {{ s.lastError }}</p>
            <p class="hint">Tools registered: {{ s.toolCount ?? 0 }}</p>

            <div class="field col">
              <button class="btn ghost danger" type="button" @click="remove(s._key)">✕ remove server</button>
            </div>
          </div>
        </div>
      </div>

      <button class="btn ghost" type="button" @click="add">＋ Add server</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onUnmounted, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import type { McpServerDto } from "../../protocol/types";
import KvRows from "./KvRows.vue";

const enabled = ref(false);
const port = ref<number | null>(null);
const hint = ref("");
const serversHint = ref("");

const off = client.on("mcp.result", p => {
  enabled.value = p.enabled !== false;
  port.value = p.port ?? null;
  hint.value = p.canPersist === false ? "no .spla project — session-only" : "";
});
onUnmounted(off);

// ── Connected servers ─────────────────────────────────────────────────────

/** Local editing shape: `_key` is a stable v-for key independent of the (user-editable) id, so
 *  renaming the id mid-edit does not remount the row and lose focus/open state. */
interface EditableServer extends McpServerDto {
  _key: string;
}

function newKey(): string {
  return Math.random().toString(36).slice(2);
}

function toEditable(d: McpServerDto, key?: string): EditableServer {
  return {
    ...d,
    id: d.id, name: d.name, enabled: d.enabled !== false, transport: d.transport || "stdio",
    command: d.command, args: d.args ? [...d.args] : [], cwd: d.cwd,
    env: d.env ? { ...d.env } : {}, url: d.url, headers: d.headers ? { ...d.headers } : {},
    description: d.description, origin: d.origin === "named" ? "named" : "unnamed", level: d.level,
    state: d.state, lastError: d.lastError, toolCount: d.toolCount,
    _key: key ?? d.id ?? newKey()
  };
}

const servers = ref<EditableServer[]>([]);
const open = ref<Set<string>>(new Set());
const reconnecting = ref<string | null>(null);

function isOpen(key: string) { return open.value.has(key); }
function toggle(key: string) {
  const next = new Set(open.value);
  next.has(key) ? next.delete(key) : next.add(key);
  open.value = next;
}

function setArgs(s: EditableServer, text: string) {
  s.args = text.split("\n").map(x => x.trim()).filter(Boolean);
}

function stateClass(state?: string | null): string {
  if (state === "Ready") return "ok";
  if (state === "Failed") return "danger";
  return "muted";
}

function summary(s: EditableServer): string {
  const bits: string[] = [];
  if (s.origin === "named") bits.push("named");
  if (s.level) bits.push(`tools: ${s.level}`);
  if (s.toolCount) bits.push(`${s.toolCount} tool${s.toolCount === 1 ? "" : "s"}`);
  return bits.join(" · ");
}

function add() {
  const s = toEditable({ id: "", enabled: true, transport: "stdio", origin: "unnamed" });
  servers.value = [...servers.value, s];
  open.value = new Set(open.value).add(s._key);
}

function remove(key: string) {
  servers.value = servers.value.filter(s => s._key !== key);
  const next = new Set(open.value);
  next.delete(key);
  open.value = next;
}

function reconnect(s: EditableServer) {
  if (!s.id) return;
  reconnecting.value = s.id;
  const off2 = client.on("mcp.servers.result", () => { off2(); reconnecting.value = null; });
  const ok = client.send("mcp.servers.reconnect", { serverId: s.id });
  if (!ok) { off2(); reconnecting.value = null; }
}

// Every reply — the initial mcp.servers.get answer AND every later broadcast (an explicit save
// elsewhere, or McpServersChanged firing because a background connect attempt finished) — goes
// through the same merge: known servers are updated in place, new ones are added, nothing is ever
// removed just because a broadcast did not mention it (that only happens through the explicit
// remove button + Save). A row nobody has expanded gets its whole config refreshed from the wire,
// since nothing local could disagree with it; a row currently open — someone mid-edit — only has its
// live-status fields (state/lastError/toolCount) refreshed, so a background reconnect updates the
// dot without stomping on unsaved keystrokes.
const offServers = client.on("mcp.servers.result", p => {
  serversHint.value = p.canPersist === false ? "no .spla project — session-only" : "";
  for (const d of p.servers || []) {
    const existing = servers.value.find(s => s.id === d.id);
    if (!existing) {
      servers.value = [...servers.value, toEditable(d)];
      continue;
    }
    existing.state = d.state ?? null;
    existing.lastError = d.lastError ?? null;
    existing.toolCount = d.toolCount ?? 0;
    if (!isOpen(existing._key)) {
      const key = existing._key;
      Object.assign(existing, toEditable(d, key));
    }
  }
});
onUnmounted(offServers);

function save(): Promise<void> {
  return Promise.all([saveHttp(), saveServers()]).then(() => {});
}

function saveHttp(): Promise<void> {
  return new Promise((resolve, reject) => {
    const timer = window.setTimeout(() => { offRes(); reject(new Error("save timed out")); }, 8000);
    const offRes = client.on("mcp.result", () => { clearTimeout(timer); offRes(); resolve(); });
    const ok = client.send("mcp.save", { enabled: enabled.value, port: port.value || null });
    if (!ok) { clearTimeout(timer); offRes(); reject(new Error("socket closed")); }
  });
}

function saveServers(): Promise<void> {
  const payload = servers.value
    .filter(s => s.id.trim())
    .map(s => ({
      id: s.id.trim(), name: s.name || undefined, enabled: s.enabled, transport: s.transport,
      command: s.transport === "stdio" ? s.command : undefined,
      args: s.transport === "stdio" ? s.args : undefined,
      cwd: s.transport === "stdio" ? s.cwd : undefined,
      env: s.transport === "stdio" && s.env && Object.keys(s.env).length ? s.env : undefined,
      url: s.transport === "http" ? s.url : undefined,
      headers: s.transport === "http" && s.headers && Object.keys(s.headers).length ? s.headers : undefined,
      description: s.description || undefined,
      origin: s.origin, level: s.level || undefined
    }));
  return new Promise((resolve, reject) => {
    const timer = window.setTimeout(() => { offRes(); reject(new Error("save timed out")); }, 8000);
    const offRes = client.on("mcp.servers.result", () => { clearTimeout(timer); offRes(); resolve(); });
    const ok = client.send("mcp.servers.save", { servers: payload });
    if (!ok) { clearTimeout(timer); offRes(); reject(new Error("socket closed")); }
  });
}

defineExpose({ save });
</script>

<style scoped>
.pl-list { display: flex; flex-direction: column; gap: var(--gap, 8px); margin: 8px 0; }
.pl-card { border: 1px solid var(--border); border-radius: var(--radius, 7px); background: var(--elevated); }
.pl-row { display: flex; align-items: center; gap: 8px; padding: 4px 8px; cursor: pointer; min-height: 26px; }
.pl-row:hover { background: color-mix(in srgb, var(--text) 4%, transparent); }
.pl-name { font-size: var(--fs-sm); }
.ver { font-family: var(--mono); font-size: var(--fs-xs); color: var(--muted); }
.pl-sum { font-size: var(--fs-xs); color: var(--muted); margin-left: 8px;
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 35%; }
.grow { flex: 1; }
.chev { color: var(--muted); font-size: var(--fs-xs); width: 12px; text-align: center; }
.pl-body { display: flex; flex-direction: column; gap: 6px; padding: 2px 8px 8px;
  border-top: 1px solid color-mix(in srgb, var(--border) 60%, transparent); }
.dot { width: 8px; height: 8px; border-radius: 50%; background: var(--muted); flex: 0 0 auto; }
.dot.ok { background: var(--ok, #3fb950); }
.dot.danger { background: var(--danger, #f85149); }
.dot.muted { background: var(--muted); }
.state-word { font-size: var(--fs-xs); }
.state-word.ok { color: var(--ok, #3fb950); }
.state-word.danger { color: var(--danger, #f85149); }
.state-word.muted { color: var(--muted); }
.btn.danger { color: var(--danger, #f85149); }
</style>
