using System.Text;
using SPLA.Domain.Host;

namespace SPLA.MCP.BasicTools.SystemTools;

/// <summary>
/// Renders a <see cref="ShellResult"/> for the model. Shared by the three shell tools so a session
/// looks the same however control came back to it.
/// </summary>
internal static class ShellResultText
{
    public static string Render(ShellResult run, int codePage)
    {
        var sb = new StringBuilder();

        if (run.Status == ShellStatus.Exited)
        {
            sb.Append("ExitCode: ").Append(run.ExitCode).Append('\n');
        }
        else
        {
            // No exit code exists yet, and printing a fake 0 would read as success.
            sb.Append("Status: ").Append(StatusText(run.Status)).Append('\n');
            sb.Append("Session: ").Append(run.SessionId).Append('\n');
        }

        sb.Append("CodePage: ").Append(codePage).Append('\n');
        sb.Append("Output:\n").Append(run.StandardOutput).Append('\n');
        sb.Append("Error:\n").Append(run.StandardError);

        var hint = Hint(run.Status);
        if (hint is not null) sb.Append("\n\n").Append(hint);

        return sb.ToString();
    }

    public static string StatusText(ShellStatus status) => status switch
    {
        ShellStatus.Exited => "exited",
        ShellStatus.WaitingForInput => "waiting_for_input",
        _ => "running"
    };

    private static string? Hint(ShellStatus status) => status switch
    {
        ShellStatus.WaitingForInput =>
            "The command has not exited and its last line has no line break — it is almost certainly "
            + "asking you something (see Output above). Send the answer with system_resume_shell, or "
            + "end it with system_kill_shell. Do not start the command again: this one is still running.",
        ShellStatus.Running =>
            "The command is still running and simply produced nothing for a while — this is not a "
            + "question. Call system_resume_shell with no input to keep waiting, or system_kill_shell "
            + "to give up on it.",
        _ => null
    };
}
