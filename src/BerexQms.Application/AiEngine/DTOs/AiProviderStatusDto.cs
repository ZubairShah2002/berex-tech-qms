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
}

/// <summary>
/// Task-to-provider mapping configuration.
/// </summary>
public sealed record AiTaskMappingDto
{
    public string TaskType { get; init; } = string.Empty;
    public string PrimaryProvider { get; init; } = string.Empty;
    public string? FallbackProvider { get; init; }
}
