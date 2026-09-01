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
      <label class="field"><span>Tool call loop guard</span>
        <span style="display: flex; align-items: center; gap: 8px">
          <input type="checkbox" v-model="loopGuard" />
          <span class="hint">on identical rapid-fire tool calls: first ask the model if it's stuck, then stop</span>
        </span>
      </label>
      <label v-if="loopGuard" class="field"><span>Repeats to trigger</span>
        <input type="number" v-model.number="loopGuardRepeats" min="2" max="20" style="width: 6em" />
      </label>
      <label class="field"><span>Shell command timeout</span>
        <span style="display: flex; align-items: center; gap: 8px">
          <input
            type="number" v-model.number="shellTimeoutSeconds" min="5" step="5" style="width: 6em"
            :disabled="shellTimeoutUnlimited"
          />
          <span class="hint">seconds</span>
          <input type="checkbox" v-model="shellTimeoutUnlimited" id="shell-timeout-unlimited" />
          <label for="shell-timeout-unlimited" class="hint">no timeout — wait until the command exits</label>
        </span>
      </label>
      <p class="hint" style="margin: -6px 0 0">
        How long <code>system_run_shell</code> may sit completely silent before it hands control back
        with "still running" instead of continuing to wait. The command itself keeps going either way —
        this only controls when the tool call returns.
      </p>
      <label class="field"><span>Save full tool trace</span>
        <span style="display: flex; align-items: center; gap: 8px">
          <input type="checkbox" v-model="saveToolCalls" />
          <span class="hint">also save every tool call and its result to the chat file, not just the visible text — bigger files, full replay of what the agent did</span>
        </span>
      </label>
      <label class="field"><span>Save abandoned generations</span>
        <span style="display: flex; align-items: center; gap: 8px">
          <input type="checkbox" v-model="saveAttempts" />
          <span class="hint">also save the repetition guard's discarded attempts (full text) to the chat file — off by default, each one can run to several kB</span>
        </span>
      </label>
      <label class="field"><span>Resource addresses</span>
        <span style="display: flex; align-items: center; gap: 8px">
          <input type="checkbox" v-model="unifiedResources" />
          <span class="hint">announce scheme:// addresses (file://, sftp://, …) to the model, on top of ordinary project files — off by default so it can be measured with and without</span>
        </span>
      </label>
    </div>
    <div v-if="resourceSchemes.length" class="conn-card">
      <div class="conn-head"><span class="id">Resource schemes</span></div>
      <div class="s-sub">Per-scheme switches under the master toggle above. A scheme still registers when switched off here; it is only left out of the model's addresses and refuses any call.</div>
      <div class="ft-list">
        <CapabilityRow
          v-for="s in resourceSchemes" :key="s.scheme"
          :item="schemeAsCapability(s)"
          :disabled="!unifiedResources"
          @toggle="enabled => (s.enabled = enabled)" />
      </div>
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
import type { CapabilityDto, ResourceSchemeDto } from "../../protocol/types";
import CapabilityRow from "./CapabilityRow.vue";

const PERMS: { key: "permRead" | "permWrite" | "permShell" | "permInternet"; label: string }[] = [
  { key: "permRead", label: "Read files" },
  { key: "permWrite", label: "Write files" },
  { key: "permShell", label: "Run commands" },
  { key: "permInternet", label: "Internet" }
];

const mode = ref("");
const customPrompt = ref("");
const loopGuard = ref(false);
const loopGuardRepeats = ref(3);
const shellTimeoutSeconds = ref(120);
const shellTimeoutUnlimited = ref(false);
const saveToolCalls = ref(false);
const saveAttempts = ref(false);
const unifiedResources = ref(false);
const resourceSchemes = ref<ResourceSchemeDto[]>([]);
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
  loopGuard.value = p.loopGuard === true;
  loopGuardRepeats.value = p.loopGuardRepeats ?? 3;
  shellTimeoutUnlimited.value = (p.shellTimeoutSeconds ?? 120) <= 0;
  if (!shellTimeoutUnlimited.value) shellTimeoutSeconds.value = p.shellTimeoutSeconds ?? 120;
  saveToolCalls.value = p.saveToolCalls === true;
  saveAttempts.value = p.saveAttempts === true;
  unifiedResources.value = p.unifiedResources === true;
  resourceSchemes.value = p.resourceSchemes || [];
  perms.permRead = p.permRead || "";
  perms.permWrite = p.permWrite || "";
  perms.permShell = p.permShell || "";
  perms.permInternet = p.permInternet || "";
  hint.value = p.canPersist === false ? "no .spla project — session-only" : "";
  lastTheme = p.theme || "";
  lastDensity = p.density || "";
});
onUnmounted(off);

/** Renders a scheme row through the shared CapabilityRow component — same row grammar as the
 * Built-in/Skills capability lists, so the panel reads as one thing rather than three. */
function schemeAsCapability(s: ResourceSchemeDto): CapabilityDto {
  return {
    id: s.scheme,
    kind: "builtin",
    name: `${s.scheme}://`,
    description: `${s.summary}\nverbs: ${s.verbs.join(", ")}`,
    enabled: s.enabled
  };
}

function save(): Promise<void> {
  return new Promise((resolve, reject) => {
    const timer = window.setTimeout(() => { offRes(); reject(new Error("save timed out")); }, 8000);
    const offRes = client.on("agent.result", () => { clearTimeout(timer); offRes(); resolve(); });
    const ok = client.send("agent.save", {
      mode: mode.value,
      customPrompt: customPrompt.value,
      loopGuard: loopGuard.value,
      loopGuardRepeats: loopGuardRepeats.value,
      shellTimeoutSeconds: shellTimeoutUnlimited.value ? 0 : shellTimeoutSeconds.value,
      saveToolCalls: saveToolCalls.value,
      saveAttempts: saveAttempts.value,
      unifiedResources: unifiedResources.value,
      resourceSchemeSwitches: resourceSchemes.value.map(s => ({ scheme: s.scheme, enabled: s.enabled })),
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

<style scoped>
.s-sub { font-size: var(--fs-xs); color: var(--muted); margin: -2px 0 8px; }
.ft-list { display: flex; flex-direction: column; gap: var(--gap, 8px); }
</style>
