<!--
  "Who talks to whom", drawn.

  The list this replaces had one row per (fromChat, toChat, topic), so two roles corresponding in
  three different chats read as three identical "agent → fool" lines and the shape of the traffic
  was invisible. Here a role is a box and a pair of roles is ONE arrow, however many chats it covers
  — the picture is the point, and the numbers ride along on it.

  Everything is plain SVG in the theme's own variables: no chart library, no canvas, and it scales
  with the panel because the only sizing is a viewBox.
-->
<template>
  <svg
    v-if="layout.nodes.length"
    class="corr-graph"
    :viewBox="`0 0 ${layout.width} ${layout.height}`"
    :style="{ maxHeight: layout.height + 'px' }"
    role="img"
    :aria-label="t('Correspondence between roles')"
  >
    <defs>
      <!-- One marker per direction; `context-stroke` keeps the head the same colour as its line. -->
      <!-- userSpaceOnUse, not the default: a marker sized in stroke-widths grows with the line,
           and since line weight here CARRIES volume, the busiest pair grew a head the size of a box. -->
      <marker id="corr-arrow" viewBox="0 0 8 8" refX="6.5" refY="4"
              markerWidth="8" markerHeight="8" markerUnits="userSpaceOnUse"
              orient="auto-start-reverse">
        <path d="M 0 1 L 7 4 L 0 7 z" fill="context-stroke" />
      </marker>
    </defs>

    <!-- Arrows first, so a box always sits on top of the line that reaches it. -->
    <g class="edges">
      <g v-for="e in arrows" :key="e.key" class="edge" :class="{ both: e.both }">
        <title>{{ e.title }}</title>
        <path :d="e.d" :stroke-width="e.weight" marker-end="url(#corr-arrow)"
              :marker-start="e.both ? 'url(#corr-arrow)' : undefined" />
        <text :x="e.labelX" :y="e.labelY" text-anchor="middle">{{ e.label }}</text>
      </g>
    </g>

    <g class="nodes">
      <g v-for="n in layout.nodes" :key="n.role" class="node" :class="{ imbalanced: isImbalanced(n.role) }">
        <title>{{ nodeTitle(n) }}</title>
        <rect :x="n.x" :y="n.y" :width="NODE_W" :height="NODE_H" rx="9" ry="9" />
        <text class="node-name" :x="n.x + NODE_W / 2" :y="n.y + 18" text-anchor="middle">{{ n.role }}</text>
        <text class="node-stat" :x="n.x + NODE_W / 2" :y="n.y + 34" text-anchor="middle">
          {{ n.sent }}↦ {{ n.sentVolume }}t · {{ n.received }}↤ {{ n.receivedVolume }}t
        </text>
      </g>
    </g>
  </svg>
</template>

<script setup lang="ts">
import { t } from "../i18n";
import { computed } from "vue";
import type { CorrespondenceEdgeDto } from "../protocol/types";
import {
  NODE_H, NODE_W, layoutGraph, neverReplies, onlyTalks,
  type GraphNode, type RolePairEdge
} from "../state/correspondenceGraph";

const props = defineProps<{ edges: CorrespondenceEdgeDto[] }>();

const layout = computed(() => layoutGraph(props.edges));

/** Roles the imbalance check flags — drawn in the warning colour rather than repeated in words. */
const imbalancedRoles = computed(() => new Set([
  ...onlyTalks(props.edges).map(r => r.role),
  ...neverReplies(props.edges).map(r => r.role)
]));
const isImbalanced = (role: string) => imbalancedRoles.value.has(role);

const byRole = computed(() => new Map(layout.value.nodes.map(n => [n.role, n])));

/** Line thickness carries volume, so the loud pair is visible before any number is read. */
function weightOf(e: RolePairEdge): number {
  const max = Math.max(1, ...layout.value.edges.map(x => x.sentVolume + x.backVolume));
  return 1 + 2.5 * ((e.sentVolume + e.backVolume) / max);
}

