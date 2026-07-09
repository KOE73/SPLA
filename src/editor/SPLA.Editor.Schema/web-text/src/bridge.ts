// Обратный канал page → host (вперёд данные приходят запечёнными в window.__SPLA_PAYLOAD, см. main.ts).
// postToHost → window.chrome.webview.postMessage (webkit-фолбэк).

export interface HostMessage {
  action: "save";
  text: string;
  docId: string; // какому файлу принадлежит text (хост пишет именно в него)
}

export function postToHost(message: HostMessage): void {
  const w = window as unknown as {
    chrome?: { webview?: { postMessage(m: unknown): void } };
    webkit?: { messageHandlers?: { spla?: { postMessage(m: unknown): void } } };
  };
  const payload = JSON.stringify(message);
  if (w.chrome?.webview?.postMessage) {
    w.chrome.webview.postMessage(payload);
  } else if (w.webkit?.messageHandlers?.spla?.postMessage) {
    w.webkit.messageHandlers.spla.postMessage(payload);
  }
}
