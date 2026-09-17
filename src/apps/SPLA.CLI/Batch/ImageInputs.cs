using SPLA.Domain.Models;

namespace SPLA.CLI.Batch;

/// <summary>One image attached to every cell of a run: where it came from, the name the model knows
/// it by, and the data URL it travels as. The path is kept because a run report that says "3 images"
/// cannot answer the only question anyone asks of it afterwards — which three.</summary>
public sealed record ImageInput(string Path, string Label, string DataUrl)
{
    /// <summary>Bytes of the encoded payload, for the size line in the run report.</summary>
    public long Bytes { get; init; }

    /// <summary>What the model is handed: the picture under the name the run gave it.</summary>
    public ImageAttachment Attachment => new(DataUrl, Label);
}

/// <summary>
/// Turns <c>--image</c> paths into the data URLs a turn carries. Vision travels as
/// <c>data:image/…;base64,…</c> on both paths a run can take — the local one straight into
/// <see cref="SPLA.Runtime.ChatRuntime.SendAsync"/>, the remote one in <c>ChatSendPayload.Images</c> —
/// so the reading and encoding happen once, here, and neither path invents its own.
/// </summary>
internal static class ImageInputs
{
    /// <summary>Extensions a vision model can be expected to take. An unknown one is refused rather
    /// than sent as octet-stream: a provider that silently ignores an unreadable attachment produces
    /// an answer that looks fine and saw nothing.</summary>
    private static readonly Dictionary<string, string> MediaTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
        [".bmp"] = "image/bmp"
    };

    public static string Supported => string.Join(", ", MediaTypes.Keys.Order());

    /// <summary>
    /// Reads every path in the order given — order is meaning when the images are frames or pages.
    /// <para>
    /// Each image is named, because a picture arrives at the model anonymous otherwise (see
    /// <see cref="ImageAttachment"/>). The default name is the file's own name: it is what the person
    /// running this already types, what any file the answer is written back to is called, and it stays
    /// right when the order changes — a positional "Image 2" does not. <paramref name="labels"/>
    /// replaces those names one for one when the file names are not the names worth using.
    /// </para>
    /// </summary>
    /// <returns>Null on the first path that cannot be used, having reported it through
    /// <paramref name="error"/>.</returns>
    public static async Task<List<ImageInput>?> LoadAsync(
        IReadOnlyList<string> paths, IReadOnlyList<string> labels, CancellationToken ct, Action<string> error)
    {
        // Paired by position, so a mismatched count is a silent mislabelling waiting to happen — every
        // image after the missing one would answer to its neighbour's name.
        if (labels.Count > 0 && labels.Count != paths.Count)
        {
            error($"--image-name is given {labels.Count} time(s) for {paths.Count} image(s): "
                + "name every image or none, in the order they are given");
            return null;
        }

        var loaded = new List<ImageInput>();
        for (var i = 0; i < paths.Count; i++)
        {
            var path = paths[i];
            if (!File.Exists(path)) { error($"image not found: {path}"); return null; }

            var ext = Path.GetExtension(path);
            if (!MediaTypes.TryGetValue(ext, out var mediaType))
            {
                error($"not an image this run can send: {path} (supported: {Supported})");
                return null;
            }

            byte[] bytes;
            try { bytes = await File.ReadAllBytesAsync(path, ct); }
            catch (Exception ex) { error($"image unreadable: {path} — {ex.Message}"); return null; }

            if (bytes.Length == 0) { error($"image is empty: {path}"); return null; }

            var label = labels.Count > 0 ? labels[i] : Path.GetFileName(path);
            loaded.Add(new ImageInput(path, label, $"data:{mediaType};base64,{Convert.ToBase64String(bytes)}")
            {
                Bytes = bytes.Length
            });
        }
        return loaded;
    }
}
