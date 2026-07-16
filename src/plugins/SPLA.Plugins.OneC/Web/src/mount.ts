import { createApp, type App } from "vue";
import BrowserPanel from "./BrowserPanel.vue";

// Contract the host expects — kept in sync by convention with
// web/src/protocol/types.ts (PluginSettingsMount) in the main project, NOT by a shared import.
// Deliberately duplicated: this plugin must build and ship independently of the host's source tree.
export interface MountApi {
  getYaml(): string | null;
  invoke<R = unknown>(type: string, payload?: unknown): Promise<R>;
}
export interface MountHandle {
  save(): string | null;
  destroy?(): void;
}

export function mount(el: HTMLElement, api: MountApi): MountHandle {
  let app: App | null = createApp(BrowserPanel, { api });
  app.mount(el);
  return {
    // The OneC browser edits no settings blob — return the unchanged YAML so a Save is a no-op.
    save: () => api.getYaml(),
    destroy: () => { app?.unmount(); app = null; }
  };
}
