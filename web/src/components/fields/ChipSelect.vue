<!--
  Pick some of a known set, or say nothing and get all of it. The "all" state is a state, not an
  empty selection: selecting nothing on purpose and never having chosen are different answers, so
  there is an explicit "everything" chip and clearing the last pick returns to it rather than
  silently meaning "none".

  Used for the narrowing lists a role carries (connections, islands) — every one of which is a
  SELECTION over what already exists, never a grant of something new.
-->
<template>
  <div class="chips">
    <button type="button" class="chip" :class="{ on: !modelValue }" @click="emit('update:modelValue', null)">
      {{ allLabel }}
    </button>
    <button v-for="o in options" :key="o.id" type="button" class="chip"
            :class="{ on: !!modelValue && modelValue.includes(o.id) }"
            :title="o.title || o.id" @click="toggle(o.id)">
      {{ o.label || o.id }}
    </button>
  </div>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{
  /** null = "everything", a list = exactly these. */
  modelValue?: string[] | null;
  options: { id: string; label?: string; title?: string }[];
  allLabel?: string;
}>(), { modelValue: null, allLabel: "everything" });

const emit = defineEmits<{ "update:modelValue": [string[] | null] }>();

function toggle(id: string) {
  const current = props.modelValue;
  // First pick out of "everything" starts a selection holding just that one — not "everything minus
  // one", which would be a different (and unwritable) thing.
  if (!current) return emit("update:modelValue", [id]);
  const next = current.includes(id) ? current.filter(x => x !== id) : [...current, id];
  emit("update:modelValue", next.length ? next : null);
}
</script>

<style scoped>
.chips { display: flex; flex-wrap: wrap; gap: 4px; }
.chip { font: inherit; font-size: var(--fs-xs); padding: 1px 8px; cursor: pointer;
  border: 1px solid var(--border); border-radius: 999px; background: var(--bg); color: var(--muted); }
.chip:hover { color: var(--text); }
.chip.on { background: var(--accent-soft); color: var(--text); border-color: var(--accent); }
</style>
