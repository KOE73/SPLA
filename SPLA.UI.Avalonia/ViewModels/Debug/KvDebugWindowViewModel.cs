using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Domain.Agent;
using System.Collections.ObjectModel;
using System.Linq;

namespace SPLA.UI.Avalonia.ViewModels.Debug;

public partial class KvDebugWindowViewModel : ObservableObject
{
    private IKeyValueStore          _session;
    private readonly IKeyValueStore _project;
    private IBlobStore?             _blobs;

    // Raw collections — full store contents.
    private readonly ObservableCollection<KvEntryViewModel>   _allSession = new();
    private readonly ObservableCollection<KvEntryViewModel>   _allProject = new();
    private readonly ObservableCollection<BlobEntryViewModel> _allBlobs   = new();

    // Filtered views — what the window actually binds to.
    public ObservableCollection<KvEntryViewModel>   SessionEntries { get; } = new();
    public ObservableCollection<KvEntryViewModel>   ProjectEntries { get; } = new();
    public ObservableCollection<BlobEntryViewModel> BlobEntries    { get; } = new();

    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private int    _sessionCount;
    [ObservableProperty] private int    _projectCount;
    [ObservableProperty] private int    _blobCount;
    [ObservableProperty] private long   _blobTotalSize;

    public string BlobTotalSizeLabel => BlobTotalSize switch
    {
        < 1024            => $"{BlobTotalSize} B",
        < 1024 * 1024     => $"{BlobTotalSize / 1024.0:F1} KB",
        _                 => $"{BlobTotalSize / (1024.0 * 1024):F1} MB"
    };

    partial void OnFilterTextChanged(string value) => ApplyFilter();
    partial void OnBlobTotalSizeChanged(long value) => OnPropertyChanged(nameof(BlobTotalSizeLabel));

    public KvDebugWindowViewModel(IKeyValueStore session, IKeyValueStore project, IBlobStore? blobs = null)
    {
        _session = session;
        _project = project;
        _blobs   = blobs;

        _session.Changed += OnSessionChanged;
        _project.Changed += OnProjectChanged;
        if (_blobs != null) _blobs.Changed += OnBlobsChanged;

        Refresh(_session, _allSession, v => SessionCount = v);
        Refresh(_project, _allProject, v => ProjectCount = v);
        if (_blobs != null) RefreshBlobs();
        ApplyFilter();
    }

    /// <summary>Re-target the window at a different chat's session store/blobs (project KV is global,
    /// so it stays put). Unsubscribes the old stores and rebinds — the window instance is reused.</summary>
    public void Rebind(IKeyValueStore session, IBlobStore? blobs)
    {
        if (ReferenceEquals(_session, session) && ReferenceEquals(_blobs, blobs)) return;

        _session.Changed -= OnSessionChanged;
        if (_blobs != null) _blobs.Changed -= OnBlobsChanged;

        _session = session;
        _blobs   = blobs;

        _session.Changed += OnSessionChanged;
        if (_blobs != null) _blobs.Changed += OnBlobsChanged;

        Refresh(_session, _allSession, v => SessionCount = v);
        if (_blobs != null) RefreshBlobs();
        else { _allBlobs.Clear(); BlobCount = 0; BlobTotalSize = 0; }
        ApplyFilter();
    }

    private void OnSessionChanged(object? sender, System.EventArgs e)
        => Dispatcher.UIThread.Post(() => { Refresh(_session, _allSession, v => SessionCount = v); ApplyFilter(); });

    private void OnProjectChanged(object? sender, System.EventArgs e)
        => Dispatcher.UIThread.Post(() => { Refresh(_project, _allProject, v => ProjectCount = v); ApplyFilter(); });

    private void OnBlobsChanged(object? sender, System.EventArgs e)
        => Dispatcher.UIThread.Post(() => { if (_blobs != null) RefreshBlobs(); ApplyFilter(); });

    private void ApplyFilter()
    {
        var f = FilterText?.Trim() ?? string.Empty;
        Apply(_allSession, SessionEntries, f);
        Apply(_allProject, ProjectEntries, f);
        ApplyBlobs(_allBlobs, BlobEntries, f);
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

    private void RefreshBlobs()
    {
        if (_blobs == null) return;
        var entries = _blobs.List();
        BlobCount     = entries.Count;
        BlobTotalSize = entries.Sum(e => e.Size);

        var handles = entries.Select(e => e.Handle).ToHashSet();
        for (int i = _allBlobs.Count - 1; i >= 0; i--)
            if (!handles.Contains(_allBlobs[i].Handle)) _allBlobs.RemoveAt(i);

        for (int i = 0; i < entries.Count; i++)
        {
            var entry    = entries[i];
            var existing = _allBlobs.FirstOrDefault(b => b.Handle == entry.Handle);
            if (existing != null)
            {
                var idx = _allBlobs.IndexOf(existing);
                if (idx != i) { _allBlobs.RemoveAt(idx); _allBlobs.Insert(i, existing); }
            }
            else
            {
                _allBlobs.Insert(i, new BlobEntryViewModel(_blobs, entry));
            }
        }
    }

    private static void ApplyBlobs(ObservableCollection<BlobEntryViewModel> all,
                                   ObservableCollection<BlobEntryViewModel> view, string filter)
    {
        var visible = string.IsNullOrEmpty(filter)
            ? all.ToList()
            : all.Where(e => e.DisplayHandle.Contains(filter, System.StringComparison.OrdinalIgnoreCase)
                          || e.KindLabel.Contains(filter, System.StringComparison.OrdinalIgnoreCase)).ToList();

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

    [RelayCommand]
    private void ClearFilter() => FilterText = string.Empty;

    [RelayCommand]
    private void ClearSession() => (_session as Domain.Agent.KeyValueStore)?.Clear();

    [RelayCommand]
    private void ClearProject() => (_project as Domain.Agent.KeyValueStore)?.Clear();

    [RelayCommand]
    private void ClearBlobs()
    {
        if (_blobs == null) return;
        foreach (var entry in _blobs.List())
            _blobs.Delete(entry.Handle);
    }
}
