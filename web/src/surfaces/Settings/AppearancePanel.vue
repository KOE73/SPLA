<template>
  <div class="s-panel" data-tab="appearance">
    <div class="s-head"><b>Appearance</b><span class="hint">saved to .spla project</span></div>
    <div class="conn-card">
      <div class="conn-head"><span class="id">Theme</span></div>
      <label class="field"><span>Color theme</span>
        <select v-model="theme" @change="saveAppearance">
          <option v-for="t in themes" :key="t" :value="t">{{ capitalize(t) }}</option>
        </select>
      </label>
      <label class="field"><span>UI density</span>
        <select v-model="density" @change="saveAppearance">
          <option v-for="d in densities" :key="d" :value="d">{{ densityLabel(d) }}</option>
        </select>
      </label>
    </div>
    <div class="conn-card">
      <div class="conn-head"><span class="id">Layout</span><span class="state" style="color:var(--muted);font-size:var(--fs-xs)">this device only</span></div>
      <button class="btn ghost" @click="resetDock">Reset panel layout</button>
    </div>
    <div class="hint">{{ hint }}</div>
  </div>
</template>

<script setup lang="ts">
import { onUnmounted, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import { projectEnvelope } from "../../state/project";
import { resetDock } from "../../dock/dockController";

const theme = ref(localStorage.getItem("spla.theme") || "dark");
const density = ref(localStorage.getItem("spla.density") || "norm");
const themes = ref<string[]>([theme.value]);
const densities = ref<string[]>([density.value]);
const hint = ref("");

function capitalize(s: string) { return s ? s[0].toUpperCase() + s.slice(1) : s; }
function densityLabel(d: string) { return ({ nano: "Nano", mini: "Mini", norm: "Normal", max: "Max" } as Record<string, string>)[d] || d; }

// Auto-applies: no Save step. Each pick persists to .spla and broadcasts appearance.changed,
// so every window updates live through one path — preview is the commit.
function saveAppearance() { client.send("appearance.save", { theme: theme.value, density: density.value }, projectEnvelope()); }

const off = client.on("agent.result", p => {
  themes.value = p.themes && p.themes.length ? p.themes : themes.value;
  densities.value = p.densities && p.densities.length ? p.densities : densities.value;
  theme.value = (p.theme || theme.value).toLowerCase();
  density.value = p.density || density.value;
  hint.value = p.canPersist === false ? "applies instantly · session-only" : "applies instantly · saved to .spla";
});
onUnmounted(off);
</script>
