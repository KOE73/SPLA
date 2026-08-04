<template>
  <Teleport to="body">
    <div ref="popEl" class="skill-popup" :style="popStyle" @keydown="onKeydown">
      <div class="sp-filter">
        <input
          ref="filterEl"
          v-model="filter"
          type="text"
          placeholder="linux ssh"
          spellcheck="false"
        />
        <span class="sp-count">{{ shown.length }} / {{ available.length }}</span>
        <span class="sp-hint" :title="hint">?</span>
      </div>

      <!-- Tags are clickable rather than only typeable: the words are normalised and a person does
           not know them by heart, and the whole point of a vocabulary is being shown it. -->
      <div v-if="vocabulary.length" class="sp-tags">
        <button
          v-for="t in vocabulary" :key="t.tag"
          class="sp-tag" :class="{ on: selected.has(t.tag) }"
          @click.stop="toggleTag(t.tag)"
        >{{ t.tag }} <span class="sp-tag-n">{{ t.count }}</span></button>
      </div>

      <div ref="listEl" class="sp-list">
        <div
          v-for="(s, i) in shown"
          :key="s.id"
          class="sp-item"
          :data-active="i === active ? '1' : undefined"
          :title="s.description || undefined"
          @click="pick(s)"
          @mousemove="active = i"
        >
          <div class="sp-id">{{ s.id }}</div>
          <div v-if="s.description" class="sp-desc">{{ s.description }}</div>
          <div v-if="s.tags?.length" class="sp-item-tags">{{ s.tags.join(" · ") }}</div>
        </div>
        <div v-if="!shown.length" class="sp-empty">nothing matches</div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
/**
 * Hands a skill to the current chat — the loan desk.
 *
 * Lists every AVAILABLE skill, including those the model is never told about: an out-of-catalog
 * source is invisible to the model and fully visible to its owner, and that asymmetry is the reason
 * the level exists. Unavailable ones stay out with their reason in the settings panel, because
 * offering something that will be refused is worse than not offering it.
 *
 * The chat pays nothing for this. The catalog is suppressed while a skill is active, so a chat with
 * a handed-out skill carries no index at all.
 */
import { computed, nextTick, onMounted, onBeforeUnmount, ref, watch } from "vue";
import type { CSSProperties } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";
import type { CapabilityDto } from "../protocol/types";

const props = defineProps<{ anchor: HTMLElement }>();
const emit = defineEmits<{ close: [] }>();

const hint =
  "Type to filter by id and description — a space separates terms, all must match. " +
  "Click subjects to narrow by tag; several tags intersect. " +
  "↑↓ to move, Enter to hand the skill to this chat, Esc to close.";

const skills = ref<CapabilityDto[]>([]);
const filter = ref("");
const active = ref(0);
const selected = ref<Set<string>>(new Set());

const popEl = ref<HTMLElement>();
const filterEl = ref<HTMLInputElement>();
const listEl = ref<HTMLElement>();

/** Only what can actually be activated — an unavailable skill would just produce a refusal. */
const available = computed(() => skills.value.filter(s => s.state === "Available"));

const vocabulary = computed(() => {
  const counts = new Map<string, number>();
  for (const s of available.value)
    for (const tag of s.tags || []) counts.set(tag, (counts.get(tag) || 0) + 1);

  return [...counts.entries()]
    .map(([tag, count]) => ({ tag, count }))
    .sort((a, b) => b.count - a.count || a.tag.localeCompare(b.tag));
});

const terms = computed(() =>
  filter.value.trim().toLowerCase().split(/\s+/).filter(Boolean));

const shown = computed(() =>
  available.value.filter(s => {
    // Tags intersect — selecting two narrows, the same arithmetic the librarian does.
    const tags = new Set(s.tags || []);
    if (![...selected.value].every(t => tags.has(t))) return false;

    const haystack = `${s.id} ${s.description ?? ""}`.toLowerCase();
    return terms.value.every(t => haystack.includes(t));
  }));

watch(shown, () => { active.value = 0; });

function toggleTag(tag: string) {
  const next = new Set(selected.value);
  next.has(tag) ? next.delete(tag) : next.add(tag);
  selected.value = next;
  filterEl.value?.focus();
}

function pick(skill: CapabilityDto) {
  if (!store.currentChat) return;
  client.send("chat.skill.activate",
    { chatId: store.currentChat, skillId: skill.id },
    { projectId: store.currentProjectId ?? undefined });
  emit("close");
}

function move(delta: number) {
  if (!shown.value.length) return;
  active.value = (active.value + delta + shown.value.length) % shown.value.length;
  nextTick(() => {
    listEl.value?.querySelector<HTMLElement>('.sp-item[data-active="1"]')
      ?.scrollIntoView({ block: "nearest" });
  });
}

