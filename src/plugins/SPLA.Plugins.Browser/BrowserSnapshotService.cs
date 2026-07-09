using Microsoft.Playwright;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser;

/// <summary>
/// Builds the compact "tree of interactive/notable elements" that is the model's primary way of
/// perceiving a page (vs. a full DOM dump or a screenshot). Implemented as a single JS walk of
/// <c>document.body</c> rather than Playwright's built-in accessibility APIs, because it needs to
/// stamp a <c>data-spla-ref</c> attribute onto each notable element so a later tool call (click,
/// fill, …) can resolve that exact element again via <see cref="ElementRefRegistry"/> — Playwright's
/// accessibility snapshot has no equivalent "give me a handle back" mechanism.
/// </summary>
internal static class BrowserSnapshotService
{
    // Walks the visible DOM and emits one line per "notable" element (interactive controls, headings,
    // landmarks): role, accessible name, and a [ref=eGEN.N] tag. Every notable element gets
    // data-spla-ref="eGEN.N" stamped onto it so ElementRefRegistry can resolve the ref back to a
    // locator later. Hidden subtrees (display:none / visibility:hidden) are skipped entirely.
    private const string Script = """
        (gen) => {
          function isVisible(el) {
            const style = window.getComputedStyle(el);
            if (style.display === 'none' || style.visibility === 'hidden' || style.opacity === '0') return false;
            const rect = el.getBoundingClientRect();
            return rect.width > 0 && rect.height > 0;
          }
          function accessibleName(el) {
            const aria = el.getAttribute('aria-label');
            if (aria) return aria.trim();
            const labelledBy = el.getAttribute('aria-labelledby');
            if (labelledBy) {
              const t = labelledBy.split(/\s+/).map(id => document.getElementById(id)?.innerText || '').join(' ').trim();
              if (t) return t;
            }
            if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {
              const ph = el.getAttribute('placeholder');
              if (ph) return ph.trim();
              if (el.id) {
                const lbl = document.querySelector(`label[for="${CSS.escape(el.id)}"]`);
                if (lbl) return lbl.innerText.trim();
              }
            }
            const alt = el.getAttribute('alt');
            if (alt) return alt.trim();
            const title = el.getAttribute('title');
            if (title) return title.trim();
            const text = (el.innerText || el.value || '').trim();
            return text.slice(0, 80);
          }
          function role(el) {
            const explicit_ = el.getAttribute('role');
            if (explicit_) return explicit_;
            const tag = el.tagName.toLowerCase();
            if (tag === 'a' && el.hasAttribute('href')) return 'link';
            if (tag === 'button') return 'button';
            if (tag === 'select') return 'combobox';
            if (tag === 'textarea') return 'textbox';
            if (tag === 'img') return 'img';
            if (/^h[1-6]$/.test(tag)) return 'heading';
            if (tag === 'input') {
              const type = (el.getAttribute('type') || 'text').toLowerCase();
              if (type === 'checkbox') return 'checkbox';
              if (type === 'radio') return 'radio';
              if (type === 'submit' || type === 'button') return 'button';
              return 'textbox';
            }
            if (tag === 'nav') return 'navigation';
            if (tag === 'main') return 'main';
            if (tag === 'form') return 'form';
            return tag;
          }
          function isInteractive(el) {
            const tag = el.tagName.toLowerCase();
            if (['a','button','input','select','textarea'].includes(tag)) return true;
            if (el.hasAttribute('role')) return true;
            if (el.hasAttribute('onclick')) return true;
            if (el.tabIndex !== undefined && el.tabIndex !== null && el.tabIndex >= 0) return true;
            return false;
          }
          function isNotable(el) {
            const tag = el.tagName.toLowerCase();
            return isInteractive(el) || /^h[1-6]$/.test(tag) || ['nav','main','form'].includes(tag);
          }

          let counter = 0;
          const lines = [];
          function walk(el, depth) {
            if (!el || el.nodeType !== 1) return;
            const style = window.getComputedStyle(el);
            if (style.display === 'none' || style.visibility === 'hidden') return;

            let nextDepth = depth;
            if (isNotable(el) && isVisible(el)) {
              const r = role(el);
              const name = accessibleName(el);
              const refId = 'e' + gen + '.' + (++counter);
              el.setAttribute('data-spla-ref', refId);
              const disabled = el.disabled ? ' disabled' : '';
              const checked = (el.tagName === 'INPUT' && (el.type === 'checkbox' || el.type === 'radio'))
                ? (el.checked ? ' checked' : ' unchecked') : '';
              lines.push('  '.repeat(depth) + '- ' + r + ' "' + name + '" [ref=' + refId + ']' + disabled + checked);
              nextDepth = depth + 1;
            }
            for (const child of el.children) walk(child, nextDepth);
          }
          walk(document.body, 0);
          return lines.join('\n') || '(no interactive elements found)';
        }
        """;

    /// <summary>Runs the snapshot walk on <paramref name="page"/>, mints the next ref generation for
    /// <paramref name="tabId"/>, and returns the rendered tree (capped to <paramref name="maxChars"/>).</summary>
    public static async Task<string> CaptureAsync(IPage page, ElementRefRegistry refs, string tabId, int maxChars = 8000)
    {
        var gen = refs.NextGeneration(tabId);
        var tree = await page.EvaluateAsync<string>(Script, gen);

        if (tree.Length > maxChars)
            tree = tree[..maxChars] +
                   $"\n…(truncated at {maxChars} of {tree.Length} chars — narrow your target or use browser_get_text/browser_inspect for detail)";

        var title = await page.TitleAsync();
        return $"Snapshot (gen {gen}) of \"{title}\" — {page.Url}\n{tree}";
    }
}