const arrows = computed(() => layout.value.edges.flatMap(e => {
  const a = byRole.value.get(e.from);
  const b = byRole.value.get(e.to);
  if (!a || !b) return [];

  // Draw from the box the conversation starts at. Ties go to the stored orientation.
  const forward = e.sent >= e.back;
  const [src, dst] = forward ? [a, b] : [b, a];
  const both = e.sent > 0 && e.back > 0;

  const route = pathFor(src, dst);

  return [{
    key: `${e.from}->${e.to}`,
    d: route.d,
    weight: weightOf(e),
    both,
    labelX: route.labelX,
    labelY: route.labelY,
    label: both ? `${e.sent}↔${e.back}` : String(Math.max(e.sent, e.back)),
    title: edgeTitle(e)
  }];
}));

/**
 * The curve from one box to the next, and where its number sits.
 *
 * Two cases, because two boxes in the SAME column have no facing sides to join: routing those out
 * the right edge the way a cross-column arrow goes put the whole bow outside the viewBox and the
 * arrow was simply cut off. Stacked boxes are joined bottom-to-top instead, bowed sideways by less
 * than half a box — which keeps every pixel of it inside the column that is already measured.
 */
function pathFor(src: GraphNode, dst: GraphNode) {
  const sameColumn = src.x === dst.x;

  if (sameColumn) {
    const [top, bottom] = src.y <= dst.y ? [src, dst] : [dst, src];
    const down = top === src;
    const x = src.x + NODE_W / 2;
    const y1 = down ? top.y + NODE_H : bottom.y;
    const y2 = down ? bottom.y : top.y + NODE_H;
    const cx = x + SIDE_BOW;
    const cy = (y1 + y2) / 2;
    return { d: `M ${x} ${y1} Q ${cx} ${cy} ${x} ${y2}`, labelX: cx + 8, labelY: cy };
  }

  const srcLeft = src.x < dst.x;
  const x1 = srcLeft ? src.x + NODE_W : src.x;
  const x2 = srcLeft ? dst.x : dst.x + NODE_W;
  const y1 = src.y + NODE_H / 2;
  const y2 = dst.y + NODE_H / 2;
  const mx = (x1 + x2) / 2;
  const my = (y1 + y2) / 2;
  const bow = Math.abs(y2 - y1) < 4 ? 0 : 14;   // a straight run needs no bow
  return { d: `M ${x1} ${y1} Q ${mx} ${my - bow} ${x2} ${y2}`, labelX: mx, labelY: my - bow - 4 };
}

/** How far a stacked pair's arrow bows sideways. Well under half a box, so it cannot leave the column. */
const SIDE_BOW = 34;

function edgeTitle(e: RolePairEdge): string {
  const chats = e.pairs === 1 ? "1 correspondence" : `${e.pairs} correspondences`;
  return `${e.from} → ${e.to}: ${e.sent} replies (${e.sentVolume}t)\n`
    + `${e.to} → ${e.from}: ${e.back} replies (${e.backVolume}t)\n${chats}`;
}

function nodeTitle(n: GraphNode): string {
  return `${n.role}\nsent ${n.sent} replies (${n.sentVolume}t)\nreceived ${n.received} (${n.receivedVolume}t)`;
}
</script>

<style scoped>
.corr-graph { width: 100%; height: auto; display: block; overflow: visible; }

.node rect { fill: var(--panel); stroke: var(--border); stroke-width: 1; }
.node .node-name { fill: var(--text); font-size: 13px; font-weight: 600; }
.node .node-stat { fill: var(--muted); font-size: 10px; font-family: var(--mono); }
/* A flagged role is coloured rather than described — the warning rows above say it in words. */
.node.imbalanced rect { stroke: var(--danger); }
.node.imbalanced .node-name { fill: var(--danger); }

.edge path { fill: none; stroke: var(--accent); opacity: .55; }
.edge text { fill: var(--muted); font-size: 9px; font-family: var(--mono); }
.edge:hover path { opacity: 1; }
.edge:hover text { fill: var(--text); }
</style>
