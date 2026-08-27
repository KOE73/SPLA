import { center, snap } from "../geometry/rect.js";
import type { Rect } from "../geometry/types.js";
import type { DiagramElement } from "../model/types.js";
import { elementRect, isContainer } from "../model/types.js";
import type { DiagramCanvas } from "../canvas/DiagramCanvas.js";
import type { ResizeDirection } from "../canvas/render/handles.js";
import { Role, hitTest, type RoleHit } from "./roles.js";

const MIN_SIZE = {
  zone: { width: 160, height: 100 },
  node: { width: 100, height: 40 },
} as const;

interface Origin {
  readonly x: number;
  readonly y: number;
  readonly width: number;
  readonly height: number;
}

interface PanGesture {
  readonly kind: "pan";
  readonly startX: number;
  readonly startY: number;
  readonly panX: number;
  readonly panY: number;
}

interface MoveGesture {
  readonly kind: "move";
  readonly lead: DiagramElement;
  readonly startX: number;
  readonly startY: number;
  readonly origins: ReadonlyMap<DiagramElement, Origin>;
  /** The elements the user actually selected, without their carried subtrees. */
  readonly moved: readonly DiagramElement[];
}

interface ResizeGesture {
  readonly kind: "resize";
  readonly dir: ResizeDirection;
  readonly startX: number;
  readonly startY: number;
  /** The box the grips were drawn around: one element, or the whole selection. */
  readonly bounds: Rect;
  readonly origins: ReadonlyMap<DiagramElement, Origin>;
  /** True when the box covers several elements and they scale proportionally. */
  readonly group: boolean;
  readonly single: DiagramElement | null;
}

interface MarqueeGesture {
  readonly kind: "marquee";
  readonly startX: number;
  readonly startY: number;
  readonly additive: boolean;
  readonly base: readonly string[];
}

type Gesture = PanGesture | MoveGesture | ResizeGesture | MarqueeGesture;

/**
 * Turns pointer input into model changes.
 *
 * Listens once on the canvas host and resolves what was hit through the
 * `data-role` marks renderers leave behind. No renderer is named here, and no
 * handler is attached to an individual element — which is what lets a renderer
 * added from outside behave like the built-in ones, and what stops thousands of
 * closures from being rebuilt on every frame.
 */
export class InteractionController {
  private gesture: Gesture | null = null;
  private readonly abort = new AbortController();

  constructor(
    private readonly canvas: DiagramCanvas,
    private readonly host: HTMLElement,
  ) {
    const signal = this.abort.signal;
    host.addEventListener("mousedown", this.onMouseDown, { signal });
    host.addEventListener("mouseup", this.onMouseUpTarget, { signal });
    host.addEventListener("dblclick", this.onDoubleClick, { signal });
    host.addEventListener("wheel", this.onWheel, { passive: false, signal });
    window.addEventListener("mousemove", this.onMouseMove, { signal });
    window.addEventListener("mouseup", this.onMouseUp, { signal });
  }

  destroy(): void {
    this.abort.abort();
  }

  // ------------------------------------------------------------- listeners

  private readonly onMouseDown = (e: MouseEvent): void => {
    if (e.button !== 0) return;
    const hit = hitTest(e.target);

    if (hit === null) {
      this.startOnEmptyCanvas(e);
      return;
    }

    if (hit.role === Role.ResizeHandle) {
      e.stopPropagation();
      this.startResize(e, hit);
      return;
    }

    if (hit.edgeId !== null && hit.elementId === null) {
      this.canvas.select(hit.edgeId);
      return;
    }

    const el = hit.elementId === null ? null : this.canvas.model?.element(hit.elementId) ?? null;
    if (el === null) return;

    switch (hit.role) {
      case Role.CollapseToggle:
        // Handled on mouse-up so that pressing the button never starts a drag.
        e.stopPropagation();
        return;

      case Role.DragHandle:
        e.stopPropagation();
        this.applyClickSelection(el, e);
        if (!this.canvas.selectedIds.has(el.id)) return;
        this.canvas.emitGestureStart("move");
        this.gesture = this.buildMove(el, e);
        return;

      case Role.Body:
        this.applyClickSelection(el, e);
        return;

      default:
        return;
    }
  };

  /**
   * Ctrl or Shift adds to the selection; a plain click replaces it. Clicking an
   * element that is already part of a multi-selection keeps the group, so that
   * dragging it moves everything rather than collapsing to one.
   */
  private applyClickSelection(el: DiagramElement, e: MouseEvent): void {
    if (e.ctrlKey || e.metaKey || e.shiftKey) {
      this.canvas.toggleSelected(el.id);
      return;
    }
    if (this.canvas.selectedIds.has(el.id) && this.canvas.selectedIds.size > 1) {
      return;
    }
    this.canvas.select(el.id);
  }

  private startOnEmptyCanvas(e: MouseEvent): void {
    const additive = e.ctrlKey || e.metaKey || e.shiftKey;
    if (additive) {
      this.gesture = {
        kind: "marquee",
        startX: e.clientX,
        startY: e.clientY,
        additive: true,
        base: [...this.canvas.selectedIds],
      };
      return;
    }

    this.canvas.select(null);
    this.gesture = {
      kind: "pan",
      startX: e.clientX,
      startY: e.clientY,
      panX: this.canvas.viewport.panX,
      panY: this.canvas.viewport.panY,
    };
    this.host.classList.add("is-panning");
  }

