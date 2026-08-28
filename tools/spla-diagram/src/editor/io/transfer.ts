import { builtinStyleSheet } from "../../model/style-defaults.js";
import type { WireStyleSheet } from "../../model/style-types.js";
import type { WireDocument } from "../../model/wire-types.js";

export interface SaveTarget {
  /** File name as the server knows it, e.g. "model-core.json". */
  readonly file: string;
}

export interface ModelStore {
  load(file: string): Promise<WireDocument>;
  save(target: SaveTarget, wire: WireDocument): Promise<void>;
}

/**
 * Reads and writes models over HTTP.
 *
 * This is the only piece that knows the server exists. `DiagramCanvas` has no
 * idea, and `DiagramEditor` talks to the interface rather than to fetch, so an
 * embedder can supply storage of its own.
 */
export class HttpModelStore implements ModelStore {
  constructor(private readonly baseUrl: string = "./") {}

  async load(file: string): Promise<WireDocument> {
    const res = await fetch(new URL(file, new URL(this.baseUrl, location.href)));
    if (!res.ok) throw new Error(`Не удалось загрузить ${file}: HTTP ${res.status}`);
    return (await res.json()) as WireDocument;
  }

  async save(target: SaveTarget, wire: WireDocument): Promise<void> {
    const res = await fetch(`/api/save?file=${encodeURIComponent(target.file)}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(wire, null, 2),
    });
    if (!res.ok) throw new Error(`Сервер ответил HTTP ${res.status}`);
  }
}

/**
 * Where the style library lives.
 *
 * Split from `ModelStore` rather than folded into it: styles outlive any one
 * model and are saved on their own schedule. A model may not even be saveable
 * (opened from disk, bound to no file on the server) while the styles it was
 * just edited through still are.
 */
export interface StyleStore {
  load(): Promise<WireStyleSheet>;
  save(sheet: WireStyleSheet): Promise<void>;
}

/** File name the sheet occupies in the models directory. */
const STYLE_FILE = "styles.json";

/**
 * Reads and writes `styles.json` next to the models, over the same endpoints
 * as `HttpModelStore`.
 *
 * A missing file is not an error: until someone saves for the first time there
 * is nothing on the server, and the built-in library is a complete, working
 * answer. Any other failure *is* surfaced — a 500 silently swallowed into
 * "здесь ваши встроенные стили" would hide the fact that the next save is about
 * to overwrite a file nobody could read (D-11).
 */
export class HttpStyleStore implements StyleStore {
  constructor(private readonly baseUrl: string = "./") {}

  async load(): Promise<WireStyleSheet> {
    const res = await fetch(new URL(STYLE_FILE, new URL(this.baseUrl, location.href)));
    if (res.status === 404) return builtinStyleSheet();
    if (!res.ok) throw new Error(`Не удалось загрузить ${STYLE_FILE}: HTTP ${res.status}`);
    return (await res.json()) as WireStyleSheet;
  }

  async save(sheet: WireStyleSheet): Promise<void> {
    const res = await fetch(`/api/save?file=${encodeURIComponent(STYLE_FILE)}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(sheet, null, 2),
    });
    if (!res.ok) throw new Error(`Сервер ответил HTTP ${res.status}`);
  }
}

/** Offer a generated file to the user as a download. */
export function download(fileName: string, content: string, mime: string): void {
  const blob = new Blob([content], { type: mime });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
}

export function readJsonFile(file: File): Promise<WireDocument> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onerror = () => reject(new Error("Не удалось прочитать файл"));
    reader.onload = () => {
      try {
        resolve(JSON.parse(String(reader.result)) as WireDocument);
      } catch (err) {
        reject(new Error(`Ошибка разбора JSON: ${(err as Error).message}`));
      }
    };
    reader.readAsText(file);
  });
}
