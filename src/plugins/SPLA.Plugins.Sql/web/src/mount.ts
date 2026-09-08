import { createApp, type App } from "vue";
import { useHostTranslator } from "./i18n";
import SettingsPanel from "./SettingsPanel.vue";

// Contract the host expects — kept in sync by convention with
// web/src/protocol/types.ts (PluginSettingsMount) in the main project, NOT by a shared import.
// Deliberately duplicated: this plugin must build and ship independently of the host's source tree.
export interface MountApi {
  /** Current opaque settings blob as JSON, or null when none. */
  getJson(): string | null;
  invoke<R = unknown>(type: string, payload?: unknown): Promise<R>;
  /** Mounts the host's credential control (secret-store picker + editor) into our element and calls
   * back with a `secret:<scope>:<key>` reference. Optional: an older host will not provide it, and
   * CredentialSlot.vue falls back to a plain reference input. No secret ever crosses this boundary. */
  mountCredentialField?(el: HTMLElement, opts: CredentialFieldOptions): CredentialFieldHandle;
  /** The host\'s translator (key = English source text). Optional: an older host sends none,
   * and every string then stays as written. */
  t?(text: string, params?: Record<string, unknown>): string;
}
export interface CredentialFieldOptions {
  value?: string;
  onChange(reference: string): void;
  allowNone?: boolean;
  noneLabel?: string;
  createScope?: "user" | "project" | "shared" | "";
}
export interface CredentialFieldHandle {
  setValue(reference: string): void;
  destroy(): void;
}
export interface MountHandle {
  save(): string | null;
  destroy?(): void;
}

export function mount(el: HTMLElement, api: MountApi): MountHandle {
  // Same window, same language: the panel speaks whatever the host is set to.
  useHostTranslator(api.t?.bind(api));
  let app: App | null = createApp(SettingsPanel, { api });
  const vm = app.mount(el) as unknown as { toJson: () => string };
  return {
    save: () => vm.toJson(),
    destroy: () => { app?.unmount(); app = null; }
  };
}
