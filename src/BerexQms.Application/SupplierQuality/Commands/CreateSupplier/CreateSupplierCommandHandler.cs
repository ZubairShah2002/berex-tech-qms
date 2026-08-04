using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.SupplierQuality.Entities;
using BerexQms.Domain.SupplierQuality.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.SupplierQuality.Commands.CreateSupplier;

internal sealed class CreateSupplierCommandHandler : ICommandHandler<CreateSupplierCommand, Guid>
{
    private readonly ISupplierRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateSupplierCommandHandler(
        ISupplierRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.CodeExistsAsync(request.Code, cancellationToken))
            return SupplierErrors.CodeExists;

        var supplier = Supplier.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.Code,
            request.Name,
            request.Tier,
            request.ContactName,
            request.ContactRole,
            request.ContactEmail,
            request.ContactPhone);

        await _repository.AddAsync(supplier, cancellationToken);
        return supplier.Id;
    }
}
