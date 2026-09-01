using System;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Host;

namespace SPLA.Tests;

/// <summary>
/// Proves the interactive shell seam: a command that asks a question comes back to the caller with
/// the question visible and a session to answer through, instead of hanging forever behind a
/// process exit that never happens.
/// See <c>docs/adr/ADR_20260824_core_interactive-shell.md</c>.
/// </summary>
public sealed class InteractiveShellTests
{
    /// <summary>Writes a prompt with no trailing line break — exactly how a real
    /// <c>Overwrite? [y/N]</c> leaves the cursor — then blocks reading stdin.</summary>
    private const string AsksAQuestion =
        """[Console]::Write("Continue? [y/N] "); $a = [Console]::In.ReadLine(); [Console]::WriteLine("got:$a"); exit 7""";

    /// <summary>Short prompt threshold so the tests are quick; generous silence threshold so slow
    /// PowerShell startup is never mistaken for the thing under test.</summary>
    private static ShellCommand Command(string command, TimeSpan? silentIdle = null) =>
        new(command,
            PromptIdle: TimeSpan.FromMilliseconds(500),
            SilentIdle: silentIdle ?? TimeSpan.FromSeconds(30));

    [Fact]
    public async Task Command_that_asks_comes_back_with_the_question_and_a_session()
    {
        using var shell = new LocalShell();

        var run = await shell.RunAsync(Command(AsksAQuestion));

        Assert.Equal(ShellStatus.WaitingForInput, run.Status);
        Assert.Contains("Continue? [y/N]", run.StandardOutput);
        Assert.NotNull(run.SessionId);
        Assert.Equal(-1, run.ExitCode);   // no exit code exists yet — the process is alive

        await shell.KillAsync(run.SessionId!);
    }

    [Fact]
    public async Task Answering_a_waiting_command_drives_it_to_completion()
    {
        using var shell = new LocalShell();

        var asked = await shell.RunAsync(Command(AsksAQuestion));
        Assert.Equal(ShellStatus.WaitingForInput, asked.Status);

        var done = await shell.ResumeAsync(asked.SessionId!, "y");

        Assert.Equal(ShellStatus.Exited, done.Status);
        Assert.Equal(7, done.ExitCode);
        Assert.Contains("got:y", done.StandardOutput);

        // The transcript is a delta, not a replay: the question was already handed over above.
        Assert.DoesNotContain("Continue?", done.StandardOutput);
    }

    [Fact]
    public async Task A_quiet_command_is_not_mistaken_for_a_question()
    {
        using var shell = new LocalShell();

        // Silent for well past PromptIdle, but it never asks anything: it must run to the end.
        var run = await shell.RunAsync(Command(
            """Start-Sleep -Milliseconds 1500; [Console]::WriteLine("finished")""",
            silentIdle: Timeout.InfiniteTimeSpan));

        Assert.Equal(ShellStatus.Exited, run.Status);
        Assert.Contains("finished", run.StandardOutput);
    }

    [Fact]
    public async Task Silence_past_the_threshold_returns_control_without_claiming_a_question()
    {
        using var shell = new LocalShell();

        var run = await shell.RunAsync(Command(
            "Start-Sleep -Seconds 30",
            silentIdle: TimeSpan.FromMilliseconds(700)));

        Assert.Equal(ShellStatus.Running, run.Status);   // quiet, not asking
        Assert.NotNull(run.SessionId);

        await shell.KillAsync(run.SessionId!);
    }

    [Fact]
    public async Task Killing_a_session_ends_it_and_forgets_it()
    {
        using var shell = new LocalShell();

        var asked = await shell.RunAsync(Command(AsksAQuestion));
        var killed = await shell.KillAsync(asked.SessionId!);

        Assert.Equal(ShellStatus.Exited, killed.Status);

        // The id is gone with the session, so a late call is a plain refusal, not a hang.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => shell.ResumeAsync(asked.SessionId!, "y"));
    }

    /// <summary>
    /// The whole point, seen from where the model actually stands: the reply carries the question,
    /// the session id, and instructions — not a spinner that never stops.
    /// </summary>
    [Fact]
    public async Task The_tool_shows_the_model_the_question_and_how_to_answer_it()
    {
        var tool = new SPLA.MCP.BasicTools.SystemTools.RunCommandTool();
        var reply = (await tool.ExecuteAsync(
            $$"""
            {"command":{{System.Text.Json.JsonSerializer.Serialize(AsksAQuestion)}},
             "cwd":null,"code_page":null,"output":"blob","output_name":"noisy"}
            """)).TextContent;

        Assert.Contains("Status: waiting_for_input", reply);
        Assert.Contains("Continue? [y/N]", reply);
        Assert.Contains("system_resume_shell", reply);

        // Asked for a blob, but an unfinished run must stay in context — a session id and a
        // question hidden behind a handle are a question nobody sees.
        Assert.DoesNotContain("blob:noisy", reply);

        var session = SessionIdIn(reply);
        var answered = (await new SPLA.MCP.BasicTools.SystemTools.ResumeShellTool().ExecuteAsync(
            $$"""{"session":"{{session}}","input":"y","output":null,"output_name":null}""")).TextContent;

        Assert.Contains("ExitCode: 7", answered);
        Assert.Contains("got:y", answered);
    }

    private static string SessionIdIn(string reply)
    {
        var match = System.Text.RegularExpressions.Regex.Match(reply, @"Session: (\S+)");
        Assert.True(match.Success, $"No session id in reply:\n{reply}");
        return match.Groups[1].Value;
    }

    [Fact]
    public async Task An_ordinary_command_still_finishes_in_a_single_call()
    {
        using var shell = new LocalShell();

        var run = await shell.RunAsync(Command("""[Console]::WriteLine("hi")"""));

        Assert.Equal(ShellStatus.Exited, run.Status);
        Assert.Equal(0, run.ExitCode);
        Assert.Contains("hi", run.StandardOutput);
        Assert.Null(run.SessionId);
    }
}
