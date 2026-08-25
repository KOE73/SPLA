import { pointOnSide } from "../../geometry/rect.js";
import type { BoundarySlot, Point, Rect } from "../../geometry/types.js";
import type { DiagramElement } from "../../model/types.js";
import { ELEMENT_ATTR, ROLE_ATTR, Role } from "../../interaction/roles.js";
import { setAttrs, svg, text } from "../svg.js";
import type { ElementRenderer, RenderContext } from "./ElementRenderer.js";
import { HEADER_HEIGHT, ZONE_DEFAULTS } from "./styles.js";

/**
 * The default container renderer: a titled box with a header that doubles as
 * the drag handle, a collapse toggle, and a right-aligned semantic id.
 *
 * The body is deliberately inert — only the header drags (R-EDIT-02) — so that
 * the space inside a zone stays available for panning and for grabbing the
 * elements that live in it.
 */
export class ContainerRenderer implements ElementRenderer {
  create(el: DiagramElement, ctx: RenderContext): SVGGElement {
    const g = svg("g", { class: "spla-zone", [ELEMENT_ATTR]: el.id });
    this.update(g, el, ctx);
    return g;
  }

  update(g: SVGGElement, el: DiagramElement, ctx: RenderContext): void {
    const collapsed = ctx.isCollapsed(el);
    const selected = ctx.isSelected(el);
    const dropTarget = ctx.dropTargetId === el.id;
    const style = { ...ZONE_DEFAULTS, ...(el.style ?? {}) };
    const height = collapsed ? HEADER_HEIGHT : el.height;

    setAttrs(g, {
      class: [
        "spla-zone",
        selected ? "is-selected" : "",
        dropTarget ? "is-drop-target" : "",
        collapsed ? "is-collapsed" : "",
      ]
        .filter(Boolean)
        .join(" "),
      opacity: ctx.opacity(el),
    });
    g.replaceChildren();

    g.appendChild(
      svg("rect", {
        [ROLE_ATTR]: Role.Body,
        class: "spla-zone-body",
        x: el.x,
        y: el.y,
        width: el.width,
        height,
        rx: 12,
        fill: style.fill,
        stroke: style.stroke,
        "stroke-width": style.strokeWidth,
        "stroke-dasharray": collapsed ? "none" : style.strokeDasharray,
      }),
    );

    g.appendChild(
      svg("rect", {
        [ROLE_ATTR]: Role.DragHandle,
        class: "spla-zone-header",
        x: el.x,
        y: el.y,
        width: el.width,
        height: HEADER_HEIGHT,
        rx: 12,
        fill: style.headerBg,
      }),
    );

    g.appendChild(this.collapseToggle(el, collapsed));

    const childCount = el.children.filter((c) => c.kind === "node").length;
    g.appendChild(
      text(
        {
          x: el.x + 36,
          y: el.y + 22,
          "font-size": 13,
          "font-weight": 700,
          fill: "#334155",
          class: "spla-zone-title",
        },
        collapsed ? `${el.label} (${childCount} компонентов)` : el.label,
      ),
    );

    if (el.semanticId !== undefined && el.semanticId !== "") {
      g.appendChild(
        text(
          {
            x: el.x + el.width - 14,
            y: el.y + 21,
            "font-size": 10,
            "font-family": "monospace",
            "text-anchor": "end",
            fill: "#64748b",
            class: "spla-zone-semantic",
          },
          el.semanticId,
        ),
      );
    }

  }

  private collapseToggle(el: DiagramElement, collapsed: boolean): SVGGElement {
    return svg(
      "g",
      { [ROLE_ATTR]: Role.CollapseToggle, class: "spla-zone-collapse" },
      [
        svg("rect", { x: el.x + 8, y: el.y + 7, width: 20, height: 20, rx: 5 }),
        text(
          {
            x: el.x + 18,
            y: el.y + 22,
            "font-size": 14,
            "text-anchor": "middle",
          },
          collapsed ? "+" : "−",
        ),
      ],
    );
  }

  visibleRect(el: DiagramElement, ctx: RenderContext): Rect {
    return {
      x: el.x,
      y: el.y,
      width: el.width,
      height: ctx.isCollapsed(el) ? HEADER_HEIGHT : el.height,
    };
  }

  pointAt(rect: Rect, slot: BoundarySlot): Point {
    return pointOnSide(rect, slot.side, slot.t);
  }
}
