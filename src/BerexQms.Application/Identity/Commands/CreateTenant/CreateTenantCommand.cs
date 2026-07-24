using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;

namespace BerexQms.Application.Identity.Commands.CreateTenant;

public sealed record CreateTenantCommand(
    string Name,
    string Code,
    string? ContactEmail = null,
    string? TimeZone = null) : ICommand<TenantDto>;
