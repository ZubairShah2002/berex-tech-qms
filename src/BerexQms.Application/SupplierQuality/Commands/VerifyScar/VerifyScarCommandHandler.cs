using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.SupplierQuality.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.SupplierQuality.Commands.VerifyScar;

internal sealed class VerifyScarCommandHandler : ICommandHandler<VerifyScarCommand>
{
    private readonly ISupplierRepository _repository;

    public VerifyScarCommandHandler(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(VerifyScarCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetWithScarsAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
            return Result.Failure(SupplierErrors.NotFound);

        switch (request.Action.ToUpperInvariant())
        {
            case "CLOSE":
                supplier.CloseScar(request.ScarId);
                break;
            case "FOLLOWUP":
                supplier.RequireFollowUpOnScar(request.ScarId);
                break;
            case "REISSUE":
                supplier.ReissueScar(request.ScarId);
                break;
            default:
                return Result.Failure(SupplierErrors.InvalidStatus);
        }

        await _repository.UpdateAsync(supplier, cancellationToken);
        return Result.Success();
    }
}
