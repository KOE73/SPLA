import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import cssInjectedByJsPlugin from "vite-plugin-css-injected-by-js";

// Library build → one self-contained ES module (Vue + yaml bundled in, zero external runtime deps).
// The host (SPLA.Service) serves this file straight from the plugin's own directory and the web
// client dynamically imports it — see docs in web/src/surfaces/Settings/PluginWebSettings.vue.
export default defineConfig({
  // Lib mode doesn't statically replace process.env.NODE_ENV inside bundled Vue — in a browser
  // there is no `process`, so the module throws on import without this define.
  define: { "process.env.NODE_ENV": JSON.stringify("production") },
  // cssInjectedByJs: lib mode extracts scoped CSS into a file nobody loads — inject it from the
  // module itself instead, keeping the single-file "settings.js is everything" contract.
  plugins: [vue(), cssInjectedByJsPlugin()],
  build: {
    outDir: "dist",
    emptyOutDir: true,
    cssCodeSplit: false,
    lib: {
      entry: "src/mount.ts",
      formats: ["es"],
      fileName: () => "settings.js"
    }
  }
});
