using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace SPLA.UI.Avalonia.Helpers;

/// <summary>
/// The native frame's share of the UI language, on the same terms as the web client: the key is the
/// English source text, so a string with no translation simply stays as it was written.
///
/// The chrome around the webview is a handful of icon buttons and their tooltips, so this does not
/// take the usual route of naming every control and binding it. It walks the tree and rewrites the
/// tooltips it finds, remembering each one's original English in a side table — that is what lets
/// the language be switched back and forth (a second pass would otherwise be looking up a Russian
/// string and finding nothing). The upshot is that a tooltip added to the XAML later is localized
/// with no code change: add the row to <see cref="Ru"/> and it is picked up on the next pass.
///
/// The language itself is never decided here. It is chosen in the web client's settings (that is the
/// only place a person can pick it) and arrives over the webview bridge — see WebViewBridge's
/// <c>lang</c> message and web/src/i18n/index.ts.
/// </summary>
public static class Localization
{
    /// <summary>Native-chrome strings only. Everything the person reads inside the webview is
    /// translated by web/src/i18n/ru.json; these are the few that never reach it.</summary>
    private static readonly Dictionary<string, string> Ru = new(StringComparer.Ordinal)
    {
        ["New / Open / Recent project"] = "Новый / Открыть / Недавние проекты",
        ["Settings (connections / agent / plugins)"] = "Настройки (соединения / агент / плагины)",
        ["Debug (memory / context / prompt) — opens as a separate window"] = "Отладка (память / контекст / промпт) — открывается отдельным окном",
        ["Wire (live protocol monitor) — opens as a separate window"] = "Провод (живой монитор протокола) — открывается отдельным окном",
        ["Open current location in browser"] = "Открыть текущий адрес в браузере",
        ["Reload web client"] = "Перезагрузить веб-клиент",
        ["Reload"] = "Перезагрузить",
        ["Minimize"] = "Свернуть",
        ["Maximize"] = "Развернуть",
        ["Restore"] = "Восстановить",
        ["Close"] = "Закрыть",
    };

    private static readonly ConditionalWeakTable<Control, string> Originals = new();
    private static readonly List<WeakReference<Control>> Roots = new();

    public static string Current { get; private set; } = "en";

    /// <summary>Translates one string; an unknown key is its own translation.</summary>
    public static string T(string text) =>
        Current == "ru" && Ru.TryGetValue(text, out var ru) ? ru : text;

    /// <summary>Localizes <paramref name="root"/>'s tooltips now, and again whenever the language
    /// changes. Call once per window, after its content exists.</summary>
    public static void Track(Control root)
    {
        Roots.Add(new WeakReference<Control>(root));
        Apply(root);
    }

    /// <summary>Sets the UI language and re-runs every tracked window. Called from the webview
    /// bridge when the web client reports which language it is showing.</summary>
    public static void SetLanguage(string? lang)
    {
        var next = string.IsNullOrWhiteSpace(lang) ? "en" : lang!.Trim().ToLowerInvariant();
        if (next == Current) return;
        Current = next;

        // Windows that have since closed are dropped on the way past — nothing else prunes this list.
        for (var i = Roots.Count - 1; i >= 0; i--)
        {
            if (Roots[i].TryGetTarget(out var root)) Apply(root);
            else Roots.RemoveAt(i);
        }
    }

    private static void Apply(Control root)
    {
        foreach (var control in Descendants(root))
        {
            if (ToolTip.GetTip(control) is not string current) continue;
            // First sighting wins: the English in the XAML is the key, whatever is on screen now.
            if (!Originals.TryGetValue(control, out var english))
            {
                english = current;
                Originals.Add(control, english);
            }
            var translated = T(english);
            if (!ReferenceEquals(translated, current) && translated != current)
                ToolTip.SetTip(control, translated);
        }
    }

    private static IEnumerable<Control> Descendants(Control root)
    {
        yield return root;
        foreach (var child in root.GetLogicalDescendants())
            if (child is Control c) yield return c;
    }
}
