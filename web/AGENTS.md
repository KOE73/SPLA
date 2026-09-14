# web/ — the Vue client

Read the root `AGENTS.md` first. Wire messages and client bus events: [`agents/protocol.md`](../agents/protocol.md).

## Chat-scoped state (recurring bug — do not regress)

The composer input, Send/Stop button, and every other per-conversation UI state belong to the
**current chat**, not the window. In `web/src` any such state MUST live in `store.ts` keyed by
`chatId` (e.g. `store.turnActiveByChat`) and be read via a computed over `store.currentChat`.
Never hold it in a component-local `ref` — it leaks across chat switches (a running turn in chat A
locked input in chat B, twice). Server events must be applied by `env.chatId` from the envelope,
never to whatever chat happens to be open.

## Panels a plugin contributes (`web_panel_entry`)

A plugin adds its own dock tab by shipping a **prebuilt ES module**, not by being added to this
client. Nothing under `web/` names a plugin, and nothing here imports one.

How it fits together:

- `meta.yaml` declares `web_panel_entry`, `panel_title` and `panel_icon`
  ([`agents/plugins.md`](../agents/plugins.md)). Title and icon are read before the bundle is
  fetched, so the tool-strip button exists even if the fetch fails.
- `panelRegistry.ts` turns each such plugin into a catalog entry of kind `plugin:<id>`.
  `PanelKind` is a plain `string`, and `definitionFor()` throws naming every known kind — that
  runtime check is the whole compensation for losing the compile-time union
  (`ADR_20260914-2_web_plugin-panels` §3.2), so keep it.
- `PluginPanel.vue` is the one dock component every plugin panel mounts through. It imports the
  bundle's URL (carried in the panel's dockview params, so a restored layout works before
  `plugins.result` arrives) and calls `mount(el, api)` with `PluginPanelMountApi`
  (`protocol/types.ts`): `send` / `on` / `invoke`, the host translator `t`, and `currentChatId` /
  `onChatChange`.
- The bundle talks to its own plugin over `plugin.panel.open` / `.input` / `.close` and receives
  `plugin.panel.opened` / `.event`. The host routes those opaquely and knows no plugin's domain.
- Build it inside the plugin (`src/plugins/<Plugin>/web/`, entry `src/panel.ts`, Vite lib mode, CSS
  injected by JS) and **commit `web/dist/panel.js`**; the `.csproj` copies `web\dist\**` next to the
  plugin. `src/plugins/SPLA.Plugins.Geometry/web/` is the worked example.

Panels are chat-scoped the same way the rest of this client is: **in dockview, mounted means open.**
Load on mount and follow `onChatChange` unconditionally. Never guard a refresh on an "is the panel
visible/open" flag — the debug panel did exactly that, its title updated while its data did not, and
it silently showed the first chat's blobs forever.

Strings in a plugin bundle are English source text passed through the host translator, so their
Russian lives in `web/src/i18n/ru.json` with everything else.
