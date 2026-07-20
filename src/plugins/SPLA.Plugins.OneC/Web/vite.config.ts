import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";

// One self-contained ES module: Vue, Cytoscape, and CSS are all bundled into settings.js.
// The host loads it through the generic web_settings_entry contract and knows no OneC types.
export default defineConfig({
  define: { "process.env.NODE_ENV": JSON.stringify("production") },
  plugins: [vue()],
  build: {
    outDir: "dist",
    emptyOutDir: true,
    lib: {
      entry: "src/mount.ts",
      formats: ["es"],
      fileName: () => "settings.js"
    }
  }
});
