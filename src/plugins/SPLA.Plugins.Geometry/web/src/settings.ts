import { createApp } from "vue";
import SettingsPanel from "./SettingsPanel.vue";
import { useHostTranslator } from "./i18n";

// The contract the host expects of a settings bundle — kept in sync by convention with
// PluginSettingsMount in web/src/protocol/types.ts of the main project, NOT by a shared import, for
// the same reason panel.ts duplicates its own: this plugin builds and ships independently.
export interface MountApi {
  /** The plugin's stored settings blob, as JSON, or null when it has never been saved. */
  getJson(): string | null;
  invoke<R = unknown>(type: string, payload?: unknown): Promise<R>;
  /** The host's translator (key = English source text). Optional: an older host sends none. */
  t?(text: string, params?: Record<string, unknown>): string;
}

export function mount(el: HTMLElement, api: MountApi) {
  useHostTranslator(api.t?.bind(api));
  const app = createApp(SettingsPanel, { api });
  const vm = app.mount(el) as unknown as { toJson(): string };
  return { save: () => vm.toJson(), destroy: () => app.unmount() };
}
