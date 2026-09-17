/**
 * PanelKind is a plain string so a plugin can contribute a panel without the client being edited
 * (ADR_20260914-2 §3.2). The compiler no longer catches a typo, so the catalog has to: an unknown
 * kind must fail by name and list what it does know, never open a blank panel. That trade is what
 * these tests hold in place, along with registration being a full replace — a plugin switched off
 * must lose its button, not keep a dead one.
 */
import { describe, expect, it } from "vitest";
import { definitionFor, panelCatalog, pluginPanelKind, registerPluginPanels, toolKinds } from "./panelRegistry";

const probe = { id: "probe", enabled: true, webPanelUrl: "/plugin-assets/probe/web/panel.js", panelTitle: "Probe", panelIcon: "🧪" };

describe("definitionFor", () => {
  it("names the kinds it knows instead of opening nothing", () => {
    expect(() => definitionFor("dbeug")).toThrowError(/Unknown panel kind "dbeug"/);
    expect(() => definitionFor("dbeug")).toThrowError(/debug/);
  });

  it("returns the built-in definitions unchanged", () => {
    expect(definitionFor("debug")).toBe(panelCatalog["debug"]);
  });
});

describe("registerPluginPanels", () => {
  it("adds a tool button and a catalog entry from the MANIFEST fields", () => {
    registerPluginPanels([probe]);

    const kind = pluginPanelKind("probe");
    expect(toolKinds).toContain(kind);
    // Title and icon come from the manifest, not the bundle: the strip draws the button before the
    // bundle is fetched and must keep drawing it if the fetch fails.
    expect(definitionFor(kind).title).toBe("Probe");
    expect(definitionFor(kind).icon).toBe("🧪");
    expect(definitionFor(kind).componentName).toBe("pluginPanel");
    expect(definitionFor(kind).params).toEqual({ panelUrl: probe.webPanelUrl, pluginId: "probe" });
  });

  it("drops the panel of a plugin that was switched off", () => {
    registerPluginPanels([probe]);
    registerPluginPanels([{ ...probe, enabled: false }]);

    expect(toolKinds).not.toContain(pluginPanelKind("probe"));
    expect(() => definitionFor(pluginPanelKind("probe"))).toThrowError(/Unknown panel kind/);
  });

  it("leaves the built-in panels alone", () => {
    registerPluginPanels([probe]);
    expect(toolKinds.filter(k => !k.startsWith("plugin:")))
      .toEqual(["workspace", "ssh", "debug", "wire", "sessions"]);
  });
});
