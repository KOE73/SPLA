import type { Envelope, ServerEvents, WireFrame } from "./types";

type Handler<P> = (payload: P, env: Envelope<P>) => void;
type WireListener = (frame: WireFrame) => void;

interface Pending {
  resolve: (v: unknown) => void;
  reject: (e: unknown) => void;
  timer: number;
}

/**
 * WebSocket transport + typed event bus + Promise-based RPC.
 * `send()` fires events (assistant streaming, broadcasts); `invoke()` is for commands
 * that expect a single correlated response (the server must echo back the same requestId).
 */
export class SplaClient {
  private ws?: WebSocket;
  private listeners = new Map<string, Set<Handler<unknown>>>();
  private pending = new Map<string, Pending>();
  private wireListeners = new Set<WireListener>();
  private reconnectTimer = 0;

  connect(): void {
    const proto = location.protocol === "https:" ? "wss://" : "ws://";
    this.ws = new WebSocket(proto + location.host + "/ws");
    this.ws.onopen = () => this.send("hello", { clientName: "web", protocolVersion: "1" });
    this.ws.onclose = () => {
      this.emit("conn", { on: false, text: "disconnected — retrying" }, { type: "conn", payload: {} });
      this.reconnectTimer = window.setTimeout(() => this.connect(), 1500);
    };
    this.ws.onmessage = ev => {
      let env: Envelope;
      try { env = JSON.parse(ev.data); } catch { return; } // malformed frame — drop, don't throw
      this.emitWire({ dir: "in", type: env.type, payload: env.payload, chatId: env.chatId, projectId: env.projectId, requestId: env.requestId, ts: Date.now() });

      if (env.requestId && this.pending.has(env.requestId)) {
        const p = this.pending.get(env.requestId)!;
        clearTimeout(p.timer);
        this.pending.delete(env.requestId);
        p.resolve(env.payload);
      }
      if (env.type === "welcome") this.emit("conn", { on: true, text: "connected" }, env);
      this.emit(env.type as keyof ServerEvents, env.payload as never, env);
    };
  }

  disconnect(): void {
    clearTimeout(this.reconnectTimer);
    this.ws?.close();
  }

  /** Fire-and-forget send. Returns false (does not throw) if the socket isn't open. */
  send(type: string, payload?: unknown, extra?: { chatId?: string; projectId?: string; requestId?: string }): boolean {
    const ok = !!this.ws && this.ws.readyState === WebSocket.OPEN;
    if (ok) this.ws!.send(JSON.stringify({ type, payload, ...extra }));
    // Only claim "out" in the wire log if it was actually sent — never lie about delivery.
    if (ok) this.emitWire({ dir: "out", type, payload, chatId: extra?.chatId, projectId: extra?.projectId, requestId: extra?.requestId, ts: Date.now() });
    return ok;
  }

  /** Command with a correlated response. Server must echo the same requestId. */
  invoke<R = unknown>(type: string, payload?: unknown, extra?: { chatId?: string; projectId?: string }, timeoutMs = 15000): Promise<R> {
    const requestId = crypto.randomUUID();
    return new Promise<R>((resolve, reject) => {
      const timer = window.setTimeout(() => {
        this.pending.delete(requestId);
        reject(new Error(`timeout waiting for response to "${type}"`));
      }, timeoutMs);
      this.pending.set(requestId, { resolve: resolve as (v: unknown) => void, reject, timer });
      if (!this.send(type, payload, { ...extra, requestId })) {
        clearTimeout(timer);
        this.pending.delete(requestId);
        reject(new Error(`socket closed — could not send "${type}"`));
      }
    });
  }

  on<K extends keyof ServerEvents>(type: K, fn: Handler<ServerEvents[K]>): () => void {
    const key = type as string;
    let set = this.listeners.get(key);
    if (!set) { set = new Set(); this.listeners.set(key, set); }
    set.add(fn as Handler<unknown>);
    return () => { this.listeners.get(key)?.delete(fn as Handler<unknown>); };
  }

  onWire(fn: WireListener): () => void {
    this.wireListeners.add(fn);
    return () => { this.wireListeners.delete(fn); };
  }

  private emit<K extends keyof ServerEvents>(type: K, payload: ServerEvents[K], env: Envelope) {
    this.listeners.get(type as string)?.forEach(fn => {
      try { fn(payload as never, env as never); } catch (e) { console.error("handler for", type, e); }
    });
  }

  private emitWire(frame: Parameters<WireListener>[0]) {
    this.wireListeners.forEach(fn => { try { fn(frame); } catch (e) { console.error("wire listener", e); } });
  }
}

export const client = new SplaClient();
