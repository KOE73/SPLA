<template>
  <div class="onec-browser">
    <aside class="onec-sidebar">
      <section class="onec-card onec-explorer">
        <div class="onec-heading">
          <strong>1C Configuration</strong>
          <button title="Refresh index summary" @click="loadOverview">↻</button>
        </div>

        <input v-model="query" class="onec-search" placeholder="Search objects…" spellcheck="false" />
        <div v-if="searching" class="onec-muted onec-inline-state">Searching…</div>
        <ul v-else-if="searchResults.length" class="onec-search-results">
          <li v-for="item in searchResults" :key="item.fullName">
            <button :class="{ selected: selected?.fullName === item.fullName }" @click="selectObject(item.fullName)">
              <span>{{ item.fullName }}</span><small>{{ item.kind }}</small>
            </button>
          </li>
        </ul>

        <div class="onec-tree">
          <section v-for="section in sections" :key="section.kind" class="onec-tree-section">
            <button class="onec-section-head" @click="openSections[section.kind] = !openSections[section.kind]">
              <span>{{ openSections[section.kind] ? "▾" : "▸" }}</span>
              <strong>{{ section.kind }}</strong>
              <small>{{ section.count }}</small>
            </button>
            <div v-if="openSections[section.kind]" class="onec-section-objects">
              <TreeNode
                v-for="node in section.objects"
                :key="node.fullName"
                :node="node"
                :selected="selected?.fullName"
                @select="selectObject($event.fullName)"
              />
            </div>
          </section>
          <div v-if="!loading && !sections.length" class="onec-empty">
            The OneC index is empty. Build it below or run <code>onec_build_index</code>.
          </div>
        </div>
      </section>

      <section class="onec-card onec-summary">
        <div><span>Objects</span><strong>{{ overview.objectCount }}</strong></div>
        <div><span>Relations</span><strong>{{ overview.relationCount }}</strong></div>
        <div><span>Sections</span><strong>{{ overview.sectionCount }}</strong></div>
      </section>

      <section class="onec-card onec-rebuild">
        <strong>Build index</strong>
        <p>Path inside the current project workspace.</p>
        <div class="onec-row">
          <input v-model="rebuildPath" class="onec-grow" placeholder="e.g. configuration/" spellcheck="false" />
          <button :disabled="rebuilding || !rebuildPath.trim()" @click="rebuildIndex">
            {{ rebuilding ? "Building…" : "Build" }}
          </button>
        </div>
      </section>
    </aside>

    <main class="onec-main onec-card">
      <header class="onec-object-header">
        <div>
          <strong>{{ selected?.fullName || "Choose an object" }}</strong>
          <span v-if="selected" class="onec-kind">{{ selected.kind }}</span>
        </div>
        <div class="onec-graph-controls">
          <select v-model="graphMode" :disabled="!selected">
            <option value="dependencies">Dependencies</option>
            <option value="references">References</option>
            <option value="dataflow">Data flow</option>
          </select>
          <label>Depth <input v-model.number="depth" type="number" min="1" max="8" /></label>
          <label>Edges <input v-model.number="limit" type="number" min="1" max="1000" step="25" /></label>
          <button :disabled="!selected || graphLoading" @click="loadGraph">
            {{ graphLoading ? "Loading…" : "Show graph" }}
          </button>
        </div>
      </header>

      <div v-if="selected" class="onec-object-details">
        <div><span>Name</span>{{ selected.name }}</div>
        <div><span>Path</span>{{ selected.path || "—" }}</div>
        <div><span>Summary</span>{{ selected.summary || "—" }}</div>
      </div>

      <div class="onec-graph-status">
        <span>{{ graphStatus }}</span>
        <span v-if="graphSummary">
          {{ graphSummary.nodeCount }} nodes · {{ graphSummary.edgeCount }} edges · depth {{ graphSummary.depth }}
          <b v-if="graphSummary.truncated"> · truncated</b>
        </span>
      </div>
      <div ref="graphElement" class="onec-graph"></div>

      <div v-if="status" class="onec-status" :class="{ error: statusIsError }">{{ status }}</div>
    </main>
  </div>
</template>

<script setup lang="ts">
import cytoscape, {
  type Core,
  type EdgeSingular,
  type ElementDefinition,
  type NodeSingular,
  type StylesheetJson
} from "cytoscape";
import { onBeforeUnmount, onMounted, reactive, ref, watch } from "vue";
import type { MountApi } from "./mount";
import TreeNode, { type TreeObject } from "./TreeNode.vue";

