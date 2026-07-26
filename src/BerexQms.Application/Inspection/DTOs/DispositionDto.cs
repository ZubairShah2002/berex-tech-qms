namespace BerexQms.Application.Inspection.DTOs;

public sealed record DispositionDto(
    string Type,
    string Justification,
    string ApprovedBy,
    DateTime ApprovedAt);
