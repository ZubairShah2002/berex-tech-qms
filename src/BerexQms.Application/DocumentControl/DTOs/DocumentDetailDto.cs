namespace BerexQms.Application.DocumentControl.DTOs;

public sealed record DocumentDetailDto(
    Guid Id,
    string DocumentNumber,
    string Title,
    string? Description,
    string DocumentType,
    string OwnerId,
    string? Department,
    bool IsActive,
    IReadOnlyList<DocumentVersionDto> Versions,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? ModifiedAt);
