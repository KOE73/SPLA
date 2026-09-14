import { createApp, type App } from "vue";
import { useHostTranslator } from "./i18n";
import GeometryPanel from "./GeometryPanel.vue";

// The contract the host expects of a panel bundle — kept in sync by convention with
// web/src/protocol/types.ts (PluginPanelMountApi) in the main project, NOT by a shared import.
// Deliberately duplicated: this plugin must build and ship independently of the host's source tree.
export interface PanelApi {
  /** Fire-and-forget wire send, e.g. send("plugin.panel.open", { panelId, panelType }). */
  send(type: string, payload?: unknown): boolean;
  /** Subscribe to a server message; returns the unsubscribe. */
  on(type: string, handler: (payload: never) => void): () => void;
  invoke<R = unknown>(type: string, payload?: unknown): Promise<R>;
  /** The host's translator (key = English source text). Optional: an older host sends none. */
  t?(text: string, params?: Record<string, unknown>): string;
  /** The chat the window shows, and a subscription to it changing. Followed unconditionally:
   * mounted means open, so there is no "is it visible" flag to guard the reload on. */
  currentChatId(): string | null;
  onChatChange(handler: (chatId: string | null) => void): () => void;
}
export interface PanelHandle {
  destroy?(): void;
}

export function mount(el: HTMLElement, api: PanelApi): PanelHandle {
  // Same window, same language: the panel speaks whatever the host is set to.
  useHostTranslator(api.t?.bind(api));
  let app: App | null = createApp(GeometryPanel, { api });
  app.mount(el);
  return { destroy: () => { app?.unmount(); app = null; } };
}
