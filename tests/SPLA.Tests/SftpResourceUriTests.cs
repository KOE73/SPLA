using SPLA.Domain.Resources;
using SPLA.Plugins.Ssh.Resources;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// <see cref="SftpResourceProvider.MapAddress"/> on its own — the pure address→(host, remote path)
/// mapping, with no SFTP connection involved. Everything else the provider does needs a live host,
/// which is why this is the whole test surface for it: what remains untested here is exercised by
/// the existing <c>sftp_*</c> tools, which already share this same host lookup.
/// </summary>
public sealed class SftpResourceUriTests
{
    private static ResourceUri Parse(string text)
    {
        Assert.True(ResourceUri.TryParse(text, out var uri, out _));
        return uri;
    }

    [Fact]
    public void Authority_becomes_the_host_name()
    {
        var (host, _) = SftpResourceProvider.MapAddress(Parse("sftp://ioBroker/etc/nginx.conf"));

        Assert.Equal("ioBroker", host);
    }

    [Fact]
    public void Leading_slash_is_restored_because_sftp_paths_are_absolute()
    {
        var (_, path) = SftpResourceProvider.MapAddress(Parse("sftp://ioBroker/etc/nginx.conf"));

        Assert.Equal("/etc/nginx.conf", path);
    }

    [Fact]
    public void Nested_paths_keep_every_segment()
    {
        var (host, path) = SftpResourceProvider.MapAddress(Parse("sftp://ioBroker/var/log/nginx/access.log"));

        Assert.Equal("ioBroker", host);
        Assert.Equal("/var/log/nginx/access.log", path);
    }

    /// <summary>Naming the root itself — <c>sftp://ioBroker</c> with nothing after the host — is
    /// legitimate per <see cref="ResourceUri.TryParse"/> and must map to the filesystem root, not to
    /// an empty or malformed path.</summary>
    [Fact]
    public void An_authority_with_no_path_maps_to_the_remote_root()
    {
        var (host, path) = SftpResourceProvider.MapAddress(Parse("sftp://ioBroker"));

        Assert.Equal("ioBroker", host);
        Assert.Equal("/", path);
    }

    [Fact]
    public void An_empty_authority_is_refused_with_the_correct_form_shown()
    {
        var uri = Parse("sftp:///etc/nginx.conf");

        var ex = Assert.Throws<InvalidOperationException>(() => SftpResourceProvider.MapAddress(uri));

        Assert.Contains("sftp://ioBroker/etc/nginx.conf", ex.Message);
    }
}
