using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SPLA.Plugins.Sql.Avalonia.Views;

public partial class DbConnectionsSettingsView : UserControl
{
    public DbConnectionsSettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
