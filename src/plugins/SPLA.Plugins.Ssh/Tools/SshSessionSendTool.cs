using System.Text;
using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// <c>ssh_session_send</c> — sends raw keys into a live session's pty without waiting for a result:
/// answers an interactive prompt, quits a pager (<c>q</c>), or interrupts a running command
/// (<c>ctrl_c</c>). On read-only hosts (no <c>allow_write</c>) only the safe control keys are
/// allowed — arbitrary raw input would bypass the read-only guard, an interrupt cannot mutate.
/// </summary>
internal sealed class SshSessionSendTool : IMcpTool
{
    private readonly SshSessionHub _hub;
    private readonly SessionToolContext _ctx;

    public SshSessionSendTool(SshSessionHub hub, SessionToolContext ctx)
    {
        _hub = hub;
        _ctx = ctx;
    }

    public string Name => "ssh_session_send";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description =
                "Sends raw keys into a live SSH session: answer an interactive prompt, press q to leave a " +
                "pager, ctrl_c to interrupt a running command. " +
                "ctrl_c is handled specially and IS the way to free a session whose command will not end: it " +
                "interrupts, checks that the shell came back and reports one of INTERRUPTED / ALREADY FINISHED / " +
                "CTRL+C DID NOT FREE THE SHELL — after INTERRUPTED the session is immediately usable again, so " +
                "go straight to ssh_session_exec and do not call ssh_session_wait. " +
                "Ctrl+C does NOT quit full-screen programs (less, top, vim, installers): if you get " +
                "CTRL+C DID NOT FREE THE SHELL, send q (pager/top) rather than repeating ctrl_c. " +
                "Special names: ctrl_c, ctrl_d, enter, esc — " +
                "anything else is sent literally (kept verbatim, no trimming). To answer a prompt or pick a menu " +
                "item that requires confirmation, set enter=true so a newline is appended after the text — e.g. " +
                "keys='y' enter=true, or keys='23' enter=true for a numbered menu. Leave enter=false (default) for " +
                "menus that react to a single keystroke with no Enter (e.g. a one-key y/n or arrow navigation). " +
                "On hosts without allow_write only ctrl_c/ctrl_d/q/enter/esc are permitted. " +
                "Watch the effect in the terminal / with ssh_session_exec.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    keys = new { type = "string", description = "What to send: literal text, or ctrl_c / ctrl_d / enter / esc / q. Sent verbatim (not trimmed)." },
                    enter = new { type = "boolean", description = "Append a newline (Enter) after the text (default false). Use true to submit a typed answer / numbered menu choice." },
                    session = new { type = "string", description = "Session id (host#N). Omit to use the sole open session." }
                },
                required = new[] { "keys" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string keys;
        string? sessionId;
        var appendEnter = false;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            // Sent verbatim — do NOT trim. GetStringTrimmed() used to strip the trailing '\n', so
            // keys="y\n" arrived as "y": the Enter was lost, the prompt stayed unanswered and the
            // session hung. A trailing newline here is a real Enter keystroke and must survive.
            keys = ToolJson.GetString(doc.RootElement, "keys") ?? "";
            sessionId = ToolJson.GetStringTrimmed(doc.RootElement, "session");
            if (doc.RootElement.TryGetProperty("enter", out var e)
                && e.ValueKind is JsonValueKind.True or JsonValueKind.False)
                appendEnter = e.GetBoolean();
        }
        catch (JsonException)
        {
            return "Error: invalid JSON arguments.";
        }

        if (keys.Length == 0) return "Error: 'keys' is required.";

        var open = _hub.List();
        sessionId ??= open.Count == 1 ? open[0].Id : null;
        if (sessionId == null)
            return open.Count == 0
                ? "Error: no open SSH sessions."
                : "Error: multiple sessions open — specify 'session'. Open: " + string.Join(", ", open.Select(s => s.Id));

        var (session, cfg, error) = await _ctx.ResolveAsync(_hub, sessionId, null, "agent", cancellationToken);
        if (session == null || cfg == null) return error ?? "Error: session not found.";

        var payload = keys switch
        {
            "ctrl_c" => "\x03",
            "ctrl_d" => "\x04",
            "enter"  => "\n",
            "esc"    => "\x1b",
            _        => keys
        };

        // Read-only hosts: only non-mutating control keys — raw text input would bypass the guard.
        // A bare Enter after 'q'/'enter'/'esc' is harmless; typed text (keys not a control name) is not.
        var safe = keys is "ctrl_c" or "ctrl_d" or "enter" or "esc" or "q";
        if (!cfg.AllowWrite && !safe)
            return "Refused (read-only host): only ctrl_c / ctrl_d / enter / esc / q may be sent here. " +
                   "Set allow_write on the host to lift.";

        if (appendEnter && !payload.EndsWith('\n'))
            payload += "\n";

        // ctrl_c is NOT a keystroke like the others: SIGINT destroys the end marker of whatever the
        // agent was running, so writing \x03 blindly would leave the session's pending run un-finishable
        // and every later ssh_session_exec refusing with "a command is still running". InterruptAsync
        // interrupts, verifies the shell returned, and settles the run — and tells us which of those
        // actually happened, so the answer below can never claim a success that didn't occur.
        if (keys == "ctrl_c")
        {
            var pending = session.PendingCommand;
            var res = await session.InterruptAsync(cancellationToken);
            var sb = new StringBuilder();
            sb.AppendLine($"[{session.Id}] Ctrl+C" + (pending is null ? "" : $" → {pending}"));
            sb.AppendLine(res.Status switch
            {
                "interrupted" => "INTERRUPTED — the command was stopped and the shell is back at a prompt. " +
                                 "The session is free: run the next command with ssh_session_exec.",
                "done" => $"ALREADY FINISHED before the interrupt landed — exit: {res.ExitCode?.ToString() ?? "?"}. " +
                          "The session is free.",
                "idle" => "Nothing was running. The shell is at a prompt and the session is free.",
                "still-busy" => "CTRL+C DID NOT FREE THE SHELL — a full-screen program still owns the terminal " +
                                "(a pager such as less, or top/vim/an installer). Ctrl+C does not quit those. " +
                                "Send keys='q' next (pagers, top), or keys='esc' then ':q!' with enter=true (vim). " +
                                "Do NOT send ctrl_c again — it will do exactly the same nothing. If nothing helps, " +
                                "ssh_session_exec with force=true abandons the stuck command, or ssh_session_close " +
                                "ends the session.",
                "disconnected" => "CONNECTION CLOSED BY REMOTE — the session is gone. Open a new one to reconnect.",
                _ => res.Status
            });
            if (!string.IsNullOrWhiteSpace(res.NewOutput)) sb.AppendLine(res.NewOutput.TrimEnd());
            return sb.ToString().TrimEnd();
        }

        session.Write(payload);
        return $"Sent to '{session.Id}'." + (session.PendingCommand is null
            ? " Nothing is running in this session — check the effect with ssh_session_exec."
            : $" '{session.PendingCommand}' is still pending here — read the effect with ssh_session_wait.");
    }
}
