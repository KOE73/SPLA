import { ref, reactive } from "vue";
import { client } from "../protocol/SplaClient";
import type { FsNode, FsBrowseResultPayload } from "../protocol/types";

const ROOT_KEY = "__root__";

/**
 * Composable for the workspace file-tree protocol (fs.browse / fs.read / fs.write).
 * Manages lazy-loaded tree state; the shell component drives expansion via `browse()`.
 */
export function useFsBrowser() {
  const rootNodes = ref<FsNode[]>([]);
  // Map from parentRef → loaded children; reactive so computed props update on Map.set
  const childrenMap = reactive(new Map<string, FsNode[]>());
  // Set of keys that have been loaded (ROOT_KEY for root, ref for folders)
  const loadedSet = reactive(new Set<string>());
  const busy = ref(false);

  async function browse(parentRef?: string): Promise<void> {
    busy.value = true;
    try {
      const result = await client.invoke<FsBrowseResultPayload>(
        "fs.browse",
        { parentRef: parentRef ?? null }
      );
      const nodes = result?.nodes ?? [];
      if (!parentRef) {
        rootNodes.value = nodes;
        loadedSet.add(ROOT_KEY);
      } else {
        childrenMap.set(parentRef, nodes);
        loadedSet.add(parentRef);
      }
    } finally {
      busy.value = false;
    }
  }

  function getChildren(ref: string): FsNode[] {
    return childrenMap.get(ref) ?? [];
  }

  function isLoaded(ref?: string): boolean {
    return loadedSet.has(ref ?? ROOT_KEY);
  }

  return { rootNodes, childrenMap, loadedSet, busy, browse, getChildren, isLoaded };
}
