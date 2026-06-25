import { StrictMode, useEffect, useMemo, useRef } from "react";
import { createRoot } from "react-dom/client";
import { JsonForms } from "@jsonforms/react";
import { vanillaRenderers, vanillaCells } from "@jsonforms/vanilla-renderers";
import type { JsonSchema, UISchemaElement } from "@jsonforms/core";
import Ajv2020 from "ajv/dist/2020";
import addFormats from "ajv-formats";

// Данные ДЕТЕРМИНИРОВАННО запечены хостом в window.__SPLA_PAYLOAD ПЕРЕД загрузкой.
interface WebTheme {
  bg: string; bgPanel: string; bgInput: string;
  border: string; borderFocus: string;
  text: string; textSub: string; accent: string;
  fontSize: number; smallSize: number;
  dark: boolean;
}
interface Payload {
  schema: JsonSchema;
  uischema: UISchemaElement | null;
  data: unknown;
  readOnly: boolean;
  docId: string;
  theme: WebTheme | string; // объект или "light"/"dark" (fallback)
}
const payload = (window as unknown as { __SPLA_PAYLOAD?: Payload }).__SPLA_PAYLOAD ?? null;

// Нормализуем тему — принимаем и объект и строку-fallback
function resolveTheme(raw: WebTheme | string | undefined): WebTheme {
  if (raw && typeof raw === "object") return raw;
  const dark = raw === "dark";
  return {
    bg:          dark ? "#1e1e1e" : "#ffffff",
    bgPanel:     dark ? "#252526" : "#f5f5f5",
    bgInput:     dark ? "#3c3c3c" : "#ffffff",
    border:      dark ? "#444444" : "#cccccc",
    borderFocus: "#007acc",
    text:        dark ? "#d4d4d4" : "#1f1f1f",
    textSub:     dark ? "#9d9d9d" : "#666666",
    accent:      "#007acc",
    fontSize:    13, smallSize: 11,
    dark,
  };
}
const theme = resolveTheme(payload?.theme);

// ── Инжектируем CSS-переменные и базовые стили ───────────────────────────────
const style = document.createElement("style");
style.textContent = `
  :root {
    color-scheme: ${theme.dark ? "dark" : "light"};
    --bg:           ${theme.bg};
    --bg-panel:     ${theme.bgPanel};
    --bg-input:     ${theme.bgInput};
    --border:       ${theme.border};
    --border-focus: ${theme.borderFocus};
    --text:         ${theme.text};
    --text-sub:     ${theme.textSub};
    --accent:       ${theme.accent};
    --fs:           ${theme.fontSize}px;
    --fs-sm:        ${theme.smallSize}px;
    --radius:       3px;
    --err-bg:       ${theme.dark ? "#3e1f1f" : "#fff3f3"};
    --err-text:     ${theme.dark ? "#f48771" : "#b00020"};
    --btn-bg:       ${theme.bgPanel};
    --btn-hover:    ${theme.bgInput};
    --hover:        ${theme.bgPanel};
    --selected:     ${theme.dark ? "#0e3a5a" : "#cce5ff"};
  }
  html, body {
    background: var(--bg); color: var(--text);
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
    font-size: var(--fs); margin: 0;
  }
  #root { padding: 10px 14px; }

  /* ── Группы / fieldset ── */
  fieldset {
    border: 1px solid var(--border); border-radius: var(--radius);
    margin: 0 0 8px; padding: 6px 10px 10px;
    background: var(--bg-panel);
  }
  legend {
    color: var(--text-sub); font-size: var(--fs-sm);
    font-weight: 600; text-transform: uppercase;
    letter-spacing: 0.05em; padding: 0 4px;
  }

  /* ── Строки control ── */
  .control { display: flex; align-items: baseline; gap: 8px; margin: 4px 0; }
  label { color: var(--text); font-size: var(--fs-sm); min-width: 80px; flex-shrink: 0; }

  /* ── Inputs / selects ── */
  input[type="text"], input[type="number"], input[type="email"],
  input[type="url"], input[type="password"], input[type="date"],
  input[type="datetime-local"], textarea, select {
    background: var(--bg-input); color: var(--text);
    border: 1px solid var(--border); border-radius: var(--radius);
    padding: 2px 6px; font-size: var(--fs); font-family: inherit;
    outline: none; transition: border-color 0.1s; flex: 1; min-width: 0;
  }
  input:focus, textarea:focus, select:focus { border-color: var(--border-focus); }
  input[readonly], input:disabled, textarea[readonly], select:disabled {
    opacity: 0.75; cursor: default;
  }

  /* ── Кнопки ── */
  button {
    background: var(--btn-bg); color: var(--text);
    border: 1px solid var(--border); border-radius: var(--radius);
    padding: 2px 10px; font-size: var(--fs-sm); font-family: inherit;
    cursor: pointer; white-space: nowrap;
  }
  button:hover { background: var(--btn-hover); }
  button[aria-label*="elete"], button[aria-label*="emove"], button.delete {
    color: var(--err-text); background: var(--err-bg); border-color: var(--border);
  }

  /* ── Таблицы (array renderers) ── */
  table { border-collapse: collapse; width: 100%; font-size: var(--fs-sm); }
  th {
    background: var(--bg); color: var(--text-sub);
    font-size: var(--fs-sm); font-weight: 600;
    text-transform: uppercase; letter-spacing: 0.04em;
    border-bottom: 1px solid var(--border);
    padding: 4px 8px; text-align: left;
  }
  td { border-bottom: 1px solid var(--border); padding: 2px 4px; vertical-align: middle; }
  tr:hover td { background: var(--hover); }

  /* ── Array: кнопка добавления ── */
  .array-layout > div:first-child { margin-bottom: 6px; }

  /* ── Чекбоксы ── */
  input[type="checkbox"] { accent-color: var(--accent); width: 13px; height: 13px; }

  /* ── Ошибки ── */
  .validation, .error { color: var(--err-text); font-size: var(--fs-sm); margin-top: 2px; }
  #err {
    background: var(--err-bg); color: var(--err-text);
    font-family: monospace; font-size: 12px;
    padding: 6px 10px; white-space: pre-wrap; display: none;
    border-bottom: 1px solid var(--border);
  }

  ${theme.dark ? `
  * { scrollbar-width: thin; scrollbar-color: ${theme.border} ${theme.bg}; }
  ::-webkit-scrollbar { width: 8px; height: 8px; }
  ::-webkit-scrollbar-track { background: ${theme.bg}; }
  ::-webkit-scrollbar-thumb { background: ${theme.border}; border-radius: 4px; }
  ` : ""}
`;
document.head.appendChild(style);

