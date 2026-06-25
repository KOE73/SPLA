using System;
using System.IO;
using System.Text.Json;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;

namespace SPLA.Domain.Agent;

/// <summary>
/// File-backed <see cref="ITokenUsageStore"/>. The lifetime total lives in a small JSON file
/// (typically <c>&lt;workspace&gt;/.spla/token-usage.json</c>) and is rewritten atomically after every
/// recorded turn, so the count survives restarts and crashes. Thread-safe; cheap enough to write on
/// each turn since turns are coarse-grained.
/// </summary>
public sealed class FileTokenUsageStore : ITokenUsageStore
{
    private readonly string _path;
    private readonly object _gate = new();
    private readonly TokenUsageTotals _total;
    private readonly TokenUsageTotals _session = new();

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public FileTokenUsageStore(string path)
    {
        _path = path;
        _total = Load(path);
    }

    public TokenUsageTotals Total
    {
        get { lock (_gate) return _total.Clone(); }
    }

    public TokenUsageTotals Session
    {
        get { lock (_gate) return _session.Clone(); }
    }

    public event EventHandler? Changed;

    public void Record(int? promptTokens, int? completionTokens)
    {
        if (promptTokens is null && completionTokens is null) return;

        lock (_gate)
        {
            _session.Add(promptTokens, completionTokens);
            _total.Add(promptTokens, completionTokens);
            Save();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static TokenUsageTotals Load(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var dto = JsonSerializer.Deserialize<Dto>(json);
                if (dto != null)
                    return new TokenUsageTotals
                    {
                        PromptTokens = dto.PromptTokens,
                        CompletionTokens = dto.CompletionTokens,
                        Turns = dto.Turns
                    };
            }
        }
        catch
        {
            // A corrupt or unreadable tally must never stop a chat; start fresh rather than throw.
        }
        return new TokenUsageTotals();
    }

    private void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            var dto = new Dto
            {
                PromptTokens = _total.PromptTokens,
                CompletionTokens = _total.CompletionTokens,
                Turns = _total.Turns,
                UpdatedUtc = DateTime.UtcNow
            };
            var json = JsonSerializer.Serialize(dto, JsonOptions);

            // Atomic-ish write: stage to a temp file then move over the target.
            var tmp = _path + ".tmp";
            File.WriteAllText(tmp, json);
            File.Move(tmp, _path, overwrite: true);
        }
        catch
        {
            // Persistence is best-effort; losing a write must not break the conversation loop.
        }
    }

    private sealed class Dto
    {
        public long PromptTokens { get; set; }
        public long CompletionTokens { get; set; }
        public long Turns { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }
}
