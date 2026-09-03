#!/usr/bin/env node
/**
 * Self-check for the edge routers (`src/canvas/routing/routers.ts`, stream G
 * of PLAN_20260903). No test runner in this package — same harness as
 * `check-content-template.mjs`: bundle the module with esbuild, then run a
 * table of degenerate cases.
 *
 * What every case is really checking: no `NaN` ever reaches the `d`
 * attribute. A path with NaN in it does not fail loudly — the line simply
 * vanishes from the canvas, which reads as "the edge is gone", not "the
 * router is broken".
 */

import { build } from "esbuild";
import { mkdtempSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { pathToFileURL } from "node:url";

const ROOT = new URL("..", import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, "$1");

async function loadModule() {
  const result = await build({
    entryPoints: [join(ROOT, "src/canvas/routing/routers.ts")],
    bundle: true,
    write: false,
    format: "esm",
    platform: "neutral",
    target: "es2022",
  });
  const dir = mkdtempSync(join(tmpdir(), "spla-routers-"));
  const file = join(dir, "routers.mjs");
  writeFileSync(file, result.outputFiles[0].text, "utf8");
  return import(pathToFileURL(file).href);
}

const { OrthogonalRouter, TreeHorizontalRouter, TreeVerticalRouter } = await loadModule();

const routers = [new OrthogonalRouter(), new TreeHorizontalRouter(), new TreeVerticalRouter()];

const cases = [
  {
    name: "coincident points, same rect",
    from: { x: 50, y: 50 }, to: { x: 50, y: 50 },
    fromSide: "east", toSide: "west",
    fromRect: { x: 0, y: 0, width: 100, height: 100 },
    toRect: { x: 0, y: 0, width: 100, height: 100 },
  },
  {
    name: "overlapping rects, opposite sides",
    from: { x: 100, y: 50 }, to: { x: 0, y: 50 },
    fromSide: "east", toSide: "west",
    fromRect: { x: 0, y: 0, width: 100, height: 100 },
    toRect: { x: 20, y: 20, width: 100, height: 100 },
  },
  {
    name: "same side both ports",
    from: { x: 100, y: 50 }, to: { x: 100, y: 150 },
    fromSide: "east", toSide: "east",
    fromRect: { x: 0, y: 0, width: 100, height: 100 },
    toRect: { x: 0, y: 100, width: 100, height: 100 },
  },
  {
    name: "zero distance one axis (level ports)",
    from: { x: 100, y: 50 }, to: { x: 300, y: 50 },
    fromSide: "east", toSide: "west",
    fromRect: { x: 0, y: 0, width: 100, height: 100 },
    toRect: { x: 300, y: 0, width: 100, height: 100 },
  },
  {
    name: "vertical, same x (level)",
    from: { x: 50, y: 100 }, to: { x: 50, y: 300 },
    fromSide: "south", toSide: "north",
    fromRect: { x: 0, y: 0, width: 100, height: 100 },
    toRect: { x: 0, y: 300, width: 100, height: 100 },
  },
  {
    name: "with marker offsets and insets",
    from: { x: 100, y: 50 }, to: { x: 300, y: 250 },
    fromSide: "east", toSide: "west",
    fromRect: { x: 0, y: 0, width: 100, height: 100 },
    toRect: { x: 300, y: 200, width: 100, height: 100 },
    fromInset: 8, toInset: 8, fromMarkerOffset: 10, toMarkerOffset: 12,
  },
];

let failures = 0;
for (const router of routers) {
  for (const c of cases) {
    const route = router.route(c);
    const nums = [route.path.match(/-?\d+(\.\d+)?/g) ?? [], [route.labelAt.x, route.labelAt.y]].flat();
    const hasNaN = route.path.includes("NaN") || !Number.isFinite(route.labelAt.x) || !Number.isFinite(route.labelAt.y);
    const status = hasNaN ? "FAIL" : "ok";
    if (hasNaN) failures++;
    console.log(`[${status}] ${router.id} :: ${c.name} -> ${route.path}`);
  }
}
console.log(failures === 0 ? "\nAll cases clean, no NaN." : `\n${failures} FAILURES`);
process.exit(failures === 0 ? 0 : 1);
