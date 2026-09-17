import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import cssInjectedByJsPlugin from "vite-plugin-css-injected-by-js";

// Library build → one self-contained ES module (Vue bundled in, zero external runtime deps), the
// same shape the SSH plugin's settings bundle has. The host serves this file from the plugin's own
// directory (web_panel_entry in meta.yaml) and the web client imports it dynamically — see
// web/src/dock/PluginPanel.vue in the main project.
export default defineConfig({
  // Lib mode doesn't statically replace process.env.NODE_ENV inside bundled Vue — in a browser
  // there is no `process`, so the module throws on import without this define.
  define: { "process.env.NODE_ENV": JSON.stringify("production") },
  // cssInjectedByJs: lib mode extracts scoped CSS into a file nobody loads — inject it from the
  // module itself instead, keeping the single-file "panel.js is everything" contract.
  plugins: [vue(), cssInjectedByJsPlugin()],
  build: {
    outDir: "dist",
    emptyOutDir: true,
    cssCodeSplit: false,
    lib: {
      entry: "src/panel.ts",
      formats: ["es"],
      fileName: () => "panel.js"
    }
  }
});
