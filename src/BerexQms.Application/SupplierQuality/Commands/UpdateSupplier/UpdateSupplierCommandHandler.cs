using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.SupplierQuality.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.SupplierQuality.Commands.UpdateSupplier;

internal sealed class UpdateSupplierCommandHandler : ICommandHandler<UpdateSupplierCommand>
{
    private readonly ISupplierRepository _repository;

    public UpdateSupplierCommandHandler(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetByIdAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
            return Result.Failure(SupplierErrors.NotFound);

        supplier.UpdateDetails(
            request.Name,
            request.Tier,
            request.ContactName,
            request.ContactRole,
            request.ContactEmail,
            request.ContactPhone);

        await _repository.UpdateAsync(supplier, cancellationToken);
        return Result.Success();
    }
}
