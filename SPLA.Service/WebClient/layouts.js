/*
  SPLA web client — layout definitions (the placement axis).

  Each layout is a skeleton with [data-slot] holes + a placement map {slot: surface}. The slot
  elements keep the same ids/classes the CSS already targets, so switching the *which surface goes
  where* never touches colours or sizing — those are separate axes (themes.css / size tokens).

  Add a layout here and it shows up in the layout picker automatically. A standalone window can
  reuse the same surfaces with a one-slot layout (see "single").
*/
(() => {
  // Default: the familiar three-pane shell — chat list | conversation | debug drawer.
  Spla.registerLayout("default", {
    label: "Default",
    skeleton: `
      <div id="app">
        <aside id="sidebar" data-slot="sidebar"></aside>
        <section id="main">
          <div id="status" data-slot="status"></div>
          <div id="filters" data-slot="filters"></div>
          <div id="log" data-slot="log"></div>
          <div id="composer" data-slot="composer"></div>
        </section>
        <aside id="debug" data-slot="debug"></aside>
      </div>`,
    placement: {
      sidebar: "chatList",
      status: "statusBar",
      filters: "filters",
      log: "chatLog",
      composer: "composer",
      debug: "debug"
    }
  });

  // Focus: no sidebar, no debug — just the conversation. Proves the placement axis is real.
  Spla.registerLayout("focus", {
    label: "Focus",
    skeleton: `
      <div id="app" class="focus">
        <section id="main">
          <div id="status" data-slot="status"></div>
          <div id="filters" data-slot="filters"></div>
          <div id="log" data-slot="log"></div>
          <div id="composer" data-slot="composer"></div>
        </section>
        <aside id="debug" data-slot="debug"></aside>
      </div>`,
    placement: {
      status: "statusBar",
      filters: "filters",
      log: "chatLog",
      composer: "composer",
      debug: "debug"
    }
  });

  // Single-surface host: one full-bleed slot. A standalone window (browser popup, Avalonia WebView
  // on /surface/<name>) picks the surface via ?surface=… — same code, different mount point.
  Spla.registerLayout("single", {
    label: "Single",
    skeleton: `<div id="app" class="single"><div id="solo" data-slot="solo"></div></div>`,
    placement: {} // filled dynamically from ?surface=… by the bootstrap
  });
})();
