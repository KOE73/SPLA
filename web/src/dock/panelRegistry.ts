import { reactive } from "vue";
import type { VueComponent } from "dockview-vue";
import type { PluginDto } from "../protocol/types";

/**
 * What panels there are, and what each one is called. No component is imported here — the Vue
 * surfaces are attached in panelCatalog.ts — so this half is testable on its own and, more to the
 * point, a plugin panel needs nothing from the shell but these fields.
 *
 * A kind is a plain string, not a closed union: a plugin contributes a panel by shipping a bundle
 * (web_panel_entry) and the client must not have to be edited for it. The cost is that a typo is no
 * longer caught by the compiler — paid back by definitionFor(), which fails loudly and names the
 * kinds it does know instead of opening a blank panel (ADR_20260914-2 §3.2).
 */
export type PanelKind = string;

/** Kinds built into the shell. Plugin kinds are "plugin:<id>" and are added at runtime. */
export const builtInKinds = ["chat", "workspace", "ssh", "browserScreencast", "debug", "wire", "sessions"] as const;

// Which panels are tools (everything the top strip can open/hide). Chat is the always-present base.
// Reactive: registerPluginPanels appends to it when plugins.result arrives.
export const toolKinds = reactive<PanelKind[]>(
  ["workspace", "ssh", "browserScreencast", "debug", "wire", "sessions"]);

export interface PanelDefinition {
  id: string;
  kind: PanelKind;
  title: string;
  /** Emoji, used on the tab. */
  icon: string;
  /** Name of a shell icon (assets/icons/<name>.svg) for the tool strip. Plugin panels have none and
   * the strip draws their emoji instead — a plugin cannot add files to the shell's icon set. */
  iconName?: string;
  /** The Vue surface, attached by panelCatalog.ts. Plugin panels have none of their own: they all
   * share the generic host component registered under `componentName`. */
  component?: VueComponent;
  /** dockview component name. Built-ins use one component per kind; every plugin panel uses the
   * generic "pluginPanel" host, which is what lets a restored layout mount one with no catalog yet. */
  componentName?: string;
  /** Extra dockview params. Plugin panels carry their bundle URL here so it survives serialisation. */
  params?: Record<string, unknown>;
  singleton: boolean;
  protected: boolean;
  defaultWidth?: number;
  minimumWidth?: number;
  maximumWidth?: number;
}

export const panelCatalog = reactive<Record<PanelKind, PanelDefinition>>({
  chat: { id: "chat", kind: "chat", title: "Chat", icon: "💬", iconName: "workspace", singleton: true, protected: true, minimumWidth: 320 },
  workspace: { id: "workspace", kind: "workspace", title: "Workspace", icon: "◫", iconName: "workspace", singleton: true, protected: false },
  // NOT a singleton: each SSH terminal is its own panel (id "ssh:<host>:<n>", host in params) —
  // the operator routinely holds several sessions to the same or different hosts.
  ssh: { id: "ssh", kind: "ssh", title: "SSH", icon: "⌨", iconName: "ssh", singleton: false, protected: false, defaultWidth: 480 },
  browserScreencast: { id: "browserScreencast", kind: "browserScreencast", title: "Browser Lab", icon: "🌐", iconName: "browser", singleton: true, protected: false, defaultWidth: 640 },
  debug: { id: "debug", kind: "debug", title: "Debug", icon: "🧠", iconName: "debug", singleton: true, protected: false, defaultWidth: 420 },
  wire: { id: "wire", kind: "wire", title: "Wire", icon: "🔌", iconName: "wire", singleton: true, protected: false, defaultWidth: 420 },
  sessions: { id: "sessions", kind: "sessions", title: "Sessions", icon: "🗂", iconName: "sessions", singleton: true, protected: false, defaultWidth: 320 },
}) as Record<PanelKind, PanelDefinition>;

/**
 * The definition to open, or a thrown error naming every kind that IS known. This is the whole
 * compensation for PanelKind no longer being a union (ADR §3.2): with a string kind a typo, or a
 * plugin that never registered, would otherwise open an empty panel and say nothing at all.
 */
export function definitionFor(kind: PanelKind): PanelDefinition {
  const definition = panelCatalog[kind];
  if (definition) return definition;
  throw new Error(
    `Unknown panel kind "${kind}". Known kinds: ${Object.keys(panelCatalog).sort().join(", ")}.`);
}

export const pluginPanelKind = (pluginId: string) => `plugin:${pluginId}`;

/** dockview component name every plugin panel is mounted through (see PluginPanel.vue). */
export const pluginPanelComponent = "pluginPanel";

/**
 * Replaces the plugin-contributed half of the catalog from a plugins.result. Title and icon come
 * from the manifest, never from the bundle: the strip has to draw the button before the bundle is
 * fetched, and keep drawing it if the fetch fails.
 */
export function registerPluginPanels(plugins: PluginDto[]): void {
  // A plugin that failed to load cannot answer its panel's transport either, so it contributes no
  // button — a dead tab would be worse than no tab.
  const wanted = plugins.filter(p =>
    p.enabled !== false && !!p.webPanelUrl && (!p.state || p.state === "Enabled"));
  const live = new Set(wanted.map(p => pluginPanelKind(p.id)));

  for (const kind of Object.keys(panelCatalog))
    if (kind.startsWith("plugin:") && !live.has(kind)) delete panelCatalog[kind];
  for (let i = toolKinds.length - 1; i >= 0; i--)
    if (toolKinds[i].startsWith("plugin:") && !live.has(toolKinds[i])) toolKinds.splice(i, 1);

  for (const plugin of wanted) {
    const kind = pluginPanelKind(plugin.id);
    panelCatalog[kind] = {
      id: kind,
      kind,
      title: plugin.panelTitle || plugin.name || plugin.id,
      icon: plugin.panelIcon || "🧩",
      componentName: pluginPanelComponent,
      params: { panelUrl: plugin.webPanelUrl, pluginId: plugin.id },
      singleton: true,
      protected: false,
      defaultWidth: 480,
    };
    if (!toolKinds.includes(kind)) toolKinds.push(kind);
  }
}
