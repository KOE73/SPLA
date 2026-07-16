<!--
  1C Configuration Browser — web port of the Avalonia OneCOverviewPanel.

  All data comes from the plugin backend over the plugin.action channel (see OneCPlugin.
  InvokeActionAsync): overview counters + owns-tree, substring search, object card, the three graph
  modes, context export, and index rebuild. The graph is rendered with Cytoscape.js loaded from a
  CDN (see TODO.md — bundling it locally is a follow-up).

  Note: this ships today through the plugin's web_settings_entry, so it opens under
  Settings → Plugins → 1C. Promoting it to a first-class dockable workspace surface is a host change
  tracked in TODO.md.
-->
<template>
  <div class="onec">
    <!-- Left column: search + tree + counters + export/rebuild -->
    <div class="col-left">
      <div class="card">
        <input v-model="query" class="search" placeholder="Поиск объекта 1С…" spellcheck="false" />
        <ul v-if="searchResults.length" class="search-results">
          <li v-for="o in searchResults" :key="o.fullName"
              :class="{ sel: selected?.fullName === o.fullName }"
              @click="select(o)">{{ o.fullName }}</li>
        </ul>
        <div class="tree">
          <div v-for="s in tree" :key="s.kind" class="tree-section">
            <div class="tree-section-head" @click="toggle(s.kind)">
              <span class="chev">{{ open[s.kind] ? '▾' : '▸' }}</span>{{ s.section }} ({{ s.count }})
            </div>
            <div v-if="open[s.kind]" class="tree-children">
              <TreeNode v-for="n in s.objects" :key="n.fullName" :node="n"
                        :selected="selected?.fullName" @select="select" />
            </div>
          </div>
        </div>
      </div>

      <div class="card counters">
        <div><span class="muted">Objects</span> <b>{{ overview.objectCount }}</b></div>
        <div><span class="muted">Relations</span> <b>{{ overview.relationCount }}</b></div>
        <div><span class="muted">Sections</span> <b>{{ overview.sectionCount }}</b></div>
      </div>

      <div class="card export">
        <div class="muted">Context export</div>
        <div class="row">
          <select v-model="formatterId">
            <option v-for="f in formatters" :key="f.id" :value="f.id">{{ f.name }}</option>
          </select>
          <button @click="copy" :disabled="!selected">Copy</button>
        </div>
        <div class="muted">Rebuild index</div>
        <div class="row">
          <input v-model="rebuildPath" class="grow" placeholder="Путь к дампу конфигурации 1С…" spellcheck="false" />
          <button @click="rebuild" :disabled="rebuilding">{{ rebuilding ? '…' : 'Rebuild' }}</button>
        </div>
        <div v-if="status" class="muted status">{{ status }}</div>
      </div>
    </div>

    <!-- Right column: object card + graph -->
    <div class="col-right card">
      <div class="obj-title">{{ selected?.fullName || 'Выберите объект' }}</div>
      <div class="row">
        <button @click="graph('dependencies')" :disabled="!selected">Dependencies Graph</button>
        <button @click="graph('references')" :disabled="!selected">References Graph</button>
        <button @click="graph('dataflow')" :disabled="!selected">Data Flow Graph</button>
      </div>
      <div class="muted graph-summary">{{ graphSummary }}</div>
      <pre v-if="selected" class="obj-details">{{ objDetails }}</pre>
      <div ref="cyEl" class="cy"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref, watch } from "vue";
import type { MountApi } from "./mount";
import TreeNode from "./TreeNode.vue";

interface ObjDto { name: string; kind: string; fullName: string; path: string; summary: string }
interface TreeObj { name: string; kind: string; fullName: string; children: TreeObj[] }
interface Section { section: string; kind: string; count: number; objects: TreeObj[] }

const props = defineProps<{ api: MountApi }>();

const overview = reactive({ objectCount: 0, relationCount: 0, sectionCount: 0 });
const tree = ref<Section[]>([]);
const open = reactive<Record<string, boolean>>({});
const query = ref("");
const searchResults = ref<ObjDto[]>([]);
const selected = ref<ObjDto | null>(null);
const formatters = ref<{ id: string; name: string }[]>([]);
const formatterId = ref("full-name");
const rebuildPath = ref("");
const rebuilding = ref(false);
const status = ref("");
const graphSummary = ref("");
const cyEl = ref<HTMLElement | null>(null);
let lastMode = "";
let cy: any = null;

