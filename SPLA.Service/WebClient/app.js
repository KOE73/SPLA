/*
  SPLA web client — bootstrap + surfaces.

  Owns the WebSocket and shared state, then fans every protocol message out onto a bus. Each UI piece
  is a *surface* (see surfaces.js): host-agnostic, talks only through its context (send/sub/emit/
  state/slot). No surface touches the socket or globals directly. A layout (layouts.js) decides which
  surface lands in which slot — that's the placement axis, independent of colour/size.

  Each surface renders straight into its slot element (the slot keeps the id/class the CSS targets),
  so the existing stylesheet keeps working unchanged.
*/
(() => {
  const R = window.SplaRenderers;
  const LS = window.localStorage;
  const $ = (s, r = document) => r.querySelector(s);

  const bus = Spla.makeBus();
  const state = { currentChat: null, bubbles: {}, attachments: [], connected: false };
  // A single-surface window (e.g. a tear-off debug panel) follows the main window's active chat
  // instead of driving its own — see focus.set / focus.changed below.
  const isSolo = !!new URLSearchParams(location.search).get("surface");

  // ── WebSocket ────────────────────────────────────────────────────────────────
  let ws;
  function send(type, payload, extra) {
    if (ws && ws.readyState === WebSocket.OPEN) ws.send(JSON.stringify(Object.assign({ type, payload }, extra)));
  }
  function connect() {
    ws = new WebSocket((location.protocol === "https:" ? "wss://" : "ws://") + location.host + "/ws");
    ws.onopen = () => send("hello", { clientName: "web", protocolVersion: "1" });
    ws.onclose = () => { setConn(false, "disconnected — retrying"); setTimeout(connect, 1500); };
    ws.onmessage = ev => {
      const env = JSON.parse(ev.data);
      if (env.type === "welcome") setConn(true, "connected");
      if (env.type === "chat.opened") {
        state.currentChat = env.payload.chatId;
        bus.emit("chat.current", env.payload);
        if (!isSolo) send("focus.set", { chatId: env.payload.chatId });   // tell tear-off windows
      }
      if (env.type === "focus.changed") {
        // The main window switched chats — follow it (used by solo surfaces with no chat list).
        state.currentChat = env.payload.chatId;
        bus.emit("focus.changed", env.payload);
      }
      bus.emit(env.type, env.payload || {}, env);
    };
  }
  function setConn(on, text) { state.connected = on; bus.emit("conn", { on, text }); }

  const ctx = { send, state, on: bus.on, emit: bus.emit };

  // ╔═══════════════════════════════════════════════════════════════════════════╗
  // ║ Surfaces — each renders into c.slot, keeping the ids/classes CSS expects     ║
  // ╚═══════════════════════════════════════════════════════════════════════════╝

  // ── Chat list (slot = #sidebar) ──────────────────────────────────────────────
  Spla.registerSurface("chatList", c => {
    c.slot.innerHTML = `<header><b>SPLA</b><button id="new" class="btn">+ New</button></header><div id="chats"></div>`;
    const chatsEl = $("#chats", c.slot);
    $("#new", c.slot).onclick = () => c.send("chat.new", { title: null });

    function render(chats) {
      chatsEl.innerHTML = "";
      for (const chat of chats) {
        const d = document.createElement("div");
        d.className = "chat-item" + (chat.id === c.state.currentChat ? " active" : "");
        d.dataset.id = chat.id;
        const t = document.createElement("span"); t.className = "t"; t.textContent = chat.title || chat.id;
        t.onclick = () => c.send("chat.open", { chatId: chat.id });
        const ren = document.createElement("span"); ren.className = "x"; ren.textContent = "✎"; ren.title = "Rename";
        ren.onclick = e => { e.stopPropagation(); const nt = prompt("Rename chat", chat.title || ""); if (nt) c.send("chat.rename", { chatId: chat.id, title: nt }); };
        const del = document.createElement("span"); del.className = "x"; del.textContent = "✕"; del.title = "Delete";
        del.onclick = e => { e.stopPropagation(); if (confirm("Delete this chat?")) { c.send("chat.delete", { chatId: chat.id }); if (chat.id === c.state.currentChat) { c.state.currentChat = null; c.emit("chat.cleared"); } } };
        d.append(t, ren, del);
        chatsEl.appendChild(d);
      }
    }
    c.sub("chat.list.result", p => render(p.chats || []));
    c.sub("chat.current", p => [...chatsEl.children].forEach(d => d.classList.toggle("active", d.dataset.id === p.chatId)));
    return { el: null };
  });

  // ── Status bar (slot = #status) ──────────────────────────────────────────────
  Spla.registerSurface("statusBar", c => {
    c.slot.innerHTML = `
      <span><span id="dot">●</span> <span id="conn">connecting…</span></span>
      <span id="project"></span>
      <label>mode <select id="modeSel" disabled></select></label>
      <label>model <select id="connSel" disabled></select></label>
      <label>theme <select id="themeSel">
        <option value="dark">Dark</option><option value="emerald">Emerald</option>
        <option value="cream">Cream</option><option value="light">Light</option></select></label>
      <label>layout <select id="layoutSel"></select></label>
      <button id="debugBtn" class="filter">debug</button>
      <span id="tokens"></span>`;
    const modeSel = $("#modeSel", c.slot), connSel = $("#connSel", c.slot),
          themeSel = $("#themeSel", c.slot), layoutSel = $("#layoutSel", c.slot);

    function applyTheme(name) {
      document.documentElement.setAttribute("data-theme", name);
      themeSel.value = name; LS.setItem("spla.theme", name); R.reinitMermaidTheme();
    }
    applyTheme(LS.getItem("spla.theme") || "dark");
    themeSel.onchange = () => applyTheme(themeSel.value);

    Spla.layoutNames().forEach(n => {
      const o = document.createElement("option"); o.value = n;
      o.textContent = (Spla.layoutDefs[n].label || n); layoutSel.appendChild(o);
    });
    layoutSel.value = LS.getItem("spla.layout") || "default";
    layoutSel.onchange = () => { LS.setItem("spla.layout", layoutSel.value); c.emit("layout.request", { name: layoutSel.value }); };

    function fill(sel, pairs, value) {
      sel.innerHTML = "";
      for (const [v, label] of pairs) { const o = document.createElement("option"); o.value = v; o.textContent = label; sel.appendChild(o); }
      if (value != null) sel.value = value;
    }
    modeSel.onchange = () => { if (c.state.currentChat) c.send("chat.settings", { chatId: c.state.currentChat, mode: modeSel.value }); };
    connSel.onchange = () => { if (c.state.currentChat) c.send("chat.settings", { chatId: c.state.currentChat, connectionId: connSel.value }); };
    $("#debugBtn", c.slot).onclick = () => c.emit("debug.open");

    c.sub("conn", p => { $("#dot", c.slot).className = p.on ? "on" : ""; $("#conn", c.slot).textContent = p.text; });
    c.sub("welcome", p => {
      $("#project", c.slot).textContent = p.projectName || p.workspacePath || "";
      fill(modeSel, (p.modes || []).map(m => [m, m]), p.defaultMode);
      fill(connSel, (p.connections || []).map(x => [x.id, x.name]), null);
      c.send("chat.list");
    });
    c.sub("chat.current", p => {
      if (p.mode) modeSel.value = p.mode;
      connSel.value = p.connectionId || "";
      modeSel.disabled = false; connSel.disabled = false;
    });
    c.sub("token.usage", p => {
      if (p.promptTokens != null || p.completionTokens != null)
        $("#tokens", c.slot).textContent = "tokens in:" + (p.promptTokens ?? "?") + " out:" + (p.completionTokens ?? "?");
    });
    c.sub("connections.result", p => {
      // Live refresh after the connections editor saves — keep the current selection if still present.
      const keep = connSel.value;
      fill(connSel, (p.connections || []).map(x => [x.id, x.name || x.model || x.id]), null);
      if ([...connSel.options].some(o => o.value === keep)) connSel.value = keep;
    });
    return { el: null };
  });

  // ── Per-type visibility filters (slot = #filters) ────────────────────────────
  Spla.registerSurface("filters", c => {
    c.slot.innerHTML = `<span>show:</span>
      <button class="filter" data-kind="user">me</button>
      <button class="filter" data-kind="assistant">assistant</button>
      <button class="filter" data-kind="tool">tools</button>
      <button class="filter" data-kind="reasoning">reasoning</button>`;
    const hidden = () => JSON.parse(LS.getItem("spla.hidden") || "[]");
    c.slot.querySelectorAll(".filter[data-kind]").forEach(btn => {
      const k = btn.dataset.kind, off = hidden().includes(k);
      btn.classList.toggle("on", !off);
      document.body.classList.toggle("hide-" + k, off);
      btn.onclick = () => {
        const willHide = btn.classList.contains("on");
        btn.classList.toggle("on", !willHide);
        document.body.classList.toggle("hide-" + k, willHide);
        const h = hidden().filter(x => x !== k); if (willHide) h.push(k);
        LS.setItem("spla.hidden", JSON.stringify(h));
      };
    });
    return { el: null };
  });

  // ── Chat log (slot = #log; the slot IS the scroll container) ─────────────────
  Spla.registerSurface("chatLog", c => {
    const log = c.slot;
    const scroll = () => { log.scrollTop = log.scrollHeight; };
    const add = el => { log.appendChild(el); scroll(); return el; };
    const B = c.state.bubbles;

    function openChat(p) {
      for (const k in B) delete B[k];
      log.innerHTML = "";
      for (const m of p.messages) {
        if (m.role === "user") add(R.userMsg(m.content, m.images));
        else if (m.role === "assistant") { const s = R.assistantShell(); add(s.el); s.setReasoning(m.reasoning); s.renderInto(m.content || ""); }
        else if (m.role === "tool") add(R.toolLine("← tool result (" + (m.content || "").length + " chars)"));
      }
      scroll();
    }
    function startBubble(i) { const shell = R.assistantShell(); add(shell.el); B[i] = { shell, raw: "", timer: 0 }; }
    function appendDelta(i, t) {
      const b = B[i]; if (!b) return;
      b.raw += t; clearTimeout(b.timer);
      b.timer = setTimeout(() => { b.shell.renderInto(b.raw).then(scroll); }, 70);
    }
    function finalizeBubble(i, msg) {
      const b = B[i] || { shell: (() => { const s = R.assistantShell(); add(s.el); return s; })() };
      clearTimeout(b.timer);
      if (msg.reasoning) b.shell.setReasoning(msg.reasoning);
      b.shell.renderInto(msg.content || "").then(scroll);
    }

    c.sub("chat.opened", openChat);
    c.sub("chat.cleared", () => { log.innerHTML = ""; for (const k in B) delete B[k]; });
    c.sub("local.userMsg", p => add(R.userMsg(p.text, p.images)));
    c.sub("llm.turn.start", p => startBubble(p.msgIndex));
    c.sub("delta", p => appendDelta(p.msgIndex, p.text));
    c.sub("reasoning", p => { B[p.msgIndex]?.shell.appendReasoning(p.text); scroll(); });
    c.sub("assistant.message", p => finalizeBubble(p.msgIndex, p.message));
    c.sub("tool.started", p => add(R.toolLine("→ " + p.toolCall.name)));
    c.sub("tool.result", p => add(R.toolLine("← " + p.toolName + " (" + (p.result || "").length + " chars)")));
    c.sub("notice", p => add(R.notice(p.text)));
    c.sub("error", p => add(R.notice("⚠ " + p.message)));
    c.sub("permission.request", (p, env) => add(R.permission(env.requestId, p, (id, dec) => c.send("permission.decision", { decision: dec }, { requestId: id }))));
    c.sub("clarify.request", (p, env) => add(R.clarify(env.requestId, p, (id, ch) => c.send("clarify.choice", { choice: ch }, { requestId: id }))));
    return { el: null };
  });

  // ── Composer (slot = #composer) ──────────────────────────────────────────────
  Spla.registerSurface("composer", c => {
    c.slot.innerHTML = `
      <div id="attachments"></div>
      <div class="row">
        <button id="attachBtn" class="icon-btn" title="Attach image">+</button>
        <input id="fileInput" type="file" accept="image/*" multiple hidden>
        <textarea id="input" rows="2" placeholder="Message…  (Enter to send, Shift+Enter for newline, paste images)" disabled></textarea>
        <button id="send" class="btn" disabled>Send</button>
        <button id="stop" class="btn danger" hidden>Stop</button>
      </div>`;
    const input = $("#input", c.slot), sendBtn = $("#send", c.slot), stopBtn = $("#stop", c.slot),
          attBox = $("#attachments", c.slot), fileInput = $("#fileInput", c.slot);

    const ready = () => c.state.connected && c.state.currentChat;
    function refresh() { const r = ready(); input.disabled = !r; sendBtn.disabled = !r; }
    function setTurnActive(on) {
      input.disabled = on; sendBtn.disabled = on; sendBtn.hidden = on; stopBtn.hidden = !on;
      if (!on) { refresh(); input.focus(); }
    }
    function renderAttachments() {
      attBox.innerHTML = "";
      c.state.attachments.forEach((src, i) => {
        const t = document.createElement("div"); t.className = "thumb";
        t.innerHTML = `<img src="${src}"><button class="rm">✕</button>`;
        t.querySelector(".rm").onclick = () => { c.state.attachments.splice(i, 1); renderAttachments(); };
        attBox.appendChild(t);
      });
    }
    function addImageFiles(files) {
      for (const f of files) {
        if (!f.type.startsWith("image/")) continue;
        const r = new FileReader();
        r.onload = () => { c.state.attachments.push(r.result); renderAttachments(); };
        r.readAsDataURL(f);
      }
    }
    function sendMessage() {
      const t = input.value.trim();
      if ((!t && !c.state.attachments.length) || !c.state.currentChat) return;
      const imgs = c.state.attachments.slice();
      c.emit("local.userMsg", { text: t, images: imgs });
      input.value = ""; setTurnActive(true);
      c.send("chat.send", { chatId: c.state.currentChat, text: t, images: imgs });
      c.state.attachments = []; renderAttachments();
    }

    sendBtn.onclick = sendMessage;
    stopBtn.onclick = () => { if (c.state.currentChat) c.send("cancel", null, { chatId: c.state.currentChat }); };
    input.addEventListener("keydown", e => { if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); sendMessage(); } });
    $("#attachBtn", c.slot).onclick = () => fileInput.click();
    fileInput.onchange = e => { addImageFiles(e.target.files); e.target.value = ""; };
    input.addEventListener("paste", e => {
      const imgs = [...(e.clipboardData?.items || [])].filter(i => i.type.startsWith("image/")).map(i => i.getAsFile());
      if (imgs.length) { e.preventDefault(); addImageFiles(imgs); }
    });

    c.sub("conn", refresh);
    c.sub("chat.current", refresh);
    c.sub("chat.cleared", refresh);
    c.sub("turn.complete", () => setTurnActive(false));
    renderAttachments(); refresh();
    return { el: null };
  });

  // ── Debug drawer (slot = #debug; CSS targets #debug.open) ────────────────────
  Spla.registerSurface("debug", c => {
    const slot = c.slot;
    slot.classList.add("debug-surface");                 // inner styling is layout-independent
    const solo = slot.id !== "debug";                    // mounted alone (e.g. a tear-off window)?
    slot.innerHTML = `
      <header><b>Debug</b><button id="debugClose" class="filter">close</button></header>
      <div class="tabs">
        <button class="tab" data-kind="kv.session">session kv</button>
        <button class="tab" data-kind="kv.project">project kv</button>
        <button class="tab" data-kind="blobs">blobs</button>
        <button class="tab" data-kind="context.last">context</button>
        <button class="tab" data-kind="prompt">prompt</button>
      </div>
      <div id="debugBody"></div>`;
    const body = $("#debugBody", slot);
    function request(kind) {
      slot.querySelectorAll(".tab").forEach(t => t.classList.toggle("on", t.dataset.kind === kind));
      c.send("debug.request", { kind }, c.state.currentChat ? { chatId: c.state.currentChat } : undefined);
    }
    $("#debugClose", slot).onclick = () => slot.classList.remove("open");
    slot.querySelectorAll(".tab").forEach(tab => tab.onclick = () => request(tab.dataset.kind));

    c.sub("debug.open", () => { slot.classList.add("open"); request("kv.session"); });
    if (solo) {
      // Standalone window: no drawer chrome, no debug button — show immediately and auto-load once
      // the socket is up (welcome). Re-request on chat switch so context-bound tabs stay live.
      $("#debugClose", slot).style.display = "none";
      const reload = () => request(slot.querySelector(".tab.on")?.dataset.kind || "kv.session");
      c.sub("welcome", reload);
      c.sub("focus.changed", reload);     // main window switched chats → refresh against the new one
      c.sub("chat.current", reload);
    }
    c.sub("debug.snapshot", p => {
      body.innerHTML = "";
      if (p.entries) {
        if (!p.entries.length) body.textContent = "(empty)";
        for (const e of p.entries) {
          const row = document.createElement("div"); row.className = "kv-row";
          row.innerHTML = `<span class="k">${R.escapeHtml(e.key)}</span><span class="v">${R.escapeHtml(e.value)}</span>`;
          body.appendChild(row);
        }
      } else if (p.text != null) {
        const pre = document.createElement("pre"); pre.style.whiteSpace = "pre-wrap"; pre.textContent = p.text; body.appendChild(pre);
      }
    });
    return { el: null };
  });

  // ── Connections editor (slot, or a tear-off window via ?surface=connections) ─
  Spla.registerSurface("connections", c => {
    const slot = c.slot;
    slot.classList.add("settings-surface");
    slot.innerHTML = `
      <header><b>Connections</b><span class="hint"></span></header>
      <div class="list"></div>
      <div class="bar">
        <button class="btn ghost add">+ Add connection</button>
        <span class="grow"></span>
        <button class="btn save">Save</button>
      </div>`;
    const listEl = $(".list", slot), hintEl = $(".hint", slot);
    let conns = [];

    const FIELDS = [["name", "Name"], ["provider", "Provider"], ["endpoint", "Endpoint"], ["model", "Model"], ["apiKey", "API key"]];

    function render() {
      listEl.innerHTML = "";
      conns.forEach((conn, i) => {
        const card = document.createElement("div"); card.className = "conn-card";
        const head = document.createElement("div"); head.className = "conn-head";
        head.innerHTML = `<span class="id">${R.escapeHtml(conn.id || "(new)")}</span>`;
        const rm = document.createElement("button"); rm.className = "x"; rm.textContent = "✕"; rm.title = "Remove";
        rm.onclick = () => { conns.splice(i, 1); render(); };
        head.appendChild(rm);
        card.appendChild(head);
        for (const [key, label] of FIELDS) {
          const row = document.createElement("label"); row.className = "field";
          row.innerHTML = `<span>${label}</span>`;
          const inp = document.createElement("input");
          inp.type = key === "apiKey" ? "password" : "text";
          inp.value = conn[key] || "";
          inp.oninput = () => { conn[key] = inp.value; if (key === "name" && !conn.id) head.querySelector(".id").textContent = "(new)"; };
          row.appendChild(inp); card.appendChild(row);
        }
        listEl.appendChild(card);
      });
    }

    $(".add", slot).onclick = () => { conns.push({ id: "", name: "", provider: "lmstudio", endpoint: "http://127.0.0.1:1234/v1/", model: "", apiKey: "" }); render(); };
    $(".save", slot).onclick = () => {
      c.send("connections.save", { connections: conns });
      $(".save", slot).textContent = "Saved ✓";
      setTimeout(() => { const b = $(".save", slot); if (b) b.textContent = "Save"; }, 1200);
    };

    c.sub("connections.result", p => {
      conns = (p.connections || []).map(x => ({ ...x }));
      hintEl.textContent = p.canPersist ? "" : "no .spla project — edits are session-only";
      render();
    });
    c.sub("welcome", () => c.send("connections.get"));
    c.send("connections.get");
    return { el: null };
  });

  // ╔═══════════════════════════════════════════════════════════════════════════╗
  // ║ Boot                                                                        ║
  // ╚═══════════════════════════════════════════════════════════════════════════╝
  const root = $("#root");
  const params = new URLSearchParams(location.search);

  function boot() {
    // Theme is global, not owned by any one surface — apply it before any layout so single-surface
    // windows (which have no status bar) are still themed.
    document.documentElement.setAttribute("data-theme", LS.getItem("spla.theme") || "dark");

    const solo = params.get("surface");
    if (solo) {
      Spla.layoutDefs.single.placement = { solo };
      Spla.applyLayout(root, "single", ctx);
    } else {
      Spla.applyLayout(root, LS.getItem("spla.layout") || "default", ctx);
    }
  }
  bus.on("layout.request", p => {
    Spla.applyLayout(root, p.name, ctx);
    // Surfaces in the fresh layout missed earlier one-shot messages — replay the essentials.
    if (state.connected) {
      bus.emit("conn", { on: true, text: "connected" });
      send("chat.list");
      if (state.currentChat) send("chat.open", { chatId: state.currentChat });
    }
  });

  boot();
  connect();
})();
