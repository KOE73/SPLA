import type {
  EntityCatalog,
  EntityEntry,
  ProjectBundle,
  ProjectManifest,
  RelationCatalog,
  TextCatalog,
  ViewDocument,
  ViewEdgePlacement,
  ViewNodePlacement,
  ViewZonePlacement,
  WireDocument,
} from "../../model/wire-types.js";
import type { ModelStore, SaveTarget } from "./types.js";

/**
 * Reads and writes multi-file project models over HTTP.
 *
 * Symmetrically coordinates:
 * - `project.json`
 * - `entities.json`
 * - `relations.json`
 * - `text.ru.json`
 * - `views/<view_id>.view.json`
 */
export class HttpProjectStore implements ModelStore {
  constructor(private readonly baseUrl: string = "./") {}

  async load(file: string): Promise<WireDocument> {
    const res = await fetch(new URL(file, new URL(this.baseUrl, location.href)));
    if (!res.ok) throw new Error(`Не удалось загрузить ${file}: HTTP ${res.status}`);
    const data = await res.json();
    return this.loadProjectBundle(file, data);
  }

  private async loadProjectBundle(viewFile: string, viewData: any): Promise<WireDocument> {
    const isView = !!viewData.project;
    let projectManifest: ProjectManifest = viewData;
    let dir = viewFile.substring(0, viewFile.lastIndexOf("/") + 1);

    if (isView) {
      dir = dir + "../";
      projectManifest = await fetch(new URL(dir + "project.json", new URL(this.baseUrl, location.href)))
        .then((r) => r.json())
        .catch(() => ({ id: viewData.project || "unknown", title: "Архитектурная схема" }));
    }

    const [entitiesRes, relationsRes, textRes]: [EntityCatalog, RelationCatalog, TextCatalog] = await Promise.all([
      fetch(new URL(dir + "entities.json", new URL(this.baseUrl, location.href)))
        .then((r) => r.json())
        .catch(() => ({ entities: [] })),
      fetch(new URL(dir + "relations.json", new URL(this.baseUrl, location.href)))
        .then((r) => r.json())
        .catch(() => ({ relations: [] })),
      fetch(new URL(dir + "text.ru.json", new URL(this.baseUrl, location.href)))
        .then((r) => r.json())
        .catch(() => ({ entries: {} })),
    ]);

    const placements = viewData.placements || viewData.nodes || [];
    const translatedNodes = placements.map((vn: any) => {
      const entityId = vn.entity || vn.id;
      const e: EntityEntry = entitiesRes.entities?.find?.((x) => x.id === entityId) ?? {
        id: entityId,
        name: entityId,
        kind: vn.type || "Component",
        codeRef: "",
      };
      const t = textRes.entries?.[entityId] ?? {
        name: e.name || entityId,
        title: e.name || entityId,
        doc: "",
        description: "",
      };
      return {
        id: entityId,
        label: t.name || t.title || e.name || entityId,
        type: vn.type || e.kind || "Component",
        zone: vn.zone || vn.container || null,
        x: vn.x,
        y: vn.y,
        width: vn.width || 170,
        height: vn.height || 50,
        styleId: vn.styleId,
        metadata: { codeRef: e.codeRef, description: t.doc || t.description },
        raw: { _entity: e },
      };
    });

    const translatedZones = (viewData.zones || []).map((vz: any) => {
      const zName = textRes.entries?.[vz.id]?.name || textRes.entries?.[vz.id]?.title || vz.name || vz.id;
      return {
        id: vz.id,
        label: zName,
        type: vz.type || "zone",
        zone: vz.parent || vz.container || null,
        x: vz.x,
        y: vz.y,
        width: vz.width,
        height: vz.height,
        styleId: vz.styleId,
        metadata: { description: textRes.entries?.[vz.id]?.doc || textRes.entries?.[vz.id]?.description },
      };
    });

    const rawEdges = Array.isArray(viewData.edges)
      ? viewData.edges
      : relationsRes.relations || [];

    const translatedEdges = rawEdges.map((ve: any, i: number) => ({
      id: ve.id || `edge_${i}`,
      from: ve.from || ve.source,
      to: ve.to || ve.target,
      type: ve.type || ve.relation || "relates",
      label: ve.label || "",
      styleId: ve.styleId,
      points: ve.points || [],
    }));

    const bundle: ProjectBundle = {
      project: projectManifest,
      entities: entitiesRes,
      relations: relationsRes,
      text: textRes,
      view: isView
        ? (viewData as ViewDocument)
        : { id: "v_main", project: projectManifest.id, zones: viewData.zones, nodes: viewData.nodes, edges: viewData.edges },
    };

    const wire: WireDocument = {
      metadata: { title: projectManifest.title, subtitle: projectManifest.subtitle },
      zones: translatedZones,
      nodes: translatedNodes,
      edges: translatedEdges,
      views: [],
      bundle,
    };

    return wire;
  }

