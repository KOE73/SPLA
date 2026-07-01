/** A leaf slot (mounts a surface) or a group (a plain wrapper div with nested slots/groups). */
export type LayoutNode =
  | { id: string; surface: string; children?: undefined }
  | { id: string; surface?: undefined; children: LayoutNode[] };

export interface LayoutDef {
  /** Tree rendered inside #app, declaratively (no innerHTML) — ids match the existing
   *  app.css selectors (#sidebar, #status, #main, ...). */
  root: LayoutNode[];
}

export const layouts: Record<string, LayoutDef> = {
  // The familiar three-pane shell — chat list | conversation | debug drawer. Wire is intentionally
  // NOT a slot here — it's solo-only (?surface=wire, a tear-off window).
  // Deliberately reordered from the old WebClient/layouts.js "default": status+filters sit BELOW
  // the composer (separated by composer's existing border-top, same divider style as log↔composer)
  // instead of above the log — a requested layout change, not a port of the original.
  default: {
    root: [
      { id: "sidebar", surface: "chatList" },
      {
        id: "main",
        children: [
          { id: "log", surface: "chatLog" },
          { id: "composer", surface: "composer" },
          { id: "status", surface: "statusBar" },
          { id: "filters", surface: "filters" }
        ]
      },
      { id: "debug", surface: "debug" }
    ]
  },
  // Workspace-focused layout: file tree + editor instead of the chat column.
  workspace: {
    root: [
      { id: "sidebar", surface: "chatList" },
      { id: "main", surface: "workspace" }
    ]
  }
};

export function layoutNames(): string[] {
  return Object.keys(layouts);
}
