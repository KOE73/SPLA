/**
 * Public API of @spla/diagram.
 *
 * Everything else under src/ is internal. Reaching past this entry point into
 * renderers or the interaction controller is not supported — those are exactly
 * the parts meant to change.
 */

import "./styles/canvas.css";

export { DiagramCanvas, defaultRegistry } from "./canvas/DiagramCanvas.js";
export type {
  CanvasEvents,
  DiagramCanvasOptions,
  Selection,
  SelectionKind,
} from "./canvas/DiagramCanvas.js";

export { DiagramEditor } from "./editor/DiagramEditor.js";
export type { CatalogEntry, DiagramEditorOptions } from "./editor/DiagramEditor.js";

export { DiagramDocument } from "./model/document.js";
export { parseDocument, serializeDocument } from "./model/wire.js";
export type {
  DiagramEdge,
  DiagramElement,
  DiagramMetadata,
  DiagramView,
  ElementKind,
  ElementStyle,
} from "./model/types.js";
export type {
  WireDocument,
  WireEdge,
  WireNode,
  WireView,
  WireZone,
} from "./model/wire-types.js";

// Extension points. Registering a renderer or swapping an algorithm is the
// supported way to change how a diagram looks, without forking the canvas.
export { TypeRegistry } from "./canvas/render/TypeRegistry.js";
export type { ElementRenderer, RenderContext } from "./canvas/render/ElementRenderer.js";
export { BoxRenderer } from "./canvas/render/BoxRenderer.js";
export { ContainerRenderer } from "./canvas/render/ContainerRenderer.js";

export {
  CenterPortAssigner,
  DiscretePortAssigner,
  UniformPortAssigner,
} from "./canvas/ports/assigners.js";
export type { PortAssigner, PortRequest } from "./canvas/ports/PortAssigner.js";

export { BezierRouter, StraightRouter } from "./canvas/routing/EdgeRouter.js";
export type { EdgeRouter, Route, RouteRequest } from "./canvas/routing/EdgeRouter.js";

export { exportDrawio } from "./editor/io/drawio.js";
export { HttpModelStore } from "./editor/io/transfer.js";
export type { ModelStore, SaveTarget } from "./editor/io/transfer.js";

export type { BoundarySlot, Point, Rect, Side } from "./geometry/types.js";