  async save(target: SaveTarget, wire: WireDocument): Promise<void> {
    const bundle = wire.bundle;

    // If this is a project-based model, save clean view layout to target.file
    if (bundle && bundle.project && bundle.view) {
      await this.saveProjectBundle(target.file, wire, bundle);
      return;
    }

    // Fallback for standalone/legacy single-file JSON models
    const res = await fetch(`/api/save?file=${encodeURIComponent(target.file)}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(wire, null, 2),
    });
    if (!res.ok) throw new Error(`Сервер ответил HTTP ${res.status}`);
  }

  private async saveProjectBundle(viewFile: string, wire: WireDocument, bundle: ProjectBundle): Promise<void> {
    // 1. Construct clean ViewDocument matching CONTRACT.md
    const zones: ViewZonePlacement[] = (wire.zones || []).map((z) => ({
      id: z.id,
      container: (z as any).zone || (z as any).container || null,
      parent: (z as any).parent || null,
      x: z.x,
      y: z.y,
      width: z.width,
      height: z.height,
      styleId: z.styleId,
    }));

    const nodes: ViewNodePlacement[] = (wire.nodes || []).map((n) => ({
      id: n.id,
      container: (n as any).zone || (n as any).container || null,
      x: n.x,
      y: n.y,
      width: n.width,
      height: n.height,
      styleId: n.styleId,
    }));

    const edges: ViewEdgePlacement[] = (wire.edges || []).map((e) => ({
      id: e.id,
      from: e.from,
      to: e.to,
      type: e.type,
      label: e.label || "",
      styleId: e.styleId,
      points: e.points || [],
    }));

    const cleanView: ViewDocument = {
      id: bundle.view.id || "v_main",
      project: bundle.project.id,
      ...(bundle.view.relations ? { relations: bundle.view.relations } : {}),
      zones,
      nodes,
      edges,
    };

    // Save the view file
    const res = await fetch(`/api/save?file=${encodeURIComponent(viewFile)}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(cleanView, null, 2),
    });
    if (!res.ok) throw new Error(`Не удалось сохранить ${viewFile}: HTTP ${res.status}`);

    // Update text and entities if any new items were created/modified
    const dir = viewFile.substring(0, viewFile.lastIndexOf("/") + 1) + "../";
    let textModified = false;
    let entitiesModified = false;

    const currentText = bundle.text.entries || {};
    const currentEntities = bundle.entities.entities || [];
    const entityMap = new Map(currentEntities.map((e) => [e.id, e]));

    for (const n of wire.nodes || []) {
      if (!entityMap.has(n.id)) {
        currentEntities.push({
          id: n.id,
          name: n.label || n.id,
          kind: n.type || "Component",
          origin: "authored",
          status: "present",
          codeRef: n.metadata?.codeRef || "",
          members: [],
        });
        entitiesModified = true;
      }
      if (n.label && currentText[n.id]?.name !== n.label) {
        currentText[n.id] = { ...currentText[n.id], name: n.label, doc: n.metadata?.description || currentText[n.id]?.doc || "" };
        textModified = true;
      }
    }

    for (const z of wire.zones || []) {
      const zoneName = z.name;
      if (zoneName && currentText[z.id]?.name !== zoneName) {
        currentText[z.id] = { ...currentText[z.id], name: zoneName, doc: z.metadata?.description || currentText[z.id]?.doc || "" };
        textModified = true;
      }
    }

    if (entitiesModified) {
      await fetch(`/api/save?file=${encodeURIComponent(dir + "entities.json")}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ entities: currentEntities }, null, 2),
      }).catch((err) => console.warn("Не удалось синхронизировать entities.json:", err));
    }

    if (textModified) {
      await fetch(`/api/save?file=${encodeURIComponent(dir + "text.ru.json")}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ entries: currentText }, null, 2),
      }).catch((err) => console.warn("Не удалось синхронизировать text.ru.json:", err));
    }
  }
}
