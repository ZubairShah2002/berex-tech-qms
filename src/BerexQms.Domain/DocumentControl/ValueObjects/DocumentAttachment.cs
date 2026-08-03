namespace BerexQms.Domain.DocumentControl.ValueObjects;

public sealed record DocumentAttachment(
    string FileName,
    string ContentType,
    long SizeBytes,
    string StoragePath,
    string ContentHash);
