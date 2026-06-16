using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Domain.Agent;

namespace SPLA.UI.Avalonia.ViewModels.Debug;

public partial class KvEntryViewModel : ObservableObject
{
    private readonly IKeyValueStore _store;

    public KvEntryViewModel(IKeyValueStore store, string key, string value)
    {
        _store = store;
        Key = key;
        Value = value;
    }

    public string Scope => _store.Scope;
    public string Key { get; }

    [ObservableProperty]
    private string _value;

    [RelayCommand]
    private void Delete() => _store.Delete(Key);
}
