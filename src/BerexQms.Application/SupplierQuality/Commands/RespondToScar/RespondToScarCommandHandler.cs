using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.SupplierQuality.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.SupplierQuality.Commands.RespondToScar;

internal sealed class RespondToScarCommandHandler : ICommandHandler<RespondToScarCommand>
{
    private readonly ISupplierRepository _repository;

    public RespondToScarCommandHandler(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(RespondToScarCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetWithScarsAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
            return Result.Failure(SupplierErrors.NotFound);

        supplier.RespondToScar(request.ScarId, request.RootCause, request.CorrectiveActions, request.EvidenceRefs);
        await _repository.UpdateAsync(supplier, cancellationToken);
        return Result.Success();
    }
}