interface ObjectDto {
  name: string;
  kind: string;
  fullName: string;
  path: string;
  summary: string;
}

interface SectionDto {
  kind: string;
  count: number;
  objects: TreeObject[];
}

interface OverviewDto {
  objectCount: number;
  relationCount: number;
  sectionCount: number;
  treeTruncated: boolean;
  sections: SectionDto[];
}

interface GraphNodeDto {
  id: string;
  label: string;
  kind: string;
  isCenter: boolean;
}

interface GraphEdgeDto {
  id: string;
  source: string;
  target: string;
  type: string;
}

interface GraphSummaryDto {
  rootFullName: string;
  mode: string;
  depth: number;
  limit: number;
  nodeCount: number;
  edgeCount: number;
  truncated: boolean;
  relationTypeCounts: Record<string, number>;
}

interface GraphDto {
  summary: GraphSummaryDto;
  nodes: GraphNodeDto[];
  edges: GraphEdgeDto[];
}

const props = defineProps<{ api: MountApi }>();
const overview = reactive({ objectCount: 0, relationCount: 0, sectionCount: 0 });
const sections = ref<SectionDto[]>([]);
const openSections = reactive<Record<string, boolean>>({});
const query = ref("");
const searching = ref(false);
const searchResults = ref<ObjectDto[]>([]);
const selected = ref<ObjectDto | null>(null);
const rebuildPath = ref("");
const rebuilding = ref(false);
const loading = ref(false);
const status = ref("");
const statusIsError = ref(false);
const graphMode = ref("dependencies");
const depth = ref(2);
const limit = ref(250);
const graphLoading = ref(false);
const graphStatus = ref("Select an object and load a graph.");
const graphSummary = ref<GraphSummaryDto | null>(null);
const graphElement = ref<HTMLElement | null>(null);
let graph: Core | null = null;
let resizeObserver: ResizeObserver | null = null;
let searchTimer: number | undefined;
let searchSequence = 0;

async function action<Result>(name: string, payload: Record<string, unknown> = {}): Promise<Result> {
  const response = await props.api.invoke<{ ok: boolean; resultJson?: string; error?: string }>("plugin.action", {
    pluginId: "onec",
    action: name,
    valueJson: JSON.stringify(payload)
  });
  if (!response.ok) throw new Error(response.error || `OneC action '${name}' failed.`);
  return (response.resultJson ? JSON.parse(response.resultJson) : null) as Result;
}

async function loadOverview() {
  loading.value = true;
  try {
    const result = await action<OverviewDto>("overview");
    overview.objectCount = result.objectCount;
    overview.relationCount = result.relationCount;
    overview.sectionCount = result.sectionCount;
    sections.value = result.sections;
    for (const section of result.sections)
      if (!(section.kind in openSections)) openSections[section.kind] = false;
    if (result.treeTruncated) setStatus("The navigation tree is limited to 20,000 indexed objects.");
    else if (status.value.startsWith("The navigation tree")) setStatus("");
  } catch (error) {
    setStatus(`Unable to load the OneC index: ${message(error)}`, true);
  } finally {
    loading.value = false;
  }
}

watch(query, value => {
  window.clearTimeout(searchTimer);
  const sequence = ++searchSequence;
  searchTimer = window.setTimeout(async () => {
    if (!value.trim()) {
      searchResults.value = [];
      searching.value = false;
      return;
    }
    searching.value = true;
    try {
      const result = await action<{ results: ObjectDto[] }>("search", { query: value });
      if (sequence === searchSequence) searchResults.value = result.results;
    } catch (error) {
      if (sequence === searchSequence) setStatus(`Search failed: ${message(error)}`, true);
    } finally {
      if (sequence === searchSequence) searching.value = false;
    }
  }, 180);
});

async function selectObject(fullName: string) {
  try {
    selected.value = await action<ObjectDto | null>("object", { fullName });
    graphSummary.value = null;
    graphStatus.value = selected.value ? "Choose a graph mode." : "The selected object no longer exists.";
    graph?.elements().remove();
  } catch (error) {
    setStatus(`Unable to load the object: ${message(error)}`, true);
  }
}

