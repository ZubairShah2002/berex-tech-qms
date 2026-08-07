namespace BerexQms.Application.AiEngine.DTOs;

public sealed record AiCapabilityStatsDto(
    string Capability,
    int TotalInteractions,
    int CompletedCount,
    int FailedCount,
    int AcceptedCount,
    int RejectedCount,
    decimal AverageConfidence,
    double AverageResponseTimeMs,
    string Period);
