using Avalonia.Controls;
using SPLA.Domain.Secrets;
using SPLA.Domain.Settings;
using SPLA.Plugins.Host.Avalonia;
using SPLA.Plugins.Sql;
using SPLA.Plugins.Sql.Avalonia.ViewModels;
using SPLA.Plugins.Sql.Avalonia.Views;

namespace SPLA.Plugins.Sql.Avalonia;

/// <summary>
/// Settings editor for the SQL plugin's database connections.
/// Reads/writes its own typed model; the host only sees an opaque YAML string.
///
/// Secrets split: literal passwords go to the global <see cref="ISecretStore"/> (project scope),
/// the blob keeps only a <c>secret:sql:&lt;name&gt;</c> reference. Matches the agent path
/// (<see cref="SqlConnectionRegistry"/>).
/// </summary>
public sealed class DbConnectionsSettingsEditor : IPluginSettingsEditor
{
    private readonly DbConnectionsSettingsViewModel _vm = new();
    private readonly DbConnectionsSettingsView _view;
    private readonly ISecretStore _secrets;
    private readonly ISecretResolver _resolver;

    public DbConnectionsSettingsEditor(string workspacePath)
    {
        _view = new DbConnectionsSettingsView { DataContext = _vm };
        var store = new FileSecretStore(workspacePath, ConfigLoader.GetDefaultsDir());
        _secrets = store;
        _resolver = new SecretResolver(store);
    }

    public Control View => _view;

    public void Load(IPluginSettingsStore store)
    {
        var settings = SqlSettings.Parse(store.GetYaml());

        // Resolve secret:/env: references to plaintext so passwords are editable in the GUI.
        foreach (var cfg in settings.Connections.Values)
            if (ISecretResolver.IsReference(cfg.Password))
                cfg.Password = _resolver.Resolve(cfg.Password);

        _vm.LoadFrom(settings);
    }

    public void Save(IPluginSettingsStore store)
    {
        var settings = _vm.ToSettings();

        // Literal passwords → secrets store; blob keeps only a reference.
        foreach (var (name, cfg) in settings.Connections)
        {
            if (!string.IsNullOrEmpty(cfg.Password) && !ISecretResolver.IsReference(cfg.Password))
            {
                _secrets.SetAsync(SqlConnectionRegistry.SecretKey(name), cfg.Password!, SecretScope.Project)
                    .AsTask().GetAwaiter().GetResult();
                cfg.Password = SqlConnectionRegistry.SecretRef(name);
            }
        }

        store.SetYaml(settings.ToYaml());
    }
}
