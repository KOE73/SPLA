using SPLA.Domain.Agent;

namespace SPLA.Agent;

/// <summary>
/// Concrete mark/checkpoint manager that lives in the Agent layer.
/// Inherits from <see cref="MarkManager"/> (SPLA.Domain) so that
/// SPLA.MCP.Core tools can accept the base type without a circular dependency on Agent.
/// </summary>
public sealed class CheckpointManager : MarkManager { }
