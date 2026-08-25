import { defineConfig } from "vite";
import dts from "vite-plugin-dts";
import { fileURLToPath, URL } from "node:url";

/**
 * Two build targets share one source tree.
 *
 *   default  — library mode: `dist/spla-diagram.js` + types, consumed by anyone
 *              who wants only the canvas.
 *   --mode app — the editor application, emitted into `docs/diagrams/app/` so the
 *              existing Go server (`docs/diagrams/server.go`) keeps serving it
 *              unchanged. Models stay one level up, next to the server.
 *
 * In dev the models directory is mounted as the public dir, so `npm run dev`
 * serves the real models without Go. Only saving needs the Go server, and that
 * is what the /api proxy is for.
 */
const modelsDir = fileURLToPath(new URL("../../docs/diagrams", import.meta.url));

export default defineConfig(({ mode }) => {
  const isApp = mode === "app";

  return {
    // In app mode the page is served from /app/, so assets must resolve
    // relatively — the Go server has no base-path rewriting.
    base: isApp ? "./" : "/",

    publicDir: modelsDir,

    server: {
      port: 5177,
      proxy: {
        // Saving is still the Go server's job (server.go, POST /api/save).
        "/api": {
          target: "http://localhost:8777",
          changeOrigin: true,
        },
      },
    },

    build: isApp
      ? {
          outDir: fileURLToPath(new URL("../../docs/diagrams/app", import.meta.url)),
          // The models live in publicDir. Copying them into the build output
          // would duplicate them next to the app; the app reads them from ../.
          copyPublicDir: false,
          emptyOutDir: true,
          // The app build is committed so that run.cmd works without a Node
          // toolchain; a source map would add ~190 kB of churn per rebuild.
          sourcemap: false,
        }
      : {
          lib: {
            entry: fileURLToPath(new URL("src/index.ts", import.meta.url)),
            name: "SplaDiagram",
            formats: ["es"],
            fileName: () => "spla-diagram.js",
            cssFileName: "spla-diagram",
          },
          copyPublicDir: false,
          emptyOutDir: true,
          sourcemap: true,
        },

    plugins: isApp
      ? []
      : [
          dts({
            include: ["src"],
            rollupTypes: true,
          }),
        ],
  };
});