async function loadGraph() {
  if (!selected.value) return;
  graphLoading.value = true;
  try {
    const result = await action<GraphDto>("graph", {
      fullName: selected.value.fullName,
      mode: graphMode.value,
      depth: depth.value,
      limit: limit.value
    });
    graphSummary.value = result.summary;
    graphStatus.value = result.nodes.length ? "" : "No graph data was found for this object.";
    renderGraph(result);
  } catch (error) {
    graphStatus.value = `Graph failed: ${message(error)}`;
  } finally {
    graphLoading.value = false;
  }
}

async function rebuildIndex() {
  rebuilding.value = true;
  setStatus("Indexing the configuration…");
  try {
    const result = await action<{
      objectsAdded: number;
      objectsUpdated: number;
      relationsAdded: number;
      filesSkipped: number;
      filesWithErrors: number;
      elapsedSeconds: number;
    }>("rebuild", { path: rebuildPath.value });
    setStatus(
      `Index ready: +${result.objectsAdded} objects, ${result.objectsUpdated} updated, `
      + `+${result.relationsAdded} relations, ${result.filesSkipped} skipped, `
      + `${result.filesWithErrors} errors in ${result.elapsedSeconds}s.`
    );
    await loadOverview();
  } catch (error) {
    setStatus(`Index build failed: ${message(error)}`, true);
  } finally {
    rebuilding.value = false;
  }
}

const kindColors: Record<string, string> = {
  Document: "#6ea8fe",
  Catalog: "#64c487",
  AccumulationRegister: "#e8798a",
  InformationRegister: "#e6a15a",
  AccountingRegister: "#d67586",
  CalculationRegister: "#d67586",
  CommonModule: "#ad8be8",
  Report: "#d8bc68",
  Processing: "#d8bc68",
  Form: "#54b8ad",
  TabularSection: "#54b8ad"
};

const relationColors: Record<string, string> = {
  writes: "#e8798a",
  reads: "#6ea8fe",
  queries: "#e6a15a",
  calls: "#64c487",
  owns: "#7d8590",
  uses: "#ad8be8"
};

const graphStyles: StylesheetJson = [
  {
    selector: "node",
    style: {
      "background-color": (node: NodeSingular) => kindColors[String(node.data("kind"))] || "#7d8590",
      label: "data(label)",
      color: "#d8dee9",
      "font-size": 10,
      "text-valign": "bottom",
      "text-margin-y": 5,
      "text-outline-width": 2,
      "text-outline-color": "#20242b",
      width: 34,
      height: 34
    }
  },
  {
    selector: "node[?isCenter]",
    style: {
      width: 50,
      height: 50,
      "border-width": 3,
      "border-color": "#d8dee9",
      "font-size": 12,
      "font-weight": "bold"
    }
  },
  {
    selector: "edge",
    style: {
      "line-color": (edge: EdgeSingular) => relationColors[String(edge.data("type"))] || "#7d8590",
      "target-arrow-color": (edge: EdgeSingular) => relationColors[String(edge.data("type"))] || "#7d8590",
      "target-arrow-shape": "triangle",
      "curve-style": "bezier",
      width: 1.5,
      opacity: 0.82,
      label: "data(type)",
      "font-size": 8,
      color: "#8b949e",
      "text-rotation": "autorotate"
    }
  }
];

function renderGraph(result: GraphDto) {
  if (!graphElement.value) return;
  const elements: ElementDefinition[] = [
    ...result.nodes.map(node => ({ data: node })),
    ...result.edges.map(edge => ({ data: edge }))
  ];
  graph?.destroy();
  graph = cytoscape({
    container: graphElement.value,
    elements,
    style: graphStyles,
    minZoom: 0.05,
    maxZoom: 1.8,
    wheelSensitivity: 0.25,
    layout: {
      name: "cose",
      animate: false,
      padding: 35,
      nodeRepulsion: () => 6_500,
      idealEdgeLength: () => 120
    }
  });
  window.setTimeout(() => graph?.fit(undefined, 30), 80);
}

function setStatus(value: string, error = false) {
  status.value = value;
  statusIsError.value = error;
}

function message(error: unknown) {
  return error instanceof Error ? error.message : String(error);
}

onMounted(() => {
  void loadOverview();
  if (graphElement.value) {
    resizeObserver = new ResizeObserver(() => graph?.resize());
    resizeObserver.observe(graphElement.value);
  }
});

onBeforeUnmount(() => {
  window.clearTimeout(searchTimer);
  resizeObserver?.disconnect();
  graph?.destroy();
});
</script>
