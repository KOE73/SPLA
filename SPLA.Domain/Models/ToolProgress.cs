using System;
using System.Collections.Generic;

namespace SPLA.Domain.Models;

/// <summary>
/// A single progress tick emitted by a running tool. Everything is optional: a tool reports
/// whatever it can — a fraction, a current/total count, a human message, and any extra "details"
/// worth surfacing (e.g. open ports discovered so far). Consumers (status bar, CLI) format what is
/// present and ignore the rest. This is the universal shape long-running tools speak; see
/// <see cref="SPLA.Domain.Tools.ToolProgressScope"/> for how a tool emits it without touching its
/// signature.
/// </summary>
public sealed class ToolProgress
{
    /// <summary>Items processed so far, when the tool counts discrete work units.</summary>
    public long? Current { get; init; }

    /// <summary>Total work units, when known up front.</summary>
    public long? Total { get; init; }

    /// <summary>Short human-readable note about the current step.</summary>
    public string? Message { get; init; }

    /// <summary>Extra interesting findings to show live (label → value), e.g. ("open", "22, 80").</summary>
    public IReadOnlyList<ToolProgressDetail>? Details { get; init; }

    /// <summary>Completion in [0,1] when both <see cref="Current"/> and a positive <see cref="Total"/> are known.</summary>
    public double? Fraction =>
        Total is long t && t > 0 && Current is long c
            ? Math.Clamp((double)c / t, 0, 1)
            : null;
}

/// <summary>A single named datum surfaced alongside progress.</summary>
public readonly record struct ToolProgressDetail(string Label, string Value);
