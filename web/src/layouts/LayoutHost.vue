<template>
  <!-- Solo window: ?surface=<name> renders one surface full-screen, used by tear-off windows
       (e.g. an Avalonia WebView pointed at /?surface=debug). Mirrors WebClient/app.js isSolo. -->
  <div v-if="soloName" id="solo">
    <component :is="surfaces[soloName]" v-if="surfaces[soloName]" />
    <div v-else class="surface-missing">no surface: {{ soloName }}</div>
  </div>
  <AppShell v-else />
  <!-- Above both branches: a surface opened over the app (settings) must cover the solo hub window
       too, which is where the other entry point lives. -->
  <SurfaceOverlay />
</template>

<script setup lang="ts">
import AppShell from "./AppShell.vue";
import SurfaceOverlay from "./SurfaceOverlay.vue";
import { surfaces } from "../surfaces/registry";

const soloName = new URLSearchParams(location.search).get("surface");
</script>
