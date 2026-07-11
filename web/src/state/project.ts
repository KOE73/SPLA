// Project focus — mirrors state/appearance.ts's pattern: apply locally, then forward to the native
// Avalonia shell (if embedded) via window.chrome.webview.postMessage, so its OS window Title follows
// the same focus change the web sidebar shows. Without this, switching projects via the in-page
// ProjectPicker left the native title bar/taskbar thumbnail stuck on whatever project the process
// started with — confusing, since the native picker (spawns a new window) DOES update its title.
import { store } from "./store";

/** Best-effort display name: the server's projectName when set, else the manifest filename (mirrors
 * LocalProjectProvider.TryReadName's server-side fallback), else the generic "SPLA" placeholder. */
export function projectLabel(projectId: string | null, projectName?: string | null): string {
  if (projectName) return projectName;
  if (!projectId) return "SPLA";
  const base = projectId.split(/[\\/]/).pop() || projectId;
  return base.replace(/\.spla$/i, "") || "SPLA";
}

/** Envelope options that scope a request to the currently focused project. Settings surfaces
 * (agent/plugins/connections/usage/appearance) MUST pass this: without a projectId the server's
 * Resolve() falls back to the connection's default project, so edits to a project opened via the
 * ProjectPicker would silently save into the wrong .spla. */
export function projectEnvelope(): { projectId?: string } {
  return { projectId: store.currentProjectId ?? undefined };
}

export function setCurrentProject(projectId: string | null, projectName?: string | null) {
  store.currentProjectId = projectId;
  store.currentProjectName = projectLabel(projectId, projectName);
  try { window.chrome?.webview?.postMessage({ kind: "project", projectName: store.currentProjectName }); }
  catch { /* not embedded — plain browser tab */ }
}
