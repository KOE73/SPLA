using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

/// <summary>
/// Full detail on one element: role/name/value/attributes plus live state (visible, enabled,
/// editable, checked, bounding box). Deliberately one tool rather than separate is_visible/
/// is_enabled/etc. tools — see the plugin plan, "не раздувать API".
/// </summary>
public sealed class BrowserInspectTool : IMcpTool
{
    // Mirrors the role()/accessibleName() heuristics in BrowserSnapshotService's walk script, scoped
    // to a single element instead of the whole tree.
    private const string RoleNameScript = """
        (el) => {
          function accessibleName(el) {
            const aria = el.getAttribute('aria-label');
            if (aria) return aria.trim();
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
            return (el.innerText || el.value || '').trim().slice(0, 200);
          }
          function role(el) {
            const explicit_ = el.getAttribute('role');
            if (explicit_) return explicit_;
            const tag = el.tagName.toLowerCase();
            if (tag === 'a' && el.hasAttribute('href')) return 'link';
            if (tag === 'select') return 'combobox';
            if (tag === 'input') {
              const type = (el.getAttribute('type') || 'text').toLowerCase();
              if (type === 'checkbox') return 'checkbox';
              if (type === 'radio') return 'radio';
              return 'textbox';
            }
            return tag;
          }
          const attrs = {};
          for (const a of el.attributes) if (a.name !== 'data-spla-ref') attrs[a.name] = a.value;
          return {
            role: role(el),
            name: accessibleName(el),
            tag: el.tagName.toLowerCase(),
            value: (el.value !== undefined) ? el.value : null,
            attributes: attrs
          };
        }
        """;

    public string Name => "browser_inspect";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Full detail on one element by ref: role, accessible name, value, attributes, bounding " +
                          "box, and live state (visible/enabled/editable/checked).",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    @ref = new { type = "string", description = "Element ref from browser_snapshot (e.g. 'e3.12')." },
                    tab_id = new { type = "string", description = "Tab id from browser_tabs. Omit for the active tab." }
                },
                required = new[] { "ref" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        string? refId, tabId;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            refId = ToolJson.GetStringTrimmed(root, "ref");
            tabId = ToolJson.GetStringTrimmed(root, "tab_id");
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        if (refId is null) return "Error: 'ref' is required.";

        var (resolvedTabId, page, tabError) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (page is null) return tabError!;

        var (locator, refError) = mgr.Refs.Resolve(page, resolvedTabId!, refId.Trim());
        if (locator is null) return refError!;

        try
        {
            var info = await locator.EvaluateAsync<JsonElement>(RoleNameScript);
            var visible = await locator.IsVisibleAsync();
            var enabled = await locator.IsEnabledAsync();
            bool? editable = null, @checked = null;
            try { editable = await locator.IsEditableAsync(); } catch { /* not an editable-capable element */ }
            try { @checked = await locator.IsCheckedAsync(); } catch { /* not a checkbox/radio */ }
            var box = await locator.BoundingBoxAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"ref: {refId}");
            sb.AppendLine($"role: {GetStr(info, "role")}");
            sb.AppendLine($"name: \"{GetStr(info, "name")}\"");
            sb.AppendLine($"tag: {GetStr(info, "tag")}");
            sb.AppendLine($"value: {GetStr(info, "value") ?? "(none)"}");
            sb.AppendLine($"state: visible={visible}, enabled={enabled}" +
                          (editable != null ? $", editable={editable}" : "") +
                          (@checked != null ? $", checked={@checked}" : ""));
            sb.AppendLine(box != null
                ? $"bounds: x={box.X:0}, y={box.Y:0}, width={box.Width:0}, height={box.Height:0}"
                : "bounds: (not available — element may be detached or hidden)");
            if (info.TryGetProperty("attributes", out var attrs) && attrs.ValueKind == JsonValueKind.Object)
            {
                sb.AppendLine("attributes:");
                foreach (var a in attrs.EnumerateObject())
                    sb.AppendLine($"  {a.Name} = {a.Value.GetString()}");
            }
            return sb.ToString().TrimEnd();
        }
        catch (PlaywrightException ex) { return BrowserToolBase.Fail("browser_inspect", ex); }
    }

    private static string? GetStr(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
}
