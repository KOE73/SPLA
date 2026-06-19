namespace SPLA.Domain.Models;

public class ToolDefinition
{
    public string Type { get; set; } = "function";
    public ToolFunctionDefinition Function { get; set; } = new();
}

public class ToolFunctionDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object? Parameters { get; set; }
    
    // Permission Metadata
    public ToolScope Scope { get; set; } = ToolScope.Project;
    public ToolEffect Effect { get; set; } = ToolEffect.Read;
    public ToolRisk Risk { get; set; } = ToolRisk.Low;

    /// <summary>
    /// When true the serializer adds "strict": true to the function payload (OpenAI strict
    /// function calling). Only set this for tools where every parameter is either listed in
    /// <c>required</c> or declared with a nullable type — the provider enforces this contract.
    /// </summary>
    public bool StrictSchema { get; set; }
}

