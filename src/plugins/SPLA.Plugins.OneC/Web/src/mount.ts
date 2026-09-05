import { createApp, type App } from "vue";
import { useHostTranslator } from "./i18n";
import BrowserPanel from "./BrowserPanel.vue";
import browserStyles from "./browser.css?inline";

// Kept in sync by convention with web/src/protocol/types.ts. The plugin ships independently and
// deliberately does not import host sources.
export interface MountApi {
  /** Current opaque settings blob as JSON, or null when none. */
  getJson(): string | null;
  invoke<R = unknown>(type: string, payload?: unknown): Promise<R>;
  /** The host\'s translator (key = English source text). Optional: an older host sends none,
   * and every string then stays as written. */
  t?(text: string, params?: Record<string, unknown>): string;
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
  // Same window, same language: the panel speaks whatever the host is set to.
  useHostTranslator(api.t?.bind(api));
  ensureStyles();
  let app: App | null = createApp(BrowserPanel, { api });
  app.mount(element);
  return {
    // The browser owns no plugin settings; Save must preserve the opaque host blob unchanged.
    save: () => api.getJson(),
    destroy: () => {
      app?.unmount();
      app = null;
    }
  };
}
