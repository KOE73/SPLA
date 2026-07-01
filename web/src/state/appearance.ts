// Global appearance (theme/density) — applied to <html data-theme/data-density>, independent of
// any single surface. Three triggers, same two functions: boot (localStorage, avoids a flash of
// default theme before the socket connects), "welcome" (server-authoritative on connect), and
// "appearance.changed" (broadcast to every window whenever ANY window changes it — including a
// native Avalonia shell bridged in via window.chrome.webview).
import { reinitMermaidTheme } from "../composables/useMarkdown";
import { client } from "../protocol/SplaClient";

declare global {
  interface Window {
    chrome?: { webview?: { postMessage(msg: unknown): void } };
  }
}

export function applyTheme(name: string) {
  // themes.css selectors are lowercase ([data-theme="cream"]); the server's stored value has been
  // observed capitalized ("Cream") for older .spla projects — normalize defensively so a stale
  // casing in project data never silently fails to apply.
  const normalized = name.toLowerCase();
  document.documentElement.setAttribute("data-theme", normalized);
  localStorage.setItem("spla.theme", normalized);
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

export function bootAppearance() {
  applyTheme(localStorage.getItem("spla.theme") || "dark");
  applyDensity(localStorage.getItem("spla.density") || "norm");

  client.on("welcome", p => applyAndForward(p.theme, p.density));
  client.on("appearance.changed", p => applyAndForward(p.theme, p.density));
}
