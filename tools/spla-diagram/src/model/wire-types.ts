/**
 * SPLA Diagram Project Format types.
 *
 * Describes the single unified project structure on disk:
 * - project.json (manifest)
 * - entities.json (entities catalog)
 * - relations.json (relations catalog)
 * - text.<lang>.json (localized descriptions and names)
 * - views/<view_id>.view.json (view layouts: zones, nodes, edges)
 */

export interface WireMetadata {
  type?: string;
  kind?: string;
  description?: string;
  codeRef?: string;
  responsibilities?: string[];
  [key: string]: unknown;
}

export interface WireZone {
  id: string;
  name?: string;
  type?: string;
  semanticId?: string;
  tags?: string[];
  x: number;
  y: number;
  width: number;
  height: number;
  styleId?: string;
  metadata?: WireMetadata;
}

export interface WireNode {
  id: string;
  label?: string;
  type?: string;
  /** Declared parent zone id. Null, absent, or dangling all mean "not declared". */
  zone?: string | null;
  x: number;
  y: number;
  width: number;
  height: number;
  tags?: string[];
  styleId?: string;
  metadata?: WireMetadata;
}

export interface WireEdge {
  id: string;
  from: string;
  to: string;
  label?: string;
  type?: string;
  styleId?: string;
  points?: Array<{ x: number; y: number }>;
}

export interface WireView {
  id: string;
  name?: string;
  icon?: string;
  description?: string;
  highlightZones?: string[];
  highlightNodes?: string[];
}

export interface WireDocumentMetadata {
  title?: string;
  subtitle?: string;
  layout?: string;
  description?: string;
  [key: string]: unknown;
}

export interface WireDocument {
  metadata?: WireDocumentMetadata;
  views?: WireView[];
  zones?: WireZone[];
  nodes?: WireNode[];
  edges?: WireEdge[];
  bundle?: ProjectBundle;
}

// ------------------------------------------------------------- Project Bundle

export interface EntityEntry {
  id: string;
  name: string;
  kind: string;
  origin?: "code" | "authored";
  status?: "present" | "missing" | "planned";
  namespace?: string;
  codeRef?: string;
  members?: string[];
  [key: string]: unknown;
}

export interface EntityCatalog {
  entities: EntityEntry[];
}

export interface RelationEvidence {
  codeRef?: string;
  symbol?: string;
  line?: number;
}

export interface RelationEntry {
  id: string;
  from: string;
  to: string;
  type: string;
  relation?: string;
  label?: string;
  styleId?: string;
  origin?: "code" | "authored";
  status?: "present" | "missing";
  evidence?: RelationEvidence[];
  points?: Array<{ x: number; y: number }>;
  [key: string]: unknown;
}

export interface RelationCatalog {
  relations: RelationEntry[];
}

export interface TextCatalog {
  entries: Record<string, { name?: string; title?: string; doc?: string; description?: string }>;
}

export interface ProjectManifest {
  id: string;
  title: string;
  subtitle?: string;
  defaultView?: string;
  languages?: string[];
  views?: string[];
  [key: string]: unknown;
}

export interface ViewZonePlacement {
  id: string;
  container?: string | null;
  parent?: string | null;
  x: number;
  y: number;
  width: number;
  height: number;
  styleId?: string;
  collapsed?: boolean;
}

export interface ViewNodePlacement {
  id?: string;
  entity?: string;
  container?: string | null;
  zone?: string | null;
  x: number;
  y: number;
  width?: number;
  height?: number;
  styleId?: string;
}

export interface ViewEdgePlacement {
  id: string;
  from: string;
  to: string;
  type?: string;
  relation?: string;
  label?: string;
  styleId?: string;
  points?: Array<{ x: number; y: number }>;
}

export interface ViewDocument {
  id: string;
  project: string;
  zones?: ViewZonePlacement[];
  nodes?: ViewNodePlacement[];
  placements?: ViewNodePlacement[];
  edges?: ViewEdgePlacement[];
  [key: string]: unknown;
}

export interface ProjectBundle {
  project: ProjectManifest;
  entities: EntityCatalog;
  relations: RelationCatalog;
  text: TextCatalog;
  view: ViewDocument;
}
