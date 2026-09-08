<!--
  One item in a ListPanel, and the shape of every collapsible row in Settings.

  The card owns its header layout, and consumers cannot rearrange it — that is the point:
      ▸  title …………………………………………………………… actions
  The caret is ALWAYS first, before the title and before anything else a panel puts in the head, so
  the eye finds "open this" in the same place in every list. Actions (delete and friends) are always
  last. A panel supplies text and buttons; it never decides where they sit.

  The title is the consumer's, because only the list's author knows what names an item — and it may
  legitimately differ by state: collapsed, the head is all the reader gets, so it usually carries
  more (a summary of what is inside); expanded, the fields say it themselves and the title can go
  back to a bare name. Hence `title` plus optional `titleOpen`, or the `#title` slot when the name
  needs markup (it receives `open`). `summary` — text or slot — is a collapsed-only afterword next
  to the title; it disappears on open, since by then the reader is looking at the real thing.

  Clicking the header toggles, unless `no-toggle-on-click` (rows whose header holds its own controls,
  a checkbox say). `:collapsible="false"` drops the caret for a card that has nothing to open.
-->
<template>
  <div class="list-card" :class="{ open }">
    <div class="list-card-head" @click="onHeadClick">
      <ExpandButton v-if="collapsible" :open="open" @update:open="emit('update:open', $event)" />
      <div class="list-card-title"><slot name="title" :open="open">{{ open && titleOpen ? titleOpen : title }}</slot></div>
      <div v-if="!open && (summary || $slots.summary)" class="list-card-summary">
        <slot name="summary">{{ summary }}</slot>
      </div>
      <div class="list-card-actions"><slot name="actions" :open="open" /></div>
    </div>
    <div v-if="open && $slots.body" class="list-card-body"><slot name="body" /></div>
  </div>
</template>

<script setup lang="ts">
import ExpandButton from "../buttons/ExpandButton.vue";

const props = withDefaults(defineProps<{
  open?: boolean;
  /** What names this item. Use the #title slot instead when it needs markup. */
  title?: string;
  /** The name to show while expanded, when it differs from the collapsed one. */
  titleOpen?: string;
  /** Collapsed-only afterword: what is inside, so a shut card still says something. */
  summary?: string;
  noToggleOnClick?: boolean;
  collapsible?: boolean;
}>(), { open: false, title: "", titleOpen: "", summary: "", noToggleOnClick: false, collapsible: true });

const emit = defineEmits<{ "update:open": [boolean] }>();

function onHeadClick() {
  if (!props.noToggleOnClick && props.collapsible) emit("update:open", !props.open);
}
</script>
