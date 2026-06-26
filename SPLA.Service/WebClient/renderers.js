/*
  SPLA web client — element renderers. One builder per chat element type, deliberately separated so a
  single element's look can be changed in isolation. app.js orchestrates the protocol and calls these;
  it never builds DOM itself.

  Exposes window.SplaRenderers:
    userMsg(content)                         → user bubble
    assistantShell()                         → { el, renderInto(text), setReasoning(text) }
    toolLine(text)                           → tool notice line
    system(text) / notice(text)              → centered system/notice line
    permission(reqId, payload, onDecide)     → permission ask block
    clarify(reqId, payload, onChoose)        → clarify ask block
    renderMarkdown(container, md)            → fills a container with rich markdown
    reinitMermaidTheme()                     → re-read CSS vars into mermaid (call on theme change)
*/
const SplaRenderers = (() => {
  const cssVar = n => getComputedStyle(document.documentElement).getPropertyValue(n).trim();

  function escapeHtml(v) {
    return String(v ?? "").replace(/[&<>"']/g, c =>
      ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
  }
  const neutralizeRawHtml = v => String(v ?? "").replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;");

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
  if (window.mermaid) mermaid.initialize(mermaidTheme());
  function reinitMermaidTheme() { if (window.mermaid) mermaid.initialize(mermaidTheme()); }

  // ── Markdown (marked) + code frames + mermaid ──────────────────────────────
  async function renderMarkdown(container, markdown) {
    const blocks = [];
    const tok = "@@SPLA_MERMAID_";
    let md = String(markdown ?? "").replace(/```mermaid[^\n\r]*(?:\r?\n)([\s\S]*?)```/gi, (_, src) => {
      const i = blocks.push(src.trim()) - 1; return `${tok}${i}@@`;
    });
    md = neutralizeRawHtml(md).replace(new RegExp(`${tok}(\\d+)@@`, "g"),
      (_, i) => `<div class="mermaid-frame" data-mi="${i}"></div>`);

    container.innerHTML = window.marked ? marked.parse(md, { gfm: true, breaks: true }) : escapeHtml(md);
    enhanceCode(container);
    await renderMermaid(container, blocks);
  }

  function enhanceCode(container) {
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
      pre.parentNode.insertBefore(frame, pre);
      frame.appendChild(head); frame.appendChild(pre);
    });
  }

  async function renderMermaid(container, blocks) {
    const hosts = container.querySelectorAll(".mermaid-frame[data-mi]");
    for (const host of hosts) {
      const src = blocks[Number(host.dataset.mi)] ?? "";
      const id = "m" + Math.random().toString(36).slice(2);
      try {
        if (!window.mermaid) throw new Error("mermaid not loaded");
        const { svg } = await mermaid.render(id, src);
        host.innerHTML = svg;
      } catch (e) {
        host.innerHTML = `<div class="diagram-error"><b>Mermaid error:</b> ${escapeHtml(e?.message || e)}<pre>${escapeHtml(src)}</pre></div>`;
      }
    }
  }

  // ── Element builders ───────────────────────────────────────────────────────
  function userMsg(content, images) {
    const el = document.createElement("div"); el.className = "msg user";
    if (images?.length) {
      const strip = document.createElement("div"); strip.style.cssText = "display:flex;gap:6px;flex-wrap:wrap;margin-bottom:6px";
      images.forEach(src => { const im = document.createElement("img"); im.src = src;
        im.style.cssText = "max-width:160px;max-height:160px;border-radius:6px"; strip.appendChild(im); });
      el.appendChild(strip);
    }
    const body = document.createElement("div"); body.className = "body plain"; body.textContent = content || "";
    el.appendChild(body);
    return el;
  }

  function assistantShell() {
    const el = document.createElement("div"); el.className = "msg assistant";
    el.innerHTML = '<div class="role">assistant</div>';
    const reasoning = document.createElement("details"); reasoning.className = "reasoning"; reasoning.hidden = true;
    reasoning.innerHTML = '<summary>reasoning</summary><div class="rbody"></div>';
    const body = document.createElement("div"); body.className = "body";
    el.appendChild(reasoning); el.appendChild(body);
    return {
      el,
      renderInto: async text => { await renderMarkdown(body, text); },
      setText: text => { body.textContent = text; },
      setReasoning: text => {
        const has = text && text.trim().length;
        reasoning.hidden = !has;
        reasoning.querySelector(".rbody").textContent = text || "";
      },
      appendReasoning: chunk => {
        reasoning.hidden = false;
        const rb = reasoning.querySelector(".rbody"); rb.textContent += chunk;
      }
    };
  }

  function toolLine(text) { const d = document.createElement("div"); d.className = "tool"; d.textContent = text; return d; }
  function notice(text) { const d = document.createElement("div"); d.className = "notice"; d.textContent = text; return d; }
  function system(text) { const d = document.createElement("div"); d.className = "msg system"; d.textContent = text; return d; }

  function permission(reqId, p, onDecide) {
    const el = document.createElement("div"); el.className = "ask";
    el.innerHTML = `<div class="q">Allow tool: <b>${escapeHtml(p.toolName)}</b>?</div>` +
      (p.arguments ? `<pre>${escapeHtml(p.arguments)}</pre>` : "") + `<div class="opts"></div>`;
    const opts = el.querySelector(".opts");
    [["Allow once", "allowOnce", "btn"], ["Allow & remember", "allowRemember", "btn"], ["Deny", "deny", "btn ghost"]]
      .forEach(([label, dec, cls]) => {
        const b = document.createElement("button"); b.className = cls; b.textContent = label;
        b.onclick = () => { onDecide(reqId, dec); el.remove(); }; opts.appendChild(b);
      });
    return el;
  }

  function clarify(reqId, p, onChoose) {
    const el = document.createElement("div"); el.className = "ask";
    el.innerHTML = `<div class="q">${escapeHtml(p.question)}</div><div class="opts"></div>`;
    const opts = el.querySelector(".opts");
    (p.options || []).forEach(o => {
      const b = document.createElement("button"); b.className = "btn"; b.textContent = o.label;
      if (o.description) b.title = o.description;
      b.onclick = () => { onChoose(reqId, o.label); el.remove(); }; opts.appendChild(b);
    });
    const skip = document.createElement("button"); skip.className = "btn ghost"; skip.textContent = "Skip";
    skip.onclick = () => { onChoose(reqId, null); el.remove(); }; opts.appendChild(skip);
    return el;
  }

  return { escapeHtml, renderMarkdown, reinitMermaidTheme,
           userMsg, assistantShell, toolLine, notice, system, permission, clarify };
})();
window.SplaRenderers = SplaRenderers;
