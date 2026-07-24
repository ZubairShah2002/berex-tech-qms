using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;
using BerexQms.Domain.Identity.Entities;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Identity.Commands.CreateTenant;

public sealed class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, TenantDto>
{
    private readonly ITenantRepository _tenantRepository;

    public CreateTenantCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<TenantDto>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        if (await _tenantRepository.CodeExistsAsync(request.Code, cancellationToken))
            return UserErrors.TenantCodeExists(request.Code);

        var tenant = Tenant.Create(
            Guid.NewGuid(),
            request.Name,
            request.Code,
            request.ContactEmail,
            request.TimeZone);

        await _tenantRepository.AddAsync(tenant, cancellationToken);

        return new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Code,
            tenant.IsActive,
            tenant.ContactEmail,
            tenant.TimeZone);
    }
}
