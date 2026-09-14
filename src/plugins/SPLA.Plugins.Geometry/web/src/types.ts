// What the plugin's panel session publishes (Panel/GeometryPanelProvider.cs). The state event
// carries no pixels: it names the renders in the ring, and each picture is asked for once by name.

export interface RenderRef {
  name: string;
  /** When this ring slot was last written. Part of the cache key: the names rotate, so the same
   * name is a different picture after the ring has wrapped. */
  at: string;
  size: number;
}

export interface GeometryState {
  chatId: string | null;
  hasSession: boolean;
  source?: string;
  sourceWidth?: number;
  sourceHeight?: number;
  view?: { id: string; width: number; height: number; fromBox?: string | null; deskewed?: boolean };
  objects?: Array<{ name: string; kind: string; status: string; visible: boolean; where: string }>;
  renders?: RenderRef[];
  /** The head of the ring — the newest render. */
  current?: string | null;
}

export interface RenderEvent {
  name: string;
  at?: string;
  mimeType?: string;
  base64?: string;
  missing?: boolean;
}
