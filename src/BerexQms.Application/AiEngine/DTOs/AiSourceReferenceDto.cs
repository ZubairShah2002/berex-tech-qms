namespace BerexQms.Application.AiEngine.DTOs;

public sealed record AiSourceReferenceDto(
    string ModuleName,
    Guid RecordId,
    string RecordType);
