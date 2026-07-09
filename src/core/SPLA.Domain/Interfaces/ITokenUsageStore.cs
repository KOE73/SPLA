using System;
using SPLA.Domain.Models;

namespace SPLA.Domain.Interfaces;

/// <summary>
/// Persistent accumulator of real token usage for the lifetime of a project. Records every counted
/// turn — successful or not — so the project keeps an ultimate, ever-growing tally of input/output
/// tokens. Separate from the per-message numbers on <see cref="Models.ChatMessage"/> (which describe
/// one turn) and from any UI counter (which is per-chat and ephemeral).
/// </summary>
public interface ITokenUsageStore
{
    /// <summary>Cumulative totals across the whole project lifetime, loaded at startup, persisted on write.</summary>
    TokenUsageTotals Total { get; }

    /// <summary>Totals accumulated since this process started (not persisted; resets each run).</summary>
    TokenUsageTotals Session { get; }

    /// <summary>
    /// Folds one turn's provider-reported usage into both <see cref="Session"/> and <see cref="Total"/>
    /// and persists the new lifetime total. Null figures mean the provider did not report that number.
    /// Safe to call from any thread.
    /// </summary>
    void Record(int? promptTokens, int? completionTokens);

    /// <summary>Raised after <see cref="Record"/> updates the totals.</summary>
    event EventHandler? Changed;
}
