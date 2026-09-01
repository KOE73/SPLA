import { SourceCodeService } from "./SourceCodeService.js";

/**
 * Read-only modal dialog for viewing source code files with syntax highlighting,
 * line numbers, copy actions, and resizable/draggable window.
 */
export class CodeViewerDialog {
  private readonly rootEl: HTMLElement;
  private readonly cardEl: HTMLElement;
  private readonly titleEl: HTMLElement;
  private readonly pathBadgeEl: HTMLElement;
  private readonly bodyEl: HTMLElement;
  private readonly footInfoEl: HTMLElement;
  private readonly langTagEl: HTMLElement;
  private readonly copyBtn: HTMLButtonElement;
  private readonly copyPathBtn: HTMLButtonElement;

  private isOpen = false;
  private currentCode = "";
  private currentPath = "";

  constructor() {
    this.rootEl = document.createElement("div");
    this.rootEl.className = "spla-code-modal";
    this.rootEl.hidden = true;

    this.rootEl.innerHTML = `
      <div class="spla-code-card">
        <div class="spla-code-head">
          <div class="spla-code-title-group">
            <span class="spla-code-title-icon">💻</span>
            <span class="spla-code-title-text">Исходный код</span>
            <span class="spla-code-path-badge" title="Путь к файлу"></span>
          </div>
          <div class="spla-code-head-actions">
            <button type="button" class="spla-code-head-btn spla-code-copy-path-btn" title="Скопировать относительный путь">
              📋 Путь
            </button>
            <button type="button" class="spla-code-head-btn spla-code-copy-btn" title="Скопировать весь код">
              📄 Копировать
            </button>
            <button type="button" class="spla-code-close-btn" title="Закрыть (Esc)">✕</button>
          </div>
        </div>
        <div class="spla-code-body"></div>
        <div class="spla-code-foot">
          <div class="spla-code-foot-info">
            <span class="spla-code-lines-count">0 строк</span>
            <span class="spla-code-size">0 KB</span>
          </div>
          <span class="spla-code-lang-tag">C#</span>
        </div>
      </div>
    `;

    this.cardEl = this.rootEl.querySelector(".spla-code-card")!;
    this.titleEl = this.rootEl.querySelector(".spla-code-title-text")!;
    this.pathBadgeEl = this.rootEl.querySelector(".spla-code-path-badge")!;
    this.bodyEl = this.rootEl.querySelector(".spla-code-body")!;
    this.footInfoEl = this.rootEl.querySelector(".spla-code-foot-info")!;
    this.langTagEl = this.rootEl.querySelector(".spla-code-lang-tag")!;
    this.copyBtn = this.rootEl.querySelector(".spla-code-copy-btn")!;
    this.copyPathBtn = this.rootEl.querySelector(".spla-code-copy-path-btn")!;

    this.setupEvents();
    document.body.appendChild(this.rootEl);
  }

