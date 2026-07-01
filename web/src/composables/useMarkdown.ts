// Markdown/mermaid rendering — ported verbatim from WebClient/renderers.js (lines 17-83).
// Do NOT "improve" the parsing logic here; it's deliberately unchanged. The only difference
// from the original is that callers now get a plain async function instead of a global object,
// and the assistant bubble drives WHEN to call it (debounced) instead of this module deciding.
declare global {
  interface Window {
    marked?: { parse(md: string, opts: { gfm: boolean; breaks: boolean }): string };
    mermaid?: { initialize(opts: unknown): void; render(id: string, src: string): Promise<{ svg: string }> };
  }
}

const cssVar = (n: string) => getComputedStyle(document.documentElement).getPropertyValue(n).trim();

function escapeHtml(v: unknown): string {
  return String(v ?? "").replace(/[&<>"']/g, c =>
    ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" } as Record<string, string>)[c]);
}
const neutralizeRawHtml = (v: unknown) => String(v ?? "").replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;");

function mermaidTheme() {
  return {
    startOnLoad: false, securityLevel: "strict", theme: "base",
    themeVariables: {
      background: cssVar("--panel"), mainBkg: cssVar("--panel"),
      primaryColor: cssVar("--assistant-bg"), primaryBorderColor: cssVar("--accent"),
      primaryTextColor: cssVar("--text"), lineColor: cssVar("--accent"),
      textColor: cssVar("--text"), fontFamily: cssVar("--font") || "sans-serif"
    }
  };
}
if (window.mermaid) window.mermaid.initialize(mermaidTheme());
export function reinitMermaidTheme() { if (window.mermaid) window.mermaid.initialize(mermaidTheme()); }

async function renderMermaid(container: HTMLElement, blocks: string[]) {
  const hosts = container.querySelectorAll<HTMLElement>(".mermaid-frame[data-mi]");
  for (const host of hosts) {
    const src = blocks[Number(host.dataset.mi)] ?? "";
    const id = "m" + Math.random().toString(36).slice(2);
    try {
      if (!window.mermaid) throw new Error("mermaid not loaded");
      const { svg } = await window.mermaid.render(id, src);
      host.innerHTML = svg;
    } catch (e) {
      host.innerHTML = `<div class="diagram-error"><b>Mermaid error:</b> ${escapeHtml(e instanceof Error ? e.message : String(e))}<pre>${escapeHtml(src)}</pre></div>`;
    }
  }
}

function enhanceCode(container: HTMLElement) {
  container.querySelectorAll("pre").forEach(pre => {
    if (pre.parentElement?.classList.contains("code-frame")) return;
    const code = pre.querySelector("code");
    const lang = (code?.className.match(/language-(\S+)/) || [, ""])[1];
    const frame = document.createElement("div"); frame.className = "code-frame";
    const head = document.createElement("div"); head.className = "code-head";
    head.innerHTML = `<span>${escapeHtml(lang || "code")}</span>`;
    const copy = document.createElement("button"); copy.className = "copy-btn"; copy.textContent = "copy";
    copy.onclick = () => navigator.clipboard?.writeText(pre.innerText);
    head.appendChild(copy);
    pre.parentNode?.insertBefore(frame, pre);
    frame.appendChild(head); frame.appendChild(pre);
  });
}

/** Renders markdown (with mermaid + code-frame post-processing) into `container`. */
export async function renderMarkdown(container: HTMLElement, markdown: string): Promise<void> {
  const blocks: string[] = [];
  const tok = "@@SPLA_MERMAID_";
  let md = String(markdown ?? "").replace(/```mermaid[^\n\r]*(?:\r?\n)([\s\S]*?)```/gi, (_, src) => {
    const i = blocks.push(src.trim()) - 1; return `${tok}${i}@@`;
  });
  md = neutralizeRawHtml(md).replace(new RegExp(`${tok}(\\d+)@@`, "g"),
    (_, i) => `<div class="mermaid-frame" data-mi="${i}"></div>`);

  container.innerHTML = window.marked ? window.marked.parse(md, { gfm: true, breaks: true }) : escapeHtml(md);
  enhanceCode(container);
  await renderMermaid(container, blocks);
}

export { escapeHtml };
