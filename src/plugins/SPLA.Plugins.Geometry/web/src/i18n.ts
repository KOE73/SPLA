// The host's translator, borrowed.
//
// A plugin bundle ships no dictionary of its own: the key is the English source text, so the host
// looks these strings up in the same table it uses for its own UI, and one dictionary covers the
// whole window. Until mount() hands the function over — and on an older host that does not send
// one — every string passes through unchanged, which is exactly the English the panel had before.
type Translate = (text: string, params?: Record<string, unknown>) => string;

let host: Translate | null = null;

export function useHostTranslator(fn?: Translate) { host = fn ?? null; }

export function t(text: string, params?: Record<string, unknown>): string {
  return host ? host(text, params) : text;
}
