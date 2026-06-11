using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;

namespace SPLA.UI.Avalonia.Helpers;

public static class IconGenerator
{
    public static WindowIcon? GenerateIcon(string? workspacePath, string? projectName)
    {
        try
        {
            // 1. Try to load custom icon
            if (workspacePath != null)
            {
                var iconPath = Path.Combine(workspacePath, ".spla", "icon.png");
                if (File.Exists(iconPath))
                {
                    return new WindowIcon(iconPath);
                }
            }

            // 2. Generate icon from initials
            var name = string.IsNullOrWhiteSpace(projectName) ? "SPLA" : projectName;
            var initials = GetInitials(name);

            // Create a simple generated bitmap (RenderTargetBitmap)
            // Avalonia 11 handles RenderTargetBitmap differently, we need a TopLevel or UI thread
            // For simplicity and avoiding complex render loops, we return null if no file exists.
            // In a real app we'd draw text on a DrawingContext and render it to a bitmap.
            // For now, let's just use the default icon if there's no custom icon.
            
            // Avalonia requires an actual stream or asset for WindowIcon. 
            // Generating it entirely in memory without a control context is tricky.
            
            return null; // Fallback to default Avalonia icon defined in axaml
        }
        catch
        {
            return null;
        }
    }

    private static string GetInitials(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "SP";
        var parts = text.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
        return (parts[0].Substring(0, 1) + parts[1].Substring(0, 1)).ToUpper();
    }
}
