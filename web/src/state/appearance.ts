// Global appearance (theme/density) — applied to <html data-theme/data-density>, independent of
// any single surface. Three triggers, same two functions: boot (localStorage, avoids a flash of
// default theme before the socket connects), "welcome" (server-authoritative on connect), and
// "appearance.changed" (broadcast to every window whenever ANY window changes it — including a
// native Avalonia shell bridged in via window.chrome.webview).
import { reinitMermaidTheme } from "../composables/useMarkdown";
import { setLocale } from "../i18n";
import { client } from "../protocol/SplaClient";

declare global {
  interface Window {
    chrome?: { webview?: { postMessage(msg: unknown): void } };
  }
}

// "system" is not a palette in themes.css — it means "resolve to light/dark from the OS and keep
// tracking it". Kept as its own constant so every place that special-cases it says why.
export const SYSTEM_THEME = "system";

function resolveSystemTheme(): string {
  return window.matchMedia?.("(prefers-color-scheme: light)").matches ? "light" : "dark";
}

let systemQuery: MediaQueryList | null = null;

function watchSystemTheme(enabled: boolean) {
  if (!window.matchMedia) return;
  systemQuery ??= window.matchMedia("(prefers-color-scheme: light)");
  systemQuery.onchange = enabled
    ? () => { document.documentElement.setAttribute("data-theme", resolveSystemTheme()); reinitMermaidTheme(); }
    : null;
}

export function applyTheme(name: string) {
  // themes.css selectors are lowercase ([data-theme="cream"]); the server's stored value has been
  // observed capitalized ("Cream") for older .spla projects — normalize defensively so a stale
  // casing in project data never silently fails to apply.
  const normalized = name.toLowerCase();
  const resolved = normalized === SYSTEM_THEME ? resolveSystemTheme() : normalized;
  document.documentElement.setAttribute("data-theme", resolved);
  localStorage.setItem("spla.theme", normalized);
  watchSystemTheme(normalized === SYSTEM_THEME);
  reinitMermaidTheme();
}

export function applyDensity(name: string) {
  document.documentElement.setAttribute("data-density", name || "norm");
  localStorage.setItem("spla.density", name || "norm");
}

function applyAndForward(theme?: string, density?: string) {
  if (theme) applyTheme(theme);
  if (density) applyDensity(density);
  try { window.chrome?.webview?.postMessage({ kind: "appearance", theme, density }); } catch { /* not embedded */ }
}

/**
 * Persists the interface language and applies it here at once.
 *
 * It goes to the machine layer (~/.spla/defaults.yaml) rather than to the project, and rather than to
 * the browser: the client is served from an ephemeral loopback port, so every launch is a new origin
 * with an empty localStorage — which is why the language used to be English again each morning. There
 * is no broadcast back, unlike appearance: on a shared server one person's language must not land on
 * anybody else's screen. Other windows of this person pick it up from their own welcome.
 */
export function saveLanguage(id: string) {
  setLocale(id);
  client.send("language.save", { language: id });
}

export function bootAppearance() {
  applyTheme(localStorage.getItem("spla.theme") || "dark");
  applyDensity(localStorage.getItem("spla.density") || "norm");

  // The server's copy is the authoritative one — see saveLanguage. setLocale (not saveLanguage) so
  // learning the value does not immediately write it back.
  client.on("welcome", p => { if (p.language) setLocale(p.language); });
  client.on("welcome", p => applyAndForward(p.theme, p.density));
  client.on("appearance.changed", p => applyAndForward(p.theme, p.density));
}
