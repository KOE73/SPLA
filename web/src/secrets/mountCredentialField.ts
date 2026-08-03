/**
 * Hands the credential control to code that cannot import it — plugin settings modules, which are
 * built and shipped separately with their own bundled Vue. The plugin supplies an element and a
 * callback and gets back a reference string; it never learns the `secret.*` protocol, never sees a
 * value, and cannot get the project envelope or a refused write wrong, because none of that code
 * lives on its side any more.
 *
 * Mounted with the HOST's Vue into the plugin's DOM node, so it shares this app's secret state and
 * CSS variables — one list, refreshed for everyone when anything writes an entry.
 */
import { createApp, h, reactive, type App } from "vue";
import CredentialField from "./CredentialField.vue";
import type { SecretScopeId } from "../protocol/types";

export interface CredentialFieldOptions {
  /** Current `secret:<scope>:<key>` reference, or empty. */
  value?: string;
  /** Called with the new reference whenever the user picks or creates one. */
  onChange(reference: string): void;
  /** Offer "(none)" — for consumers that can also work without a credential. Default true. */
  allowNone?: boolean;
  noneLabel?: string;
  createScope?: SecretScopeId | "";
}

export interface CredentialFieldHandle {
  /** Push a new reference in (e.g. the host reloaded the config under the panel). */
  setValue(reference: string): void;
  destroy(): void;
}

export function mountCredentialField(el: HTMLElement, opts: CredentialFieldOptions): CredentialFieldHandle {
  const state = reactive({ value: opts.value ?? "" });
  let app: App | null = createApp({
    render: () => h(CredentialField, {
      modelValue: state.value,
      allowNone: opts.allowNone !== false,
      noneLabel: opts.noneLabel,
      createScope: opts.createScope ?? "",
      "onUpdate:modelValue": (reference: string) => {
        state.value = reference;
        opts.onChange(reference);
      }
    })
  });
  app.mount(el);

  return {
    setValue: (reference: string) => { state.value = reference; },
    destroy: () => { app?.unmount(); app = null; }
  };
}