const objDetails = ref("");

/** Thin wrapper over the host plugin.action RPC. */
async function action<R = unknown>(name: string, payload: Record<string, unknown> = {}): Promise<R> {
  const r = await props.api.invoke<{ ok: boolean; resultJson?: string; error?: string }>("plugin.action", {
    pluginId: "onec",
    action: name,
    valueJson: JSON.stringify(payload)
  });
  if (!r.ok) throw new Error(r.error || "action failed");
  return (r.resultJson ? JSON.parse(r.resultJson) : null) as R;
}

onMounted(async () => {
  await loadOverview();
  try {
    const f = await action<{ formatters: { id: string; name: string }[] }>("formatters");
    formatters.value = f.formatters;
  } catch { /* formatters are optional */ }
});

async function loadOverview() {
  try {
    const o = await action<{ objectCount: number; relationCount: number; sectionCount: number; tree: Section[] }>("overview");
    overview.objectCount = o.objectCount;
    overview.relationCount = o.relationCount;
    overview.sectionCount = o.sectionCount;
    tree.value = o.tree;
    for (const s of o.tree) if (!(s.kind in open)) open[s.kind] = false;
  } catch (e) {
    status.value = "Не удалось открыть индекс 1С: " + msg(e);
  }
}

let searchTimer: number | undefined;
watch(query, (q) => {
  clearTimeout(searchTimer);
  searchTimer = window.setTimeout(async () => {
    if (!q.trim()) { searchResults.value = []; return; }
    try {
      const r = await action<{ results: ObjDto[] }>("search", { query: q });
      searchResults.value = r.results;
    } catch { searchResults.value = []; }
  }, 150);
});

function toggle(kind: string) { open[kind] = !open[kind]; }

function select(o: { fullName: string }) {
  action<ObjDto | null>("object", { fullName: o.fullName }).then((dto) => {
    selected.value = dto;
    graphSummary.value = "Выберите Dependencies, References или Data Flow.";
    objDetails.value = dto
      ? `kind: ${dto.kind}\nname: ${dto.name}\nfull_name: ${dto.fullName}\npath: ${dto.path}\nsummary: ${dto.summary}`
      : "";
    lastMode = "";
    cy?.elements().remove();
  });
}

async function graph(mode: string) {
  if (!selected.value) return;
  lastMode = mode;
  try {
    const g = await action<any>("graph", { fullName: selected.value.fullName, mode, depth: 3, limit: 400 });
    graphSummary.value = `mode: ${g.mode} · nodes: ${g.nodeCount} · edges: ${g.edgeCount} · depth: ${g.depth} · truncated: ${g.truncated}`;
    await renderGraph(g.elements);
  } catch (e) {
    graphSummary.value = "Не удалось построить граф: " + msg(e);
  }
}

async function copy() {
  if (!selected.value) return;
  try {
    const r = await action<{ text: string }>("format", {
      formatterId: formatterId.value,
      fullName: selected.value.fullName,
      mode: lastMode,
      depth: 3,
      limit: 400
    });
    await navigator.clipboard.writeText(r.text);
    status.value = "Скопировано в буфер обмена.";
  } catch (e) {
    status.value = "Копирование не удалось: " + msg(e);
  }
}

async function rebuild() {
  if (!rebuildPath.value.trim()) { status.value = "Укажите путь к дампу конфигурации."; return; }
  rebuilding.value = true;
  status.value = "Индексация запущена…";
  try {
    const r = await action<any>("rebuild", { path: rebuildPath.value.trim() });
    status.value = `Готово. Объектов: +${r.objectsAdded} ~${r.objectsUpdated}, связей: +${r.relationsAdded}, пропущено: ${r.filesSkipped}, ошибок: ${r.filesWithErrors}, за ${r.elapsedSeconds}с`;
    await loadOverview();
  } catch (e) {
    status.value = "Индексация не удалась: " + msg(e);
  } finally {
    rebuilding.value = false;
  }
}

// ── Cytoscape (loaded from CDN on first graph render) ───────────────────────
const KIND_COLOR: Record<string, string> = {
  Document: "#89b4fa", Catalog: "#a6e3a1", AccumulationRegister: "#f38ba8",
  InformationRegister: "#fab387", AccountingRegister: "#f38ba8", CalculationRegister: "#f38ba8",
  CommonModule: "#cba6f7", Report: "#f9e2af", Processing: "#f9e2af", Form: "#94e2d5", TabularSection: "#94e2d5"
};
const EDGE_COLOR: Record<string, string> = {
  writes: "#f38ba8", reads: "#89b4fa", queries: "#fab387", calls: "#a6e3a1", owns: "#6c7086", uses: "#cba6f7"
};

