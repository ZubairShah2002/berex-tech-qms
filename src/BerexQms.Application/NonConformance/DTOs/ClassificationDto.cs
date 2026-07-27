namespace BerexQms.Application.NonConformance.DTOs;

public sealed record ClassificationDto(
    string Category,
    string DefectType,
    string? DefectCode);
