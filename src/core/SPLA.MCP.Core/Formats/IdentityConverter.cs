using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Formats;
using SPLA.Domain.Resources;

namespace SPLA.MCP.Core.Formats;

/// <summary>
/// The projection that does nothing: an image asked for as an image comes back exactly as it arrived,
/// same bytes, same type.
///
/// <para><b>A wildcard target means "preserves the source type".</b> <c>image/* → image/*</c> does not
/// say "any image becomes some other image"; it says the pair is answered for every image family and
/// that the answer keeps whatever the source already was. A JPEG in is a JPEG out — this converter
/// never re-encodes, because re-encoding is exactly the kind of silent loss the interface's doc
/// comment forbids treating as equality.</para>
///
/// <para><b>Why identity is a registered converter rather than a branch in the registry.</b> The
/// commonest call in the system — show me this screenshot — is the one that would otherwise skip the
/// lookup entirely, and a lookup path exercised only by rare calls is a lookup path nobody notices is
/// broken. Registering identity puts the ordinary case through the same resolve, the same failure
/// wording and the same card list as everything else.</para>
/// </summary>
public sealed class IdentityConverter : IFormatConverter
{
    public string SourceType => "image/*";
    public string TargetType => "image/*";

    public string Summary =>
        "image to image — the same bytes, unchanged: image/png stays image/png, nothing is re-encoded";

    public Task<ResourceContent> ConvertAsync(
        ResourceContent source,
        IReadOnlyDictionary<string, object?>? options,
        CancellationToken ct = default)
        => Task.FromResult(source);
}
