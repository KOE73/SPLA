using System.Collections.Generic;
using System.Linq;

namespace SPLA.Plugins.Browser;

/// <summary>Bounded ring of console messages and uncaught JS errors collected for one tab, exposed
/// by <c>browser_console</c> / <c>browser_page_errors</c>.</summary>
internal sealed class PageDiagnostics
{
    private const int MaxItems = 200;
    private readonly object _lock = new();
    private readonly List<string> _console = new();
    private readonly List<string> _errors = new();

    public void AddConsole(string entry)
    {
        lock (_lock)
        {
            _console.Add(entry);
            if (_console.Count > MaxItems) _console.RemoveAt(0);
        }
    }

    public void AddError(string entry)
    {
        lock (_lock)
        {
            _errors.Add(entry);
            if (_errors.Count > MaxItems) _errors.RemoveAt(0);
        }
    }

    public IReadOnlyList<string> Console()
    {
        lock (_lock) return _console.ToList();
    }

    public IReadOnlyList<string> Errors()
    {
        lock (_lock) return _errors.ToList();
    }
}
