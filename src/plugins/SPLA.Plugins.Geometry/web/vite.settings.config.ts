import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import cssInjectedByJsPlugin from "vite-plugin-css-injected-by-js";

// The settings bundle, built separately from the panel rather than as a second entry of one build:
// a multi-entry lib build hoists the shared half of Vue into a chunk file, and the host loads each
// bundle as one self-contained module with no loader of its own. Two builds, two single files.
export default defineConfig({
  define: { "process.env.NODE_ENV": JSON.stringify("production") },
  plugins: [vue(), cssInjectedByJsPlugin()],
  build: {
    outDir: "dist",
    // The panel is built first and lives in the same folder: emptying it here would delete it.
    emptyOutDir: false,
    cssCodeSplit: false,
    lib: {
      entry: "src/settings.ts",
      formats: ["es"],
      fileName: () => "settings.js"
    }
  }
});
