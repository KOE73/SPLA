using Avalonia.Platform;
using SPLA.UI.Avalonia.ViewModels.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace SPLA.UI.Avalonia.Services.Chat;

public sealed record WebChatTheme(
    string AppBackground,
    string PanelBackground,
    string PanelBorder,
    string TextForeground,
    string SubTextForeground,
    string Accent,
    string MessageBackground,
    string UserBubbleBackground,
    string AiBubbleBackground,
    string SystemBubbleBackground,
    string ToolBubbleBackground,
    string ErrorBubbleBackground,
    string StandardMargin,
    string StandardPadding,
    string CompactMargin,
    string StandardSpacing,
    string ChatBubbleMargin,
    string UserChatBubbleMargin,
    string AiChatBubbleMargin,
    string StandardCornerRadius,
    string HeaderFontSize,
    string BaseFontSize,
    string SmallFontSize);

public static class ChatHtmlRenderer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static long _renderSequence;

    public static RenderedWebChat RenderToFile(IEnumerable<MessageViewModel> messages, WebChatTheme theme, string? profileId = null)
    {
        EnsureAssetFiles();
        CleanupOldRuntimeFiles();

        var html = Render(messages, theme, profileId ?? "classic");
        var documentPath = Path.Combine(GetRuntimeDirectory(), $"chat-{Environment.ProcessId}-{Interlocked.Increment(ref _renderSequence)}.html");
        File.WriteAllText(documentPath, html, Encoding.UTF8);

        return new RenderedWebChat(documentPath, new Uri(documentPath));
    }

    public static string SerializeMessage(MessageViewModel message, int index)
    {
        var perm = message as PermissionMessageViewModel;
        var msg = new ChatHtmlMessage(
            index,
            message.Role.ToString(),
            message.Content,
            message.Reasoning ?? "",
            message.RetentionIcon,
            message.RetentionDescription,
            message.IsUser,
            message.IsAssistant,
            message.IsSystem,
            message.IsTool,
            message.IsError,
            message.IsToolCallNotice,
            message.IsPlainText,
            message.IsMarkdown,
            perm != null,
            perm?.IsAnswered ?? false,
            perm?.Arguments ?? "");
        return JsonSerializer.Serialize(msg, JsonOptions);
    }

    private static string Render(IEnumerable<MessageViewModel> messages, WebChatTheme theme, string profileId)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(theme);

        var payload = messages
            .Select((message, index) =>
            {
                var perm = message as PermissionMessageViewModel;
                return new ChatHtmlMessage(
                    index,
                    message.Role.ToString(),
                    message.Content,
                    message.Reasoning ?? "",
                    message.RetentionIcon,
                    message.RetentionDescription,
                    message.IsUser,
                    message.IsAssistant,
                    message.IsSystem,
                    message.IsTool,
                    message.IsError,
                    message.IsToolCallNotice,
                    message.IsPlainText,
                    message.IsMarkdown,
                    perm != null,
                    perm?.IsAnswered ?? false,
                    perm?.Arguments ?? "");
            })
            .Where(message => !string.IsNullOrWhiteSpace(message.Content) || !string.IsNullOrWhiteSpace(message.Reasoning) || message.IsPermission)
            .ToArray();

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        return $$"""
<!doctype html>
<html>
<head>
<meta charset="utf-8">
<meta http-equiv="Content-Security-Policy" content="default-src 'none'; img-src data:; style-src 'unsafe-inline'; script-src 'self' 'unsafe-inline' file:;">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<style>
:root {
  --app-bg: {{theme.AppBackground}};
  --panel-bg: {{theme.PanelBackground}};
  --panel-border: {{theme.PanelBorder}};
  --text: {{theme.TextForeground}};
  --muted: {{theme.SubTextForeground}};
  --accent: {{theme.Accent}};
  --message-bg: {{theme.MessageBackground}};
  --user-bg: {{theme.UserBubbleBackground}};
  --assistant-bg: {{theme.AiBubbleBackground}};
  --system-bg: {{theme.SystemBubbleBackground}};
  --tool-bg: {{theme.ToolBubbleBackground}};
  --error-bg: {{theme.ErrorBubbleBackground}};
  --standard-margin: {{theme.StandardMargin}};
  --standard-padding: {{theme.StandardPadding}};
  --compact-margin: {{theme.CompactMargin}};
  --standard-spacing: {{theme.StandardSpacing}};
  --chat-bubble-margin: {{theme.ChatBubbleMargin}};
  --user-chat-bubble-margin: {{theme.UserChatBubbleMargin}};
  --ai-chat-bubble-margin: {{theme.AiChatBubbleMargin}};
  --radius: {{theme.StandardCornerRadius}};
  --header-font-size: {{theme.HeaderFontSize}};
  --base-font-size: {{theme.BaseFontSize}};
  --small-font-size: {{theme.SmallFontSize}};
}

* { box-sizing: border-box; }
html, body { min-height: 100%; margin: 0; background: var(--app-bg); color: var(--text); }
body {
  font-family: "Segoe UI", "Noto Sans", system-ui, sans-serif;
  font-size: var(--base-font-size);
  line-height: 1.5;
}

#chat {
  display: flex;
  flex-direction: column;
  gap: var(--standard-spacing);
  padding: var(--standard-margin);
}

.message {
  position: relative;
  width: auto;
  margin: var(--chat-bubble-margin);
  border: 1px solid var(--panel-border);
  border-radius: var(--radius);
  background: var(--message-bg);
  overflow: hidden;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.04);
}

.message.user {
  margin: var(--user-chat-bubble-margin);
  background: var(--user-bg);
}
.message.assistant {
  margin: var(--ai-chat-bubble-margin);
  background: var(--assistant-bg);
}
.message.system { background: var(--system-bg); }
.message.tool { background: var(--tool-bg); }
.message.error { background: var(--error-bg); border-color: var(--accent); }
.message.tool-call { opacity: 0.88; }

.message-header {
  display: flex;
  align-items: center;
  gap: var(--standard-spacing);
  min-height: calc(var(--small-font-size) + 12px);
  padding: var(--compact-margin) var(--standard-padding) 0 var(--standard-padding);
}

.role {
  color: var(--accent);
  font-size: var(--small-font-size);
  font-weight: 700;
  letter-spacing: .04em;
  text-transform: uppercase;
}

.message.user .role,
.message.assistant .role {
  display: none;
}

.actions {
  position: absolute;
  top: 6px;
  right: 8px;
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--muted);
  user-select: none;
}

.action {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: calc(var(--small-font-size) + 12px);
  height: calc(var(--small-font-size) + 12px);
  border: 1px solid transparent;
  border-radius: 999px;
  background: transparent;
  color: var(--muted);
  cursor: pointer;
  font: inherit;
  font-size: calc(var(--small-font-size) + 1px);
  line-height: 1;
  padding: 0;
}

.action:hover {
  border-color: var(--panel-border);
  color: var(--text);
  background: rgba(127, 127, 127, .09);
}

.retention {
  display: none;
  align-items: center;
  justify-content: center;
  width: 22px;
  height: 22px;
  border-radius: 999px;
  background: rgba(127, 127, 127, .12);
  font-size: 12px;
}

.content {
  padding: var(--standard-padding);
  padding-right: max(96px, var(--standard-padding));
  overflow-wrap: anywhere;
}

/* ── Reasoning block ── */
.reasoning {
  margin: var(--compact-margin) var(--standard-padding) 0 var(--standard-padding);
  border: 1px dashed var(--panel-border);
  border-radius: var(--radius);
  background: rgba(127, 127, 127, .06);
}
.reasoning[hidden] { display: none; }
.reasoning > summary {
  cursor: pointer;
  user-select: none;
  padding: 6px 10px;
  font-size: var(--small-font-size);
  font-weight: 600;
  color: var(--muted);
  letter-spacing: .03em;
}
.reasoning > summary::marker { color: var(--muted); }
.reasoning .reasoning-body {
  padding: 0 12px 10px 12px;
  margin: 0;
  white-space: pre-wrap;
  font-size: var(--small-font-size);
  font-style: italic;
  color: var(--muted);
}

.plain {
  margin: 0;
  white-space: pre-wrap;
  font-family: inherit;
  color: var(--text);
}

.content h1, .content h2, .content h3, .content h4 {
  margin: 10px 0 8px 0;
  line-height: 1.2;
}

.content p { margin: 8px 0; }
.content ul, .content ol { margin: 8px 0; padding-left: 24px; }
.content li { margin: 4px 0; }
.content blockquote {
  margin: 10px 0;
  padding: 8px 12px;
  border-left: 4px solid var(--accent);
  background: rgba(127, 127, 127, .08);
}

.content table {
  width: 100%;
  border-collapse: collapse;
  margin: 12px 0;
  font-size: 14px;
}

.content th, .content td {
  border: 1px solid var(--panel-border);
  padding: 6px 8px;
  vertical-align: top;
}

.content th {
  background: rgba(127, 127, 127, .10);
  text-align: left;
}

.content a {
  color: var(--accent);
  text-decoration: none;
}

.content a:hover { text-decoration: underline; }

.content code {
  border: 1px solid var(--panel-border);
  border-radius: 5px;
  background: rgba(127, 127, 127, .11);
  padding: 1px 4px;
  font-family: "Cascadia Mono", "Consolas", monospace;
  font-size: 0.92em;
}

.code-frame {
  position: relative;
  margin: 12px 0;
}

.mermaid-frame {
  position: relative;
}

.block-copy {
  position: absolute;
  top: 6px;
  right: 6px;
  z-index: 1;
  border: 1px solid var(--panel-border);
  border-radius: calc(var(--radius) * .75);
  background: var(--panel-bg);
  color: var(--muted);
  cursor: pointer;
  font-size: var(--small-font-size);
  line-height: 1;
  padding: 5px 7px;
}

.block-copy:hover {
  color: var(--text);
  border-color: var(--accent);
}

.content pre {
  margin: 0;
  overflow: auto;
  border: 1px solid var(--panel-border);
  border-radius: var(--radius);
  background: rgba(0, 0, 0, .08);
  padding: var(--standard-padding);
}

.content pre code {
  border: 0;
  border-radius: 0;
  background: transparent;
  padding: 0;
  color: var(--text);
}

.mermaid-frame {
  margin: 14px 0;
  padding: var(--compact-margin);
  border: 1px solid var(--panel-border);
  border-radius: var(--radius);
  background: var(--panel-bg);
  overflow: auto;
}

.mermaid-frame svg {
  display: block;
  max-width: 100%;
  height: auto;
  margin: 0 auto;
}

.mermaid-frame .block-copy {
  opacity: .72;
}

.diagram-error {
  border: 1px solid var(--accent);
  border-radius: var(--radius);
  padding: var(--compact-margin);
  color: var(--text);
  background: rgba(225, 143, 47, .12);
}

.diagram-error pre {
  margin-top: var(--compact-margin);
}

/* ── Permission block ── */

.perm-block {
  padding: var(--standard-padding);
  padding-top: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.perm-args {
  margin: 0;
  padding: 8px 12px;
  border: 1px solid var(--panel-border);
  border-radius: var(--radius);
  background: rgba(0, 0, 0, .06);
  font-family: "Cascadia Mono", "Consolas", monospace;
  font-size: var(--small-font-size);
  color: var(--muted);
  white-space: pre-wrap;
  overflow-wrap: anywhere;
}

.perm-actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.perm-btn {
  padding: 6px 14px;
  border: none;
  border-radius: var(--radius);
  cursor: pointer;
  font: inherit;
  font-size: var(--small-font-size);
  font-weight: 600;
  color: #fff;
  transition: opacity .15s;
}
.perm-btn:hover { opacity: .85; }

.perm-allow-remember { background: #2E7D32; }
.perm-allow-once     { background: var(--accent); }
.perm-deny           { background: #C62828; }

.perm-decided {
  font-size: var(--small-font-size);
  font-style: italic;
  color: var(--muted);
}

/* ── Clarify widget ── */

#clarify-widget {
  display: none;
  position: sticky;
  bottom: 0;
  background: var(--panel-bg);
  border-top: 1px solid var(--accent);
  padding: 12px 16px 14px 16px;
}

#clarify-widget.visible { display: block; }

#clarify-question {
  font-size: var(--base-font-size);
  font-weight: 600;
  color: var(--text);
  margin-bottom: 10px;
}

#clarify-options {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 8px;
}

.clarify-btn {
  padding: 5px 14px;
  border: none;
  border-radius: var(--radius);
  background: var(--accent);
  color: #fff;
  font: inherit;
  font-size: var(--small-font-size);
  font-weight: 600;
  cursor: pointer;
  transition: opacity .15s;
}
.clarify-btn:hover { opacity: .82; }

#clarify-skip {
  background: transparent;
  border: none;
  color: var(--muted);
  font: inherit;
  font-size: var(--small-font-size);
  cursor: pointer;
  padding: 0;
  display: block;
}
#clarify-skip:hover { color: var(--text); }

/* ── Profile overrides ── */

/* Bubbles: margin-based offset, full width within those margins */
body.profile-bubbles .message.system { display: none; }
body.profile-bubbles .message.user {
  margin-left: 80px;
  margin-right: var(--compact-margin);
  border-radius: 18px 18px 4px 18px;
}
body.profile-bubbles .message.assistant,
body.profile-bubbles .message.tool-call {
  margin-left: var(--compact-margin);
  margin-right: 80px;
  border-radius: 18px 18px 18px 4px;
}

/* Diagnostic: show retention + metadata */
body.profile-diagnostic .retention { display: inline-flex; }
body.profile-diagnostic .meta { display: inline-flex; }
</style>
</head>
<body class="profile-{{profileId}}">
<main id="chat"></main>
<div id="clarify-widget">
  <div id="clarify-question"></div>
  <div id="clarify-options"></div>
  <button id="clarify-skip" data-action="clarify-dismiss">Skip / let agent decide</button>
</div>
<script src="../assets/marked.min.js"></script>
<script src="../assets/mermaid.min.js"></script>
<script>
const messages = JSON.parse(new TextDecoder().decode(Uint8Array.from(atob("{{payloadBase64}}"), c => c.charCodeAt(0))));

mermaid.initialize({
  startOnLoad: false,
  securityLevel: "strict",
  theme: "base",
  themeVariables: {
    background: getCss("--panel-bg"),
    mainBkg: getCss("--panel-bg"),
    primaryColor: getCss("--assistant-bg"),
    primaryBorderColor: getCss("--accent"),
    primaryTextColor: getCss("--text"),
    secondaryColor: getCss("--user-bg"),
    secondaryBorderColor: getCss("--panel-border"),
    secondaryTextColor: getCss("--text"),
    tertiaryColor: getCss("--system-bg"),
    tertiaryBorderColor: getCss("--panel-border"),
    tertiaryTextColor: getCss("--text"),
    lineColor: getCss("--accent"),
    textColor: getCss("--text"),
    fontFamily: "Segoe UI, Noto Sans, system-ui, sans-serif"
  }
});

function getCss(name) {
  return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
}

function escapeHtml(value) {
  return String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
}

function neutralizeRawHtml(value) {
  return String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;");
}

function postToHost(payload) {
  const message = JSON.stringify(payload);
  if (typeof window.invokeCSharpAction === "function") {
    window.invokeCSharpAction(message);
    return;
  }
  if (window.chrome?.webview?.postMessage) {
    window.chrome.webview.postMessage(message);
    return;
  }
  if (window.webkit?.messageHandlers?.spla?.postMessage) {
    window.webkit.messageHandlers.spla.postMessage(message);
    return;
  }
  console.info("SPLA host bridge is not available", payload);
}

function messageClasses(message) {
  const classes = ["message"];
  if (message.isUser) classes.push("user");
  if (message.isAssistant) classes.push("assistant");
  if (message.isSystem) classes.push("system");
  if (message.isTool) classes.push("tool");
  if (message.isError) classes.push("error");
  if (message.isToolCallNotice) classes.push("tool-call");
  if (message.isPermission) classes.push("permission");
  return classes.join(" ");
}

function buildShell(message) {
  const hasReasoning = message.reasoning && message.reasoning.trim().length > 0;
  return `
    <article class="${messageClasses(message)}" data-index="${message.index}">
      <header class="message-header">
        <div class="role">${escapeHtml(message.role)}</div>
        <div class="actions">
          <span class="retention" title="${escapeHtml(message.retentionDescription)}">${escapeHtml(message.retentionIcon)}</span>
          <button class="action" title="Delete message" aria-label="Delete message" data-action="delete" data-index="${message.index}">🗑</button>
          <button class="action" title="Copy message" aria-label="Copy message" data-action="copy" data-index="${message.index}">⧉</button>
        </div>
      </header>
      <details class="reasoning" data-reasoning="${message.index}" ${hasReasoning ? "open" : "hidden"}>
        <summary>💭 Reasoning</summary>
        <pre class="reasoning-body">${escapeHtml(message.reasoning ?? "")}</pre>
      </details>
      <section class="content" data-content="${message.index}"></section>
      ${message.isPermission ? buildPermBlock(message) : ""}
    </article>`;
}

function buildPermBlock(message) {
  if (message.isAnswered) {
    return `<div class="perm-block"><div class="perm-decided">Decision made.</div></div>`;
  }
  const args = message.permArguments && message.permArguments.trim()
    ? `<pre class="perm-args">${escapeHtml(message.permArguments)}</pre>`
    : "";
  return `
    <div class="perm-block">
      ${args}
      <div class="perm-actions">
        <button class="perm-btn perm-allow-remember" data-action="permission" data-index="${message.index}" data-decision="allowRemember">Allow &amp; remember</button>
        <button class="perm-btn perm-allow-once" data-action="permission" data-index="${message.index}" data-decision="allowOnce">Allow once</button>
        <button class="perm-btn perm-deny" data-action="permission" data-index="${message.index}" data-decision="deny">Deny</button>
      </div>
    </div>`;
}

function setReasoning(index, text) {
  const chat = document.getElementById("chat");
  const details = chat.querySelector(`[data-reasoning="${index}"]`);
  if (!details) return;
  const body = details.querySelector(".reasoning-body");
  const hasText = text && text.trim().length > 0;
  if (body) body.textContent = text ?? "";
  details.hidden = !hasText;
}

async function renderMarkdownInto(container, markdown, messageIndex) {
  const mermaidBlocks = [];
  const tokenPrefix = "@@SPLA_MERMAID_";
  const protectedMarkdown = String(markdown ?? "").replace(/```mermaid[^\n\r]*(?:\r?\n)([\s\S]*?)```/gi, (_, source) => {
    const index = mermaidBlocks.push(source.trim()) - 1;
    return `${tokenPrefix}${index}@@`;
  });
  let escapedMarkdown = neutralizeRawHtml(protectedMarkdown);
  escapedMarkdown = escapedMarkdown.replace(new RegExp(`${tokenPrefix}(\\d+)@@`, "g"), (_, index) =>
    `<div class="mermaid-frame" data-mermaid-index="${index}"></div>`);

  container.innerHTML = marked.parse(escapedMarkdown, { gfm: true, breaks: true });
  enhanceCodeBlocks(container, messageIndex);
  await renderMermaidBlocks(container, mermaidBlocks, messageIndex);
}

function enhanceCodeBlocks(container, messageIndex) {
  container.querySelectorAll("pre").forEach((pre, codeIndex) => {
    if (pre.parentElement?.classList.contains("code-frame")) return;

    const frame = document.createElement("div");
    frame.className = "code-frame";
    const button = document.createElement("button");
    button.className = "block-copy";
    button.title = "Copy code";
    button.textContent = "⧉";
    button.dataset.action = "copy-code";
    button.dataset.index = String(messageIndex);
    button.dataset.codeIndex = String(codeIndex);

    pre.parentNode.insertBefore(frame, pre);
    frame.appendChild(pre);
    frame.appendChild(button);
  });
}

async function renderMermaidBlocks(container, blocks, messageIndex) {
  const hosts = container.querySelectorAll(".mermaid-frame[data-mermaid-index]");
  for (const host of hosts) {
    const blockIndex = Number(host.dataset.mermaidIndex);
    const source = blocks[blockIndex] ?? "";
    const renderId = `spla-mermaid-${messageIndex}-${blockIndex}-${Date.now()}`;
    const scratch = document.createElement("div");
    scratch.style.position = "absolute";
    scratch.style.left = "-100000px";
    scratch.style.top = "0";
    scratch.style.width = "1px";
    scratch.style.height = "1px";
    scratch.style.overflow = "hidden";
    document.body.appendChild(scratch);
    try {
      await mermaid.parse(source);
      const result = await mermaid.render(renderId, source, scratch);
      host.innerHTML = `<button class="block-copy" title="Copy Mermaid source" data-action="copyText" data-index="${messageIndex}">⧉</button>${result.svg}`;
      host.querySelector(".block-copy").dataset.text = source;
    } catch (error) {
      host.innerHTML = `
        <div class="diagram-error">
          <button class="block-copy" title="Copy Mermaid source" data-action="copyText" data-index="${messageIndex}" data-text="${escapeHtml(source)}">⧉</button>
          <strong>Mermaid render error:</strong> ${escapeHtml(error?.message ?? error)}
          <pre><code>${escapeHtml(source)}</code></pre>
        </div>`;
    } finally {
      scratch.remove();
      cleanupMermaidArtifacts(renderId);
    }
  }
}

function cleanupMermaidArtifacts(renderId) {
  document.querySelectorAll(`[id="${renderId}"], [id^="d${renderId}"]`).forEach(element => element.remove());
  document.body.querySelectorAll(":scope > .mermaid, :scope > svg, :scope > div[id^='dmermaid']").forEach(element => {
    if (element.id !== "chat") element.remove();
  });
}

async function renderMessages() {
  const chat = document.getElementById("chat");
  chat.innerHTML = messages.map(buildShell).join("");

  for (const message of messages) {
    const container = chat.querySelector(`[data-content="${message.index}"]`);
    if (!container) continue;
    if (message.isMarkdown) {
      await renderMarkdownInto(container, message.content, message.index);
    } else {
      container.innerHTML = `<pre class="plain">${escapeHtml(message.content)}</pre>`;
    }
  }

  window.scrollTo(0, document.body.scrollHeight);
}

document.addEventListener("click", event => {
  const button = event.target.closest("[data-action]");
  if (!button) return;

  const action = button.dataset.action;
  const index = Number(button.dataset.index);
  if (action === "copy-code") {
    const frame = button.closest(".code-frame");
    const text = frame?.querySelector("pre")?.innerText ?? "";
    postToHost({ action: "copyText", index, text });
    return;
  }
  if (action === "copyText") {
    postToHost({ action: "copyText", index, text: button.dataset.text ?? "" });
    return;
  }
  if (action === "permission") {
    postToHost({ action: "permission", index, decision: button.dataset.decision ?? "" });
    return;
  }
  if (action === "clarify-choose") {
    postToHost({ action: "clarify", choice: button.dataset.choice ?? "" });
    return;
  }
  if (action === "clarify-dismiss") {
    postToHost({ action: "clarify", choice: null });
    return;
  }

  postToHost({ action, index });
});

renderMessages();

// ── Incremental update API (called from C# via InvokeScript) ──────────────

window.__splaAppendMessage = async function(json) {
  const message = JSON.parse(json);
  const chat = document.getElementById("chat");
  const existing = chat.querySelector(`[data-index="${message.index}"]`);
  if (existing) { existing.remove(); }
  chat.insertAdjacentHTML("beforeend", buildShell(message));
  const container = chat.querySelector(`[data-content="${message.index}"]`);
  if (!container) return;
  if (message.isMarkdown) {
    await renderMarkdownInto(container, message.content, message.index);
  } else {
    container.innerHTML = `<pre class="plain">${escapeHtml(message.content)}</pre>`;
  }
  window.scrollTo(0, document.body.scrollHeight);
};

window.__splaAppendDelta = function(index, delta) {
  const chat = document.getElementById("chat");
  const container = chat.querySelector(`[data-content="${index}"]`);
  if (!container) return;
  const raw = (container.dataset.raw ?? "") + delta;
  container.dataset.raw = raw;

  // Debounce markdown re-parse at ~80ms
  clearTimeout(container._debounce);
  container._debounce = setTimeout(async () => {
    container.dataset.raw = container.dataset.raw; // keep raw across closure
    await renderMarkdownInto(container, container.dataset.raw, index);
    window.scrollTo(0, document.body.scrollHeight);
  }, 80);
};

window.__splaFinalizeMessage = async function(index, content) {
  const chat = document.getElementById("chat");
  const container = chat.querySelector(`[data-content="${index}"]`);
  if (!container) return;
  clearTimeout(container._debounce);
  container.dataset.raw = content;
  await renderMarkdownInto(container, content, index);
  window.scrollTo(0, document.body.scrollHeight);
};

window.__splaRemoveMessage = function(index) {
  const chat = document.getElementById("chat");
  const article = chat.querySelector(`[data-index="${index}"]`);
  if (article) article.remove();
};

window.__splaSetReasoning = function(index, text) {
  setReasoning(index, text);
};

// ── Clarify widget ────────────────────────────────────────────────────────

window.__splaClarify = function(json) {
  const data = JSON.parse(json);
  const widget = document.getElementById("clarify-widget");
  document.getElementById("clarify-question").textContent = data.question ?? "";
  const optionsEl = document.getElementById("clarify-options");
  optionsEl.innerHTML = "";
  for (const opt of (data.options ?? [])) {
    const btn = document.createElement("button");
    btn.className = "clarify-btn";
    btn.dataset.action = "clarify-choose";
    btn.dataset.choice = opt.label;
    btn.textContent = opt.label;
    if (opt.description) btn.title = opt.description;
    optionsEl.appendChild(btn);
  }
  widget.classList.add("visible");
  window.scrollTo(0, document.body.scrollHeight);
};

window.__splaClearClarify = function() {
  const widget = document.getElementById("clarify-widget");
  widget.classList.remove("visible");
};
</script>
</body>
</html>
""";
    }

    private static void EnsureAssetFiles()
    {
        WriteAssetFileIfChanged("marked.min.js");
        WriteAssetFileIfChanged("mermaid.min.js");
    }

    private static void WriteAssetFileIfChanged(string fileName)
    {
        var targetPath = Path.Combine(GetAssetsDirectory(), fileName);
        var content = ReadAssetBytes(fileName);

        if (File.Exists(targetPath))
        {
            var fileInfo = new FileInfo(targetPath);
            if (fileInfo.Length == content.Length) return;
        }

        File.WriteAllBytes(targetPath, content);
    }

    private static void CleanupOldRuntimeFiles()
    {
        var runtimeDirectory = GetRuntimeDirectory();
        var threshold = DateTime.UtcNow.AddHours(-6);

        foreach (var file in Directory.EnumerateFiles(runtimeDirectory, "chat-*.html"))
        {
            try
            {
                if (File.GetLastWriteTimeUtc(file) < threshold)
                {
                    File.Delete(file);
                }
            }
            catch
            {
                // Stale webview files are harmless and should not block chat rendering.
            }
        }
    }

    private static string GetRuntimeDirectory() => EnsureDirectory(Path.Combine(GetWebChatDirectory(), "runtime"));

    private static string GetAssetsDirectory() => EnsureDirectory(Path.Combine(GetWebChatDirectory(), "assets"));

    private static string GetWebChatDirectory()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return EnsureDirectory(Path.Combine(localAppData, "SPLA", "webchat"));
    }

    private static string EnsureDirectory(string path)
    {
        Directory.CreateDirectory(path);
        return path;
    }

    private static byte[] ReadAssetBytes(string fileName)
    {
        var uri = new Uri($"avares://SPLA.UI.Avalonia/Assets/webchat/{fileName}");
        using var stream = AssetLoader.Open(uri);
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private static string ReadAssetText(string fileName)
    {
        var uri = new Uri($"avares://SPLA.UI.Avalonia/Assets/webchat/{fileName}");
        using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private sealed record ChatHtmlMessage(
        int Index,
        string Role,
        string Content,
        string Reasoning,
        string RetentionIcon,
        string RetentionDescription,
        bool IsUser,
        bool IsAssistant,
        bool IsSystem,
        bool IsTool,
        bool IsError,
        bool IsToolCallNotice,
        bool IsPlainText,
        bool IsMarkdown,
        bool IsPermission,
        bool IsAnswered,
        string PermArguments);
}

public sealed record RenderedWebChat(string DocumentPath, Uri DocumentUri);
