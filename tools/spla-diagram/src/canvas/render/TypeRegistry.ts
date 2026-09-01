import type { DiagramElement } from "../../model/types.js";
import type { ElementRenderer } from "./ElementRenderer.js";

/**
 * Maps an element's `type` to the renderer that draws it.
 *
 * A registry rather than a class hierarchy, deliberately: to add a type from
 * outside this library you register a renderer under a string key, without
 * subclassing anything or knowing how the built-in ones are put together.
 */
export class TypeRegistry {
  private readonly byType = new Map<string, ElementRenderer>();
  private readonly byKind = new Map<string, ElementRenderer>();

  /** Register a renderer for one specific `type` value. */
  register(type: string, renderer: ElementRenderer): this {
    this.byType.set(type, renderer);
    return this;
  }

  /**
   * Register the fallback used for any element of this kind whose `type` has
   * no renderer of its own. Every kind must have one.
   */
  registerDefault(kind: DiagramElement["kind"], renderer: ElementRenderer): this {
    this.byKind.set(kind, renderer);
    return this;
  }

  resolve(el: DiagramElement): ElementRenderer {
    const exact = this.byType.get(el.type);
    if (exact !== undefined) return exact;

    const fallback = this.byKind.get(el.kind);
    if (fallback !== undefined) return fallback;

    throw new Error(`No renderer registered for ${el.kind} "${el.type}" (element ${el.id})`);
  }

  has(type: string): boolean {
    return this.byType.has(type);
  }
}
