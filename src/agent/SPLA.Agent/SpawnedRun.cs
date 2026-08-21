using System;
using System.Collections.Generic;
using SPLA.Domain.Models;

namespace SPLA.Agent;

/// <summary>
/// A finished spawned run, kept around so a caller (or a person on the other end of a debug view)
/// can go back and read what actually happened, not just the one sentence <see cref="Result"/> that
/// was handed back to the model. Everything here is a snapshot taken at the moment the run ended —
/// nothing is live, nothing updates.
/// </summary>
public sealed record SpawnedRun
{
    /// <summary>"r-" + 8 lowercase hex, e.g. "r-a1b2c3d4". Short on purpose: it travels on every
    /// progress tick for the run's whole lifetime, so it has to be cheap to carry.</summary>
    public required string Id { get; init; }

    /// <summary>The task label — the same one the progress node uses, so a run found in the log and a
    /// branch found in the tree are recognizably the same thing.</summary>
    public required string Label { get; init; }

    public string? SkillId { get; init; }

    /// <summary><see cref="Domain.Models.AgentMode"/>.ToString() — a plain string here because this
    /// record is meant to be read by things that never need the enum itself.</summary>
    public required string Mode { get; init; }

    public required DateTimeOffset StartedAt { get; init; }
    public required DateTimeOffset FinishedAt { get; init; }

    /// <summary>"completed" | "failed" | "cancelled".</summary>
    public required string Outcome { get; init; }

    /// <summary>The exception message when <see cref="Outcome"/> is "failed"; null otherwise.</summary>
    public string? Error { get; init; }

    /// <summary>The last assistant message — what <see cref="SpawnedAgentRunner.RunAsync"/> already
    /// returns to its caller. Kept here too so a reader of the log does not need the tool result to
    /// still be sitting in the parent's conversation.</summary>
    public required string Result { get; init; }

    /// <summary>The whole conversation the run produced — system prompt, seed message, every turn and
    /// tool call. This is the actual point of the type: the thing that used to be thrown away.</summary>
    public required IReadOnlyList<ChatMessage> Messages { get; init; }
}
