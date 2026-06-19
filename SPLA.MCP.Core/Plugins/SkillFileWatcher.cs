using SPLA.Domain.Agent;
using System;
using System.IO;
using System.Threading;

namespace SPLA.MCP.Core.Plugins;

/// <summary>
/// Watches the plugins directory for skill file changes (.md) and triggers
/// <see cref="SkillManager.Reload"/> with a debounce. Reload is deferred while
/// a skill is active — it fires as soon as the skill is deactivated.
/// Dispose to stop watching.
/// </summary>
public sealed class SkillFileWatcher : IDisposable
{
    private readonly SkillManager _skillManager;
    private readonly ISkillSession _session;
    private readonly FileSystemWatcher _watcher;
    private readonly int _debounceMs;
    private Timer? _debounceTimer;
    private bool _pendingReload;

    public SkillFileWatcher(SkillManager skillManager, ISkillSession session,
        string pluginsDirectory, int debounceMs = 500)
    {
        _skillManager = skillManager;
        _session = session;
        _debounceMs = debounceMs;

        _session.Changed += OnSkillSessionChanged;

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

    private void OnFileEvent(object sender, FileSystemEventArgs e) => ScheduleReload();

    private void ScheduleReload()
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ => TryReload(), null, _debounceMs, Timeout.Infinite);
    }

    private void TryReload()
    {
        if (_session.ActiveSkillId is not null)
        {
            // Skill is active — defer until deactivated.
            _pendingReload = true;
            return;
        }

        _pendingReload = false;
        _skillManager.Reload();
    }

    private void OnSkillSessionChanged(object? sender, EventArgs e)
    {
        // Skill just deactivated and there is a pending reload.
        if (_session.ActiveSkillId is null && _pendingReload)
            TryReload();
    }

    public void Dispose()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _debounceTimer?.Dispose();
        _session.Changed -= OnSkillSessionChanged;
    }
}
