using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Domain.Agent;
using System;

namespace SPLA.UI.Avalonia.ViewModels.Debug;

public partial class BlobEntryViewModel : ObservableObject
{
    private readonly IBlobStore _store;

    public BlobEntryViewModel(IBlobStore store, BlobEntry entry)
    {
        _store     = store;
        Handle     = entry.Handle;
        Name       = entry.Name;
        Kind       = entry.Kind;
        Size       = entry.Size;
        CreatedAt  = entry.CreatedAt;
    }

    public string            Handle    { get; }
    public string?           Name      { get; }
    public BlobKind          Kind      { get; }
    public long              Size      { get; }
    public DateTimeOffset    CreatedAt { get; }

    public string SizeLabel => Size switch
    {
        < 1024             => $"{Size} B",
        < 1024 * 1024      => $"{Size / 1024.0:F1} KB",
        _                  => $"{Size / (1024.0 * 1024):F1} MB"
    };

    public string KindLabel => Kind == BlobKind.Text ? "txt" : "bin";

    public string DisplayHandle => Name is not null ? $"blob:{Name}" : Handle;

    [RelayCommand]
    private void Delete() => _store.Delete(Handle);
}
