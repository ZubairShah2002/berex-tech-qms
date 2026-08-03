namespace BerexQms.Application.AuditManagement.DTOs;

public sealed record AuditReportDto(
    string Summary,
    string Recommendations,
    string? AuditorNotes,
    DateTime GeneratedAt);
