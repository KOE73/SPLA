<template>
  <!-- Solo window: ?surface=<name> renders one surface full-screen, used by tear-off windows
       (e.g. an Avalonia WebView pointed at /?surface=debug). Mirrors WebClient/app.js isSolo. -->
  <div v-if="soloName" id="solo">
    <component :is="surfaces[soloName]" v-if="surfaces[soloName]" />
    <div v-else class="surface-missing">no surface: {{ soloName }}</div>
  </div>
  <div v-else id="app" :data-layout="store.layout">
    <LayoutNodeView v-for="node in layout.root" :key="node.id" :node="node" />
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import LayoutNodeView from "./LayoutNodeView.vue";
import { layouts } from "./layouts";
import { surfaces } from "../surfaces/registry";
import { store } from "../state/store";

const soloName = new URLSearchParams(location.search).get("surface");
const layout = computed(() => layouts[store.layout] ?? layouts[Object.keys(layouts)[0]]);
</script>
