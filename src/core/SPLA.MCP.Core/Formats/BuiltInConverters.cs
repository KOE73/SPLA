using SPLA.Domain.Formats;
using SPLA.Domain.Resources;

namespace SPLA.MCP.Core.Formats;

/// <summary>
/// The projections the host ships with — the whole day-one set, in one readable list.
///
/// <para>Kept as one call rather than four scattered <c>Register</c> lines so that "what can this
/// project convert before any plugin loads" is answerable by reading a single method, which is the
/// same property the registry's own no-composition rule exists to protect.</para>
/// </summary>
public static class BuiltInConverters
{
    public static void RegisterInto(FormatConverterRegistry registry)
    {
        // Identity: the commonest call in the system, deliberately routed through the lookup.
        registry.Register(new IdentityConverter());

        // The two sources whose bytes might be text. One implementation, two pairs — the registry
        // keys on the pair, so this is registration, not a clash.
        registry.Register(new Utf8TextConverter("text/*"));
        registry.Register(new Utf8TextConverter(ContentTypes.Unknown));

        // JSON is text that the MIME registry files under 'application/', so 'text/*' above does not
        // answer for it. Without this line the commonest thing a model does with a .json address —
        // ask to see it — would be refused by a registry that had a decoder for those very bytes.
        registry.Register(new Utf8TextConverter("application/json"));

        // The first real reshape: parses, rebuilds, emits a different number of bytes.
        registry.Register(new JsonToYamlConverter());
    }
}
