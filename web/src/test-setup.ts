// The bits of a browser that module top-level code touches. Kept to a stub rather than pulling in a
// full DOM: these tests exercise state and dispatch, which need no document at all.
const store = new Map<string, string>();

Object.defineProperty(globalThis, "localStorage", {
  value: {
    getItem: (k: string) => store.get(k) ?? null,
    setItem: (k: string, v: string) => void store.set(k, String(v)),
    removeItem: (k: string) => void store.delete(k),
    clear: () => store.clear()
  },
  configurable: true
});

if (!("navigator" in globalThis)) {
  Object.defineProperty(globalThis, "navigator", { value: {}, configurable: true });
}
