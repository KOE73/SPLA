namespace SPLA.Domain.Models;

/// <summary>
/// One picture attached to a message, with the name it is known by.
/// <para>
/// The name exists because a picture has no identity of its own on the wire. Every vision API in
/// reach — the OpenAI-compatible <c>image_url</c> parts this project speaks, and Anthropic's
/// <c>image</c> blocks — carries the bytes and nothing else: no file name, no id, and image metadata
/// is not read. With several pictures in one message the only thing telling them apart is their
/// position, which is exactly what breaks down at ten frames of the same camera. So the name is not
/// decoration: it is the sole handle a prompt (and the answer coming back) can use to say
/// <i>which</i> picture — and the provider only ever sees it because
/// <c>SPLA.LLM.OpenAiCompat.OpenAiCompatibleClient.BuildMultimodalContent</c> writes it out as a
/// text part of its own in front of the image.
/// </para>
/// </summary>
/// <param name="Url">The picture itself: a data URL (<c>data:image/png;base64,…</c>) on the way to a
/// model, or a stored reference once a client has persisted it.</param>
/// <param name="Label">What to call it — a file name, a frame id, anything a person would say out
/// loud. Null when nobody named it; then only its position identifies it, as before.</param>
public sealed record ImageAttachment(string Url, string? Label = null);
