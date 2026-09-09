using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace SPLA.Domain.Models;

/// <summary>
/// Reads and writes a persisted image attachment in whichever of the two shapes fits it:
/// <c>- name.png</c> for one nobody named, <c>- {file: name.png, label: seg012.jpg}</c> for one
/// somebody did.
/// <para>
/// The scalar shape is the one every chat written before names existed already has on disk, and
/// keeping it readable is the point of this converter — but so is keeping it <i>written</i>: an
/// unnamed image round-trips back to the bare file name it arrived as, so opening old chats does not
/// silently rewrite years of history into a wordier shape that says nothing new.
/// </para>
/// </summary>
public sealed class ChatSessionImageConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(ChatSessionImage);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        if (parser.TryConsume<Scalar>(out var scalar))
            return new ChatSessionImage(scalar.Value);

        parser.Consume<MappingStart>();
        var image = new ChatSessionImage();
        while (!parser.TryConsume<MappingEnd>(out _))
        {
            var key = parser.Consume<Scalar>().Value;
            var value = parser.Consume<Scalar>().Value;
            switch (key)
            {
                case "file": image.File = value; break;
                case "label": image.Label = value.Length > 0 ? value : null; break;
                // Unknown keys are skipped rather than refused, matching the reader's
                // IgnoreUnmatchedProperties everywhere else in the chat file.
            }
        }
        return image;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var image = (ChatSessionImage?)value;
        if (image == null) { emitter.Emit(new Scalar(string.Empty)); return; }

        if (image.Label is not { Length: > 0 })
        {
            emitter.Emit(new Scalar(image.File));
            return;
        }

        emitter.Emit(new MappingStart());
        emitter.Emit(new Scalar("file"));
        emitter.Emit(new Scalar(image.File));
        emitter.Emit(new Scalar("label"));
        emitter.Emit(new Scalar(image.Label));
        emitter.Emit(new MappingEnd());
    }
}
