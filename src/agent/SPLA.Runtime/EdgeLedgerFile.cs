using System;
using System.Collections.Generic;
using System.Linq;
using SPLA.Domain.Project;
using SPLA.Domain.Security;
using SPLA.MCP.Core.Security;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Runtime;

/// <summary>
/// Keeps the edge ledger across runs, under <c>.spla/</c>.
///
/// <para>Shadow mode measures a week of ordinary work, and a reading that dies at exit measures
/// nothing — restarting the app was resetting the only evidence the enforcement defaults were meant
/// to come from.</para>
///
/// <para>Location does the work the earlier reluctance to persist was trying to do: <c>.spla/</c> is
/// inside the boundary's cutout, so the agent cannot read its own account of itself; it is not in
/// git, so nobody else gets it with a clone; and it is one file, so a person who does not want it
/// deletes it.</para>
/// </summary>
public sealed class EdgeLedgerFile
{
    private const string Key = "edges.yaml";

    private readonly IBucket _bucket;
    private readonly EdgeLedger _ledger;

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public EdgeLedgerFile(IBucket bucket, EdgeLedger ledger)
    {
        _bucket = bucket;
        _ledger = ledger;

        Load();
        // Subscribed after loading, so restoring does not immediately write back what was just read.
        _ledger.Persist += (_, _) => Save();
    }

    private void Load()
    {
        var yaml = _bucket.ReadText(Key);
        if (string.IsNullOrWhiteSpace(yaml)) return;

        try
        {
            var rows = Deserializer.Deserialize<List<PersistedEdge>>(yaml);
            if (rows is not { Count: > 0 }) return;

            _ledger.Restore(rows
                .Where(r => r is { Source: not null, Sink: not null })
                .Select(r => new EdgeTraffic(
                    new ZoneEdge(ParseZone(r.Source!), ParseZone(r.Sink!), ParseEffect(r.Effect)),
                    r.Calls,
                    r.FirstSeen,
                    r.LastSeen,
                    r.LastTool ?? string.Empty)));
        }
        catch
        {
            // A record we cannot read is a record we start again; it is a reading, not an account
            // anybody depends on.
        }
    }

    private void Save()
    {
        try
        {
            _bucket.WriteText(Key, Serializer.Serialize(_ledger.List()
                .Select(t => new PersistedEdge
                {
                    Source = t.Edge.Source.ToString(),
                    Sink = t.Edge.Sink.ToString(),
                    Effect = t.Edge.Effect.ToString().ToLowerInvariant(),
                    Calls = t.Calls,
                    FirstSeen = t.FirstSeen,
                    LastSeen = t.LastSeen,
                    LastTool = t.LastTool
                })
                .ToList()));
        }
        catch { /* best effort — losing the reading must never cost the call */ }
    }

    /// <summary>Splits <c>kind:instance</c> back apart. Instances may contain colons (a fingerprint
    /// does not, but a host name with a port would), so only the first one separates.</summary>
    private static Zone ParseZone(string text)
    {
        var i = text.IndexOf(':');
        return i < 0 ? new Zone(text) : new Zone(text[..i], text[(i + 1)..]);
    }

    private static ZoneEffect ParseEffect(string? text) =>
        Enum.TryParse<ZoneEffect>(text, ignoreCase: true, out var effect) ? effect : ZoneEffect.Read;

    /// <summary>One row as it sits on disk. Deliberately readable: this is a file a person opens to
    /// decide what to allow, not a cache.</summary>
    private sealed class PersistedEdge
    {
        [YamlMember(Alias = "source")] public string? Source { get; set; }
        [YamlMember(Alias = "sink")] public string? Sink { get; set; }
        [YamlMember(Alias = "effect")] public string? Effect { get; set; }
        [YamlMember(Alias = "calls")] public int Calls { get; set; }
        [YamlMember(Alias = "first_seen")] public DateTimeOffset FirstSeen { get; set; }
        [YamlMember(Alias = "last_seen")] public DateTimeOffset LastSeen { get; set; }
        [YamlMember(Alias = "last_tool")] public string? LastTool { get; set; }
    }
}
