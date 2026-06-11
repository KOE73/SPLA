namespace SPLA.UI.Avalonia.ViewModels.Projects;

public class RecentProjectViewModel
{
    public string Name { get; }
    public string Path { get; }

    public RecentProjectViewModel(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path)) ?? "SPLA Project";
    }
}
