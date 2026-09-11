using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using SPLA.Plugins.Android.Adb;

namespace SPLA.Plugins.Android.Ui;

internal sealed record UiElement(string Ref, int X, int Y, string Text);
internal sealed record UiDump(string Text, Dictionary<string, UiElement> Refs);

internal static class UiAutomatorDump
{
    public static async Task<string> ReadAsync(AdbRunner adb, string serial, CancellationToken ct)
    {
        const string path = "/data/local/tmp/spla_ui.xml";
        try
        {
            for (var attempt = 0; attempt < 2; attempt++)
            {
                // Remove an earlier dump so a failed command can never return stale content.
                await adb.CheckedAsync(serial, ["shell", "rm", "-f", path], ct);
                var result = await adb.RunAsync(serial, ["shell", "uiautomator", "dump", path], TimeSpan.FromSeconds(30), ct);
                if (result.ExitCode == 0 && !(result.StdErr + result.StdOut).Contains("ERROR", StringComparison.OrdinalIgnoreCase)) break;
                if (attempt == 1) throw new IOException($"UI dump failed: {result.StdErr} {result.StdOut}");
                await Task.Delay(500, ct);
            }
            return await adb.CheckedAsync(serial, ["exec-out", "cat", path], ct);
        }
        finally
        {
            try { await adb.CheckedAsync(serial, ["shell", "rm", "-f", path], CancellationToken.None, 5000); } catch (Exception) { }
        }
    }

    public static UiDump Parse(string xml, int width, int height, string? filter = null)
    {
        using var reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, MaxCharactersInDocument = 4_000_000, XmlResolver = null });
        var document = XDocument.Load(reader);
        var nodes = document.Descendants("node").ToArray();
        var first = nodes.FirstOrDefault() ?? throw new IOException("UI dump has no nodes.");
        static int[] Bounds(XElement node) => Regex.Matches((string?)node.Attribute("bounds") ?? "", @"-?\d+")
            .Select(m => int.Parse(m.Value, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        var display = Bounds(first);
        if (display.Length != 4 || display[2] <= 0 || display[3] <= 0) throw new IOException("UI dump has invalid display bounds.");
        Dictionary<string, UiElement> refs = []; List<string> lines = [];
        var total = 0;
        foreach (var node in nodes)
        {
            string Attr(string name) => (string?)node.Attribute(name) ?? "";
            var text = Attr("text"); var description = Attr("content-desc");
            var flags = new[] { "clickable", "long-clickable", "scrollable", "checkable" }.Where(name => Attr(name) == "true").ToArray();
            if (text.Length == 0 && description.Length == 0 && flags.Length == 0) continue;
            if (filter is not null && !(text + description + Attr("resource-id")).Contains(filter, StringComparison.OrdinalIgnoreCase)) continue;
            var bounds = Bounds(node); if (bounds.Length != 4) continue;
            total++;
            if (lines.Count == 400) continue;
            var x = Math.Clamp((int)(((long)bounds[0] + bounds[2]) * width / (2L * display[2])), 0, width - 1);
            var y = Math.Clamp((int)(((long)bounds[1] + bounds[3]) * height / (2L * display[3])), 0, height - 1);
            var reference = "e" + total;
            static string Clean(string value) => value.Replace('\r', ' ').Replace('\n', ' ').Replace('"', '\'');
            var line = $"{new string(' ', Math.Min(20, node.Ancestors("node").Count()) * 2)}[{reference}] {Attr("class").Split('.').Last()} \"{Clean(text)}\" id={Attr("resource-id")} desc=\"{Clean(description)}\" center=({x},{y}) {string.Join(' ', flags)}";
            if (line.Length > 1000) line = line[..1000] + "…";
            lines.Add(line); refs[reference] = new(reference, x, y, text + " " + description);
        }
        if (total > lines.Count) lines.Add($"… {total - lines.Count} more nodes");
        return new(string.Join('\n', lines), refs);
    }
}
