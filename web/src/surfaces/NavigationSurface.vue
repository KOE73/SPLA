<template><div id="sidebar" class="navigation-surface"><ChatList /></div></template>

<script setup lang="ts">
import ChatList from "./ChatList.vue";
</script>

<style scoped>
.navigation-surface { width: 100%; height: 100%; min-width: 0; min-height: 0; }
</style>
