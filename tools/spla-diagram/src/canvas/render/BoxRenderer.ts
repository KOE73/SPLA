import { pointOnSide } from "../../geometry/rect.js";
import type { BoundarySlot, Point, Rect } from "../../geometry/types.js";
import type { DiagramElement } from "../../model/types.js";
import { elementRect } from "../../model/types.js";
import { ELEMENT_ATTR, ROLE_ATTR, Role } from "../../interaction/roles.js";
import { setAttrs, svg, text } from "../svg.js";
import type { ElementRenderer, RenderContext } from "./ElementRenderer.js";
import { alignX, dashArray, textAttrs } from "./textAttrs.js";
import { resolveElementRelations } from "../../model/relations-resolver.js";

/**
 * The default leaf renderer: a rounded rectangle with a caption, a subtitle and
 * nothing else. Covers every node type in the current models — they differ only
 * in the style they resolve to, which this renderer reads and does not choose.
 */
export class BoxRenderer implements ElementRenderer {
  create(el: DiagramElement, ctx: RenderContext): SVGGElement {
    const g = svg("g", {
      class: "spla-node",
      [ELEMENT_ATTR]: el.id,
    });
    this.update(g, el, ctx);
    return g;
  }

  update(g: SVGGElement, el: DiagramElement, ctx: RenderContext): void {
    const style = ctx.styleOf(el);
    const selected = ctx.isSelected(el);
    const rect = elementRect(el);

    setAttrs(g, {
      class: `spla-node${selected ? " is-selected" : ""}`,
      opacity: ctx.opacity(el),
      // Per style, not per renderer: a flat "note" and a raised "service" are
      // the same shape and differ only in whether they cast a shadow.
      filter: style.shadow ? "url(#spla-shadow)" : null,
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
        rx: style.radius,
        fill: ctx.paints.fill(style.fill),
        stroke: style.border.color,
        "stroke-width": style.border.width,
        "stroke-dasharray": dashArray(style.border.dash),
        "stroke-opacity": style.border.opacity === 1 ? null : style.border.opacity,
      }),
    );

    // Two captions in a short box crowd each other; a tall one has room to
    // breathe. Kept from the original — it is layout, not look, so no style
    // says anything about it.
    const tall = el.height > 60;

    if (style.title.show) {
      const icon = style.icon.show ? `${style.icon.glyph} ` : "";
      g.appendChild(
        text(
          {
            ...textAttrs(style.title),
            ...alignX(style.title, rect),
            y: el.y + (tall ? 26 : 24),
            class: "spla-node-label",
          },
          `${icon}${el.label}`,
        ),
      );
    }

    if (style.subtitle.show) {
      const subtitle = typeof el.metadata.type === "string" ? el.metadata.type : el.type;
      g.appendChild(
        text(
          {
            ...textAttrs(style.subtitle),
            ...alignX(style.subtitle, rect),
            y: el.y + (tall ? 46 : 42),
            class: "spla-node-subtitle",
          },
          subtitle,
        ),
      );
    }

    // Link count badge in the top-right corner
    const relSummary = resolveElementRelations(ctx.doc, el);
    const visibleCount = relSummary.visible;
    const total = relSummary.total;

    if (total > 0) {
      const isGhost = ctx.ghostNodeId === el.id;
      const badgeText = `${total}/${visibleCount}`;
      const bw = Math.max(22, badgeText.length * 6 + 8);
      const bx = el.x + el.width - bw - 5;
      const by = el.y + 5;

      const badgeGroup = svg("g", {
        class: `spla-node-badge${isGhost ? " is-active" : ""}`,
        [ROLE_ATTR]: Role.GhostToggle,
        style: "cursor: pointer;",
      }, [
        svg("rect", {
          x: bx,
          y: by,
          width: bw,
          height: 14,
          rx: 7,
          fill: isGhost ? "var(--accent)" : "var(--panel-alt)",
          stroke: isGhost ? "var(--accent)" : "var(--line)",
          "stroke-width": 1,
        }),
        text(
          {
            x: bx + bw / 2,
            y: by + 10,
            "text-anchor": "middle",
            "font-size": "9px",
            "font-family": "monospace",
            "font-weight": "700",
            fill: isGhost ? "#ffffff" : "var(--muted)",
            "pointer-events": "none",
          },
          badgeText,
        ),
      ]);
      g.appendChild(badgeGroup);
    }

    // Resize grips are drawn by the canvas around the current selection, not
    // here: with several elements selected they belong to the selection's
    // bounding box rather than to any one element.
  }

  visibleRect(el: DiagramElement): Rect {
    return elementRect(el);
  }

  pointAt(rect: Rect, slot: BoundarySlot): Point {
    return pointOnSide(rect, slot.side, slot.t);
  }
}
