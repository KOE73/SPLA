import { EditorState, type Extension, Compartment } from "@codemirror/state";
import { EditorView, basicSetup } from "codemirror";
import { markdown } from "@codemirror/lang-markdown";
import { json } from "@codemirror/lang-json";
import { yaml } from "@codemirror/lang-yaml";
import { sql } from "@codemirror/lang-sql";
import {
  StreamLanguage, HighlightStyle, syntaxHighlighting,
  LanguageDescription, LanguageSupport,
} from "@codemirror/language";
import { csharp, c, cpp, java } from "@codemirror/legacy-modes/mode/clike";
import { tags as t } from "@lezer/highlight";
import { Marked } from "marked";
import { markedHighlight } from "marked-highlight";
import hljs from "highlight.js/lib/core";
import hljsCs from "highlight.js/lib/languages/csharp";
import hljsJs from "highlight.js/lib/languages/javascript";
import hljsTs from "highlight.js/lib/languages/typescript";
import hljsJson from "highlight.js/lib/languages/json";
import hljsSql from "highlight.js/lib/languages/sql";
import hljsXml from "highlight.js/lib/languages/xml";
import hljsBash from "highlight.js/lib/languages/bash";
import hljsPy from "highlight.js/lib/languages/python";
import hljsYaml from "highlight.js/lib/languages/yaml";
import hljsCss from "highlight.js/lib/languages/css";
import lightCss from "highlight.js/styles/github.css?inline";
import darkCss from "highlight.js/styles/github-dark.css?inline";
import { postToHost } from "./bridge";

// ── highlight.js для fenced-кода в markdown-превью ──────────────────────────
hljs.registerLanguage("csharp", hljsCs);
hljs.registerLanguage("javascript", hljsJs);
hljs.registerLanguage("typescript", hljsTs);
hljs.registerLanguage("json", hljsJson);
hljs.registerLanguage("sql", hljsSql);
hljs.registerLanguage("xml", hljsXml);
hljs.registerLanguage("bash", hljsBash);
hljs.registerLanguage("python", hljsPy);
hljs.registerLanguage("yaml", hljsYaml);
hljs.registerLanguage("css", hljsCss);

// ── Payload ──────────────────────────────────────────────────────────────────
interface WebTheme {
  bg: string; bgPanel: string; bgInput: string;
  border: string; borderFocus: string;
  text: string; textSub: string; accent: string;
  fontSize: number; smallSize: number;
  dark: boolean;
}
interface Payload {
  text: string;
  contentType: string;
  readOnly: boolean;
  docId: string;
  theme: WebTheme | string;
}
const payload = (window as unknown as { __SPLA_PAYLOAD?: Payload }).__SPLA_PAYLOAD ?? {
  text: "", contentType: "txt", readOnly: true, docId: "", theme: "light",
};

function resolveTheme(raw: WebTheme | string): WebTheme {
  if (raw && typeof raw === "object") return raw;
  const dark = raw === "dark";
  return {
    bg: dark ? "#1e1e1e" : "#ffffff", bgPanel: dark ? "#252526" : "#f5f5f5",
    bgInput: dark ? "#3c3c3c" : "#ffffff", border: dark ? "#444" : "#ccc",
    borderFocus: "#007acc", text: dark ? "#d4d4d4" : "#1f1f1f",
    textSub: dark ? "#9d9d9d" : "#666", accent: "#007acc",
    fontSize: 13, smallSize: 11, dark,
  };
}
const theme = resolveTheme(payload.theme);
const isDark = theme.dark;

// ── Инжектируем нужный hljs CSS (один из двух, собранных в бандл через ?inline) ──
const hljsStyle = document.createElement("style");
hljsStyle.textContent = isDark ? darkCss : lightCss;
document.head.appendChild(hljsStyle);

