<!-- Recursive owns-hierarchy node for the 1C object tree. -->
<template>
  <div class="node">
    <div class="node-head" :class="{ sel: selected === node.fullName }" @click="onClick">
      <span class="chev" v-if="node.children.length" @click.stop="expanded = !expanded">{{ expanded ? '▾' : '▸' }}</span>
      <span class="chev" v-else>·</span>
      {{ node.name }} <span class="kind">[{{ node.kind }}]</span>
    </div>
    <div v-if="expanded && node.children.length" class="children">
      <TreeNode v-for="c in node.children" :key="c.fullName" :node="c" :selected="selected" @select="$emit('select', $event)" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";

interface TreeObj { name: string; kind: string; fullName: string; children: TreeObj[] }
const props = defineProps<{ node: TreeObj; selected?: string }>();
const emit = defineEmits<{ (e: "select", node: TreeObj): void }>();
const expanded = ref(false);
function onClick() { emit("select", props.node); }
</script>

<style scoped>
.node-head { cursor: pointer; padding: 1px 0; white-space: nowrap; }
.node-head.sel { color: var(--accent, #007acc); font-weight: 600; }
.chev { display: inline-block; width: 14px; }
.kind { color: var(--muted, #888); font-size: 11px; }
.children { padding-left: 12px; }
</style>
