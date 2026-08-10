namespace BerexQms.Application.AiEngine.DTOs;

/// <summary>
/// Status of a configured AI provider — health, configuration, and capabilities.
/// </summary>
public sealed record AiProviderStatusDto
{
    public string Provider { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public bool IsHealthy { get; init; }
    public string Model { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; }
    public int MaxRetries { get; init; }
    public IReadOnlyList<string> SupportedTaskTypes { get; init; } = [];
    public string? LastErrorMessage { get; init; }
    public DateTime? LastSuccessAt { get; init; }
    public DateTime? LastErrorAt { get; init; }

    /// <summary>Whether this provider incurs API token costs (false = Local).</summary>
    public bool HasApiCost { get; init; } = true;
}

/// <summary>
/// Task-to-provider mapping configuration.
/// Supports multi-provider fallback chains (Sprint 17).
/// </summary>
public sealed record AiTaskMappingDto
{
    public string TaskType { get; init; } = string.Empty;
    public string PrimaryProvider { get; init; } = string.Empty;
    public string? FallbackProvider { get; init; }
}

/// <summary>
/// Status of an installed local model from the Ollama server.
/// </summary>
public sealed record AiLocalModelDto
{
    public string Name { get; init; } = string.Empty;
    public bool Available { get; init; }
}
