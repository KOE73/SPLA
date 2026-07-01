// Local-only UI events that never touch the network (e.g. "open the debug drawer").
// Mirrors the old app.js bus.emit/.on for events that aren't server protocol messages.
type Handler = (payload?: unknown) => void;

const listeners = new Map<string, Set<Handler>>();

export const uiBus = {
  emit(event: string, payload?: unknown) {
    listeners.get(event)?.forEach(fn => { try { fn(payload); } catch (e) { console.error("uiBus handler for", event, e); } });
  },
  on(event: string, fn: Handler): () => void {
    let set = listeners.get(event);
    if (!set) { set = new Set(); listeners.set(event, set); }
    set.add(fn);
    return () => { listeners.get(event)?.delete(fn); };
  }
};
