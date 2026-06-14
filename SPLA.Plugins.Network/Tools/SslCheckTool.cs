using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class SslCheckTool : IMcpTool
{
    public string Name => "network.ssl.check";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Connects via TLS and returns full certificate diagnostics: protocol, cipher, OS validation result, leaf cert details (key size, SANs, EKU, AIA/OCSP, CRL, CT), and the full certificate chain. Always connects even to expired or self-signed certs to allow inspection.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    host = new { type = "string", description = "Domain name or IP address (e.g. 'example.com')." },
                    port = new { type = "integer", description = "TLS port (default: 443)." },
                    timeout = new { type = "integer", description = "Connection timeout in milliseconds (default: 5000)." },
                    sniHost = new { type = "string", description = "SNI hostname override (defaults to 'host'). Set when the IP serves multiple vhosts." }
                },
                required = new[] { "host" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (!doc.RootElement.TryGetProperty("host", out var hostEl))
                return "Error: Missing 'host' parameter.";

            var host = hostEl.GetString();
            if (string.IsNullOrWhiteSpace(host))
                return "Error: Host is empty.";

            var port = doc.RootElement.TryGetProperty("port", out var portEl) && portEl.TryGetInt32(out var p)
                ? Math.Clamp(p, 1, 65535) : 443;
            var timeoutMs = doc.RootElement.TryGetProperty("timeout", out var timeoutEl) && timeoutEl.TryGetInt32(out var t)
                ? Math.Clamp(t, 500, 60_000) : 5000;
            var sniHost = doc.RootElement.TryGetProperty("sniHost", out var sniEl) ? sniEl.GetString() : null;
            sniHost = string.IsNullOrWhiteSpace(sniHost) ? host : sniHost;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeoutMs);

            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port, cts.Token);

            // Capture OS validation result without blocking the connection — we want to inspect broken certs too.
            SslPolicyErrors policyErrors = SslPolicyErrors.None;
            using var sslStream = new SslStream(tcpClient.GetStream(), false,
                (_, _, _, errors) => { policyErrors = errors; return true; });

            await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
            {
                TargetHost = sniHost,
                EnabledSslProtocols = SslProtocols.None,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck
            }, cts.Token);

            var leaf = sslStream.RemoteCertificate as X509Certificate2
                ?? (sslStream.RemoteCertificate is { } raw ? new X509Certificate2(raw) : null);

            if (leaf == null)
                return "Error: No certificate returned by the server.";

            var sb = new StringBuilder();
            var now = DateTime.UtcNow;

            // ── Connection ───────────────────────────────────────────────────────
            sb.AppendLine($"TLS: {host}:{port}");
            sb.AppendLine($"Protocol:        {sslStream.SslProtocol}");
            sb.AppendLine($"Cipher suite:    {sslStream.NegotiatedCipherSuite}");
            sb.AppendLine();

            // ── OS Validation ────────────────────────────────────────────────────
            sb.AppendLine("=== Validation ===");
            if (policyErrors == SslPolicyErrors.None)
            {
                sb.AppendLine("OS validation:   VALID");
            }
            else
            {
                sb.AppendLine($"OS validation:   FAILED ({policyErrors})");
                if (policyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
                    sb.AppendLine("  ! Hostname does not match CN / SANs");
                if (policyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
                    sb.AppendLine("  ! Chain has errors (expired, untrusted root, revoked, etc.)");
                if (policyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable))
                    sb.AppendLine("  ! No certificate was provided by the server");
            }

            using var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            bool chainBuilt = chain.Build(leaf);
            if (!chainBuilt && chain.ChainStatus.Length > 0)
            {
                sb.AppendLine("Chain errors:");
                foreach (var status in chain.ChainStatus)
                    sb.AppendLine($"  ! {status.Status}: {status.StatusInformation.Trim()}");
            }
            sb.AppendLine();

            // ── Leaf Certificate ─────────────────────────────────────────────────
            sb.AppendLine("=== Leaf Certificate ===");
            var notBefore = leaf.NotBefore.ToUniversalTime();
            var notAfter  = leaf.NotAfter.ToUniversalTime();
            var daysLeft  = (notAfter - now).TotalDays;
            var expiryTag = daysLeft < 0 ? " [EXPIRED]" : daysLeft < 30 ? " [EXPIRING SOON]" : "";

            sb.AppendLine($"Subject:         {leaf.Subject}");
            sb.AppendLine($"Issuer:          {leaf.Issuer}");
            sb.AppendLine($"Not before:      {notBefore:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine($"Not after:       {notAfter:yyyy-MM-dd HH:mm:ss} UTC{expiryTag}");
            sb.AppendLine($"Days until exp:  {daysLeft:F1}");
            sb.AppendLine($"Serial:          {leaf.SerialNumber}");
            sb.AppendLine($"Thumbprint:      {leaf.Thumbprint}");

            // Key size
            var keyAlg = leaf.PublicKey.Oid.FriendlyName ?? leaf.PublicKey.Oid.Value ?? "?";
            int? keyBits = null;
            try
            {
                keyBits = leaf.PublicKey.Oid.Value switch
                {
                    "1.2.840.113549.1.1.1" => leaf.GetRSAPublicKey()?.KeySize,   // RSA
                    "1.2.840.10045.2.1"    => leaf.GetECDsaPublicKey()?.KeySize, // ECDSA
                    "1.2.840.10040.4.1"    => leaf.GetDSAPublicKey()?.KeySize,   // DSA
                    _                      => null
                };
            }
            catch { }

            sb.AppendLine($"Key:             {keyAlg}{(keyBits.HasValue ? $" {keyBits} bits" : "")}");
            sb.AppendLine($"Sig algorithm:   {leaf.SignatureAlgorithm.FriendlyName ?? leaf.SignatureAlgorithm.Value}");
            sb.AppendLine();

            // ── Extensions ───────────────────────────────────────────────────────
            sb.AppendLine("=== Extensions ===");
            foreach (X509Extension ext in leaf.Extensions)
            {
                switch (ext.Oid?.Value)
                {
                    case "2.5.29.15": // Key Usage
                        var kue = new X509KeyUsageExtension(ext, ext.Critical);
                        sb.AppendLine($"Key usage:       {kue.KeyUsages}");
                        break;

                    case "2.5.29.37": // Extended Key Usage
                        var eku = new X509EnhancedKeyUsageExtension(ext, ext.Critical);
                        var ekuNames = eku.EnhancedKeyUsages.Cast<Oid>()
                            .Select(o => o.FriendlyName ?? o.Value ?? "?");
                        sb.AppendLine($"EKU:             {string.Join(", ", ekuNames)}");
                        break;

                    case "2.5.29.19": // Basic Constraints
                        var bc = new X509BasicConstraintsExtension(ext, ext.Critical);
                        sb.AppendLine($"Is CA:           {bc.CertificateAuthority}" +
                            (bc.HasPathLengthConstraint ? $", max path length: {bc.PathLengthConstraint}" : ""));
                        break;

                    case "2.5.29.17": // Subject Alternative Names
                        sb.AppendLine($"SANs:            {ext.Format(false)}");
                        break;

                    case "1.3.6.1.5.5.7.1.1": // Authority Information Access (OCSP / CA Issuers)
                        sb.AppendLine($"AIA/OCSP:        {ext.Format(false)}");
                        break;

                    case "2.5.29.31": // CRL Distribution Points
                        sb.AppendLine($"CRL:             {ext.Format(false)}");
                        break;

                    case "2.5.29.32": // Certificate Policies
                        sb.AppendLine($"Policies:        {ext.Format(false)}");
                        break;

                    case "1.3.6.1.4.1.11129.2.4.2": // Certificate Transparency SCTs
                        sb.AppendLine($"CT SCTs:         present ({ext.RawData.Length} bytes)");
                        break;
                }
            }
            sb.AppendLine();

            // ── Certificate Chain ────────────────────────────────────────────────
            sb.AppendLine($"=== Chain ({chain.ChainElements.Count} elements) ===");
            for (int i = 0; i < chain.ChainElements.Count; i++)
            {
                var el   = chain.ChainElements[i];
                var role = i == 0 ? "leaf" : i == chain.ChainElements.Count - 1 ? "root" : "intermediate";
                var elDays  = (el.Certificate.NotAfter.ToUniversalTime() - now).TotalDays;
                var elTag   = elDays < 0 ? " [EXPIRED]" : "";

                sb.AppendLine($"[{i}] {role}: {el.Certificate.Subject}");
                sb.AppendLine($"    Issuer:     {el.Certificate.Issuer}");
                sb.AppendLine($"    Valid:      {el.Certificate.NotBefore.ToUniversalTime():yyyy-MM-dd} .. {el.Certificate.NotAfter.ToUniversalTime():yyyy-MM-dd} UTC{elTag}");
                sb.AppendLine($"    Thumbprint: {el.Certificate.Thumbprint}");

                if (el.ChainElementStatus.Length > 0)
                {
                    var issues = string.Join(", ", el.ChainElementStatus.Select(s => s.Status.ToString()));
                    sb.AppendLine($"    Issues:     {issues}");
                }
            }

            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error checking TLS: {ex.Message}";
        }
    }
}