// ── Сохранение page → host ────────────────────────────────────────────────────
function postSave(data: unknown, docId: string) {
  const w = window as unknown as {
    chrome?: { webview?: { postMessage(m: unknown): void } };
    webkit?: { messageHandlers?: { spla?: { postMessage(m: unknown): void } } };
  };
  const msg = JSON.stringify({ action: "save", data: JSON.stringify(data), docId });
  if (w.chrome?.webview?.postMessage) w.chrome.webview.postMessage(msg);
  else if (w.webkit?.messageHandlers?.spla?.postMessage) w.webkit.messageHandlers.spla.postMessage(msg);
}

function createAjv2020() {
  const ajv = new Ajv2020({ allErrors: true, verbose: true, strict: false, useDefaults: true });
  addFormats(ajv);
  return ajv;
}

function Editor() {
  const renderers = useMemo(() => vanillaRenderers, []);
  const cells = useMemo(() => vanillaCells, []);
  const ajv = useMemo(() => createAjv2020(), []);
  const dataRef = useRef<unknown>(payload?.data);
  const dirtyRef = useRef(false);
  const initRef = useRef(true);

  useEffect(() => {
    const onBlur = () => {
      if (payload && !payload.readOnly && dirtyRef.current) {
        dirtyRef.current = false;
        postSave(dataRef.current, payload.docId);
      }
    };
    window.addEventListener("blur", onBlur);
    return () => window.removeEventListener("blur", onBlur);
  }, []);

  if (!payload) return <div style={{ padding: 12, color: theme.textSub }}>No payload.</div>;

  return (
    <JsonForms
      schema={payload.schema}
      uischema={payload.uischema ?? undefined}
      data={payload.data}
      renderers={renderers}
      cells={cells}
      ajv={ajv}
      readonly={payload.readOnly}
      onChange={({ data }) => {
        dataRef.current = data;
        if (initRef.current) initRef.current = false;
        else dirtyRef.current = true;
      }}
    />
  );
}

const boot = document.getElementById("boot");
if (boot) boot.remove();

createRoot(document.getElementById("root")!).render(
  <StrictMode><Editor /></StrictMode>
);
