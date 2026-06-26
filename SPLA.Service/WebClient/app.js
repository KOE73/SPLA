/*
  SPLA web client — protocol orchestrator. Owns the WebSocket, app state, and all wiring; delegates
  every DOM build to SplaRenderers. Talks the same envelope protocol as any client.
*/
(() => {
  const R = window.SplaRenderers;
  const $ = s => document.querySelector(s);
  const log = $("#log"), chatsEl = $("#chats");
  const LS = window.localStorage;

  let ws, currentChat = null;
  const bubbles = {};            // msgIndex → { shell, raw, timer }
  let attachments = [];          // pending image data URLs

  // ── Theme (browser-local, decoupled from the project/EXE theme) ────────────
  function applyTheme(name) {
    document.documentElement.setAttribute("data-theme", name);
    $("#themeSel").value = name;
    LS.setItem("spla.theme", name);
    R.reinitMermaidTheme();
  }
  applyTheme(LS.getItem("spla.theme") || "dark");
  $("#themeSel").onchange = () => applyTheme($("#themeSel").value);

  // ── Per-type visibility toggles (persisted) ────────────────────────────────
  function loadVisibility() {
    const hidden = JSON.parse(LS.getItem("spla.hidden") || "[]");
    document.querySelectorAll(".filter[data-kind]").forEach(btn => {
      const k = btn.dataset.kind, off = hidden.includes(k);
      btn.classList.toggle("on", !off);
      document.body.classList.toggle("hide-" + k, off);
    });
  }
  document.querySelectorAll(".filter[data-kind]").forEach(btn => {
    btn.onclick = () => {
      const k = btn.dataset.kind, willHide = btn.classList.contains("on");
      btn.classList.toggle("on", !willHide);
      document.body.classList.toggle("hide-" + k, willHide);
      const hidden = JSON.parse(LS.getItem("spla.hidden") || "[]").filter(x => x !== k);
      if (willHide) hidden.push(k);
      LS.setItem("spla.hidden", JSON.stringify(hidden));
    };
  });
  loadVisibility();

  // ── WebSocket ──────────────────────────────────────────────────────────────
  function connect() {
    ws = new WebSocket((location.protocol === "https:" ? "wss://" : "ws://") + location.host + "/ws");
    ws.onopen = () => send("hello", { clientName: "web", protocolVersion: "1" });
    ws.onclose = () => { setConn(false, "disconnected — retrying"); setTimeout(connect, 1500); };
    ws.onmessage = ev => handle(JSON.parse(ev.data));
  }
  function send(type, payload, extra) { ws.send(JSON.stringify(Object.assign({ type, payload }, extra))); }
  function setConn(on, text) {
    $("#dot").className = on ? "on" : ""; $("#conn").textContent = text;
    const ready = on && currentChat;
    $("#input").disabled = !ready; $("#send").disabled = !ready;
  }
  const scroll = () => { log.scrollTop = log.scrollHeight; };

  function handle(env) {
    const p = env.payload || {};
    switch (env.type) {
      case "welcome":
        setConn(true, "connected");
        $("#project").textContent = p.projectName || p.workspacePath || "";
        fillSelect("#modeSel", (p.modes || []).map(m => [m, m]), p.defaultMode);
        fillSelect("#connSel", (p.connections || []).map(c => [c.id, c.name]), null);
        send("chat.list");
        break;
      case "chat.list.result": renderChatList(p.chats); break;
      case "chat.opened": openChat(p); break;
      case "llm.turn.start": startBubble(p.msgIndex); break;
      case "delta": appendDelta(p.msgIndex, p.text); break;
      case "reasoning": appendReasoning(p.msgIndex, p.text); break;
      case "assistant.message": finalizeBubble(p.msgIndex, p.message); break;
      case "tool.started": add(R.toolLine("→ " + p.toolCall.name)); break;
      case "tool.result": add(R.toolLine("← " + p.toolName + " (" + (p.result || "").length + " chars)")); break;
      case "notice": add(R.notice(p.text)); break;
      case "token.usage": showTokens(p); break;
      case "permission.request": add(R.permission(env.requestId, p, decide)); break;
      case "clarify.request": add(R.clarify(env.requestId, p, choose)); break;
      case "turn.complete": turnDone(); break;
      case "debug.snapshot": renderDebug(p); break;
      case "error": add(R.notice("⚠ " + p.message)); break;
    }
  }

  // ── Chat list ───────────────────────────────────────────────────────────────
  function renderChatList(chats) {
    chatsEl.innerHTML = "";
    for (const c of chats) {
      const d = document.createElement("div");
      d.className = "chat-item" + (c.id === currentChat ? " active" : "");
      d.dataset.id = c.id;
      const t = document.createElement("span"); t.className = "t"; t.textContent = c.title || c.id;
      t.onclick = () => send("chat.open", { chatId: c.id });
      const ren = document.createElement("span"); ren.className = "x"; ren.textContent = "✎"; ren.title = "Rename";
      ren.onclick = e => { e.stopPropagation(); const nt = prompt("Rename chat", c.title || ""); if (nt) send("chat.rename", { chatId: c.id, title: nt }); };
      const del = document.createElement("span"); del.className = "x"; del.textContent = "✕"; del.title = "Delete";
      del.onclick = e => { e.stopPropagation(); if (confirm("Delete this chat?")) { send("chat.delete", { chatId: c.id }); if (c.id === currentChat) { currentChat = null; log.innerHTML = ""; } } };
      d.append(t, ren, del);
      chatsEl.appendChild(d);
    }
  }

  function openChat(p) {
    currentChat = p.chatId;
    for (const k in bubbles) delete bubbles[k];
    log.innerHTML = "";
    for (const m of p.messages) {
      if (m.role === "user") add(R.userMsg(m.content, m.images));
      else if (m.role === "assistant") { const s = R.assistantShell(); add(s.el); s.setReasoning(m.reasoning); s.renderInto(m.content || ""); }
      else if (m.role === "tool") add(R.toolLine("← tool result (" + (m.content || "").length + " chars)"));
    }
    [...chatsEl.children].forEach(d => d.classList.toggle("active", d.dataset.id === p.chatId));
    if (p.mode) $("#modeSel").value = p.mode;
    $("#connSel").value = p.connectionId || "";
    $("#modeSel").disabled = false; $("#connSel").disabled = false;
    setConn(true, "connected");
    scroll();
  }

  // ── Streaming bubbles ─────────────────────────────────────────────────────
  function add(el) { log.appendChild(el); scroll(); return el; }
  function startBubble(i) { const shell = R.assistantShell(); add(shell.el); bubbles[i] = { shell, raw: "", timer: 0 }; }
  function appendDelta(i, t) {
    const b = bubbles[i]; if (!b) return;
    b.raw += t;
    clearTimeout(b.timer);
    b.timer = setTimeout(() => { b.shell.renderInto(b.raw).then(scroll); }, 70);
  }
  function appendReasoning(i, t) { bubbles[i]?.shell.appendReasoning(t); scroll(); }
  function finalizeBubble(i, msg) {
    const b = bubbles[i] || { shell: (() => { const s = R.assistantShell(); add(s.el); return s; })() };
    clearTimeout(b.timer);
    if (msg.reasoning) b.shell.setReasoning(msg.reasoning);
    b.shell.renderInto(msg.content || "").then(scroll);
  }

  function showTokens(p) {
    if (p.promptTokens != null || p.completionTokens != null)
      $("#tokens").textContent = "tokens in:" + (p.promptTokens ?? "?") + " out:" + (p.completionTokens ?? "?");
  }

  // ── Permission / clarify replies ──────────────────────────────────────────
  const decide = (reqId, decision) => send("permission.decision", { decision }, { requestId: reqId });
  const choose = (reqId, choice) => send("clarify.choice", { choice }, { requestId: reqId });

  // ── Sending ────────────────────────────────────────────────────────────────
  function sendMessage() {
    const t = $("#input").value.trim();
    if ((!t && !attachments.length) || !currentChat) return;
    add(R.userMsg(t, attachments));
    $("#input").value = "";
    setTurnActive(true);
    send("chat.send", { chatId: currentChat, text: t, images: attachments });
    attachments = []; renderAttachments();
  }
  function setTurnActive(on) {
    $("#input").disabled = on; $("#send").disabled = on;
    $("#send").hidden = on; $("#stop").hidden = !on;
    if (!on) $("#input").focus();
  }
  function turnDone() { setTurnActive(false); }

  $("#send").onclick = sendMessage;
  $("#stop").onclick = () => { if (currentChat) send("cancel", null, { chatId: currentChat }); };
  $("#input").addEventListener("keydown", e => { if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); sendMessage(); } });
  $("#new").onclick = () => send("chat.new", { title: null });

  // ── Settings selectors ────────────────────────────────────────────────────
  function fillSelect(sel, pairs, value) {
    const el = $(sel); el.innerHTML = "";
    for (const [v, label] of pairs) { const o = document.createElement("option"); o.value = v; o.textContent = label; el.appendChild(o); }
    if (value != null) el.value = value;
  }
  $("#modeSel").onchange = () => { if (currentChat) send("chat.settings", { chatId: currentChat, mode: $("#modeSel").value }); };
  $("#connSel").onchange = () => { if (currentChat) send("chat.settings", { chatId: currentChat, connectionId: $("#connSel").value }); };

  // ── Image attach / paste ──────────────────────────────────────────────────
  function addImageFiles(files) {
    for (const f of files) {
      if (!f.type.startsWith("image/")) continue;
      const r = new FileReader();
      r.onload = () => { attachments.push(r.result); renderAttachments(); };
      r.readAsDataURL(f);
    }
  }
  function renderAttachments() {
    const box = $("#attachments"); box.innerHTML = "";
    attachments.forEach((src, i) => {
      const t = document.createElement("div"); t.className = "thumb";
      t.innerHTML = `<img src="${src}"><button class="rm">✕</button>`;
      t.querySelector(".rm").onclick = () => { attachments.splice(i, 1); renderAttachments(); };
      box.appendChild(t);
    });
  }
  $("#attachBtn").onclick = () => $("#fileInput").click();
  $("#fileInput").onchange = e => { addImageFiles(e.target.files); e.target.value = ""; };
  $("#input").addEventListener("paste", e => {
    const imgs = [...(e.clipboardData?.items || [])].filter(i => i.type.startsWith("image/")).map(i => i.getAsFile());
    if (imgs.length) { e.preventDefault(); addImageFiles(imgs); }
  });

  // ── Debug drawer ───────────────────────────────────────────────────────────
  $("#debugBtn").onclick = () => { $("#debug").classList.add("open"); requestDebug("kv.session"); };
  $("#debugClose").onclick = () => $("#debug").classList.remove("open");
  document.querySelectorAll("#debug .tab").forEach(tab => {
    tab.onclick = () => { document.querySelectorAll("#debug .tab").forEach(t => t.classList.remove("on")); tab.classList.add("on"); requestDebug(tab.dataset.kind); };
  });
  function requestDebug(kind) {
    document.querySelectorAll("#debug .tab").forEach(t => t.classList.toggle("on", t.dataset.kind === kind));
    send("debug.request", { kind }, currentChat ? { chatId: currentChat } : undefined);
  }
  function renderDebug(p) {
    const body = $("#debugBody"); body.innerHTML = "";
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
  }

  connect();
})();
