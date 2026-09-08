using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SPLA.Domain.Settings;

/// <summary>
/// The project's monotonic per-role instance counters — the thing that hands a freshly created chat
/// its <c>as_instance</c> (<c>docs/adr/ADR_20260906_core_one-address.md</c> §2.1).
///
/// <para><b>Why a file and not a scan of the chats folder.</b> Counting the sessions on disk answers
/// "how many architects exist", but the number being handed out answers "how many have ever existed".
/// Delete <c>architect_3</c> and a scan makes the next architect the third again — while the string
/// <c>architect_3</c> is already sitting in someone else's session file, in a log line and on an edge
/// of the correspondence graph. The number is a name, and a name that comes back means two different
/// chats answered to it. So: the counter only goes up, and archiving neither changes nor frees a
/// number (trap 11 of <c>PLAN_20260906</c>: no renaming after the fact).</para>
///
/// <para><b>Why the file is re-read on every allocation.</b> Two <see cref="ChatManager"/>s over one
/// project — a service and a CLI run, or simply two tests — would otherwise each hand out the same
/// number from their own stale copy. Chats are created rarely and a read costs microseconds, so the
/// on-disk value is treated as the authority and the in-memory map only as a floor. That also makes
/// "survives a restart" true by construction rather than by care.</para>
///
/// <para>Shape mirrors <c>SPLA.Domain.Agent.FileTokenUsageStore</c>, the project's other
/// lifetime tally: a small JSON file under <c>.spla/</c>, staged to <c>.tmp</c> and renamed over the
/// target so a reader never sees half a write.</para>
/// </summary>
public sealed class RoleInstanceCounters
{
    /// <summary>File name under the project's runtime area. Plural because the file holds one counter
    /// per role, not one counter.</summary>
    public const string FileName = "role-instances.json";

    private readonly string _path;
    private readonly Func<IEnumerable<KeyValuePair<string, int>>>? _salvage;
    private readonly object _gate = new();
    private Dictionary<string, int> _last = NewMap();

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <param name="path">Full path of the counter file.</param>
    /// <param name="salvage">Highest number already visible elsewhere, per role — consulted
    /// <i>only</i> when the file exists but cannot be parsed. See <see cref="Load"/> for why a corrupt
    /// file must not silently become a zeroed one.</param>
    public RoleInstanceCounters(string path, Func<IEnumerable<KeyValuePair<string, int>>>? salvage = null)
    {
        _path = path;
        _salvage = salvage;
        lock (_gate) _last = Load();
    }

    /// <summary>Allocates the next number for <paramref name="role"/> and persists it before
    /// returning: the caller is about to stamp the number onto a chat, and a number handed out but not
    /// written is a number the next process hands out again.</summary>
    public int Next(string role)
    {
        var key = Key(role);
        lock (_gate)
        {
            // Disk wins over memory — another instance of the app may have moved the counter since we
            // loaded it. Max, never assignment: our own copy may equally be the newer one.
            var onDisk = Load();
            foreach (var pair in onDisk)
                if (!_last.TryGetValue(pair.Key, out var mine) || mine < pair.Value)
                    _last[pair.Key] = pair.Value;

            var next = (_last.TryGetValue(key, out var last) ? last : 0) + 1;
            _last[key] = next;
            Save();
            return next;
        }
    }

    /// <summary>Last number handed out for <paramref name="role"/> (0 = none yet), without allocating.
    /// Reads the in-memory copy: a diagnostic, not a reservation.</summary>
    public int Peek(string role)
    {
        lock (_gate) return _last.TryGetValue(Key(role), out var last) ? last : 0;
    }

    /// <summary>Roles are compared the way every other role lookup in the project compares them —
    /// case-insensitively, on the trimmed name.</summary>
    private static string Key(string role) => role.Trim();

    private static Dictionary<string, int> NewMap() => new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Reads the file. Two failure modes, deliberately handled differently:
    ///
    /// <para><b>Absent</b> — a project that has never named anyone. Starting at zero is the correct
    /// answer, not a fallback.</para>
    ///
    /// <para><b>Present but unreadable</b> — starting at zero here would silently re-issue names that
    /// already exist, which is the one outcome this whole class exists to prevent. So the damaged file
    /// is moved aside under a timestamped name (evidence survives; the next write starts clean) and
    /// the floor is rebuilt from <c>salvage</c> — the numbers already stamped on sessions on disk.
    /// That floor is weaker than the counter it replaces: it cannot see chats that were deleted. It is
    /// nonetheless the strongest statement available, and strictly better than pretending the project
    /// is new.</para>
    /// </summary>
    private Dictionary<string, int> Load()
    {
        if (!File.Exists(_path)) return NewMap();

        try
        {
            var dto = JsonSerializer.Deserialize<Dto>(File.ReadAllText(_path));
            if (dto?.Last != null)
            {
                var map = NewMap();
                foreach (var pair in dto.Last)
                    if (!string.IsNullOrWhiteSpace(pair.Key) && pair.Value > 0)
                        map[Key(pair.Key)] = Math.Max(map.TryGetValue(Key(pair.Key), out var have) ? have : 0, pair.Value);
                return map;
            }
        }
        catch
        {
            // Falls through to salvage below — never rethrown: a broken counter must not make the
            // project unopenable, it must only stop being trusted.
        }

        try { File.Move(_path, _path + $".corrupt-{DateTime.UtcNow:yyyyMMdd-HHmmss}", overwrite: true); }
        catch { /* best-effort: the salvaged floor is what matters, the evidence is a courtesy */ }

        var floor = NewMap();
        if (_salvage != null)
        {
            try
            {
                foreach (var pair in _salvage())
                    if (!string.IsNullOrWhiteSpace(pair.Key) && pair.Value > 0
                        && (!floor.TryGetValue(Key(pair.Key), out var have) || have < pair.Value))
                        floor[Key(pair.Key)] = pair.Value;
            }
            catch { /* a salvage that throws leaves the floor at zero, same as no salvage at all */ }
        }
        return floor;
    }

    private void Save()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(
            new Dto { Last = new Dictionary<string, int>(_last, StringComparer.OrdinalIgnoreCase), UpdatedUtc = DateTime.UtcNow },
            JsonOptions);

        // Unlike the token tally, this write is not best-effort: losing it re-issues a name. Let the
        // exception reach the caller creating the chat — failing to create a chat is recoverable,
        // creating a second chat with someone else's public name is not.
        var tmp = _path + ".tmp";
        File.WriteAllText(tmp, json);
        File.Move(tmp, _path, overwrite: true);
    }

    private sealed class Dto
    {
        /// <summary>Role → last number handed out. Role case is whatever the first caller used; lookups
        /// are case-insensitive regardless.</summary>
        public Dictionary<string, int>? Last { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }
}
