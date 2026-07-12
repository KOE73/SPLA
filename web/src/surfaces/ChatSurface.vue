<template>
  <div id="main" class="chat-surface">
    <div id="log"><ChatLog /></div>
    <div id="composer"><Composer /></div>
    <div id="status"><StatusBar /></div>
    <div id="filters"><Filters /></div>
  </div>
</template>

<script setup lang="ts">
import ChatLog from "./ChatLog.vue";
import Composer from "./Composer.vue";
import StatusBar from "./StatusBar.vue";
import Filters from "./Filters.vue";
</script>

<style scoped>
.chat-surface { width: 100%; height: 100%; min-width: 0; min-height: 0; }
</style>
