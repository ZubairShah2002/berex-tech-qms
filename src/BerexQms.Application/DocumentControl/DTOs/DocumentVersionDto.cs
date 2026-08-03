namespace BerexQms.Application.DocumentControl.DTOs;

public sealed record DocumentVersionDto(
    Guid Id,
    string VersionNumber,
    string Status,
    string Content,
    string? ChangeDescription,
    string AuthorId,
    DateTime? EffectiveDate,
    DocumentAttachmentDto? Attachment,
    DateTime CreatedAt,
    DateTime? ReleasedAt,
    string? ReleasedBy);
