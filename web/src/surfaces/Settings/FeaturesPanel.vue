<template>
  <div class="s-panel" data-tab="features">
    <div class="s-head"><b>Built-in</b><span class="hint">{{ hint }}</span></div>

    <div class="ft-list">
      <div v-if="!features.length" class="notice">no capabilities reported</div>

      <CapabilityRow
        v-for="feature in features" :key="feature.id"
        :item="feature"
        :badge="badgeFor(feature)"
        @toggle="enabled => onToggle(feature, enabled)" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { onUnmounted, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import { projectEnvelope } from "../../state/project";
import type { CapabilityDto } from "../../protocol/types";
import CapabilityRow from "./CapabilityRow.vue";

const features = ref<CapabilityDto[]>([]);
const hint = ref("");

function badgeFor(feature: CapabilityDto): string | undefined {
  return feature.requires?.length ? `needs ${feature.requires.join(", ")}` : undefined;
}

/**
 * Dependencies are pulled in here as well as on the server, so the checkbox the user just clicked
 * agrees with what will be saved. Turning a capability OFF also turns off anything that depends on
 * it — the server would resolve those back on otherwise, and a switch that silently undoes itself is
 * worse than one that explains what it took with it.
 */
function onToggle(feature: CapabilityDto, enabled: boolean) {
  feature.enabled = enabled;

  if (enabled) {
    for (const dep of feature.requires || []) {
      const target = features.value.find(f => f.id === dep);
      if (target && !target.enabled) onToggle(target, true);
    }
    return;
  }

  for (const dependent of features.value)
    if (dependent.enabled && dependent.requires?.includes(feature.id))
      onToggle(dependent, false);
}

const off = client.on("features.result", p => {
  features.value = p.features || [];
  const bits: string[] = [];
  if (p.canPersist === false) bits.push("no .spla project — session-only");
  if (p.restartToApply) bits.push("applies on next launch");
  hint.value = bits.join(" · ");
});
onUnmounted(off);

function save(): Promise<void> {
  return new Promise((resolve, reject) => {
    const timer = window.setTimeout(() => { offRes(); reject(new Error("save timed out")); }, 8000);
    const offRes = client.on("features.result", () => { clearTimeout(timer); offRes(); resolve(); });
    const ok = client.send("features.save", { features: features.value }, projectEnvelope());
    if (!ok) { clearTimeout(timer); offRes(); reject(new Error("socket closed")); }
  });
}

defineExpose({ save });
</script>

<style scoped>
.ft-list { display: flex; flex-direction: column; gap: var(--gap, 8px); }
</style>