async function ensureCytoscape(): Promise<any> {
  if ((window as any).cytoscape) return (window as any).cytoscape;
  await new Promise<void>((resolve, reject) => {
    const s = document.createElement("script");
    s.src = "https://cdnjs.cloudflare.com/ajax/libs/cytoscape/3.30.2/cytoscape.min.js";
    s.onload = () => resolve();
    s.onerror = () => reject(new Error("cytoscape CDN load failed"));
    document.head.appendChild(s);
  });
  return (window as any).cytoscape;
}

async function renderGraph(data: { nodes: any[]; edges: any[] }) {
  const cytoscape = await ensureCytoscape();
  if (!cyEl.value) return;
  const elements = [
    ...data.nodes.map((n) => ({ data: n })),
    ...data.edges.map((e) => ({ data: e }))
  ];
  if (cy) cy.destroy();
  cy = cytoscape({
    container: cyEl.value,
    elements,
    wheelSensitivity: 0.25,
    style: [
      { selector: "node", style: {
        "background-color": (n: any) => KIND_COLOR[n.data("kind")] || "#6c7086",
        label: "data(label)", color: "#cdd6f4", "font-size": "10px",
        "text-valign": "bottom", "text-margin-y": 4, "text-outline-width": 2,
        "text-outline-color": "#1e1e2e", width: 34, height: 34
      } },
      { selector: "node[?isCenter]", style: {
        width: 52, height: 52, "border-width": 3, "border-color": "#cdd6f4",
        "font-size": "12px", "font-weight": "bold"
      } },
      { selector: "edge", style: {
        "line-color": (e: any) => EDGE_COLOR[e.data("type")] || "#6c7086",
        "target-arrow-color": (e: any) => EDGE_COLOR[e.data("type")] || "#6c7086",
        "target-arrow-shape": "triangle", "curve-style": "bezier", width: 1.6, opacity: 0.85,
        label: "data(type)", "font-size": 8, color: "#6c7086", "text-rotation": "autorotate"
      } }
    ],
    layout: { name: "cose", animate: false, padding: 35, nodeRepulsion: 6500, idealEdgeLength: 120 }
  });
  setTimeout(() => cy?.fit(undefined, 30), 100);
}

function msg(e: unknown) { return e instanceof Error ? e.message : String(e); }
</script>

<style scoped>
.onec { display: flex; gap: 12px; height: 100%; min-height: 480px; font-size: 13px; }
.col-left { width: 360px; display: flex; flex-direction: column; gap: 12px; }
.col-right { flex: 1; display: flex; flex-direction: column; gap: 10px; }
.card { background: var(--panel-bg, #252526); border: 1px solid var(--border, #333); border-radius: 8px; padding: 12px; }
.col-left .card:first-child { flex: 1; display: flex; flex-direction: column; gap: 8px; min-height: 0; }
.search { width: 100%; box-sizing: border-box; }
.search-results { list-style: none; margin: 0; padding: 0; max-height: 160px; overflow: auto; border: 1px solid var(--border, #333); border-radius: 6px; }
.search-results li { padding: 3px 6px; cursor: pointer; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.search-results li:hover, .search-results li.sel { background: var(--accent, #007acc); color: #fff; }
.tree { flex: 1; overflow: auto; min-height: 0; }
.tree-section-head { cursor: pointer; font-weight: 600; padding: 2px 0; }
.chev { display: inline-block; width: 14px; }
.tree-children { padding-left: 8px; }
.counters { display: flex; gap: 16px; }
.export { display: flex; flex-direction: column; gap: 6px; }
.row { display: flex; gap: 6px; align-items: center; }
.grow { flex: 1; }
.muted { color: var(--muted, #888); }
.status { white-space: pre-wrap; }
.obj-title { font-size: 16px; font-weight: 700; }
.graph-summary { white-space: pre-wrap; }
.obj-details { margin: 0; max-height: 150px; overflow: auto; white-space: pre-wrap; font-family: inherit; }
.cy { flex: 1; min-height: 260px; border: 1px solid var(--border, #333); border-radius: 6px; }
button { cursor: pointer; }
button:disabled { opacity: 0.5; cursor: default; }
</style>
