import { createApp, type App } from "vue";
import { useHostTranslator } from "./i18n";
import ScreencastPanel from "./ScreencastPanel.vue";

// Contract the host expects — kept in sync by convention with
// web/src/protocol/types.ts (PluginPanelMountApi) in the main project, NOT by a shared import.
// Deliberately duplicated: this plugin must build and ship independently of the host's source tree.
export interface MountApi {
  /** Fire-and-forget wire send; false when there is no connection yet. */
  send(type: string, payload?: unknown): boolean;
  /** Subscribe to a server message; returns the unsubscribe. */
  on(type: string, handler: (payload: any) => void): () => void;
  invoke<R = unknown>(type: string, payload?: unknown): Promise<R>;
  /** The host's translator (key = English source text). */
  t?(text: string, params?: Record<string, unknown>): string;
  currentChatId?(): string | null;
  onChatChange?(handler: (chatId: string | null) => void): () => void;
}
export interface MountHandle {
  destroy?(): void;
}

export function mount(el: HTMLElement, api: MountApi): MountHandle {
  // Same window, same language: the panel speaks whatever the host is set to.
  useHostTranslator(api.t?.bind(api));
  let app: App | null = createApp(ScreencastPanel, { api });
  app.mount(el);
  return { destroy: () => { app?.unmount(); app = null; } };
}
