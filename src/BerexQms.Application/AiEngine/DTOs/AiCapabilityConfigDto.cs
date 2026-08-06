namespace BerexQms.Application.AiEngine.DTOs;

public sealed record AiCapabilityConfigDto(
    Guid Id,
    string Capability,
    bool IsEnabled,
    decimal LowConfidenceThreshold,
    decimal ModerateConfidenceThreshold,
    decimal HighConfidenceThreshold);
