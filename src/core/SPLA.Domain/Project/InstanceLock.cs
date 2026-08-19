using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPLA.Domain.Project;

/// <summary>
/// What a live instance publishes about itself: enough to refuse a second writer, and enough to
/// tell whoever was refused where to connect instead.
/// </summary>
public sealed class InstanceInfo
{
    /// <summary>Identity of this instance, independent of the OS. Checked against <c>/health</c>
    /// before trusting an endpoint, because a port can be reused by something else entirely.</summary>
    public string InstanceId { get; set; } = "";

    /// <summary>Where clients connect, once the instance knows. Null while it is not serving —
    /// a REPL or an MCP session holds the project without offering an address.</summary>
    public string? Endpoint { get; set; }

    /// <summary>How this instance is using the project: <c>serve</c>, <c>repl</c>, <c>mcp</c>,
    /// <c>chat-run</c>, <c>ui</c>. Shown when a second one is refused, so the message names what is
    /// actually holding it rather than "something".</summary>
    public string Mode { get; set; } = "";

    /// <summary>The machine holding it. A project on a network share can be opened from two
    /// computers, and only one of them can act on a pid.</summary>
    public string Machine { get; set; } = "";

    public int Pid { get; set; }

    /// <summary>Process start time, paired with <see cref="Pid"/>. The OS reuses pid numbers; the
    /// pair does not.</summary>
    public DateTimeOffset StartedAt { get; set; }

    [JsonIgnore]
    public bool IsLocal => string.Equals(Machine, Environment.MachineName, StringComparison.OrdinalIgnoreCase);

    /// <summary>One line naming this instance, for a refusal message.</summary>
    public string Describe()
    {
        var where = Endpoint is { Length: > 0 } ? Endpoint : "no endpoint";
        var who = IsLocal ? $"pid {Pid}" : $"{Machine}, pid {Pid}";
        return $"{Mode} ({who}, {where}, since {StartedAt.LocalDateTime:t})";
    }
}

/// <summary>Thrown when a project already has a writer.</summary>
public sealed class ProjectBusyException : Exception
{
    public ProjectBusyException(InstanceInfo holder, string message) : base(message) => Holder = holder;

    /// <summary>The instance that holds the project — carries the endpoint to connect to instead.</summary>
    public InstanceInfo Holder { get; }
}

/// <summary>
/// The one-writer invariant, and the instance directory, in a single file:
/// <c>&lt;project&gt;/.spla/instance.json</c>.
///
/// <para><b>Why one file for two jobs.</b> "Nobody else may write here" and "here is where the live
/// one listens" are the same fact seen from two sides. Splitting them would mean two files that can
/// disagree, and the disagreement would show up exactly when it hurts — a stale address pointing at
/// a process that already let go.</para>
///
/// <para><b>Why the OS decides liveness.</b> The holder keeps the file open with write denied for as
/// long as it lives, so a reader answers "is it alive?" by trying to open it for writing. Windows
/// closes handles on any death, including a kill and a crash, which no heartbeat or pid check
/// survives as cleanly: pid numbers get reused, and a pid+start-time pair still cannot be checked
/// from another machine. Over SMB the same deny-write is honoured, so a project on a share is
/// protected from two computers, not just from two processes on one.</para>
///
/// <para><b>Why it lives beside the project.</b> The thing being protected is the project's
/// <c>.spla/</c> — its chats, its KV, its usage tally. A registry in the machine's home directory
/// would protect nothing on a share and would need pruning; this file is deleted by the OS letting
/// go of it.</para>
///
/// <para>Running without a manifest takes no lock at all: there is no project to protect, and the
/// runtime area is then the shared home, which is not one project's to claim.</para>
/// </summary>
public sealed class InstanceLock : IDisposable
{
    public const string FileName = "instance.json";

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly FileStream _stream;
    private readonly object _gate = new();

    public InstanceInfo Info { get; }
    public string Path { get; }

    private InstanceLock(FileStream stream, string path, InstanceInfo info)
    {
        _stream = stream;
        Path = path;
        Info = info;
        Write();
    }

