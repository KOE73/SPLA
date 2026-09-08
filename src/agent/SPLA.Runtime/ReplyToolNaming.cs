using System.Text;

namespace SPLA.Runtime;

/// <summary>
/// Turns a role name and an instance number into the virtual <c>reply_&lt;role&gt;[_&lt;n&gt;]</c>
/// tool name — see <c>docs/adr/ADR_20260827-2_core_roles.md</c> §2.3 ("имя инструмента есть адрес")
/// and <c>docs/plans/PLAN_20260906_core_chat-directory-and-await.md</c> §2.1/2.3: the address is a
/// system-issued ordinal, never the model-supplied <c>purpose</c> text — a model reads the name off
/// the tool list every turn, it never has to reproduce it, so there is nothing left for a topic-based
/// name to buy and a whole class of "pересказ/opечатка founds a new collision" bugs to cause.
/// A pure string transform, deliberately: the decision of WHICH ordinal a chat gets is made once, by
/// the project's per-role counter (<c>SPLA.Domain.Settings.RoleInstanceCounters</c>), and stored on
/// <c>ChatSession.AsInstance</c>; a correspondence merely copies it
/// (<c>docs/adr/ADR_20260906_core_one-address.md</c> §2.1). This class only knows how to spell a name,
/// not which number to hand out.
/// </summary>
internal static class ReplyToolNaming
{
    /// <summary>
    /// The chat's <b>public name</b>: <c>&lt;role&gt;_&lt;n&gt;</c> — the one string that stands for a
    /// chat everywhere it is spoken about (ADR_20260906 §2.2: the chat directory, the argument of
    /// <c>agent_correspond</c>, the reply tool's name, the log line, the graph). Spelled here and
    /// nowhere else, so that the four windows cannot drift apart.
    /// <para><b>Why the first instance is <c>architect_1</c> and not bare <c>architect</c>.</b> Two
    /// reasons, and the second is decisive. First: since ADR_20260906 the ordinal comes from a
    /// project-wide monotonic counter, so "1" is no longer "the default one this chat happens to
    /// have" — it is a specific chat among all the architects the project ever made, and dropping the
    /// number from it would be as arbitrary as dropping it from the seventh. Second: §2.3 makes
    /// <c>agent_correspond</c> accept either a role name or a public name, and a bare <c>architect</c>
    /// would be both at once — the role path wins by necessity (it is the older, declared meaning),
    /// which would leave the very first architect the one chat nobody can address by name. That is
    /// precisely the hole the ADR exists to close.
    /// <para>The price, paid knowingly: the old spelling <c>reply_architect</c> for a first instance is
    /// gone. Names already written into session files are NOT rewritten — <c>ChatRuntime</c>'s restore
    /// path replays the persisted <c>tool_name</c> verbatim (plan trap 11), so an old chat keeps
    /// calling its architect <c>reply_architect</c> for as long as that correspondence lives.</para>
    /// </para>
    /// </summary>
    public static string BuildPublicName(string role, int instanceNo) =>
        $"{Normalize(role)}_{(instanceNo < 1 ? 1 : instanceNo)}";

    /// <summary>Builds the stable name for a correspondence — the correspondent's public name with
    /// <c>reply_</c> in front, and nothing else: one spelling, one place
    /// (<see cref="BuildPublicName"/>), so "the name in the tool list" and "the name in the directory"
    /// are the same string by construction rather than by two functions agreeing.</summary>
    public static string BuildToolName(string role, int instanceNo) =>
        "reply_" + BuildPublicName(role, instanceNo);

    /// <summary>
    /// Normalises free text into a tool-name-safe identifier: common Cyrillic letters are
    /// transliterated to their Latin spelling first (PLAN_20260906 §3 wave 0: "транслитерация вместо
    /// вычищения") so a Russian role name still reads as itself instead of collapsing to a placeholder;
    /// everything left is lower-cased to ASCII letters, digits and underscores, every other run of
    /// characters collapsed to a single underscore, leading/trailing underscores trimmed. Never returns
    /// an empty string — text that is still empty after transliteration (all punctuation, emoji, a
    /// script this table does not cover) falls back to <c>"x"</c> rather than producing a malformed
    /// <c>reply_</c>/<c>reply__</c> name; <see cref="ChatRuntime.OpenCorrespondence"/> is what actually
    /// guarantees uniqueness when that fallback (or any other collision) would otherwise hand two
    /// correspondences the same name.
    /// </summary>
    public static string Normalize(string s)
    {
        var sb = new StringBuilder(s.Length);
        var lastWasUnderscore = false;
        foreach (var ch in Transliterate(s.Trim()).ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(ch))
            {
                sb.Append(ch);
                lastWasUnderscore = false;
            }
            else if (!lastWasUnderscore && sb.Length > 0)
            {
                sb.Append('_');
                lastWasUnderscore = true;
            }
        }
        while (sb.Length > 0 && sb[^1] == '_') sb.Length--;

        return sb.Length == 0 ? "x" : sb.ToString();
    }

    /// <summary>Practical Cyrillic→Latin transliteration (не ГОСТ/ISO 9, а читаемое написание), letter
    /// by letter — good enough for a tool name, not for a passport. Anything not in the table (Latin
    /// letters, digits, punctuation, other scripts) passes through unchanged for <see cref="Normalize"/>
    /// to deal with.</summary>
    private static string Transliterate(string s)
    {
        var sb = new StringBuilder(s.Length * 2);
        foreach (var ch in s)
            sb.Append(CyrillicToLatin.TryGetValue(ch, out var t) ? t : ch.ToString());
        return sb.ToString();
    }

    private static readonly Dictionary<char, string> CyrillicToLatin = new()
    {
        ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['д'] = "d", ['е'] = "e", ['ё'] = "yo",
        ['ж'] = "zh", ['з'] = "z", ['и'] = "i", ['й'] = "y", ['к'] = "k", ['л'] = "l", ['м'] = "m",
        ['н'] = "n", ['о'] = "o", ['п'] = "p", ['р'] = "r", ['с'] = "s", ['т'] = "t", ['у'] = "u",
        ['ф'] = "f", ['х'] = "h", ['ц'] = "ts", ['ч'] = "ch", ['ш'] = "sh", ['щ'] = "sch", ['ъ'] = "",
        ['ы'] = "y", ['ь'] = "", ['э'] = "e", ['ю'] = "yu", ['я'] = "ya",
        ['А'] = "A", ['Б'] = "B", ['В'] = "V", ['Г'] = "G", ['Д'] = "D", ['Е'] = "E", ['Ё'] = "Yo",
        ['Ж'] = "Zh", ['З'] = "Z", ['И'] = "I", ['Й'] = "Y", ['К'] = "K", ['Л'] = "L", ['М'] = "M",
        ['Н'] = "N", ['О'] = "O", ['П'] = "P", ['Р'] = "R", ['С'] = "S", ['Т'] = "T", ['У'] = "U",
        ['Ф'] = "F", ['Х'] = "H", ['Ц'] = "Ts", ['Ч'] = "Ch", ['Ш'] = "Sh", ['Щ'] = "Sch", ['Ъ'] = "",
        ['Ы'] = "Y", ['Ь'] = "", ['Э'] = "E", ['Ю'] = "Yu", ['Я'] = "Ya"
    };
}
