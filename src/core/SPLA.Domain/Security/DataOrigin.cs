using System;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.Domain.Security;

/// <summary>
/// Where a piece of data came from, carried along with it once it is detached from its source.
///
/// <para>The discriminator is <b>not</b> "could this text contain an injected instruction" — by that
/// test everything is suspect, including a file on your own server and a string your own stored
/// procedure returned, and a mark that is always on carries no information. It is <b>did the
/// operator name this source</b>. A configured host, a configured database, the project itself and
/// anything a person handed over are named. The open web is the one zone nobody named, and it is the
/// only one that raises doubt.</para>
/// </summary>
/// <param name="Zone">Display name of the zone: <c>project</c>, <c>internet</c>, <c>sql:prod</c>.</param>
/// <param name="OperatorNamed">False only for sources nobody configured.</param>
public sealed record DataOrigin(string Zone, bool OperatorNamed)
{
    /// <summary>Whether data from here should raise the chat's doubt flag.</summary>
    public bool RaisesDoubt => !OperatorNamed;

    /// <summary>Nothing was recorded. Treated as named — a mark applied by default would be on
    /// everywhere within minutes and would tell nobody anything.</summary>
    public static readonly DataOrigin Unrecorded = new("unrecorded", true);

    /// <summary>Files inside the project.</summary>
    public static readonly DataOrigin Project = new("project", true);

    /// <summary>The filesystem outside the project. Still this machine, still the operator's.</summary>
    public static readonly DataOrigin Machine = new("machine", true);

    /// <summary>Handed over by the person: a pasted file, a screenshot they took.</summary>
    public static readonly DataOrigin User = new("user", true);

    /// <summary>The open web, and anything reached through it. The one unnamed zone.</summary>
    public static readonly DataOrigin Internet = new("internet", false);

    /// <summary>
    /// A declared mount. Named by default — the operator wrote the folder into the manifest and said
    /// what it is for, which is the same act that names an SSH host.
    ///
    /// <para><paramref name="trusted"/> is false for the one case that genuinely differs: a folder
    /// other people put files into — a shared drop, someone else's export, a downloads directory.
    /// Making that the default instead would mean building a perimeter around your own
    /// infrastructure, which burns the question budget and buys nothing.</para>
    /// </summary>
    public static DataOrigin Mount(string name, bool trusted) => new($"mount:{name}", trusted);

    /// <summary>A configured, credentialed system — named by definition, since somebody set it up.</summary>
    public static DataOrigin Island(IslandIdentity island) => new(island.Key, true);

    /// <summary>A domain the operator put on the trusted list stops being part of the open web.</summary>
    public static DataOrigin Site(string domain, bool listed) => new($"web:{domain}", listed);

    public override string ToString() => Zone;
}

/// <summary>One thing that raised the flag, kept so the person deciding whether to clear it can see
/// what they are clearing.</summary>
/// <param name="What">What arrived — a URL, a tool name, a handle.</param>
public sealed record DoubtCause(DataOrigin Origin, string What, DateTimeOffset At);

/// <summary>
/// Whether this chat has taken in anything from a source nobody named.
///
/// <para>One bit, not a ladder: an order between "a random page" and "a foreign tool server" cannot
/// be assigned honestly, and a three-valued scale gets applied by guesswork.</para>
///
/// <para>It lives here rather than in the conversation because everything in the context is text the
/// model reads, and an instruction injected from a page can argue with text. It only ever goes up,
/// and nothing automatic takes it down — not a rollback, not a checkpoint, not a reload. A person
/// clears it, having seen <see cref="Causes"/>; there is deliberately no tool for it, since a mark
/// removable by what it guards against guards nothing.</para>
///
/// <para>What it costs while raised is exactly one thing: pushing data outward asks again even where
/// a grant exists. Reading the web and then editing project files costs nothing at all.</para>
/// </summary>
public sealed class ChatDoubt
{
    private readonly List<DoubtCause> _causes = [];
    private readonly object _lock = new();

    public bool IsRaised
    {
        get { lock (_lock) return _causes.Count > 0; }
    }

    public IReadOnlyList<DoubtCause> Causes
    {
        get { lock (_lock) return _causes.ToList(); }
    }

    /// <summary>Records an arrival. Origins the operator named are ignored, which is what keeps the
    /// flag meaningful.</summary>
    public void Observe(DataOrigin origin, string what)
    {
        if (!origin.RaisesDoubt) return;

        lock (_lock)
        {
            // The same page fetched twice is one cause; the list is for a human to read.
            if (_causes.Any(c => c.Origin == origin && c.What == what)) return;
            _causes.Add(new DoubtCause(origin, what, DateTimeOffset.Now));
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Cleared by a person who has looked at <see cref="Causes"/> and is satisfied. Does not
    /// un-send anything that already left; it only affects what gets asked from here on, which is why
    /// the cost of clearing it wrongly is bounded.</summary>
    public void Clear()
    {
        lock (_lock)
        {
            if (_causes.Count == 0) return;
            _causes.Clear();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Restores the flag from a reopened chat, so a reload does not launder it.</summary>
    public void Restore(IEnumerable<DoubtCause> causes)
    {
        lock (_lock)
        {
            _causes.Clear();
            _causes.AddRange(causes);
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Changed;
}
