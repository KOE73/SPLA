// Localization with the English source text as the key.
//
// Deliberately not vue-i18n and deliberately not invented keys ("settings.mcp.title"). Two properties
// pay for that choice:
//   * a missing translation renders the English original, never a raw key — so the dictionary can be
//     filled panel by panel without the UI ever showing debris;
//   * a string that arrives from the server or from a plugin bundle translates for free, because the
//     string itself is the key. Nothing on the backend has to learn about locales.
//
// The locale is a per-device preference (like the dock layout), not project data: it lives in
// localStorage and never travels to .spla. t() reads the reactive ref, so switching the language
// re-renders every template that called it — no reload.
import { ref } from "vue";
import ru from "./ru.json";

export const LOCALES = [
  { id: "en", label: "English" },
  { id: "ru", label: "Русский" },
] as const;

const dictionaries: Record<string, Record<string, string>> = { ru };

export const locale = ref(localStorage.getItem("spla.lang") || "en");

export function setLocale(id: string) {
  locale.value = id;
  localStorage.setItem("spla.lang", id);
  document.documentElement.setAttribute("lang", id);
  tellTheShell(id);
}

/**
 * The Avalonia shell draws the window frame around this page and has a few strings of its own
 * (the title-bar tooltips). It has no language setting — this is the only place one is chosen — so
 * it is told, on the same bridge the theme uses. A plain browser tab has no shell and no-ops here.
 */
function tellTheShell(id: string) {
  try { window.chrome?.webview?.postMessage({ kind: "lang", lang: id }); } catch { /* not embedded */ }
}

/**
 * Translates `text`, falling back to `text` itself. Placeholders are {named} and substituted after
 * lookup, so the dictionary entry keeps them and translators can reorder them freely.
 */
export function t(text: string, params?: Record<string, unknown>): string {
  const out = dictionaries[locale.value]?.[text] ?? text;
  return params ? out.replace(/\{(\w+)\}/g, (m, k) => (k in params ? String(params[k]) : m)) : out;
}

export function bootLocale() {
  document.documentElement.setAttribute("lang", locale.value);
  tellTheShell(locale.value);
}
