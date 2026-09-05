<!-- The "✕ Remove" that sits in a card header and removes the whole item. Mirrors
     web/src/components/buttons/RemoveButton.vue: pass `label` for "✕ Remove", omit it for a bare ✕. -->
<template>
  <button type="button" class="gbtn gbtn-danger gbtn-text" :title="title" @click.stop="$emit('click', $event)">
    <template v-if="label">✕ {{ label }}</template>
    <template v-else>✕</template>
  </button>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ label?: string; title?: string }>(), { title: "Remove" });
defineEmits<{ click: [MouseEvent] }>();
</script>
