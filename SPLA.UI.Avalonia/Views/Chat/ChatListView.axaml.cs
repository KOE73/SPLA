using Avalonia.Controls;
using SPLA.UI.Avalonia.ViewModels;
using SPLA.UI.Avalonia.ViewModels.Projects;

namespace SPLA.UI.Avalonia.Views.Chat;

public partial class ChatListView : UserControl
{
    public ChatListView()
    {
        InitializeComponent();
    }

    private void RecentProjects_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm == null) return;

        var lb = sender as ListBox;
        if (lb?.SelectedItem is RecentProjectViewModel project)
        {
            lb.SelectedItem = null;
            vm.OpenRecentProjectCommand.Execute(project);
        }
    }
}
