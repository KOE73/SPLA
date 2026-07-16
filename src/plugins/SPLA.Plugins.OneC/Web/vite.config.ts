import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";

// Library build → one self-contained ES module (Vue bundled in, zero external runtime deps).
// The host (SPLA.Service) serves this file straight from the plugin's own directory and the web
// client dynamically imports it — see web/src/surfaces/Settings/PluginWebSettings.vue in the host.
export default defineConfig({
  define: { "process.env.NODE_ENV": JSON.stringify("production") },
  plugins: [vue()],
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
