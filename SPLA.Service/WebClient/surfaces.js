/*
  SPLA web client — surface framework.

  A *surface* is a host-agnostic component: a factory `(ctx) → { el, dispose? }` that builds one
  piece of UI and never reaches into globals (no ws, no currentChat). It talks to the rest of the
  app only through its context:
    ctx.send(type, payload, extra)   — send a protocol envelope
    ctx.sub(event, fn)               — subscribe to a bus event (auto-unsubscribed on dispose)
    ctx.emit(event, data)            — publish a bus event
    ctx.state                        — shared mutable app state
    ctx.slot                         — the element this surface was mounted into

  A *layout* is a skeleton (HTML with [data-slot] holes) + a placement map {slotName: surfaceName}.
  Layouts are selectable just like colour themes and size tokens — the third, placement axis. The
  same surface code runs whether it's mounted in a page slot here or as the sole content of a
  standalone window (e.g. an Avalonia WebView on /surface/<name>): only the mount point differs.
*/
window.Spla = (() => {
  const surfaceDefs = {};
  const layoutDefs = {};

  function registerSurface(name, factory) { surfaceDefs[name] = factory; }
  function registerLayout(name, def) { layoutDefs[name] = def; }
  function layoutNames() { return Object.keys(layoutDefs); }

  /** A tiny synchronous event bus. on() returns an unsubscribe fn. */
  function makeBus() {
    const listeners = {};
    return {
      on(ev, fn) {
        (listeners[ev] ||= []).push(fn);
        return () => { listeners[ev] = (listeners[ev] || []).filter(f => f !== fn); };
      },
      emit(ev, data, env) {
        (listeners[ev] || []).slice().forEach(fn => {
          try { fn(data, env); } catch (e) { console.error("surface handler for", ev, e); }
        });
      }
    };
  }

  /** Mount one surface into a slot. Wraps the context so subscriptions auto-clean on dispose. */
  function mountSurface(name, slot, ctx) {
    const factory = surfaceDefs[name];
    if (!factory) {
      slot.innerHTML = `<div class="surface-missing">no surface: ${name}</div>`;
      return { el: null, dispose() {} };
    }
    const offs = [];
    const sctx = Object.assign({}, ctx, {
      slot,
      sub(ev, fn) { offs.push(ctx.on(ev, fn)); }
    });
    const inst = factory(sctx) || {};
    if (inst.el && inst.el.parentNode !== slot) slot.appendChild(inst.el);
    const userDispose = inst.dispose;
    inst.dispose = () => { try { userDispose && userDispose(); } finally { offs.forEach(o => o && o()); } };
    return inst;
  }

  let _mounted = [];

  /**
   * Build a layout into `root` and mount its surfaces. Disposes whatever was mounted before, so
   * switching layouts (the placement axis) is clean — no leaked bus subscriptions.
   */
  function applyLayout(root, name, ctx) {
    _mounted.forEach(m => m && m.dispose && m.dispose());
    _mounted = [];

    const def = layoutDefs[name] || layoutDefs[layoutNames()[0]];
    root.innerHTML = def.skeleton;
    root.setAttribute("data-layout", name);

    for (const [slotName, surfaceName] of Object.entries(def.placement)) {
      const slot = root.querySelector(`[data-slot="${slotName}"]`);
      if (slot) _mounted.push(mountSurface(surfaceName, slot, ctx));
    }
    ctx.emit && ctx.emit("layout.applied", { name });
  }

  return {
    registerSurface, registerLayout, layoutNames,
    makeBus, mountSurface, applyLayout,
    surfaceDefs, layoutDefs
  };
})();
