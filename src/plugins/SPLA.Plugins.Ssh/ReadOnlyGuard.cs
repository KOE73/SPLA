namespace SPLA.Plugins.Ssh;

/// <summary>
/// Enforces "read-only, no destructive commands" for the SSH plugin. This is a deliberately
/// conservative <b>allowlist</b>: every pipeline segment must start with a known read-only command,
/// and the whole line is rejected if it contains anything that can mutate state — output redirection,
/// command substitution, or privilege escalation. An allowlist fails closed (an unknown command is
/// refused, not run), which is the right default when the operator on the other end is a live server.
///
/// <para>Scope of protection and its limits: this blocks the obvious destructive/mutating paths
/// (rm, dd, mkfs, mv, kill, shutdown, package installs, <c>&gt;</c> redirection, <c>sudo</c>). It is
/// NOT a sandbox — a read-only command can still read sensitive files it has access to. The guard's
/// job is preventing <em>change</em>, not preventing <em>reading</em>; the account's own permissions
/// bound what can be read.</para>
/// </summary>
internal static class ReadOnlyGuard
{
    // First-token allowlist. Commands here cannot, by themselves, mutate the remote system.
    // Intentionally excludes awk/sed/perl/python/xargs (arbitrary code / in-place edit) and anything
    // that writes. Grows by review, not by guesswork.
    private static readonly HashSet<string> Allowed = new(StringComparer.Ordinal)
    {
        // shell/session state — navigation and env only; change the shell, not the system. Essential
        // for a persistent session (cd must carry over) and harmless read-only-wise.
        "cd", "pushd", "popd", "dirs", "export",
        // identity / system info
        "uname", "hostname", "whoami", "id", "uptime", "date", "w", "who", "last",
        "lsb_release", "arch", "nproc", "getconf", "locale", "tty",
        // files & directories (read)
        "ls", "cat", "head", "tail", "stat", "file", "wc", "du", "df", "pwd",
        "readlink", "realpath", "basename", "dirname", "tree", "find", "namei",
        // text inspection (no code execution, no in-place write)
        "grep", "egrep", "fgrep", "zgrep", "sort", "uniq", "cut", "tr", "nl", "rev",
        "column", "cmp", "diff", "comm", "fold", "expand", "unexpand", "strings",
        "md5sum", "sha1sum", "sha256sum", "cksum", "echo", "printf", "seq", "yes",
        // processes / resources (read)
        "ps", "free", "vmstat", "iostat", "mpstat", "lscpu", "lsblk", "lsmem",
        "lspci", "lsusb", "lsof", "lsmod", "dmidecode", "sensors", "env", "printenv",
        // network (read / diagnostic)
        "ip", "ifconfig", "netstat", "ss", "route", "arp", "ping", "ping6",
        "traceroute", "tracepath", "dig", "nslookup", "host", "getent", "curl", "wget",
        // packages / versions (query only)
        "dpkg", "rpm", "which", "whereis", "type", "command", "apt-cache",
        "systemctl", "journalctl", "service",
    };

    // Query-only commands that become mutating with the wrong flag. Reject these specific flags.
    private static readonly Dictionary<string, string[]> BannedFlags = new(StringComparer.Ordinal)
    {
        ["find"]      = new[] { "-delete", "-exec", "-execdir", "-ok", "-okdir", "-fprint", "-fprintf", "-fls" },
        ["systemctl"] = new[] { "start", "stop", "restart", "reload", "enable", "disable", "mask", "unmask", "kill", "set-property", "edit" },
        ["service"]   = new[] { "start", "stop", "restart", "reload", "force-reload" },
        ["curl"]      = new[] { "-o", "-O", "--output", "--remote-name", "-T", "--upload-file", "-d", "--data", "-X", "--request" },
        ["wget"]      = new[] { "-O", "--output-document", "-r", "--recursive", "--post-data", "--post-file" },
        ["dpkg"]      = new[] { "-i", "--install", "-r", "--remove", "-P", "--purge", "--unpack", "--configure" },
        ["rpm"]       = new[] { "-i", "--install", "-e", "--erase", "-U", "--upgrade", "-F", "--freshen" },
        ["ip"]        = new[] { "add", "del", "delete", "set", "change", "replace", "flush" },
        ["journalctl"] = new[] { "--flush", "--rotate", "--vacuum-size", "--vacuum-time", "--vacuum-files" },
    };

    // Metacharacters that enable writing or arbitrary execution regardless of the command.
    private static readonly string[] BannedTokens = { ">", ">>", "<<", "`", "$(", "${", "&>", "|&" };

    /// <summary>Returns null when the command is allowed, or a human-readable reason when it is refused.</summary>
    public static string? Reject(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return "empty command";

        foreach (var bad in BannedTokens)
            if (command.Contains(bad, StringComparison.Ordinal))
                return $"contains '{bad}' — redirection/substitution is not allowed in read-only mode";

        // Split into pipeline/sequence segments; every segment's leading command must be allowlisted.
        var segments = command.Split(new[] { "|", "||", "&&", ";", "&", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
            return "empty command";

        foreach (var raw in segments)
        {
            var tokens = raw.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) continue;

            var cmd = tokens[0];

            // Reject inline env assignment prefix (VAR=val cmd) — hides the real command.
            if (cmd.Contains('=', StringComparison.Ordinal))
                return $"inline assignment '{cmd}' is not allowed";

            if (cmd.Equals("sudo", StringComparison.Ordinal) || cmd.Equals("su", StringComparison.Ordinal) || cmd.Equals("doas", StringComparison.Ordinal))
                return "privilege escalation (sudo/su/doas) is not allowed in read-only mode";

            if (!Allowed.Contains(cmd))
                return $"'{cmd}' is not on the read-only allowlist";

            if (BannedFlags.TryGetValue(cmd, out var banned))
            {
                foreach (var t in tokens.Skip(1))
                    if (banned.Contains(t, StringComparer.Ordinal))
                        return $"'{cmd} {t}' is a mutating operation, not allowed in read-only mode";
            }
        }

        return null;
    }
}
