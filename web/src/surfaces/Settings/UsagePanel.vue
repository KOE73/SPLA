<template>
  <div class="s-panel" data-tab="usage">
    <div class="s-head"><b>Token Usage</b><span class="hint">real, provider-reported counts</span></div>
    <div class="conn-card" v-for="s in scopes" :key="s.label">
      <div class="conn-head"><span class="id">{{ s.label }}</span><span class="state">{{ s.value.totalTokens.toLocaleString() }} tokens</span></div>
      <div class="usage-bar">
        <div class="usage-bar-in" :style="{ width: inputShare(s.value) + '%' }"></div>
      </div>
      <div class="usage-rows">
        <span>in: {{ s.value.promptTokens.toLocaleString() }}</span>
        <span>out: {{ s.value.completionTokens.toLocaleString() }}</span>
        <span>turns: {{ s.value.turns.toLocaleString() }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import type { TokenUsageScope } from "../../protocol/types";

const empty = (): TokenUsageScope => ({ promptTokens: 0, completionTokens: 0, turns: 0, totalTokens: 0 });

const session = ref<TokenUsageScope>(empty());
const project = ref<TokenUsageScope>(empty());
const machine = ref<TokenUsageScope>(empty());

const scopes = computed(() => [
  { label: "Session", value: session.value },
  { label: "Project", value: project.value },
  { label: "Machine", value: machine.value }
]);

function inputShare(s: TokenUsageScope) {
  return s.totalTokens > 0 ? Math.round((s.promptTokens / s.totalTokens) * 100) : 0;
}

client.on("usage.result", p => {
  session.value = p.session;
  project.value = p.project;
  machine.value = p.machine;
});

function fetchUsage() { client.send("usage.get"); }
defineExpose({ fetchUsage });
client.on("welcome", fetchUsage);
fetchUsage();
</script>

<style scoped>
.usage-bar { height: 6px; border-radius: 3px; background: var(--bg-2, #2a2a2a); overflow: hidden; margin: 4px 0; }
.usage-bar-in { height: 100%; background: var(--accent, #5b8def); }
.usage-rows { display: flex; gap: 14px; font-size: var(--fs-xs); color: var(--muted); }
</style>
