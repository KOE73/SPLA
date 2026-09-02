using System.Text;

namespace SPLA.Runtime;

/// <summary>
/// Turns a role name and a correspondence topic into the virtual <c>reply_&lt;role&gt;[_&lt;topic&gt;]</c>
/// tool name — see <c>docs/adr/ADR_20260827-2_core_roles.md</c> §2.3 ("имя инструмента есть адрес").
/// A pure string transform, deliberately: the decision of WHETHER the topic joins the name (only when
/// this chat already holds another correspondent of the same role) is made once, at
/// <see cref="ChatRuntime.OpenCorrespondence"/> time, and stored on <see cref="Correspondence.ToolName"/>
/// rather than recomputed here — this class only knows how to spell a name, not when to use which shape.
/// </summary>
internal static class ReplyToolNaming
{
    /// <summary>Builds the stable name for a freshly-opened correspondence.</summary>
    public static string BuildToolName(string role, string topic, bool includeTopic) =>
        includeTopic
            ? $"reply_{Normalize(role)}_{Normalize(topic)}"
            : $"reply_{Normalize(role)}";

    /// <summary>
    /// Normalises free text into a tool-name-safe identifier: lower-case ASCII letters, digits and
    /// underscores, every other run of characters collapsed to a single underscore, leading/trailing
    /// underscores trimmed. Never returns an empty string — a normalised-to-nothing input (all
    /// punctuation, all non-ASCII) falls back to <c>"x"</c> rather than producing a malformed
    /// <c>reply_</c>/<c>reply__</c> name.
    /// </summary>
    public static string Normalize(string s)
    {
        var sb = new StringBuilder(s.Length);
        var lastWasUnderscore = false;
        foreach (var ch in s.Trim().ToLowerInvariant())
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
}
