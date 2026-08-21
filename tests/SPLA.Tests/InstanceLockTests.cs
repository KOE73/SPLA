using System;
using System.IO;
using System.Text.Json;
using SPLA.Domain.Project;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Tests for InstanceLock: one-writer invariant, liveness checking, and instance info publishing.
/// </summary>
public sealed class InstanceLockTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(
        Path.GetTempPath(),
        $"spla-instance-tests-{Guid.NewGuid():N}");

    public InstanceLockTests()
    {
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempRoot, recursive: true); } catch { /* best effort */ }
    }

    // ── Acquire ───────────────────────────────────────────────────────────────

    [Fact]
    public void Acquire_on_empty_directory_succeeds_and_creates_file()
    {
        var runtimeDir = Path.Combine(_tempRoot, "acquire-test");
        var lockFile = Path.Combine(runtimeDir, InstanceLock.FileName);

        using var lock1 = InstanceLock.Acquire(runtimeDir, "serve");

        Assert.True(File.Exists(lockFile));
        Assert.NotNull(lock1.Info);
    }

    [Fact]
    public void Acquire_fills_info_with_non_empty_instance_id()
    {
        var runtimeDir = Path.Combine(_tempRoot, "acquire-id-test");

        using var lock1 = InstanceLock.Acquire(runtimeDir, "serve");

        Assert.NotEmpty(lock1.Info.InstanceId);
    }

    [Fact]
    public void Acquire_fills_info_with_current_process_id()
    {
        var runtimeDir = Path.Combine(_tempRoot, "acquire-pid-test");
        var currentPid = System.Diagnostics.Process.GetCurrentProcess().Id;

        using var lock1 = InstanceLock.Acquire(runtimeDir, "serve");

        Assert.Equal(currentPid, lock1.Info.Pid);
    }

    [Fact]
    public void Acquire_fills_info_with_current_machine_name()
    {
        var runtimeDir = Path.Combine(_tempRoot, "acquire-machine-test");
        var currentMachine = Environment.MachineName;

        using var lock1 = InstanceLock.Acquire(runtimeDir, "serve");

        Assert.Equal(currentMachine, lock1.Info.Machine);
    }

    [Fact]
    public void Acquire_fills_info_with_mode_string()
    {
        var runtimeDir = Path.Combine(_tempRoot, "acquire-mode-test");

        using var lock1 = InstanceLock.Acquire(runtimeDir, "repl");

        Assert.Equal("repl", lock1.Info.Mode);
    }

    [Fact]
    public void Acquire_second_time_on_same_directory_throws_ProjectBusyException()
    {
        var runtimeDir = Path.Combine(_tempRoot, "acquire-busy-test");

        using var lock1 = InstanceLock.Acquire(runtimeDir, "serve");

        var ex = Assert.Throws<ProjectBusyException>(
            () => InstanceLock.Acquire(runtimeDir, "repl"));

        Assert.NotNull(ex.Holder);
    }

    [Fact]
    public void ProjectBusyException_holder_reports_first_instance_mode()
    {
        var runtimeDir = Path.Combine(_tempRoot, "busy-mode-test");

        using var lock1 = InstanceLock.Acquire(runtimeDir, "mcp");

        var ex = Assert.Throws<ProjectBusyException>(
            () => InstanceLock.Acquire(runtimeDir, "chat-run"));

        Assert.Equal("mcp", ex.Holder.Mode);
    }

    [Fact]
    public void ProjectBusyException_holder_reports_first_instance_pid()
    {
        var runtimeDir = Path.Combine(_tempRoot, "busy-pid-test");
        var currentPid = System.Diagnostics.Process.GetCurrentProcess().Id;

        using var lock1 = InstanceLock.Acquire(runtimeDir, "serve");

        var ex = Assert.Throws<ProjectBusyException>(
            () => InstanceLock.Acquire(runtimeDir, "ui"));

        Assert.Equal(currentPid, ex.Holder.Pid);
    }

    // ── TryAcquire ────────────────────────────────────────────────────────────

    [Fact]
    public void TryAcquire_on_held_directory_returns_null()
    {
        var runtimeDir = Path.Combine(_tempRoot, "try-held-test");

        using var lock1 = InstanceLock.Acquire(runtimeDir, "serve");

        var lock2 = InstanceLock.TryAcquire(runtimeDir, "repl", out var holder);

        Assert.Null(lock2);
    }

    [Fact]
    public void TryAcquire_on_held_directory_reports_holder()
    {
        var runtimeDir = Path.Combine(_tempRoot, "try-held-holder-test");

        using var lock1 = InstanceLock.Acquire(runtimeDir, "serve");

        InstanceLock.TryAcquire(runtimeDir, "repl", out var holder);

        Assert.NotNull(holder);
        Assert.Equal("serve", holder.Mode);
        Assert.Equal(lock1.Info.Pid, holder.Pid);
    }

    [Fact]
    public void TryAcquire_on_free_directory_returns_live_lock()
    {
        var runtimeDir = Path.Combine(_tempRoot, "try-free-test");

        var lock1 = InstanceLock.TryAcquire(runtimeDir, "serve", out var holder);

        try
        {
            Assert.NotNull(lock1);
            Assert.Null(holder);
        }
        finally
        {
            lock1?.Dispose();
        }
    }

    // ── Read ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Read_returns_null_for_directory_with_no_instance_file()
    {
        var runtimeDir = Path.Combine(_tempRoot, "read-no-file-test");
        Directory.CreateDirectory(runtimeDir);

        var info = InstanceLock.Read(runtimeDir);

        Assert.Null(info);
    }

    [Fact]
    public void Read_returns_live_instance_info_while_held()
    {
        var runtimeDir = Path.Combine(_tempRoot, "read-held-test");

        using var lock1 = InstanceLock.Acquire(runtimeDir, "serve");

        var info = InstanceLock.Read(runtimeDir);

        Assert.NotNull(info);
        Assert.Equal("serve", info.Mode);
        Assert.Equal(lock1.Info.InstanceId, info.InstanceId);
    }

    [Fact]
    public void Read_returns_null_for_leftover_file_not_held()
    {
        var runtimeDir = Path.Combine(_tempRoot, "read-leftover-test");
        var filePath = Path.Combine(runtimeDir, InstanceLock.FileName);

        // Acquire and capture the bytes
        byte[] instanceBytes;
        using (var lock1 = InstanceLock.Acquire(runtimeDir, "serve"))
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                instanceBytes = new byte[stream.Length];
                stream.ReadExactly(instanceBytes);
            }
        }

        // Now the lock is disposed, and the file should be deleted by Dispose
        // But we write back the leftover bytes to simulate a crashed process
        File.WriteAllBytes(filePath, instanceBytes);

        // Read should return null because the file is not held (nobody has it open for writing)
        var info = InstanceLock.Read(runtimeDir);

        Assert.Null(info);
    }

    // ── Publish ───────────────────────────────────────────────────────────────

    [Fact]
    public void Publish_makes_endpoint_visible_to_Read()
    {
        var runtimeDir = Path.Combine(_tempRoot, "publish-test");

        using var lock1 = InstanceLock.Acquire(runtimeDir, "serve");

        Assert.Null(lock1.Info.Endpoint);

        lock1.Publish("http://localhost:8080");

        var info = InstanceLock.Read(runtimeDir);
        Assert.NotNull(info);
        Assert.Equal("http://localhost:8080", info.Endpoint);
    }

    // ── Dispose ────────────────────────────────────────────────────────────────

    [Fact]
    public void After_Dispose_directory_can_be_acquired_again()
    {
        var runtimeDir = Path.Combine(_tempRoot, "dispose-test");

        using (var lock1 = InstanceLock.Acquire(runtimeDir, "serve"))
        {
            // locked
        }

        // After dispose, should succeed
        using var lock2 = InstanceLock.Acquire(runtimeDir, "repl");

        Assert.NotNull(lock2.Info);
    }

    [Fact]
    public void After_Dispose_Read_returns_null()
    {
        var runtimeDir = Path.Combine(_tempRoot, "dispose-read-test");

        using (var lock1 = InstanceLock.Acquire(runtimeDir, "serve"))
        {
            var info = InstanceLock.Read(runtimeDir);
            Assert.NotNull(info);
        }

        // After dispose
        var infoAfter = InstanceLock.Read(runtimeDir);
        Assert.Null(infoAfter);
    }

    // ── InstanceInfo ───────────────────────────────────────────────────────────

    [Fact]
    public void InstanceInfo_IsLocal_true_for_instance_on_this_machine()
    {
        var runtimeDir = Path.Combine(_tempRoot, "islocal-test");
        var currentMachine = Environment.MachineName;

        using var lock1 = InstanceLock.Acquire(runtimeDir, "serve");

        Assert.True(lock1.Info.IsLocal);
    }

    [Fact]
    public void InstanceInfo_Describe_includes_mode_and_pid()
    {
        var runtimeDir = Path.Combine(_tempRoot, "describe-test");
        var currentPid = System.Diagnostics.Process.GetCurrentProcess().Id;

        using var lock1 = InstanceLock.Acquire(runtimeDir, "mcp");

        var description = lock1.Info.Describe();

        Assert.Contains("mcp", description);
        Assert.Contains(currentPid.ToString(), description);
    }
}
