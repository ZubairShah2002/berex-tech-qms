namespace BerexQms.Application.Identity.DTOs;

public sealed record TenantDto(
    Guid Id,
    string Name,
    string Code,
    bool IsActive,
    string? ContactEmail,
    string? TimeZone);