  private setupEvents(): void {
    const headEl = this.rootEl.querySelector<HTMLElement>(".spla-code-head")!;
    const closeBtn = this.rootEl.querySelector<HTMLElement>(".spla-code-close-btn")!;

    closeBtn.addEventListener("click", () => this.close());

    this.rootEl.addEventListener("click", (e) => {
      if (e.target === this.rootEl) {
        this.close();
      }
    });

    window.addEventListener("keydown", (e) => {
      if (!this.isOpen) return;
      if (e.key === "Escape") {
        e.preventDefault();
        e.stopPropagation();
        this.close();
      }
    });

    this.copyBtn.addEventListener("click", () => {
      if (!this.currentCode) return;
      navigator.clipboard.writeText(this.currentCode).then(() => {
        const orig = this.copyBtn.innerHTML;
        this.copyBtn.innerHTML = "✓ Скопировано!";
        setTimeout(() => {
          this.copyBtn.innerHTML = orig;
        }, 1500);
      });
    });

    this.copyPathBtn.addEventListener("click", () => {
      if (!this.currentPath) return;
      navigator.clipboard.writeText(this.currentPath).then(() => {
        const orig = this.copyPathBtn.innerHTML;
        this.copyPathBtn.innerHTML = "✓ Путь скопирован!";
        setTimeout(() => {
          this.copyPathBtn.innerHTML = orig;
        }, 1500);
      });
    });

    // Draggable window header
    let isDragging = false;
    let startX = 0;
    let startY = 0;
    let initialLeft = 0;
    let initialTop = 0;

    headEl.addEventListener("mousedown", (e) => {
      if ((e.target as HTMLElement).closest("button")) return;
      isDragging = true;
      startX = e.clientX;
      startY = e.clientY;

      const rect = this.cardEl.getBoundingClientRect();
      initialLeft = rect.left;
      initialTop = rect.top;

      this.cardEl.style.position = "fixed";
      this.cardEl.style.left = `${initialLeft}px`;
      this.cardEl.style.top = `${initialTop}px`;
      this.cardEl.style.margin = "0";

      const onMouseMove = (ev: MouseEvent) => {
        if (!isDragging) return;
        const dx = ev.clientX - startX;
        const dy = ev.clientY - startY;
        this.cardEl.style.left = `${initialLeft + dx}px`;
        this.cardEl.style.top = `${initialTop + dy}px`;
      };

      const onMouseUp = () => {
        isDragging = false;
        window.removeEventListener("mousemove", onMouseMove);
        window.removeEventListener("mouseup", onMouseUp);
      };

      window.addEventListener("mousemove", onMouseMove);
      window.addEventListener("mouseup", onMouseUp);
    });
  }

  /**
   * Open the modal code viewer for a specific codeRef.
   */
  async open(codeRef: string, title?: string): Promise<void> {
    this.isOpen = true;
    this.currentPath = codeRef;
    this.currentCode = "";

    this.titleEl.textContent = title ? `Исходный код: ${title}` : "Исходный код";
    this.pathBadgeEl.textContent = codeRef;
    this.pathBadgeEl.title = codeRef;
    this.langTagEl.textContent = SourceCodeService.getLanguageLabel(codeRef);

    this.bodyEl.innerHTML = `
      <div class="spla-code-loading">
        <span style="font-size: 24px; animation: spin 1s linear infinite;">⏳</span>
        <span>Загрузка файла ${codeRef}...</span>
      </div>
    `;

    this.rootEl.hidden = false;

    // Reset centering position
    this.cardEl.style.position = "";
    this.cardEl.style.left = "";
    this.cardEl.style.top = "";
    this.cardEl.style.margin = "";

    try {
      const source = await SourceCodeService.fetchSource(codeRef);
      this.currentCode = source;

      const result = SourceCodeService.highlight(source, codeRef);
      this.bodyEl.innerHTML = result.html;

      const sizeKb = (new Blob([source]).size / 1024).toFixed(1);
      this.footInfoEl.innerHTML = `
        <span class="spla-code-lines-count">${result.lineCount} строк</span>
        <span class="spla-code-size">${sizeKb} KB</span>
      `;
      this.langTagEl.textContent = result.language;
    } catch (err: any) {
      this.bodyEl.innerHTML = `
        <div class="spla-code-error">
          <div class="spla-code-error-icon">⚠️</div>
          <div class="spla-code-error-title">Не удалось загрузить исходный код</div>
          <div class="spla-code-error-msg">${err?.message || "Файл недоступен на сервере"}</div>
          <div style="font-size: 11px; color: #71717a; margin-top: 8px;">
            Проверьте параметр запуска сервера <code>-root</code> и наличие файла <code>${codeRef}</code>
          </div>
        </div>
      `;
      this.footInfoEl.innerHTML = `
        <span class="spla-code-lines-count">Ошибка загрузки</span>
      `;
    }
  }

  close(): void {
    this.isOpen = false;
    this.rootEl.hidden = true;
  }

  destroy(): void {
    this.rootEl.remove();
  }
}
