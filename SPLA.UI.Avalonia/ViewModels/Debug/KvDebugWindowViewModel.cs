using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Domain.Agent;
using System.Collections.ObjectModel;
using System.Linq;

namespace SPLA.UI.Avalonia.ViewModels.Debug;

public partial class KvDebugWindowViewModel : ObservableObject
{
    private readonly IKeyValueStore _session;
    private readonly IKeyValueStore _project;

    // Raw collections — full store contents.
    private readonly ObservableCollection<KvEntryViewModel> _allSession = new();
    private readonly ObservableCollection<KvEntryViewModel> _allProject = new();

    // Filtered views — what the window actually binds to.
    public ObservableCollection<KvEntryViewModel> SessionEntries { get; } = new();
    public ObservableCollection<KvEntryViewModel> ProjectEntries { get; } = new();

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private int _sessionCount;

    [ObservableProperty]
    private int _projectCount;

    partial void OnFilterTextChanged(string value) => ApplyFilter();

    public KvDebugWindowViewModel(IKeyValueStore session, IKeyValueStore project)
    {
        _session = session;
        _project = project;

        _session.Changed += (_, _) => Dispatcher.UIThread.Post(() => { Refresh(_session, _allSession, v => SessionCount = v); ApplyFilter(); });
        _project.Changed += (_, _) => Dispatcher.UIThread.Post(() => { Refresh(_project, _allProject, v => ProjectCount = v); ApplyFilter(); });

        Refresh(_session, _allSession, v => SessionCount = v);
        Refresh(_project, _allProject, v => ProjectCount = v);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var f = FilterText?.Trim() ?? string.Empty;
        Apply(_allSession, SessionEntries, f);
        Apply(_allProject, ProjectEntries, f);
    }

    private static void Apply(ObservableCollection<KvEntryViewModel> all, ObservableCollection<KvEntryViewModel> view, string filter)
    {
        var visible = string.IsNullOrEmpty(filter)
            ? all.ToList()
            : all.Where(e => e.Key.Contains(filter, System.StringComparison.OrdinalIgnoreCase)
                          || e.Value.Contains(filter, System.StringComparison.OrdinalIgnoreCase)).ToList();

        for (int i = view.Count - 1; i >= 0; i--)
            if (!visible.Contains(view[i])) view.RemoveAt(i);

        for (int i = 0; i < visible.Count; i++)
        {
            if (i < view.Count)
            {
                if (view[i] != visible[i]) { view.RemoveAt(i); view.Insert(i, visible[i]); }
            }
            else view.Add(visible[i]);
        }
    }

    private static void Refresh(IKeyValueStore store, ObservableCollection<KvEntryViewModel> target, System.Action<int> setCount)
    {
        var items = store.List();
        setCount(items.Count);

        // Update in-place: add/remove/update rather than clear (avoids UI flicker).
        var keys = items.Select(kv => kv.Key).ToHashSet();

        // Remove stale entries.
        for (int i = target.Count - 1; i >= 0; i--)
            if (!keys.Contains(target[i].Key))
                target.RemoveAt(i);

        // Update existing + insert new.
        for (int i = 0; i < items.Count; i++)
        {
            var (key, value) = (items[i].Key, items[i].Value);
            var existing = target.FirstOrDefault(e => e.Key == key);
            if (existing != null)
            {
                existing.Value = value;
                var idx = target.IndexOf(existing);
                if (idx != i) { target.RemoveAt(idx); target.Insert(i, existing); }
            }
            else
            {
                target.Insert(i, new KvEntryViewModel(store, key, value));
            }
        }
    }

    [RelayCommand]
    private void ClearFilter() => FilterText = string.Empty;

    [RelayCommand]
    private void ClearSession() => (_session as Domain.Agent.KeyValueStore)?.Clear();

    [RelayCommand]
    private void ClearProject() => (_project as Domain.Agent.KeyValueStore)?.Clear();
}
