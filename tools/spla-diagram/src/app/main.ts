import "../styles/dockview.css";
import "../styles/editor.css";
import "../styles/canvas.css";
import "../styles/ribbon.css";

import { Workbench } from "../workbench/Workbench.js";
import { HttpProjectStore, HttpStyleStore, loadCatalog } from "../editor/io/index.js";

/**
 * Application entry point with Workbench Architecture.
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

  const workbench = new Workbench(root, {
    catalog,
    store: new HttpProjectStore(MODELS_BASE),
    // styles.json sits with the models, not with the app bundle.
    styleStore: new HttpStyleStore(MODELS_BASE),
  });

  // Handy for console debugging and testing
  Object.assign(window, {
    splaWorkbench: workbench,
    splaEditor: workbench.editor,
    splaCommands: workbench.commands,
  });
}

void main();
