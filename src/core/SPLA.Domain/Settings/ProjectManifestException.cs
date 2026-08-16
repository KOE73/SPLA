using System;

namespace SPLA.Domain.Settings;

/// <summary>
/// The manifest says something that cannot be honoured, so the project does not open.
///
/// <para>Distinct from an I/O failure and from a YAML parse error on purpose: the file was read and
/// understood, and what it asked for is refused. The message names the manifest and the rule, and it
/// is written to be read by a person and by a model — whoever hits it has to know what to change.
/// </para>
/// </summary>
public sealed class ProjectManifestException : Exception
{
    public ProjectManifestException(string manifestPath, string reason)
        : base($"{manifestPath}: {reason}")
    {
        ManifestPath = manifestPath;
        Reason = reason;
    }

    /// <summary>The manifest that could not be honoured.</summary>
    public string ManifestPath { get; }

    /// <summary>What is wrong, without the file prefix — for callers that show the file separately.</summary>
    public string Reason { get; }
}
