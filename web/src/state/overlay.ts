import { ref } from "vue";

/**
 * The one surface currently shown as a full-screen layer over the app (today: settings).
 *
 * Settings used to be its own browser window (window.open("/?surface=settings")), which made an
 * internal address — 127.0.0.1:<port>/?surface=settings — visible in a real address bar and made
 * the settings look like a separate program. They are a surface of THIS app, so they mount here:
 * one connection, one store, one event bus, no second document. Full screen rather than a dialog
 * because our settings are list-shaped (connections, secrets, roles, plugins) and a list whose item
 * must be opened, edited and returned from does not fit a dialog without stacking a layer on a
 * layer. See ADR_20260904-3_web_settings-placement — including why this is NOT a user preference.
 */
export const overlaySurface = ref<string | null>(null);

export function openOverlay(name: string) { overlaySurface.value = name; }
export function closeOverlay() { overlaySurface.value = null; }
