using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using SPLA.Service;
using SPLA.Service.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPLA.Tests;

/// <summary>
/// The rule connections live by: a credential travels to a client as a <b>reference</b> or not at
/// all. Everything here is about the seam between the stored config and the editor — the place a
/// key would leak from, and the place an editor that was never shown one could destroy it.
/// </summary>
public sealed class ConnectionCredentialTests
{
    private static AgentRuntime BuildRuntime(string apiKey, string? adminKey = null)
    {
        var root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-conn-cred-{Guid.NewGuid():N}")).FullName;
        var manifest = Path.Combine(root, "test.spla");
        var admin = adminKey == null ? "" : $"\n    admin_key: {adminKey}";
        File.WriteAllText(manifest, $"""
            version: 1
            name: ConnCredTest
            workspace: .
            connections:
              - id: openrouter
                name: OpenRouter
                provider: openrouter
                endpoint: https://openrouter.ai/api/v1
                api_key: {apiKey}{admin}
                models:
                  - id: openrouter-free
                    model: some/model:free
            """);

        return new AgentRuntime(ConfigLoader.LoadAndResolve(manifest), NullLoggerFactory.Instance);
    }

    private static ConnectionEditDto Only(ConnectionsPayload p) => p.Connections.Single();

    /// <summary>The reason this seam exists: the value a browser is handed must be a pointer.</summary>
    [Fact]
    public void A_stored_literal_is_withheld_and_a_reference_is_published()
    {
        var literal = Only(SettingsOps.GetConnections(BuildRuntime("sk-or-v1-REALKEY")));
        Assert.Null(literal.ApiKey);
        Assert.True(literal.ApiKeyIsLiteral);

        var reference = Only(SettingsOps.GetConnections(BuildRuntime("secret:user:openrouter#token")));
        Assert.Equal("secret:user:openrouter#token", reference.ApiKey);
        Assert.False(reference.ApiKeyIsLiteral);
    }

    /// <summary>Opening the panel and pressing Save must not destroy a key the editor never saw —
    /// the blank field it round-trips means "unchanged", which is what the flag carries.</summary>
    [Fact]
    public void Saving_an_untouched_withheld_literal_keeps_it()
    {
        var runtime = BuildRuntime("sk-or-v1-REALKEY", "sk-admin-REALKEY");

        // Exactly what a client that changed nothing sends back.
        var echoed = SettingsOps.GetConnections(runtime).Connections;
        SettingsOps.SaveConnections(runtime, echoed);

        var stored = runtime.Settings.Connections.Single();
        Assert.Equal("sk-or-v1-REALKEY", stored.ApiKey);
        Assert.Equal("sk-admin-REALKEY", stored.AdminKey);

        // And it survives to disk, not just in memory.
        Assert.Contains("sk-or-v1-REALKEY", File.ReadAllText(runtime.Settings.ProjectFilePath!));
    }

    /// <summary>Picking a secret replaces the literal — the migration this whole change is for.</summary>
    [Fact]
    public void Choosing_a_reference_replaces_the_literal()
    {
        var runtime = BuildRuntime("sk-or-v1-REALKEY");

        var edited = Only(SettingsOps.GetConnections(runtime));
        edited.ApiKey = "secret:user:openrouter#token";
        edited.ApiKeyIsLiteral = false;   // what the editor clears when the user picks
        SettingsOps.SaveConnections(runtime, new List<ConnectionEditDto> { edited });

        Assert.Equal("secret:user:openrouter#token", runtime.Settings.Connections.Single().ApiKey);

        var onDisk = File.ReadAllText(runtime.Settings.ProjectFilePath!);
        Assert.Contains("secret:user:openrouter#token", onDisk);
        Assert.DoesNotContain("sk-or-v1-REALKEY", onDisk);
    }

    /// <summary>"Keep it" must not be the only option a blank field can mean, or a credential could
    /// never be removed. Blank without the flag is the explicit clear.</summary>
    [Fact]
    public void Clearing_the_flag_and_the_field_removes_the_credential()
    {
        var runtime = BuildRuntime("sk-or-v1-REALKEY");

        var edited = Only(SettingsOps.GetConnections(runtime));
        edited.ApiKey = "";
        edited.ApiKeyIsLiteral = false;
        SettingsOps.SaveConnections(runtime, new List<ConnectionEditDto> { edited });

        Assert.Null(runtime.Settings.Connections.Single().ApiKey);
        Assert.DoesNotContain("sk-or-v1-REALKEY", File.ReadAllText(runtime.Settings.ProjectFilePath!));
    }

    /// <summary>A brand-new connection has nothing stored to fall back to; a blank stays blank
    /// rather than inheriting from whatever shares its slot.</summary>
    [Fact]
    public void A_new_connection_with_no_credential_stores_none()
    {
        var runtime = BuildRuntime("secret:user:openrouter#token");

        var existing = Only(SettingsOps.GetConnections(runtime));
        var added = new ConnectionEditDto { Id = "local", Provider = "lmstudio", ApiKeyIsLiteral = true };
        SettingsOps.SaveConnections(runtime, new List<ConnectionEditDto> { existing, added });

        Assert.Null(runtime.Settings.Connections.Single(c => c.Id == "local").ApiKey);
    }
}
