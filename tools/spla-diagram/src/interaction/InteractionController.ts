import { center, snap } from "../geometry/rect.js";
import type { DiagramElement } from "../model/types.js";
import { elementRect, isContainer } from "../model/types.js";
import type { DiagramCanvas } from "../canvas/DiagramCanvas.js";
import { Role, hitTest, type RoleHit } from "./roles.js";

const MIN_SIZE = {
  zone: { width: 160, height: 100 },
  node: { width: 100, height: 40 },
} as const;

interface PanGesture {
  readonly kind: "pan";
  readonly startX: number;
  readonly startY: number;
  readonly panX: number;
  readonly panY: number;
}

interface MoveGesture {
  readonly kind: "move";
  readonly el: DiagramElement;
  readonly startX: number;
  readonly startY: number;
  readonly origins: ReadonlyMap<DiagramElement, { x: number; y: number }>;
}

interface ResizeGesture {
  readonly kind: "resize";
  readonly el: DiagramElement;
  readonly startX: number;
  readonly startY: number;
  readonly width: number;
  readonly height: number;
}

type Gesture = PanGesture | MoveGesture | ResizeGesture;

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
      this.gesture = {
        kind: "pan",
        startX: e.clientX,
        startY: e.clientY,
        panX: this.canvas.viewport.panX,
        panY: this.canvas.viewport.panY,
      };
      this.host.classList.add("is-panning");
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

      case Role.ResizeHandle:
        e.stopPropagation();
        this.canvas.select(el.id);
        this.canvas.emitGestureStart("resize");
        this.gesture = {
          kind: "resize",
          el,
          startX: e.clientX,
          startY: e.clientY,
          width: el.width,
          height: el.height,
        };
        return;

      case Role.DragHandle:
        e.stopPropagation();
        this.canvas.select(el.id);
        this.canvas.emitGestureStart("move");
        this.gesture = {
          kind: "move",
          el,
          startX: e.clientX,
          startY: e.clientY,
          origins: this.captureOrigins(el),
        };
        return;

      case Role.Body:
        this.canvas.select(el.id);
        return;
    }
  };

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
      case "pan": {
        this.canvas.viewport.set({
          panX: gesture.panX + (e.clientX - gesture.startX),
          panY: gesture.panY + (e.clientY - gesture.startY),
        });
        return;
      }
      case "move":
        this.applyMove(gesture, e);
        return;
      case "resize":
        this.applyResize(gesture, e);
        return;
    }
  };

  private readonly onMouseUp = (): void => {
    const gesture = this.gesture;
    this.gesture = null;
    this.host.classList.remove("is-panning");
    if (gesture === null) return;

    if (gesture.kind === "pan") return;

    if (gesture.kind === "move") {
      this.finishMove(gesture);
    }
    this.canvas.emitGestureEnd(gesture.kind);
  };

  private readonly onDoubleClick = (e: MouseEvent): void => {
    const hit = hitTest(e.target);
    if (hit === null || hit.role !== Role.ResizeHandle || hit.elementId === null) return;
    const el = this.canvas.model?.element(hit.elementId);
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
    this.canvas.zoomBy(e.deltaY < 0 ? 1.1 : 0.9);
  };

  // --------------------------------------------------------------- gestures

  /**
   * Remember where everything sat before a move.
   *
   * A container carries its whole subtree — nested containers included. Without
   * the descendants the diagram visually falls apart the moment a container
   * holding another container is moved (R-EDIT-03).
   */
  private captureOrigins(el: DiagramElement): Map<DiagramElement, { x: number; y: number }> {
    const origins = new Map<DiagramElement, { x: number; y: number }>();
    origins.set(el, { x: el.x, y: el.y });

    if (isContainer(el) && this.canvas.containerDrag) {
      const doc = this.canvas.model;
      if (doc !== null) {
        for (const child of doc.descendants(el)) {
          origins.set(child, { x: child.x, y: child.y });
        }
      }
    }
    return origins;
  }

  private applyMove(gesture: MoveGesture, e: MouseEvent): void {
    const raw = this.canvas.viewport.scaleDelta(
      e.clientX - gesture.startX,
      e.clientY - gesture.startY,
    );
    const origin = gesture.origins.get(gesture.el);
    if (origin === undefined) return;

    const step = this.canvas.gridStep;
    // Snap the resulting coordinate, not the pointer delta, so an element that
    // is on the grid stays on it across successive drags (R-EDIT-04).
    const dx = snap(origin.x + raw.x, step) - origin.x;
    const dy = snap(origin.y + raw.y, step) - origin.y;

    for (const [el, from] of gesture.origins) {
      el.x = from.x + dx;
      el.y = from.y + dy;
    }

    if (!isContainer(gesture.el)) {
      const doc = this.canvas.model;
      const target = doc?.containerAt(center(elementRect(gesture.el)), gesture.el) ?? null;
      this.canvas.setDropTarget(target?.id ?? null);
    }

    this.canvas.notifyModelChanged("move");
  }

  private applyResize(gesture: ResizeGesture, e: MouseEvent): void {
    const raw = this.canvas.viewport.scaleDelta(
      e.clientX - gesture.startX,
      e.clientY - gesture.startY,
    );
    const min = isContainer(gesture.el) ? MIN_SIZE.zone : MIN_SIZE.node;
    const step = this.canvas.gridStep;

    gesture.el.width = snap(Math.max(min.width, gesture.width + raw.x), step);
    gesture.el.height = snap(Math.max(min.height, gesture.height + raw.y), step);
    this.canvas.notifyModelChanged("resize");
  }

  private finishMove(gesture: MoveGesture): void {
    const doc = this.canvas.model;
    const dropTargetId = this.canvas.dropTargetId;
    this.canvas.setDropTarget(null);

    if (doc === null || isContainer(gesture.el)) return;

    const target = dropTargetId === null ? null : doc.element(dropTargetId) ?? null;
    if (target !== gesture.el.parent) {
      doc.reparent(gesture.el, target);
      this.canvas.notifyModelChanged("reparent");
    }
  }
}

export type { RoleHit };
