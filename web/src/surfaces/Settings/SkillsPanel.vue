<template>
  <div class="s-panel" data-tab="skills">
    <div class="s-head">
      <b>Skills</b>
      <span class="hint">{{ hint }}</span>
      <span class="grow"></span>
      <input class="sk-search" v-model="search" type="text" placeholder="filter by id or text" spellcheck="false">
      <label class="filter"><input type="checkbox" v-model="showUnavailable"> show unavailable</label>
    </div>

    <div v-if="!skills.length" class="notice">
      no skills found — drop a .md file into one of the folders below
    </div>

    <!-- The whole vocabulary, always, not a search box: the drift a normaliser cannot catch is two
         words for one subject ("ssh" and "ssh-access"), and seeing them side by side is the only
         thing that makes it noticeable. Selecting several intersects — the same set arithmetic the
         model gets, so what you see here is what it would find. -->
    <div v-if="vocabulary.length" class="sk-facets">
      <button
        v-for="tag in vocabulary" :key="tag.tag"
        class="sk-tag" :class="{ on: selectedTags.has(tag.tag) }"
        @click="toggleTag(tag.tag)">
        {{ tag.tag }} <span class="sk-tag-n">{{ tag.count }}</span>
      </button>
      <button v-if="selectedTags.size" class="sk-tag clear" @click="selectedTags = new Set()">clear</button>
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
        <!-- Why a switched-on skill is never chosen: the source is not in the model's catalog. Read
             only — the level is a .spla decision, not a panel toggle. -->
        <span v-if="group.source.level && group.source.level !== 'OnShelf'"
              class="sk-level" :title="levelHint(group.source.level)">{{ levelLabel(group.source.level) }}</span>
        <span class="grow"></span>
        <!-- Acts on what is VISIBLE, not on the whole source: with a filter applied, "all" meaning
             something other than what you are looking at is how people switch off things they never
             saw. Superseded rows are skipped — their toggle does nothing anyway. -->
        <button v-if="group.items.length > 1" class="btn tiny"
                :title="`Switch on the ${toggleable(group).length} shown skill(s)`"
                @click.stop="setGroup(group, true)">all on</button>
        <button v-if="group.items.length > 1" class="btn tiny"
                :title="`Switch off the ${toggleable(group).length} shown skill(s)`"
                @click.stop="setGroup(group, false)">all off</button>
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
          :disabled="skill.state === 'Superseded'"
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
const search = ref("");
const collapsed = ref<Set<string>>(new Set());
const selectedTags = ref<Set<string>>(new Set());

/** All terms must match, in any order, against id and description — same shape as the model picker's
 *  filter, so one habit works in both places. */
function matchesSearch(skill: CapabilityDto) {
  const terms = search.value.trim().toLowerCase().split(/\s+/).filter(Boolean);
  if (!terms.length) return true;
  const haystack = `${skill.id} ${skill.description ?? ""}`.toLowerCase();
  return terms.every(t => haystack.includes(t));
}

type Group = { source: SkillSourceDto; items: CapabilityDto[] };

/** A Superseded row's toggle changes nothing, so a bulk action must not pretend otherwise. */
function toggleable(group: Group) {
  return group.items.filter(s => s.state !== "Superseded");
}

function setGroup(group: Group, enabled: boolean) {
  for (const skill of toggleable(group)) skill.enabled = enabled;
}

/** Every tag in the fond with its count, commonest first — the same order the prompt prints. */
const vocabulary = computed(() => {
  const counts = new Map<string, number>();
  for (const skill of skills.value)
    for (const tag of skill.tags || []) counts.set(tag, (counts.get(tag) || 0) + 1);

  return [...counts.entries()]
    .map(([tag, count]) => ({ tag, count }))
    .sort((a, b) => b.count - a.count || a.tag.localeCompare(b.tag));
});

function toggleTag(tag: string) {
  const next = new Set(selectedTags.value);
  next.has(tag) ? next.delete(tag) : next.add(tag);
  selectedTags.value = next;
}

/** Intersection, not union: selecting two tags narrows. Deterministic set arithmetic is the whole
 *  reason tags were chosen over a fixed rubric. */
function matchesTags(skill: CapabilityDto) {
  if (!selectedTags.value.size) return true;
  const tags = new Set(skill.tags || []);
  return [...selectedTags.value].every(t => tags.has(t));
}

const LEVELS: Record<string, [string, string]> = {
  OutOfCatalog: ["not in catalog", "The model is told nothing about these — not the id, not the tags. Only a person can hand one to a chat."],
  Findable: ["findable only", "Reachable through skill_find, absent from the prompt entirely."],
  InCatalog: ["by subject", "Tags reach the prompt; descriptions do not. The model asks for specifics."],
  OnShelf: ["listed", "Id and description in every request."]
};

function levelLabel(level: string) { return LEVELS[level]?.[0] ?? level; }
function levelHint(level: string) { return LEVELS[level]?.[1] ?? ""; }

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
      s.source === source.id &&
      (showUnavailable.value || s.state === "Available") &&
      matchesTags(s) && matchesSearch(s))
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
.sk-level { font-size: var(--fs-xs); color: var(--muted);
  border: 1px solid color-mix(in srgb, var(--text) 18%, transparent); border-radius: 3px; padding: 0 4px; }
.sk-search { background: transparent; color: var(--text); font-size: var(--fs-xs);
  border: 1px solid color-mix(in srgb, var(--text) 18%, transparent); border-radius: 3px;
  padding: 1px 6px; width: 12em; }
.sk-facets { display: flex; flex-wrap: wrap; gap: 4px; margin-bottom: var(--gap, 8px); }
.sk-tag { font-size: var(--fs-xs); font-family: var(--mono); cursor: pointer;
  background: transparent; color: var(--muted); padding: 1px 6px; border-radius: 3px;
  border: 1px solid color-mix(in srgb, var(--text) 18%, transparent); }
.sk-tag:hover { background: color-mix(in srgb, var(--text) 6%, transparent); }
.sk-tag.on { color: var(--text); border-color: var(--accent, currentColor);
  background: color-mix(in srgb, var(--accent, var(--text)) 14%, transparent); }
.sk-tag-n { opacity: 0.6; }
.sk-tag.clear { font-style: italic; }
.sk-count { font-size: var(--fs-xs); color: var(--muted); }
.sk-path { font-family: var(--mono); font-size: var(--fs-xs); color: var(--muted);
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 50%; }
.sk-items { display: flex; flex-direction: column; gap: var(--gap, 8px); padding-left: 14px; }
.sk-empty { font-family: var(--mono); font-size: var(--fs-xs); color: var(--muted); padding: 2px 8px; }
.chev { color: var(--muted); font-size: var(--fs-xs); width: 12px; text-align: center; }
.grow { flex: 1; }
.btn.tiny { font-size: var(--fs-xs); padding: 1px 6px; }
</style>
