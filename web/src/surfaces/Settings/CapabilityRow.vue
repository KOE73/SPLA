<template>
  <!-- One switchable capability. Deliberately shared between the Built-in and Skills tabs: the three
       Capabilities lists are the same kind of thing (an id, a switch, a resolved state, a reason) and
       should look and behave identically. Plugins keep their own richer card because they also carry
       a settings editor, but the row grammar here matches it. -->
  <div class="cap-row" :class="{ off: !item.enabled, blocked: isBlocked }">
    <input type="checkbox" :checked="item.enabled" :disabled="disabled" @change="onToggle">

    <b class="cap-name">{{ item.name }}</b>
    <span v-if="badge" class="cap-badge">{{ badge }}</span>
    <span v-if="item.description" class="cap-desc">{{ item.description }}</span>

    <span class="grow"></span>

    <span v-if="item.stateReason" class="cap-reason" :class="{ warn: isBlocked }">{{ item.stateReason }}</span>
    <slot name="actions" />
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { CapabilityDto } from "../../protocol/types";

const props = defineProps<{
  item: CapabilityDto;
  /** Small label before the description — the source for a skill, the dependency note for a built-in. */
  badge?: string;
  disabled?: boolean;
}>();

const emit = defineEmits<{ (e: "toggle", enabled: boolean): void }>();

/** States the user cannot fix with this checkbox alone — they need a plugin enabled. Rendered in the
 * warning colour so "off because you said so" and "off because something is missing" never look the
 * same. */
const isBlocked = computed(() => props.item.state === "MissingPrerequisites");

function onToggle(e: Event) {
  emit("toggle", (e.target as HTMLInputElement).checked);
}
</script>

<style scoped>
.cap-row { display: flex; align-items: center; gap: 8px; padding: 4px 8px; min-height: 26px;
  border: 1px solid var(--border); border-radius: var(--radius, 7px); background: var(--elevated); }
.cap-row:hover { background: color-mix(in srgb, var(--text) 4%, transparent); }
.cap-row.off .cap-name { color: var(--muted); }
.cap-name { font-size: var(--fs-sm); white-space: nowrap; }
.cap-badge { font-family: var(--mono); font-size: var(--fs-xs); color: var(--muted);
  border: 1px solid var(--border); border-radius: 4px; padding: 0 4px; white-space: nowrap; }
.cap-desc { font-size: var(--fs-xs); color: var(--muted);
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.grow { flex: 1; min-width: 8px; }
.cap-reason { font-size: var(--fs-xs); color: var(--muted); white-space: nowrap;
  overflow: hidden; text-overflow: ellipsis; max-width: 45%; }
.cap-reason.warn { color: var(--danger, #f85149); }
</style>
