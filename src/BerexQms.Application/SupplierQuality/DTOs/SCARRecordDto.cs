namespace BerexQms.Application.SupplierQuality.DTOs;

public sealed record SCARRecordDto(
    Guid Id,
    string ScarNumber,
    Guid? NonConformanceId,
    string DefectDescription,
    string Severity,
    DateTime IssuedDate,
    DateTime ResponseDeadline,
    string Status,
    string? ResponseRootCause,
    string? ResponseCorrectiveActions,
    string? ResponseEvidenceRefs,
    DateTime? ResponseDate);
