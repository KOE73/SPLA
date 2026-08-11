using System.Reflection;
using Renci.SshNet;
using SPLA.Plugins.Ssh;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Guards the one part of the terminal resize that cannot fail loudly. SSH.NET exposes no public way
/// to resize a <see cref="ShellStream"/> after it is created, so <see cref="SshLiveSession.Resize"/>
/// reaches the channel's <c>window-change</c> request by reflection — and reflection that stops
/// matching returns null instead of failing to compile. The symptom would be silent and remote: the
/// pty stays at its opening size, the shell wraps at the wrong column, and full-screen programs paint
/// into a rectangle smaller than the window. A red test here beats discovering that over SSH.
/// </summary>
public sealed class SshTerminalResizeTests
{
    [Fact]
    public void ShellStream_still_holds_the_channel_this_build_reflects_on()
    {
        var field = typeof(ShellStream).GetField("_channel", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);

        var method = field!.FieldType.GetMethod("SendWindowChangeRequest", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);

        // (columns, rows, pixelWidth, pixelHeight) — all uint, in that order.
        Assert.Equal([typeof(uint), typeof(uint), typeof(uint), typeof(uint)],
            method!.GetParameters().Select(p => p.ParameterType));
    }

    [Fact]
    public void Session_reports_resize_support_on_this_ssh_net()
        => Assert.True(SshLiveSession.SupportsResize,
            "SSH.NET no longer matches the reflection hook — terminal resize has silently become a no-op.");
}
