using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Plugins.Sql;
using System.Collections.ObjectModel;
using System.Linq;

namespace SPLA.Plugins.Sql.Avalonia.ViewModels;

public partial class DbConnectionsSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _defaultConnection = "";
    [ObservableProperty] private int _defaultLimit = 10;

    public ObservableCollection<DbConnectionViewModel> Connections { get; } = new();

    [RelayCommand]
    private void AddConnection()
        => Connections.Add(new DbConnectionViewModel { Name = $"db{Connections.Count + 1}" });

    [RelayCommand]
    private void RemoveConnection(DbConnectionViewModel conn)
        => Connections.Remove(conn);

    public void LoadFrom(SqlSettings settings)
    {
        DefaultConnection = settings.DefaultConnection ?? "";
        DefaultLimit = settings.DefaultLimit;
        Connections.Clear();
        foreach (var kvp in settings.Connections)
            Connections.Add(DbConnectionViewModel.FromConfig(kvp.Key, kvp.Value));
    }

    public SqlSettings ToSettings() => new()
    {
        DefaultConnection = string.IsNullOrWhiteSpace(DefaultConnection) ? null : DefaultConnection,
        DefaultLimit = DefaultLimit <= 0 ? 10 : DefaultLimit,
        Connections = Connections
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .ToDictionary(c => c.Name, c => c.ToConfig())
    };
}
