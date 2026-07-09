// Bridge between the WebView page and the Avalonia host.
//
// host → page:  host calls window.__splaEditorLoad(schemaJson, uiSchemaJson, dataJson)
//               via NativeWebView.InvokeScript (strings are JSON-encoded on the C# side).
// page → host:  postToHost({...}) → window.chrome.webview.postMessage (WebView2)
//               with a webkit fallback, mirroring ChatHtmlRenderer's bridge.

export interface HostMessage {
  action: "ready" | "change" | "save";
  data?: unknown;
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

export interface LoadPayload {
  schema: unknown;
  uischema: unknown | null;
  data: unknown;
}

type LoadHandler = (payload: LoadPayload) => void;

// Registers window.__splaEditorLoad so the host can push schema + data into the form.
export function onHostLoad(handler: LoadHandler): void {
  (window as unknown as Record<string, unknown>).__splaEditorLoad = (
    schemaJson: string,
    uiSchemaJson: string | null,
    dataJson: string
  ) => {
    handler({
      schema: JSON.parse(schemaJson),
      uischema: uiSchemaJson ? JSON.parse(uiSchemaJson) : null,
      data: dataJson ? JSON.parse(dataJson) : {},
    });
  };
}
