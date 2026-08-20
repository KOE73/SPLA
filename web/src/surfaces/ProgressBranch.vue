<!--
  One node of a turn's progress tree and everything beneath it.

  Recursive on purpose, and shallow in practice: the depth is however many tools called tools, which
  is two or three in anything anyone has run. The breadth is the part that grows — a script fanning
  out to two hundred parallel children — so the list is capped and says how many it is not showing
  rather than becoming the page.

  Renders children only; the node the card already names (the root) is not repeated here.
-->
<template>
  <ul class="pb-list">
    <li v-for="child in visible" :key="child.nodeId" class="pb-item">
      <div class="pb-row" :class="'pb-' + child.state">
        <span class="pb-mark">{{ mark(child.state) }}</span>
        <span class="pb-label">{{ child.label }}</span>
        <span v-if="child.message" class="pb-msg">{{ child.message }}</span>
      </div>
      <ProgressBranch v-if="child.childIds.length" :nodes="nodes" :parent-id="child.nodeId" />
    </li>
    <li v-if="hidden > 0" class="pb-item pb-more">…и ещё {{ hidden }}</li>
  </ul>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { ProgressNodeState } from "../state/chatSessions";

const props = defineProps<{
  nodes: Record<string, ProgressNodeState>;
  parentId: string;
}>();

/** Above this, a wide fan-out would push the conversation off the screen. Running children are kept
 *  first: what is still going is what a person is watching. */
const LIMIT = 12;

const children = computed(() => {
  const parent = props.nodes[props.parentId];
  if (!parent) return [];
  // A stub parent (named by a child before its own frame arrived) has no label yet; its children are
  // still real and still worth showing.
  return parent.childIds
    .map(id => props.nodes[id])
    .filter((n): n is ProgressNodeState => n != null && n.label !== "");
});

const visible = computed(() => {
  const all = children.value;
  if (all.length <= LIMIT) return all;
  const running = all.filter(n => n.state === "running");
  return running.length >= LIMIT ? running.slice(0, LIMIT) : [...running, ...all.filter(n => n.state !== "running")].slice(0, LIMIT);
});

const hidden = computed(() => children.value.length - visible.value.length);

function mark(state: ProgressNodeState["state"]) {
  return state === "completed" ? "✓" : state === "failed" ? "✗" : "•";
}
</script>

<style scoped>
.pb-list { list-style: none; margin: 0; padding: 0 0 0 12px; border-left: 1px solid var(--border); }
.pb-item { margin: 0; }
.pb-row { display: flex; align-items: baseline; gap: 6px; font-size: var(--fs-xs); line-height: 1.5; }
.pb-mark { width: 1em; text-align: center; color: var(--muted); flex: none; }
.pb-running .pb-mark { color: var(--accent); }
.pb-failed .pb-mark { color: var(--danger, #c0392b); }
.pb-label { font-weight: 600; }
.pb-completed .pb-label, .pb-completed .pb-msg { color: var(--muted); }
.pb-msg { color: var(--muted); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.pb-more { font-size: var(--fs-xs); color: var(--muted); font-style: italic; padding-left: 1.4em; }
</style>