// ── Цвета preview-блока и базовые стили по теме ──────────────────────────────
const previewStyle = document.createElement("style");
previewStyle.textContent = `
  html, body { background: ${theme.bg}; color: ${theme.text}; font-size: ${theme.fontSize}px; margin: 0; }
  #preview { border-left: 1px solid ${theme.border}; }
  #preview pre { background: ${theme.bgPanel}; border-radius: 3px; padding: 8px; }
  #preview code:not(.hljs) { background: ${theme.bgPanel}; padding: 2px 4px; border-radius: 3px; }
  #preview a { color: ${theme.accent}; }
  #preview h1, #preview h2, #preview h3 { border-bottom: 1px solid ${theme.border}; padding-bottom: 4px; }
  ${isDark ? `* { scrollbar-width: thin; scrollbar-color: #555 ${theme.bg}; }` : ""}
`;
document.head.appendChild(previewStyle);

const marked = new Marked(
  markedHighlight({
    langPrefix: "hljs language-",
    highlight(code, lang) {
      const language = hljs.getLanguage(lang) ? lang : "plaintext";
      return hljs.highlight(code, { language }).value;
    },
  })
);

// ── Подсветка вложенного кода в markdown-редакторе ───────────────────────────
const mdCodeLanguages = [
  LanguageDescription.of({ name: "json", load: async () => json() }),
  LanguageDescription.of({ name: "sql", load: async () => sql() }),
  LanguageDescription.of({ name: "yaml", load: async () => yaml() }),
  LanguageDescription.of({ name: "csharp", alias: ["cs"], load: async () => new LanguageSupport(StreamLanguage.define(csharp)) }),
  LanguageDescription.of({ name: "c", load: async () => new LanguageSupport(StreamLanguage.define(c)) }),
  LanguageDescription.of({ name: "cpp", load: async () => new LanguageSupport(StreamLanguage.define(cpp)) }),
  LanguageDescription.of({ name: "java", load: async () => new LanguageSupport(StreamLanguage.define(java)) }),
];

// ── Подсветка синтаксиса CodeMirror: светлая и тёмная ────────────────────────
const lightHighlight = HighlightStyle.define([
  { tag: [t.keyword, t.definitionKeyword, t.modifier, t.controlKeyword], color: "#0033b3", fontWeight: "bold" },
  { tag: [t.string, t.special(t.string)], color: "#067d17" },
  { tag: [t.comment, t.lineComment, t.blockComment], color: "#8c8c8c", fontStyle: "italic" },
  { tag: [t.number, t.bool, t.null, t.atom], color: "#1750eb" },
  { tag: [t.typeName, t.className, t.namespace], color: "#00627a" },
  { tag: [t.propertyName, t.attributeName], color: "#871094" },
  { tag: [t.function(t.variableName), t.labelName], color: "#7a3e9d" },
  { tag: [t.operator, t.punctuation], color: "#5c6370" },
  { tag: [t.tagName], color: "#0033b3" },
  { tag: t.heading, color: "#0033b3", fontWeight: "bold" },
  { tag: t.strong, fontWeight: "bold" },
  { tag: t.emphasis, fontStyle: "italic" },
  { tag: [t.link, t.url], color: "#1750eb", textDecoration: "underline" },
]);

const darkHighlight = HighlightStyle.define([
  { tag: [t.keyword, t.definitionKeyword, t.modifier, t.controlKeyword], color: "#569cd6", fontWeight: "bold" },
  { tag: [t.string, t.special(t.string)], color: "#ce9178" },
  { tag: [t.comment, t.lineComment, t.blockComment], color: "#6a9955", fontStyle: "italic" },
  { tag: [t.number, t.bool, t.null, t.atom], color: "#b5cea8" },
  { tag: [t.typeName, t.className, t.namespace], color: "#4ec9b0" },
  { tag: [t.propertyName, t.attributeName], color: "#9cdcfe" },
  { tag: [t.function(t.variableName), t.labelName], color: "#dcdcaa" },
  { tag: [t.operator, t.punctuation], color: "#d4d4d4" },
  { tag: [t.tagName], color: "#569cd6" },
  { tag: t.heading, color: "#569cd6", fontWeight: "bold" },
  { tag: t.strong, fontWeight: "bold" },
  { tag: t.emphasis, fontStyle: "italic" },
  { tag: [t.link, t.url], color: "#4ec9b0", textDecoration: "underline" },
]);

// Базовая тёмная тема для CM (фон, курсор, выделение) — из реальных цветов темы
const darkEditorTheme = EditorView.theme({
  "&": { background: theme.bg, color: theme.text, fontSize: theme.fontSize + "px" },
  ".cm-content": { caretColor: theme.text },
  ".cm-cursor": { borderLeftColor: theme.text },
  "&.cm-focused .cm-selectionBackground, .cm-selectionBackground": { background: "#264f78" },
  ".cm-gutters": { background: theme.bg, color: theme.textSub, borderRight: `1px solid ${theme.border}` },
  ".cm-activeLineGutter": { background: theme.bgPanel },
  ".cm-activeLine": { background: theme.bgPanel },
  ".cm-line": { padding: "0 4px" },
}, { dark: true });

const lightEditorTheme = EditorView.theme({
  "&": { background: theme.bg, color: theme.text, fontSize: theme.fontSize + "px" },
  ".cm-gutters": { background: theme.bgPanel, color: theme.textSub, borderRight: `1px solid ${theme.border}` },
});

const editorEl = document.getElementById("editor")!;
const previewEl = document.getElementById("preview")!;
const language = new Compartment();

let dirty = false;

function languageExtension(contentType: string): Extension {
  switch (contentType) {
    case "md": return markdown({ codeLanguages: mdCodeLanguages });
    case "json":
    case "jsonl": return json();
    case "yaml": return yaml();
    case "sql": return sql();
    case "cs": return StreamLanguage.define(csharp);
    case "c": return StreamLanguage.define(c);
    case "cpp": return StreamLanguage.define(cpp);
    case "java": return StreamLanguage.define(java);
    default: return [];
  }
}

function applyLayout(text: string) {
  const isMd = payload.contentType === "md";
  const previewOnly = isMd && payload.readOnly;
  editorEl.style.display = previewOnly ? "none" : "";
  editorEl.classList.toggle("full", !isMd);
  previewEl.classList.toggle("on", isMd);
  if (isMd) previewEl.innerHTML = marked.parse(text, { async: false }) as string;
}

function saveIfDirty() {
  if (!payload.readOnly && dirty && view) {
    dirty = false;
    postToHost({ action: "save", text: view.state.doc.toString(), docId: payload.docId });
  }
}

const extensions: Extension[] = [
  basicSetup,
  ...(isDark ? [darkEditorTheme, syntaxHighlighting(darkHighlight)] : [lightEditorTheme, syntaxHighlighting(lightHighlight)]),
  language.of(languageExtension(payload.contentType)),
  EditorState.readOnly.of(payload.readOnly),
  EditorView.updateListener.of((u) => {
    if (u.docChanged) {
      dirty = true;
      applyLayout(u.state.doc.toString());
    }
  }),
  EditorView.domEventHandlers({ blur: () => { saveIfDirty(); return false; } }),
];

const view = new EditorView({
  state: EditorState.create({ doc: payload.text, extensions }),
  parent: editorEl,
});

applyLayout(payload.text);
window.addEventListener("blur", saveIfDirty);