function onKeydown(e: KeyboardEvent) {
  if (e.key === "ArrowDown") { e.preventDefault(); move(1); }
  else if (e.key === "ArrowUp") { e.preventDefault(); move(-1); }
  else if (e.key === "Enter") {
    e.preventDefault();
    const s = shown.value[active.value];
    if (s) pick(s);
  } else if (e.key === "Escape") { e.preventDefault(); emit("close"); }
}

// ── positioning: same flip-above-when-no-room behaviour as the model picker ──

const top = ref(0);
const left = ref<number | null>(null);
const popStyle = computed<CSSProperties>(() => ({
  top: top.value + "px",
  left: (left.value ?? 0) + "px",
  visibility: left.value === null ? "hidden" : undefined
}));

function position(flip = true) {
  const rect = props.anchor.getBoundingClientRect();
  const el = popEl.value;
  const popW = el?.offsetWidth ?? 420;
  const popH = el?.offsetHeight ?? 380;
  const noRoomBelow = rect.bottom + 2 + popH > window.innerHeight - 4;
  top.value = flip && noRoomBelow && rect.top - 2 - popH > 4
    ? rect.top - 2 - popH
    : Math.max(4, Math.min(top.value || rect.bottom + 2, window.innerHeight - popH - 4));
  let l = flip ? rect.right - popW : (left.value ?? rect.right - popW);
  if (l < 4) l = Math.min(rect.left, window.innerWidth - popW - 4);
  left.value = Math.max(4, Math.min(l, Math.max(4, window.innerWidth - popW - 4)));
}

function onOutsideClick(e: MouseEvent) {
  if (popEl.value && !popEl.value.contains(e.target as Node) && e.target !== props.anchor) emit("close");
}

const off = client.on("skills.result", p => { skills.value = p.skills || []; });
let ro: ResizeObserver | undefined;
const onWindowResize = () => position();

onMounted(() => {
  client.send("skills.get", undefined, { projectId: store.currentProjectId ?? undefined });
  position();
  filterEl.value?.focus();
  ro = new ResizeObserver(() => position(false));
  if (popEl.value) ro.observe(popEl.value);
  window.addEventListener("resize", onWindowResize);
  setTimeout(() => document.addEventListener("click", onOutsideClick), 0);
});
onBeforeUnmount(() => {
  off();
  document.removeEventListener("click", onOutsideClick);
  window.removeEventListener("resize", onWindowResize);
  ro?.disconnect();
});
</script>

<style scoped>
.skill-popup { position: fixed; z-index: 60; width: 420px; max-height: 60vh;
  display: flex; flex-direction: column;
  background: var(--panel, var(--bg)); color: var(--text);
  border: 1px solid color-mix(in srgb, var(--text) 22%, transparent); border-radius: 4px;
  box-shadow: 0 6px 24px rgba(0,0,0,0.28); }
.sp-filter { display: flex; align-items: center; gap: 6px; padding: 6px;
  border-bottom: 1px solid color-mix(in srgb, var(--text) 12%, transparent); }
.sp-filter input { flex: 1; background: transparent; color: var(--text);
  border: 1px solid color-mix(in srgb, var(--text) 18%, transparent); border-radius: 3px;
  padding: 2px 6px; font-family: var(--mono); font-size: var(--fs-sm); }
.sp-count, .sp-hint { font-size: var(--fs-xs); color: var(--muted); }
.sp-hint { cursor: help; }
.sp-tags { display: flex; flex-wrap: wrap; gap: 4px; padding: 6px;
  border-bottom: 1px solid color-mix(in srgb, var(--text) 12%, transparent); }
.sp-tag { font-size: var(--fs-xs); font-family: var(--mono); cursor: pointer;
  background: transparent; color: var(--muted); padding: 1px 6px; border-radius: 3px;
  border: 1px solid color-mix(in srgb, var(--text) 18%, transparent); }
.sp-tag:hover { background: color-mix(in srgb, var(--text) 6%, transparent); }
.sp-tag.on { color: var(--text); border-color: var(--accent, currentColor);
  background: color-mix(in srgb, var(--accent, var(--text)) 14%, transparent); }
.sp-tag-n { opacity: 0.6; }
.sp-list { overflow-y: auto; padding: 4px; }
.sp-item { padding: 4px 6px; border-radius: 3px; cursor: pointer; }
.sp-item[data-active="1"] { background: color-mix(in srgb, var(--text) 10%, transparent); }
.sp-id { font-family: var(--mono); font-size: var(--fs-sm); }
.sp-desc { font-size: var(--fs-xs); color: var(--muted);
  display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.sp-item-tags { font-family: var(--mono); font-size: var(--fs-xs); opacity: 0.55; }
.sp-empty { font-family: var(--mono); font-size: var(--fs-xs); color: var(--muted); padding: 6px; }
</style>
