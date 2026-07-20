<template>
  <div class="onec-tree-node">
    <button class="onec-node-head" :class="{ selected: selected === node.fullName }" @click="selectNode">
      <span v-if="node.children.length" class="onec-chevron" @click.stop="expanded = !expanded">
        {{ expanded ? "▾" : "▸" }}
      </span>
      <span v-else class="onec-chevron">·</span>
      <span class="onec-node-name">{{ node.name }}</span>
      <span class="onec-kind">{{ node.kind }}</span>
    </button>
    <div v-if="expanded && node.children.length" class="onec-node-children">
      <TreeNode
        v-for="child in node.children"
        :key="child.fullName"
        :node="child"
        :selected="selected"
        @select="emit('select', $event)"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";

export interface TreeObject {
  name: string;
  kind: string;
  fullName: string;
  children: TreeObject[];
}

const props = defineProps<{ node: TreeObject; selected?: string }>();
const emit = defineEmits<{ select: [node: TreeObject] }>();
const expanded = ref(false);

function selectNode() {
  emit("select", props.node);
}
</script>
