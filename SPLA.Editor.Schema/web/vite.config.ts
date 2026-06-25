import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { viteSingleFile } from "vite-plugin-singlefile";

// Produces ONE self-contained dist/index.html with all JS/CSS inlined.
// That file is embedded as a resource into SPLA.Editor.Schema.dll — no loose
// .js/.css/wwwroot ship with the app, and no Node/npm on the user's machine.
export default defineConfig({
  plugins: [react(), viteSingleFile()],
  build: {
    target: "es2020",
    cssCodeSplit: false,
    assetsInlineLimit: 100_000_000,
    chunkSizeWarningLimit: 100_000_000,
    rollupOptions: {
      output: { inlineDynamicImports: true },
    },
  },
});
