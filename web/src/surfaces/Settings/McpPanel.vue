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
  </div>
</template>

<script setup lang="ts">
import { onUnmounted, ref } from "vue";
import { client } from "../../protocol/SplaClient";

const enabled = ref(false);
const port = ref<number | null>(null);
const hint = ref("");

const off = client.on("mcp.result", p => {
  enabled.value = p.enabled !== false;
  port.value = p.port ?? null;
  hint.value = p.canPersist === false ? "no .spla project — session-only" : "";
});
onUnmounted(off);

function save(): Promise<void> {
  return new Promise((resolve, reject) => {
    const timer = window.setTimeout(() => { offRes(); reject(new Error("save timed out")); }, 8000);
    const offRes = client.on("mcp.result", () => { clearTimeout(timer); offRes(); resolve(); });
    const ok = client.send("mcp.save", { enabled: enabled.value, port: port.value || null });
    if (!ok) { clearTimeout(timer); offRes(); reject(new Error("socket closed")); }
  });
}

defineExpose({ save });
</script>
