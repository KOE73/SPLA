using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Resources;
using SPLA.Domain.Security;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Tools;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Geometry.Image;

/// <summary>What came back from an address, or why nothing did.</summary>
/// <param name="Origin">Where the bytes came from, when the source says. Carried out so the tool that
/// opened the session can raise the chat's doubt for it — a picture pulled off the open web is other
/// people's content whether or not it is a picture.</param>
internal readonly record struct LoadedImage(byte[] Bytes, DataOrigin? Origin, string? Error)
{
    public bool Ok => Error is null;

    public static LoadedImage Fail(string error) => new(Array.Empty<byte>(), null, error);

    public static LoadedImage From(byte[] bytes, DataOrigin? origin = null) => new(bytes, origin, null);
}

/// <summary>
/// The one door an image address goes through. Three forms, tried in this order:
/// a <c>blob:</c> handle from another tool, a URI served by a registered resource provider
/// (<c>file:///…</c> and any scheme the project has registered), or a plain path in the workspace.
/// <para>
/// A plain path goes through <see cref="IWorkspace"/> rather than <c>System.IO</c>: the workspace is
/// where the project's boundary is enforced, and a plugin reading files around it would be a plugin
/// granting itself reach the host did not give.
/// </para>
/// <para>
/// Failures come back as text, never as exceptions: to the model a bad address is an answer to
/// correct, not a crash.
/// </para>
/// </summary>
internal static class ImageSource
{
    public static async Task<LoadedImage> LoadAsync(
        string address, ResolvedSettings settings, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(address))
            return LoadedImage.Fail("No image address was given.");

        address = address.Trim();

        // 1. A handle produced by another tool in this chat.
        if (DataChannel.IsHandle(address))
        {
            if (!DataChannel.ResolveBytes(address, out var blobBytes, out var blobError))
                return LoadedImage.Fail(blobError ?? $"Could not read blob handle '{address}'.");
            if (blobBytes.Length == 0)
                return LoadedImage.Fail($"Blob handle '{address}' holds no data.");

            // DataChannel already inherited the doubt; the origin is carried out for the record.
            var origin = AgentSessionScope.Current?.Blobs.Describe(address)?.Origin;
            return LoadedImage.From(blobBytes, origin);
        }

        // 2. An address with a scheme — file:// and whatever else this project registered.
        if (address.Contains("://", StringComparison.Ordinal))
        {
            if (!ResourceRegistry.For(settings).TryResolve(address, out var provider, out var uri, out var error))
                return LoadedImage.Fail(error ?? $"Could not resolve address '{address}'.");

            if (!ResourceRegistry.Supports(provider, ResourceVerb.Read))
                return LoadedImage.Fail($"Scheme '{uri.Scheme}' cannot be read from.");

            try
            {
                var content = await provider.ReadAsync(uri, ct).ConfigureAwait(false);
                return content.Bytes is { Length: > 0 }
                    ? LoadedImage.From(content.Bytes)
                    : LoadedImage.Fail($"'{address}' holds no data.");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return LoadedImage.Fail($"Could not read '{address}': {ex.Message}");
            }
        }

        // 3. A plain path inside the workspace.
        try
        {
            var bytes = await HostServices.Sandbox.Workspace.ReadAllBytesAsync(address, ct).ConfigureAwait(false);
            return bytes is { Length: > 0 }
                ? LoadedImage.From(bytes)
                : LoadedImage.Fail($"'{address}' holds no data.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return LoadedImage.Fail($"Could not read '{address}': {ex.Message}");
        }
    }
}