  private readonly onMouseUpTarget = (e: MouseEvent): void => {
    const hit = hitTest(e.target);
    if (hit?.role === Role.CollapseToggle && hit.elementId !== null) {
      e.stopPropagation();
      this.canvas.toggleCollapse(hit.elementId);
    }
  };

  private readonly onMouseMove = (e: MouseEvent): void => {
    const gesture = this.gesture;
    if (gesture === null) return;

    switch (gesture.kind) {
      case "pan":
        this.canvas.viewport.set({
          panX: gesture.panX + (e.clientX - gesture.startX),
          panY: gesture.panY + (e.clientY - gesture.startY),
        });
        return;
      case "move":
        this.applyMove(gesture, e);
        return;
      case "resize":
        this.applyResize(gesture, e);
        return;
      case "marquee":
        this.applyMarquee(gesture, e);
        return;
    }
  };

  private readonly onMouseUp = (): void => {
    const gesture = this.gesture;
    this.gesture = null;
    this.host.classList.remove("is-panning");
    if (gesture === null) return;

    if (gesture.kind === "pan") return;

    if (gesture.kind === "marquee") {
      this.canvas.marqueeRect = null;
      this.canvas.render();
      return;
    }

    if (gesture.kind === "move") {
      this.finishMove(gesture);
    }
    this.canvas.emitGestureEnd(gesture.kind);
  };

  private readonly onDoubleClick = (e: MouseEvent): void => {
    const hit = hitTest(e.target);
    if (hit === null || hit.role !== Role.ResizeHandle) return;

    const elements = this.canvas.selectedElements();
    const el = elements.length === 1 ? elements[0] : undefined;
    if (el === undefined || isContainer(el)) return;

    e.stopPropagation();
    const fitted = Math.max(120, el.label.length * 8 + 40);
    if (fitted === el.width) return;
    this.canvas.emitGestureStart("fit-width");
    el.width = fitted;
    this.canvas.notifyModelChanged("fit-width");
    this.canvas.emitGestureEnd("fit-width");
  };

  private readonly onWheel = (e: WheelEvent): void => {
    e.preventDefault();
    this.canvas.zoomAtClient(e.deltaY < 0 ? 1.1 : 0.9, e.clientX, e.clientY);
  };

  // --------------------------------------------------------------- gestures

  /**
   * Remember where everything sat before a move.
   *
   * Everything selected moves together, and a container carries its whole
   * subtree — nested containers included. Without the descendants the diagram
   * visually falls apart the moment a container holding another container is
   * moved (R-EDIT-03).
   */
  private buildMove(lead: DiagramElement, e: MouseEvent): MoveGesture {
    const doc = this.canvas.model;
    const origins = new Map<DiagramElement, Origin>();
    const moved = this.canvas.selectedElements();
    const targets = moved.length > 0 ? moved : [lead];

    const remember = (el: DiagramElement): void => {
      if (!origins.has(el)) origins.set(el, snapshotOf(el));
    };

    for (const el of targets) {
      remember(el);
      if (isContainer(el) && this.canvas.containerDrag && doc !== null) {
        for (const child of doc.descendants(el)) remember(child);
      }
    }

    return {
      kind: "move",
      lead,
      startX: e.clientX,
      startY: e.clientY,
      origins,
      moved: targets,
    };
  }

  private applyMove(gesture: MoveGesture, e: MouseEvent): void {
    const raw = this.canvas.viewport.scaleDelta(
      e.clientX - gesture.startX,
      e.clientY - gesture.startY,
    );
    const origin = gesture.origins.get(gesture.lead);
    if (origin === undefined) return;

    const step = this.canvas.gridStep;
    // Snap the resulting coordinate, not the pointer delta, so an element that
    // is on the grid stays on it across successive drags (R-EDIT-04). The whole
    // group shifts by the lead element's snapped delta, keeping relative
    // positions exact.
    const dx = snap(origin.x + raw.x, step) - origin.x;
    const dy = snap(origin.y + raw.y, step) - origin.y;

    for (const [el, from] of gesture.origins) {
      el.x = from.x + dx;
      el.y = from.y + dy;
    }

    // Reparenting targets a single dragged element, leaf or container alike;
    // moving a multi-selection into a container is a different operation and
    // is left alone. `containerAt` excludes the dragged element's own subtree,
    // so a container can never be dropped into itself or a descendant.
    if (gesture.moved.length === 1) {
      const doc = this.canvas.model;
      const target = doc?.containerAt(center(elementRect(gesture.lead)), gesture.lead) ?? null;
      this.canvas.setDropTarget(target?.id ?? null);
    }

    this.canvas.notifyModelChanged("move");
  }

