<template>
  <div class="msg user">
    <div v-if="images?.length" style="display:flex;gap:6px;flex-wrap:wrap;margin-bottom:6px">
      <img v-for="(src, i) in images" :key="i" :src="src" style="max-width:160px;max-height:160px;border-radius:6px">
    </div>
    <div class="body plain">{{ text }}</div>
  </div>
</template>

<script setup lang="ts">
defineProps<{ text: string; images?: string[] }>();
</script>
