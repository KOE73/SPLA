<template>
  <div class="s-panel" data-tab="agent">
    <div class="s-head"><b>Agent</b><span class="hint">{{ hint }}</span></div>
    <div class="conn-card">
      <label class="field"><span>Default mode</span>
        <select v-model="mode">
          <option v-for="m in modes" :key="m" :value="m">{{ m }}</option>
        </select>
      </label>
      <label class="field col"><span>Custom prompt</span>
        <textarea v-model="customPrompt" rows="4" placeholder="Appended to the system prompt for every chat"></textarea>
      </label>
    </div>
    <div class="conn-card">
      <div class="conn-head"><span class="id">Permissions</span></div>
      <label v-for="p in PERMS" :key="p.key" class="field"><span>{{ p.label }}</span>
        <select v-model="perms[p.key]">
          <option value="">(mode default)</option>
          <option value="allow">allow</option>
          <option value="ask">ask</option>
          <option value="deny">deny</option>
        </select>
      </label>
    </div>
    <div class="conn-card">
      <div class="conn-head"><span class="id">File association</span></div>
      <p class="hint">Associate .spla project files with this app in Windows Explorer, so double-clicking opens them.</p>
      <button class="btn" type="button" @click="registerAssociation" :disabled="registering">{{ assocLabel }}</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onUnmounted, reactive, ref } from "vue";
import { client } from "../../protocol/SplaClient";

const PERMS: { key: "permRead" | "permWrite" | "permShell" | "permInternet"; label: string }[] = [
  { key: "permRead", label: "Read files" },
  { key: "permWrite", label: "Write files" },
  { key: "permShell", label: "Run commands" },
  { key: "permInternet", label: "Internet" }
];

const mode = ref("");
const customPrompt = ref("");
const modes = ref<string[]>([]);
const perms = reactive<Record<string, string>>({ permRead: "", permWrite: "", permShell: "", permInternet: "" });
const hint = ref("");
const registering = ref(false);
const assocLabel = ref("Register .spla File Association");
// Theme/density are owned by AppearancePanel, but the server bundles them into agent.result —
// stash them so save() doesn't clobber appearance with stale values.
let lastTheme = "";
let lastDensity = "";

const off = client.on("agent.result", p => {
  modes.value = p.modes || [];
  mode.value = p.mode || "";
  customPrompt.value = p.customPrompt || "";
  perms.permRead = p.permRead || "";
  perms.permWrite = p.permWrite || "";
  perms.permShell = p.permShell || "";
  perms.permInternet = p.permInternet || "";
  hint.value = p.canPersist === false ? "no .spla project — session-only" : "";
  lastTheme = p.theme || "";
  lastDensity = p.density || "";
});
onUnmounted(off);

function save(): Promise<void> {
  return new Promise((resolve, reject) => {
    const timer = window.setTimeout(() => { offRes(); reject(new Error("save timed out")); }, 8000);
    const offRes = client.on("agent.result", () => { clearTimeout(timer); offRes(); resolve(); });
    const ok = client.send("agent.save", {
      mode: mode.value,
      customPrompt: customPrompt.value,
      permRead: perms.permRead, permWrite: perms.permWrite, permShell: perms.permShell, permInternet: perms.permInternet,
      theme: lastTheme, density: lastDensity
    });
    if (!ok) { clearTimeout(timer); offRes(); reject(new Error("socket closed")); }
  });
}

async function registerAssociation() {
  registering.value = true;
  try {
    const p = await client.invoke<{ ok?: boolean; message?: string }>("system.register_association");
    assocLabel.value = p.message || (p.ok ? "Registered" : "Failed");
  } catch (e) {
    assocLabel.value = e instanceof Error ? e.message : "Failed";
  } finally {
    registering.value = false;
    setTimeout(() => { assocLabel.value = "Register .spla File Association"; }, 3000);
  }
}

defineExpose({ save });
</script>
