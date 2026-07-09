using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Plugins.Sql;
using System.Threading.Tasks;

namespace SPLA.Plugins.Sql.Avalonia.ViewModels;

public partial class DbConnectionViewModel : ObservableObject
{
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _provider = "mssql";
    [ObservableProperty] private string _server = "";
    [ObservableProperty] private string _database = "";
    [ObservableProperty] private string _user = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private bool _trustedConnection = true;
    [ObservableProperty] private string _description = "";
    [ObservableProperty] private string _testStatus = "";

    public string[] AvailableProviders { get; } = ["mssql", "postgres", "sqlite"];

    public bool IsMssql    => Provider == "mssql";
    public bool IsPostgres => Provider == "postgres";
    public bool IsSqlite   => Provider == "sqlite";
    public bool ShowTrustedConnection => Provider == "mssql";

    partial void OnProviderChanged(string value)
    {
        OnPropertyChanged(nameof(IsMssql));
        OnPropertyChanged(nameof(IsPostgres));
        OnPropertyChanged(nameof(IsSqlite));
        OnPropertyChanged(nameof(ShowTrustedConnection));
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        TestStatus = "Connecting...";
        // Same code path as the sql_test_connection agent tool — one implementation, identical messages.
        var result = await SqlConnectionTester.TestAsync(ToConfig());
        TestStatus = result.Message;
    }

    public SqlConnectionConfig ToConfig() => new()
    {
        Provider          = Provider,
        Server            = Provider == "sqlite" ? null : (string.IsNullOrWhiteSpace(Server) ? null : Server),
        Host              = Provider == "postgres" && !string.IsNullOrWhiteSpace(Server) ? Server : null,
        File              = Provider == "sqlite" ? (string.IsNullOrWhiteSpace(Server) ? null : Server) : null,
        Database          = string.IsNullOrWhiteSpace(Database) ? null : Database,
        User              = string.IsNullOrWhiteSpace(User) ? null : User,
        Password          = string.IsNullOrWhiteSpace(Password) ? null : Password,
        TrustedConnection = TrustedConnection,
        Description       = string.IsNullOrWhiteSpace(Description) ? null : Description
    };

    public static DbConnectionViewModel FromConfig(string name, SqlConnectionConfig cfg) => new()
    {
        Name              = name,
        Provider          = cfg.Provider,
        Server            = cfg.Server ?? cfg.Host ?? cfg.File ?? "",
        Database          = cfg.Database ?? "",
        User              = cfg.User ?? "",
        Password          = cfg.Password ?? "",
        TrustedConnection = cfg.TrustedConnection,
        Description       = cfg.Description ?? ""
    };
}
