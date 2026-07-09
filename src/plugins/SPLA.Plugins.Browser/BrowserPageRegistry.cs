using Microsoft.Playwright;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.Plugins.Browser;

/// <summary>
/// Tracks a session's open tabs under stable short ids (<c>t1</c>, <c>t2</c>, …) instead of exposing
/// Playwright's <see cref="IPage"/> objects directly to tool arguments. Also tracks which tab is
/// "active" — the implicit target for any tool call that omits <c>tab_id</c>.
/// </summary>
internal sealed class BrowserPageRegistry
{
    private readonly object _lock = new();
    private readonly Dictionary<string, IPage> _pages = new();
    private int _counter;

    public string? ActiveTabId { get; private set; }

    /// <summary>
    /// Registers a page, making it active if it's the first tab. Idempotent by reference: Playwright
    /// can fire the context's <c>Page</c> event for a page that the caller also already has a
    /// handle to (e.g. one just opened via <c>Context.NewPageAsync()</c>), so a second registration
    /// of the same page returns its existing id instead of creating a duplicate tab entry. The
    /// close-removal handler is wired only on first registration. Returns the id and whether this
    /// call actually created a new entry (callers that wire one-time-only state, like console/error
    /// listeners, key off that).
    /// </summary>
    public (string Id, bool IsNew) RegisterEx(IPage page)
    {
        string id;
        bool isNew;
        lock (_lock)
        {
            foreach (var kv in _pages)
            {
                if (!ReferenceEquals(kv.Value, page)) continue;
                ActiveTabId ??= kv.Key;
                return (kv.Key, false);
            }
            id = $"t{++_counter}";
            _pages[id] = page;
            ActiveTabId ??= id;
            isNew = true;
        }
        page.Close += (_, _) => Unregister(id);
        return (id, isNew);
    }

    public string Register(IPage page) => RegisterEx(page).Id;

    private void Unregister(string id)
    {
        lock (_lock)
        {
            _pages.Remove(id);
            if (ActiveTabId == id)
                ActiveTabId = _pages.Keys.FirstOrDefault();
        }
    }

    public bool TryGet(string id, out IPage page)
    {
        lock (_lock) return _pages.TryGetValue(id, out page!);
    }

    public IPage? Active
    {
        get
        {
            lock (_lock)
                return ActiveTabId != null && _pages.TryGetValue(ActiveTabId, out var p) ? p : null;
        }
    }

    public bool SetActive(string id)
    {
        lock (_lock)
        {
            if (!_pages.ContainsKey(id)) return false;
            ActiveTabId = id;
            return true;
        }
    }

    public IReadOnlyList<(string Id, IPage Page)> All()
    {
        lock (_lock) return _pages.Select(kv => (kv.Key, kv.Value)).ToList();
    }

    public async Task<bool> CloseAsync(string id)
    {
        IPage? page;
        lock (_lock)
        {
            if (!_pages.TryGetValue(id, out page)) return false;
        }
        await page!.CloseAsync();
        return true; // Unregister(id) runs via the Close event handler above.
    }

    /// <summary>Drops all tracked tabs without closing them — used when the whole browser is torn
    /// down (the pages are gone anyway once the context/browser closes).</summary>
    public void Clear()
    {
        lock (_lock)
        {
            _pages.Clear();
            ActiveTabId = null;
        }
    }
}