  private finishMove(gesture: MoveGesture): void {
    const doc = this.canvas.model;
    const dropTargetId = this.canvas.dropTargetId;
    this.canvas.setDropTarget(null);

    if (doc === null) return;
    if (gesture.moved.length !== 1) return;

    const target = dropTargetId === null ? null : doc.element(dropTargetId) ?? null;
    if (target !== gesture.lead.parent) {
      doc.reparent(gesture.lead, target);
      this.canvas.notifyModelChanged("reparent");
    }
  }

  private startResize(e: MouseEvent, hit: RoleHit): void {
    const dir = (hit.handle ?? "se") as ResizeDirection;
    const bounds = this.canvas.selectionBounds();
    const elements = this.canvas.selectedElements();
    if (bounds === null || elements.length === 0) return;

    const origins = new Map<DiagramElement, Origin>();
    const doc = this.canvas.model;
    for (const el of elements) {
      origins.set(el, snapshotOf(el));
      // Scaling a group moves whatever those containers hold, so their contents
      // must travel too or the diagram comes apart.
      if (elements.length > 1 && isContainer(el) && doc !== null) {
        for (const child of doc.descendants(el)) {
          if (!origins.has(child)) origins.set(child, snapshotOf(child));
        }
      }
    }

    this.canvas.emitGestureStart("resize");
    this.gesture = {
      kind: "resize",
      dir,
      startX: e.clientX,
      startY: e.clientY,
      bounds,
      origins,
      group: elements.length > 1,
      single: elements.length === 1 ? elements[0]! : null,
    };
  }

  private applyResize(gesture: ResizeGesture, e: MouseEvent): void {
    const raw = this.canvas.viewport.scaleDelta(
      e.clientX - gesture.startX,
      e.clientY - gesture.startY,
    );
    const step = this.canvas.gridStep;
    const min = gesture.single !== null && isContainer(gesture.single)
      ? MIN_SIZE.zone
      : MIN_SIZE.node;

    const next = resizeRect(gesture.bounds, gesture.dir, raw.x, raw.y, min, step);

    if (gesture.single !== null) {
      const el = gesture.single;
      el.x = next.x;
      el.y = next.y;
      el.width = next.width;
      el.height = next.height;
    } else {
      this.scaleGroup(gesture, next);
    }

    this.canvas.notifyModelChanged("resize");
  }

  /**
   * Map every element from the original selection box into the new one.
   *
   * Positions and sizes scale proportionally, so the arrangement the author
   * built is preserved — the group is stretched, not re-laid-out.
   */
  private scaleGroup(gesture: ResizeGesture, next: Rect): void {
    const from = gesture.bounds;
    const sx = from.width === 0 ? 1 : next.width / from.width;
    const sy = from.height === 0 ? 1 : next.height / from.height;

    for (const [el, origin] of gesture.origins) {
      el.x = next.x + (origin.x - from.x) * sx;
      el.y = next.y + (origin.y - from.y) * sy;
      el.width = Math.max(20, origin.width * sx);
      el.height = Math.max(16, origin.height * sy);
    }
  }

  private applyMarquee(gesture: MarqueeGesture, e: MouseEvent): void {
    const a = this.canvas.toModel(gesture.startX, gesture.startY);
    const b = this.canvas.toModel(e.clientX, e.clientY);
    const rect: Rect = {
      x: Math.min(a.x, b.x),
      y: Math.min(a.y, b.y),
      width: Math.abs(b.x - a.x),
      height: Math.abs(b.y - a.y),
    };
    this.canvas.marqueeRect = rect;

    const doc = this.canvas.model;
    if (doc === null) return;

    const caught: string[] = [];
    for (const el of doc.elements()) {
      const r = elementRect(el);
      // Fully enclosed only: brushing past an element must not grab it.
      const inside =
        r.x >= rect.x &&
        r.y >= rect.y &&
        r.x + r.width <= rect.x + rect.width &&
        r.y + r.height <= rect.y + rect.height;
      if (inside) caught.push(el.id);
    }

    const ids = gesture.additive ? [...new Set([...gesture.base, ...caught])] : caught;
    this.canvas.selectMany(ids);
  }
}

function snapshotOf(el: DiagramElement): Origin {
  return { x: el.x, y: el.y, width: el.width, height: el.height };
}

/**
 * Apply a drag on one grip to a rectangle.
 *
 * North and west grips move the origin as well as the size, which is what makes
 * resizing from any side behave the way people expect: the opposite edge stays
 * put.
 */
function resizeRect(
  rect: Rect,
  dir: ResizeDirection,
  dx: number,
  dy: number,
  min: { width: number; height: number },
  step: number,
): Rect {
  let { x, y, width, height } = rect;

  if (dir.includes("e")) {
    width = Math.max(min.width, snap(rect.width + dx, step));
  }
  if (dir.includes("s")) {
    height = Math.max(min.height, snap(rect.height + dy, step));
  }
  if (dir.includes("w")) {
    const right = rect.x + rect.width;
    x = Math.min(snap(rect.x + dx, step), right - min.width);
    width = right - x;
  }
  if (dir.includes("n")) {
    const bottom = rect.y + rect.height;
    y = Math.min(snap(rect.y + dy, step), bottom - min.height);
    height = bottom - y;
  }

  return { x, y, width, height };
}

export type { RoleHit };
