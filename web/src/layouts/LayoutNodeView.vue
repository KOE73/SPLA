<template>
  <!-- Leaf: mount the registered surface component into a div with the CSS-targeted id. -->
  <div v-if="node.surface" :id="node.id">
    <component :is="surfaces[node.surface]" v-if="surfaces[node.surface]" />
    <div v-else class="surface-missing">no surface: {{ node.surface }}</div>
  </div>
  <!-- Group: plain wrapper div (e.g. #main) with nested slots. -->
  <div v-else :id="node.id">
    <LayoutNodeView v-for="child in node.children" :key="child.id" :node="child" />
  </div>
</template>

<script setup lang="ts">
import type { LayoutNode } from "./layouts";
import { surfaces } from "../surfaces/registry";

defineProps<{ node: LayoutNode }>();
</script>
