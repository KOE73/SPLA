# Web Chat Assets

Local browser-side renderer dependencies for `ChatWebView`.

- `marked.min.js`: Marked 15.0.12, MIT license.
- `mermaid.min.js`: Mermaid 11.12.0, MIT license.

These files are embedded as Avalonia resources and loaded through `Avalonia.Platform.AssetLoader`, so the chat renderer does not depend on external network access.