    /// <summary>
    /// Claims <paramref name="runtimeDir"/> for this process.
    /// </summary>
    /// <param name="runtimeDir">The project's runtime directory (<c>&lt;project&gt;/.spla</c>).</param>
    /// <param name="mode">What this instance is doing with the project — see <see cref="InstanceInfo.Mode"/>.</param>
    /// <exception cref="ProjectBusyException">Someone already holds it.</exception>
    public static InstanceLock Acquire(string runtimeDir, string mode)
    {
        var path = System.IO.Path.Combine(runtimeDir, FileName);
        Directory.CreateDirectory(runtimeDir);

        FileStream stream;
        try
        {
            // FileShare.Read, not None: a refused caller must still be able to read the address it is
            // being redirected to. Denying writes is the whole claim.
            stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        }
        catch (IOException)
        {
            var holder = ReadFile(path) ?? new InstanceInfo { Mode = "unknown", Machine = "?" };
            throw new ProjectBusyException(holder,
                $"This project is already open: {holder.Describe()}. Connect to it instead of opening a second writer.");
        }

        var process = System.Diagnostics.Process.GetCurrentProcess();
        var info = new InstanceInfo
        {
            InstanceId = Guid.NewGuid().ToString("N"),
            Mode = mode,
            Machine = Environment.MachineName,
            Pid = process.Id,
            StartedAt = process.StartTime.ToUniversalTime()
        };
        return new InstanceLock(stream, path, info);
    }

    /// <summary>
    /// Claims the project, or returns null when it is already held — for callers whose reaction to a
    /// busy project is to connect to it rather than to fail.
    /// </summary>
    public static InstanceLock? TryAcquire(string runtimeDir, string mode, out InstanceInfo? holder)
    {
        try
        {
            holder = null;
            return Acquire(runtimeDir, mode);
        }
        catch (ProjectBusyException ex)
        {
            holder = ex.Holder;
            return null;
        }
    }

    /// <summary>
    /// The instance holding <paramref name="runtimeDir"/>, or null when nobody does.
    ///
    /// <para>A file left behind by a process that died is indistinguishable from a live one by its
    /// contents, so the contents are not what is asked: the check is whether the file can be opened
    /// for writing. It can only be opened if nobody is holding it.</para>
    /// </summary>
    public static InstanceInfo? Read(string runtimeDir)
    {
        var path = System.IO.Path.Combine(runtimeDir, FileName);
        if (!File.Exists(path)) return null;

        try
        {
            using var probe = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.Read);
            return null;   // opened for writing ⇒ nobody holds it ⇒ the record is a leftover
        }
        catch (IOException)
        {
            return ReadFile(path);   // held: whoever holds it is alive
        }
        catch (UnauthorizedAccessException)
        {
            // Read-only file or no write permission — cannot prove liveness either way. Report what
            // it says rather than claiming the project is free; the caller verifies the endpoint.
            return ReadFile(path);
        }
    }

    private static InstanceInfo? ReadFile(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return JsonSerializer.Deserialize<InstanceInfo>(stream, Json);
        }
        catch { return null; }
    }

    /// <summary>Publishes the address clients should use. Called once the listener is actually up —
    /// an endpoint written before it accepts connections is a lie for as long as startup takes.</summary>
    public void Publish(string endpoint)
    {
        lock (_gate)
        {
            Info.Endpoint = endpoint;
            Write();
        }
    }

    private void Write()
    {
        _stream.SetLength(0);
        _stream.Position = 0;
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Info, Json));
        _stream.Write(bytes, 0, bytes.Length);
        _stream.Flush(flushToDisk: true);
    }

    /// <summary>Releases the claim and removes the record. The OS would release the handle anyway —
    /// deleting the file too just keeps the directory honest for a human reading it.</summary>
    public void Dispose()
    {
        try { _stream.Dispose(); } catch { /* already gone */ }
        try { File.Delete(Path); } catch { /* another process may have taken it already */ }
    }
}
