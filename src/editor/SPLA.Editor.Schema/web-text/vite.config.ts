import { defineConfig } from "vite";
import { viteSingleFile } from "vite-plugin-singlefile";

// Один self-contained dist/index.html со всем инлайном — вшивается в SPLA.Editor.Schema.dll.
// Те же требования, что и у schema-editor: ни одного loose-файла, без Node у пользователя.
export default defineConfig({
  plugins: [viteSingleFile()],
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
