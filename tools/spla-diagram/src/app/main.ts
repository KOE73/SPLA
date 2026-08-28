import "../styles/editor.css";
import "../styles/canvas.css";

import { DiagramEditor, type CatalogEntry } from "../editor/DiagramEditor.js";
import { HttpModelStore, HttpStyleStore } from "../editor/io/transfer.js";

/**
 * Application entry point.
 *
 * In dev the models directory is Vite's public dir, so models sit at the root.
 * The built app is served from `docs/diagrams/app/` by the Go server, with the
 * models one level up.
 */
const MODELS_BASE = import.meta.env.DEV ? "./" : "../";

interface CatalogFile {
  schemas?: CatalogEntry[];
}

async function loadCatalog(): Promise<CatalogEntry[]> {
  try {
    const res = await fetch(new URL("catalog.json", new URL(MODELS_BASE, location.href)));
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const data = (await res.json()) as CatalogFile;
    return data.schemas ?? [];
  } catch (err) {
    // No embedded fallback copy: a missing catalog is reported, not replaced
    // with a stale duplicate baked into the bundle (D-07, D-11).
    console.warn("Каталог схем не загружен:", err);
    return [];
  }
}

async function main(): Promise<void> {
  const root = document.getElementById("app");
  if (root === null) throw new Error("Missing #app root");

  const editor = new DiagramEditor(root, {
    catalog: await loadCatalog(),
    store: new HttpModelStore(MODELS_BASE),
    // styles.json sits with the models, not with the app bundle.
    styleStore: new HttpStyleStore(MODELS_BASE),
  });

  // Handy while working on the library: swap port assignment or routing from
  // the console without a rebuild.
  Object.assign(window, { splaEditor: editor });
}

void main();
