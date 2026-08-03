namespace BerexQms.Domain.AuditManagement.ValueObjects;

public sealed record AuditReport(
    string Summary,
    string Recommendations,
    string? AuditorNotes,
    DateTime GeneratedAt);
