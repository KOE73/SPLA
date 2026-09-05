<template>
  <div class="s-panel" data-tab="appearance">
    <div class="s-head"><b>{{ t('Appearance') }}</b><span class="hint">{{ t('saved to .spla project') }}</span></div>
    <div class="conn-card">
      <div class="conn-head"><span class="id">{{ t('Theme') }}</span></div>
      <label class="field"><span>{{ t('Color theme') }}</span>
        <select v-model="theme" @change="saveAppearance">
          <option v-for="name in themes" :key="name" :value="name">{{ capitalize(name) }}</option>
        </select>
      </label>
      <label class="field"><span>{{ t('UI density') }}</span>
        <select v-model="density" @change="saveAppearance">
          <option v-for="d in densities" :key="d" :value="d">{{ densityLabel(d) }}</option>
        </select>
      </label>
    </div>
    <div class="conn-card">
      <div class="conn-head"><span class="id">{{ t('Language') }}</span><span class="state" style="color:var(--muted);font-size:var(--fs-xs)">{{ t('yours, in every project') }}</span></div>
      <label class="field"><span>{{ t('Interface language') }}</span>
        <select :value="locale" @change="e => saveLanguage((e.target as HTMLSelectElement).value)">
          <option v-for="l in LOCALES" :key="l.id" :value="l.id">{{ l.label }}</option>
        </select>
      </label>
      <span class="hint">{{ t('Applies at once and is remembered for you, not for the project. English is the source text: anything not yet translated stays in English.') }}</span>
    </div>
    <div class="conn-card">
      <div class="conn-head"><span class="id">{{ t('Sessions') }}</span></div>
      <label class="field"><span>{{ t('Auto-open spawned sessions') }}</span>
        <span style="display: flex; align-items: center; gap: 8px">
          <input type="checkbox" v-model="autoOpenSubagents" @change="saveAppearance" />
          <span class="hint">{{ t("open a window by itself the moment a subagent's chat appears in the tree — off by default") }}</span>
        </span>
      </label>
    </div>
    <div class="conn-card">
      <div class="conn-head"><span class="id">{{ t('Layout') }}</span><span class="state" style="color:var(--muted);font-size:var(--fs-xs)">{{ t('this device only') }}</span></div>
      <button class="btn ghost" @click="resetDock">{{ t('Reset panel layout') }}</button>
    </div>
    <div class="hint">{{ t(hint) }}</div>
  </div>
</template>

<script setup lang="ts">
import { LOCALES, locale, t } from "../../i18n";
import { onUnmounted, ref } from "vue";
import { saveLanguage } from "../../state/appearance";
import { client } from "../../protocol/SplaClient";
import { resetDock } from "../../dock/dockController";

const theme = ref(localStorage.getItem("spla.theme") || "dark");
const density = ref(localStorage.getItem("spla.density") || "norm");
const themes = ref<string[]>([theme.value]);
const densities = ref<string[]>([density.value]);
const autoOpenSubagents = ref(false);
const hint = ref("");

// Auto-applies and auto-saves, like the theme and density directly above it — a reversible preference
// with a visible result, so the preview IS the commit (root AGENTS.md, "Auto-apply vs Save"). It is
// saved per person and not into .spla, so two people sharing a project each read in their own
// language; the store is the machine layer, because this page's localStorage does not survive a
// restart (see saveLanguage).

function capitalize(s: string) { return s ? s[0].toUpperCase() + s.slice(1) : s; }
function densityLabel(d: string) { return ({ nano: "Nano", mini: "Mini", norm: "Normal", max: "Max" } as Record<string, string>)[d] || d; }

// Auto-applies: no Save step. Each pick persists to .spla and broadcasts appearance.changed,
// so every window updates live through one path — preview is the commit.
function saveAppearance() {
  client.send("appearance.save", {
    theme: theme.value, density: density.value, autoOpenSubagents: autoOpenSubagents.value
  });
}

const off = client.on("agent.result", p => {
  themes.value = p.themes && p.themes.length ? p.themes : themes.value;
  densities.value = p.densities && p.densities.length ? p.densities : densities.value;
  theme.value = (p.theme || theme.value).toLowerCase();
  density.value = p.density || density.value;
  autoOpenSubagents.value = p.autoOpenSubagents === true;
  // Kept in English here and translated at render, so switching language re-renders it.
  hint.value = p.canPersist === false ? "applies instantly · session-only" : "applies instantly · saved to .spla";
});
onUnmounted(off);
</script>
