import { pointOnSide } from "../../geometry/rect.js";
import type { BoundarySlot, Point, Rect, Side } from "../../geometry/types.js";
import type { DiagramElement } from "../../model/types.js";
import type { ResolvedBlockStyle } from "../../model/StyleLibrary.js";
import { elementRect } from "../../model/types.js";
import { ELEMENT_ATTR, ROLE_ATTR, Role } from "../../interaction/roles.js";
import { setAttrs, svg, text } from "../svg.js";
import type { ElementRenderer, RenderContext } from "./ElementRenderer.js";
import { alignX, dashArray, textAttrs } from "./textAttrs.js";
import { resolveElementRelations } from "../../model/relations-resolver.js";
import { SourceCodeService } from "../../editor/code/SourceCodeService.js";
import { DIAGRAM_CONFIG } from "../../constants/diagram-constants.js";

/**
 * The default leaf renderer: a rounded rectangle with a caption, a subtitle and
 * nothing else. Covers every node type in the current models — they differ only
 * in the style they resolve to, which this renderer reads and does not choose.
 */
const NODE_LAYOUT = DIAGRAM_CONFIG.node;

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
      filter: style.shadow ? "url(#spla-shadow)" : null,
    });
    g.replaceChildren();

    // 1. Background / Drag handle rect
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

    // 2. Top Bar Zone: Controls (Doc button on left, Code button, Relations count on right)
    const textEntry = ctx.doc ? ctx.doc.getText(el.id, "ru") || ctx.doc.getText(el.id, "en") : undefined;
    const hasDoc = Boolean(textEntry?.doc?.trim());
    const docX = el.x + 6;
    const docY = el.y + NODE_LAYOUT.topBarY;

    const docGroup = svg("g", {
      class: `spla-node-doc${hasDoc ? " has-doc" : ""}`,
      [ROLE_ATTR]: Role.DocEdit,
      style: "cursor: pointer;",
    }, [
      svg("rect", {
        x: docX,
        y: docY,
        width: NODE_LAYOUT.docButtonWidth,
        height: NODE_LAYOUT.topBarHeight,
        rx: 4,
        class: "spla-node-doc-rect",
      }),
      text(
        {
          x: docX + 9,
          y: docY + 10,
          "text-anchor": "middle",
          "font-size": "9px",
          class: "spla-node-doc-icon",
          "pointer-events": "none",
        },
        hasDoc ? "📝" : "📄",
      ),
    ]);
    g.appendChild(docGroup);

    // 2b. Code button if codeRef is present and available
    const codeRef = typeof el.metadata?.codeRef === "string" ? el.metadata.codeRef.trim() : "";
    const isAvailable = codeRef ? SourceCodeService.isFileAvailable(codeRef) : false;
    if (codeRef && isAvailable !== false) {
      const codeX = docX + NODE_LAYOUT.docButtonWidth + 4;
      const codeGroup = svg("g", {
        class: "spla-node-code",
        [ROLE_ATTR]: Role.CodeView,
        style: "cursor: pointer;",
      }, [
        svg("rect", {
          x: codeX,
          y: docY,
          width: NODE_LAYOUT.codeButtonWidth,
          height: NODE_LAYOUT.topBarHeight,
          rx: 4,
          class: "spla-node-code-rect",
        }),
        text(
          {
            x: codeX + 9,
            y: docY + 10,
            "text-anchor": "middle",
            "font-size": "9px",
            class: "spla-node-code-icon",
            "pointer-events": "none",
          },
          "💻",
        ),
      ]);
      g.appendChild(codeGroup);
    }

    // Link count badge in the top-right corner
    const relSummary = resolveElementRelations(ctx.doc, el);
    const visibleCount = relSummary.visible;
    const total = relSummary.total;

    if (total > 0) {
      const isGhost = ctx.ghostNodeId === el.id;
      const badgeText = `${total}/${visibleCount}`;
      const bw = Math.max(22, badgeText.length * 6 + 8);
      const bx = el.x + el.width - bw - 6;
      const by = el.y + NODE_LAYOUT.topBarY;

      const badgeGroup = svg("g", {
        class: `spla-node-badge${isGhost ? " is-active" : ""}`,
        [ROLE_ATTR]: Role.GhostToggle,
        style: "cursor: pointer;",
      }, [
        svg("rect", {
          x: bx,
          y: by,
          width: bw,
          height: NODE_LAYOUT.topBarHeight,
          rx: 7,
          class: "spla-node-badge-rect",
        }),
        text(
          {
            x: bx + bw / 2,
            y: by + 10,
            "text-anchor": "middle",
            "font-size": "9px",
            "font-family": "monospace",
            "font-weight": "700",
            class: "spla-node-badge-text",
            "pointer-events": "none",
          },
          badgeText,
        ),
      ]);
      g.appendChild(badgeGroup);
    }

    // 3. Title Zone (strictly below top bar, without icon prefix)
    const tall = el.height > 60;

    if (style.title.show) {
      g.appendChild(
        text(
          {
            ...textAttrs(style.title),
            ...alignX(style.title, rect, NODE_LAYOUT.padX),
            y: el.y + NODE_LAYOUT.titleY(tall),
            class: "spla-node-label",
          },
          el.label,
        ),
      );
    }

    // 4. Subtitle Zone (strictly below title)
    if (style.subtitle.show) {
      const subtitle = typeof el.metadata.type === "string" ? el.metadata.type : el.type;
      g.appendChild(
        text(
          {
            ...textAttrs(style.subtitle),
            ...alignX(style.subtitle, rect, NODE_LAYOUT.padX),
            y: el.y + NODE_LAYOUT.subtitleY(tall),
            class: "spla-node-subtitle",
          },
          subtitle,
        ),
      );
    }
  }

  visibleRect(el: DiagramElement): Rect {
    return elementRect(el);
  }

  pointAt(rect: Rect, slot: BoundarySlot): Point {
    return pointOnSide(rect, slot.side, slot.t);
  }

  cornerInset(_side: Side, style?: ResolvedBlockStyle): number {
    return (style?.radius ?? DIAGRAM_CONFIG.node.defaultRadius) + DIAGRAM_CONFIG.ports.extraCornerGap;
  }
}
