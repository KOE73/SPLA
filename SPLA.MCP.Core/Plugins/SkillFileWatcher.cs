using System;
using System.IO;
using System.Threading;

namespace SPLA.MCP.Core.Plugins;

/// <summary>
/// Watches the plugins directory for skill file changes (.md) and triggers
/// <see cref="SkillManager.Reload"/> with a debounce. Reload is deferred while any chat has a skill
/// active (supplied by <c>isSkillActive</c>) — it retries shortly after. Dispose to stop watching.
/// </summary>
public sealed class SkillFileWatcher : IDisposable
{
    private readonly SkillManager _skillManager;
    private readonly Func<bool> _isSkillActive;
    private readonly FileSystemWatcher _watcher;
    private readonly int _debounceMs;
    private Timer? _debounceTimer;

    /// <param name="isSkillActive">Returns true while any chat is mid-skill, so a reload that would
    /// swap a running skill's body is deferred. With per-chat skill sessions this aggregates across
    /// all chats.</param>
    public SkillFileWatcher(SkillManager skillManager, Func<bool> isSkillActive,
        string pluginsDirectory, int debounceMs = 500)
    {
        _skillManager = skillManager;
        _isSkillActive = isSkillActive;
        _debounceMs = debounceMs;

        _watcher = new FileSystemWatcher(pluginsDirectory, "*.md")
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            EnableRaisingEvents = Directory.Exists(pluginsDirectory)
        };
        _watcher.Changed += OnFileEvent;
        _watcher.Created += OnFileEvent;
        _watcher.Deleted += OnFileEvent;
        _watcher.Renamed += OnFileEvent;
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e) => ScheduleReload(_debounceMs);

    private void ScheduleReload(int delayMs)
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ => TryReload(), null, delayMs, Timeout.Infinite);
    }

    private void TryReload()
    {
        if (_isSkillActive())
        {
            // A skill is active somewhere — retry shortly rather than swapping its body mid-run.
            ScheduleReload(_debounceMs);
            return;
        }

        _skillManager.Reload();
    }

    public void Dispose()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _debounceTimer?.Dispose();
    }
}
