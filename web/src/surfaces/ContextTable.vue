<template>
  <div class="ctx-header">
    <span class="ctx-stats">
      {{ showAll ? `all ${snapshot.totalCount} msgs · ${snapshot.contextCount} in context · ~${snapshot.approxTokens} tok`
                 : `${snapshot.contextCount} in context · ~${snapshot.approxTokens} tok` }}
      {{ snapshot.contextIsLive ? "" : " · preview (no live request yet)" }}
    </span>
    <button class="ctx-toggle" :class="{ on: showAll }" @click="showAll = !showAll">
      {{ showAll ? "in-context only" : "show all" }}
    </button>
  </div>
  <div class="ctx-scroll">
    <table class="ctx-table">
      <colgroup><col class="c-idx"><col class="c-id"><col class="c-tok"><col class="c-src"><col class="c-pre"></colgroup>
      <thead><tr>
        <th class="c-idx">#</th><th class="c-id">msgId</th>
        <th class="c-tok">~tok</th><th class="c-src">source</th><th class="c-pre">preview</th>
      </tr></thead>
      <tbody>
        <template v-for="l in lines" :key="l.index">
          <tr class="ctx-row" :class="[srcClass(l.source), { dimmed: !l.inContext }]" @click="toggleExpand(l.index)">
            <td class="c-idx">{{ l.index }}</td>
            <td class="c-id">{{ l.msgId }}</td>
            <td class="c-tok">{{ l.approxTokens }}</td>
            <td class="c-src">{{ l.source }}</td>
            <td class="c-pre">{{ l.preview }}</td>
          </tr>
          <tr v-if="expanded === l.index" class="ctx-expand">
            <td colspan="5"><pre>{{ l.full || "(empty)" }}</pre></td>
          </tr>
        </template>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import type { DebugSnapshotPayload } from "../protocol/types";

const props = defineProps<{ snapshot: DebugSnapshotPayload }>();

const showAll = ref(false);
const expanded = ref<number | null>(null);

const lines = computed(() => {
  const all = props.snapshot.contextLines || [];
  return showAll.value ? all : all.filter(l => l.inContext);
});

function toggleExpand(i: number) { expanded.value = expanded.value === i ? null : i; }

// Explicit map, not string-mangling — fragile substring replacement chains silently produce
// class names that don't match any selector.
const SRC_CLASS: Record<string, string> = {
  system: "src-system", "working-mem": "src-working", user: "src-user",
  assistant: "src-assistant", "asst→tool": "src-assistant", "tool-result": "src-tool"
};
function srcClass(source: string) {
  if (SRC_CLASS[source]) return SRC_CLASS[source];
  if (source.startsWith("label")) return "src-label";
  return "src-assistant";
}
</script>
