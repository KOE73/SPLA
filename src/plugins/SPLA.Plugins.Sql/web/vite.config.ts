import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";

// Library build → one self-contained ES module (Vue + yaml bundled in, zero external runtime deps).
// The host (SPLA.Service) serves this file straight from the plugin's own directory and the web
// client dynamically imports it — see docs in web/src/surfaces/Settings/PluginWebSettings.vue.
export default defineConfig({
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
