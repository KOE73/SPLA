<!-- The "✕" or "✕ Remove" that sits in a card/row header, removing the whole item (as opposed to
     DeleteButton's trash, which reads as "delete this stored thing"). Same visual family, text
     variant: pass `label` for "✕ Remove", omit it for a bare corner ✕. -->
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
