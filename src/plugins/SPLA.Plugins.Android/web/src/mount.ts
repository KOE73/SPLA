import { createApp } from "vue";
import SettingsPanel from "./SettingsPanel.vue";
import { useHostTranslator } from "./i18n";

export interface MountApi {
  getJson(): string | null;
  invoke<R = unknown>(type: string, payload?: unknown): Promise<R>;
  t?(text: string, params?: Record<string, unknown>): string;
}
export function mount(el: HTMLElement, api: MountApi) {
  useHostTranslator(api.t?.bind(api));
  const app = createApp(SettingsPanel, { api });
  const vm = app.mount(el) as unknown as { toJson(): string };
  return { save: () => vm.toJson(), destroy: () => app.unmount() };
}
