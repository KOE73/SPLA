import { createApp, type App } from "vue";
import BrowserPanel from "./BrowserPanel.vue";
import browserStyles from "./browser.css?inline";

// Kept in sync by convention with web/src/protocol/types.ts. The plugin ships independently and
// deliberately does not import host sources.
export interface MountApi {
  getYaml(): string | null;
  invoke<R = unknown>(type: string, payload?: unknown): Promise<R>;
}

export interface MountHandle {
  save(): string | null;
  destroy?(): void;
}

const styleId = "spla-onec-web-styles";

function ensureStyles() {
  if (document.getElementById(styleId)) return;
  const style = document.createElement("style");
  style.id = styleId;
  style.textContent = browserStyles;
  document.head.appendChild(style);
}

export function mount(element: HTMLElement, api: MountApi): MountHandle {
  ensureStyles();
  let app: App | null = createApp(BrowserPanel, { api });
  app.mount(element);
  return {
    // The browser owns no plugin settings; Save must preserve the opaque host blob unchanged.
    save: () => api.getYaml(),
    destroy: () => {
      app?.unmount();
      app = null;
    }
  };
}
