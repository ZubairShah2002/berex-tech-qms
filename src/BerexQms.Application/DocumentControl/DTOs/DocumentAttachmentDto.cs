namespace BerexQms.Application.DocumentControl.DTOs;

public sealed record DocumentAttachmentDto(
    string FileName,
    string ContentType,
    long SizeBytes,
    string StoragePath);
