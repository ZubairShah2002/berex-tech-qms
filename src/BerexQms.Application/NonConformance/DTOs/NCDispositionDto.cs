namespace BerexQms.Application.NonConformance.DTOs;

public sealed record NCDispositionDto(
    string Type,
    string Justification,
    string ApprovedBy,
    DateTime ApprovedAt);
