import { createApp, type App } from "vue";
import SettingsPanel from "./SettingsPanel.vue";

// Contract the host expects — kept in sync by convention with
// web/src/protocol/types.ts (PluginSettingsMount) in the main project, NOT by a shared import.
// Deliberately duplicated: this plugin must build and ship independently of the host's source tree.
export interface MountApi {
  /** Current opaque settings blob as JSON, or null when none. */
  getJson(): string | null;
  invoke<R = unknown>(type: string, payload?: unknown): Promise<R>;
}
export interface MountHandle {
  save(): string | null;
  destroy?(): void;
}

export function mount(el: HTMLElement, api: MountApi): MountHandle {
  let app: App | null = createApp(SettingsPanel, { api });
  const vm = app.mount(el) as unknown as { toJson: () => string };
  return {
    save: () => vm.toJson(),
    destroy: () => { app?.unmount(); app = null; }
  };
}
