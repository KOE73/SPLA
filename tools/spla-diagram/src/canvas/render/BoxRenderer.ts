import { pointOnSide } from "../../geometry/rect.js";
import type { BoundarySlot, Point, Rect } from "../../geometry/types.js";
import type { DiagramElement } from "../../model/types.js";
import { elementRect } from "../../model/types.js";
import { ELEMENT_ATTR, ROLE_ATTR, Role } from "../../interaction/roles.js";
import { setAttrs, svg, text } from "../svg.js";
import type { ElementRenderer, RenderContext } from "./ElementRenderer.js";
import { nodeStyle } from "./styles.js";

/**
 * The default leaf renderer: a rounded rectangle with a caption, a monospace
 * subtitle and a resize grip. Covers every node type in the current models —
 * they differ only in fill, stroke and icon, which come from the style table.
 */
export class BoxRenderer implements ElementRenderer {
  create(el: DiagramElement, ctx: RenderContext): SVGGElement {
    const g = svg("g", {
      class: "spla-node",
      [ELEMENT_ATTR]: el.id,
      filter: "url(#spla-shadow)",
    });
    this.update(g, el, ctx);
    return g;
  }

  update(g: SVGGElement, el: DiagramElement, ctx: RenderContext): void {
    const style = nodeStyle(el.type);
    const selected = ctx.selectedId === el.id;

    setAttrs(g, {
      class: `spla-node${selected ? " is-selected" : ""}`,
      opacity: ctx.opacity(el),
    });
    g.replaceChildren();

    // The whole box is the drag handle: pressing anywhere on a node moves it
    // (R-EDIT-01), unlike a container, which moves only by its header.
    g.appendChild(
      svg("rect", {
        [ROLE_ATTR]: Role.DragHandle,
        x: el.x,
        y: el.y,
        width: el.width,
        height: el.height,
        rx: 8,
        fill: style.fill,
        stroke: style.stroke,
        "stroke-width": style.strokeWidth,
      }),
    );

    const tall = el.height > 60;
    g.appendChild(
      text(
        {
          x: el.x + 12,
          y: el.y + (tall ? 26 : 24),
          "font-size": 12.5,
          "font-weight": 600,
          fill: "#1e293b",
          class: "spla-node-label",
        },
        `${style.icon} ${el.label}`,
      ),
    );

    const subtitle = typeof el.metadata.type === "string" ? el.metadata.type : el.type;
    g.appendChild(
      text(
        {
          x: el.x + 12,
          y: el.y + (tall ? 46 : 42),
          "font-size": 10.5,
          "font-family": "monospace",
          fill: "#64748b",
          class: "spla-node-subtitle",
        },
        subtitle,
      ),
    );

    g.appendChild(
      svg("rect", {
        [ROLE_ATTR]: Role.ResizeHandle,
        class: "spla-node-resize",
        x: el.x + el.width - 8,
        y: el.y + el.height - 8,
        width: 8,
        height: 8,
        fill: "transparent",
      }),
    );
  }

  visibleRect(el: DiagramElement): Rect {
    return elementRect(el);
  }

  pointAt(rect: Rect, slot: BoundarySlot): Point {
    return pointOnSide(rect, slot.side, slot.t);
  }
}
