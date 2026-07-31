<template>
  <div class="s-panel" data-tab="skills">
    <div class="s-head">
      <b>Skills</b>
      <span class="hint">{{ hint }}</span>
      <span class="grow"></span>
      <label class="filter"><input type="checkbox" v-model="showUnavailable"> show unavailable</label>
    </div>

    <div v-if="!skills.length" class="notice">
      no skills found — drop a .md file into one of the folders below
    </div>

    <!-- Grouped by source, not flattened: the list is the one that grows without bound (a user can
         write any number of skills), and "where did this come from" is the first question about a
         skill you did not write yourself. -->
    <div v-for="group in groups" :key="group.source.id" class="sk-group">
      <div class="sk-group-head" @click="toggleGroup(group.source.id)">
        <span class="chev">{{ isOpen(group.source.id) ? "▾" : "▸" }}</span>
        <b>{{ group.source.label }}</b>
        <span class="sk-id">{{ group.source.id }}</span>
        <span v-if="group.source.trust !== 'Trusted'" class="sk-untrusted">untrusted</span>
        <span class="grow"></span>
        <!-- The folder is the actionable part of a skill source: it is where you put a new .md file. -->
        <span v-if="group.source.path" class="sk-path" :title="group.source.path">{{ group.source.path }}</span>
        <span class="sk-count">{{ group.items.length }}</span>
      </div>

      <div v-if="isOpen(group.source.id)" class="sk-items">
        <div v-if="!group.items.length" class="sk-empty">nothing here</div>

        <CapabilityRow
          v-for="skill in group.items" :key="skill.id"
          :item="skill"
          :badge="skill.preloaded ? 'preloaded' : undefined"
          :disabled="skill.state === 'Shadowed'"
          @toggle="enabled => onToggle(skill, enabled)">
          <template #actions>
            <!-- The missing piece is almost always a disabled plugin, so offer the jump rather than
                 leaving the user to work out which tab fixes it. -->
            <button
              v-for="plugin in skill.missingPlugins || []" :key="plugin"
              class="btn tiny" @click="emit('open-plugin', plugin)">
              enable {{ plugin }}
            </button>
          </template>
        </CapabilityRow>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onUnmounted, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import { projectEnvelope } from "../../state/project";
import type { CapabilityDto, SkillSourceDto } from "../../protocol/types";
import CapabilityRow from "./CapabilityRow.vue";

const emit = defineEmits<{ (e: "open-plugin", pluginId: string): void }>();

const skills = ref<CapabilityDto[]>([]);
const sources = ref<SkillSourceDto[]>([]);
const hint = ref("");
const showUnavailable = ref(true);
const collapsed = ref<Set<string>>(new Set());

function isOpen(id: string) { return !collapsed.value.has(id); }
function toggleGroup(id: string) {
  const next = new Set(collapsed.value);
  next.has(id) ? next.delete(id) : next.add(id);
  collapsed.value = next;
}

const groups = computed(() =>
  sources.value.map(source => ({
    source,
    items: skills.value.filter(s =>
      s.source === source.id && (showUnavailable.value || s.state === "Available"))
  })));

function onToggle(skill: CapabilityDto, enabled: boolean) {
  skill.enabled = enabled;
}

const off = client.on("skills.result", p => {
  skills.value = p.skills || [];
  sources.value = p.sources || [];
  hint.value = p.canPersist === false
    ? "no .spla project — session-only"
    : `${skills.value.filter(s => s.state === "Available").length} of ${skills.value.length} available`;
});
onUnmounted(off);

// Toggling a plugin changes which skills exist and which are blocked, and the server resolves that
// while handling plugins.save — so re-ask rather than leave a stale list on screen.
const offPlugins = client.on("plugins.result", () => {
  client.send("skills.get", undefined, projectEnvelope());
});
onUnmounted(offPlugins);

function save(): Promise<void> {
  return new Promise((resolve, reject) => {
    const timer = window.setTimeout(() => { offRes(); reject(new Error("save timed out")); }, 8000);
    const offRes = client.on("skills.result", () => { clearTimeout(timer); offRes(); resolve(); });
    const ok = client.send("skills.save", { skills: skills.value }, projectEnvelope());
    if (!ok) { clearTimeout(timer); offRes(); reject(new Error("socket closed")); }
  });
}

defineExpose({ save });
</script>

<style scoped>
.s-head .grow { flex: 1; }
.filter { font-size: var(--fs-xs); color: var(--muted); display: flex; align-items: center; gap: 4px; }
.sk-group { display: flex; flex-direction: column; gap: var(--gap, 8px); margin-bottom: var(--gap, 8px); }
.sk-group-head { display: flex; align-items: center; gap: 6px; cursor: pointer;
  font-size: var(--fs-sm); padding: 2px 4px; }
.sk-group-head:hover { background: color-mix(in srgb, var(--text) 4%, transparent); }
.sk-id { font-family: var(--mono); font-size: var(--fs-xs); color: var(--muted); }
.sk-untrusted { font-size: var(--fs-xs); color: var(--danger, #f85149); }
.sk-count { font-size: var(--fs-xs); color: var(--muted); }
.sk-path { font-family: var(--mono); font-size: var(--fs-xs); color: var(--muted);
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 50%; }
.sk-items { display: flex; flex-direction: column; gap: var(--gap, 8px); padding-left: 14px; }
.sk-empty { font-family: var(--mono); font-size: var(--fs-xs); color: var(--muted); padding: 2px 8px; }
.chev { color: var(--muted); font-size: var(--fs-xs); width: 12px; text-align: center; }
.grow { flex: 1; }
.btn.tiny { font-size: var(--fs-xs); padding: 1px 6px; }
</style>
