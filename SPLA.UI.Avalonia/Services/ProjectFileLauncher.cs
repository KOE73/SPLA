using System;
using System.Diagnostics;
using System.IO;

namespace SPLA.UI.Avalonia.Services;

internal static class ProjectFileLauncher
{
    public static void OpenNewInstance(string projectFilePath)
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath)) return;

        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = Path.GetDirectoryName(projectFilePath) ?? Environment.CurrentDirectory,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add(projectFilePath);

        Process.Start(startInfo);
    }
}
