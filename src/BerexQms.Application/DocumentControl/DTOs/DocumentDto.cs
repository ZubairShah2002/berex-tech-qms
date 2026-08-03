namespace BerexQms.Application.DocumentControl.DTOs;

public sealed record DocumentDto(
    Guid Id,
    string DocumentNumber,
    string Title,
    string DocumentType,
    string OwnerId,
    string? Department,
    bool IsActive,
    int VersionCount,
    string? CurrentVersionNumber,
    string? CurrentVersionStatus,
    DateTime CreatedAt);
