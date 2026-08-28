import "../styles/editor.css";
import "../styles/canvas.css";

import { DiagramEditor } from "../editor/DiagramEditor.js";
import { HttpProjectStore, HttpStyleStore, loadCatalog } from "../editor/io/index.js";

/**
 * Application entry point.
 *
 * In dev the models directory is Vite's public dir, so models sit at the root.
 * The built app is served from `docs/diagrams/app/` by the Go server, with the
 * models one level up.
 */
const MODELS_BASE = import.meta.env.DEV ? "./" : "../";

async function main(): Promise<void> {
  const root = document.getElementById("app");
  if (root === null) throw new Error("Missing #app root");

  const catalog = await loadCatalog(MODELS_BASE);

  const editor = new DiagramEditor(root, {
    catalog,
    store: new HttpProjectStore(MODELS_BASE),
    // styles.json sits with the models, not with the app bundle.
    styleStore: new HttpStyleStore(MODELS_BASE),
  });

  // Handy while working on the library: swap port assignment or routing from
  // the console without a rebuild.
  Object.assign(window, { splaEditor: editor });
}

void main();
